using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/person")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
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
                        nextApproval.IsProcessing = true;
                        db.SaveChanges();

                        var getApprovers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();

                        return Ok(new
                        {
                            state = "true",
                            approvers = getApprovers,
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
                            nextSigner.IsProcessing = true;
                            db.SaveChanges();

                            var nextApprovers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();

                            return Ok(new
                            {
                                state = "true",
                                approvers = nextApprovers,
                                isLast = true,
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
                        nextSigned.IsProcessing = true;
                        db.SaveChanges();

                        var getSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();

                        return Ok(new
                        {
                            state = "true",
                            signers = getSigners,
                        });
                    }
                    else
                    {
                        var nextSigner = db.ApprovalPersons
                        .Where(p => p.PersonDuty == 2 && p.IsSign == false
                        && p.DocumentApprovalId == signed.DocumentApprovalId)
                        .OrderBy(p => p.Index)
                        .FirstOrDefault();
                        if (nextSigner != null)
                        {
                            signed.IsSign = true;
                            signed.IsProcessing = false;
                            db.SaveChanges();

                            var nextSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();

                            return Ok(new
                            {
                                state = "true",
                                signers = nextSigners,
                                isLast = true,
                            });
                        }
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
    }
}
