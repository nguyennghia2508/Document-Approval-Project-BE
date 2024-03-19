namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApprovalPersons",
                c => new
                    {
                        ApprovalPersonId = c.Int(nullable: false, identity: true),
                        ApprovalPersonName = c.String(),
                        DocumentApprovalId = c.String(),
                    })
                .PrimaryKey(t => t.ApprovalPersonId);
            
            CreateTable(
                "dbo.DepartmentPersons",
                c => new
                    {
                        PersonId = c.Int(nullable: false, identity: true),
                        PersonName = c.String(),
                        PersonMail = c.String(),
                        PersonPosition = c.String(),
                        DepartmentPosition = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PersonId);
            
            AddColumn("dbo.DocumentApprovals", "PresentApplicant", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovals", "PresentApplicant");
            DropTable("dbo.DepartmentPersons");
            DropTable("dbo.ApprovalPersons");
        }
    }
}
