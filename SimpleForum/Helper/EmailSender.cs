using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Helper
{
    public class EmailSender : IEmailSender
    {
        //private readonly string apiSendGrid= "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";
        private readonly EmailSettings emailSettings;

        public EmailSender(EmailSettings emailSettings)
        {
            this.emailSettings = emailSettings;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return ExecuteAsync(subject, message, email);
        }
        public async Task ExecuteAsync(string subject, string message, string email)
        {
            var messageToSend = new MimeMessage
            {
                Sender = new MailboxAddress(emailSettings.SenderName, emailSettings.Sender),
                Subject = subject,
            };
            messageToSend.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.Sender));
            messageToSend.Body = new TextPart(TextFormat.Html) { Text = message };
            messageToSend.To.Add(new MailboxAddress(email));

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(emailSettings.MailServer, emailSettings.MailPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(emailSettings.Sender, emailSettings.Password);
                await smtp.SendAsync(messageToSend);
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
