namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Notifications", "ItemId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "ItemId", c => c.String());
        }
    }
}
