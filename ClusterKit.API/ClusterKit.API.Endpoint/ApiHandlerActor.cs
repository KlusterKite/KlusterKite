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
    using ClusterKit.API.Client.Messages;
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
        private readonly ApiProvider apiProvider;

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
            this.Receive<NodeSearchApiRequest>(m => this.HandleNodeSearch(m));

            foreach (var generationError in apiProvider.GenerationErrors)
            {
                Context.GetLogger().Error(
                    "{Type}  generationError error: {GenerationError}",
                    apiProvider.GetType().Name,
                    generationError);
            }
        }

        /// <summary>
        /// Handles mutation requests
        /// </summary>
        /// <param name="request">The request</param>
        private void HandleMutation(MutationApiRequest request)
        {
            var system = Context.System;
            this.apiProvider.ResolveMutation(
                    request,
                    request.Context,
                    exception => system.Log.Error(exception, "{Type}: mutation resolve exception", this.GetType().Name))
                .PipeTo(
                    this.Sender,
                    this.Self,
                    json => (SurrogatableJObject)json,
                    e => this.HandleResolveException(e, system));
        }

        /// <summary>
        /// Handles query requests
        /// </summary>
        /// <param name="request">The query request</param>
        private void HandleQuery(QueriApiRequest request)
        {
            var system = Context.System;
            this.apiProvider.ResolveQuery(
                    request.Fields,
                    request.Context,
                    exception => system.Log.Error(exception, "{Type}: query resolve exception", this.GetType().Name))
                .PipeTo(
                    this.Sender,
                    this.Self,
                    json => (SurrogatableJObject)json,
                    e => this.HandleResolveException(e, system));
        }

        /// <summary>
        /// Handles node search requests
        /// </summary>
        /// <param name="request">The query request</param>
        private void HandleNodeSearch(NodeSearchApiRequest request)
        {
            var system = Context.System;
            system.Log.Info(
                "{Type}: Resolving query for API {ApiName}",
                this.GetType().Name,
                this.apiProvider.ApiDescription.ApiName);

            this.apiProvider.SearchNode(
                    request.Id,
                    request.Path,
                    request.Request,
                    request.Context,
                    exception => system.Log.Error(exception, "{Type}: node search exception", this.GetType().Name))
                .PipeTo(
                    this.Sender,
                    this.Self,
                    json => (SurrogatableJObject)json,
                    e => this.HandleResolveException(e, system));
        }

        /// <summary>
        /// Handles the execution exception
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="system">The actor system</param>
        /// <returns>Result to send</returns>
        private object HandleResolveException(Exception exception, ActorSystem system)
        {
            system.Log.Error(exception, "{Type}: resolve exception", this.GetType().Name);
            return (SurrogatableJObject)new JObject();
        }
    }
}