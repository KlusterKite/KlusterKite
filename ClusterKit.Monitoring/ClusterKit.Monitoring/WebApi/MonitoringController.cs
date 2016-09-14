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
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Monitoring.Messages;

    /// <summary>
    /// Manages web requests to describe current cluster health state
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/monitoring")]
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
        /// Gets the last cluster scan result.
        /// Changes as system receives the scan results from nodes
        /// </summary>
        /// <returns>The cluster scan result</returns>
        [Route("getScanResult")]
        public async Task<ClusterTree> GetClusterTree()
        {
            try
            {
                return
                    await
                        this.system.ActorSelection("/user/Monitoring/ClusterScannerProxy")
                            .Ask<ClusterTree>(new ClusterScanResultRequest(), this.systemTimeout);
            }
            catch (Exception exception)
            {
                this.system.Log.Error(exception, "{Type}: error on GetClusterTree", this.GetType().Name);
                throw;
            }
        }

        /// <summary>
        /// Initiates the new actor system scan
        /// </summary>
        /// <returns>The success of the operation</returns>
        [Route("initiateScan")]
        public bool InitiateScan()
        {
            this.system.ActorSelection("/user/Monitoring/ClusterScannerProxy").Tell(new ClusterScanRequest());
            return true;
        }
    }
}