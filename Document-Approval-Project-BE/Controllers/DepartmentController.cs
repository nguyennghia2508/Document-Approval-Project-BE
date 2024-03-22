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
using System.Web.Hosting;
using System.Web;
using System.Web.Http;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/department")]
    public class DepartmentController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddDepartment([FromBody] Department department)
        {
            try
            {
                if (department == null)
                {
                    return Ok(new
                    {
                        state = "false",
                        message = "Empty"
                    });
                }
                Department dpartment = new Department
                {
                    DepartmentName = department.DepartmentName,
                    DepartmentCode = department.DepartmentCode,
                    DepartmentLevel = department.DepartmentLevel,
                    ContactInfo = department.ContactInfo,
                };

                db.Departments.Add(dpartment);
                db.SaveChanges();

                return Ok(new
                {
                    state = "true",
                    dp = dpartment,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
