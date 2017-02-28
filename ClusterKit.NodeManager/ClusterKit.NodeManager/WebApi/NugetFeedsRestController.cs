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

    using JetBrains.Annotations;

    /// <summary>
    /// All rest actions with <see cref="SeedAddress"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/nugetFeed")]
    [UsedImplicitly]
    [RequireUser]
    [RequireUserPrivilege(Privileges.NugetFeed, CombinePrivilegeWithActionName = true, Severity = EnSeverity.Crucial)]
    public class NugetFeedsRestController : BaseRestController<NugetFeed, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetFeedsRestController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public NugetFeedsRestController(ActorSystem system)
            : base(system)
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<NugetFeed, bool>> DefaultFilter => null;

        /// <inheritdoc />
        protected override List<SortingCondition> DefaultSort
            => new List<SortingCondition> { new SortingCondition(nameof(NugetFeed.Id), SortingCondition.EnDirection.Asc) };

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}