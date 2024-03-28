using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using DevOne.Security.Cryptography.BCrypt;
using System.Web.Http.Cors;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/user")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        // GET: api/user/{email}
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] User user)
        {
            if (user == null)
            {
                return Ok(new
                {
                    state = "false",
                    message = "Empty"
                });
            }
            var userExist = db.Users.SingleOrDefault(p => p.Email == user.Email);
            if(userExist == null)
            {
                var hashPassword = BCryptHelper.HashPassword(user.Password, BCryptHelper.GenerateSalt(10));
                db.Users.Add(new User()
                {
                    Username = user.Username,
                    Password = hashPassword,
                    Email = user.Email
                });
                db.SaveChanges();
                return Ok(new
                {
                    state = "true",
                    message = "Register success"
                });
            }
            return Ok(new
            {
                state = "false",
                message = "Register false"
            });
        }

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
            var userInfo = db.Users.SingleOrDefault(p => p.Email == user.Email);
            if (userInfo == null)
            {
                return Ok(new
                {
                    state = "false",
                    msg = "Invalid email or password"

                });
            }
            if(BCryptHelper.CheckPassword(user.Password,userInfo.Password))
            {
                return Ok(new
                {
                    state = "true",
                    userInfor = userInfo
                });
            }
            return Ok(new
            {
                state = "false",
                msg = "Invalid email or password"

            });
        }

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetALlUser()
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var listUser = db.Users.ToList();
            return Ok(new
            {
                state = "true",
                listUser
            });
        }
    }
}