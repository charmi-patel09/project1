using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace JsonCrudApp.Services
{
    public class EmailService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public EmailService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public void SendOtpEmail(string email, string otp)
        {
            var logPath = Path.Combine(_env.ContentRootPath, "wwwroot", "logs", "otp_emails.log");
            var directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] To: {email} | OTP: {otp}{Environment.NewLine}";
            File.AppendAllText(logPath, logEntry);

            try
            {
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(smtpUser!);
                    mailMessage.To.Add(email);
                    mailMessage.Subject = "Your Login OTP";
                    mailMessage.Body = $"Your One-Time Password (OTP) for login is: {otp}. This code will expire in 2 minutes.";

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[ERROR] Failed to send email to {email}: {ex.Message}{Environment.NewLine}");
            }
        }

        public void SendResetLinkEmail(string email, string link)
        {
            var logPath = Path.Combine(_env.ContentRootPath, "wwwroot", "logs", "reset_emails.log");
            var directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] To: {email} | Link: {link}{Environment.NewLine}";
            File.AppendAllText(logPath, logEntry);

            try
            {
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(smtpUser!);
                    mailMessage.To.Add(email);
                    mailMessage.Subject = "Reset Your Password";
                    mailMessage.Body = $"Please click the following link to reset your password: {link}. This link will expire in 10 minutes.";
                    mailMessage.IsBodyHtml = false;

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[ERROR] Failed to send reset link to {email}: {ex.Message}{Environment.NewLine}");
            }
        }
    }
}
