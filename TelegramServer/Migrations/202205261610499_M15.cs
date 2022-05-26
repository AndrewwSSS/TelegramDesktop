namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M15 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ImageContainer", "GroupChat_Id", "dbo.GroupChat");
            DropForeignKey("dbo.ImageContainer", "User_Id", "dbo.User");
            DropForeignKey("dbo.FileContainer", "ChatMessage_Id", "dbo.BaseMessage");
            DropForeignKey("dbo.ImageContainer", "ChatMessage_Id", "dbo.BaseMessage");
            DropForeignKey("dbo.ImageContainer", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.ImageContainer", "PublicUserInfo_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.PublicUserInfo", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.BaseMessage", "User_Id", "dbo.User");
            DropIndex("dbo.ImageContainer", new[] { "GroupChat_Id" });
            DropIndex("dbo.ImageContainer", new[] { "User_Id" });
            DropIndex("dbo.ImageContainer", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ImageContainer", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.ImageContainer", new[] { "PublicUserInfo_Id" });
            DropIndex("dbo.FileContainer", new[] { "ChatMessage_Id" });
            DropIndex("dbo.PublicUserInfo", new[] { "PublicGroupInfo_Id" });
            AddColumn("dbo.User", "VisitDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.BaseMessage", "User_Id1", c => c.Int());
            CreateIndex("dbo.BaseMessage", "User_Id1");
            AddForeignKey("dbo.BaseMessage", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.BaseMessage", "User_Id1", "dbo.User", "Id");
            DropColumn("dbo.ImageContainer", "GroupChat_Id");
            DropColumn("dbo.ImageContainer", "User_Id");
            DropColumn("dbo.ImageContainer", "ChatMessage_Id");
            DropColumn("dbo.ImageContainer", "PublicGroupInfo_Id");
            DropColumn("dbo.ImageContainer", "PublicUserInfo_Id");
            DropColumn("dbo.User", "LastVisitDate");
            DropColumn("dbo.FileContainer", "ChatMessage_Id");
            DropColumn("dbo.PublicUserInfo", "PublicGroupInfo_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PublicUserInfo", "PublicGroupInfo_Id", c => c.Int());
            AddColumn("dbo.FileContainer", "ChatMessage_Id", c => c.Int());
            AddColumn("dbo.User", "LastVisitDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ImageContainer", "PublicUserInfo_Id", c => c.Int());
            AddColumn("dbo.ImageContainer", "PublicGroupInfo_Id", c => c.Int());
            AddColumn("dbo.ImageContainer", "ChatMessage_Id", c => c.Int());
            AddColumn("dbo.ImageContainer", "User_Id", c => c.Int());
            AddColumn("dbo.ImageContainer", "GroupChat_Id", c => c.Int());
            DropForeignKey("dbo.BaseMessage", "User_Id1", "dbo.User");
            DropForeignKey("dbo.BaseMessage", "User_Id", "dbo.User");
            DropIndex("dbo.BaseMessage", new[] { "User_Id1" });
            DropColumn("dbo.BaseMessage", "User_Id1");
            DropColumn("dbo.User", "VisitDate");
            CreateIndex("dbo.PublicUserInfo", "PublicGroupInfo_Id");
            CreateIndex("dbo.FileContainer", "ChatMessage_Id");
            CreateIndex("dbo.ImageContainer", "PublicUserInfo_Id");
            CreateIndex("dbo.ImageContainer", "PublicGroupInfo_Id");
            CreateIndex("dbo.ImageContainer", "ChatMessage_Id");
            CreateIndex("dbo.ImageContainer", "User_Id");
            CreateIndex("dbo.ImageContainer", "GroupChat_Id");
            AddForeignKey("dbo.BaseMessage", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.PublicUserInfo", "PublicGroupInfo_Id", "dbo.PublicGroupInfo", "Id");
            AddForeignKey("dbo.ImageContainer", "PublicUserInfo_Id", "dbo.PublicUserInfo", "Id");
            AddForeignKey("dbo.ImageContainer", "PublicGroupInfo_Id", "dbo.PublicGroupInfo", "Id");
            AddForeignKey("dbo.ImageContainer", "ChatMessage_Id", "dbo.BaseMessage", "Id");
            AddForeignKey("dbo.FileContainer", "ChatMessage_Id", "dbo.BaseMessage", "Id");
            AddForeignKey("dbo.ImageContainer", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.ImageContainer", "GroupChat_Id", "dbo.GroupChat", "Id");
        }
    }
}
