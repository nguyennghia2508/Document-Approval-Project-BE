using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public int DepartmentName {  get; set; }
        public string ParentNode { get; set; }
        public string ContactInfo { get; set; }
        public string DepartmentCode {  get; set; }
        public string DepartmentManager {  get; set; }
        public string Supervisor { get; set; }
    }
}