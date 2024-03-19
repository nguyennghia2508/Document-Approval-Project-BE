namespace Document_Approval_Project_BE.Migrations
{
    using Document_Approval_Project_BE.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Document_Approval_Project_BE.Models.ProjectDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Document_Approval_Project_BE.Models.ProjectDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            if (!context.Users.Any())
            {
                context.Users.AddRange(new User[]
                {
                    new User()
                    {
                        //UserId = 1,
                        Username = "Admin",
                        Password = "123456",
                        Email = "admin@gmail.com"
                    },
                    new User()
                    {
                        //UserId = 2,
                        Username = "Tester01",
                        Email = "tester01@gmail.com",
                        Password = "123456"
                    }
                });
                context.SaveChanges();
            }
        }
    }
}
