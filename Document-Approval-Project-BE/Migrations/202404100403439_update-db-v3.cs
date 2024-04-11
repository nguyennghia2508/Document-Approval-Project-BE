namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "ApprovalPersonEmail", c => c.String());
            DropColumn("dbo.ApprovalPersons", "ApprovalEmail");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ApprovalPersons", "ApprovalEmail", c => c.String());
            DropColumn("dbo.ApprovalPersons", "ApprovalPersonEmail");
        }
    }
}
