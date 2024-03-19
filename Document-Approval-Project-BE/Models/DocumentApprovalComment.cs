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
        public int CommentId { get; set; }
        public int ApprovalPersonId {  get; set; }
        public string CommentContent {  get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CommentTime { get; set; }
        public Boolean IsSubComment { get; set; }

    }
}