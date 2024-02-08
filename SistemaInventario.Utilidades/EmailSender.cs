using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendWithBrevo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Utilidades
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }

        public EmailSender(IConfiguration _config)
        {
            SendGridSecret = _config.GetValue<string>("Sendgrid:SecretKey");
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            BrevoClient client = new BrevoClient(SendGridSecret);

            Sender sender = new Sender("Xavi", "xvv1988@gmail.com");
            List<Recipient> recipients = new List<Recipient>
            {
                new Recipient("", email)
            };
            return client.SendAsync(sender, recipients, subject, htmlMessage, true);

            //var from = new EmailAddress("xvv1988@gmail.com");
            //var to = new EmailAddress(email);
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            //return client.SendEmailAsync(msg);
        }
    }
}
