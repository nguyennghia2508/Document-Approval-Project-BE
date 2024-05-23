namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "CreateBy", c => c.Int(nullable: false));
            AddColumn("dbo.Notifications", "ItemId", c => c.Guid(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Notifications", "ItemId");
            DropColumn("dbo.Notifications", "CreateBy");
        }
    }
}
