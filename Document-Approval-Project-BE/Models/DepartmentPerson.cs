using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DepartmentPerson
    {
        [Key]
        public int Id { get; set; }
        public Guid PersonId { get; set; } = Guid.NewGuid();
        public string PersonName { get; set; }
        public string PersonMail { get; set; }
        public string PersonPosition { get; set; }
        public Guid DepartmentId { get; set; } = Guid.Empty;
        public int DepartmentPosition { get; set; }
    }
}