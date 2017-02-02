// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetFeedsRestController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   All rest actions with <see cref="SeedAddress" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Web.Rest;

    using JetBrains.Annotations;

    /// <summary>
    /// All rest actions with <see cref="SeedAddress"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/nugetFeed")]
    [UsedImplicitly]
    public class NugetFeedsRestController : BaseRestController<NugetFeed, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetFeedsRestController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public NugetFeedsRestController(ActorSystem system) : base(system)
        {
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}