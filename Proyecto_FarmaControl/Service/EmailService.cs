
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models.settings;

namespace Proyecto_FarmaControl.Service
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSetting _smtp;

        public EmailService(IOptions<SmtpSetting> settings)
        {
            _smtp = settings.Value;
        }

        public void SendPasswordResetEmail(
            string toEmail,
            string body)
        {
            if (string.IsNullOrWhiteSpace(_smtp.Host) ||
                string.IsNullOrWhiteSpace(_smtp.User) ||
                string.IsNullOrWhiteSpace(_smtp.Password))
            {
                throw new InvalidOperationException(
                    "La configuración SMTP está incompleta.");
            }

            using var client = new SmtpClient(
                _smtp.Host,
                _smtp.Port)
            {
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(
                    _smtp.User,
                    _smtp.Password),

                EnableSsl = _smtp.EnableSsl
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(
                    _smtp.User,
                    "FarmaControl"),

                Subject = "Confirmación / Recuperación de cuenta",

                Body = body,

                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            client.Send(mail);
        }
    }
}
