namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentApprovalComments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        ApprovalPersonId = c.Int(nullable: false),
                        CommentContent = c.String(),
                        CommentTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        IsSubComment = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DocumentApprovalComments");
        }
    }
}
