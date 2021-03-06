﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiHandlerActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The actor to handle api requests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Endpoint
{
    using System;

    using Akka.Actor;
    using Akka.Event;

    using KlusterKite.API.Client;
    using KlusterKite.API.Client.Messages;
    using KlusterKite.API.Provider;

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
            this.Receive<QueryApiRequest>(m => this.HandleQuery(m));

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
        private void HandleQuery(QueryApiRequest request)
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