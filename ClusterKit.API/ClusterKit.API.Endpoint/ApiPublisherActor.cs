// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiPublisherActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ApiPublisherActor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Endpoint
{
    using System.Collections.Generic;

    using Akka.Actor;

    using Castle.Windsor;

    using ClusterKit.API.Client.Messages;
    using ClusterKit.API.Provider;

    using JetBrains.Annotations;

    /// <summary>
    /// Publishes defined API to the cluster
    /// </summary>
    [UsedImplicitly]
    public class ApiPublisherActor : ReceiveActor
    {
        /// <summary>
        /// The api handlers.
        /// </summary>
        private readonly List<ApiDiscoverResponse> apiHandlers = new List<ApiDiscoverResponse>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPublisherActor"/> class.
        /// </summary>
        /// <param name="container">
        /// The DI container.
        /// </param>
        public ApiPublisherActor(IWindsorContainer container)
        {
            var providers = container.ResolveAll<ApiProvider>();
            foreach (var apiProvider in providers)
            {
                var actor = Context.ActorOf(
                    Props.Create(() => new ApiHandlerActor(apiProvider)),
                    apiProvider.ApiDescription.ApiName);
                this.apiHandlers.Add(
                    new ApiDiscoverResponse { Description = apiProvider.ApiDescription, Handler = actor });
            }

            this.Receive<ApiDiscoverRequest>(r => this.Sender.Tell(this.apiHandlers));
        }
    }
}
