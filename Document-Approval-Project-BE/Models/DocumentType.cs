using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }
        public Guid DocumentTypeId { get; set; } = Guid.NewGuid();
        public string DocumentTypeName { get; set; }
        public Guid CategoryId { get; set; } = Guid.Empty;
    }
}