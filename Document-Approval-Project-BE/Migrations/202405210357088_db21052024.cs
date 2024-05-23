namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class db21052024 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Modules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ModuleId = c.Guid(nullable: false),
                        ModuleName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationId = c.Guid(nullable: false),
                        ModuleId = c.Int(nullable: false),
                        Avatar = c.String(),
                        CreateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreateBy = c.String(),
                        ItemId = c.Int(nullable: false),
                        Type = c.String(),
                        Parameters = c.String(),
                        Url = c.String(),
                        IsHidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.ApprovalPersons", "IsShare");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ApprovalPersons", "IsShare", c => c.Boolean(nullable: false));
            DropTable("dbo.Notifications");
            DropTable("dbo.Modules");
        }
    }
}
