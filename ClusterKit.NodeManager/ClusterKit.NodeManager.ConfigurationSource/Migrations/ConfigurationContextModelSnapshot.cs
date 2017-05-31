using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ClusterKit.NodeManager.ConfigurationSource;
using ClusterKit.NodeManager.Client.ORM;

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    [DbContext(typeof(ConfigurationContext))]
    partial class ConfigurationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", 1)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.CompatibleTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", 1);

                    b.Property<int>("CompatibleReleaseId");

                    b.Property<int>("ReleaseId");

                    b.Property<string>("TemplateCode");

                    b.HasKey("Id");

                    b.HasIndex("CompatibleReleaseId");

                    b.HasIndex("ReleaseId");

                    b.ToTable("CompatibleTemplate");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.Migration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", 1);

                    b.Property<int?>("Direction");

                    b.Property<DateTimeOffset?>("Finished");

                    b.Property<int>("FromReleaseId");

                    b.Property<bool>("IsActive");

                    b.Property<DateTimeOffset>("Started");

                    b.Property<int>("State");

                    b.Property<int>("ToReleaseId");

                    b.HasKey("Id");

                    b.HasIndex("FromReleaseId");

                    b.HasIndex("ToReleaseId");

                    b.ToTable("Migrations");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.MigrationLogRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", 1);

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<int?>("MigrationId");

                    b.Property<string>("MigratorName");

                    b.Property<string>("MigratorTemplateCode");

                    b.Property<string>("MigratorTemplateName");

                    b.Property<string>("MigratorTypeName");

                    b.Property<int>("ReleaseId");

                    b.Property<string>("ResourceCode");

                    b.Property<string>("ResourceName");

                    b.HasKey("Id");

                    b.HasIndex("MigrationId");

                    b.HasIndex("ReleaseId");

                    b.ToTable("MigrationLogRecords");

                    b.HasDiscriminator<string>("Discriminator").HasValue("MigrationLogRecord");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.Release", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", 1);

                    b.Property<string>("ConfigurationJson");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<DateTimeOffset?>("Finished");

                    b.Property<bool>("IsStable");

                    b.Property<int>("MajorVersion");

                    b.Property<int>("MinorVersion");

                    b.Property<string>("Name");

                    b.Property<string>("Notes");

                    b.Property<DateTimeOffset?>("Started");

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.ToTable("Releases");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.Role", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllowedScopeJson");

                    b.Property<string>("DeniedScopeJson");

                    b.Property<string>("Name");

                    b.HasKey("Uid");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.RoleUser", b =>
                {
                    b.Property<Guid>("UserUid");

                    b.Property<Guid>("RoleUid");

                    b.HasKey("UserUid", "RoleUid");

                    b.HasIndex("RoleUid");

                    b.ToTable("RoleUsers");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.User", b =>
                {
                    b.Property<Guid>("Uid")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("ActiveTill");

                    b.Property<DateTimeOffset?>("BlockedTill");

                    b.Property<bool>("IsBlocked");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Login");

                    b.Property<string>("Password");

                    b.HasKey("Uid");

                    b.HasIndex("Login");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.MigrationError", b =>
                {
                    b.HasBaseType("ClusterKit.NodeManager.Client.ORM.MigrationLogRecord");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("ErrorMessage");

                    b.Property<string>("ErrorStackTrace");

                    b.ToTable("MigrationErrors");

                    b.HasDiscriminator().HasValue("MigrationError");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.MigrationOperation", b =>
                {
                    b.HasBaseType("ClusterKit.NodeManager.Client.ORM.MigrationLogRecord");

                    b.Property<string>("DestinationPoint");

                    b.Property<int?>("ErrorId");

                    b.Property<DateTimeOffset>("Finished");

                    b.Property<string>("SourcePoint");

                    b.Property<DateTimeOffset>("Started");

                    b.HasIndex("ErrorId");

                    b.ToTable("MigrationOperations");

                    b.HasDiscriminator().HasValue("MigrationOperation");
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.CompatibleTemplate", b =>
                {
                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Release", "CompatibleRelease")
                        .WithMany("CompatibleTemplatesForward")
                        .HasForeignKey("CompatibleReleaseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Release", "Release")
                        .WithMany("CompatibleTemplatesBackward")
                        .HasForeignKey("ReleaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.Migration", b =>
                {
                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Release", "FromRelease")
                        .WithMany()
                        .HasForeignKey("FromReleaseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Release", "ToRelease")
                        .WithMany()
                        .HasForeignKey("ToReleaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.MigrationLogRecord", b =>
                {
                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Migration", "Migration")
                        .WithMany("Logs")
                        .HasForeignKey("MigrationId");

                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Release", "Release")
                        .WithMany("MigrationLogs")
                        .HasForeignKey("ReleaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.RoleUser", b =>
                {
                    b.HasOne("ClusterKit.NodeManager.Client.ORM.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleUid")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ClusterKit.NodeManager.Client.ORM.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserUid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ClusterKit.NodeManager.Client.ORM.MigrationOperation", b =>
                {
                    b.HasOne("ClusterKit.NodeManager.Client.ORM.MigrationError", "Error")
                        .WithMany()
                        .HasForeignKey("ErrorId");
                });
        }
    }
}
