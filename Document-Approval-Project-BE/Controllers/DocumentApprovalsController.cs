using Document_Approval_Project_BE.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.UI.WebControls;
using static Document_Approval_Project_BE.Controllers.DocumentApprovalsController;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/documentapproval")]
    public class DocumentApprovalsController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;
        [NonAction]
        public string GetFilePath(string path)
        {
            string rootPath = HostingEnvironment.MapPath("~/");

            if (!path.Equals(""))
                return Path.Combine(rootPath, "Upload\\Files", path);
            return Path.Combine(rootPath, "Upload\\Files");
        }

        [HttpPost]
        [Route("add")]
        public async Task<IHttpActionResult> AddDocument()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            try
            {
                var httpRequest = currentContext.Request;
                //var provider = new MultipartMemoryStreamProvider();
                //await Request.Content.ReadAsMultipartAsync(provider);

                dynamic body = JsonConvert.DeserializeObject(httpRequest.Form["Data"]);
                var files = httpRequest.Files;
                DocumentApproval dcument = new DocumentApproval
                {
                    Applicant = body.Applicant,
                    CategoryName = body.CategoryName
                };
                if (files.Count > 0)
                {
                    var fileApprovals = new List<object>();

                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFile fileUpload = files[i];
                            FileInfo fileInfo = new FileInfo(fileUpload.FileName);

                            // Lấy đường dẫn cho thư mục lưu trữ
                            string Filepath = GetFilePath(dcument.CategoryName + '-' + dcument.Applicant);

                            // Tạo thư mục nếu chưa tồn tại
                            if (!Directory.Exists(Filepath))
                            {
                                Directory.CreateDirectory(Filepath);
                            }

                            // Tạo đường dẫn đầy đủ cho tệp tin
                            string fullPath = Path.Combine(Filepath, fileUpload.FileName);

                            // Lưu tệp tin vào thư mục
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await fileUpload.InputStream.CopyToAsync(stream);
                            }

                            // Tạo object fileApproval
                            var fileApproval = new
                            {
                                fileUpload.FileName,
                                FilePath = fullPath, // Sử dụng đường dẫn đầy đủ
                                FileSize = fileUpload.ContentLength,
                                FileType = fileUpload.ContentType
                            };

                            fileApprovals.Add(fileApproval);
                        }

                    }


                    return Ok(new
                    {
                        state = "true",
                        msg = dcument,
                        files = fileApprovals
                    });
                }

                return Ok(new
                {
                    state = "true",
                    msg = dcument
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


    }
}