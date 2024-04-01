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
        public string RequestCode {  get; set; }
        public Guid DocumentApprovalId { get; set; } = Guid.NewGuid();
        public int? ApplicantId { get; set; }
        public string ApplicantName {  get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DocumentTypeId {  get; set; }
        public string DocumentTypeName { get; set; }
        public string RelatedProposal { get; set; }
        public string Subject { get; set; }
        public string ContentSum { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate {  get; set; } = DateTime.Now;
        public int? Status { get; set; } = null;
        public string ProcessingBy { get; set; }
        public Boolean IsProcessing { get; set; } = false;
        public Boolean IsSigningProcess { get; set; } = false;
        public int? SharingToUsers { get; set; } = null;
    }
}