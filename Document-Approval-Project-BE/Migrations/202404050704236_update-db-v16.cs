namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv16 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovalComments", "IsFirst", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovalComments", "IsFirst");
        }
    }
}
