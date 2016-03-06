// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EfConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the EfConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    /// EF6 Code-Based Configuration
    /// </summary>
    public class EfConfiguration : DbConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfConfiguration"/> class.
        /// </summary>
        public EfConfiguration()
        {
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("v12.0"));
            this.SetProviderFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            this.SetProviderServices("Npgsql", Npgsql.NpgsqlServices.Instance);
        }
    }
}