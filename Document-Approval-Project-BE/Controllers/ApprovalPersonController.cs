﻿using Document_Approval_Project_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
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

                        updateStatus.ProcessingBy = approval.ApprovalPersonName;

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

                            updateStatus.ProcessingBy = approval.ApprovalPersonName;
                            updateStatus.Status = 2;

                            db.DocumentApprovalComments.Add(comment);
                            db.SaveChanges();

                            var nextApprovers = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 1).ToList();
                            var listSigner = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                            listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                            return Ok(new
                            {
                                state = "true",
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

                        updateStatus.ProcessingBy = signed.ApprovalPersonName;

                        signed.ExecutionDate = DateTime.Now;
                        nextSigned.IsProcessing = true;

                        db.DocumentApprovalComments.Add(comment);
                        db.SaveChanges();

                        var getSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                        listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                        return Ok(new
                        {
                            state = "true",
                            signers = getSigners,
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
                        && p.DocumentApprovalId == signed.DocumentApprovalId)
                        .OrderBy(p => p.Index)
                        .FirstOrDefault();
                        if (nextSigner != null)
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

                            updateStatus.ProcessingBy = signed.ApprovalPersonName;
                            updateStatus.Status = 4;

                            db.DocumentApprovalComments.Add(comment);
                            db.SaveChanges();

                            var nextSigners = db.ApprovalPersons.Where(p => p.DocumentApprovalId == approvalPerson.DocumentApprovalId && p.PersonDuty == 2).ToList();
                            listComment = db.DocumentApprovalComments.Where(c => c.DocumentApprovalId == approvalPerson.DocumentApprovalId).ToList();

                            return Ok(new
                            {
                                state = "true",
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
