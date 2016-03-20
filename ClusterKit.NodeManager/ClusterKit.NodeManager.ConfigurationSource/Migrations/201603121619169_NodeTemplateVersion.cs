namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class NodeTemplateVersion : DbMigration
    {
        public override void Down()
        {
            DropColumn("dbo.NodeTemplates", "Version");
        }

        public override void Up()
        {
            AddColumn("dbo.NodeTemplates", "Version", c => c.Int(nullable: false, defaultValue: 0));
        }
    }
}