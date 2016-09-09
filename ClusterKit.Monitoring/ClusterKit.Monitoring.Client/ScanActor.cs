// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScanActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Actor scans current system for full actor tree
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Client
{
    using Akka.Actor;

    using ClusterKit.Monitoring.Client.Messages;

    /// <summary>
    /// Actor scans current system for full actor tree
    /// </summary>
    public class ScanActor : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanActor"/> class.
        /// </summary>
        public ScanActor()
        {
            this.Receive<ActorSystemScanRequest>(m => this.OnScanRequest());
        }

        /// <summary>
        /// Processes the scan request
        /// </summary>
        private void OnScanRequest()
        {
            
        }
    }
}
