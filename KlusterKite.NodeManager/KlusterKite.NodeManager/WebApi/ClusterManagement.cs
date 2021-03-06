﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterManagement.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Gets the access to current cluster state management
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Attributes.Authorization;
    using KlusterKite.API.Client;
    using KlusterKite.Core;
    using KlusterKite.Core.Monads;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.CRUD.Exceptions;
    using KlusterKite.NodeManager.Client;
    using KlusterKite.NodeManager.Client.Messages.Migration;
    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.Security.Attributes;

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
        /// Gets the current cluster configuration
        /// </summary>
        [DeclareField("The current cluster configuration")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetResourceState)]
        [UsedImplicitly]
        public Task<Configuration> CurrentConfiguration => this.Actor.Ask<Configuration>(new CurrentConfigurationRequest(), this.AkkaTimeout);

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
        /// The node management actor
        /// </summary>
        private ActorSelection Actor => this.actorSystem.ActorSelection(NodeManagerApi.GetManagerActorProxyPath());

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Reloads current cluster configuration/migration state
        /// </summary>
        /// <returns>
        /// The result of operation
        /// </returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [DeclareMutation("Reloads current cluster configuration/migration state")]
        public Task<bool> RecheckState()
        {
            return this.Actor.Ask<bool>(new RecheckState(), this.AkkaTimeout);
        }

        /// <summary>
        /// Gets the current cluster migration
        /// </summary>
        /// <returns>
        /// The current migration
        /// </returns>
        [DeclareField("The current cluster migration")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetResourceState)]
        [UsedImplicitly]
        public async Task<Migration> CurrentMigration()
        {
            return await this.Actor.Ask<Maybe<Migration>>(new CurrentMigrationRequest(), this.AkkaTimeout);
        }

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
        /// Creates a new cluster migration
        /// </summary>
        /// <param name="newConfigurationId">
        /// The destination configuration id
        /// </param>
        /// <returns>
        /// The result of operation
        /// </returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.MigrateCluster)]
        [LogAccess(Severity = EnSeverity.Crucial, LogMessage = "A new cluster migration created")]
        [DeclareMutation("Creates a new migration")]
        public async Task<MutationResult<Migration>> MigrationCreate(int newConfigurationId)
        {
            var result = await this.Actor.Ask<CrudActionResponse<Migration>>(
                             new UpdateClusterRequest { Id = newConfigurationId },
                             this.AkkaTimeout);

            var mutationException = result.Exception as MutationException;
            if (mutationException != null)
            {
                return new MutationResult<Migration> { Errors = mutationException.Errors };
            }

            if (result.Exception != null)
            {
                return new MutationResult<Migration>
                           {
                               Result = result.Data,
                               Errors =
                                   new List<ErrorDescription>
                                       {
                                           new ErrorDescription(
                                               null,
                                               result.Exception.Message)
                                       }
                           };
            }

            return new MutationResult<Migration> { Result = result.Data };
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
        public Task<bool> MigrationResourceUpdate(
            [ApiDescription("The list of resources to update")] ResourceUpgradeRequest request)
        {
            return this.Actor.Ask<bool>(request.Resources, this.AkkaTimeout);
        }
    }
}