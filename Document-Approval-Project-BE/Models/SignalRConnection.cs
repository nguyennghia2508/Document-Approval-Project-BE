using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class SignalRConnection
    {
        public int Id {  get; set; }
        public Guid Guid { get; set; } = Guid.Empty;
        public string Name { get; set; }
        public string Message {  get; set; }
    }
}