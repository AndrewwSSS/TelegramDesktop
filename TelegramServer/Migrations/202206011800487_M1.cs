namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserGroupChat", "User_Id", "dbo.User");
            DropForeignKey("dbo.UserGroupChat", "GroupChat_Id", "dbo.GroupChat");
            DropIndex("dbo.UserGroupChat", new[] { "User_Id" });
            DropIndex("dbo.UserGroupChat", new[] { "GroupChat_Id" });
            AddColumn("dbo.GroupChat", "User_Id", c => c.Int());
            AddColumn("dbo.User", "GroupChat_Id", c => c.Int());
            AddColumn("dbo.User", "GroupChat_Id1", c => c.Int());
            CreateIndex("dbo.GroupChat", "User_Id");
            CreateIndex("dbo.User", "GroupChat_Id");
            CreateIndex("dbo.User", "GroupChat_Id1");
            AddForeignKey("dbo.GroupChat", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.User", "GroupChat_Id", "dbo.GroupChat", "Id");
            AddForeignKey("dbo.User", "GroupChat_Id1", "dbo.GroupChat", "Id");
            DropTable("dbo.UserGroupChat");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserGroupChat",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        GroupChat_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.GroupChat_Id });
            
            DropForeignKey("dbo.User", "GroupChat_Id1", "dbo.GroupChat");
            DropForeignKey("dbo.User", "GroupChat_Id", "dbo.GroupChat");
            DropForeignKey("dbo.GroupChat", "User_Id", "dbo.User");
            DropIndex("dbo.User", new[] { "GroupChat_Id1" });
            DropIndex("dbo.User", new[] { "GroupChat_Id" });
            DropIndex("dbo.GroupChat", new[] { "User_Id" });
            DropColumn("dbo.User", "GroupChat_Id1");
            DropColumn("dbo.User", "GroupChat_Id");
            DropColumn("dbo.GroupChat", "User_Id");
            CreateIndex("dbo.UserGroupChat", "GroupChat_Id");
            CreateIndex("dbo.UserGroupChat", "User_Id");
            AddForeignKey("dbo.UserGroupChat", "GroupChat_Id", "dbo.GroupChat", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserGroupChat", "User_Id", "dbo.User", "Id", cascadeDelete: true);
        }
    }
}
