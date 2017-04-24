// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201704211334033_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The initial database configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrator.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// The initial database configuration
    /// </summary>
    public partial class Init : DbMigration
    {
        /// <inheritdoc />
        public override void Down()
        {
            this.DropForeignKey("dbo.MigrationOperations", "ErrorId", "dbo.MigrationErrors");
            this.DropForeignKey("dbo.MigrationOperations", "Migration_Id", "dbo.Migrations");
            this.DropForeignKey("dbo.MigrationOperations", "Release_Id", "dbo.Releases");
            this.DropForeignKey("dbo.MigrationOperations", "Id", "dbo.MigrationLogRecords");
            this.DropForeignKey("dbo.MigrationErrors", "Migration_Id", "dbo.Migrations");
            this.DropForeignKey("dbo.MigrationErrors", "Release_Id", "dbo.Releases");
            this.DropForeignKey("dbo.MigrationErrors", "Id", "dbo.MigrationLogRecords");
            this.DropForeignKey("dbo.UserRoles", "Role_Uid", "dbo.Roles");
            this.DropForeignKey("dbo.UserRoles", "User_Uid", "dbo.Users");
            this.DropForeignKey("dbo.MigrationLogRecords", "ReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.MigrationLogRecords", "MigrationId", "dbo.Migrations");
            this.DropForeignKey("dbo.Migrations", "ToReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.Migrations", "FromReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.CompatibleTemplates", "ReleaseId", "dbo.Releases");
            this.DropForeignKey("dbo.CompatibleTemplates", "CompatibleReleaseId", "dbo.Releases");
            this.DropIndex("dbo.MigrationOperations", new[] { "ErrorId" });
            this.DropIndex("dbo.MigrationOperations", new[] { "Migration_Id" });
            this.DropIndex("dbo.MigrationOperations", new[] { "Release_Id" });
            this.DropIndex("dbo.MigrationOperations", new[] { "Id" });
            this.DropIndex("dbo.MigrationErrors", new[] { "Migration_Id" });
            this.DropIndex("dbo.MigrationErrors", new[] { "Release_Id" });
            this.DropIndex("dbo.MigrationErrors", new[] { "Id" });
            this.DropIndex("dbo.UserRoles", new[] { "Role_Uid" });
            this.DropIndex("dbo.UserRoles", new[] { "User_Uid" });
            this.DropIndex("dbo.Users", new[] { "Login" });
            this.DropIndex("dbo.CompatibleTemplates", new[] { "ReleaseId" });
            this.DropIndex("dbo.CompatibleTemplates", new[] { "CompatibleReleaseId" });
            this.DropIndex("dbo.Migrations", new[] { "ToReleaseId" });
            this.DropIndex("dbo.Migrations", new[] { "FromReleaseId" });
            this.DropIndex("dbo.MigrationLogRecords", new[] { "ReleaseId" });
            this.DropIndex("dbo.MigrationLogRecords", new[] { "MigrationId" });
            this.DropTable("dbo.MigrationOperations");
            this.DropTable("dbo.MigrationErrors");
            this.DropTable("dbo.UserRoles");
            this.DropTable("dbo.Users");
            this.DropTable("dbo.Roles");
            this.DropTable("dbo.CompatibleTemplates");
            this.DropTable("dbo.Releases");
            this.DropTable("dbo.Migrations");
            this.DropTable("dbo.MigrationLogRecords");
        }

        /// <inheritdoc />
        public override void Up()
        {
            this
                .CreateTable(
                    "dbo.MigrationLogRecords",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 MigrationId = c.Int(),
                                 ReleaseId = c.Int(nullable: false),
                                 MigratorTemplateCode = c.String(),
                                 MigratorTemplateName = c.String(),
                                 MigratorTypeName = c.String(),
                                 MigratorName = c.String(),
                                 ResourceCode = c.String(),
                                 ResourceName = c.String(),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Migrations", t => t.MigrationId)
                .ForeignKey("dbo.Releases", t => t.ReleaseId, cascadeDelete: true)
                .Index(t => t.MigrationId)
                .Index(t => t.ReleaseId);

            this
                .CreateTable(
                    "dbo.Migrations",
                    c => new
                             {
                                 Id = c.Int(nullable: false, identity: true),
                                 IsActive = c.Boolean(nullable: false),
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

            this
                .CreateTable(
                    "dbo.MigrationErrors",
                    c => new
                             {
                                 Id = c.Int(nullable: false),
                                 Release_Id = c.Int(),
                                 Migration_Id = c.Int(),
                                 Created = c.DateTimeOffset(nullable: false, precision: 7),
                                 ErrorMessage = c.String(),
                                 ErrorStackTrace = c.String(),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MigrationLogRecords", t => t.Id)
                .ForeignKey("dbo.Releases", t => t.Release_Id)
                .ForeignKey("dbo.Migrations", t => t.Migration_Id)
                .Index(t => t.Id)
                .Index(t => t.Release_Id)
                .Index(t => t.Migration_Id);

            this
                .CreateTable(
                    "dbo.MigrationOperations",
                    c => new
                             {
                                 Id = c.Int(nullable: false),
                                 Release_Id = c.Int(),
                                 Migration_Id = c.Int(),
                                 Started = c.DateTimeOffset(nullable: false, precision: 7),
                                 Finished = c.DateTimeOffset(nullable: false, precision: 7),
                                 SourcePoint = c.String(),
                                 DestinationPoint = c.String(),
                                 ErrorId = c.Int(),
                             })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MigrationLogRecords", t => t.Id)
                .ForeignKey("dbo.Releases", t => t.Release_Id)
                .ForeignKey("dbo.Migrations", t => t.Migration_Id)
                .ForeignKey("dbo.MigrationErrors", t => t.ErrorId)
                .Index(t => t.Id)
                .Index(t => t.Release_Id)
                .Index(t => t.Migration_Id)
                .Index(t => t.ErrorId);
        }
    }
}