using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/document-type")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DocumentTypeController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllDocumentType()
        {
            try
            {
                var listDocumentType = db.DocumentTypes.ToList();
                var listCategory = db.Categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

                var modifiedListDocumentType = listDocumentType
                    .GroupBy(dt => dt.CategoryId)
                    .Select(group => new
                    {
                        Id = db.Categories.Single(id => id.CategoryId == group.Key).Id,
                        CategoryId = group.Key,
                        CategoryName = listCategory.ContainsKey(group.Key) ? listCategory[group.Key] : null,
                        Children = group.Select(dt => new
                        {
                            Id=dt.Id,
                            DocumentTypeId = dt.DocumentTypeId,
                            DocumentTypeName = dt.DocumentTypeName
                        }).ToList()
                    }).ToList();

                return Ok(new
                {
                    state = "true",
                    listDocumentType = modifiedListDocumentType
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddDocumentType([FromBody] DocumentType documentType)
        {
            try
            {
                if (documentType == null)
                {
                    return Ok(new
                    {
                        state = "false",
                        message = "Empty"
                    });
                }
                DocumentType dcumentType = new DocumentType
                {
                    DocumentTypeName = documentType.DocumentTypeName,
                    CategoryId = documentType.CategoryId,
                };

                db.DocumentTypes.Add(dcumentType);
                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    dcumentType,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
