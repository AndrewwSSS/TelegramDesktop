namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PublicGroupInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupType = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PublicUserInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastVisitDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        PublicGroupInfo_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PublicGroupInfoes", t => t.PublicGroupInfo_Id)
                .Index(t => t.PublicGroupInfo_Id);
            
            AddColumn("dbo.ImageContainers", "PublicGroupInfo_Id", c => c.Int());
            AddColumn("dbo.ImageContainers", "PublicUserInfo_Id", c => c.Int());
            AddColumn("dbo.BaseMessages", "PublicGroupInfo_Id", c => c.Int());
            AddColumn("dbo.BaseMessages", "GroupInfo_Id", c => c.Int());
            CreateIndex("dbo.ImageContainers", "PublicGroupInfo_Id");
            CreateIndex("dbo.ImageContainers", "PublicUserInfo_Id");
            CreateIndex("dbo.BaseMessages", "PublicGroupInfo_Id");
            CreateIndex("dbo.BaseMessages", "GroupInfo_Id");
            AddForeignKey("dbo.ImageContainers", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes", "Id");
            AddForeignKey("dbo.BaseMessages", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes", "Id");
            AddForeignKey("dbo.ImageContainers", "PublicUserInfo_Id", "dbo.PublicUserInfoes", "Id");
            AddForeignKey("dbo.BaseMessages", "GroupInfo_Id", "dbo.PublicGroupInfoes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseMessages", "GroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.PublicUserInfoes", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.ImageContainers", "PublicUserInfo_Id", "dbo.PublicUserInfoes");
            DropForeignKey("dbo.BaseMessages", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.ImageContainers", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropIndex("dbo.PublicUserInfoes", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.BaseMessages", new[] { "GroupInfo_Id" });
            DropIndex("dbo.BaseMessages", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.ImageContainers", new[] { "PublicUserInfo_Id" });
            DropIndex("dbo.ImageContainers", new[] { "PublicGroupInfo_Id" });
            DropColumn("dbo.BaseMessages", "GroupInfo_Id");
            DropColumn("dbo.BaseMessages", "PublicGroupInfo_Id");
            DropColumn("dbo.ImageContainers", "PublicUserInfo_Id");
            DropColumn("dbo.ImageContainers", "PublicGroupInfo_Id");
            DropTable("dbo.PublicUserInfoes");
            DropTable("dbo.PublicGroupInfoes");
        }
    }
}
