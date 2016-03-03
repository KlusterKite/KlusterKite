// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitoringController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Manages web requests to describe current cluster health state
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Monitoring.Messages;

    /// <summary>
    /// Manages web requests to describe current cluster health state
    /// </summary>
    public class MonitoringController : ApiController
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// The actor system negotiation timeout
        /// </summary>
        private readonly TimeSpan systemTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringController"/> class.
        /// </summary>
        /// <param name="system">
        /// The actor system.
        /// </param>
        public MonitoringController(ActorSystem system)
        {
            this.system = system;
            this.systemTimeout = system.Settings.Config.GetTimeSpan("ClusterKit.AskTimeout", TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets current cluster member list
        /// </summary>
        /// <returns>The member list</returns>
        [Route("MonitoringApi/GetClusterMemberList")]
        [HttpGet, HttpPost]
        public async Task<List<MemberDescription>> GetClusterMemberList()
        {
            return await this.system.ActorSelection("/user/Monitoring/Watcher")
                .Ask<List<MemberDescription>>(new ClusterMemberListRequest(), this.systemTimeout);
        }

        /// <summary>
        /// Hello world
        /// </summary>
        /// <returns>The Hello world</returns>
        [Route("MonitoringApi/Hello")]
        [HttpGet, HttpPost]
        public Task<string> Hello()
        {
            return Task.FromResult("Hello");
        }
    }
}