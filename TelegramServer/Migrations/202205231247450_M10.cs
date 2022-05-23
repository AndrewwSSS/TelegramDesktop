namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M10 : DbMigration
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
                        PublicGroupInfo_Id = c.Int(),
                        PublicUserInfo_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupChats", t => t.GroupChat_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.BaseMessages", t => t.ChatMessage_Id)
                .ForeignKey("dbo.PublicGroupInfoes", t => t.PublicGroupInfo_Id)
                .ForeignKey("dbo.PublicUserInfoes", t => t.PublicUserInfo_Id)
                .Index(t => t.GroupChat_Id)
                .Index(t => t.User_Id)
                .Index(t => t.ChatMessage_Id)
                .Index(t => t.PublicGroupInfo_Id)
                .Index(t => t.PublicUserInfo_Id);
            
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
                        Time = c.DateTime(nullable: false),
                        Login = c.String(),
                        Name = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        Result = c.Int(),
                        Message = c.String(),
                        RegistrationDate = c.DateTime(),
                        FromUserId = c.Int(),
                        RepostUserId = c.Int(),
                        RespondingTo = c.Int(),
                        Text = c.String(),
                        GroupId = c.Int(),
                        FromUserId1 = c.Int(),
                        GroupId1 = c.Int(),
                        UserId = c.Int(),
                        Result1 = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        PublicGroupInfo_Id = c.Int(),
                        GroupInfo_Id = c.Int(),
                        User_Id = c.Int(),
                        GroupChat_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PublicGroupInfoes", t => t.PublicGroupInfo_Id)
                .ForeignKey("dbo.PublicGroupInfoes", t => t.GroupInfo_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.GroupChats", t => t.GroupChat_Id)
                .Index(t => t.PublicGroupInfo_Id)
                .Index(t => t.GroupInfo_Id)
                .Index(t => t.User_Id)
                .Index(t => t.GroupChat_Id);
            
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
                "dbo.PublicGroupInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupType = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PublicUserInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        RegistrationDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        PublicGroupInfo_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PublicGroupInfoes", t => t.PublicGroupInfo_Id)
                .Index(t => t.PublicGroupInfo_Id);
            
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
            DropForeignKey("dbo.BaseMessages", "GroupChat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.BaseMessages", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BaseMessages", "GroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.PublicUserInfoes", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.ImageContainers", "PublicUserInfo_Id", "dbo.PublicUserInfoes");
            DropForeignKey("dbo.BaseMessages", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.ImageContainers", "PublicGroupInfo_Id", "dbo.PublicGroupInfoes");
            DropForeignKey("dbo.ImageContainers", "ChatMessage_Id", "dbo.BaseMessages");
            DropForeignKey("dbo.FileContainers", "ChatMessage_Id", "dbo.BaseMessages");
            DropForeignKey("dbo.ImageContainers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserGroupChats", "GroupChat_Id", "dbo.GroupChats");
            DropForeignKey("dbo.UserGroupChats", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ImageContainers", "GroupChat_Id", "dbo.GroupChats");
            DropIndex("dbo.UserGroupChats", new[] { "GroupChat_Id" });
            DropIndex("dbo.UserGroupChats", new[] { "User_Id" });
            DropIndex("dbo.PublicUserInfoes", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.FileContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.BaseMessages", new[] { "GroupChat_Id" });
            DropIndex("dbo.BaseMessages", new[] { "User_Id" });
            DropIndex("dbo.BaseMessages", new[] { "GroupInfo_Id" });
            DropIndex("dbo.BaseMessages", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.Users", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "PublicUserInfo_Id" });
            DropIndex("dbo.ImageContainers", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.ImageContainers", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ImageContainers", new[] { "User_Id" });
            DropIndex("dbo.ImageContainers", new[] { "GroupChat_Id" });
            DropTable("dbo.UserGroupChats");
            DropTable("dbo.PublicUserInfoes");
            DropTable("dbo.PublicGroupInfoes");
            DropTable("dbo.FileContainers");
            DropTable("dbo.BaseMessages");
            DropTable("dbo.Users");
            DropTable("dbo.ImageContainers");
            DropTable("dbo.GroupChats");
        }
    }
}
