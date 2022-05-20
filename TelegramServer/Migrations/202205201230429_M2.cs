namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M2 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BaseMessages", name: "Chat_Id", newName: "GroupChat_Id");
            RenameIndex(table: "dbo.BaseMessages", name: "IX_Chat_Id", newName: "IX_GroupChat_Id");
            AddColumn("dbo.BaseMessages", "GroupId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BaseMessages", "GroupId");
            RenameIndex(table: "dbo.BaseMessages", name: "IX_GroupChat_Id", newName: "IX_Chat_Id");
            RenameColumn(table: "dbo.BaseMessages", name: "GroupChat_Id", newName: "Chat_Id");
        }
    }
}
