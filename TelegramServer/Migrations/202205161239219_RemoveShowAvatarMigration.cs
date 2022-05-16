namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveShowAvatarMigration : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ChatMessages", "ShowAvatar");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatMessages", "ShowAvatar", c => c.Boolean(nullable: false));
        }
    }
}
