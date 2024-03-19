namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class testdata : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocumentApprovals", "CreateDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.Users", "Birtday", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Birtday", c => c.DateTime(nullable: false));
            AlterColumn("dbo.DocumentApprovals", "CreateDate", c => c.DateTime(nullable: false));
        }
    }
}
