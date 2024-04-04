namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "Comment");
        }
    }
}
