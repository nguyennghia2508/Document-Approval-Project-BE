namespace Document_Approval_Project_BE.Migrations
{
    using DevOne.Security.Cryptography.BCrypt;
    using Document_Approval_Project_BE.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.SqlServer;
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
                context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Users', RESEED, 0)");
                // Thực thi truy vấn SQL
                var hashPassword = BCryptHelper.HashPassword("123456", BCryptHelper.GenerateSalt(10));

                context.Users.AddRange(new User[]
                {
                    new User()
                    {
                        //UserId = 1,
                        Username = "Admin",
                        Password = hashPassword,
                        Email = "admin@gmail.com"
                    },
                    new User()
                    {
                        //UserId = 2,
                        Username = "Tester01",
                        Email = "tester01@gmail.com",
                        Password = hashPassword,
                    },
                    new User()
                    {
                        //UserId = 3,
                        Username = "Tester02",
                        Email = "tester02@gmail.com",
                        Password = hashPassword,
                    }
                });
                context.SaveChanges();
            }
        }
    }
}
