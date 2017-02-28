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
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization.Attributes;
    using ClusterKit.Web.Rest;

    /// <summary>
    /// All rest actions with <see cref="SeedAddress"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/seed")]
    [RequireUser]
    [RequireUserPrivilege(Privileges.SeedAddress, CombinePrivilegeWithActionName = true, Severity = EnSeverity.Crucial)]
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
        protected override Expression<Func<SeedAddress, bool>> DefaultFilter => null;

        /// <inheritdoc />
        protected override List<SortingCondition> DefaultSort
            => new List<SortingCondition> { new SortingCondition(nameof(SeedAddress.Id), SortingCondition.EnDirection.Asc) };

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}