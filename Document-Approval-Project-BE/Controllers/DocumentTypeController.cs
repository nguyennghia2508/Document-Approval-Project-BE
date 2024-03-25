using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/document-type")]
    public class DocumentTypeController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllDocumentType()
        {
            try
            {
                var listdcumentType = db.DocumentTypes.ToList();
                var listCategory = db.Categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

                var modifiedListdcumentType = listdcumentType.Select(doc =>
                {
                    if(listCategory.ContainsKey(doc.CategoryId))
                    {
                        string categoryName = listCategory[doc.CategoryId];
                        return new
                        {
                            doc.Id,
                            doc.DocumentTypeId,
                            doc.DocumentTypeName,
                            CategoryName = categoryName
                        };
                    }
                    return null;
                }).ToList();

                return Ok(new
                {
                    state = "true",
                    listdcumentType = modifiedListdcumentType,
                    listCategory
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
