// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The test context migration configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests.Migrations
{
    using System.Data.Entity.Migrations;

    using ClusterKit.NodeManager.DbTests.Context;

    /// <summary>
    /// The test context migration configuration
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<TestContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = false;
        }

        /// <inheritdoc />
        protected override void Seed(TestContext context)
        {
            // This method will be called after migrating to the latest version.

            // You can use the DbSet<T>.AddOrUpdate() helper extension method 
            // to avoid creating duplicate seed data. E.g.
            // context.People.AddOrUpdate(
            // p => p.FullName,
            // new Person { FullName = "Andrew Peters" },
            // new Person { FullName = "Brice Lambson" },
            // new Person { FullName = "Rowan Miller" }
            // );
        }
    }
}