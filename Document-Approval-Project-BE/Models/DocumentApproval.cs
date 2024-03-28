using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DocumentApproval
    {
        [Key]
        public int Id { get; set; }
        public Guid DocumentApprovalId { get; set; } = Guid.NewGuid();
        public int ApplicantId { get; set; }
        public string ApplicantName {  get; set; }
        public int DepartmentId { get; set; }
        public int SectionId { get; set; }
        public int UnitId { get; set; }
        public int CategoryId { get; set; }
        public int DocumentTypeId {  get; set; }
        public string RelatedProposal { get; set; }
        public string Subject { get; set; }
        public string ContentSum { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate {  get; set; } = DateTime.Now;
        public int Status { get; set; }
        public int PresentApplicant { get; set; }

    }
}