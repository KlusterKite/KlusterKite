// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseEntityFrameworkInstaller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to configure Entity Framework
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Base class to configure Entity Framework
    /// </summary>
    public abstract class BaseEntityFrameworkInstaller
    {
        /// <summary>
        /// Creates singleton instance of connection manager for future dependency injection
        /// </summary>
        /// <returns>Instance of connection manager</returns>
        public abstract BaseConnectionManager CreateConnectionManager();

        /*
        /// <summary>
        /// Gets the configuration for entity framework
        /// </summary>
        /// <returns>EF configuration</returns>
        public abstract DbConfiguration GetConfiguration();
        */
    }
}