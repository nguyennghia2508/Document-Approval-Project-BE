namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "Index", c => c.Int(nullable: false));
            AddColumn("dbo.ApprovalPersons", "IsApproved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "IsApproved");
            DropColumn("dbo.ApprovalPersons", "Index");
        }
    }
}
