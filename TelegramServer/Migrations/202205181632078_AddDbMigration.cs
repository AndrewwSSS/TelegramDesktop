namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDbMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BaseMessages", "PublicGroupInfo_Id", c => c.Int());
            AddColumn("dbo.PublicGroupInfoes", "GroupType", c => c.Int(nullable: false));
            CreateIndex("dbo.BaseMessages", "PublicGroupInfo_Id");
            AddForeignKey("dbo.BaseMessages", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseMessages", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropIndex("dbo.BaseMessages", new[] { "PublicGroupInfo_Id" });
            DropColumn("dbo.PublicGroupInfoes", "GroupType");
            DropColumn("dbo.BaseMessages", "PublicGroupInfo_Id");
        }
    }
}
