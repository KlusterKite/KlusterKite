﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationDbContextFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Creates context in with test database
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource.Migrator
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
            builder.UseNpgsql("configurationContext", b => b.MigrationsAssembly("KlusterKite.NodeManager.ConfigurationSource"));
            return new ConfigurationContext(builder.Options);
        }
    }
}
