namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv13 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApprovalPersons", "IsLast", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApprovalPersons", "IsLast");
        }
    }
}
