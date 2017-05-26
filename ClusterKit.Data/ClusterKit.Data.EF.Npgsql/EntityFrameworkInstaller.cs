// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkInstaller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Configures Entity Framework to use postgres
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.Npgsql
{
    using ClusterKit.Data.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// Configures Entity Framework to use postgres
    /// </summary>
    [UsedImplicitly]
    public class EntityFrameworkInstaller : BaseEntityFrameworkInstaller
    {
        /// <summary>
        /// Creates singleton instance of connection manager for future dependency injection
        /// </summary>
        /// <returns>Instance of connection manager</returns>
        public override BaseConnectionManager CreateConnectionManager() => new ConnectionManager();

        /*
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
        */
    }
}