using Document_Approval_Project_BE.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Messaging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Cors;

namespace Document_Approval_Project_BE.Hubs
{
    [HubName("SignalRHub")]
    public class SignalRHub : Hub
    {
        //private readonly IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SignalRHub>();
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        //public async Task SendNotification(string userId,string type, object parameter)
        //{
        //    await Clients.User(userId).addNotification(new
        //    {
        //        type,
        //        parameter,
        //    });
        //}
    }
}