// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplateCrudsController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   All rest actions with <see cref="NodeTemplate" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.Web.CRUDS;

    using Serilog;

    /// <summary>
    /// All rest actions with <see cref="NodeTemplate"/>
    /// </summary>
    [RoutePrefix("nodemanager/templates")]
    public class NodeTemplatesController : BaseCrudController<NodeTemplate, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeTemplatesController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public NodeTemplatesController(ActorSystem system)
            : base(system)
        {
        }

        /// <summary>
        /// Gets the list of all node templates
        /// </summary>
        /// <param name="count">
        /// The count of elements to return.
        /// </param>
        /// <param name="skip">
        /// The count of elements to skip.
        /// </param>
        /// <returns>
        /// list of node templates
        /// </returns>
        [Route("list")]
        [HttpGet]
        public async Task<List<NodeTemplate>> GetList(int count = 100, int skip = 0)
        {
            return
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<List<NodeTemplate>>(new CollectionRequest { Count = count, Skip = skip }, this.AkkaTimeout);
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected override string GetDbActorProxyPath() => "/user/NodeManager/DbWorkerProxy";
    }
}