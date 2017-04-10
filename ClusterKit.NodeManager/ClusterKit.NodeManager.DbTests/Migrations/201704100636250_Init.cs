// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201704100636250_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The start migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// The start migration
    /// </summary>
    public partial class Init : DbMigration
    {
        /// <inheritdoc />
        public override void Down()
        {
            this.DropTable("dbo.TestObjects");
        }

        /// <inheritdoc />
        public override void Up()
        {
            this.CreateTable(
                    "dbo.TestObjects",
                    c => new { Id = c.Int(nullable: false, identity: true), Name = c.String(), })
                .PrimaryKey(t => t.Id);
        }
    }
}