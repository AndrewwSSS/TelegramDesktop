namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PublicUserInfoes", "Login", c => c.String());
            AddColumn("dbo.PublicUserInfoes", "RegistrationDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.PublicUserInfoes", "LastVisitDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PublicUserInfoes", "LastVisitDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.PublicUserInfoes", "RegistrationDate");
            DropColumn("dbo.PublicUserInfoes", "Login");
        }
    }
}
