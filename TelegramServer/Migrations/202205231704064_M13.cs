namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M13 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BaseMessage", "GroupChat_Id", "dbo.GroupChat");
            AddForeignKey("dbo.BaseMessage", "GroupChat_Id", "dbo.GroupChat", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseMessage", "GroupChat_Id", "dbo.GroupChat");
            AddForeignKey("dbo.BaseMessage", "GroupChat_Id", "dbo.GroupChat", "Id");
        }
    }
}
