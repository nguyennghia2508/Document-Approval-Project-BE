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
        public Guid ApplicantId { get; set; } = Guid.Empty;
        public string ApplicantName {  get; set; }
        public Guid DepartmentId { get; set; } = Guid.Empty;
        public Guid SectionId { get; set; } = Guid.Empty;
        public Guid UnitId { get; set; } = Guid.Empty;
        public Guid CategoryId { get; set; } = Guid.Empty;
        public Guid DocumentTypeId {  get; set; } = Guid.Empty;
        public string RelatedProposal { get; set; }
        public string Subject { get; set; }
        public string ContentSum { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate {  get; set; } = DateTime.Now;
        public int Status { get; set; }
        public Guid PresentApplicant { get; set; } = Guid.Empty;

    }
}