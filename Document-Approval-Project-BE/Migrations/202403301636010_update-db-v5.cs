namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Birthday", c => c.DateTime(precision: 7, storeType: "datetime2"));
            DropColumn("dbo.Users", "Birtday");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Birtday", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            DropColumn("dbo.Users", "Birthday");
        }
    }
}
