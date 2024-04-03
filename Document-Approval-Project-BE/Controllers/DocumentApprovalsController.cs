using DevOne.Security.Cryptography.BCrypt;
using Document_Approval_Project_BE.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
                    };
                }

                db.DocumentApprovals.Add(dcument);
                db.SaveChanges();

                dcument.RequestCode = $"{dcument.Id}-IDOC-AVN-{DateTime.Now.Year}";
                db.SaveChanges();

                var approvalPerson = JObject.Parse(httpRequest.Form["ApprovalPerson"]);

                Dictionary<string, List<ApprovalPerson>> listPerson = new Dictionary<string, List<ApprovalPerson>>();

                string[] keysToCheck = { "approvers", "signers" };

                // Dictionary để lưu trữ số thứ tự của mỗi loại người
                Dictionary<string, int> indexMap = new Dictionary<string, int>();

                foreach (var key in keysToCheck)
                {
                    listPerson[key] = new List<ApprovalPerson>();
                    JArray items = (JArray)approvalPerson[key];

                    if (items != null)
                    {
                        // Khởi tạo index cho loại người hiện tại
                        indexMap[key] = 1;

                        foreach (var item in items)
                        {
                            ApprovalPerson aP = new ApprovalPerson
                            {
                                Index = indexMap[key],
                                ApprovalPersonId = ((int)item["ApprovalPersonId"]),
                                ApprovalPersonName = item["ApprovalPersonName"].ToString(),
                                DocumentApprovalId = dcument.DocumentApprovalId,
                                PersonDuty = key == "approvers" ? 1 : 2,
                                IsProcessing = key == "approvers" && indexMap[key] == 1,
                            };
                            listPerson[key].Add(aP);
                            db.ApprovalPersons.Add(aP);

                            // Tăng index sau mỗi lần lặp
                            indexMap[key]++;
                        }
                    }
                }

                if (listPerson.ContainsKey("approvers") && listPerson["approvers"].Count > 0)
                {
                    dcument.ProcessingBy = listPerson["approvers"][0].ApprovalPersonName;
                    db.SaveChanges();
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

                            string Filepath = GetFilePath(dcument.DocumentApprovalId.ToString());

                            if (!Directory.Exists(Filepath))
                            {
                                Directory.CreateDirectory(Filepath);
                            }

                            string fullPath = Path.Combine(Filepath, fileUpload.FileName);
                            string alterPath = "Upload/Files/" + 
                                dcument.DocumentApprovalId.ToString() + "/" + fileUpload.FileName;

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await fileUpload.InputStream.CopyToAsync(stream);
                            }

                            var fileApproval = new
                            {
                                fileUpload.FileName,
                                FilePath = alterPath,
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
                        dc = dcument,
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

            var listFilter = data["dataFilter"];

            IQueryable<DocumentApproval> query = db.DocumentApprovals.OrderByDescending(d => d.CreateDate)
          .Where(u => db.ApprovalPersons.Any(p => p.DocumentApprovalId == u.DocumentApprovalId && p.ApprovalPersonId == UserId));

            int totalItems = await db.DocumentApprovals.CountAsync();

            if (listFilter != null && listFilter.HasValues)
            {
                if (listFilter["requestcode"] != null && !string.IsNullOrEmpty(listFilter["requestcode"].ToString()))
                {
                    string requestcode = listFilter["requestcode"].ToString().Trim();
                    query = query.Where(item => item.RequestCode.Trim().Contains(requestcode));
                }
                if (listFilter.SelectToken("documentType") != null && !listFilter["documentType"].ToString().Equals("all"))
                {
                    string documentType = listFilter["documentType"].ToString();
                    if (int.TryParse(documentType, out int documentTypeId))
                    {
                        query = query.Where(item => item.DocumentTypeId == documentTypeId);
                    }
                }
                if (listFilter.SelectToken("attorney") != null && !listFilter["attorney"].ToString().Equals("all"))
                {
                    string attorney = listFilter["attorney"].ToString();
                    if (int.TryParse(attorney, out int attorneyId))
                    {
                        query = query.Where(item => item.ApplicantId == attorneyId);
                    }
                }
                if (listFilter.SelectToken("authorizer") != null && !listFilter["authorizer"].ToString().Equals("all"))
                {
                    string authorizer = listFilter["authorizer"].ToString();
                    if (int.TryParse(authorizer, out int attorneyId))
                    {
                        query = query.Where(item => item.ApplicantId == attorneyId);
                    }
                }
                if (listFilter["subject"] != null && !string.IsNullOrEmpty(listFilter["subject"].ToString()))
                {
                    string subject = listFilter["subject"].ToString();
                    query = query.Where(item => item.Subject.Contains(subject));
                }
                if (listFilter["createStart"] != null && listFilter["createEnd"] != null)
                {
                    if (DateTime.TryParse(listFilter["createStart"].ToString(), out DateTime createStart)
                        && DateTime.TryParse(listFilter["createEnd"].ToString(), out DateTime createEnd))
                    {
                        createEnd = createEnd.Date.AddDays(1).AddSeconds(-1);
                        query = query.Where(item => item.CreateDate >= createStart && item.CreateDate <= createEnd);
                    }
                }
                if (!listFilter["department"].Equals("all"))
                {
                    string department = listFilter["department"].ToString();
                    if (int.TryParse(department, out int departmentId))
                    {
                        query = query.Where(item => item.DepartmentId == departmentId);
                        var departmentParenNode = db.Departments.FirstOrDefault(d => d.Id == departmentId);
                        // Nếu section được chọn
                        if (listFilter.SelectToken("section") != null && !listFilter["section"].ToString().Equals("all"))
                        {
                            string section = listFilter["section"].ToString();
                            if (int.TryParse(section, out int sectionId))
                            {
                                var sectionInDepartment = db.Departments.FirstOrDefault(s => s.ParentNode == departmentParenNode.DepartmentId && s.Id == sectionId);
                                if (sectionInDepartment != null)
                                {
                                    query = query.Where(item => item.SectionId == sectionInDepartment.Id);
                                    var sectionParenNode = db.Departments.FirstOrDefault(s => s.Id == sectionId);

                                    if (listFilter.SelectToken("unit") != null && !listFilter["unit"].ToString().Equals("all"))
                                    {
                                        string unit = listFilter["unit"].ToString();

                                        if (int.TryParse(unit, out int unitId))
                                        {
                                            var unitInSection = db.Departments.FirstOrDefault(u => u.ParentNode == sectionParenNode.DepartmentId && u.Id == unitId);
                                            if (unitInSection != null)
                                            {
                                                query = query.Where(item => item.UnitId == unitId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!listFilter["status"].ToString().Equals("all") && listFilter["status"] != null)
                {
                    var status = listFilter["status"].ToString(); 
                    query = query.Where(item => item.Status.ToString().Equals(status));
                }
            }
            else
            {
                Regex regex = new Regex(@"status(\d+)");

                Match match = regex.Match(tabName);

                string numberStr = match.Groups[1].Value;
                if (tabName.Equals("status" + numberStr))
                {
                    query = query.Where(item => item.Status.ToString().Equals(numberStr));
                    totalItems = await query.CountAsync();
                }
            }

            if (!tabName.IsEmpty() && !tabName.Equals("all"))
            {
                if (tabName.Equals("sendToMe"))
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

                //Regex regex = new Regex(@"status(\d+)");

                //Match match = regex.Match(tabName);

                //string numberStr = match.Groups[1].Value;
                //if (tabName.Equals("status" + numberStr) && listFilter.SelectToken("status") != null)
                //{
                //    query = query.Where(item => item.Status.ToString().Equals(numberStr));
                //    totalItems = await query.CountAsync();
                //}
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
                d.RequestCode,
                d.DocumentApprovalId,
                d.ApplicantId,
                createBy = d.ApplicantName,
                categories = d.CategoryName,
                createDate = d.CreateDate.ToString("dd/MM/yyyy"),
                department = d.DepartmentName,
                section = d.SectionName,
                unit = d.UnitName,
                documentType = d.DocumentTypeName,
                Processing = d.ProcessingBy,
                isProcessing = CheckIsProcessing(UserId, d.DocumentApprovalId),
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
                UserId,
                data
            });
        }

        bool CheckIsProcessing(int? userId, Guid documentApprovalId)
        {
            var documents = db.ApprovalPersons
            .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalPersonId == userId);

            if (documents != null)
            {
                var isProcessing = documents.Any(p => (p.PersonDuty == 1 && p.IsProcessing == true && p.IsApprove == false) || (p.PersonDuty == 2 && p.IsSign == false && p.IsProcessing == true));
                return isProcessing;
            }
            else
            {
                // Nếu không tìm thấy userApprovalPerson, không có gì để xử lý
                return false;
            }
        }

        [HttpGet]
        [Route("view/{id}")]
        public IHttpActionResult GetDocumentById(int id)
        {
            var document = db.DocumentApprovals.FirstOrDefault(p => p.Id == id);
            if (document != null)
            {
                var documentInfo = new
                {
                    document,
                    files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == document.DocumentApprovalId).ToList(),
                    approvers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == document.DocumentApprovalId && p.PersonDuty == 1).ToList(),
                    signers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == document.DocumentApprovalId && p.PersonDuty == 2).ToList()
                };
                return Ok(documentInfo);
            }
            var content = new
            {
                state = "false",
                message = "Document not exist",
                document = new Object()
            };
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, content));
        }
    }
}