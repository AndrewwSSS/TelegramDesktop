namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupChat",
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
                "dbo.ImageContainer",
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
                .ForeignKey("dbo.GroupChat", t => t.GroupChat_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.BaseMessage", t => t.ChatMessage_Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.PublicGroupInfo_Id)
                .ForeignKey("dbo.PublicUserInfo", t => t.PublicUserInfo_Id)
                .Index(t => t.GroupChat_Id)
                .Index(t => t.User_Id)
                .Index(t => t.ChatMessage_Id)
                .Index(t => t.PublicGroupInfo_Id)
                .Index(t => t.PublicUserInfo_Id);
            
            CreateTable(
                "dbo.User",
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
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.BaseMessage",
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
                        GroupId2 = c.Int(),
                        NewDescription = c.String(),
                        NewName = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        PublicGroupInfo_Id = c.Int(),
                        GroupInfo_Id = c.Int(),
                        NewUser_Id = c.Int(),
                        RemovedUser_Id = c.Int(),
                        User_Id = c.Int(),
                        GroupChat_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.PublicGroupInfo_Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.GroupInfo_Id)
                .ForeignKey("dbo.PublicUserInfo", t => t.NewUser_Id)
                .ForeignKey("dbo.PublicUserInfo", t => t.RemovedUser_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.GroupChat", t => t.GroupChat_Id)
                .Index(t => t.PublicGroupInfo_Id)
                .Index(t => t.GroupInfo_Id)
                .Index(t => t.NewUser_Id)
                .Index(t => t.RemovedUser_Id)
                .Index(t => t.User_Id)
                .Index(t => t.GroupChat_Id);
            
            CreateTable(
                "dbo.FileContainer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Data = c.Binary(),
                        ChatMessage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BaseMessage", t => t.ChatMessage_Id)
                .Index(t => t.ChatMessage_Id);
            
            CreateTable(
                "dbo.PublicGroupInfo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupType = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PublicUserInfo",
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
                .ForeignKey("dbo.PublicGroupInfo", t => t.PublicGroupInfo_Id)
                .Index(t => t.PublicGroupInfo_Id);
            
            CreateTable(
                "dbo.UserGroupChat",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        GroupChat_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.GroupChat_Id })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.GroupChat", t => t.GroupChat_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.GroupChat_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseMessage", "GroupChat_Id", "dbo.GroupChat");
            DropForeignKey("dbo.BaseMessage", "User_Id", "dbo.User");
            DropForeignKey("dbo.BaseMessage", "RemovedUser_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.BaseMessage", "NewUser_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.BaseMessage", "GroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.PublicUserInfo", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.ImageContainer", "PublicUserInfo_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.BaseMessage", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.ImageContainer", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.ImageContainer", "ChatMessage_Id", "dbo.BaseMessage");
            DropForeignKey("dbo.FileContainer", "ChatMessage_Id", "dbo.BaseMessage");
            DropForeignKey("dbo.ImageContainer", "User_Id", "dbo.User");
            DropForeignKey("dbo.UserGroupChat", "GroupChat_Id", "dbo.GroupChat");
            DropForeignKey("dbo.UserGroupChat", "User_Id", "dbo.User");
            DropForeignKey("dbo.User", "User_Id", "dbo.User");
            DropForeignKey("dbo.ImageContainer", "GroupChat_Id", "dbo.GroupChat");
            DropIndex("dbo.UserGroupChat", new[] { "GroupChat_Id" });
            DropIndex("dbo.UserGroupChat", new[] { "User_Id" });
            DropIndex("dbo.PublicUserInfo", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.FileContainer", new[] { "ChatMessage_Id" });
            DropIndex("dbo.BaseMessage", new[] { "GroupChat_Id" });
            DropIndex("dbo.BaseMessage", new[] { "User_Id" });
            DropIndex("dbo.BaseMessage", new[] { "RemovedUser_Id" });
            DropIndex("dbo.BaseMessage", new[] { "NewUser_Id" });
            DropIndex("dbo.BaseMessage", new[] { "GroupInfo_Id" });
            DropIndex("dbo.BaseMessage", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.User", new[] { "User_Id" });
            DropIndex("dbo.ImageContainer", new[] { "PublicUserInfo_Id" });
            DropIndex("dbo.ImageContainer", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.ImageContainer", new[] { "ChatMessage_Id" });
            DropIndex("dbo.ImageContainer", new[] { "User_Id" });
            DropIndex("dbo.ImageContainer", new[] { "GroupChat_Id" });
            DropTable("dbo.UserGroupChat");
            DropTable("dbo.PublicUserInfo");
            DropTable("dbo.PublicGroupInfo");
            DropTable("dbo.FileContainer");
            DropTable("dbo.BaseMessage");
            DropTable("dbo.User");
            DropTable("dbo.ImageContainer");
            DropTable("dbo.GroupChat");
        }
    }
}
