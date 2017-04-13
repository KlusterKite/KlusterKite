// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseSeeder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates the initial resources for cluster to run new configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator
{
    using Akka.Configuration;

    /// <summary>
    /// Creates the initial resources for cluster to run new configuration
    /// </summary>
    /// <remarks>
    /// This is defined for quick sandbox creation, not for production.
    /// </remarks>
    public abstract class BaseSeeder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSeeder"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        protected BaseSeeder(Config config)
        {
            this.Config = config;
        }

        /// <summary>
        /// Gets the overall seeding config
        /// </summary>
        protected Config Config { get; }
    }
}
