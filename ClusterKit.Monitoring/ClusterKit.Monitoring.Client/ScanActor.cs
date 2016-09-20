// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScanActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Actor scans current system for full actor tree
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Client
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Cluster.Tools.PublishSubscribe;

    using ClusterKit.Monitoring.Client.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Actor scans current system for full actor tree
    /// </summary>
    [UsedImplicitly]
    public class ScanActor : ReceiveActor
    {
        /// <summary>
        /// Getting the protected property of ActorCell
        /// </summary>
        private static readonly PropertyInfo ActorProperty = typeof(ActorCell).GetProperty("Actor", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Node configuration template name
        /// </summary>
        private readonly string nodeTemplateName;

        /// <summary>
        /// The minimum timespan between subsequent scans
        /// </summary>
        private readonly TimeSpan scanMemoize;

        /// <summary>
        /// The previous scan result
        /// </summary>
        private Node lastScanResult;

        /// <summary>
        /// The previous scan time
        /// </summary>
        private DateTimeOffset lastScanTime;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanActor"/> class.
        /// </summary>
        public ScanActor()
        {
            // ClusterKit.NodeManager writes some properties to config... but this is not necessary
            var nodeTemplateVersion = Context.System.Settings.Config.GetInt("ClusterKit.NodeManager.NodeTemplateVersion");
            var templateName = Context.System.Settings.Config.GetString("ClusterKit.NodeManager.NodeTemplate");

            this.nodeTemplateName = string.IsNullOrWhiteSpace(templateName)
                                        ? null
                                        : $"{templateName}-v{nodeTemplateVersion}";

            this.scanMemoize = Context.System.Settings.Config.GetTimeSpan(
                "ClusterKit.Monitoring.ScanMemoize",
                TimeSpan.FromSeconds(10));
            this.Receive<ActorSystemScanRequest>(m => this.OnScanRequest());

            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Put(this.Self));
        }

        /// <summary>
        /// Draws the whole actors network (for the current node)
        /// </summary>
        /// <returns>The actor's network</returns>
        private Node GetActorMap()
        {
            var network = new Node
            {
                Name = string.Empty,
                Children = this.GetChildren(Context.System.ActorSelection("/").Anchor as ActorRefWithCell)
            };

            this.SetNodeStats(network);
            return network;
        }

        /// <summary>
        /// Gets the description of the actor's children
        /// </summary>
        /// <param name="actorRef">The actor ref</param>
        /// <returns>The description of the children</returns>
        private Node[] GetChildren(ActorRefWithCell actorRef)
        {
            if (!actorRef.Children.Any())
            {
                return Enumerable.Empty<Node>().ToArray();
            }

            var nodes = actorRef.Children.Cast<ActorRefWithCell>()
                .Select(
                    child =>
                    {
                        var node = new Node
                        {
                            Name = child.Path.Name,
                            ActorType = child.Underlying.Props.TypeName,
                            Children = this.GetChildren(child),
                            QueueSize = child.Underlying.NumberOfMessages,
                            CurrentMessage = (child.Underlying as ActorCell)?.CurrentMessage?.GetType()
                                               .FullName,
                            DispatcherId = (child.Underlying as ActorCell)?.Dispatcher.Id,
                            DispatcherType = (child.Underlying as ActorCell)?.Dispatcher.GetType().Name,
                        };
                        var cell = child.Underlying as ActorCell;
                        if (cell != null)
                        {
                            node.ActorType = ActorProperty.GetMethod.Invoke(cell, new object[0])?.GetType().FullName;
                        }

                        return node;
                    })
                .ToArray();

            return nodes;
        }

        /// <summary>
        /// Processes the scan request
        /// </summary>
        private void OnScanRequest()
        {
            Node result;

            if (this.lastScanResult == null || (DateTimeOffset.Now - this.lastScanTime) > this.scanMemoize)
            {
                var cluster = Cluster.Get(Context.System);
                Context.System.Log.Info("{Type}: scan started", this.GetType().Name);
                result = this.GetActorMap();
                result.Name = this.nodeTemplateName != null
                                  ? $"{this.nodeTemplateName} {cluster.SelfAddress.Host}:{cluster.SelfAddress.Port}"
                                  : $"{cluster.SelfAddress.Host}:{cluster.SelfAddress.Port}";
                this.lastScanTime = DateTimeOffset.Now;
                this.lastScanResult = result;
                Context.System.Log.Info("{Type}: scan finished", this.GetType().Name);
            }
            else
            {
                result = this.lastScanResult;
                Context.System.Log.Info("{Type}: Sending scan cache", this.GetType().Name);
            }

            this.Sender.Tell(result);
        }

        /// <summary>
        /// Calculates the sub-network overall stats
        /// </summary>
        /// <param name="node">The sub-network</param>
        private void SetNodeStats(Node node)
        {
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    this.SetNodeStats(child);
                }
            }

            if (node.Children != null && node.Children.Length > 0)
            {
                node.MaxQueueSize = node.Children.Max(c => Math.Max(c.MaxQueueSize, c.QueueSize));
                node.QueueSizeSum = node.QueueSize + node.Children.Sum(c => c.QueueSizeSum);
            }
            else
            {
                node.MaxQueueSize = 0;
                node.QueueSizeSum = node.QueueSize;
            }
        }
    }
}