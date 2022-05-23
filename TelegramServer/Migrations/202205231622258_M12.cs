namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M12 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PublicUserInfo", "RegistrationDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PublicUserInfo", "RegistrationDate", c => c.DateTime(nullable: false));
        }
    }
}
