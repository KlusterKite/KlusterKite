using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiKit.Core.Cluster
{
    using Akka.Actor;
    using Akka.Cluster;
    using Akka.IO;

    public interface IMessageToBusinessObjectActor<T>
    {
        T Id { get; set; }
    }

    /// <summary>
    /// Base class to manage child actors that impersonates som business objects, assuming that there should be only one actor per object across the cluster.
    /// </summary>
    /// <typeparam name="T">Type of business object identificator</typeparam>
    public abstract class ClusterBusinessObjectActorSupervisor<T> : ReceiveActor
    {
        public readonly List<Address> activeNodes = new List<Address>();
        public readonly Dictionary<T, IActorRef> children = new Dictionary<T, IActorRef>();

        public ClusterBusinessObjectActorSupervisor()
        {
            this.CurrentCluster = Akka.Cluster.Cluster.Get(Context.System);

            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberUp(m));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(m => m.Role == this.ClusterRole, m => this.OnRoleLeaderChanged(m));
            this.Receive<IMessageToBusinessObjectActor<T>>(m => this.ForwardMessageToChild(m));

            this.CurrentCluster.Subscribe(this.Self,
                ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });
        }

        /// <summary>
        /// Cluster node role name, that handles such objects.
        /// </summary>
        protected abstract string ClusterRole { get; }

        /// <summary>
        /// Gets the currents cluster.
        /// </summary>
        protected Cluster CurrentCluster { get; }

        protected bool IsLeader { get; private set; }

        protected Address RoleLeader { get; private set; }

        protected override void Unhandled(object message)
        {
            if (message is ClusterEvent.IClusterDomainEvent)
            {
                return;
            }

            base.Unhandled(message);
        }

        private void ForwardMessageToChild(IMessageToBusinessObjectActor<T> message)
        {
            IActorRef child;
            if (this.children.TryGetValue(message.Id, out child))
            {
                child.Forward(message);
            }
            else
            {
                // TODO: find node and create child
            }
        }

        private void OnMemberDown(ClusterEvent.MemberRemoved message)
        {
            this.activeNodes.Remove(message.Member.Address);
            if (this.IsLeader)
            {
                // TODO: restore lost actors
            }
        }

        private void OnMemberUp(ClusterEvent.MemberUp message)
        {
            this.activeNodes.Add(message.Member.Address);
        }

        private void OnRoleLeaderChanged(ClusterEvent.RoleLeaderChanged message)
        {
            this.IsLeader = message.Leader == this.Self.Path.Root.Address;
            this.RoleLeader = message.Leader;

            /*
                var leaderAddress = this.Self.Path.Address.WithHost(this.RoleLeader.Host)
                    .WithPort(this.RoleLeader.Port)
                    .WithProtocol(this.RoleLeader.Protocol);

                Context.System.ActorSelection(leaderAddress.ToString());
                */

            if (this.IsLeader)
            {
                // TODO: check if there are actors, that need to be restored
            }
        }
    }
}