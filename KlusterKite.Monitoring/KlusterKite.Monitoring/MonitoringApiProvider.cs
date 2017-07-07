// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitoringApiProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MonitoringApiProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Monitoring
{
    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Provider;

    /// <summary>
    /// Provides API access to the monitoring functions
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("KlusterKite node monitoring", Name = "KlusterKiteMonitoring")]
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
            this.KlusterKiteMonitoringApi = new MonitoringApi(system);
        }

        /// <summary>
        /// Gets an access to the monitoring api
        /// </summary>
        [UsedImplicitly]
        [DeclareField("KlusterKite node monitoring")]
        public MonitoringApi KlusterKiteMonitoringApi { get; }
    }
}
