namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class NodeTemplatesFields : DbMigration
    {
        public override void Down()
        {
            AlterColumn("dbo.NodeTemplates", "Name", c => c.String());
            AlterColumn("dbo.NodeTemplates", "Code", c => c.String());
            DropColumn("dbo.NodeTemplates", "Priority");
            DropColumn("dbo.NodeTemplates", "PackagesList");
            DropColumn("dbo.NodeTemplates", "MininmumRequiredInstances");
            DropColumn("dbo.NodeTemplates", "MaximumNeededInstances");
            DropColumn("dbo.NodeTemplates", "ContainerTypesList");
            DropColumn("dbo.NodeTemplates", "Configuration");
        }

        public override void Up()
        {
            AddColumn("dbo.NodeTemplates", "Configuration", c => c.String());
            AddColumn("dbo.NodeTemplates", "ContainerTypesList", c => c.String());
            AddColumn("dbo.NodeTemplates", "MaximumNeededInstances", c => c.Int());
            AddColumn("dbo.NodeTemplates", "MininmumRequiredInstances", c => c.Int(nullable: false));
            AddColumn("dbo.NodeTemplates", "PackagesList", c => c.String());
            AddColumn("dbo.NodeTemplates", "Priority", c => c.Double(nullable: false));
            AlterColumn("dbo.NodeTemplates", "Code", c => c.String(nullable: false));
            AlterColumn("dbo.NodeTemplates", "Name", c => c.String(nullable: false));
        }
    }
}