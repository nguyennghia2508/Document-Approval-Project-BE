using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DocumentApprovalFile
    {
        [Key]
        public int Id { get; set; }
        public Guid DocumentFileId { get; set; } = Guid.NewGuid();
        public Guid DocumentApprovalId { get; set; } = Guid.Empty;
        public Guid CommentId { get; set; } = Guid.Empty;
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public int DocumentType {  get; set; }

    }
}