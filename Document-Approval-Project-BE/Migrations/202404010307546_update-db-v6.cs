namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentApprovals", "RequestCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentApprovals", "RequestCode");
        }
    }
}
