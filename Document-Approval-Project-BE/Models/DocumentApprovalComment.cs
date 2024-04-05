using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DocumentApprovalComment
    {
        [Key]
        public int Id { get; set; }
        public Guid DocumentApprovalId { get; set; } = Guid.Empty;
        public Guid CommentId { get; set; } = Guid.NewGuid();
        public Boolean IsFirst { get; set; }
        public int? ApprovalPersonId { get; set; } = null;
        public string ApprovalPersonName { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string CommentContent {  get; set; }
        public int? ParentNode { get; set; } = null;
        public Boolean IsSubComment { get; set; }
        public int? CommentStatus { get; set; }

    }
}