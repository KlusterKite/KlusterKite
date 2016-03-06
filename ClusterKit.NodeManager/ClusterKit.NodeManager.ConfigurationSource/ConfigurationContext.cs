// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationContext.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Configuration database context
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Data.Common;
    using System.Data.Entity;

    /// <summary>
    /// Configuration database context
    /// </summary>
    [DbConfigurationType(typeof(EfConfiguration))]
    public class ConfigurationContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        /// <param name="existingConnection">
        /// The existing connection.
        /// </param>
        /// <param name="contextOwnsConnection">
        /// The context owns connection.
        /// </param>
        public ConfigurationContext(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        public ConfigurationContext()
        {
        }

        /// <summary>
        /// Gets or sets the list of node templates in database
        /// </summary>
        public DbSet<NodeTemplate> Templates { get; set; }
    }
}