using Document_Approval_Project_BE.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/comment")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CommentController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;
        public string GetFilePath(string path)
        {
            string rootPath = HostingEnvironment.MapPath("~/");

            if (!path.Equals(""))
                return Path.Combine(rootPath, "Upload\\Files", path);
            return Path.Combine(rootPath, "Upload\\Files");
        }

        [HttpPost]
        [Route("add")]
        public async Task<IHttpActionResult> AddComment()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }
            try
            {
                var httpRequest = currentContext.Request;

                var bodyJson = httpRequest.Form["Data"];
                var body = JsonConvert.DeserializeObject<DocumentApprovalComment>(bodyJson);

                var files = httpRequest.Files;
                DocumentApprovalComment comment = new DocumentApprovalComment();
                if (body != null)
                {
                    comment = new DocumentApprovalComment
                    {
                        ParentNode = body.ParentNode,
                        DocumentApprovalId = body.DocumentApprovalId,
                        ApprovalPersonId = body.ApprovalPersonId,
                        ApprovalPersonName = body.ApprovalPersonName,
                        CommentContent = body.CommentContent,
                        IsSubComment = body.IsSubComment,
                    };
                }

                db.DocumentApprovalComments.Add(comment);

                db.SaveChanges();

                var listComment = db.DocumentApprovalComments.ToList();

                if (files.Count > 0)
                {
                    var fileApprovals = new List<object>();

                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFile fileUpload = files[i];
                            FileInfo fileInfo = new FileInfo(fileUpload.FileName);

                            string Filepath = GetFilePath(comment.DocumentApprovalId.ToString() + "/comment");

                            if (!Directory.Exists(Filepath))
                            {
                                Directory.CreateDirectory(Filepath);
                            }

                            string fullPath = Path.Combine(Filepath, fileUpload.FileName);
                            string alterPath = "Upload/Files/" +
                                comment.DocumentApprovalId.ToString() + "/comment/" + fileUpload.FileName;

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
                                comment.DocumentApprovalId,
                                comment.CommentId,
                                DocumentType = 3,
                            };

                            db.DocumentApprovalFiles.Add(new DocumentApprovalFile
                            {
                                FileName = fileApproval.FileName,
                                FileSize = fileApproval.FileSize,
                                FilePath = fileApproval.FilePath,
                                FileType = fileApproval.FileType,
                                CommentId = fileApproval.CommentId,
                                DocumentApprovalId = fileApproval.DocumentApprovalId,
                                DocumentType = fileApproval.DocumentType
                            });
                        }

                    }

                    db.SaveChanges();

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
                        files = db.DocumentApprovalFiles
                        .Where(f => f.DocumentApprovalId == comment.DocumentApprovalId).ToList(),
                    });
                }

                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    comments = listComment.OrderByDescending(d => d.CreateDate)
                    .Where(c => c.ParentNode == null)
                    .Select(c => new
                    {
                        comment = c,
                        children = listComment.OrderByDescending(d => d.CreateDate).Where(child => child.ParentNode == c.Id).ToList(),
                        files = db.DocumentApprovalFiles
                        .Where(f => f.DocumentApprovalId == comment.DocumentApprovalId).ToList(),
                    })
                    .ToList(),
                    files = db.DocumentApprovalFiles.Where(f => f.DocumentApprovalId == comment.DocumentApprovalId).ToList(),
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
