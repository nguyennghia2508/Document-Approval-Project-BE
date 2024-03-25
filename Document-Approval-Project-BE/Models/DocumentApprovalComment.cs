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
        public Guid CommentId { get; set; } = Guid.NewGuid();
        public Guid ApprovalPersonId { get; set; } = Guid.Empty;
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string CommentContent {  get; set; }
        public string CommentTime { get; set; }
        public Boolean IsSubComment { get; set; }

    }
}