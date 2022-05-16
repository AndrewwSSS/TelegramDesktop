namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupChats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ImageContainers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Data = c.Binary(),
                        GroupChat_Id = c.Int(),
                        User_Id = c.Int(),
                        ChatMessage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupChats", t => t.GroupChat_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.ChatMessages", t => t.ChatMessage_Id)
                .Index(t => t.GroupChat_Id)
                .Index(t => t.User_Id)
                .Index(t => t.ChatMessage_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Name = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        RegistrationDate = c.DateTime(nullable: false),
                        LastVisitDate = c.DateTime(nullable: false),
                        ProfileDescription = c.String(),
                        Banned = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShowAvatar = c.Boolean(nullable: false),
                        Text = c.String(),
                        Type = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Chat_Id = c.Int(),
                        FromUser_Id = c.Int(),
                        ResendUser_Id = c.Int(),
                        RespondingTo_Id = c.Int(),
                        ToUser_Id = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupChats", t => t.Chat_Id)
                .ForeignKey("dbo.Users", t => t.FromUser_Id)
                .ForeignKey("dbo.Users", t => t.ResendUser_Id)
                .ForeignKey("dbo.ChatMessages", t => t.RespondingTo_Id)
                .ForeignKey("dbo.Users", t => t.ToUser_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.Chat_Id)
                .Index(t => t.FromUser_Id)
                .Index(t => t.ResendUser_Id)
                .Index(t => t.RespondingTo_Id)
                .Index(t => t.ToUser_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.FileContainers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Data = c.Binary(),
                        ChatMessage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatMessages", t => t.ChatMessage_Id)
                .Index(t => t.ChatMessage_Id);
            
            CreateTable(
                "dbo.UserGroupChats",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        GroupChat_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.GroupChat_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.GroupChats", t => t.GroupChat_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.GroupChat_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "ToUser_Id", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "RespondingTo_Id", "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "ResendUser_Id", "dbo.Users");
            DropForeignKey("dbo.ImageContainers", "ChatMessage_Id", "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "FromUser_Id", "dbo.Users");
            DropForeignKey("dbo.FileContainers", "ChatMessage_Id", "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "Chat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.ImageContainers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserGroupChats", "GroupChat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.UserGroupChats", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ImageContainers", "GroupChat_Id", "dbo.GroupChats");
            DropIndex("dbo.UserGroupChats", new[] { "GroupChat_Id" });
            DropIndex("dbo.UserGroupChats", new[] { "User_Id" });
            DropIndex("dbo.FileContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ChatMessages", new[] { "User_Id" });
            DropIndex("dbo.ChatMessages", new[] { "ToUser_Id" });
            DropIndex("dbo.ChatMessages", new[] { "RespondingTo_Id" });
            DropIndex("dbo.ChatMessages", new[] { "ResendUser_Id" });
            DropIndex("dbo.ChatMessages", new[] { "FromUser_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Chat_Id" });
            DropIndex("dbo.Users", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ImageContainers", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "GroupChat_Id" });
            DropTable("dbo.UserGroupChats");
            DropTable("dbo.FileContainers");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.Users");
            DropTable("dbo.ImageContainers");
            DropTable("dbo.GroupChats");
        }
    }
}
