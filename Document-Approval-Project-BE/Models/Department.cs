using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public Guid DepartmentId { get; set; } = Guid.NewGuid();
        public string DepartmentName {  get; set; }
        public Guid ParentNode { get; set; } = Guid.Empty;
        public int DepartmentLevel { get; set; }
        public Guid ChildrenNode { get; set; } = Guid.Empty;
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string ContactInfo { get; set; }
        public string DepartmentCode {  get; set; }
        public Guid DepartmentManager { get; set; } = Guid.Empty;
        public Guid Supervisor { get; set; } = Guid.Empty;
    }
}