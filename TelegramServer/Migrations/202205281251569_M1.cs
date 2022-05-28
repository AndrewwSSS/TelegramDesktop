namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.User", "User_Id", "dbo.User");
            DropIndex("dbo.User", new[] { "User_Id" });
            DropColumn("dbo.User", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "User_Id", c => c.Int());
            CreateIndex("dbo.User", "User_Id");
            AddForeignKey("dbo.User", "User_Id", "dbo.User", "Id");
        }
    }
}
