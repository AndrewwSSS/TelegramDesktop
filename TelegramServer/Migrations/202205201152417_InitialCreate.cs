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
                        DateCreated = c.DateTime(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
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
                .ForeignKey("dbo.BaseMessages", t => t.ChatMessage_Id)
                .Index(t => t.GroupChat_Id)
                .Index(t => t.User_Id)
                .Index(t => t.ChatMessage_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Banned = c.Boolean(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                        LastVisitDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.BaseMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Login = c.String(),
                        Name = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        Result = c.Int(),
                        Message = c.String(),
                        RegistrationDate = c.DateTime(),
                        Text = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
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
                .ForeignKey("dbo.BaseMessages", t => t.RespondingTo_Id)
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
                .ForeignKey("dbo.BaseMessages", t => t.ChatMessage_Id)
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
            DropForeignKey("dbo.BaseMessages", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BaseMessages", "ToUser_Id", "dbo.Users");
            DropForeignKey("dbo.BaseMessages", "RespondingTo_Id", "dbo.BaseMessages");
            DropForeignKey("dbo.BaseMessages", "ResendUser_Id", "dbo.Users");
            DropForeignKey("dbo.ImageContainers", "ChatMessage_Id", "dbo.BaseMessages");
            DropForeignKey("dbo.BaseMessages", "FromUser_Id", "dbo.Users");
            DropForeignKey("dbo.FileContainers", "ChatMessage_Id", "dbo.BaseMessages");
            DropForeignKey("dbo.BaseMessages", "Chat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.ImageContainers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserGroupChats", "GroupChat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.UserGroupChats", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ImageContainers", "GroupChat_Id", "dbo.GroupChats");
            DropIndex("dbo.UserGroupChats", new[] { "GroupChat_Id" });
            DropIndex("dbo.UserGroupChats", new[] { "User_Id" });
            DropIndex("dbo.FileContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.BaseMessages", new[] { "User_Id" });
            DropIndex("dbo.BaseMessages", new[] { "ToUser_Id" });
            DropIndex("dbo.BaseMessages", new[] { "RespondingTo_Id" });
            DropIndex("dbo.BaseMessages", new[] { "ResendUser_Id" });
            DropIndex("dbo.BaseMessages", new[] { "FromUser_Id" });
            DropIndex("dbo.BaseMessages", new[] { "Chat_Id" });
            DropIndex("dbo.Users", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ImageContainers", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "GroupChat_Id" });
            DropTable("dbo.UserGroupChats");
            DropTable("dbo.FileContainers");
            DropTable("dbo.BaseMessages");
            DropTable("dbo.Users");
            DropTable("dbo.ImageContainers");
            DropTable("dbo.GroupChats");
        }
    }
}
