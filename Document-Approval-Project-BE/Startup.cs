using Document_Approval_Project_BE.Provider;
using Document_Approval_Project_BE.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(Document_Approval_Project_BE.Startup))]

namespace Document_Approval_Project_BE
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var idProvider = new SignalRUserIdProvider();

            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);


            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfig = new HubConfiguration();
                map.RunSignalR(hubConfig);
            });
        }
    }
}
