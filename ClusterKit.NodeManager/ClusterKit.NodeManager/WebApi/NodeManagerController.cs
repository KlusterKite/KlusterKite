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
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Web.CRUDS;

    using JetBrains.Annotations;

    /// <summary>
    /// Serves node management api functions
    /// </summary>
    [UsedImplicitly]
    [RoutePrefix("api/1.x/clusterkit/nodemanager")]
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
        /// Gets list of available templates for specified container type for current cluster state
        /// </summary>
        /// <returns>The list of available templates</returns>
        [Route("availableTemplates/{containerType}")]
        [HttpGet]
        public async Task<List<NodeTemplate>> GetAvailableTemplates(string containerType)
        {
            return await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = containerType }, this.AkkaTimeout);
        }

        /// <summary>
        /// Gets configuration for new empty node
        /// </summary>
        /// <param name="request">The configuration request</param>
        /// <returns>The configuration to apply</returns>
        [Route("getConfiguration")]
        [HttpPost]
        public async Task<NodeStartUpConfiguration> GetConfiguration(NewNodeTemplateRequest request)
        {
            var result = await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<object>(request, this.AkkaTimeout);

            var configuration = result as NodeStartUpConfiguration;
            if (configuration != null)
            {
                return configuration;
            }

            var waitMessage = result as NodeStartupWaitMessage;
            if (waitMessage != null)
            {
                var httpResponseMessage = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable
                };

                httpResponseMessage.Headers.Add("Retry-After", ((int)waitMessage.WaitTime.TotalSeconds).ToString());
                throw new HttpResponseException(httpResponseMessage);
            }

            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Gets list of available packages from local cluster repository
        /// </summary>
        /// <returns>The list of available packages</returns>
        [Route("getPackages")]
        [HttpGet]
        public async Task<List<PackageDescription>> GetPackages()
        {
            return await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<List<PackageDescription>>(new PackageListRequest(), this.AkkaTimeout);
        }

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        /// <returns>Current cluster statistics</returns>
        [Route("stats")]
        [HttpGet]
        public async Task<TemplatesUsageStatistics> GetTemplateStatistics()
        {
            return await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest(), this.AkkaTimeout);
        }

        /// <summary>
        /// Request to server to reload package list
        /// </summary>
        /// <returns>Success of the operation</returns>
        [Route("reloadPackages")]
        [HttpPost]
        public async Task ReloadPackages()
        {
            var result = await this.System.ActorSelection(this.GetManagerActorProxyPath()).Ask<bool>(new ReloadPackageListRequest(), this.AkkaTimeout);
            throw result
                      ? new HttpResponseException(HttpStatusCode.OK)
                      : new HttpResponseException(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Manual node upgrade request
        /// </summary>
        /// <param name="address">Address of node to upgrade</param>
        /// <returns>Execution task</returns>
        [Route("upgradeNode")]
        [HttpPost]
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