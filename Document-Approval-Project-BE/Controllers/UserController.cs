using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        // GET: api/user/{email}
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Request body is empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userInfo = db.Users.SingleOrDefault(p => p.Email == user.Email && p.Password == user.Password);
            if (userInfo == null)
            {
                return Ok(new
                {
                    state = "false",
                    msg = "Invalid email or password"

                });
            }
            return Ok(new
            {
                state = "true",
                userInfor = userInfo
            });
        }
    }
}