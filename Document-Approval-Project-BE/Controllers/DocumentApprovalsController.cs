using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using Document_Approval_Project_BE.Models;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/documentapproval")]
    public class DocumentApprovalsController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        [NonAction]
        public string GetFilePath(string path)
        {
            if(!path.Equals(""))
                return Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files\\" + path);
            return Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");
        }

        public class DocumentData
        {
            public DocumentApproval Document { get; set; }
            public DocumentApprovalFile File { get; set; }
        }


        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddDocument([FromBody] DocumentData documentData)
        {
            if (documentData == null || documentData.Document == null)
            {
                return BadRequest("Request body is empty or document data is missing.");
            }

            DocumentApproval document = documentData.Document;
            DocumentApprovalFile file = documentData.File;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null)
            {
                db.DocumentApprovals.Add(document);
                try
                {
                    db.SaveChanges();
                    return Ok(new
                    {
                        state = "true",
                        msg = "Add document success"

                    });
                }
                catch (DbUpdateException)
                {
                    return InternalServerError();
                }
            }
            return Ok(new
            {
                state = "true",
                msg = "nothing"

            });
        }

    }
}