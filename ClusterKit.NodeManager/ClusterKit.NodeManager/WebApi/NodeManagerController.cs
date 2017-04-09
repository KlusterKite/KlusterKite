// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Serves node management api functions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Web.Authorization.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Serves node management api functions
    /// </summary>
    [UsedImplicitly]
    [RequireSession]
    [RoutePrefix("api/1.x/clusterkit/nodemanager")]
    public class NodeManagerController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerController"/> class.
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
        /// Gets configuration for new empty node
        /// </summary>
        /// <param name="request">The configuration request</param>
        /// <returns>The configuration to apply</returns>
        [Route("getConfiguration")]
        [HttpPost]
        [RequireClientPrivilege(Privileges.GetConfiguration)]
        public async Task<NodeStartUpConfiguration> GetConfiguration(NewNodeTemplateRequest request)
        {
            object result;
            try
            {
                result = await this.System.ActorSelection(this.GetManagerActorProxyPath())
                             .Ask<object>(request, this.AkkaTimeout);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: GetConfiguration exception", this.GetType().Name);
                throw;
            }

            var configuration = result as NodeStartUpConfiguration;
            if (configuration != null)
            {
                this.System.Log.Info("{Type}: sending configuration", this.GetType().Name);
                return configuration;
            }

            var waitMessage = result as NodeStartupWaitMessage;
            if (waitMessage != null)
            {
                this.System.Log.Info(
                    "{Type}: cluster is full. Container {ContainerType} with framework {FrameworkName} in not needed now",
                    this.GetType().Name,
                    request.ContainerType,
                    request.FrameworkRuntimeType);
                var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable };

                httpResponseMessage.Headers.Add("Retry-After", ((int)waitMessage.WaitTime.TotalSeconds).ToString());
                throw new HttpResponseException(httpResponseMessage);
            }

            this.System.Log.Info("{Type}: NewNodeTemplateRequest - unknown response of type {ResponseType}", this.GetType().Name, result?.GetType().Name);
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Manual node upgrade request
        /// </summary>
        /// <param name="address">Address of node to upgrade</param>
        /// <returns>Execution task</returns>
        [Route("upgradeNode")]
        [HttpPost]
        [RequireUserPrivilege(Privileges.UpgradeNode)]
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