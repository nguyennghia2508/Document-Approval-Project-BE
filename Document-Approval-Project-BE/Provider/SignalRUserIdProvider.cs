using Microsoft.AspNet.SignalR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Web;

namespace Document_Approval_Project_BE.Provider
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            try
            {
                var userId = request.QueryString["userId"];

                if(userId != null)
                {
                    return userId;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}