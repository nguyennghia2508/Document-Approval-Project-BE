using Autofac.Core;
using Document_Approval_Project_BE.Models;
using Microsoft.AspNet.SignalR.Messaging;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Syncfusion.DocIO.DLS;
using Syncfusion.XPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Xml.Linq;

namespace Document_Approval_Project_BE.Services
{
    public class MailService
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        public async Task SendEmail(DocumentApproval document, User person, string type, string url,string comment)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("System Center", "nmnhan2641999@gmail.com"));
                message.To.Add(new MailboxAddress(person.Email, person.Email));

                string htmlEmail = MailContent(type);
                htmlEmail = htmlEmail.Replace("{{Url}}", url);
                if (type.Equals("COMMENT"))
                {
                    message.Subject = "Request " + document.RequestCode + " has new comment";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Comment}}", comment);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Approve");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is waiting for your approval");
                }
                else if (type.Equals("WAITING_FOR_APPROVAL"))
                {
                    message.Subject = "[DOCUMENT] Request " + document.RequestCode + " is waiting for your approval";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Approve");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is waiting for your approval");
                }
                else if (type.Equals("APPROVED"))
                {
                    message.Subject = "[DOCUMENT] Request " + document.RequestCode + " is approved";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Approve");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is approved");
                }
                else if (type.Equals("WAITING_FOR_SIGNATURE"))
                {
                    message.Subject = "[DOCUMENT] Request " + document.RequestCode + " is waiting for your signature";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Sign");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is waiting for your signature");
                }
                else if (type.Equals("SIGNED"))
                {
                    message.Subject = "[DOCUMENT] Request " + document.RequestCode + " is signed";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Sign");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is signed");
                }
                else if (type.Equals("REJECTED"))
                {
                    message.Subject = "[DOCUMENT] Request " + document.RequestCode + " is reject";
                    htmlEmail = htmlEmail.Replace("{{UserName}}", person.Username);
                    htmlEmail = htmlEmail.Replace("{{RequestCode}}", document.RequestCode);
                    htmlEmail = htmlEmail.Replace("{{Subject}}", document.Subject);
                    htmlEmail = htmlEmail.Replace("{{Category}}", document.CategoryName);
                    htmlEmail = htmlEmail.Replace("{{DocumentType}}", document.DocumentTypeName);
                    htmlEmail = htmlEmail.Replace("{{Content}}", document.ContentSum);
                    htmlEmail = htmlEmail.Replace("{{Applicant}}", document.ApplicantName);
                    htmlEmail = htmlEmail.Replace("{{Type}}", "Reject");
                    htmlEmail = htmlEmail.Replace("{{TypeName}}", "is reject");
                }


                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlEmail
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync("nmnhan2641999@gmail.com", "hgac ltcn ayyf gptr");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string MailContent(string type)
        {
            if(type.Equals("COMMENT"))
            {
                return
            @"
            <!DOCTYPE html>
            <html lang=""en"">

            <body>
                <div style=""margin:0;padding:0"">
                    <table align=""center"" border=""1"" cellpadding=""0"" cellspacing=""0"" width=""800"" style=""border-collapse:collapse"">


                        <tbody>
                            <tr>
                                <td>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td width=""260"" valign=""top"" style=""padding:10px 0 10px 0""><img alt=""Not Found""
                                                        width=""119"" height=""75"" style=""display:block""
                                                        src=""https://ci3.googleusercontent.com/meips/ADKq_NYJZLfcME8kUygTqkl5QUIWtVfSMHShJitPbvizk58-FKBOK9x1dmqg9w4zuEhhPaLN7dPJ61OmuMRYRGL8vDidc0dKqDHJGRHTASz6gi_tC2-GypB0jHQ3y-laMIQu3qoz3CzKyMrzTwLCxDzeU5v7nG1ZN2RKqkcN5adxyKeSvhr_uVgBNx9DcnBYs4l4SDnb2oxuq1PZzFE2_CJiBgk0gV7wzlvTaDzdR0ZoCWFJUGXbe0-YWB9OO8YqOBMsNcnlsUTRq4HNDfJTwmUMSfTAsrt-7Th3OHdrXRMx=s0-d-e1-ft#https://taskenstorageaccount.blob.core.windows.net/taskencontainer/logo_202003120338530924.PNG?sv=2017-11-09&amp;sr=b&amp;sig=7A7uQm14j7mRFezxJatkqYcRt0yd1htHimSNn5Ng45M%3D&amp;se=2024-07-11T04%3A47%3A47Z&amp;sp=rl""
                                                        class=""CToWUd"" data-bit=""iit"">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#2e8193"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td
                                                    style=""color:#ffffff;font-family:Arial,sans-serif;font-size:16px;line-height:20px;padding:10px 0 10px 30px"">
                                                    <b>APPROVAL SYSTEM</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td
                                                    style=""color:#ffffff;font-family:Arial,sans-serif;font-size:16px;line-height:20px;padding:2px 10px 10px 30px"">
                                                    Request 00004-eDOC-LMART-2024 has new comment </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#ffffff""
                                    style=""font-size:13px;font-family:'Segoe UI Regular';line-height:20px;padding:20px 30px 20px 30px"">
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td bgcolor=""#ffffff"" style=""padding-top:5px;padding-bottom:5px"">Dear {{UserName}}, </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-bottom:5px"">New comment on your request 
                                                        <a href=""#m_-7502411160828829170_m_-6625880887437377346_"">
                                                            {{RequestCode}} - {{Subject}}
                                                        </a>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-bottom:5px"">{{Comment}}</td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-bottom:5px""><a
                                                        href=""{{Url}}"" target=""_blank""
                                                        ;source=gmail&amp;ust=1715741914377000&amp;usg=AOvVaw2ybYcYPEwGQADkvEdpiHV-"">
                                                        Click here</a> to view detail.
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-top:10px"">Thank you, <br>
                                                    Online Approval System </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td bgcolor=""#2e8193"" style=""font-size:0;line-height:0"" height=""30""></td>
                            </tr>

                            <tr>
                                <td bgcolor=""#f4f4f4"" style=""padding:10px 20px 10px 20px"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style=""font-family:'Segoe UI Regular';font-size:11px;line-height:20px"">This
                                                    message is sent by Tasken - one of the programs, services and products of
                                                    EOFFICE solution package developed and operated by the company
                                                    <a href=""http://o365.vn/"" target=""_blank""
                                                        data-saferedirecturl=""https://www.google.com/url?q=http://o365.vn/&amp;source=gmail&amp;ust=1715741914377000&amp;usg=AOvVaw0LuCMZH3Z1q_ifOei0y1RZ"">Opus
                                                        Solution </a>.
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                        <tbody>
                                                            <tr>
                                                                <td width=""260"" valign=""top""
                                                                    style=""font-family:'Segoe UI Regular';font-size:12px;line-height:20px;padding:10px 0 10px 0"">
                                                                    Has been verified by Microsoft </td>
                                                                <td width=""260"" valign=""top"" align=""right""
                                                                    style=""padding:10px 0 10px 0""><img alt=""Chivas Solution""
                                                                        width=""124"" height=""22"" style=""display:block""
                                                                        src=""https://ci3.googleusercontent.com/meips/ADKq_NY3BCkDrIqtELiBJhkO0LSPHSoXiXqXI2JgDkxzgRBOUBj-yhvDc_-v-Kef3cSl7OLJKfMNEcSR6K47MvJm=s0-d-e1-ft#https://tasken.io/images/Logo-Tasken.png""
                                                                        class=""CToWUd"" data-bit=""iit"">
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                        </tbody>
                    </table>
                    <div class=""yj6qo""></div>
                    <div class=""adL"">
                    </div>
                    <div class=""adL""></div>
                    <div class=""adL"">
                    </div>
                    <div class=""adL"">
                    </div>
                    <div class=""adL"">
                    </div>
                </div>


            </body>

            </html>  
            ";
            }
            else if(type.Equals("REJECTED"))
            {
                return @"
            <!DOCTYPE html>
            <html lang=""en"">

            <body>
                <div style=""margin:0;padding:0"">
                    <div style=""display:none!important"">eOffice System Dear Nghia Nguyen Thanh, .Subject: test.Categories: Internal Documents.Document type: Charter.Document content: test.Requested by: Nghia Nguyen Thanh Approve &nbsp; Reject &nbsp; More detail Thank you, Online Approval
                        System This message is sent by Tasken - one of the programs, services and products of EOFFICE solution package developed and operated by the company Opus Solution . Has been verified by Microsoft</div>
                    <table align=""center"" border=""1"" cellpadding=""0"" cellspacing=""0"" width=""800"" style=""border-collapse:collapse"">

                        <tbody>
                            <tr>
                                <td>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td width=""260"" valign=""top"" style=""padding:10px 0 10px 0""><img alt=""Not Found"" width=""119"" height=""75"" style=""display:block"" src=""https://ci3.googleusercontent.com/meips/ADKq_NaopSdmLBhKqk17E8JfutNdnJRzih2hFht-ajPw7bRYGQ8GTlaDnGiu4DEr-UmmyXH5uhGWmyYqGnPJevkBz0bvnHGC5Y8MsL86lD8hEunuZfbDPi1pv4Rv-4jU-5ypdgj9D0sKxZoW-Mfeo-gRt-Jjb8u6-PT0DS7Ogya39uKwkqCANiMcVkdjxL8_k1DllCgJXZVtF9gItY57RYVdquWnHNAymAWIqL1sWofHQrvXKle_16Mz8zzjyggy9ar3LbNKR5IF-DsELcBCktP0VDDVSgUWcDNhOvNDf7x6KhZEJg_xIA2QDA=s0-d-e1-ft#https://taskenstorageaccount.blob.core.windows.net/taskencontainer/logo_202003120338530924.PNG?sv=2017-11-09&amp;sr=b&amp;sig=LZpAlN%2B%2FMIC2vTxDhJMlf%2FL4SqV%2BtAzUTh%2BHM9DEOug%3D&amp;se=2024-08-17T03%3A33%3A39Z&amp;sp=rl"" class=""CToWUd"" data-bit=""iit"">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#2e8193"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style=""color:#ffffff;font-family:Arial,sans-serif;font-size:16px;line-height:20px;padding:10px 0 10px 30px"">
                                                    <b>eOffice System</b> </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#ffffff"" style=""font-size:13px;font-family:'Segoe UI Regular';line-height:20px;padding:20px 30px 20px 30px"">
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td style=""padding-bottom:20px""><b>Request {{RequestCode}} is rejected</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor=""#ffffff"" style=""padding:5px 50px"">Dear {{UserName}}, </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding:5px 50px"">
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                        <tbody>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Subject: {{Subject}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Categories: {{Category}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Document type: {{DocumentType}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Document content: {{Content}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Requested by: {{Applicant}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>Please <a href=""{{Url}}"" target=""_blank"";source=gmail&amp;ust=1716432991096000&amp;usg=AOvVaw1eJL_hzSMbpx7EJ-XIiWZS"">
                                                                [click here]</a> to review</td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-top:20px""></td>
                                            </tr>
                                            <tr>
                                                <td style=""font-family:Arial,sans-serif;font-size:14px;font-weight:bold"">
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""300"">
                                                        <tbody>
                                                            <tr>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#4caf50"">
                                                                    <a style=""color:white;text-decoration-color:#4caf50;border-radius:5px;display:inline-block""  href=""{{Url}}?type={{Type}}"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw3VwqPmnSDAgF0GULYvAt53"">{{Type}}
                                                                    </a></td>
                                                                <td style=""font-size:0;line-height:0"" width=""10"">&nbsp; </td>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#f53525"">
                                                                    <a style=""color:white;text-decoration-color:#f53525;border-radius:5px;display:inline-block"" href=""{{Url}}?type=reject"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw2jAZ3domlcxdvZ2vPgg_Kl"">Reject
                                                                    </a></td>
                                                                <td style=""font-size:0;line-height:0"" width=""10"">&nbsp; </td>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#0ea3e1"">
                                                                    <a style=""color:white;text-decoration-color:#0ea3e1;border-radius:5px;display:inline-block"" href=""{{Url}}"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw31tsX8iphjBtvEHw2rlsDb"">More detail
                                                                    </a></td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-top:5px"">Thank you, <br>
                                                    Online Approval System </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td bgcolor=""#2e8193"" style=""font-size:0;line-height:0"" height=""30""></td>
                            </tr>

                            <tr>
                                <td bgcolor=""#f4f4f4"" style=""padding:10px 20px 10px 20px"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style=""font-family:'Segoe UI Regular';font-size:11px;line-height:20px"">This message is sent by Tasken - one of the programs, services and products of EOFFICE solution package developed and operated by the company
                                                    <a href=""http://o365.vn/"" target=""_blank"" data-saferedirecturl=""https://www.google.com/url?q=http://o365.vn/&amp;source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw0FAqX6rgkTp0ghJpAOcPhF"">Opus Solution </a>. </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                        <tbody>
                                                            <tr>
                                                                <td width=""260"" valign=""top"" style=""font-family:'Segoe UI Regular';font-size:12px;line-height:20px;padding:10px 0 10px 0"">
                                                                    Has been verified by Microsoft </td>
                                                                <td width=""260"" valign=""top"" align=""right"" style=""padding:10px 0 10px 0""><img alt=""Chivas Solution"" width=""124"" height=""22"" style=""display:block"" src=""https://ci3.googleusercontent.com/meips/ADKq_NY3BCkDrIqtELiBJhkO0LSPHSoXiXqXI2JgDkxzgRBOUBj-yhvDc_-v-Kef3cSl7OLJKfMNEcSR6K47MvJm=s0-d-e1-ft#https://tasken.io/images/Logo-Tasken.png"" class=""CToWUd"" data-bit=""iit"">
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                        </tbody>
                    </table><div class=""yj6qo""></div><div class=""adL"">
                    </div>
                </div>


            </body>

            </html>  
            ";
            }
            else
            {
                return @"
            <!DOCTYPE html>
            <html lang=""en"">

            <body>
                <div style=""margin:0;padding:0"">
                    <div style=""display:none!important"">eOffice System Dear Nghia Nguyen Thanh, .Subject: test.Categories: Internal Documents.Document type: Charter.Document content: test.Requested by: Nghia Nguyen Thanh Approve &nbsp; Reject &nbsp; More detail Thank you, Online Approval
                        System This message is sent by Tasken - one of the programs, services and products of EOFFICE solution package developed and operated by the company Opus Solution . Has been verified by Microsoft</div>
                    <table align=""center"" border=""1"" cellpadding=""0"" cellspacing=""0"" width=""800"" style=""border-collapse:collapse"">

                        <tbody>
                            <tr>
                                <td>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td width=""260"" valign=""top"" style=""padding:10px 0 10px 0""><img alt=""Not Found"" width=""119"" height=""75"" style=""display:block"" src=""https://ci3.googleusercontent.com/meips/ADKq_NaopSdmLBhKqk17E8JfutNdnJRzih2hFht-ajPw7bRYGQ8GTlaDnGiu4DEr-UmmyXH5uhGWmyYqGnPJevkBz0bvnHGC5Y8MsL86lD8hEunuZfbDPi1pv4Rv-4jU-5ypdgj9D0sKxZoW-Mfeo-gRt-Jjb8u6-PT0DS7Ogya39uKwkqCANiMcVkdjxL8_k1DllCgJXZVtF9gItY57RYVdquWnHNAymAWIqL1sWofHQrvXKle_16Mz8zzjyggy9ar3LbNKR5IF-DsELcBCktP0VDDVSgUWcDNhOvNDf7x6KhZEJg_xIA2QDA=s0-d-e1-ft#https://taskenstorageaccount.blob.core.windows.net/taskencontainer/logo_202003120338530924.PNG?sv=2017-11-09&amp;sr=b&amp;sig=LZpAlN%2B%2FMIC2vTxDhJMlf%2FL4SqV%2BtAzUTh%2BHM9DEOug%3D&amp;se=2024-08-17T03%3A33%3A39Z&amp;sp=rl"" class=""CToWUd"" data-bit=""iit"">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#2e8193"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style=""color:#ffffff;font-family:Arial,sans-serif;font-size:16px;line-height:20px;padding:10px 0 10px 30px"">
                                                    <b>eOffice System</b> </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td bgcolor=""#ffffff"" style=""font-size:13px;font-family:'Segoe UI Regular';line-height:20px;padding:20px 30px 20px 30px"">
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tbody>
                                            <tr>
                                                <td style=""padding-bottom:20px""><b>Request {{RequestCode}} {{TypeName}}</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor=""#ffffff"" style=""padding:5px 50px"">Dear {{UserName}}, </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding:5px 50px"">
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                        <tbody>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Subject: {{Subject}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Categories: {{Category}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Document type: {{DocumentType}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Document content: {{Content}}</td>
                                                            </tr>
                                                            <tr>
                                                                <td>.</td>
                                                                <td>Requested by: {{Applicant}}</td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-top:20px""></td>
                                            </tr>
                                            <tr>
                                                <td style=""font-family:Arial,sans-serif;font-size:14px;font-weight:bold"">
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""300"">
                                                        <tbody>
                                                            <tr>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#4caf50"">
                                                                    <a style=""color:white;text-decoration-color:#4caf50;border-radius:5px;display:inline-block""  href=""{{Url}}?type={{Type}}"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw3VwqPmnSDAgF0GULYvAt53"">{{Type}}
                                                                    </a></td>
                                                                <td style=""font-size:0;line-height:0"" width=""10"">&nbsp; </td>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#f53525"">
                                                                    <a style=""color:white;text-decoration-color:#f53525;border-radius:5px;display:inline-block"" href=""{{Url}}?type=reject"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw2jAZ3domlcxdvZ2vPgg_Kl"">Reject
                                                                    </a></td>
                                                                <td style=""font-size:0;line-height:0"" width=""10"">&nbsp; </td>
                                                                <td align=""center"" style=""padding-top:8px;padding-bottom:8px;border-radius:5px;background-color:#0ea3e1"">
                                                                    <a style=""color:white;text-decoration-color:#0ea3e1;border-radius:5px;display:inline-block"" href=""{{Url}}"" target=""_blank"";source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw31tsX8iphjBtvEHw2rlsDb"">More detail
                                                                    </a></td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding-top:5px"">Thank you, <br>
                                                    Online Approval System </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td bgcolor=""#2e8193"" style=""font-size:0;line-height:0"" height=""30""></td>
                            </tr>

                            <tr>
                                <td bgcolor=""#f4f4f4"" style=""padding:10px 20px 10px 20px"">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td style=""font-family:'Segoe UI Regular';font-size:11px;line-height:20px"">This message is sent by Tasken - one of the programs, services and products of EOFFICE solution package developed and operated by the company
                                                    <a href=""http://o365.vn/"" target=""_blank"" data-saferedirecturl=""https://www.google.com/url?q=http://o365.vn/&amp;source=gmail&amp;ust=1716264855605000&amp;usg=AOvVaw0FAqX6rgkTp0ghJpAOcPhF"">Opus Solution </a>. </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                        <tbody>
                                                            <tr>
                                                                <td width=""260"" valign=""top"" style=""font-family:'Segoe UI Regular';font-size:12px;line-height:20px;padding:10px 0 10px 0"">
                                                                    Has been verified by Microsoft </td>
                                                                <td width=""260"" valign=""top"" align=""right"" style=""padding:10px 0 10px 0""><img alt=""Chivas Solution"" width=""124"" height=""22"" style=""display:block"" src=""https://ci3.googleusercontent.com/meips/ADKq_NY3BCkDrIqtELiBJhkO0LSPHSoXiXqXI2JgDkxzgRBOUBj-yhvDc_-v-Kef3cSl7OLJKfMNEcSR6K47MvJm=s0-d-e1-ft#https://tasken.io/images/Logo-Tasken.png"" class=""CToWUd"" data-bit=""iit"">
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                        </tbody>
                    </table><div class=""yj6qo""></div><div class=""adL"">
                    </div>
                </div>


            </body>

            </html>  
            ";
            }
        }
    }
}