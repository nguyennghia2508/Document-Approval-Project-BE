namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovals", "DepartmentName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "SectionName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "UnitName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "CategoryName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "DocumentTypeName", c => c.String());
            AddColumn("dbo.DocumentApprovals", "PresentApplicantName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovals", "PresentApplicantName");
            DropColumn("dbo.DocumentApprovals", "DocumentTypeName");
            DropColumn("dbo.DocumentApprovals", "CategoryName");
            DropColumn("dbo.DocumentApprovals", "UnitName");
            DropColumn("dbo.DocumentApprovals", "SectionName");
            DropColumn("dbo.DocumentApprovals", "DepartmentName");
        }
    }
}
