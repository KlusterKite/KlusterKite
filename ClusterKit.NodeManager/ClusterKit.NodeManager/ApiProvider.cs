// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager API provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The node manager API provider
    /// </summary>
    [ApiDescription(Description = "The root provider")]
    public class ApiProvider : API.Provider.ApiProvider
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// The node manager data.
        /// </summary>
        private readonly NodeManagerApi nodeManagerData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public ApiProvider(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
            this.nodeManagerData = new NodeManagerApi(actorSystem);
        }

        /// <summary>
        /// Gets the main node manager api
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The ClusterKit node managing API")]
        public NodeManagerApi NodeManagerData
        {
            get
            {
                this.actorSystem.Log.Info("{Type}: NodeManagerData accessed", this.GetType().Name);
                return this.nodeManagerData;
            }
        }

        /// <inheritdoc />
        public override async Task<JObject> ResolveQuery(
            List<ApiRequest> requests,
            RequestContext context,
            Action<Exception> onErrorCallback)
        {
            try
            {
                this.actorSystem.Log.Info("{Type} ResolveQuery execution", this.GetType().Name);
                var resolveQuery = await base.ResolveQuery(
                                       requests,
                                       context,
                                       e => this.actorSystem.Log.Error(e, "{Type}: resolve error", this.GetType().Name));
                this.actorSystem.Log.Info(
                    "{Type} ResolveQuery execution succeeded: {JSON}",
                    this.GetType().Name,
                    resolveQuery.ToString());
                return resolveQuery;
            }
            catch (Exception exception)
            {
                this.actorSystem.Log.Error(exception, "{Type} exception during query resolve", this.GetType().Name);
                throw;
            }
        }
    }
}
