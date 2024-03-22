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
                        Id = c.Int(nullable: false, identity: true),
                        ApprovalPersonId = c.Guid(nullable: false),
                        ApprovalPersonName = c.String(),
                        DocumentApprovalId = c.Guid(nullable: false),
                        PersonDuty = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Guid(nullable: false),
                        CategoryName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DepartmentPersons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Guid(nullable: false),
                        PersonName = c.String(),
                        PersonMail = c.String(),
                        PersonPosition = c.String(),
                        DepartmentId = c.Guid(nullable: false),
                        DepartmentPosition = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Guid(nullable: false),
                        DepartmentName = c.String(),
                        ParentNode = c.Guid(nullable: false),
                        DepartmentLevel = c.Int(nullable: false),
                        ChildrenNode = c.Guid(nullable: false),
                        ContactInfo = c.String(),
                        DepartmentCode = c.String(),
                        DepartmentManager = c.Guid(nullable: false),
                        Supervisor = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentApprovalComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommentId = c.Guid(nullable: false),
                        ApprovalPersonId = c.Guid(nullable: false),
                        CommentContent = c.String(),
                        CommentTime = c.String(),
                        IsSubComment = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentApprovalFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentFileId = c.Guid(nullable: false),
                        DocumentApprovalId = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        FilePath = c.String(),
                        FileSize = c.Long(nullable: false),
                        DocumentType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentApprovals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentApprovalId = c.Guid(nullable: false),
                        ApplicantId = c.Guid(nullable: false),
                        ApplicantName = c.String(),
                        DepartmentId = c.Guid(nullable: false),
                        SectionId = c.Guid(nullable: false),
                        UnitId = c.Guid(nullable: false),
                        CategoryId = c.Guid(nullable: false),
                        RelatedProposal = c.String(),
                        Subject = c.String(),
                        ContentSum = c.String(),
                        CreateDate = c.String(),
                        Status = c.Int(nullable: false),
                        PresentApplicant = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentTypeId = c.Guid(nullable: false),
                        DocumentTypeName = c.String(),
                        CategoryId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        Username = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Birtday = c.String(),
                        Position = c.String(),
                        Gender = c.Int(nullable: false),
                        JobTitle = c.String(),
                        Company = c.String(),
                        DepartmentId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.DocumentApprovals");
            DropTable("dbo.DocumentApprovalFiles");
            DropTable("dbo.DocumentApprovalComments");
            DropTable("dbo.Departments");
            DropTable("dbo.DepartmentPersons");
            DropTable("dbo.Categories");
            DropTable("dbo.ApprovalPersons");
        }
    }
}
