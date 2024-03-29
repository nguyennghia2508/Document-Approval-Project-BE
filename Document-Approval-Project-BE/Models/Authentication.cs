using Antlr.Runtime;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Document_Approval_Project_BE.Models
{
    public class Authentication
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        private static readonly string SecrectKey = "Tasken-project-avn-documentApproval";

        public JwtPayload TokenDecode(HttpRequest req)
        {
            var bearerHeader = req.Headers["Authorization"];

            if (!string.IsNullOrEmpty(bearerHeader))
            {
                var bearer = bearerHeader.Split(' ')[1];
                try
                {
                    var handler = new JwtSecurityTokenHandler();

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecrectKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = handler.ValidateToken(bearer, validationParameters, out SecurityToken validatedToken);

                    var jwtPayload = (JwtSecurityToken)validatedToken;
                    var payload = jwtPayload.Payload;

                    return payload;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public User VerifyToken(HttpRequest req)
        {
            var tokenDecoded = TokenDecode(req);
            if (tokenDecoded != null)
            {
                var userIdClaim = tokenDecoded["Id"].ToString();
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    Guid userIdGuid;
                    if (Guid.TryParse(userIdClaim, out userIdGuid))
                    {
                        var user = db.Users.SingleOrDefault(u => u.UserId == userIdGuid);
                        return user;
                    }
                }
            }
            return null;
        }

        public string GenerateToken(Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecrectKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}