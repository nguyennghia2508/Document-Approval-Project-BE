namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovals", "DocumentTypeId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovals", "DocumentTypeId");
        }
    }
}
