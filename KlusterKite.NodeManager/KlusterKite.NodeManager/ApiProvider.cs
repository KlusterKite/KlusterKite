// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager API provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager
{
    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.WebApi;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The node manager API provider
    /// </summary>
    [ApiDescription("The root provider", Name = "KlusterKiteNodeApi")]
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
        /// <param name="packageRepository">The package repository</param>
        public ApiProvider(ActorSystem actorSystem, IPackageRepository packageRepository)
        {
            this.actorSystem = actorSystem;
            this.KlusterKiteNodesApi = new NodeManagerApi(actorSystem, packageRepository);
        }

        /// <summary>
        /// Gets the main node manager api
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The KlusterKite node managing API")]
        public NodeManagerApi KlusterKiteNodesApi { get; }

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
