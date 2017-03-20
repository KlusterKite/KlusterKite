// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerApi.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Client.Attributes.Authorization;
    using ClusterKit.Core;
    using ClusterKit.Data.CRUD;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// The node manager api
    /// </summary>
    [ApiDescription(Description = "The main ClusterKit node managing methods", Name = "ClusterKitNodeManagement")]
    public class NodeManagerApi
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerApi"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public NodeManagerApi(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(actorSystem);
        }

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Gets the list of available packages from local cluster repository
        /// </summary>
        /// <returns>The list of available packages</returns>
        [UsedImplicitly]
        [DeclareField(Description = "The list of available packages from local cluster repository")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetPackages, Scope = EnPrivilegeScope.User)]
        public async Task<List<PackageDescriptionForApi>> GetPackages()
        {
            return
                (await this.actorSystem.ActorSelection(this.GetManagerActorProxyPath())
                     .Ask<List<PackageDescription>>(new PackageListRequest(), this.AkkaTimeout))
                     .Select(d => new PackageDescriptionForApi(d)).ToList();
        }

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        /// <returns>Current cluster statistics</returns>
        [UsedImplicitly]
        [DeclareField(Description = "Current cluster node template usage for debug purposes")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetTemplateStatistics, Scope = EnPrivilegeScope.User)]
        public async Task<TemplatesUsageStatistics> GetTemplateStatistics()
        {
            return
                await this.actorSystem.ActorSelection(this.GetManagerActorProxyPath())
                    .Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest(), this.AkkaTimeout);
        }

        /// <summary>
        /// Request to server to reload package list
        /// </summary>
        /// <returns>Success of the operation</returns>
        [UsedImplicitly]
        [DeclareMutation(Description = "Request to server to reload package list")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.ReloadPackages, Scope = EnPrivilegeScope.User)]
        [LogAccess]
        public async Task<MutationResult<bool>> ReloadPackages()
        {
            var result =
                await this.actorSystem.ActorSelection(this.GetManagerActorProxyPath())
                    .Ask<bool>(new ReloadPackageListRequest(), this.AkkaTimeout);
            return new MutationResult<bool> { Result = result };
        }

        /// <summary>
        /// Manual node upgrade request
        /// </summary>
        /// <param name="address">Address of node to upgrade</param>
        /// <returns>Execution task</returns>
        [UsedImplicitly]
        [DeclareMutation(Description = "Manual node upgrade request")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.UpgradeNode, Scope = EnPrivilegeScope.User)]
        [LogAccess]
        public async Task<MutationResult<bool>> UpgradeNode(string address)
        {
            var result =
                await this.actorSystem.ActorSelection(this.GetManagerActorProxyPath())
                    .Ask<bool>(new NodeUpgradeRequest { Address = Address.Parse(address) }, this.AkkaTimeout);
            return new MutationResult<bool> { Result = result };
        }

        /// <summary>
        /// Gets current cluster active nodes descriptions
        /// </summary>
        /// <returns>The list of descriptions</returns>
        [UsedImplicitly]
        [DeclareField(Description = "The list of known active nodes")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetActiveNodeDescriptions, Scope = EnPrivilegeScope.User)]
        public async Task<List<NodeDescription>> GetActiveNodeDescriptions()
        {
            var activeNodeDescriptions =
                await this.actorSystem.ActorSelection(this.GetManagerActorProxyPath())
                    .Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), this.AkkaTimeout);

            return
                activeNodeDescriptions.OrderBy(n => n.NodeTemplate)
                    .ThenBy(n => n.ContainerType)
                    .ThenBy(n => n.NodeAddress.ToString())
                    .ToList();
        }

        /// <summary>
        /// The connection to the <see cref="NodeTemplate"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CreateDescription = "Creates the new node template", CanDelete = true,
            DeleteDescription = "Deletes the node template", CanUpdate = true,
            UpdateDescription = "Updates the node template", Description = "Node templates")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.NodeTemplate, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public Connection<NodeTemplate, int> NodeTemplates(RequestContext context)
        {
            return new Connection<NodeTemplate, int>(
                this.actorSystem,
                this.GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// The connection to the <see cref="NugetFeed"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CreateDescription = "Creates the new nuget feed link", CanDelete = true,
            DeleteDescription = "Deletes the nuget feed link", CanUpdate = true,
            UpdateDescription = "Updates the nuget feed link", Description = "Node templates")]
        [RequirePrivilege(Privileges.NugetFeed, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public Connection<NugetFeed, int> NugetFeeds(RequestContext context)
        {
            return new Connection<NugetFeed, int>(
                this.actorSystem,
                this.GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// The connection to the <see cref="SeedAddress"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CreateDescription = "Creates the new seed address", CanDelete = true,
            DeleteDescription = "Deletes the seed address", CanUpdate = true,
            UpdateDescription = "Updates the seed address", Description = "Node templates")]
        [RequirePrivilege(Privileges.SeedAddress, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public Connection<SeedAddress, int> SeedAddresses(RequestContext context)
        {
            return new Connection<SeedAddress, int>(
                this.actorSystem,
                this.GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// The connection to the <see cref="User"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CreateDescription = "Creates the new user", CanUpdate = true,
            UpdateDescription = "Updates the user", Description = "ClusterKit managing system users")]
        [RequirePrivilege(Privileges.User, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public Connection<User, Guid> Users(RequestContext context)
        {
            return new Connection<User, Guid>(
                this.actorSystem,
                this.GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// The connection to the <see cref="Role"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CreateDescription = "Creates the new managing system role",
            CanUpdate = true, UpdateDescription = "Updates the managing system role",
            Description = "ClusterKit managing system security roles")]
        [RequirePrivilege(Privileges.Role, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public Connection<Role, Guid> Roles(RequestContext context)
        {
            return new Connection<Role, Guid>(
                this.actorSystem,
                this.GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        private string GetManagerActorProxyPath() => "/user/NodeManager/NodeManagerProxy";

        /// <summary>
        /// Description of the Nuget package for API output
        /// </summary>
        [ApiDescription(Description = "The nuget package", Name = "ClusterKitNugetPackage")]
        public class PackageDescriptionForApi
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PackageDescriptionForApi"/> class.
            /// </summary>
            /// <param name="description">
            /// The original description.
            /// </param>
            public PackageDescriptionForApi(PackageDescription description)
            {
                this.Id = description.Id;
                this.Version = description.Version;
            }

            /// <summary>
            /// Gets the package Id
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "The package name")]
            public string Id { get; }

            /// <summary>
            /// Gets the package latest version
            /// </summary>
            [DeclareField(Description = "The package version")]
            [UsedImplicitly]
            public string Version { get; }
        }
    }
}