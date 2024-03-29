namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovals", "ProcessingBy", c => c.String());
            AddColumn("dbo.DocumentApprovals", "IsProcessing", c => c.Boolean(nullable: false));
            AddColumn("dbo.DocumentApprovals", "IsSigningProcess", c => c.Boolean(nullable: false));
            AddColumn("dbo.DocumentApprovals", "SharingToUsers", c => c.Int(nullable: false));
            DropColumn("dbo.DocumentApprovals", "PresentApplicant");
            DropColumn("dbo.DocumentApprovals", "PresentApplicantName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DocumentApprovals", "PresentApplicantName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "PresentApplicant", c => c.Int(nullable: false));
            DropColumn("dbo.DocumentApprovals", "SharingToUsers");
            DropColumn("dbo.DocumentApprovals", "IsSigningProcess");
            DropColumn("dbo.DocumentApprovals", "IsProcessing");
            DropColumn("dbo.DocumentApprovals", "ProcessingBy");
        }
    }
}
