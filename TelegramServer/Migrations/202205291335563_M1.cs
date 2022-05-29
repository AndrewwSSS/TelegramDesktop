namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BaseMessage", "User_Id1", "dbo.User");
            DropIndex("dbo.BaseMessage", new[] { "User_Id1" });
            AddColumn("dbo.BaseMessage", "Guid", c => c.String());
            AddColumn("dbo.BaseMessage", "UserClient_Id", c => c.Int());
            CreateIndex("dbo.BaseMessage", "UserClient_Id");
            AddForeignKey("dbo.BaseMessage", "UserClient_Id", "dbo.UserClient", "Id");
            DropColumn("dbo.BaseMessage", "User_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseMessage", "User_Id1", c => c.Int());
            DropForeignKey("dbo.BaseMessage", "UserClient_Id", "dbo.UserClient");
            DropIndex("dbo.BaseMessage", new[] { "UserClient_Id" });
            DropColumn("dbo.BaseMessage", "UserClient_Id");
            DropColumn("dbo.BaseMessage", "Guid");
            CreateIndex("dbo.BaseMessage", "User_Id1");
            AddForeignKey("dbo.BaseMessage", "User_Id1", "dbo.User", "Id");
        }
    }
}
