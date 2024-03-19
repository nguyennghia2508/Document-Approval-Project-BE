namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createdb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
               "dbo.Users",
               c => new
               {
                   UserId = c.Int(nullable: false, identity: true),
                   Username = c.String(),
                   Password = c.String(),
                   Email = c.String(),
                   FirstName = c.String(),
                   LastName = c.String(),
                   Birtday = c.DateTime(nullable: false),
                   Position = c.String(),
                   Gender = c.Int(nullable: false),
                   JobTitle = c.String(),
                   Company = c.String(),
               })
       .PrimaryKey(t => t.UserId);
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        DepartmentName = c.Int(nullable: false),
                        ParentNode = c.String(),
                        ContactInfo = c.String(),
                        DepartmentCode = c.String(),
                        DepartmentManager = c.String(),
                        Supervisor = c.String(),
                    })
                .PrimaryKey(t => t.DepartmentId);
            
            CreateTable(
                "dbo.DocumentApprovalFiles",
                c => new
                    {
                        DocumentFileId = c.Int(nullable: false, identity: true),
                        DocumentApprovalId = c.Int(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        FilePath = c.String(),
                        FileSize = c.String(),
                        DocumentType = c.String(),
                    })
                .PrimaryKey(t => t.DocumentFileId);
            
            CreateTable(
                "dbo.DocumentApprovals",
                c => new
                    {
                        DocumentApprovalId = c.Int(nullable: false, identity: true),
                        Applicant = c.String(),
                        DepartmentName = c.String(),
                        CategoryName = c.String(),
                        RelatedProposal = c.String(),
                        Subject = c.String(),
                        ContentSum = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentApprovalId);
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.DocumentApprovals");
            DropTable("dbo.DocumentApprovalFiles");
            DropTable("dbo.Departments");
        }
    }
}
