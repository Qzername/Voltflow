using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using VoltflowAPI.Models;

namespace VoltflowAPI.Services
{
    public class DefaultEmailSender : IEmailSender
    {
        SmtpClient smtpClient;

        public DefaultEmailSender(IOptions<EmailSettings> options)
        {
            var settings = options.Value;

            smtpClient = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Email, settings.Password),
                EnableSsl = true
            };
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            MailMessage message = new MailMessage();

            message.To.Add(email);
            message.Subject = subject;
            message.Body = body;

            await smtpClient.SendMailAsync(message);
        }
    }
}
