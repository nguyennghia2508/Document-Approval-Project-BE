using Document_Approval_Project_BE.Models;
using Document_Approval_Project_BE.Services;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/notification")]
    public class NotificationController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        //private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;
        //private readonly NotificationService _notificationService;

        //public NotificationController()
        //{
        //    _notificationService = new NotificationService();
        //}

        //[HttpGet]
        //[Route("getAll")]
        //public IHttpActionResult GetAll()
        //{
        //    try
        //    {
        //        var listNotificationsAll = db.Notifications.ToList();
        //        return Ok(new
        //        {
        //            state = "true",
        //            listNotificationsAll
        //        });
        //    }
        //    catch
        //    {
        //        return Ok(new
        //        {
        //            state = "false",
        //        });
        //    }
               
       
        //}


        [HttpGet]
        [Route("getNotificationBy/{userId}")]
        public IHttpActionResult GetAllNotification(int userId)
        {
            try
            {
                var userExists = db.Notifications.Any(u => u.CreateBy == userId);
                if (!userExists) 
                {
                    return Ok(new
                    {
                        state = "false",
                        message = "User not found"
                    });
                }
                var listNotificationsAll = db.Notifications.Where(p => p.CreateBy == userId).ToList();
                return Ok(new
                {
                    state = "true",
                    listNotificationsAll
                });

            }
            catch
            {
                return Ok(new
                {
                    state = "false",
                });
            }


        }

        //[HttpGet]
        //[Route("notificationBy/{id}")]
        //public IHttpActionResult GetNotificationsById(int? id)
        //{
        //    var listNotifications = db.Notifications.Where(n => n.CreateBy == id).ToList(); // Assuming there's a UserId field
        //    return Ok(new
        //    {
        //        state = "true",
        //        listNotifications
        //    });
        //}
    }
}
