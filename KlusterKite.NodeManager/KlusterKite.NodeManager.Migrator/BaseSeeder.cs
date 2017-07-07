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
    /// <summary>
    /// Creates the initial resources for cluster to run new configuration
    /// </summary>
    /// <remarks>
    /// This is defined for quick sandbox creation, not for production.
    /// </remarks>
    public abstract class BaseSeeder
    {
        /// <summary>
        /// Performs seeding
        /// </summary>
        public abstract void Seed();
    }
}