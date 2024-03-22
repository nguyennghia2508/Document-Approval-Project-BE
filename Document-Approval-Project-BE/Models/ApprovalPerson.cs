using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class ApprovalPerson
    {
        [Key]
        public int Id { get; set; }
        public Guid ApprovalPersonId { get; set; } = Guid.NewGuid();
        public string ApprovalPersonName { get; set; }
        public Guid DocumentApprovalId { get; set; } = Guid.Empty;
        public int PersonDuty {  get; set; }
    }
}