using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Document_Approval_Project_BE.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime Birtday { get; set; }
        public string Position { get; set; }
        public int Gender {  get; set; }
        public string JobTitle { get; set; }
        public string Company {  get; set; }

    }
}