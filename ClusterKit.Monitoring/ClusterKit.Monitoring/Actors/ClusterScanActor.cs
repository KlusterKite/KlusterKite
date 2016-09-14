// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterScanActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Actor, that scans whole cluster and gets actor's tree
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Actors
{
    using Akka.Actor;
    using Akka.Cluster.Tools.PublishSubscribe;

    using ClusterKit.Monitoring.Client.Messages;
    using ClusterKit.Monitoring.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Actor, that scans whole cluster and gets actor's tree
    /// </summary>
    [UsedImplicitly]
    public class ClusterScanActor : ReceiveActor
    {
        /// <summary>
        /// Last saved cluster tree result
        /// </summary>
        private readonly ClusterTree clusterTree = new ClusterTree();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterScanActor"/> class.
        /// </summary>
        public ClusterScanActor()
        {
            this.Receive<ClusterScanRequest>(m => this.OnClusterScanRequest());
            this.Receive<ClusterScanResultRequest>(m => this.Sender.Tell(this.clusterTree));
            this.Receive<Node>(n => this.clusterTree.Nodes[this.GetNodeName(n)] = n);

            Context.System.Log.Info("{Type}: started", this.GetType().Name);
        }

        /// <summary>
        /// Constructs the node name
        /// </summary>
        /// <param name="n">The node</param>
        /// <returns>The node name</returns>
        private string GetNodeName(Node n)
        {
            return string.IsNullOrWhiteSpace(n.Name) ? this.Sender.Path.Address.ToString() : n.Name;
        }

        /// <summary>
        /// Performs the global cluster scan
        /// </summary>
        private void OnClusterScanRequest()
        {
            Context.System.Log.Info("{Type}: initiating cluster scan", this.GetType().Name);
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new SendToAll("/user/Monitoring/Scanner", new ActorSystemScanRequest()));
            this.clusterTree.Nodes.Clear();
            Context.System.Log.Info("{Type}: cluster scan was initiated", this.GetType().Name);
        }
    }
}