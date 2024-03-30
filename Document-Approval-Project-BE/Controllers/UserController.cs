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
        private readonly Authentication auth = new Authentication();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

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

            var userInfor = db.Users.Single(p => p.Email == user.Email);
            if (userInfor == null)
            {
                return Ok(new
                {
                    state = "false",
                    msg = "Invalid email or password"
                });
            }

            // Kiểm tra mật khẩu
            if (BCryptHelper.CheckPassword(user.Password, userInfor.Password))
            {
                // Tạo token và trả về thông tin người dùng và token
                var token = auth.GenerateToken(userInfor.UserId);
                var getUserInfor = new
                {
                    userInfor.Id,
                    userInfor.UserId,
                    userInfor.Username,
                    userInfor.Email,
                    userInfor.FirstName,
                    userInfor.LastName,
                    userInfor.Birthday,
                    userInfor.Position,
                    userInfor.Gender,
                    userInfor.JobTitle,
                    userInfor.Company,
                    userInfor.DepartmentId
                };
                return Ok(new
                {
                    state = "true",
                    user = getUserInfor,
                    token
                });
            }
            else
            {
                return Ok(new
                {
                    state = "false",
                    msg = "Invalid email or password",
                    user
                });
            }
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

        [HttpPost]
        [Route("verify-token")]
        public IHttpActionResult VerifyToken()
        {
            var verifyUser = auth.VerifyToken(currentContext.Request);

            if (verifyUser != null)
            {
                var getUserInfor = new
                {
                    verifyUser.Id,
                    verifyUser.UserId,
                    verifyUser.Username,
                    verifyUser.Email,
                    verifyUser.FirstName,
                    verifyUser.LastName,
                    verifyUser.Birthday,
                    verifyUser.Position,
                    verifyUser.Gender,
                    verifyUser.JobTitle,
                    verifyUser.Company,
                    verifyUser.DepartmentId
                };

                return Ok(new
                {
                    verify = true,
                    user = getUserInfor
                });
            }
            else
            {
                var response = new
                {
                    verify = false,
                    message = "Unauthorized user"
                };
                return Content(System.Net.HttpStatusCode.Unauthorized, response);
            }
        }

    }
}