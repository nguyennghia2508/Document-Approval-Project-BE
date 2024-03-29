namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocumentApprovals", "ApplicantId", c => c.Int());
            AlterColumn("dbo.DocumentApprovals", "UnitId", c => c.Int());
            AlterColumn("dbo.DocumentApprovals", "Status", c => c.Int());
            AlterColumn("dbo.DocumentApprovals", "SharingToUsers", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentApprovals", "SharingToUsers", c => c.Int(nullable: false));
            AlterColumn("dbo.DocumentApprovals", "Status", c => c.Int(nullable: false));
            AlterColumn("dbo.DocumentApprovals", "UnitId", c => c.Int(nullable: false));
            AlterColumn("dbo.DocumentApprovals", "ApplicantId", c => c.Int(nullable: false));
        }
    }
}
