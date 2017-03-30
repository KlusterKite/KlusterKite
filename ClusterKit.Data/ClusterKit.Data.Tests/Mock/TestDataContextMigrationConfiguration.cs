// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataContextMigrationConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Test context migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Test context migration
    /// </summary>
    public class TestDataContextMigrationConfiguration : DbMigrationsConfiguration<TestDataContext>
    {
        /// <inheritdoc />
        public TestDataContextMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }
    }
}
