// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerCollectorActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Collects all links to swagger and publishes them to web api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.Monitor
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;
    using Akka.Routing;

    using ClusterKit.Web.Swagger.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Collects all links to swagger and publishes them to web api
    /// </summary>
    [UsedImplicitly]
    public class SwaggerCollectorActor : ReceiveActor
    {
        private readonly IActorRef watcher;

        /// <summary>
        /// List of workers to process requests
        /// </summary>
        private readonly IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerCollectorActor"/> class.
        /// </summary>
        public SwaggerCollectorActor()
        {
            this.workers = Context.ActorOf(Props.Create(typeof(Worker)).WithRouter(FromConfig.Instance), "workers");
            this.watcher = Context.ActorOf(Props.Create(typeof(Watcher)));

            this.Receive<SwaggerPublishDescription>(m => this.watcher.Forward(m));
            this.Receive<ReadOnlyCollection<string>>(m => this.workers.Tell(new Broadcast(m)));
            this.Receive<object>(m => this.workers.Forward(m));
        }

        /// <summary>
        /// Request to acquire list of active swagger services
        /// </summary>
        [UsedImplicitly]
        public class SwaggerListRequest
        {
        }

        /// <summary>
        /// Actor, that watches for cluster modification and tracks swagger links
        /// </summary>
        private class Watcher : ReceiveActor
        {
            /// <summary>
            /// List of published swagger nodes
            /// </summary>
            private readonly Dictionary<Address, string> publishedUrls = new Dictionary<Address, string>();

            /// <summary>
            /// Initializes a new instance of the <see cref="Watcher"/> class.
            /// </summary>
            public Watcher()
            {
                Cluster.Get(Context.System)
                    .Subscribe(
                        this.Self,
                        ClusterEvent.InitialStateAsEvents,
                        new[] { typeof(ClusterEvent.MemberRemoved), typeof(ClusterEvent.MemberUp) });

                this.Receive<ClusterEvent.MemberUp>(
                    m => m.Member.Roles.Contains("Web.Swagger.Publish"),
                    m => this.OnNodeUp(m.Member.Address));

                this.Receive<ClusterEvent.MemberRemoved>(
                    m => m.Member.Roles.Contains("Web.Swagger.Publish"),
                    m => this.OnNodeDown(m.Member.Address));

                this.Receive<SwaggerPublishDescription>(m => this.OnNodeDescription(m));
            }

            /// <summary>
            /// Processes <seealso cref="SwaggerPublishDescription"/> message from Swagger node
            /// </summary>
            /// <param name="description">The message, containing swagger publication definition </param>
            private void OnNodeDescription(SwaggerPublishDescription description)
            {
                bool collectionModified = false;
                var address = this.Sender.Path.Address;
                string oldUrl;
                if (this.publishedUrls.TryGetValue(address, out oldUrl))
                {
                    if (oldUrl != description.Url)
                    {
                        collectionModified = !this.publishedUrls.Values.Contains(description.Url);
                        this.publishedUrls[address] = description.Url;
                        collectionModified = collectionModified || !this.publishedUrls.Values.Contains(oldUrl);
                    }
                }
                else
                {
                    collectionModified = !this.publishedUrls.Values.Contains(description.Url);
                    this.publishedUrls[address] = description.Url;
                }

                if (collectionModified)
                {
                    this.UpdateWorkers();
                }
            }

            /// <summary>
            /// Process the swagger node down cluster event
            /// </summary>
            /// <param name="address">Address of the new node</param>
            private void OnNodeDown(Address address)
            {
                string oldUrl;
                if (this.publishedUrls.TryGetValue(address, out oldUrl))
                {
                    this.publishedUrls.Remove(address);
                    if (!this.publishedUrls.Values.Contains(oldUrl))
                    {
                        this.UpdateWorkers();
                    }
                }
            }

            /// <summary>
            /// Process the swagger node up cluster event
            /// </summary>
            /// <param name="address">Address of the new node</param>
            private void OnNodeUp(Address address)
            {
                Context.ActorSelection($"{address}/user/Web/Swagger/Descriptor")
                    .Tell(new SwaggerPublishDescriptionRequest());
            }

            /// <summary>
            /// Sends new list of published urls to worker for future distribution
            /// </summary>
            private void UpdateWorkers()
            {
                Context.Parent.Tell(this.publishedUrls.Values.Distinct().ToList().AsReadOnly());
            }
        }

        /// <summary>
        /// Worker to process all requests
        /// </summary>
        private class Worker : ReceiveActor
        {
            /// <summary>
            /// List of registered swagger services
            /// </summary>
            private ReadOnlyCollection<string> registeredServices = new List<string>().AsReadOnly();

            /// <summary>
            /// Initializes a new instance of the <see cref="Worker"/> class.
            /// </summary>
            public Worker()
            {
                this.Receive<ReadOnlyCollection<string>>(m => this.registeredServices = m);
                this.Receive<SwaggerListRequest>(m => this.Sender.Tell(this.registeredServices));
            }
        }
    }
}