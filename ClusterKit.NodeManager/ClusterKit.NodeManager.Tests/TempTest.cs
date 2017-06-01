using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterKit.NodeManager.Tests
{
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;

    using Microsoft.EntityFrameworkCore;

    using Npgsql;

    using Xunit;

    public class TempTest
    {
        [Fact]
        public void MigratorTest()
        {
            //User ID=postgres;Host=docker;Port=5432;Database=clusterkit.nodemanager.configuration;Pooling=true
            var connectionStringBuilder =
                new NpgsqlConnectionStringBuilder("User ID=postgres;Host=docker;Port=5432;Pooling=true")
                    {
                        Database = Guid.NewGuid().ToString("N")
                    };

            var connectionString = connectionStringBuilder.ConnectionString;
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var context = new ConfigurationContext(optionsBuilder.Options))
            {
                try
                {
                    context.Database.Migrate();
                    var release = new Release();
                    context.Releases.Add(release);
                    context.SaveChanges();
                }
                finally
                {
                    context.Database.EnsureDeleted();
                }
            }
        }
    }
}
