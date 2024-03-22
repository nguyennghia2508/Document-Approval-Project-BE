using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddCategory([FromBody] Category category)
        {
            try
            {
                if (category == null)
                {
                    return Ok(new
                    {
                        state = "false",
                        message = "Empty"
                    });
                }
                Category ctgry = new Category
                {
                    CategoryName = category.CategoryName,
                };

                db.Categories.Add(ctgry);
                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    ctgry,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
