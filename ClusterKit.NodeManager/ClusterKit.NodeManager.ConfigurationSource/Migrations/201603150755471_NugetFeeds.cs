namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NugetFeeds : DbMigration
    {
        public override void Up()
        {
            CreateTable(
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
        
        public override void Down()
        {
            DropTable("dbo.NugetFeeds");
        }
    }
}
