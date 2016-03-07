// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationDbWorker.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Singleton actor perfoming all node configuration related database working
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using Akka.Actor;

    using ClusterKit.Core.Ping;

    /// <summary>
    /// Singleton actor performing all node configuration related database working
    /// </summary>
    public class ConfigurationDbWorker : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDbWorker"/> class.
        /// </summary>
        public ConfigurationDbWorker()
        {
            this.Receive<PingMessage>(m => this.Sender.Tell(new PongMessage()));
        }
    }
}