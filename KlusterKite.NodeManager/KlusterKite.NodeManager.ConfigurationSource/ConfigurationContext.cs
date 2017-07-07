// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationContext.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Configuration database context
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource
{
    using JetBrains.Annotations;

    using KlusterKite.NodeManager.Client.ORM;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Configuration database context
    /// </summary>
    public class ConfigurationContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        /// <param name="options">
        /// The context options.
        /// </param>
        public ConfigurationContext(DbContextOptions<ConfigurationContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        public ConfigurationContext()
        {
        }

        /// <summary>
        /// Gets or sets the list of migrations
        /// </summary>
        [UsedImplicitly]
        public DbSet<Migration> Migrations { get; set; }

        /// <summary>
        /// Gets or sets the list of web API users
        /// </summary>
        [UsedImplicitly]
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the list of web API user roles
        /// </summary>
        [UsedImplicitly]
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of role to user links
        /// </summary>
        [UsedImplicitly]
        public DbSet<RoleUser> RoleUsers { get; set; }

        /// <summary>
        /// Gets or sets the list of releases
        /// </summary>
        [UsedImplicitly]
        public DbSet<Release> Releases { get; set; }

        /// <summary>
        /// Gets or sets the global resource migration log
        /// </summary>
        [UsedImplicitly]
        public DbSet<MigrationLogRecord> MigrationLogs { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>().Property(r => r.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<User>().HasIndex(u => u.Login);

            modelBuilder.Entity<CompatibleTemplate>().HasOne(t => t.Release)
                .WithMany(r => r.CompatibleTemplatesBackward).HasForeignKey(t => t.ReleaseId);

            modelBuilder.Entity<CompatibleTemplate>().HasOne(t => t.CompatibleRelease)
                .WithMany(r => r.CompatibleTemplatesForward).HasForeignKey(t => t.CompatibleReleaseId);

            modelBuilder.Entity<MigrationLogRecord>().HasOne(r => r.Migration).WithMany(m => m.Logs)
                .HasForeignKey(r => r.MigrationId);

            modelBuilder.Entity<MigrationLogRecord>().HasOne(r => r.Release).WithMany(m => m.MigrationLogs)
                .HasForeignKey(r => r.ReleaseId);

            modelBuilder.Entity<RoleUser>().HasKey(t => new { t.UserUid, t.RoleUid });
        }
    }
}