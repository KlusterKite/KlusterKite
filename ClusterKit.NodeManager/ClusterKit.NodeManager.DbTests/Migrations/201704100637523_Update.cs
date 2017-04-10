// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201704100637523_Update.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The update migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// The update migration
    /// </summary>
    public partial class Update : DbMigration
    {
        /// <inheritdoc />
        public override void Down()
        {
            this.DropColumn("dbo.TestObjects", "Value");
        }

        /// <inheritdoc />
        public override void Up()
        {
            this.AddColumn("dbo.TestObjects", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}