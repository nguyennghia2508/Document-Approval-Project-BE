﻿using DevOne.Security.Cryptography.BCrypt;
using Document_Approval_Project_BE.Hubs;
using Document_Approval_Project_BE.Models;
using Document_Approval_Project_BE.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
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
    public class DocumentApprovalsController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;
        private readonly NotificationService _notificationService;
        private readonly IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<SignalRHub>();

        public DocumentApprovalsController()
        {
            _notificationService = new NotificationService();
        }

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
                DocumentApprovalComment comment = new DocumentApprovalComment();

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
                        IsDraft = body.IsDraft,
                        Status = body.IsDraft ? 0 : 1,
                    };
                }

                db.DocumentApprovals.Add(dcument);
                db.SaveChanges();

                var departmentCode = db.Departments.FirstOrDefault(d => d.Id == dcument.DepartmentId).DepartmentCode;
                dcument.RequestCode = $"{dcument.Id:D5}-IDOC-{departmentCode}-{DateTime.Now.Year}";
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
                                ApprovalPersonEmail = item["ApprovalPersonEmail"].ToString(),
                                PersonDuty = (int)item["PersonDuty"],
                                IsProcessing = key == "approvers" && !dcument.IsDraft && indexMap[key] == 1,
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
                    if (!dcument.IsDraft)
                    {
                        dcument.ProcessingBy = listPerson["approvers"][0].ApprovalPersonName;
                        db.SaveChanges();
                    }
                }


                if (files.Count > 0)
                {
                    var fileApprovals = new List<DocumentApprovalFile>();

                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFile fileUpload = files[i];

                        string Filepath = GetFilePath(dcument.DocumentApprovalId.ToString());

                        if (!Directory.Exists(Filepath))
                        {
                            Directory.CreateDirectory(Filepath);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);

                        string fileExtension = Path.GetExtension(fileUpload.FileName);

                        string alterPath = "Upload/Files/" +
                            dcument.DocumentApprovalId.ToString() + "/" + fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                        string fullName = dcument.DocumentApprovalId.ToString() + "/" +  fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                        string fullPath = GetFilePath(fullName);

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

                    if (!dcument.IsDraft)
                    {
                        comment = new DocumentApprovalComment
                        {
                            ApprovalPersonId = dcument.ApplicantId,
                            ApprovalPersonName = dcument.ApplicantName,
                            DocumentApprovalId = dcument.DocumentApprovalId,
                            CommentContent = "Submit the request",
                            IsFirst = true,
                        };

                        db.DocumentApprovalComments.Add(comment);

                        var parameter = new
                        {
                            code = dcument.RequestCode,
                            userDisplayName = dcument.ApplicantName,
                        };
                        var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                        var approval = db.ApprovalPersons.FirstOrDefault(p => p.DocumentApprovalId == dcument.DocumentApprovalId 
                        && p.PersonDuty == 1 && p.Index == 1 && p.IsProcessing == true).ApprovalPersonId;

                        await _notificationService.SendNotification("WAITING_FOR_APPROVAL", parameter, module, dcument, approval,null);

                    }

                    db.SaveChanges();

                    return Ok(new
                    {
                        state = "true",
                        dc = dcument,
                        message = dcument.IsDraft ? "The request successfully saved" : "The request successfully submited",
                    });
                }

                if (!dcument.IsDraft)
                {
                    comment = new DocumentApprovalComment
                    {
                        ApprovalPersonId = dcument.ApplicantId,
                        ApprovalPersonName = dcument.ApplicantName,
                        DocumentApprovalId = dcument.DocumentApprovalId,
                        CommentContent = "Submit the request",
                        IsFirst = true,
                    };

                    db.DocumentApprovalComments.Add(comment);
                }

                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    message = dcument.IsDraft ? "The request successfully saved" : "The request successfully submited",
                    dc = dcument,
                    //ap = listPerson,
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
                    totalItems = await query.CountAsync();

                }
                if (listFilter.SelectToken("documentType") != null && !listFilter["documentType"].ToString().Equals("all"))
                {
                    string documentType = listFilter["documentType"].ToString();
                    if (int.TryParse(documentType, out int documentTypeId))
                    {
                        query = query.Where(item => item.DocumentTypeId == documentTypeId);
                        totalItems = await query.CountAsync();
                    }
                }
                if (listFilter.SelectToken("attorney") != null && !listFilter["attorney"].ToString().Equals("all"))
                {
                    string attorney = listFilter["attorney"].ToString();
                    if (int.TryParse(attorney, out int attorneyId))
                    {
                        query = query.Where(item => item.ApplicantId == attorneyId);
                        totalItems = await query.CountAsync();
                    }
                }
                if (listFilter.SelectToken("authorizer") != null && !listFilter["authorizer"].ToString().Equals("all"))
                {
                    string authorizer = listFilter["authorizer"].ToString();
                    if (int.TryParse(authorizer, out int attorneyId))
                    {
                        query = query.Where(item => item.ApplicantId == attorneyId);
                        totalItems = await query.CountAsync();
                    }
                }
                if (listFilter["subject"] != null && !string.IsNullOrEmpty(listFilter["subject"].ToString()))
                {
                    string subject = listFilter["subject"].ToString();
                    query = query.Where(item => item.Subject.Contains(subject));
                    totalItems = await query.CountAsync();
                }
                if (listFilter["createStart"] != null && listFilter["createEnd"] != null)
                {
                    if (DateTime.TryParse(listFilter["createStart"].ToString(), out DateTime createStart)
                        && DateTime.TryParse(listFilter["createEnd"].ToString(), out DateTime createEnd))
                    {
                        createEnd = createEnd.Date.AddDays(1).AddSeconds(-1);
                        query = query.Where(item => item.CreateDate >= createStart && item.CreateDate <= createEnd);
                        totalItems = await query.CountAsync();
                    }
                }
                if (!listFilter["department"].Equals("all"))
                {
                    string department = listFilter["department"].ToString();
                    if (int.TryParse(department, out int departmentId))
                    {
                        query = query.Where(item => item.DepartmentId == departmentId);
                        var departmentParenNode = db.Departments.FirstOrDefault(d => d.Id == departmentId);
                        totalItems = await query.CountAsync();
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
                                    totalItems = await query.CountAsync();
                                    if (listFilter.SelectToken("unit") != null && !listFilter["unit"].ToString().Equals("all"))
                                    {
                                        string unit = listFilter["unit"].ToString();

                                        if (int.TryParse(unit, out int unitId))
                                        {
                                            var unitInSection = db.Departments.FirstOrDefault(u => u.ParentNode == sectionParenNode.DepartmentId && u.Id == unitId);
                                            if (unitInSection != null)
                                            {
                                                query = query.Where(item => item.UnitId == unitId);
                                                totalItems = await query.CountAsync();
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

            if (UserId != null)
            {
                query = query.Where(u =>
                    u.ApplicantId == UserId ||
                    (u.Status != 0 ||
                        u.Status == 1 || db.ApprovalPersons.Any(p =>
                            p.DocumentApprovalId == u.DocumentApprovalId &&
                            p.ApprovalPersonId == UserId &&
                            u.Status == 1)

                    )
                );
                totalItems = await query.CountAsync();
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
                d.IsDraft,
                d.IsReject,
                subject = d.Subject,
            }).ToList();

            return Ok(new
            {
                state = "true",
                listDcapproval,
                totalItems,
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
        public IHttpActionResult GetDocumentById(int? id, int? v)
        {
            if (id == null)
            {
                var errorResponse = new
                {
                    state = "false",
                    message = "The request is invalid"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse));
            }

            // Kiểm tra xem tham số v có giá trị null không
            if (v == null)
            {
                var errorResponse = new
                {
                    state = "false",
                    message = "The request is invalid"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse));
            }

            var checkUser = db.DocumentApprovals.Where(dc => dc.Id == id)
                .Any(doc => doc.ApplicantId == v
                ||
                doc.Status != 0 && db.ApprovalPersons.Any(user => user.DocumentApprovalId == doc.DocumentApprovalId
                && user.ApprovalPersonId == v)
            );
            if (checkUser)
            {
                var document = db.DocumentApprovals.FirstOrDefault(p => p.Id == id);
                if (document != null)
                {
                    var listComment = db.DocumentApprovalComments
                        .Where(c => c.DocumentApprovalId == document.DocumentApprovalId).ToList();

                    var documentInfo = new
                    {
                        document,
                        files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == document.DocumentApprovalId)
                            .OrderByDescending(p => p.CreateDate).ToList(),
                        persons = db.ApprovalPersons.OrderByDescending(p => p.PersonDuty).OrderBy(p => p.Index).Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList(),
                        comments = listComment
                        .OrderByDescending(d => d.CreateDate)
                        .Where(c => c.ParentNode == null) // Lọc các comment gốc (không có parentNode)
                        .Select(c => new
                        {
                            comment = c,
                            children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList() // Lấy các children comment của comment hiện tại
                        })
                        .ToList()
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
            else
            {
                var rejectPermission = new
                {
                    state = "false",
                    message = "You do not have permission to access this Document",
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, rejectPermission));
            }
        }

        [HttpGet]
        [Route("edit/{id}")]
        public IHttpActionResult GetEditDocumentById(int? id, int? v)
        {
            if (id == null)
            {
                var errorResponse = new
                {
                    state = "false",
                    message = "The request is invalid"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse));
            }

            // Kiểm tra xem tham số v có giá trị null không
            if (v == null)
            {
                var errorResponse = new
                {
                    state = "false",
                    message = "The request is invalid"
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse));
            }

            var checkUser = db.DocumentApprovals.Where(dc => dc.Id == id)
                .Any(dc => dc.ApplicantId == v
            );
            if (checkUser)
            {
                var document = db.DocumentApprovals.FirstOrDefault(dc => dc.Id == id);
                if (document != null)
                {
                    var listComment = db.DocumentApprovalComments
                    .Where(c => c.DocumentApprovalId == document.DocumentApprovalId).ToList();

                    var documentInfo = new
                    {
                        state = "true",
                        document,
                        files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == document.DocumentApprovalId)
                            .OrderByDescending(p => p.CreateDate).ToList(),
                        persons = db.ApprovalPersons.OrderByDescending(p => p.PersonDuty).OrderBy(p => p.Index).Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList(),
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
            else
            {
                var rejectPermission = new
                {
                    state = "false",
                    message = "You do not have permission to access this Document",
                };
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, rejectPermission));
            }
        }

        [HttpPost]
        [Route("edit/{id}")]
        public async Task<IHttpActionResult> EditDocument(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            try
            {
                var document = db.DocumentApprovals.FirstOrDefault(p => p.Id == id);
                var httpRequest = currentContext.Request;

                var bodyJson = httpRequest.Form["Data"];
                var body = JsonConvert.DeserializeObject<DocumentApproval>(bodyJson);

                var files = httpRequest.Files;

                DocumentApprovalComment comment = new DocumentApprovalComment();

                if (body != null)
                {
                    document.ApplicantId = body.ApplicantId;
                    document.ApplicantName = body.ApplicantName;
                    document.DepartmentId = body.DepartmentId;
                    document.DepartmentName = db.Departments.SingleOrDefault(item => item.Id == body.DepartmentId)?.DepartmentName;
                    document.SectionId = body.SectionId;
                    document.SectionName = db.Departments.SingleOrDefault(item => item.Id == body.SectionId)?.DepartmentName;
                    document.UnitId = body.UnitId;
                    document.UnitName = db.Departments.SingleOrDefault(item => item.Id == body.UnitId)?.DepartmentName;
                    document.CategoryId = body.CategoryId;
                    document.CategoryName = db.Categories.SingleOrDefault(item => item.Id == body.CategoryId)?.CategoryName;
                    document.DocumentTypeId = body.DocumentTypeId;
                    document.DocumentTypeName = db.DocumentTypes.SingleOrDefault(item => item.Id == body.DocumentTypeId)?.DocumentTypeName;
                    document.RelatedProposal = body.RelatedProposal;
                    document.Subject = body.Subject;
                    document.ContentSum = body.ContentSum;
                    document.IsDraft = body.IsDraft;
                    document.Status = body.IsDraft ? 0 : 1;
                    document.IsReject = false;
                    document.ProcessingBy = null;
                }

                db.SaveChanges();

                var approvalPerson = JObject.Parse(httpRequest.Form["ApprovalPerson"]);

                Dictionary<string, List<ApprovalPerson>> listPerson = new Dictionary<string, List<ApprovalPerson>>();

                string[] keysToCheck = { "approvers", "signers" };

                Dictionary<string, int> indexMap = new Dictionary<string, int>();

                var persons = db.ApprovalPersons.Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList();
                var personsDelete = new List<ApprovalPerson>();

                HashSet<(int, int, int)> processedPersons = new HashSet<(int, int, int)>();

                foreach (var key in keysToCheck)
                {
                    listPerson[key] = new List<ApprovalPerson>();
                    JArray items = (JArray)approvalPerson[key];

                    if (items != null)
                    {
                        indexMap[key] = 1;

                        foreach (var item in items)
                        {
                            int approvalPersonId = (int)item["ApprovalPersonId"];
                            int index = indexMap[key];
                            int personDuty = (int)item["PersonDuty"];
                            Guid documentApprovalId = Guid.Parse(item["DocumentApprovalId"].ToString());

                            // Tạo key để xác định mỗi ApprovalPerson
                            var personKey = (approvalPersonId, index, personDuty);

                            if (!processedPersons.Contains(personKey))
                            {
                                // Tạo mới ApprovalPerson
                                ApprovalPerson aP = new ApprovalPerson
                                {
                                    Index = index,
                                    ApprovalPersonId = approvalPersonId,
                                    ApprovalPersonName = item["ApprovalPersonName"].ToString(),
                                    ApprovalPersonEmail = item["ApprovalPersonEmail"].ToString(),
                                    DocumentApprovalId = documentApprovalId,
                                    PersonDuty = personDuty,
                                    IsProcessing = key == "approvers" && index == 1,
                                };

                                // Thêm ApprovalPerson vào danh sách đã xử lý
                                processedPersons.Add(personKey);

                                // Kiểm tra xem ApprovalPerson đã tồn tại trong cơ sở dữ liệu chưa
                                var existingPerson = persons.FirstOrDefault(p =>
                                    p.ApprovalPersonId == approvalPersonId &&
                                    p.Index == index &&
                                    p.PersonDuty == personDuty);

                                if (existingPerson == null)
                                {
                                    // Nếu không tồn tại, thêm mới vào cơ sở dữ liệu
                                    db.ApprovalPersons.Add(aP);
                                }
                                else
                                {
                                    if (existingPerson.PersonDuty == 1)
                                    {
                                        existingPerson.IsApprove = false;
                                    }
                                    else if (existingPerson.PersonDuty == 2)
                                    {
                                        existingPerson.IsSign = false;
                                    }

                                    if (existingPerson.IsLast)
                                    {
                                        existingPerson.IsLast = false;
                                    }

                                    if (existingPerson.IsReject)
                                    {
                                        existingPerson.IsReject = false;
                                    }

                                    existingPerson.ExecutionDate = null;
                                    existingPerson.IsProcessing = false;
                                }

                                listPerson[key].Add(aP);
                            }

                            indexMap[key]++;
                        }
                    }
                }

                // Xóa các ApprovalPerson không được xử lý trong danh sách items
                foreach (var person in persons)
                {
                    var personKey = (person.ApprovalPersonId, person.Index, person.PersonDuty);
                    if (!processedPersons.Contains(personKey))
                    {
                        personsDelete.Add(person);
                    }
                }

                // Xóa các ApprovalPerson cần xóa khỏi cơ sở dữ liệu
                foreach (var personToDelete in personsDelete)
                {
                    db.ApprovalPersons.Remove(personToDelete);
                }

                db.SaveChanges();


                if (listPerson.ContainsKey("approvers") && listPerson["approvers"].Count > 0)
                {
                    if (!document.IsDraft)
                    {
                        var resetProcessing = db.ApprovalPersons.Where(u => u.Index == 1
                        && u.PersonDuty == 1 && u.DocumentApprovalId == document.DocumentApprovalId).FirstOrDefault();
                        resetProcessing.IsProcessing = true;
                        document.ProcessingBy = listPerson["approvers"][0].ApprovalPersonName;
                        db.SaveChanges();
                    }
                }


                if (files.Count > 0)
                {
                    var fileApprovals = new List<Object>();
                    var listFile = db.DocumentApprovalFiles.Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList();
                    var filesDelete = new List<DocumentApprovalFile>();
                    HashSet<(string, int)> processeFile = new HashSet<(string, int)>();

                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFile fileUpload = files[i];

                        string Filepath = GetFilePath(document.DocumentApprovalId.ToString());

                        if (!Directory.Exists(Filepath))
                        {
                            Directory.CreateDirectory(Filepath);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);

                        string fileExtension = Path.GetExtension(fileUpload.FileName);

                        string alterPath = "Upload/Files/" +
                             document.DocumentApprovalId.ToString() + "/" + fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                        string fullName = document.DocumentApprovalId.ToString() + "/" + fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                        string fullPath = GetFilePath(fullName);

                        var fileApproval = new
                        {
                            fileUpload.FileName,
                            FilePath = alterPath,
                            FileSize = fileUpload.ContentLength,
                            FileType = fileUpload.ContentType,
                            document.DocumentApprovalId,
                            DocumentType = files.GetKey(i).Equals("approve") ? 1 : 2,
                        };

                        var fileKey = (fileApproval.FileName, fileApproval.DocumentType);

                        if (!processeFile.Contains(fileKey))
                        {
                            processeFile.Add(fileKey);

                            var existFile = listFile.FirstOrDefault(f => f.FileName == fileApproval.FileName
                            && f.DocumentType == fileApproval.DocumentType);

                            if (existFile == null)
                            {
                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    await fileUpload.InputStream.CopyToAsync(stream);
                                }

                                db.DocumentApprovalFiles.Add(new DocumentApprovalFile
                                {
                                    FileName = fileApproval.FileName,
                                    FileSize = fileApproval.FileSize,
                                    FilePath = fileApproval.FilePath,
                                    FileType = fileApproval.FileType,
                                    DocumentApprovalId = fileApproval.DocumentApprovalId,
                                    DocumentType = fileApproval.DocumentType
                                });

                                db.SaveChanges();
                            }
                            else
                            {
                                var listFileDeleteJson = httpRequest.Form["listFileDelete"];
                                var listFileDelete = JsonConvert.DeserializeObject<List<DocumentApprovalFile>>(listFileDeleteJson);
                                if(listFileDelete != null && listFileDelete.Count > 0)
                                {
                                    foreach (var file in listFileDelete)
                                    {
                                        if (file.DocumentFileId == existFile.DocumentFileId)
                                        {
                                            using (var stream = new FileStream(fullPath, FileMode.Create))
                                            {
                                                await fileUpload.InputStream.CopyToAsync(stream);
                                            }

                                            string filePathDelete = Path.Combine(HostingEnvironment.MapPath("~/"),existFile.FilePath);
                                            
                                            if(File.Exists(filePathDelete))
                                            {
                                                File.Delete(filePathDelete);

                                                existFile.FilePath = alterPath;

                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var file in listFile)
                    {
                        var fileKey = (file.FileName, file.DocumentType);
                        if (!processeFile.Contains(fileKey))
                        {
                            filesDelete.Add(file);
                        }
                    }

                    foreach (var fileToDelete in filesDelete)
                    {
                        string deleteFilePath = Path.Combine(HostingEnvironment.MapPath("~/"), fileToDelete.FilePath);
                        File.Delete(deleteFilePath);

                        db.DocumentApprovalFiles.Remove(fileToDelete);

                        db.SaveChanges();

                    }

                    if (!document.IsDraft)
                    {
                        comment = new DocumentApprovalComment
                        {
                            ApprovalPersonId = document.ApplicantId,
                            ApprovalPersonName = document.ApplicantName,
                            DocumentApprovalId = document.DocumentApprovalId,
                            CommentContent = "Submit the request",
                            IsFirst = true,
                        };

                        var parameter = new
                        {
                            code = document.RequestCode,
                            userDisplayName = document.ApplicantName,
                        };
                        var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                        var approval = db.ApprovalPersons.FirstOrDefault(p => p.DocumentApprovalId == document.DocumentApprovalId
                        && p.PersonDuty == 1 && p.Index == 1 && p.IsProcessing == true).ApprovalPersonId;

                        await _notificationService.SendNotification("WAITING_FOR_APPROVAL", parameter, module, document, approval, null);
                    }

                    db.DocumentApprovalComments.Add(comment);

                    db.SaveChanges();

                    return Ok(new
                    {
                        state = "true",
                        dc = document,
                        files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == document.DocumentApprovalId)
                            .OrderByDescending(p => p.CreateDate).ToList(),
                        persons = db.ApprovalPersons.OrderByDescending(p => p.PersonDuty).OrderBy(p => p.Index).Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList(),
                        message = document.IsDraft ? "The request successfully saved" : "The request successfully submited",
                    });
                }

                if (!document.IsDraft)
                {
                    comment = new DocumentApprovalComment
                    {
                        ApprovalPersonId = document.ApplicantId,
                        ApprovalPersonName = document.ApplicantName,
                        DocumentApprovalId = document.DocumentApprovalId,
                        CommentContent = "Submit the request",
                        IsFirst = true,
                    };
                }

                db.DocumentApprovalComments.Add(comment);

                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    dc = document,
                    files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == document.DocumentApprovalId)
                        .OrderByDescending(p => p.CreateDate).ToList(),
                    persons = db.ApprovalPersons.OrderByDescending(p => p.PersonDuty).OrderBy(p => p.Index).Where(p => p.DocumentApprovalId == document.DocumentApprovalId).ToList(),
                    message = document.IsDraft ? "The request successfully saved" : "The request successfully submited",
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("all")]
        public async Task<IHttpActionResult> GetAllListDocument()
        {
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
            }

            if (UserId != null)
            {
                query = query.Where(u =>
                    u.ApplicantId == UserId ||
                    (u.Status != 0 ||
                        u.Status == 1 || db.ApprovalPersons.Any(p =>
                            p.DocumentApprovalId == u.DocumentApprovalId &&
                            p.ApprovalPersonId == UserId &&
                            u.Status == 1)

                    )
                );
            }

            var dcapproval = query.ToList();
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
                d.IsDraft,
                d.IsReject,
                subject = d.Subject,
            }).ToList();



            return Ok(new
            {
                state = "true",
                listDcapproval,
                totalItems,
            });
        }
    }
}