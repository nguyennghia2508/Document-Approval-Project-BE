namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "ForwardName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalComments", "ForwardName");
        }
    }
}
