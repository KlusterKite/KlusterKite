// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeedAddressesRestController.cs" company="ClusterKit">
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

    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.Web.Rest;

    /// <summary>
    /// All rest actions with <see cref="SeedAddress"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/seed")]
    public class SeedAddressesRestController : BaseRestController<SeedAddress, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeedAddressesRestController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public SeedAddressesRestController(ActorSystem system)
                    : base(system)
        {
        }

        /// <inheritdoc />
        protected override bool DataIsLarge => true;

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}