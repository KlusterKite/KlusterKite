// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiHandlerActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The actor to handle api requests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Endpoint
{
    using System;

    using Akka.Actor;
    using Akka.Event;

    using ClusterKit.API.Client;
    using ClusterKit.API.Provider;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The actor to handle api requests
    /// </summary>
    public class ApiHandlerActor : ReceiveActor
    {
        /// <summary>
        /// The api provider
        /// </summary>
        private ApiProvider apiProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHandlerActor"/> class.
        /// </summary>
        /// <param name="apiProvider">
        /// The api provider.
        /// </param>
        public ApiHandlerActor(ApiProvider apiProvider)
        {
            this.apiProvider = apiProvider;
            this.Receive<MutationApiRequest>(m => this.HandleMutation(m));
            this.Receive<QueriApiRequest>(m => this.HandleQuery(m));
        }

        /// <summary>
        /// Handles mutation requests
        /// </summary>
        /// <param name="request">The request</param>
        private void HandleMutation(MutationApiRequest request)
        {
            var context = Context;
            this.apiProvider.ResolveMutation(
                    request,
                    request.Context,
                    exception => context.GetLogger().Error(exception, "{Type}: mutation resolve exception"))
                .PipeTo(this.Sender, this.Self, failure: e => this.HandleResolveException(e, context));
        }

        /// <summary>
        /// Handles query requests
        /// </summary>
        /// <param name="request">The query request</param>
        private void HandleQuery(QueriApiRequest request)
        {
            var context = Context;
            this.apiProvider.ResolveQuery(
                    request.Fields,
                    request.Context,
                    exception => context.GetLogger().Error(exception, "{Type}: query resolve exception"))
                .PipeTo(this.Sender, this.Self, failure: e => this.HandleResolveException(e, context));
        }

        /// <summary>
        /// Handles the execution exception
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="context">The actor context</param>
        /// <returns>Result to send</returns>
        private object HandleResolveException(Exception exception, IActorContext context)
        {
            context.GetLogger().Error(exception, "{Type}: execution exception");
            return new JObject();
        }
    }
}