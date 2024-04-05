namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv14 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "CommentStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalComments", "CommentStatus");
        }
    }
}
