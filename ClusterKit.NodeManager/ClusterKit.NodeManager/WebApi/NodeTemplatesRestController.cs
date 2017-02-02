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
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Web.Rest;

    /// <summary>
    /// All rest actions with <see cref="NodeTemplate"/>
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/templates")]
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

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}