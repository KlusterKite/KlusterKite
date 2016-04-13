// --------------------------------------------------------------------------------------------------------------------
// <copyright file="201603121111201_NodeTemplatesFields.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Add package related fields
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Add package related fields
    /// </summary>
    public partial class NodeTemplatesFields : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.AlterColumn("dbo.NodeTemplates", "Name", c => c.String());
            this.AlterColumn("dbo.NodeTemplates", "Code", c => c.String());
            this.DropColumn("dbo.NodeTemplates", "Priority");
            this.DropColumn("dbo.NodeTemplates", "PackagesList");
            this.DropColumn("dbo.NodeTemplates", "MininmumRequiredInstances");
            this.DropColumn("dbo.NodeTemplates", "MaximumNeededInstances");
            this.DropColumn("dbo.NodeTemplates", "ContainerTypesList");
            this.DropColumn("dbo.NodeTemplates", "Configuration");
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.AddColumn("dbo.NodeTemplates", "Configuration", c => c.String());
            this.AddColumn("dbo.NodeTemplates", "ContainerTypesList", c => c.String());
            this.AddColumn("dbo.NodeTemplates", "MaximumNeededInstances", c => c.Int());
            this.AddColumn("dbo.NodeTemplates", "MininmumRequiredInstances", c => c.Int(nullable: false));
            this.AddColumn("dbo.NodeTemplates", "PackagesList", c => c.String());
            this.AddColumn("dbo.NodeTemplates", "Priority", c => c.Double(nullable: false));
            this.AlterColumn("dbo.NodeTemplates", "Code", c => c.String(nullable: false));
            this.AlterColumn("dbo.NodeTemplates", "Name", c => c.String(nullable: false));
        }
    }
}