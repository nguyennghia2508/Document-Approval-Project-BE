namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApprovalPersons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Index = c.Int(nullable: false),
                        ApprovalPersonId = c.Int(nullable: false),
                        ApprovalPersonName = c.String(),
                        DocumentApprovalId = c.Guid(nullable: false),
                        PersonDuty = c.Int(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentApprovalComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommentId = c.Guid(nullable: false),
                        ApprovalPersonId = c.Guid(nullable: false),
                        CreateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CommentContent = c.String(),
                        CommentTime = c.String(),
                        ParentNode = c.Int(nullable: false),
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
                        DocumentType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentApprovals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestCode = c.String(),
                        DocumentApprovalId = c.Guid(nullable: false),
                        ApplicantId = c.Int(),
                        ApplicantName = c.String(),
                        DepartmentId = c.Int(nullable: false),
                        DepartmentName = c.String(),
                        SectionId = c.Int(nullable: false),
                        SectionName = c.String(),
                        UnitId = c.Int(),
                        UnitName = c.String(),
                        CategoryId = c.Int(nullable: false),
                        CategoryName = c.String(),
                        DocumentTypeId = c.Int(nullable: false),
                        DocumentTypeName = c.String(),
                        RelatedProposal = c.String(),
                        Subject = c.String(),
                        ContentSum = c.String(),
                        CreateDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.Int(),
                        ProcessingBy = c.String(),
                        IsProcessing = c.Boolean(nullable: false),
                        IsSigningProcess = c.Boolean(nullable: false),
                        SharingToUsers = c.Int(),
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
                        Birthday = c.DateTime(precision: 7, storeType: "datetime2"),
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
            DropTable("dbo.DocumentApprovals");
            DropTable("dbo.DocumentApprovalFiles");
            DropTable("dbo.DocumentApprovalComments");
            DropTable("dbo.ApprovalPersons");
        }
    }
}
