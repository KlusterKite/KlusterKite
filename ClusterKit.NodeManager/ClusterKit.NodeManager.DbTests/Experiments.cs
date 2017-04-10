// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Experiments.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of migration experimenting tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using ClusterKit.NodeManager.DbTests.Context;
    using ClusterKit.NodeManager.DbTests.Migrations;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// The list of migration experimenting tests
    /// </summary>
    public class Experiments
    {
        /// <summary>
        /// The test output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="Experiments"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public Experiments(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Test 1
        /// </summary>
        [Fact]
        public void Test()
        {
            Database.SetInitializer(new NullDatabaseInitializer<TestContext>());
            using (var connection = TempConnection.Create(this.output))
            {
                var configuration = new Configuration
                                        {
                                            TargetDatabase = new DbConnectionInfo(
                                                connection.ConnectionString,
                                                "Npgsql")
                                        };
                var migrator = new DbMigrator(configuration);
                migrator.Update("Update");
                
                using (var context = new TestContext(connection, false))
                { 
                    var testObject = new TestObject { Name = "test1" };
                    context.TestObjects.Add(testObject);
                    context.SaveChanges();
                }

                using (var context = new TestContext(connection, false))
                {
                    Assert.Equal(1, context.TestObjects.Count());
                    Assert.Equal("test1", context.TestObjects.First().Name);
                }

                migrator.Update("Init");

                using (var context = new TestContext(connection, false))
                {
                    Assert.Equal(1, context.TestObjects.Count());
                    Assert.Equal("test1", context.TestObjects.First().Name);
                }
            }
        }
    }
}
