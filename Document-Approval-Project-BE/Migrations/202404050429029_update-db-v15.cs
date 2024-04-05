namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv15 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocumentApprovalComments", "CommentStatus", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentApprovalComments", "CommentStatus", c => c.Int(nullable: false));
        }
    }
}
