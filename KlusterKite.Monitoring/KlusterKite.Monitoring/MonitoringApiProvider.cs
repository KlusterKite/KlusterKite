// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitoringApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MonitoringApiProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring
{
    using Akka.Actor;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Provider;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides API access to the monitoring functions
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("ClusterKit node monitoring", Name = "ClusterKitMonitoring")]
    public class MonitoringApiProvider : ApiProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringApiProvider"/> class.
        /// </summary>
        /// <param name="system">
        /// The actor system.
        /// </param>
        public MonitoringApiProvider(ActorSystem system)
        {
            this.ClusterKitMonitoringApi = new MonitoringApi(system);
        }

        /// <summary>
        /// Gets an access to the monitoring api
        /// </summary>
        [UsedImplicitly]
        [DeclareField("ClusterKit node monitoring")]
        public MonitoringApi ClusterKitMonitoringApi { get; }
    }
}
