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

    using ClusterKit.NodeManager.Client.ORM;

    using JetBrains.Annotations;

    /// <summary>
    /// Configuration database context
    /// </summary>
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
        /// Gets or sets a value indicating whether current context is test mock
        /// </summary>
        [UsedImplicitly]
        public virtual bool IsMoq { get; set; }

        /// <summary>
        /// Gets or sets the list of used NuGet feeds
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<NugetFeed> NugetFeeds { get; set; }

        /// <summary>
        ///  Gets or sets the list of addresses of cluster seeds
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<SeedAddress> SeedAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of node templates in database
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<NodeTemplate> Templates { get; set; }

        /// <summary>
        /// Gets or sets the list of web API users
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the list of web API user roles
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of releases
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<Release> Releases { get; set; }
    }
}