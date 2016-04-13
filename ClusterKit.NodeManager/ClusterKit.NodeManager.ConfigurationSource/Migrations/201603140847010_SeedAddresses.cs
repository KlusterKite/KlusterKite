// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201603140847010_SeedAddresses.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Seed addresses
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Seed addresses
    /// </summary>
    public partial class SeedAddresses : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.DropTable("dbo.SeedAddresses");
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.CreateTable(
                "dbo.SeedAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Address = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }
    }
}