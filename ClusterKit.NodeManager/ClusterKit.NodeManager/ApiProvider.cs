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
    using Akka.Actor;

    using ClusterKit.API.Attributes;
    using ClusterKit.NodeManager.WebApi;
    using ClusterKit.Security.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The node manager API provider
    /// </summary>
    [ApiDescription("The root provider", Name = "ClusterKitNodeApi")]
    public class ApiProvider : API.Provider.ApiProvider
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public ApiProvider(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
            this.ClusterKitNodesApi = new NodeManagerApi(actorSystem);
        }

        /// <summary>
        /// Gets the main node manager api
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The ClusterKit node managing API")]
        public NodeManagerApi ClusterKitNodesApi { get; }

        /// <summary>
        /// Gets the current user data API
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The current user API</returns>
        [UsedImplicitly]
        [DeclareField(Description = "The current user")]
        public CurrentUserApi Me(RequestContext context) => new CurrentUserApi(context, this.actorSystem);
    }
}
