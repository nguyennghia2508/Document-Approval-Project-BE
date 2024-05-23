using Document_Approval_Project_BE.Hubs;
using Document_Approval_Project_BE.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Document_Approval_Project_BE.Services
{
    public class NotificationService
    {
        private readonly IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<SignalRHub>();
        private readonly ProjectDBContext db = new ProjectDBContext();
        private readonly MailService _mailService;

        public NotificationService()
        {
            _mailService = new MailService();
        }

        public async Task SendNotification(string type, object parameter, Module module, DocumentApproval item, int userId,string message)
        {
            try
            {
                var person = db.Users.FirstOrDefault(p => p.Id == userId);
                var url = "";
                if (module.Id == 2)
                {
                    url = "http://localhost:3000/avn/documentapproval/view/" + item.Id;
                }
                var addNotification = new Notification
                {
                    ModuleId = module.Id,
                    Type = type,
                    CreateBy = person.Id,
                    Url = url,
                    Parameters = JsonConvert.SerializeObject(parameter),
                    ItemId = item.DocumentApprovalId
                };

                db.Notifications.Add(addNotification);

                db.SaveChanges();

                await _mailService.SendEmail(item, person, type, url,message);

                _hubContext.Clients.User(person.Id.ToString()).addNotification(new
                {
                    type,
                    parameter,
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}