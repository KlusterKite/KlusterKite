// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiPublisherActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ApiPublisherActor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Endpoint
{
    using System;
    using System.Collections.Generic;

    using Akka.Actor;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.API.Client.Messages;
    using KlusterKite.API.Provider;

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
        /// <param name="componentContext">
        /// The DI container.
        /// </param>
        public ApiPublisherActor(IComponentContext componentContext)
        {
            var providers = componentContext.Resolve<IEnumerable<ApiProvider>>();
            foreach (var apiProvider in providers)
            {
                var props = Props.Create(() => new ApiHandlerActor(apiProvider));
                try
                {
                    var actor = Context.ActorOf(props, apiProvider.ApiDescription.ApiName);
                    this.apiHandlers.Add(
                        new ApiDiscoverResponse { Description = apiProvider.ApiDescription, Handler = actor });
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }

            this.Receive<ApiDiscoverRequest>(r => this.Sender.Tell(this.apiHandlers));
        }
    }
}
