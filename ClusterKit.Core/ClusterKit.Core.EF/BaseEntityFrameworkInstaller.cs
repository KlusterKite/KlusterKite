// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseEntityFrameworkInstaller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to configure Entity Framework
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.EF
{
    using System.Data.Entity;

    /// <summary>
    /// Base class to configure Entity Framework
    /// </summary>
    public abstract class BaseEntityFrameworkInstaller
    {
        /// <summary>
        /// Gets the configuration for entity framework
        /// </summary>
        /// <returns>EF configuration</returns>
        public abstract DbConfiguration GetConfiguration();
    }
}