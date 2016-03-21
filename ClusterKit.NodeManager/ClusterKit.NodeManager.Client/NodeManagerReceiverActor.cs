// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerReceiverActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Small client actor for node managing. Provides current node configuration information and executes update related commands
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client
{
    using System;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Core;
    using ClusterKit.Core.Utils;
    using ClusterKit.NodeManager.Client.Messages;

    /// <summary>
    /// Small client actor for node managing. Provides current node configuration information and executes update related commands
    /// </summary>
    public class NodeManagerReceiverActor : ReceiveActor
    {
        /// <summary>
        /// Current node description
        /// </summary>
        private readonly NodeDescription description;

        /// <summary>
        /// Start time of the node
        /// </summary>
        private readonly long startTimeStamp = DateTimeOffset.UtcNow.UtcTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerReceiverActor"/> class.
        /// </summary>
        public NodeManagerReceiverActor()
        {
            var nodeIdString = Context.System.Settings.Config.GetString("ClusterKit.NodeManager.NodeId");
            Guid nodeId;
            if (!Guid.TryParse(nodeIdString, out nodeId))
            {
                nodeId = Guid.NewGuid();
            }

            this.description = new NodeDescription
            {
                NodeAddress = Cluster.Get(Context.System).SelfAddress,
                NodeTemplate =
                                Context.System.Settings.Config.GetString(
                                    "ClusterKit.NodeManager.NodeTemplate"),
                ContainerType =
                                Context.System.Settings.Config.GetString(
                                    "ClusterKit.NodeManager.ContainerType"),
                NodeTemplateVersion =
                                Context.System.Settings.Config.GetInt(
                                    "ClusterKit.NodeManager.NodeTemplateVersion"),
                NodeId = nodeId,
                StartTimeStamp = this.startTimeStamp,
                Modules =
                                AppDomain.CurrentDomain.GetAssemblies()
                                .Where(
                                    a =>
                                    a.GetTypes()
                                        .Any(t => t.IsSubclassOf(typeof(BaseInstaller))))
                                .Select(
                                    a =>
                                    new PackageDescription()
                                    {
                                        Id = a.GetName().Name,
                                        Version =
                                                a.GetName()
                                                .Version
                                                .ToString()
                                    })
                                .ToList()
            };

            this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(this.description));
            this.Receive<ShutdownMessage>(m => Context.System.Terminate());
        }
    }
}