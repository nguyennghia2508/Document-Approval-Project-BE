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
using System.Web.Hosting;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private readonly Authentication auth = new Authentication();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

        [NonAction]
        public string GetFilePath(string path)
        {
            string rootPath = HostingEnvironment.MapPath("~/");

            if (!path.Equals(""))
                return Path.Combine(rootPath, "Upload\\Users", path);
            return Path.Combine(rootPath, "Upload\\Users");
        }

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

            var userInfor = db.Users.SingleOrDefault(p => p.Email == user.Email);
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
                    userInfor.DepartmentId,
                    userInfor.SignatureFileName,
                    userInfor.SignatureFilePath,
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
                    verifyUser.DepartmentId,
                    verifyUser.SignatureFileName,
                    verifyUser.SignatureFilePath,
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


        [HttpPost]
        [Route("add-signature/{id}")]
        public async Task<IHttpActionResult> AddSignatureAsync(int id)
        {
            var httpRequest = currentContext.Request;
            var files = httpRequest.Files;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if(user != null)
            {
                if (files.Count > 0)
                {
                    HttpPostedFile fileUpload = files[0];
                    FileInfo fileInfo = new FileInfo(fileUpload.FileName);

                    string filePath = GetFilePath(Path.Combine(user.Id.ToString(), "signature"));

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    string fileName = "signature_" + DateTime.Now.Ticks.ToString() + ".png";
                    string fullPath = Path.Combine(filePath,fileName);
                    string alterPath = "Upload/Users/" +
                        user.Id.ToString() + "/signature/" + fileName;

                    if(user.SignatureFilePath != null)
                    {
                        string previousFile = Path.Combine(HostingEnvironment.MapPath("~/"), user.SignatureFilePath);

                        if (File.Exists(previousFile))
                        {
                            File.Delete(previousFile);
                        }
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await fileUpload.InputStream.CopyToAsync(stream);
                    }

                    var fileSignature = new
                    {
                        SignatureFileName = fileName,
                        SignatureFilePath = alterPath,
                    };

                    user.SignatureFileName = fileSignature.SignatureFileName;
                    user.SignatureFilePath = fileSignature.SignatureFilePath;

                    db.SaveChanges();

                    var getUserInfor = new
                    {
                        user.Id,
                        user.UserId,
                        user.Username,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Birthday,
                        user.Position,
                        user.Gender,
                        user.JobTitle,
                        user.Company,
                        user.DepartmentId,
                        user.SignatureFileName,
                        user.SignatureFilePath,
                    };

                    return Ok(new
                    {
                        state = "true",
                        msg = "Upload your signature success",
                        user = getUserInfor
                    });
                }
            }

            return Ok(new
            {
                state = "false",
                msg = "Invalid user",
            });

        }
    }
}