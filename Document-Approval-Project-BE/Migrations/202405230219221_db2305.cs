using System.Data.Entity.Migrations;

public partial class db2305 : DbMigration
{
    public override void Up()
    {
        AddColumn("dbo.Notifications", "CreateBy", c => c.String());
        AddColumn("dbo.Notifications", "ItemId", c => c.Int(nullable: false));
    }

    public override void Down()
    {
        DropColumn("dbo.Notifications", "ItemId");
        DropColumn("dbo.Notifications", "CreateBy");
    }
}