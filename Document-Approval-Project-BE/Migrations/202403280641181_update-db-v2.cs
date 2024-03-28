namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ApprovalPersons", "ApprovalPersonId");

            AddColumn("dbo.ApprovalPersons", "ApprovalPersonId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "ApprovalPersonId");

            AddColumn("dbo.ApprovalPersons", "ApprovalPersonId", c => c.Guid(nullable: false));
        }
    }
}
