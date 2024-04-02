namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "ParentNode", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalComments", "ParentNode");
        }
    }
}
