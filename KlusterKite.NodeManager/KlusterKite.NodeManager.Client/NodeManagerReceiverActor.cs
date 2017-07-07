// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerReceiverActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Small client actor for node managing. Provides current node configuration information and executes update related commands
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;

    using JetBrains.Annotations;

    using KlusterKite.Core;
    using KlusterKite.NodeManager.Client.Messages;
    using KlusterKite.NodeManager.Launcher.Messages;

    /// <summary>
    /// Small client actor for node managing. Provides current node configuration information and executes update related commands
    /// </summary>
    [UsedImplicitly]
    public class NodeManagerReceiverActor : ReceiveActor
    {
        /// <summary>
        /// Start time of the node
        /// </summary>
        private readonly long startTimeStamp = DateTimeOffset.UtcNow.UtcTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerReceiverActor"/> class.
        /// </summary>
        /// <param name="baseInstallers">
        /// The base Installers.
        /// </param>
        public NodeManagerReceiverActor(IEnumerable<BaseInstaller> baseInstallers)
        {
            var config = Context.System.Settings.Config;
            var nodeIdString = config.GetString("KlusterKite.NodeManager.NodeId");
            Guid nodeId;
            if (!Guid.TryParse(nodeIdString, out nodeId))
            {
                nodeId = Guid.NewGuid();
            }

            NodeDescription description;
            try
            {
                description = new NodeDescription
                {
                    IsInitialized = true,
                    NodeAddress = Cluster.Get(Context.System).SelfAddress,
                    NodeTemplate = config.GetString("KlusterKite.NodeManager.NodeTemplate"),
                    ContainerType = config.GetString("KlusterKite.NodeManager.ContainerType"),
                    ReleaseId = config.GetInt("KlusterKite.NodeManager.ReleaseId"),
                    NodeId = nodeId,
                    StartTimeStamp = this.startTimeStamp,
                    Roles = config.GetStringList("akka.cluster.roles")?.ToList() ?? new List<string>(),
                    Modules = baseInstallers.Select(i => i.GetType().GetTypeInfo().Assembly).Distinct()
                                .Select(
                                    a =>
                                        new PackageDescription
                                        {
                                            Id = a.GetName().Name,
                                            Version = a.GetName().Version.ToString()
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
                            .Error(loaderException.InnerException, "{Type}: Inner exception", this.GetType().Name);
                    }
                }

                throw;
            }

            this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(description));
            this.ReceiveAsync<ShutdownMessage>(
                async m =>
                    {
                        var cluster = Cluster.Get(Context.System);
                        var system = Context.System;
                        await cluster.LeaveAsync();
                        await system.Terminate();
                    });
        }
    }
}