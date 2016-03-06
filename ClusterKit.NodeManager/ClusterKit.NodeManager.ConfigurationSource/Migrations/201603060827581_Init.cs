namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Init : DbMigration
    {
        public override void Down()
        {
            DropTable("dbo.NodeTemplates");
        }

        public override void Up()
        {
            CreateTable(
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