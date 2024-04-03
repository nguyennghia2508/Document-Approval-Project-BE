namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalFiles", "CommentId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalFiles", "CommentId");
        }
    }
}
