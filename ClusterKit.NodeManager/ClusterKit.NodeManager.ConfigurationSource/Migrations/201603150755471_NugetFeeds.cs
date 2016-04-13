// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201603150755471_NugetFeeds.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Nuget feeds
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Nuget feeds
    /// </summary>
    public partial class NugetFeeds : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
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
        }
    }
}