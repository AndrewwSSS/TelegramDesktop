namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migration1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BaseMessages", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseMessages", "UserId", c => c.Int());
        }
    }
}
