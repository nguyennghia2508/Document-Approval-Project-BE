using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DocumentApproval
    {
        public int DocumentApprovalId { get; set; }
        public string Applicant {  get; set; }
        public int DepartmentId { get; set; }
        public string CategoryName { get; set; }
        public string RelatedProposal { get; set; }
        public string Subject { get; set; }
        public string ContentSum { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate {  get; set; }
        public int Status { get; set; }
        public string PresentApplicant { get; set; }

    }
}