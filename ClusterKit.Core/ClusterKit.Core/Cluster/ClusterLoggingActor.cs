// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterLoggingActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Logging cluster system events
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Cluster
{
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;

    using JetBrains.Annotations;

    /// <summary>
    /// Logging cluster system events
    /// </summary>
    [UsedImplicitly]
    public class ClusterLoggingActor : UntypedActor
    {
        /// <summary>
        /// The cluster.
        /// </summary>
        private Cluster cluster;

        /// <summary>
        /// Processing cluster event
        /// </summary>
        /// <param name="message">The message.</param>
        protected override void OnReceive(object message)
        {
            var memberStatusChange = message as ClusterEvent.MemberStatusChange;
            if (memberStatusChange != null)
            {
                var mem = memberStatusChange;
                Context.System.Log.Info(
                    "{Type}: MemberStatusChanged {ClusterMember} {MessageType}",
                    this.GetType().Name,
                    mem.Member,
                    message.GetType().Name);
            }
            else if (message is ClusterEvent.ReachabilityEvent)
            {
                var unreachable = (ClusterEvent.ReachabilityEvent)message;
                Context.System.Log.Info(
                    "{Type}: ReachabilityEvent {ClusterMember} {MessageType}",
                    this.GetType().Name,
                    unreachable.Member,
                    message.GetType().Name);
            }
            else
            {
                Context.System.Log.Debug(
                    "{Address} {Type}: Cluster log {ClusterLogMessage}",
                    this.cluster.SelfAddress.ToString(),
                    this.GetType().Name,
                    message.GetType().Name);
            }
        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            this.cluster.Unsubscribe(this.Self);
        }

        /// <summary>
        /// Need to subscribe to cluster changes
        /// </summary>
        protected override void PreStart()
        {
            this.cluster = Cluster.Get(Context.System);
            this.cluster.Subscribe(
                this.Self,
                ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IClusterDomainEvent) });

            Context.GetLogger().Debug(
                "{Type}: Cluster log up",
                this.GetType().Name);

            var seeds = Context.System.Settings.Config.GetStringList("akka.cluster.seed-nodes");
            if (seeds != null && seeds.Count > 0)
            {
                Context.GetLogger().Debug(
                "{Type}: Joining cluster",
                this.GetType().Name);
                this.cluster.JoinSeedNodes(seeds.Select(Address.Parse));
            }

            

            this.cluster.RegisterOnMemberUp(
                () =>
                    {
                        Context.GetLogger().Debug("{Type}: Cluster connection is up", this.GetType().Name);
                    });
        }
    }
}