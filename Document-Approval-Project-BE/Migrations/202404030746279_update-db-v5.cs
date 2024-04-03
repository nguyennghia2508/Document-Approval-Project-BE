namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "ApprovalPersonId", c => c.Int());
            AlterColumn("dbo.DocumentApprovalComments", "ParentNode", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentApprovalComments", "ParentNode", c => c.Int(nullable: false));
            DropColumn("dbo.DocumentApprovalComments", "ApprovalPersonId");
        }
    }
}
