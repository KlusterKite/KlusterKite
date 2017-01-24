// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201609131410062_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node configuration database initialization
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// The node configuration database initialization
    /// </summary>
    public partial class Init : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.DropTable("dbo.NodeTemplates");
            this.DropTable("dbo.SeedAddresses");
            this.DropTable("dbo.NugetFeeds");
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.CreateTable(
                "dbo.NugetFeeds",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Address = c.String(),
                    Password = c.String(),
                    Type = c.Int(nullable: false),
                    UserName = c.String(),
                })
                .PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.SeedAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Address = c.String(),
                })
                .PrimaryKey(t => t.Id);

            this.CreateTable(
                "dbo.NodeTemplates",
                c => new
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
                })
                .PrimaryKey(t => t.Id);
        }
    }
}