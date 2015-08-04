// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterLoggingActor.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Logging cluster system events
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Cluster
{
    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;

    /// <summary>
    /// Logging cluster system events
    /// </summary>
    public class ClusterLoggingActor : UntypedActor
    {
        /// <summary>
        /// The cluster.
        /// </summary>
        private Akka.Cluster.Cluster cluster;

        /// <summary>
        /// Need to subscribe to cluster changes
        /// </summary>
        protected override void PreStart()
        {
            this.cluster = Akka.Cluster.Cluster.Get(Context.System);
            this.cluster.Subscribe(
                this.Self,
                ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.IReachabilityEvent), typeof(ClusterEvent.IClusterDomainEvent) });

            Context.GetLogger().Debug(
                "{Type}: Cluster log up",
                this.GetType().Name);

            this.cluster.RegisterOnMemberUp(
                () =>
                    {
                        Context.GetLogger().Debug("{Type}: Cluster connection is up", this.GetType().Name);
                    });
        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            this.cluster.Unsubscribe(this.Self);
        }

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
                    "{Address} {Type}: Cluster log {ClustreLogMessage}",
                    this.cluster.SelfAddress.ToString(),
                    this.GetType().Name,
                    message.GetType().Name);
            }
        }
    }
}