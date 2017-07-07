// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationDbContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates context in with test database
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrator
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    /// <summary>
    /// Creates context in with test database
    /// </summary>
    public class ConfigurationDbContextFactory : IDbContextFactory<ConfigurationContext>
    {
        /// <inheritdoc />
        public ConfigurationContext Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<ConfigurationContext>();
            builder.UseNpgsql("configurationContext", b => b.MigrationsAssembly("ClusterKit.NodeManager.ConfigurationSource"));
            return new ConfigurationContext(builder.Options);
        }
    }
}
