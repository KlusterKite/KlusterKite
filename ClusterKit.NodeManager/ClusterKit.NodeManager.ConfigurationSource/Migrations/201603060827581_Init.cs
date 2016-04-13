// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201603060827581_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Initial database creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Initial database creation
    /// </summary>
    public partial class Init : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.DropTable("dbo.NodeTemplates");
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.CreateTable(
                "dbo.NodeTemplates",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Code = c.String(),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }
    }
}