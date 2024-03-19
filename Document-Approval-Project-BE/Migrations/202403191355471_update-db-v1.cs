namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DepartmentPersons", "DepartmentId", c => c.Int(nullable: false));
            AddColumn("dbo.DocumentApprovals", "DepartmentId", c => c.Int(nullable: false));
            AlterColumn("dbo.ApprovalPersons", "DocumentApprovalId", c => c.Int(nullable: false));
            DropColumn("dbo.DocumentApprovals", "DepartmentName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DocumentApprovals", "DepartmentName", c => c.String());
            AlterColumn("dbo.ApprovalPersons", "DocumentApprovalId", c => c.String());
            DropColumn("dbo.DocumentApprovals", "DepartmentId");
            DropColumn("dbo.DepartmentPersons", "DepartmentId");
        }
    }
}
