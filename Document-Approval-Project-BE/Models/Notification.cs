using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public Guid NotificationId { get; set; } = Guid.NewGuid();
        public int ModuleId { get; set; }
        public string Avatar {  get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public int CreateBy { get; set; }
        public Guid ItemId { get; set; }
        public string Type {  get; set; }
        public string Parameters { get; set; }
        public string Url {  get; set; }
        public Boolean IsHidden { get; set; } = false;

    }
}