// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201703280617533_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The initial database configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
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
            this.DropForeignKey("dbo.UserRoles", "Role_Uid", "dbo.Roles");
            this.DropForeignKey("dbo.UserRoles", "User_Uid", "dbo.Users");
            this.DropIndex("dbo.UserRoles", new[] { "Role_Uid" });
            this.DropIndex("dbo.UserRoles", new[] { "User_Uid" });
            this.DropIndex("dbo.Users", new[] { "Login" });
            this.DropTable("dbo.UserRoles");
            this.DropTable("dbo.NodeTemplates");
            this.DropTable("dbo.SeedAddresses");
            this.DropTable("dbo.Users");
            this.DropTable("dbo.Roles");
            this.DropTable("dbo.Releases");
            this.DropTable("dbo.NugetFeeds");
        }

        /// <inheritdoc />
        public override void Up()
        {
            this.CreateTable(
                "dbo.NugetFeeds",
                c =>
                    new
                        {
                            Id = c.Int(nullable: false, identity: true),
                            Address = c.String(),
                            Password = c.String(),
                            Type = c.Int(nullable: false),
                            UserName = c.String(),
                        }).PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.Releases",
                c =>
                    new
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
                        }).PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.Roles",
                c =>
                    new
                        {
                            Uid = c.Guid(nullable: false),
                            Name = c.String(),
                            AllowedScopeJson = c.String(),
                            DeniedScopeJson = c.String(),
                        }).PrimaryKey(t => t.Uid);

            this.CreateTable(
                "dbo.Users",
                c =>
                    new
                        {
                            Uid = c.Guid(nullable: false),
                            Password = c.String(),
                            ActiveTill = c.DateTimeOffset(precision: 7),
                            BlockedTill = c.DateTimeOffset(precision: 7),
                            IsBlocked = c.Boolean(nullable: false),
                            IsDeleted = c.Boolean(nullable: false),
                            Login = c.String(),
                        }).PrimaryKey(t => t.Uid).Index(t => t.Login, unique: true);

            this.CreateTable(
                "dbo.SeedAddresses",
                c => new { Id = c.Int(nullable: false, identity: true), Address = c.String(), }).PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.NodeTemplates",
                c =>
                    new
                        {
                            Id = c.Int(nullable: false, identity: true),
                            Code = c.String(nullable: false),
                            Configuration = c.String(),
                            ContainerTypesList = c.String(),
                            MaximumNeededInstances = c.Int(),
                            MinimumRequiredInstances = c.Int(nullable: false),
                            Name = c.String(nullable: false),
                            PackagesList = c.String(),
                            Priority = c.Double(nullable: false),
                            Version = c.Int(nullable: false),
                        }).PrimaryKey(t => t.Id);

            this.CreateTable(
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