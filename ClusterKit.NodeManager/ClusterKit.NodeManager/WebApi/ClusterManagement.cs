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
    using ClusterKit.NodeManager.Client.MigrationStates;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Security.Attributes;

    using JetBrains.Annotations;

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
        [UsedImplicitly]
        public Task<ResourceState> ResourceState => this.Actor.Ask<ResourceState>(
            new ResourceStateRequest(),
            this.AkkaTimeout);

        /// <summary>
        /// Gets the current cluster release
        /// </summary>
        [DeclareField("The current cluster release")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetResourceState)]
        [UsedImplicitly]
        public Task<Release> CurrentRelease => this.Actor.Ask<Release>(
            new CurrentReleaseRequest(),
            this.AkkaTimeout);

        /// <summary>
        /// Gets the current cluster release
        /// </summary>
        [DeclareField("The current cluster release")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetResourceState)]
        [UsedImplicitly]
        public Task<Migration> CurrentMigration => this.Actor.Ask<Migration>(
            new CurrentMigrationRequest(),
            this.AkkaTimeout);

        /// <summary>
        /// The node management actor
        /// </summary>
        private ActorSelection Actor => this.actorSystem.ActorSelection(NodeManagerApi.GetManagerActorProxyPath());

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Cancels the current migration
        /// </summary>
        /// <returns>The result of operation</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [LogAccess(Severity = EnSeverity.Crucial, LogMessage = "Cluster migration canceled")]
        [DeclareMutation("Cancels the current migration")]
        public Task<bool> MigrationCancel()
        {
            return this.Actor.Ask<bool>(new MigrationCancel(), this.AkkaTimeout);
        }

        /// <summary>
        /// Finishes the current migration
        /// </summary>
        /// <returns>The result of operation</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [LogAccess(Severity = EnSeverity.Crucial, LogMessage = "Cluster migration finished")]
        [DeclareMutation("Finishes the current migration")]
        public Task<bool> MigrationFinish()
        {
            return this.Actor.Ask<bool>(new MigrationFinish(), this.AkkaTimeout);
        }

        /// <summary>
        /// Updates resources in the cluster
        /// </summary>
        /// <param name="request">
        /// The resource update commands.
        /// </param>
        /// <returns>
        /// The result of operation
        /// </returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [LogAccess(Severity = EnSeverity.Crucial, LogMessage = "Resources were updated")]
        [DeclareMutation("Updates resources in the cluster")]
        public Task<bool> MigrationResourceUpdate([ApiDescription("The list of resources to update")] ResourceUpgradeRequest request)
        {
            return this.Actor.Ask<bool>(request.Resources, this.AkkaTimeout);
        }

        /// <summary>
        /// Initiate cluster node update procedure
        /// </summary>
        /// <param name="target">
        /// The update direction
        /// </param>
        /// <returns>
        /// The result of operation
        /// </returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [LogAccess(Severity = EnSeverity.Crucial, LogMessage = "Cluster node update procedure initiated")]
        [DeclareMutation("Initiate cluster node update procedure")]
        public Task<bool> MigrationNodesUpdate([ApiDescription("The update direction")] EnMigrationSide target)
        {
            return this.Actor.Ask<bool>(new NodesUpgrade { Target = target }, this.AkkaTimeout);
        }
    }
}