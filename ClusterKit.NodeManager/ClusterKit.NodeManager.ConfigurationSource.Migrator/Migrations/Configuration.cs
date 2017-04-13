// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Database initialization configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrator.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Database initialization configuration
    /// </summary>
    public sealed class Configuration : DbMigrationsConfiguration<ConfigurationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = false;
        }

        /// <inheritdoc />
        protected override void Seed(ConfigurationContext context)
        {
        }
    }
}