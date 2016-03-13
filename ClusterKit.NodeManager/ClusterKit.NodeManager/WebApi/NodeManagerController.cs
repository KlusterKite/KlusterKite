// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Serves node managment api functions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Web;
    using ClusterKit.Web.CRUDS;

    using JetBrains.Annotations;

    /// <summary>
    /// Serves node management api functions
    /// </summary>
    [UsedImplicitly]
    [RoutePrefix("nodemanager")]
    public class NodeManagerController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCrudController{TObject,TId}"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public NodeManagerController(ActorSystem system)
        {
            this.System = system;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(system);
        }

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Gets the actor system
        /// </summary>
        private ActorSystem System { get; }

        /// <summary>
        /// Gets current cluster active nodes descriptions
        /// </summary>
        /// <returns>The list of descriptions</returns>
        [Route("getDescriptions")]
        public async Task<List<NodeDescription>> GetActiveNodeDescriptions()
        {
            return await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), this.AkkaTimeout);
        }

        /// <summary>
        /// Manual node upgrade request
        /// </summary>
        /// <param name="address">Address of node to upgrade</param>
        /// <returns>Execution task</returns>
        [Route("upgradeNode")]
        public async Task UpgradeNode(Address address)
        {
            var result = await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<bool>(new NodeUpgradeRequest { Address = address }, this.AkkaTimeout);
            if (!result)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            throw new HttpResponseException(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        private string GetManagerActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}