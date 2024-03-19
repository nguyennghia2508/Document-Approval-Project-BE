using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class ApprovalPerson
    {
        [Key]
        public int ApprovalPersonId { get; set; }
        public string ApprovalPersonName { get; set; }
        public int DocumentApprovalId { get; set; }
    }
}