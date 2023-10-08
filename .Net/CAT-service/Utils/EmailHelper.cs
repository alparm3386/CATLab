using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace cat.utils
{
    public class EmailHelper
    {
        private static Logger logger = new Logger();
        public static void SendDebugEmail(String msg, String subject, String to)
        {
            String from = "Errors@toppandigital.com";
            //other tweaks goes here.
            SendEmail(from, to, msg, subject);
        }

        public static void SendEmail(String from, String to, String msg, String subject)
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
                    String[] aTo = to.Split(';');
                    foreach (String toAddr in aTo)
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
                logger.Log("Email errors.log", "Failed to send email: " + ex + "\nsubject: " + subject + "\nmsg: " + msg);
            }
        }
    }
}