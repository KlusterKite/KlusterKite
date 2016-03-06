// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkInstaller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Configures Entity Framework to use postgres
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.EF.Npgsql
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    using JetBrains.Annotations;

    /// <summary>
    /// Configures Entity Framework to use postgres
    /// </summary>
    [UsedImplicitly]
    public class EntityFrameworkInstaller : BaseEntityFrameworkInstaller
    {
        /// <summary>
        /// Gets the configuration for entity framework
        /// </summary>
        /// <returns>EF configuration</returns>
        public override DbConfiguration GetConfiguration()
        {
            return new Configuration();
        }

        /// <summary>
        /// EF6 Code-Based Configuration
        /// </summary>
        private class Configuration : DbConfiguration
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Configuration"/> class.
            /// </summary>
            public Configuration()
            {
                this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("v12.0"));
                this.SetProviderFactory("Npgsql", global::Npgsql.NpgsqlFactory.Instance);
                this.SetProviderServices("Npgsql", global::Npgsql.NpgsqlServices.Instance);
            }
        }
    }
}