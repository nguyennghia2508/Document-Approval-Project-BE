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
using static System.Collections.Specialized.BitVector32;

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

                dynamic body = JsonConvert.DeserializeObject(httpRequest.Form["Data"]);

                var files = httpRequest.Files;

                DocumentApproval dcument = new DocumentApproval
                {
                    ApplicantId = body.ApplicantId != null ? body.ApplicantId : 0,
                    ApplicantName = body.ApplicantName,
                    DepartmentId = body.DepartmentId ?? body.DepartmentId,
                    SectionId = body.SectionId ?? body.SectionId,
                    UnitId = body.UnitId ?? body.UnitId,
                    CategoryId = body.CategoryId ?? body.CategoryId,
                    DocumentTypeId = body.DocumentTypeId ?? body.DocumentTypeId ,
                    RelatedProposal = body.RelatedProposal,
                    Subject = body.Subject,
                    ContentSum = body.ContentSum,
                    CreateDate = body.CreateDate ?? DateTime.Now.ToString("dd/MM/yyyy"),
                    Status = 1,
                    PresentApplicant = body.ApplicantId != null ? body.ApplicantId : 0
                };

                db.DocumentApprovals.Add(dcument);

                var approvalPerson = JObject.Parse(httpRequest.Form["ApprovalPerson"]);

                Dictionary<string, List<ApprovalPerson>> listPerson = new Dictionary<string, List<ApprovalPerson>>();

                string[] keysToCheck = { "approvers", "signers" };

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
                                ApprovalPersonId = ((int)item["ApprovalPersonId"]),
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

                            string Filepath = GetFilePath(dcument.Subject + '-' + dcument.ApplicantId);

                            if (!Directory.Exists(Filepath))
                            {
                                Directory.CreateDirectory(Filepath);
                            }

                            string fullPath = Path.Combine(Filepath, fileUpload.FileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await fileUpload.InputStream.CopyToAsync(stream);
                            }

                            var fileApproval = new
                            {
                                fileUpload.FileName,
                                FilePath = fullPath,
                                FileSize = fileUpload.ContentLength,
                                FileType = fileUpload.ContentType,
                                dcument.DocumentApprovalId,
                                DocumentType = files.GetKey(i).Equals("approve") ? 1 : 2,
                            };

                            fileApprovals.Add(fileApproval);
                            db.DocumentApprovalFiles.Add(new DocumentApprovalFile
                            {
                                FileName = fileApproval.FileName,
                                FileSize = fileApproval.FileSize,
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
                        ap = listPerson,
                    });
                }

                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    dc = dcument,
                    ap = listPerson,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("page/{page}")]
        public async Task<IHttpActionResult> GetAllDocument(int page)
        {
            int limit = 10;
            int skip = (page - 1) * limit;

            var dcapproval = db.DocumentApprovals
                                .OrderByDescending(d => d.CreateDate)
                                .Skip(skip)
                                .Take(limit)
                                .ToList();
            var listDcapproval = dcapproval
            .Select((d, index) => new
            {
                key = index+1,
                d.Id,
                d.DocumentApprovalId,
                d.ApplicantId,
                createBy = d.ApplicantName,
                categories = db.Categories.Single(item => item.Id == d.CategoryId).CategoryName,
                createDate = d.CreateDate.ToString("dd/MM/yyyy"),
                department = db.Departments.Single(item => item.Id == d.DepartmentId).DepartmentName,
                section = db.Departments.Single(item => item.Id == d.SectionId).DepartmentName,
                unit = db.Departments.Single(item => item.Id == d.UnitId).DepartmentName,
                documentType = db.DocumentTypes.Single(item => item.Id == d.DocumentTypeId).DocumentTypeName,
                Processing = db.Users.Single(item => item.Id == d.PresentApplicant).Username,
                d.RelatedProposal,
                d.Status,
                subject = d.Subject,
            })
            .ToList();


            return Ok(new
            {
                state = "true",
                listDcapproval,
                totalItems = db.DocumentApprovals.ToList().Count
            });
        }

    }
}