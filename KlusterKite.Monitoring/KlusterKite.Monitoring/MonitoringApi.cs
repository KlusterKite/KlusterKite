// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitoringApi.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Monitoring api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Attributes.Authorization;
    using ClusterKit.LargeObjects.Client;
    using ClusterKit.Monitoring.Client;
    using ClusterKit.Monitoring.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Monitoring api
    /// </summary>
    [ApiDescription(Description = "ClusterKit monitoring api", Name = "Root")]
    public class MonitoringApi
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
        /// Initializes a new instance of the <see cref="MonitoringApi"/> class.
        /// </summary>
        /// <param name="system">
        /// The actor system.
        /// </param>
        public MonitoringApi(ActorSystem system)
        {
            this.system = system;
            this.systemTimeout = system.Settings.Config.GetTimeSpan("ClusterKit.AskTimeout", TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets the last cluster scan result.
        /// Changes as system receives the scan results from nodes
        /// </summary>
        /// <returns>The cluster scan result</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetClusterTree, Scope = EnPrivilegeScope.User)]
        [DeclareField("Gets the last cluster scan result. Changes as system receives the scan results from nodes")]
        public async Task<ClusterTree> GetClusterTree()
        {
            try
            {
                var notification = await this.system
                            .ActorSelection("/user/Monitoring/ClusterScannerProxy")
                            .Ask<ParcelNotification>(new ClusterScanResultRequest(), this.systemTimeout);

                return await notification.Receive(this.system) as ClusterTree;
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
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.InitiateScan, Scope = EnPrivilegeScope.User)]
        [DeclareMutation("Initiates the new actor system scan")]
        public bool InitiateScan()
        {
            this.system.ActorSelection("/user/Monitoring/ClusterScannerProxy").Tell(new ClusterScanRequest());
            return true;
        }
    }
}