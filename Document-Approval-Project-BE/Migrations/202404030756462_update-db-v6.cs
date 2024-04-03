namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "DocumentApprovalId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalComments", "DocumentApprovalId");
        }
    }
}
