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
using System.Web.Http.Cors;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/department")]
    public class DepartmentController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllDepartment()
        {
            try
            {
                var departments = db.Departments.ToList();

                var topLevelDepartments = departments.Where(d => d.DepartmentLevel == 1).ToList();
                var departmentHierarchy = topLevelDepartments.Select(d => MapDepartmentHierarchy(d, departments)).ToList();

                return Ok(new
                {
                    state = "true",
                    departmentHierarchy
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

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
                    ParentNode = department.ParentNode,
                    ChildrenNode = department.ChildrenNode,
                    DepartmentManager = department.DepartmentManager,
                    Supervisor = department.Supervisor
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

        private dynamic MapDepartmentHierarchy(Department parentDepartment, List<Department> allDepartments)
        {
            var children = allDepartments.Where(d => d.ParentNode == parentDepartment.DepartmentId).ToList();
            var childrenNodes = children.Select(child => MapDepartmentHierarchy(child, allDepartments)).ToList();

            if(parentDepartment.ParentNode != Guid.Empty)
            {
                return new
                {
                    Id = parentDepartment.Id,
                    DepartmentId = parentDepartment.DepartmentId,
                    DepartmentName = parentDepartment.DepartmentName,
                    ParentNode = parentDepartment.ParentNode,
                    ParentNodeId = db.Departments.Single(id => id.DepartmentId == parentDepartment.ParentNode).Id,
                    DepartmentLevel = parentDepartment.DepartmentLevel,
                    Children = childrenNodes,
                };
            }
            return new
            {
                Id = parentDepartment.Id,
                DepartmentId = parentDepartment.DepartmentId,
                DepartmentName = parentDepartment.DepartmentName,
                ParentNode = parentDepartment.ParentNode,
                DepartmentLevel = parentDepartment.DepartmentLevel,
                Children = childrenNodes,
            };
        }
    }
}
