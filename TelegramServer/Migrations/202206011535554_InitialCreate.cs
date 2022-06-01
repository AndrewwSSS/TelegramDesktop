namespace TelegramServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileContainer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Data = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Banned = c.Boolean(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                        VisitDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserClient",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MachineName = c.String(),
                        Guid = c.String(),
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
                        FromUserId = c.Int(),
                        RepostUserId = c.Int(),
                        Text = c.String(),
                        GroupId = c.Int(),
                        FromUserId1 = c.Int(),
                        GroupId1 = c.Int(),
                        UserId = c.Int(),
                        Guid = c.String(),
                        Result1 = c.Int(),
                        GroupId2 = c.Int(),
                        GroupId3 = c.Int(),
                        NewDescription = c.String(),
                        NewName = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        RespondingTo_Id = c.Int(),
                        PublicGroupInfo_Id = c.Int(),
                        GroupInfo_Id = c.Int(),
                        NewUser_Id = c.Int(),
                        RemovedUser_Id = c.Int(),
                        Group_Id = c.Int(),
                        UserClient_Id = c.Int(),
                        User_Id = c.Int(),
                        GroupChat_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BaseMessage", t => t.RespondingTo_Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.PublicGroupInfo_Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.GroupInfo_Id)
                .ForeignKey("dbo.PublicUserInfo", t => t.NewUser_Id)
                .ForeignKey("dbo.PublicUserInfo", t => t.RemovedUser_Id)
                .ForeignKey("dbo.PublicGroupInfo", t => t.Group_Id)
                .ForeignKey("dbo.UserClient", t => t.UserClient_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.GroupChat", t => t.GroupChat_Id, cascadeDelete: true)
                .Index(t => t.RespondingTo_Id)
                .Index(t => t.PublicGroupInfo_Id)
                .Index(t => t.GroupInfo_Id)
                .Index(t => t.NewUser_Id)
                .Index(t => t.RemovedUser_Id)
                .Index(t => t.Group_Id)
                .Index(t => t.UserClient_Id)
                .Index(t => t.User_Id)
                .Index(t => t.GroupChat_Id);
            
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PreparatoryUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Password = c.String(),
                        Login = c.String(),
                        Email = c.String(),
                        StartTime = c.DateTime(nullable: false),
                        ExpectedCode = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
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
            DropForeignKey("dbo.UserClient", "User_Id", "dbo.User");
            DropForeignKey("dbo.BaseMessage", "UserClient_Id", "dbo.UserClient");
            DropForeignKey("dbo.BaseMessage", "Group_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.BaseMessage", "RemovedUser_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.BaseMessage", "NewUser_Id", "dbo.PublicUserInfo");
            DropForeignKey("dbo.BaseMessage", "GroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.BaseMessage", "PublicGroupInfo_Id", "dbo.PublicGroupInfo");
            DropForeignKey("dbo.BaseMessage", "RespondingTo_Id", "dbo.BaseMessage");
            DropForeignKey("dbo.UserGroupChat", "GroupChat_Id", "dbo.GroupChat");
            DropForeignKey("dbo.UserGroupChat", "User_Id", "dbo.User");
            DropIndex("dbo.UserGroupChat", new[] { "GroupChat_Id" });
            DropIndex("dbo.UserGroupChat", new[] { "User_Id" });
            DropIndex("dbo.BaseMessage", new[] { "GroupChat_Id" });
            DropIndex("dbo.BaseMessage", new[] { "User_Id" });
            DropIndex("dbo.BaseMessage", new[] { "UserClient_Id" });
            DropIndex("dbo.BaseMessage", new[] { "Group_Id" });
            DropIndex("dbo.BaseMessage", new[] { "RemovedUser_Id" });
            DropIndex("dbo.BaseMessage", new[] { "NewUser_Id" });
            DropIndex("dbo.BaseMessage", new[] { "GroupInfo_Id" });
            DropIndex("dbo.BaseMessage", new[] { "PublicGroupInfo_Id" });
            DropIndex("dbo.BaseMessage", new[] { "RespondingTo_Id" });
            DropIndex("dbo.UserClient", new[] { "User_Id" });
            DropTable("dbo.UserGroupChat");
            DropTable("dbo.PreparatoryUser");
            DropTable("dbo.ImageContainer");
            DropTable("dbo.PublicUserInfo");
            DropTable("dbo.PublicGroupInfo");
            DropTable("dbo.BaseMessage");
            DropTable("dbo.UserClient");
            DropTable("dbo.User");
            DropTable("dbo.GroupChat");
            DropTable("dbo.FileContainer");
        }
    }
}
