namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "ExecutionDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "ExecutionDate");
        }
    }
}
