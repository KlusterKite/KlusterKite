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
    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The test data context
    /// </summary>
    public class TestDataContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataContext"/> class.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        [UsedImplicitly]
        public TestDataContext(DbContextOptions<TestDataContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the roles table
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the users table
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleUser>().HasKey(t => new { t.UserUid, t.RoleUid });
        }
    }
}