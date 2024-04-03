namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv7 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DocumentApprovalComments", "CommentTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DocumentApprovalComments", "CommentTime", c => c.String());
        }
    }
}
