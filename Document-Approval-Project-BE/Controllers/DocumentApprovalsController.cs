using Document_Approval_Project_BE.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static Document_Approval_Project_BE.Controllers.DocumentApprovalsController;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/documentapproval")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
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
                    ApplicantId = body.ApplicantId != null ? Guid.Parse(body.ApplicantId.ToString()) : Guid.Empty,
                    ApplicantName = body.ApplicantName,
                    DepartmentId = body.DepartmentId != null ? Guid.Parse(body.DepartmentId.ToString()) : Guid.Empty,
                    SectionId = body.SectionId != null ? Guid.Parse(body.SectionId.ToString()) : Guid.Empty,
                    UnitId = body.UnitId != null ? Guid.Parse(body.UnitId.ToString()) : Guid.Empty,
                    CategoryId = body.CategoryId != null ? Guid.Parse(body.CategoryId.ToString()) : Guid.Empty,
                    DocumentTypeId = body.DocumentTypeId != null ? Guid.Parse(body.DocumentTypeId.ToString()) : Guid.Empty,
                    RelatedProposal = body.RelatedProposal,
                    Subject = body.Subject,
                    ContentSum = body.ContentSum,
                    //CreateDate = body.CreateDate ?? DateTime.Now.ToString("dd/MM/yyyy"),
                };

                db.DocumentApprovals.Add(dcument);

                var approvalPerson = JObject.Parse(httpRequest.Form["ApprovalPerson"]);

                Dictionary<string, List<ApprovalPerson>> listPerson = new Dictionary<string, List<ApprovalPerson>>();

                string[] keysToCheck = { "approvers", "reference" };

                foreach (var key in keysToCheck)
                {
                    listPerson[key] = new List<ApprovalPerson>();
                    JArray items = (JArray)approvalPerson[key];

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            ApprovalPerson aP = new ApprovalPerson
                            {
                                ApprovalPersonName = item["ApprovalPersonName"].ToString(),
                                DocumentApprovalId = dcument.DocumentApprovalId,
                                PersonDuty = key == "approvers" ? 1 : 2
                            };
                            listPerson[key].Add(aP);
                            db.ApprovalPersons.Add(aP);
                        }
                    }
                }

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
                            string Filepath = GetFilePath(dcument.Subject + '-' + dcument.ApplicantId);

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
                                FileType = fileUpload.ContentType,
                                dcument.DocumentApprovalId,
                                DocumentType = files.GetKey(i).Equals("approvers") ? 1 : 2,
                            };

                            fileApprovals.Add(fileApproval);
                            db.DocumentApprovalFiles.Add(new DocumentApprovalFile
                            {
                                FileName = fileApproval.FileName,
                                FileSize =  fileApproval.FileSize,
                                FilePath = fileApproval.FilePath,
                                FileType = fileApproval.FileType,
                                DocumentApprovalId = fileApproval.DocumentApprovalId,
                                DocumentType = fileApproval.DocumentType
                            });
                        }

                    }

                    db.SaveChanges();

                    return Ok(new
                    {
                        state = "true",
                        msg = dcument,
                        files = fileApprovals,
                        ap = listPerson
                    });
                }

                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    dc = dcument,
                    ap = listPerson
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetAllDocument()
        {
            var listDocument = db.DocumentApprovals.ToList();
            return Ok(new
            {
                state = "true",
                listDocument
            });
        }

    }
}