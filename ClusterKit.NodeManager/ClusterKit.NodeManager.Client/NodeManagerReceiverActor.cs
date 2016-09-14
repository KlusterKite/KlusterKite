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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Launcher.Messages;

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
            var config = Context.System.Settings.Config;
            var nodeIdString = config.GetString("ClusterKit.NodeManager.NodeId");
            Guid nodeId;
            if (!Guid.TryParse(nodeIdString, out nodeId))
            {
                nodeId = Guid.NewGuid();
            }

            try
            {
                this.description = new NodeDescription
                {
                    IsInitialized = true,
                    NodeAddress = Cluster.Get(Context.System).SelfAddress,
                    NodeTemplate = config.GetString("ClusterKit.NodeManager.NodeTemplate"),
                    ContainerType = config.GetString("ClusterKit.NodeManager.ContainerType"),
                    NodeTemplateVersion = config.GetInt("ClusterKit.NodeManager.NodeTemplateVersion"),
                    NodeId = nodeId,
                    StartTimeStamp = this.startTimeStamp,
                    Roles = config.GetStringList("akka.cluster.roles")?.ToList() ?? new List<string>(),
                    Modules = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => a.GetTypes().Any(t => t.IsSubclassOf(typeof(BaseInstaller))))
                                .Select(
                                    a =>
                                        new PackageDescription
                                        {
                                            Id = a.GetName().Name,
                                            Version = a.GetName().Version.ToString(),
                                            BuildDate = a.GetCustomAttributes<AssemblyMetadataAttribute>()
                                                        .FirstOrDefault(attr => attr.Key == "BuildDate")?.Value
                                        })
                                .OrderBy(a => a.Id)
                                .ToList()
                };
            }
            catch (ReflectionTypeLoadException exception)
            {
                foreach (var loaderException in exception.LoaderExceptions)
                {
                    Context.GetLogger()
                        .Error(loaderException, "{Type}: exception during assemblies read", this.GetType().Name);
                    if (loaderException.InnerException != null)
                    {
                        Context.GetLogger()
                            .Error(loaderException.InnerException, "{Type}: Innerxception", this.GetType().Name);
                    }
                }

                throw;
            }

            this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(this.description));
            this.Receive<ShutdownMessage>(m => { Context.System.Terminate(); });
        }
    }
}