// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestContext.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The test database context
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests.Context
{
    using System.Data.Common;
    using System.Data.Entity;

    /// <summary>
    /// The test database context
    /// </summary>
    public class TestContext : DbContext
    {
        /// <inheritdoc />
        public TestContext()
        {
        }

        /// <inheritdoc />
        public TestContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        /// <summary>
        /// Gets or sets the list of test objects
        /// </summary>
        public DbSet<TestObject> TestObjects { get; set; }
    }
}
