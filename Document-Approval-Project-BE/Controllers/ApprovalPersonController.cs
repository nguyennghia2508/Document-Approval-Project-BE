using Document_Approval_Project_BE.Models;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/person")]
    public class ApprovalPersonController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

        [HttpPost]
        [Route("approval")]
        public IHttpActionResult AddApproval([FromBody] ApprovalPerson approvalPerson)
        {
            try
            {
                var approval = db.ApprovalPersons
                    .FirstOrDefault(p => p.ApprovalPersonId == approvalPerson.ApprovalPersonId
                    && p.DocumentApprovalId == approvalPerson.DocumentApprovalId 
                    && p.Index == approvalPerson.Index && p.PersonDuty == 1);

                DocumentApprovalComment comment = new DocumentApprovalComment();
                var listComment = new List<DocumentApprovalComment>();
                var updateStatus = db.DocumentApprovals.FirstOrDefault(p => p.DocumentApprovalId == approval.DocumentApprovalId);

                if (approval != null)
                {

                    var nextApproval = db.ApprovalPersons
                    .Where(p => p.Index > approval.Index && p.PersonDuty == 1 && p.IsApprove == false)
                    .OrderBy(p => p.Index)
                    .FirstOrDefault();

                    // Nếu tìm thấy phần tử, cập nhật thuộc tính IsProcessing và lưu thay đổi vào cơ sở dữ liệu
                    if (nextApproval != null)
                    {
                        approval.IsApprove = true;
                        approval.IsProcessing = false;
                        
                        comment = new DocumentApprovalComment
                        {
                            DocumentApprovalId = approvalPerson.DocumentApprovalId,
                            ApprovalPersonId = approvalPerson.ApprovalPersonId,
                            ApprovalPersonName = approvalPerson.ApprovalPersonName,
                            CommentContent = approvalPerson.Comment,
                            CommentStatus = 1,
                        };

                        updateStatus.ProcessingBy = nextApproval.ApprovalPersonName;

                        approval.ExecutionDate = DateTime.Now;
                        nextApproval.IsProcessing = true;

                        db.DocumentApprovalComments.Add(comment);
                        db.SaveChanges();

                        var getApprovers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();
                        return Ok(new
                        {
                            state = "true",
                            approvers = getApprovers,
                            message = "Request " + updateStatus.RequestCode + " has been approved by " + approvalPerson.ApprovalPersonName,
                            comments = listComment.OrderByDescending(d => d.CreateDate)
                            .Where(c => c.ParentNode == null)
                            .Select(c => new
                            {
                                comment = c,
                                children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList()
                            })
                            .ToList(),
                        });
                    }
                    else
                    {
                        var nextSigner = db.ApprovalPersons
                        .Where(p => p.PersonDuty == 2 && p.IsSign == false 
                        && p.DocumentApprovalId == approval.DocumentApprovalId)
                        .OrderBy(p => p.Index)
                        .FirstOrDefault();
                        if (nextSigner != null)
                        {
                            approval.IsApprove = true;
                            approval.IsProcessing = false;

                            comment = new DocumentApprovalComment
                            {
                                DocumentApprovalId = approvalPerson.DocumentApprovalId,
                                ApprovalPersonId = approvalPerson.ApprovalPersonId,
                                ApprovalPersonName = approvalPerson.ApprovalPersonName,
                                CommentContent = approvalPerson.Comment,
                                CommentStatus = 1,
                            };

                            approval.ExecutionDate = DateTime.Now;
                            approval.IsLast = true;
                            nextSigner.IsProcessing = true;

                            updateStatus.ProcessingBy = nextSigner.ApprovalPersonName;
                            updateStatus.Status = 2;

                            db.DocumentApprovalComments.Add(comment);
                            db.SaveChanges();

                            var nextApprovers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();
                            var listSigner = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                            listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                            return Ok(new
                            {
                                state = "true",
                                message = "Request " + updateStatus.RequestCode + " has been approved by " + approvalPerson.ApprovalPersonName,
                                document = updateStatus,
                                approvers = nextApprovers,
                                signers = listSigner,
                                comments = listComment.OrderByDescending(d => d.CreateDate)
                                .Where(c => c.ParentNode == null)
                                .Select(c => new
                                {
                                    comment = c,
                                    children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList()
                                })
                                .ToList(),
                            });
                        }
                    }

                    //var approvers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();

                    //return Ok(new
                    //{
                    //    state = "true",
                    //    approvers,
                    //});
                }
                return Ok(new
                {
                    state = "false",
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("signed")]
        public IHttpActionResult AddSigner([FromBody] ApprovalPerson approvalPerson)
        {
            try
            {
                var signed = db.ApprovalPersons
                    .FirstOrDefault(p => p.ApprovalPersonId == approvalPerson.ApprovalPersonId
                    && p.DocumentApprovalId == approvalPerson.DocumentApprovalId 
                    && p.Index == approvalPerson.Index && p.PersonDuty == 2);

                DocumentApprovalComment comment = new DocumentApprovalComment();
                var listComment = new List<DocumentApprovalComment>();
                var updateStatus = db.DocumentApprovals.FirstOrDefault(p => p.DocumentApprovalId == signed.DocumentApprovalId);

                if (signed != null)
                {
                    var nextSigned = db.ApprovalPersons
                    .Where(p => p.Index > signed.Index && p.PersonDuty == 2 && p.IsSign == false)
                    .OrderBy(p => p.Index)
                    .FirstOrDefault();

                    // Nếu tìm thấy phần tử, cập nhật thuộc tính IsProcessing và lưu thay đổi vào cơ sở dữ liệu
                    if (nextSigned != null)
                    {
                        signed.IsSign = true;
                        signed.IsProcessing = false;

                        comment = new DocumentApprovalComment
                        {
                            DocumentApprovalId = approvalPerson.DocumentApprovalId,
                            ApprovalPersonId = approvalPerson.ApprovalPersonId,
                            ApprovalPersonName = approvalPerson.ApprovalPersonName,
                            CommentContent = approvalPerson.Comment,
                            CommentStatus = 2,
                        };

                        updateStatus.ProcessingBy = nextSigned.ApprovalPersonName;

                        signed.ExecutionDate = DateTime.Now;
                        nextSigned.IsProcessing = true;

                        db.DocumentApprovalComments.Add(comment);
                        db.SaveChanges();

                        var files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == updateStatus.DocumentApprovalId).ToList();
                        foreach (var file in files)
                        {
                            var filePath = Path.Combine(HostingEnvironment.MapPath("~/"), file.FilePath);
                            using (FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                PdfLoadedForm form = loadedDocument.Form;
                                if (form != null && form.Fields != null)
                                {
                                    PdfLoadedFormFieldCollection fieldCollection = form.Fields;
                                    if (fieldCollection.Count > 0)
                                    {
                                        foreach (PdfLoadedField field in fieldCollection)
                                        {
                                            if (field.Name != null && field is PdfLoadedTextBoxField)
                                            {
                                                string fieldName = field.Name;

                                                if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                {
                                                    int startIndex = "[{SignedDate".Length; // Độ dài của "[{SignerName"
                                                    int dotIndex = fieldName.IndexOf('.', startIndex); // Tìm vị trí của dấu chấm (.) sau "SignerName"

                                                    if (dotIndex != -1)
                                                    {
                                                        // Lấy phần số giữa "SignerName" và dấu chấm (.)
                                                        string signerNumber = fieldName.Substring(startIndex, dotIndex - startIndex);

                                                        if (int.TryParse(signerNumber, out int signerIndex))
                                                        {
                                                            if (signerIndex == signed.Index)
                                                            {
                                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                                (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).Text = DateTime.Now.ToString("dd/MM/yyyy");
                                                                (field as PdfLoadedTextBoxField).ReadOnly = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).ReadOnly = true;
                                            }
                                        }
                                        form.SetDefaultAppearance(false);
                                        //loadedDocument.Form.Flatten = true;
                                        loadedDocument.Save(docStream);
                                        loadedDocument.Close(true);
                                    }
                                    else
                                    {
                                        loadedDocument.Close(true);
                                    }
                                }
                                else
                                {
                                    loadedDocument.Close(true);
                                }
                            }
                        }

                        var getSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                        return Ok(new
                        {
                            state = "true",
                            document = updateStatus,
                            signers = getSigners,
                            message = "Request " + updateStatus.RequestCode + " has been signed by " + approvalPerson.ApprovalPersonName,
                            comments = listComment.OrderByDescending(d => d.CreateDate)
                            .Where(c => c.ParentNode == null)
                            .Select(c => new
                            {
                                comment = c,
                                children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList()
                            })
                            .ToList(),
                        });
                    }
                    else
                    {
                        signed.IsSign = true;
                        signed.IsProcessing = false;

                        comment = new DocumentApprovalComment
                        {
                            DocumentApprovalId = approvalPerson.DocumentApprovalId,
                            ApprovalPersonId = approvalPerson.ApprovalPersonId,
                            ApprovalPersonName = approvalPerson.ApprovalPersonName,
                            CommentContent = approvalPerson.Comment,
                            CommentStatus = 2,
                        };

                        signed.ExecutionDate = DateTime.Now;
                        signed.IsLast = true;

                        updateStatus.ProcessingBy = null;
                        updateStatus.Status = 4;

                        db.DocumentApprovalComments.Add(comment);
                        db.SaveChanges();

                        var files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == updateStatus.DocumentApprovalId).ToList();
                        foreach (var file in files)
                        {
                            var filePath = Path.Combine(HostingEnvironment.MapPath("~/"), file.FilePath);
                            using (FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                PdfLoadedForm form = loadedDocument.Form;
                                if (form != null && form.Fields != null)
                                {
                                    PdfLoadedFormFieldCollection fieldCollection = form.Fields;
                                    if (fieldCollection.Count > 0)
                                    {
                                        foreach (PdfLoadedField field in fieldCollection)
                                        {
                                            if (field.Name != null && field is PdfLoadedTextBoxField)
                                            {
                                                string fieldName = field.Name;

                                                if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                {
                                                    int startIndex = "[{SignedDate".Length; // Độ dài của "[{SignerName"
                                                    int dotIndex = fieldName.IndexOf('.', startIndex); // Tìm vị trí của dấu chấm (.) sau "SignerName"

                                                    if (dotIndex != -1)
                                                    {
                                                        // Lấy phần số giữa "SignerName" và dấu chấm (.)
                                                        string signerNumber = fieldName.Substring(startIndex, dotIndex - startIndex);

                                                        if (int.TryParse(signerNumber, out int signerIndex))
                                                        {
                                                            if (signerIndex == signed.Index)
                                                            {
                                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                                (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                                (field as PdfLoadedTextBoxField).Text = DateTime.Now.ToString("dd/MM/yyyy");
                                                                (field as PdfLoadedTextBoxField).ReadOnly = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                (field as PdfLoadedTextBoxField).ReadOnly = true;
                                            }
                                        }
                                        form.SetDefaultAppearance(false);
                                        loadedDocument.Save(docStream);
                                        loadedDocument.Close(true);
                                    }
                                    else
                                    {
                                        form.SetDefaultAppearance(false);
                                        loadedDocument.Save(docStream);
                                        loadedDocument.Close(true);
                                    }
                                }
                                else
                                {
                                    loadedDocument.Save(docStream);
                                    loadedDocument.Close(true);
                                }
                            }
                        }
                        var nextSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                        return Ok(new
                        {
                            state = "true",
                            document = updateStatus,
                            message = "Request " + updateStatus.RequestCode + " has been signed by " + approvalPerson.ApprovalPersonName,
                            signers = nextSigners,
                            comments = listComment.OrderByDescending(d => d.CreateDate)
                            .Where(c => c.ParentNode == null)
                            .Select(c => new
                            {
                                comment = c,
                                children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList()
                            })
                            .ToList(),
                        });
                    }

                    //var signers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();

                    //return Ok(new
                    //{
                    //    state = "true",
                    //    signers,
                    //});
                }
                return Ok(new
                {
                    state = "false",
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("reject")]
        public IHttpActionResult RejectDocument([FromBody] ApprovalPerson approvalPerson)
        {
            try
            {
                var person = db.ApprovalPersons
                    .FirstOrDefault(p => p.ApprovalPersonId == approvalPerson.ApprovalPersonId
                    && p.DocumentApprovalId == approvalPerson.DocumentApprovalId
                    && p.Index == approvalPerson.Index && p.PersonDuty == approvalPerson.PersonDuty);

                DocumentApprovalComment comment = new DocumentApprovalComment();
                var listComment = new List<DocumentApprovalComment>();
                var updateStatus = db.DocumentApprovals.FirstOrDefault(p => p.DocumentApprovalId == person.DocumentApprovalId);

                if (person != null)
                {
                    person.IsReject = true;
                    person.IsProcessing = false;
                    person.ExecutionDate = DateTime.Now;

                    comment = new DocumentApprovalComment
                    {
                        DocumentApprovalId = approvalPerson.DocumentApprovalId,
                        ApprovalPersonId = approvalPerson.ApprovalPersonId,
                        ApprovalPersonName = approvalPerson.ApprovalPersonName,
                        CommentContent = approvalPerson.Comment,
                        CommentStatus = 3,
                    };

                    updateStatus.ProcessingBy = db.DocumentApprovals
                    .FirstOrDefault(p => p.DocumentApprovalId == person.DocumentApprovalId).ApplicantName;
                    updateStatus.Status = 3;
                    updateStatus.IsReject = true;
                    updateStatus.ProcessingBy = null;

                    db.DocumentApprovalComments.Add(comment);
                    db.SaveChanges();

                    var getAllPerson = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId);
                    listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                    return Ok(new
                    {
                        state = "true",
                        document = updateStatus,
                        approvers = getAllPerson.Where(p => p.PersonDuty == 1).ToList(),
                        signers = getAllPerson.Where(p => p.PersonDuty == 2).ToList(),
                        comments = listComment.OrderByDescending(d => d.CreateDate)
                        .Where(c => c.ParentNode == null)
                        .Select(c => new
                        {
                            comment = c,
                            children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList()
                        })
                        .ToList(),
                    });
                }
                return Ok(new
                {
                    state = "false",
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("tagcode-info/{id}")]
        public IHttpActionResult GetInfoDocument(string id)
        {
            try
            {
                var document = db.DocumentApprovals
                    .FirstOrDefault(p => p.DocumentApprovalId.ToString() == id);

                ApprovalPerson person = new ApprovalPerson();

                if (document != null)
                {

                    var signers = db.ApprovalPersons
                    .Where(p => p.DocumentApprovalId == document.DocumentApprovalId && p.PersonDuty == 2)
                    .OrderBy(p => p.Index)
                    .ToList();

                    // Nếu tìm thấy phần tử, cập nhật thuộc tính IsProcessing và lưu thay đổi vào cơ sở dữ liệu
                    if (signers != null)
                    {

                        return Ok(new
                        {
                            state = "true",
                            document,
                            signers
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            state = "true",
                            document,
                        });
                    }
                }
                return Ok(new
                {
                    state = "false",
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
