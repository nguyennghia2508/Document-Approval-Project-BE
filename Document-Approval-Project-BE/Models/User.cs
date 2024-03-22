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
        public int Id { get; set; }
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birtday { get; set; }
        public string Position { get; set; }
        public int Gender {  get; set; }
        public string JobTitle { get; set; }
        public string Company {  get; set; }
        public Guid DepartmentId { get; set; } = Guid.Empty;

    }
}