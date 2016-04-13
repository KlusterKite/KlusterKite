// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201603121619169_NodeTemplateVersion.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Template version
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Template version
    /// </summary>
    public partial class NodeTemplateVersion : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.DropColumn("dbo.NodeTemplates", "Version");
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.AddColumn("dbo.NodeTemplates", "Version", c => c.Int(nullable: false, defaultValue: 0));
        }
    }
}