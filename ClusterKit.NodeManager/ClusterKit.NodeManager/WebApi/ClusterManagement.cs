// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterManagement.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Gets the access to current cluster state management
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Attributes.Authorization;
    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.Messages.Migration;

    /// <summary>
    /// Gets the access to current cluster state management
    /// </summary>
    [ApiDescription("Publishes access to the authenticated user information", Name = "ClusterManagement")]
    public class ClusterManagement
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterManagement"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public ClusterManagement(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(actorSystem);
        }

        /// <summary>
        /// Gets current cluster resources state
        /// </summary>
        [DeclareField("The current cluster resources state")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetResourceState)]
        public Task<ResourceState> ResourceState => this.actorSystem.ActorSelection(NodeManagerApi.GetManagerActorProxyPath())
            .Ask<ResourceState>(new ResourceStateRequest(), this.AkkaTimeout);

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }
    }
}
