using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class ApprovalPerson
    {
        [Key]
        public int Id { get; set; }
        public int Index { get; set; }
        public int ApprovalPersonId { get; set; }
        public string ApprovalPersonName { get; set; }
        public Guid DocumentApprovalId { get; set; } = Guid.Empty;
        public int PersonDuty {  get; set; }
        public Boolean IsApprove { get; set; } = false;
        public Boolean IsProcessing { get; set; } = false;
        public Boolean IsSign {  get; set; } = false;
        public Boolean IsLast { get; set; } = false;
        public Boolean IsReject {  get; set; } = false;
        [Column(TypeName = "DateTime2")]
        public DateTime? ExecutionDate { get; set; }
        public string Comment {  get; set; }
    }
}