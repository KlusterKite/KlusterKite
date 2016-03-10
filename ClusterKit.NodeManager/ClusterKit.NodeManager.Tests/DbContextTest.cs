// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbContextTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Dev. time db communication tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System.Data.Entity;
    using System.Linq;

    using ClusterKit.Core.EF.Npgsql;
    using ClusterKit.NodeManager.ConfigurationSource;

    using JetBrains.Annotations;

    using Npgsql;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Development time database communication tests
    /// </summary>
    [UsedImplicitly]
    public class DbContextTest
    {
        /// <summary>
        /// XUnit output stream
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public DbContextTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Just opening database connection
        /// </summary>
        [Fact]
        public void DbOpenTest()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var context = new ConfigurationContext(connection))
                {
                    var migrator = new MigrateDatabaseToLatestVersion<ConfigurationContext, ConfigurationSource.Migrations.Configuration>(true);
                    migrator.InitializeDatabase(context);

                    // ;
                    this.output.WriteLine($"There is {context.Templates.Count()} templates in db");
                }
            }
        }

        /// <summary>
        /// Just opening database connection
        /// </summary>
        [Fact]
        public void InsertTest()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var context = new ConfigurationContext(connection))
                {
                    var migrator = new MigrateDatabaseToLatestVersion<ConfigurationContext, ConfigurationSource.Migrations.Configuration>(true);
                    migrator.InitializeDatabase(context);

                    var template = context.Templates.Create();
                    template.Name = "Test template";
                    template.Code = "test";
                    context.Templates.Add(template);
                    context.SaveChanges();

                    this.output.WriteLine($"Insert succeded, {template.Id} new id");

                    template = context.Templates.FirstOrDefault(t => t.Code == "test");
                    Assert.NotNull(template);
                    this.output.WriteLine($"Select succeded, {template.Id} new id");
                }
            }
        }

        /// <summary>
        /// Just opening database connection
        /// </summary>
        [Fact]
        public void RecreateDbTest()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var context = new ConfigurationContext(connection))
                {
                    context.Database.Log = s => this.output.WriteLine(s);

                    var migrator = new MigrateDatabaseToLatestVersion<ConfigurationContext, ConfigurationSource.Migrations.Configuration>(true);
                    migrator.InitializeDatabase(context);

                    var template = context.Templates.Create();
                    template.Name = "Test template";
                    template.Code = "test";
                    context.Templates.Add(template);
                    context.SaveChanges();

                    this.output.WriteLine($"Insert succeded, {template.Id} new id");

                    template = context.Templates.FirstOrDefault(t => t.Code == "test");
                    Assert.NotNull(template);
                    this.output.WriteLine($"Select succeded, {template.Id} new id");
                }
            }
        }

        /// <summary>
        ///  Gets the new test connection
        /// </summary>
        /// <returns></returns>
        private static NpgsqlConnection GetConnection()
        {
            DbConfiguration.SetConfiguration(new EntityFrameworkInstaller().GetConfiguration());
            return new NpgsqlConnection("User ID=postgres;Host=192.168.99.100;Port=5432;Database=clusterkit.nodemanager;Pooling=true");
        }
    }
}