using Document_Approval_Project_BE.Hubs;
using Document_Approval_Project_BE.Models;
using Document_Approval_Project_BE.Services;
using Microsoft.AspNet.SignalR;
using Syncfusion.Pdf;
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
using System.Threading.Tasks;
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
        private readonly NotificationService _notificationService;
        public ApprovalPersonController()
        {
            _notificationService = new NotificationService();
        }

        [HttpPost]
        [Route("approval")]
        public async Task<IHttpActionResult> AddApproval([FromBody] ApprovalPerson approvalPerson)
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
                    .Where(p => p.Index > approval.Index && p.DocumentApprovalId == approval.DocumentApprovalId && p.PersonDuty == 1 && p.IsApprove == false)
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

                        var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                        var parameterApproved = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = approval.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("APPROVED", parameterApproved, module, updateStatus,approval.ApprovalPersonId,null);

                        var parameterApproving = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = nextApproval.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("WAITING_FOR_APPROVAL", parameterApproving, module, updateStatus,nextApproval.ApprovalPersonId,null);

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
                        .Where(p => p.PersonDuty == 2 && p.IsSign == false && p.Index == 1
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

                            var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                            var parameterApproved = new
                            {
                                code = updateStatus.RequestCode,
                                userDisplayName = approval.ApprovalPersonName,
                            };

                            await _notificationService.SendNotification("APPROVED", parameterApproved, module, item: updateStatus, approval.ApprovalPersonId, null);

                            var parameterSign = new
                            {
                                code = updateStatus.RequestCode,
                                userDisplayName = nextSigner.ApprovalPersonName,
                            };

                            await _notificationService.SendNotification("WAITING_FOR_SIGNATURE", parameterSign, module, item: updateStatus, nextSigner.ApprovalPersonId, null);

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
        public async Task<IHttpActionResult> AddSigner([FromBody] ApprovalPerson approvalPerson)
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
                    .Where(p => p.Index > signed.Index && p.PersonDuty == 2 && p.DocumentApprovalId == signed.DocumentApprovalId && p.IsSign == false)
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
                            if (file.DocumentType == 1)
                            {
                                if (file.FileType.Equals("application/pdf"))
                                {
                                    var filePath = Path.Combine(HostingEnvironment.MapPath("~/"), file.FilePath);

                                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                                    string newFileName = fileName + "_" + DateTime.Now.Ticks.ToString() + ".pdf";

                                    string alterPath = "Upload/Files/" + signed.DocumentApprovalId.ToString() + "/" + newFileName;

                                    string tempFilePath = Path.Combine(HostingEnvironment.MapPath("~/"), alterPath);

                                    file.FilePath = alterPath;

                                    db.SaveChanges();

                                    if (File.Exists(filePath))
                                    {
                                        using (FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                                        {
                                            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                            PdfLoadedForm form = loadedDocument.Form;
                                            if (form != null && form.Fields != null)
                                            {
                                                PdfLoadedFormFieldCollection fieldCollection = form.Fields;
                                                List<PdfLoadedField> fieldsToRemove = new List<PdfLoadedField>();
                                                if (fieldCollection.Count > 0)
                                                {
                                                    foreach (PdfLoadedField field in fieldCollection)
                                                    {
                                                        if (field.Name != null && field is PdfLoadedTextBoxField)
                                                        {
                                                            string fieldName = field.Name;

                                                            if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                            {
                                                                int startIndex = "[{SignedDate".Length;
                                                                int dotIndex = fieldName.IndexOf('.', startIndex);

                                                                if (dotIndex != -1)
                                                                {
                                                                    int signerNumber = int.Parse(fieldName.Substring(startIndex, dotIndex - startIndex));

                                                                    if (signerNumber == signed.Index)
                                                                    {
                                                                        if ((field as PdfLoadedTextBoxField).ReadOnly)
                                                                        {
                                                                            (field as PdfLoadedTextBoxField).ReadOnly = false;
                                                                            (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                            (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                                            (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).Text = DateTime.Now.ToString("dd/MM/yyyy");
                                                                            (field as PdfLoadedTextBoxField).ReadOnly = true;
                                                                        }
                                                                        else
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
                                                        if (field.Name != null && field is PdfLoadedSignatureField)
                                                        {
                                                            string fieldName = field.Name;

                                                            if (fieldName.StartsWith("[{Signature") && fieldName.EndsWith("}]"))
                                                            {
                                                                int startIndex = "[{Signature".Length;
                                                                int dotIndex = fieldName.IndexOf('.', startIndex);

                                                                if (dotIndex != -1)
                                                                {
                                                                    int signerNumber = int.Parse(fieldName.Substring(startIndex, dotIndex - startIndex));

                                                                    if (signerNumber == signed.Index)
                                                                    {

                                                                        PdfPageBase currentPage = (field as PdfLoadedSignatureField).Page;

                                                                        PdfGraphics graphics = currentPage.Graphics;

                                                                        var signature = db.Users.FirstOrDefault(p => p.Id == signed.ApprovalPersonId);
                                                                        var signaturePath = Path.Combine(HostingEnvironment.MapPath("~/"), signature.SignatureFilePath);

                                                                        var bounds = (field as PdfLoadedSignatureField).Bounds;

                                                                        float centerX = (float)(bounds.X + bounds.Width / 5);

                                                                        float halfWidth = (float)(bounds.Width / 1.5);

                                                                        var centeredBounds = new RectangleF(centerX, bounds.Y, halfWidth, bounds.Height);

                                                                        PdfBitmap signatureImage = new PdfBitmap(signaturePath);

                                                                        graphics.DrawImage(signatureImage, centeredBounds);

                                                                        fieldsToRemove.Add(field);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    foreach (PdfLoadedField fieldToRemove in fieldsToRemove)
                                                    {
                                                        fieldCollection.Remove(fieldToRemove);
                                                    }
                                                    form.SetDefaultAppearance(false);

                                                    loadedDocument.Save(tempFilePath);
                                                    loadedDocument.Close(true);
                                                }
                                                else
                                                {
                                                    loadedDocument.Save(tempFilePath);
                                                    loadedDocument.Close(true);
                                                }
                                            }
                                            else
                                            {
                                                loadedDocument.Save(tempFilePath);
                                                loadedDocument.Close(true);
                                            }
                                        }
                                        File.Delete(filePath);
                                    }
                                }
                            }
                        }

                        var getSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                        var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                        var parameterSigned = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = signed.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("SIGNED", parameterSigned, module, item: updateStatus, signed.ApprovalPersonId, null);

                        var parameterSigning = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = nextSigned.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("WAITING_FOR_SIGNATURE", parameterSigning, module, item: updateStatus, nextSigned.ApprovalPersonId, null);


                        return Ok(new
                        {
                            state = "true",
                            document = updateStatus,
                            signers = getSigners,
                            files,
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
                            if (file.DocumentType == 1)
                            {

                                if (file.FileType.Equals("application/pdf"))
                                {
                                    var filePath = Path.Combine(HostingEnvironment.MapPath("~/"), file.FilePath);

                                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                                    string newFileName = fileName + "_" + DateTime.Now.Ticks.ToString() + ".pdf";

                                    string alterPath = "Upload/Files/" + signed.DocumentApprovalId.ToString() + "/" + newFileName;

                                    string tempFilePath = Path.Combine(HostingEnvironment.MapPath("~/"), alterPath);

                                    file.FilePath = alterPath;

                                    db.SaveChanges();

                                    if (File.Exists(filePath))
                                    {
                                        using (FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                                        {
                                            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                            PdfLoadedForm form = loadedDocument.Form;
                                            if (form != null && form.Fields != null)
                                            {
                                                PdfLoadedFormFieldCollection fieldCollection = form.Fields;
                                                List<PdfLoadedField> fieldsToRemove = new List<PdfLoadedField>();
                                                if (fieldCollection.Count > 0)
                                                {
                                                    foreach (PdfLoadedField field in fieldCollection)
                                                    {
                                                        if (field.Name != null && field is PdfLoadedTextBoxField)
                                                        {
                                                            string fieldName = field.Name;

                                                            if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                            {
                                                                int startIndex = "[{SignedDate".Length;
                                                                int dotIndex = fieldName.IndexOf('.', startIndex);

                                                                if (dotIndex != -1)
                                                                {
                                                                    int signerNumber = int.Parse(fieldName.Substring(startIndex, dotIndex - startIndex));

                                                                    if (signerNumber == signed.Index)
                                                                    {
                                                                        if ((field as PdfLoadedTextBoxField).ReadOnly)
                                                                        {
                                                                            (field as PdfLoadedTextBoxField).ReadOnly = false;
                                                                            (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                            (field as PdfLoadedTextBoxField).BorderColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).BackColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).BorderWidth = 0;
                                                                            (field as PdfLoadedTextBoxField).ForeColor = Color.Empty;
                                                                            (field as PdfLoadedTextBoxField).Text = DateTime.Now.ToString("dd/MM/yyyy");
                                                                            (field as PdfLoadedTextBoxField).ReadOnly = true;
                                                                        }
                                                                        else
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
                                                        if (field.Name != null && field is PdfLoadedSignatureField)
                                                        {
                                                            string fieldName = field.Name;

                                                            if (fieldName.StartsWith("[{Signature") && fieldName.EndsWith("}]"))
                                                            {
                                                                int startIndex = "[{Signature".Length;
                                                                int dotIndex = fieldName.IndexOf('.', startIndex);

                                                                if (dotIndex != -1)
                                                                {
                                                                    int signerNumber = int.Parse(fieldName.Substring(startIndex, dotIndex - startIndex));

                                                                    if (signerNumber == signed.Index)
                                                                    {

                                                                        PdfPageBase currentPage = (field as PdfLoadedSignatureField).Page;

                                                                        PdfGraphics graphics = currentPage.Graphics;

                                                                        var signature = db.Users.FirstOrDefault(p => p.Id == signed.ApprovalPersonId);
                                                                        var signaturePath = Path.Combine(HostingEnvironment.MapPath("~/"), signature.SignatureFilePath);

                                                                        var bounds = (field as PdfLoadedSignatureField).Bounds;

                                                                        float centerX = (float)(bounds.X + bounds.Width / 5);

                                                                        float halfWidth = (float)(bounds.Width / 1.5);

                                                                        var centeredBounds = new RectangleF(centerX, bounds.Y, halfWidth, bounds.Height);

                                                                        PdfBitmap signatureImage = new PdfBitmap(signaturePath);

                                                                        graphics.DrawImage(signatureImage, centeredBounds);

                                                                        fieldsToRemove.Add(field);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    foreach (PdfLoadedField fieldToRemove in fieldsToRemove)
                                                    {
                                                        fieldCollection.Remove(fieldToRemove);
                                                    }
                                                    form.SetDefaultAppearance(false);

                                                    loadedDocument.Save(tempFilePath);
                                                    loadedDocument.Close(true);
                                                }
                                                else
                                                {
                                                    loadedDocument.Save(tempFilePath);
                                                    loadedDocument.Close(true);
                                                }
                                            }
                                            else
                                            {
                                                loadedDocument.Save(tempFilePath);
                                                loadedDocument.Close(true);
                                            }
                                        }
                                        File.Delete(filePath);
                                    }
                                }
                            }
                        }
                        var nextSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                        var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                        var parameterSigned = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = signed.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("SIGNED", parameterSigned, module, item: updateStatus, signed.ApprovalPersonId, null);

                        return Ok(new
                        {
                            state = "true",
                            document = updateStatus,
                            message = "Request " + updateStatus.RequestCode + " has been signed by " + approvalPerson.ApprovalPersonName,
                            signers = nextSigners,
                            files,
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
        public async Task<IHttpActionResult> RejectDocument([FromBody] ApprovalPerson approvalPerson)
        {
            try
            {
                var person = db.ApprovalPersons
                    .FirstOrDefault(p => p.ApprovalPersonId == approvalPerson.ApprovalPersonId
                    && p.DocumentApprovalId == approvalPerson.DocumentApprovalId
                    && p.Index == approvalPerson.Index && p.PersonDuty == approvalPerson.PersonDuty);

                var applicant = db.DocumentApprovals
                    .FirstOrDefault(p => p.DocumentApprovalId == person.DocumentApprovalId);

                var applicantMail = db.Users
                   .FirstOrDefault(p => p.Id == applicant.ApplicantId);

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
                    ApprovalPerson Email = new ApprovalPerson();
                    Email = new ApprovalPerson
                    {
                        //ApprovalPersonName=approvalPerson.ApprovalPersonName,
                        ApprovalPersonEmail = applicantMail.Email,
                        DocumentApprovalId = approvalPerson.DocumentApprovalId,
                        ApprovalPersonName = applicant.ApplicantName

                    };

                    //person.ApprovalPersonEmail = applicantMail.Email;

                    var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                    var parameterRejected = new
                    {
                        code = updateStatus.RequestCode,
                        userDisplayName = updateStatus.ApplicantName,
                    };

                    await _notificationService.SendNotification("REJECTED", parameterRejected, module, item: updateStatus, Email.ApprovalPersonId, null);

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
                    .Join(db.Users,
                        ap => ap.ApprovalPersonId,
                        u => u.Id,
                        (ap, u) => new {
                            ApprovalPerson = ap,
                            User = u
                        })
                    .Select(result => new {
                        result.ApprovalPerson,
                        SignatureName = result.User.SignatureFileName,
                        SignaturePath = result.User.SignatureFilePath,
                    })
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

        [HttpPost]
        [Route("forward")]
        public async Task<IHttpActionResult> forwardPerson([FromBody] ApprovalPerson approvalPerson)
        {
            try
            {
                var forward = db.ApprovalPersons.
                    FirstOrDefault(p => p.Index == approvalPerson.Index
                    && p.PersonDuty == approvalPerson.PersonDuty
                    && p.DocumentApprovalId == approvalPerson.DocumentApprovalId);

                DocumentApprovalComment comment = new DocumentApprovalComment();
                var listComment = new List<DocumentApprovalComment>();
                var updateStatus = db.DocumentApprovals.FirstOrDefault(p => p.DocumentApprovalId == forward.DocumentApprovalId);


                if (forward != null && forward.IsProcessing == true)
                {
                    comment = new DocumentApprovalComment
                    {
                        DocumentApprovalId = approvalPerson.DocumentApprovalId,
                        ApprovalPersonId = approvalPerson.ApprovalPersonId,
                        ApprovalPersonName = approvalPerson.ApprovalPersonName,
                        CommentContent = approvalPerson.Comment,
                        CommentStatus = 4,
                        ForwardName = approvalPerson.ApprovalPersonEmail

                    };

                    var shared = new ApprovalPerson
                    {
                        ApprovalPersonId = forward.ApprovalPersonId,
                        ApprovalPersonName = forward.ApprovalPersonName,
                        DocumentApprovalId = forward.DocumentApprovalId,
                        ApprovalPersonEmail = forward.ApprovalPersonEmail,
                    };
                    db.ApprovalPersons.Add(shared);

                    forward.ApprovalPersonId = approvalPerson.ApprovalPersonId;
                    forward.ApprovalPersonName = approvalPerson.ApprovalPersonName;
                    forward.ApprovalPersonEmail = approvalPerson.ApprovalPersonEmail;
                    forward.PersonDuty = approvalPerson.PersonDuty;




                    db.DocumentApprovalComments.Add(comment);
                    db.SaveChanges();


                    var getAllPerson = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId);
                    listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();



                    var module = db.Modules.FirstOrDefault(p => p.Id == 2);

                    if (forward != null && forward.PersonDuty == 1)
                    {
                        var parameterApproving = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = forward.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("WAITING_FOR_APPROVAL", parameterApproving, module, item: updateStatus, forward.ApprovalPersonId, null);
                    }

                    else if (forward != null && forward.PersonDuty == 2)
                    {
                        var parameterSigning = new
                        {
                            code = updateStatus.RequestCode,
                            userDisplayName = forward.ApprovalPersonName,
                        };

                        await _notificationService.SendNotification("WAITING_FOR_SIGNATURE", parameterSigning, module, item: updateStatus, forward.ApprovalPersonId, null);
                    }
                    return Ok(new
                    {
                        state = "true",
                        document = updateStatus,
                        approvers = getAllPerson.Where(p => p.PersonDuty == 1).ToList(),
                        signers = getAllPerson.Where(p => p.PersonDuty == 2).ToList(),
                        forwardEmail = forward,
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

        [HttpPost]
        [Route("shared")]
        public IHttpActionResult sharePerson([FromBody] List<ApprovalPerson> approvalPersons)
        {

            try
            {
                DocumentApprovalComment comment = new DocumentApprovalComment();
                var listComment = new List<DocumentApprovalComment>();

                if (approvalPersons.Count > 0)
                {
                    foreach (var item in approvalPersons)
                    {
                        var existShare = db.ApprovalPersons.FirstOrDefault(p => p.ApprovalPersonId == item.ApprovalPersonId
                        && p.DocumentApprovalId == item.DocumentApprovalId);
                        if (existShare == null)
                        {
                            db.ApprovalPersons.Add(item);
                        }
                        db.SaveChanges();
                    }
                    return Ok(new
                    {
                        state = "true",
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
    }
}
