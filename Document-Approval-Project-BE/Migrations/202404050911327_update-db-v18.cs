namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv18 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "IsReject", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "IsReject");
        }
    }
}
