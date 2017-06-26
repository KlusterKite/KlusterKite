// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationContextTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Validating the <see cref="ConfigurationContext" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Linq;

    using ClusterKit.Data.EF.InMemory;
    using ClusterKit.NodeManager.ConfigurationSource;

    using Microsoft.EntityFrameworkCore;

    using Xunit;

    /// <summary>
    /// Validating the <see cref="ConfigurationContext"/>
    /// </summary>
    [Collection("ClusterKit.NodeManager.Tests.ConfigurationContext")]
    public class ConfigurationContextTests : IDisposable
    {
        /// <summary>
        /// Validates the context integrity
        /// </summary>
        [Fact]
        public void ContextValidate()
        {
            using (var context = this.CreateContext())
            {
                Assert.NotNull(context.Users.ToList());
                Assert.NotNull(context.Roles.ToList());
                Assert.NotNull(context.Migrations.ToList());
                Assert.NotNull(context.MigrationLogs.ToList());
                Assert.NotNull(context.Releases.ToList());
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.CreateContext().Database.EnsureDeleted();
        }

        /// <summary>
        /// Creates a new data context
        /// </summary>
        /// <returns>The data context</returns>
        private ConfigurationContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<ConfigurationContext>();
            builder.UseInMemoryDatabase("ClusterKit.NodeManager.Tests.ConfigurationContextTests");
            var configurationContext = new ConfigurationContext(builder.Options);
            configurationContext.ResetValueGenerators();
            return configurationContext;
        }
    }
}