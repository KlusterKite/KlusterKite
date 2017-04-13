// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201704130958264_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The initial database creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrator.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// The initial database creation
    /// </summary>
    public partial class Init : DbMigration
    {
        /// <inheritdoc />
        public override void Down()
        {
            this.DropForeignKey("dbo.UserRoles", "Role_Uid", "dbo.Roles");
            this.DropForeignKey("dbo.UserRoles", "User_Uid", "dbo.Users");
            this.DropForeignKey("dbo.Migrations", "ToReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.MigrationOperations", "ClusterMigrationId", "dbo.Migrations");
            this.DropForeignKey("dbo.Migrations", "FromReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.CompatibleTemplates", "ReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.CompatibleTemplates", "CompatibleReleaseId", "dbo.Releases");
            this.DropIndex("dbo.UserRoles", new[] { "Role_Uid" });
            this.DropIndex("dbo.UserRoles", new[] { "User_Uid" });
            this.DropIndex("dbo.Users", new[] { "Login" });
            this.DropIndex("dbo.MigrationOperations", new[] { "ClusterMigrationId" });
            this.DropIndex("dbo.CompatibleTemplates", new[] { "ReleaseId" });
            this.DropIndex("dbo.CompatibleTemplates", new[] { "CompatibleReleaseId" });
            this.DropIndex("dbo.Migrations", new[] { "ToReleaseId" });
            this.DropIndex("dbo.Migrations", new[] { "FromReleaseId" });
            this.DropTable("dbo.UserRoles");
            this.DropTable("dbo.Users");
            this.DropTable("dbo.Roles");
            this.DropTable("dbo.MigrationOperations");
            this.DropTable("dbo.CompatibleTemplates");
            this.DropTable("dbo.Releases");
            this.DropTable("dbo.Migrations");
        }

        /// <inheritdoc />
        public override void Up()
        {
            this
                .CreateTable(
                    "dbo.Migrations",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 State = c.Int(nullable: false),
                                 Direction = c.Int(),
                                 Started = c.DateTimeOffset(nullable: false, precision: 7),
                                 Finished = c.DateTimeOffset(precision: 7),
                                 FromReleaseId = c.Int(nullable: false),
                                 ToReleaseId = c.Int(nullable: false),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Releases", t => t.FromReleaseId, cascadeDelete: true)
                .ForeignKey("dbo.Releases", t => t.ToReleaseId, cascadeDelete: true)
                .Index(t => t.FromReleaseId)
                .Index(t => t.ToReleaseId);

            this.CreateTable(
                    "dbo.Releases",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 MajorVersion = c.Int(nullable: false),
                                 MinorVersion = c.Int(nullable: false),
                                 Name = c.String(),
                                 Notes = c.String(),
                                 Created = c.DateTimeOffset(nullable: false, precision: 7),
                                 Started = c.DateTimeOffset(precision: 7),
                                 Finished = c.DateTimeOffset(precision: 7),
                                 State = c.Int(nullable: false),
                                 IsStable = c.Boolean(nullable: false),
                                 ConfigurationJson = c.String(),
                             })
                .PrimaryKey(t => t.Id);

            this
                .CreateTable(
                    "dbo.CompatibleTemplates",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 CompatibleReleaseId = c.Int(nullable: false),
                                 ReleaseId = c.Int(nullable: false),
                                 TemplateCode = c.String(),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Releases", t => t.CompatibleReleaseId, cascadeDelete: true)
                .ForeignKey("dbo.Releases", t => t.ReleaseId, cascadeDelete: true)
                .Index(t => t.CompatibleReleaseId)
                .Index(t => t.ReleaseId);

            this
                .CreateTable(
                    "dbo.MigrationOperations",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 ClusterMigrationId = c.Int(nullable: false),
                                 State = c.Int(nullable: false),
                                 Name = c.String(),
                                 ConnectionString = c.String(),
                                 MigratorName = c.String(),
                                 MigratorTemplate = c.String(),
                                 Order = c.Int(nullable: false),
                                 Started = c.DateTimeOffset(precision: 7),
                                 Finished = c.DateTimeOffset(precision: 7),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Migrations", t => t.ClusterMigrationId, cascadeDelete: true)
                .Index(t => t.ClusterMigrationId);

            this.CreateTable(
                    "dbo.Roles",
                    c => new
                             {
                                 Uid = c.Guid(nullable: false),
                                 Name = c.String(),
                                 AllowedScopeJson = c.String(),
                                 DeniedScopeJson = c.String(),
                             })
                .PrimaryKey(t => t.Uid);

            this.CreateTable(
                    "dbo.Users",
                    c => new
                             {
                                 Uid = c.Guid(nullable: false),
                                 Password = c.String(),
                                 ActiveTill = c.DateTimeOffset(precision: 7),
                                 BlockedTill = c.DateTimeOffset(precision: 7),
                                 IsBlocked = c.Boolean(nullable: false),
                                 IsDeleted = c.Boolean(nullable: false),
                                 Login = c.String(),
                             })
                .PrimaryKey(t => t.Uid)
                .Index(t => t.Login, unique: true);

            this
                .CreateTable(
                    "dbo.UserRoles",
                    c => new { User_Uid = c.Guid(nullable: false), Role_Uid = c.Guid(nullable: false), })
                .PrimaryKey(t => new { t.User_Uid, t.Role_Uid })
                .ForeignKey("dbo.Users", t => t.User_Uid, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.Role_Uid, cascadeDelete: true)
                .Index(t => t.User_Uid)
                .Index(t => t.Role_Uid);
        }
    }
}