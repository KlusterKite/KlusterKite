// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplatesRestController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   All rest actions with <see cref="NodeTemplate" />
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
    using ClusterKit.Security.Attributes;
    using ClusterKit.Web.Authorization.Attributes;
    using ClusterKit.Web.Rest;

    /// <summary>
    /// All rest actions with <see cref="NodeTemplate"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/templates")]
    [RequireUser]
    [RequireUserPrivilege(Privileges.NodeTemplate, CombinePrivilegeWithActionName = true, Severity = EnSeverity.Crucial)
    ]
    public class NodeTemplatesRestController : BaseRestController<NodeTemplate, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeTemplatesRestController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public NodeTemplatesRestController(ActorSystem system)
            : base(system)
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<NodeTemplate, bool>> DefaultFilter => null;

        /// <inheritdoc />
        protected override List<SortingCondition> DefaultSort
            => new List<SortingCondition> { new SortingCondition(nameof(NodeTemplate.Code), SortingCondition.EnDirection.Asc) };

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}