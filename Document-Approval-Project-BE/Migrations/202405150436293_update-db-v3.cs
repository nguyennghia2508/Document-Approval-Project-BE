namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "IsHidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "IsHidden");
        }
    }
}
