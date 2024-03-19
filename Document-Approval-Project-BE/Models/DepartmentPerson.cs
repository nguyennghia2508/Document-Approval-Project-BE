using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DepartmentPerson
    {
        [Key]
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonMail { get; set; }
        public string PersonPosition { get; set; }
        public int DepartmentId { get; set; }
        public int DepartmentPosition { get; set; }
    }
}