using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }
        public Guid ModuleId { get; set; } = Guid.NewGuid();
        public string ModuleName { get; set; }
    }
}