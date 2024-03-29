namespace Document_Approval_Project_BE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedbv1 : DbMigration
    {
        public override void Up()
        {
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
                        Birtday = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
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
        }
    }
}
