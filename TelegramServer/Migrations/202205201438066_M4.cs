namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M4 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BaseMessages", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseMessages", "Type", c => c.Int(nullable: false));
        }
    }
}
