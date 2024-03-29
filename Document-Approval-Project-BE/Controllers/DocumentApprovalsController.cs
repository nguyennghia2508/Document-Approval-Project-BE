using Document_Approval_Project_BE.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Web.WebPages;
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

                var bodyJson = httpRequest.Form["Data"];
                var body = JsonConvert.DeserializeObject<DocumentApproval>(bodyJson);

                var files = httpRequest.Files;
                DocumentApproval dcument = new DocumentApproval();
                if (body != null)
                {
                    dcument = new DocumentApproval
                    {
                        ApplicantId = body.ApplicantId,
                        ApplicantName = body.ApplicantName,
                        DepartmentId = body.DepartmentId,
                        DepartmentName = db.Departments.SingleOrDefault(item => item.Id == body.DepartmentId)?.DepartmentName,
                        SectionId = body.SectionId,
                        SectionName = db.Departments.SingleOrDefault(item => item.Id == body.SectionId)?.DepartmentName,
                        UnitId = body.UnitId,
                        UnitName = db.Departments.SingleOrDefault(item => item.Id == body.UnitId)?.DepartmentName,
                        CategoryId = body.CategoryId,
                        CategoryName = db.Categories.SingleOrDefault(item => item.Id == body.CategoryId)?.CategoryName,
                        DocumentTypeId = body.DocumentTypeId,
                        DocumentTypeName = db.DocumentTypes.SingleOrDefault(item => item.Id == body.DocumentTypeId)?.DocumentTypeName,
                        RelatedProposal = body.RelatedProposal,
                        Subject = body.Subject,
                        ContentSum = body.ContentSum,
                        CreateDate = body.CreateDate,
                        Status = 1,
                        ProcessingBy = body.ApplicantName,
                        IsProcessing = true
                    };
                }

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

        [HttpPost]
        [Route("page/{page}")]
        public async Task<IHttpActionResult> GetAllDocument(int page)
        {
            int limit = 10;
            int skip = (page - 1) * limit;

            var rawMessage = await Request.Content.ReadAsStringAsync();
            var data = JObject.Parse(rawMessage);
            var tabName = data["tabName"].ToString();
            var UserId = (int?)data["userId"];
            IQueryable<DocumentApproval> query = db.DocumentApprovals.OrderByDescending(d => d.CreateDate);
            int totalItems = await db.DocumentApprovals.CountAsync();

            if (!tabName.IsEmpty() && !tabName.Equals("all"))
            {
                if(tabName.Equals("sendToMe"))
                {
                    var userDocumentApprovalIds = db.ApprovalPersons
                    .Where(ap => ap.ApprovalPersonId == UserId)
                    .Select(ap => ap.DocumentApprovalId)
                    .ToList();
                    query = query.Where(item => userDocumentApprovalIds.Contains(item.DocumentApprovalId));
                    totalItems = await query.CountAsync();
                }
                if (tabName.Equals("sendByMe"))
                {
                    query = query.Where(item => item.ApplicantId == UserId);
                    totalItems = await query.CountAsync();
                }
                if (tabName.Equals("shareWithMe"))
                {
                    query = query.Where(item => item.SharingToUsers == UserId);
                    totalItems = await query.CountAsync();
                }

                Regex regex = new Regex(@"status(\d+)");

                Match match = regex.Match(tabName);

                string numberStr = match.Groups[1].Value;
                if (tabName.Equals("status" + numberStr))
                {
                    query = query.Where(item => item.Status.ToString() == numberStr);
                    totalItems = await query.CountAsync();
                }
            }

            var dcapproval = query.Skip(skip).Take(limit).ToList();

            if (dcapproval.Count == 0)
            {
                return Ok(new
                {
                    state = "false",
                    listDcapproval = new List<object>(),
                });
            }

            var listDcapproval = dcapproval.Select((d, index) => new
            {
                key = index + 1,
                d.Id,
                d.DocumentApprovalId,
                d.ApplicantId,
                createBy = d.ApplicantName,
                categories = d.CategoryName,
                createDate = d.CreateDate.ToString("dd/MM/yyyy"),
                department = d.DepartmentName,
                section = d.DepartmentName,
                unit = d.DepartmentName,
                documentType = d.DocumentTypeName,
                Processing = d.ProcessingBy,
                d.IsProcessing,
                d.RelatedProposal,
                d.Status,
                subject = d.Subject,
            }).ToList();

            return Ok(new
            {
                state = "true",
                listDcapproval,
                totalItems,
                tabName,
                UserId
            });
        }
    }
}