namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "IsApprove", c => c.Boolean(nullable: false));
            AddColumn("dbo.ApprovalPersons", "IsSign", c => c.Boolean(nullable: false));
            DropColumn("dbo.ApprovalPersons", "IsApproved");
            DropColumn("dbo.DocumentApprovals", "IsProcessing");
            DropColumn("dbo.DocumentApprovals", "IsSigningProcess");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DocumentApprovals", "IsSigningProcess", c => c.Boolean(nullable: false));
            AddColumn("dbo.DocumentApprovals", "IsProcessing", c => c.Boolean(nullable: false));
            AddColumn("dbo.ApprovalPersons", "IsApproved", c => c.Boolean(nullable: false));
            DropColumn("dbo.ApprovalPersons", "IsSign");
            DropColumn("dbo.ApprovalPersons", "IsApprove");
        }
    }
}
