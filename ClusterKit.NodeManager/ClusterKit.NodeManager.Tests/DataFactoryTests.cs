// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataFactoryTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DataFactoryTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Data.EF.Npgsql;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;

    using Npgsql;

    using Xunit;
    using Xunit.Abstractions;

    using Configuration = ClusterKit.NodeManager.ConfigurationSource.Migrations.Configuration;

    /// <summary>
    /// Testing the <see cref="ConfigurationContext"/>
    /// </summary>
    public class DataFactoryTests
    {
        /// <summary>
        /// The test output stream
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFactoryTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public DataFactoryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing overall connection to database
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task TestDbConnection()
        {
            using (await TempDatabaseConnection.Create(this.output))
            {
            }
        }

        /// <summary>
        /// Tests the default user operations
        /// </summary>
        [Fact]
        public void TestDefaultUsers()
        {
            using (var connection = TempDatabaseConnection.Create(this.output).Result)
            {
                this.SeedDatabase(connection);
                using (var context = new ConfigurationContext(connection, false))
                {
                    var admin = context.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Login == "admin");
                    Assert.NotNull(admin);
                    Assert.True(admin.CheckPassword("admin"));
                    Assert.False(admin.CheckPassword("1234"));
                    Assert.NotEqual("admin", admin.Password);
                    Assert.NotNull(admin.Roles);
                    Assert.Equal(1, admin.Roles.Count);
                    Assert.Equal("Admin", admin.Roles.First().Name);

                    var guest = context.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Login == "guest");
                    Assert.NotNull(guest);
                    Assert.True(guest.CheckPassword("guest"));
                    Assert.False(guest.CheckPassword("1234"));
                    Assert.NotEqual("guest", guest.Password);
                    Assert.NotNull(guest.Roles);
                    Assert.Equal(1, guest.Roles.Count);
                    Assert.Equal("Guest", guest.Roles.First().Name);
                    Assert.Equal(4, guest.GetScope().Count());
                }
            }
        }

        /// <summary>
        /// Tests the release compatibility set-up
        /// </summary>
        [Fact]
        public void TestReleaseCompatibilitySet()
        {
            using (var connection = TempDatabaseConnection.Create(this.output).Result)
            {
                var activeRelease = new Release
                                        {
                    MinorVersion = 3,
                    Name = "active",
                    State = Release.EnState.Active,
                    Configuration = new ReleaseConfiguration
                                        {
                                            NodeTemplates = new List<NodeTemplate>
                                                                 {
                                                                     new NodeTemplate { Id = 1, Code = "compatible", Configuration = "1", PackagesList = "p1; p2" },
                                                                     new NodeTemplate { Id = 1, Code = "incompatible", Configuration = "1", PackagesList = "p2; p3" },
                                                                 }
                                        }
                                        };

            }
        }

        /// <summary>
        /// Tests the default user operations
        /// </summary>
        [Fact]
        public void TestDefaultRoles()
        {
            using (var connection = TempDatabaseConnection.Create(this.output).Result)
            {
                this.SeedDatabase(connection);
                using (var context = new ConfigurationContext(connection, false))
                {
                    var admin = context.Roles.Include(nameof(Role.Users)).FirstOrDefault(u => u.Name == "Admin");
                    Assert.NotNull(admin);
                    Assert.NotNull(admin.Users);
                    Assert.Equal(1, admin.Users.Count);
                    Assert.Equal("admin", admin.Users.First().Login);

                    var guest = context.Roles.Include(nameof(Role.Users)).FirstOrDefault(u => u.Name == "Guest");
                    Assert.NotNull(guest);
                    Assert.NotNull(guest.Users);
                    Assert.Equal(1, guest.Users.Count);
                    Assert.Equal("guest", guest.Users.First().Login);
                }
            }
        }

        /// <summary>
        /// Tests predefined number of templates
        /// </summary>
        [Fact]
        public void TestTemplates()
        {
            using (var connection = TempDatabaseConnection.Create(this.output).Result)
            {
                this.SeedDatabase(connection);
                using (var context = new ConfigurationContext(connection, false))
                {
                   Assert.NotEqual(0, context.Templates.Count());
                }
            }
        }

        /// <summary>
        /// Seeds the test database with initial data
        /// </summary>
        /// <param name="connection">The database connection</param>
        private void SeedDatabase(TempDatabaseConnection connection)
        {
            using (var context = new ConfigurationContext(connection, false))
            {
                var seeder = new ConfigurationSeeder();
                seeder.Seed(context);
            }
        }

        /// <summary>
        /// Opens connection to the newly created database. Database is destroyed after connection is closed.
        /// </summary>
        private class TempDatabaseConnection : IDisposable
        {
            /// <summary>
            /// The test output stream
            /// </summary>
            private readonly ITestOutputHelper output;

            /// <summary>
            /// The database connection
            /// </summary>
            private NpgsqlConnection connection;

            /// <summary>
            /// The database name
            /// </summary>
            private string databaseName;

            /// <summary>
            /// Initializes a new instance of the <see cref="TempDatabaseConnection"/> class.
            /// </summary>
            /// <param name="output">
            /// The output.
            /// </param>
            private TempDatabaseConnection(ITestOutputHelper output)
            {
                this.output = output;
            }

            /// <summary>
            /// The database connection creation
            /// </summary>
            /// <param name="output">
            /// The output.
            /// </param>
            /// <returns>The connection</returns>
            public static async Task<TempDatabaseConnection> Create(ITestOutputHelper output)
            {
                var tempConnection = new TempDatabaseConnection(output)
                                         {
                                             connection =
                                                 new NpgsqlConnection(
                                                     ConfigurationManager.ConnectionStrings["testPostgres"].ConnectionString),
                                             databaseName = Guid.NewGuid().ToString("N")
                                         };

                try
                {
                    await tempConnection.connection.OpenAsync();
                    output.WriteLine($"Creating temp database {tempConnection.databaseName}");
                    new ConnectionManager().CheckCreateDatabase(tempConnection.connection, tempConnection.databaseName);
                    tempConnection.connection.ChangeDatabase(tempConnection.databaseName);

                    using (var context = new ConfigurationContext(tempConnection.connection, false))
                    {
                        var migration = new MigrateDatabaseToLatestVersion<ConfigurationContext, Configuration>(true);
                        migration.InitializeDatabase(context);
                        output.WriteLine($"temp database {tempConnection.databaseName} was initialized");
                    }

                    return tempConnection;
                }
                catch (Exception)
                {
                    tempConnection.Dispose();
                    throw;
                }
            }

            /// <summary>
            /// Converts wrapper to the original object
            /// </summary>
            /// <param name="obj">Wrapped object</param>
            public static implicit operator NpgsqlConnection(TempDatabaseConnection obj)
            {
                return obj.connection;
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                this.connection.Dispose();

                using (
                    var dropDatabaseConnection =
                        new NpgsqlConnection(ConfigurationManager.ConnectionStrings["testPostgres"].ConnectionString))
                {
                    dropDatabaseConnection.Open();
                    this.output.WriteLine($"removing temp database {this.databaseName}");
                    using (var cmd = dropDatabaseConnection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $@"SELECT pg_terminate_backend(pg_stat_activity.pid)
                                FROM pg_stat_activity
                                WHERE pg_stat_activity.datname = '{this.databaseName}'
                                  AND pid <> pg_backend_pid()";
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = dropDatabaseConnection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $"DROP DATABASE \"{this.databaseName}\"";
                        cmd.ExecuteNonQuery();
                    }

                    this.output.WriteLine($"temp database {this.databaseName} was removed");
                }
            }
        }
    }
}