// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatcherActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Watcher actor. It's main purpose to monitor any cluster changes and store complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Actors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Monitoring.Messages;

    using Microsoft.AspNet.SignalR;

    /// <summary>
    /// Watcher actor. It's main purpose to monitor any cluster changes and store complete data about current cluster health
    /// </summary>
    public class WatcherActor : ReceiveActor
    {
        /// <summary>
        /// Current cluster members
        /// </summary>
        private readonly Dictionary<Address, Member> clusterMembers = new Dictionary<Address, Member>();

        /// <summary>
        /// Current akka system cluster
        /// </summary>
        private Cluster cluster;

        /// <summary>
        /// Initializes a new instance of the <see cref="WatcherActor"/> class.
        /// </summary>
        public WatcherActor()
        {
            this.cluster = Cluster.Get(Context.System);
            Cluster.Get(Context.System)
                .Subscribe(
                    this.Self,
                    ClusterEvent.InitialStateAsEvents,
                    new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });

            this.Receive<ClusterEvent.MemberStatusChange>(m => this.MemberStatusChange(m));
            this.Receive<ClusterMemberListRequest>(m => this.OnClusterMemberListRequest());
        }

        /// <summary>
        /// Monitors current cluster members changing
        /// </summary>
        /// <param name="memberStatusChange">
        /// The member Status Change.
        /// </param>
        /// <returns>
        /// Processing task
        /// </returns>
        private Task MemberStatusChange(ClusterEvent.MemberStatusChange memberStatusChange)
        {
            this.clusterMembers[memberStatusChange.Member.Address] = memberStatusChange.Member;
            var context = GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();
            context.Clients.All.memberUpdate(memberStatusChange.Member);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Serves cluster member list request
        /// </summary>
        /// <returns>Current cluster member list</returns>
        private Task OnClusterMemberListRequest()
        {
            this.Sender.Tell(this.clusterMembers.Values.ToList());
            return Task.CompletedTask;
        }
    }
}