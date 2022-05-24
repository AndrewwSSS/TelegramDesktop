namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M14 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BaseMessage", "RegistrationDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseMessage", "RegistrationDate", c => c.DateTime());
        }
    }
}
