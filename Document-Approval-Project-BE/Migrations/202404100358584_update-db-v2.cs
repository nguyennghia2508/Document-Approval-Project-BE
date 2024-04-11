namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "ApprovalEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "ApprovalEmail");
        }
    }
}
