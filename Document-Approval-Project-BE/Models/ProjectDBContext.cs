﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Document_Approval_Project_BE.Models
{
    public class ProjectDBContext : DbContext
    {
        public ProjectDBContext() : base("MyConnectionString") { }
        public DbSet<User> Users { get; set; }
        public DbSet<DocumentApproval> DocumentApprovals { get; set; }
        public DbSet<DocumentApprovalFile> DocumentApprovalFiles { get; set; }
        public DbSet<DocumentApprovalComment> DocumentApprovalComments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentPerson> DepartmentPersons { get; set; }
        public DbSet<ApprovalPerson> ApprovalPersons { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Module> Modules { get; set; }


    }
}