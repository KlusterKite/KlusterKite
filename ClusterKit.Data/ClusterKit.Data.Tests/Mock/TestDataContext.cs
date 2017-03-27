// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataContext.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The test data context
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System.Data.Common;
    using System.Data.Entity;

    using JetBrains.Annotations;

    /// <summary>
    /// The test data context
    /// </summary>
    public class TestDataContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataContext"/> class.
        /// </summary>
        [UsedImplicitly]
        public TestDataContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataContext"/> class.
        /// </summary>
        /// <param name="existingConnection">
        /// The existing connection.
        /// </param>
        /// <param name="contextOwnsConnection">
        /// The context owns connection.
        /// </param>
        [UsedImplicitly]
        public TestDataContext(DbConnection existingConnection, bool contextOwnsConnection = true)
        {
        }

        /// <summary>
        /// Gets or sets the users table
        /// </summary>
        public virtual DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the roles table
        /// </summary>
        public virtual DbSet<Role> Roles { get; set; }
    }
}
