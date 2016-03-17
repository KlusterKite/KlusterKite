namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedAddresses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SeedAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SeedAddresses");
        }
    }
}
