using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace SABLANCE.Service
{
    public class ImailService
    {
        
        public async Task<bool> SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32("587")))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("dovy1915@gmail.com", "ixrplxaburpuglpi");// thay gmail và pass
                    smtpClient.EnableSsl = true;
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress("dovy1915@gmail.com");// thay gmail và pass
                        mailMessage.To.Add(toAddress);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                        mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                        await smtpClient.SendMailAsync(mailMessage);
                      
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendEmailBirthDay(string toAddress, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32("587")))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("dovy1915@gmail.com", "ixrplxaburpuglpi");// thay gmail và pass
                    smtpClient.EnableSsl = true;
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress("dovy1915@gmail.com");// thay gmail và pass
                        mailMessage.To.Add(toAddress);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                        mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                        await smtpClient.SendMailAsync(mailMessage);

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}