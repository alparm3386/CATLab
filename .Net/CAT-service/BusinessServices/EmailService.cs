using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace CAT.BusinessServices
{
    public class EmailService
    {
        private ILogger _logger;

        public EmailService(ILogger logger)
        {
            _logger = logger;
        }

        public void SendDebugEmail(string msg, string subject, string to)
        {
            string from = "Errors@toppandigital.com";
            //other tweaks goes here.
            SendEmail(from, to, msg, subject);
        }

        public void SendEmail(string from, string to, string msg, string subject)
        {
            try
            {
                //set the smtp
                var smtp = new SmtpClient
                {
                    Host = "10.0.20.217"
                    /*Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, "password")*/
                };

                //send the message
                //using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = msg, IsBodyHtml = true })
                using (var message = new MailMessage())
                {
                    //from
                    message.From = new MailAddress(from, from);
                    //to
                    string[] aTo = to.Split(';');
                    foreach (string toAddr in aTo)
                        message.To.Add(new MailAddress(toAddr));
                    //Subject
                    message.Subject = subject;
                    //body 
                    message.Body = msg;
                    //...
                    message.IsBodyHtml = true;

                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Email errors.log", "Failed to send email: " + ex + "\nsubject: " + subject + "\nmsg: " + msg);
            }
        }
    }
}