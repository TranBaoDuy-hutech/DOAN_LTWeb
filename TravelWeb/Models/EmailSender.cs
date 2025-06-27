using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging; 

namespace TravelWeb.Services 
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["SmtpSettings:Host"];
                var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
                var smtpUsername = _configuration["SmtpSettings:Username"];
                var smtpPassword = _configuration["SmtpSettings:Password"];
                var smtpEnableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]);
                var fromEmail = _configuration["SmtpSettings:FromEmail"];

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = smtpEnableSsl;
                    client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "Việt Lữ Travel | Support"), 
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    _logger.LogInformation($"Attempting to send email to {email} from {fromEmail} via {smtpHost}:{smtpPort}");
                    client.Send(mailMessage); 
                    _logger.LogInformation($"Email sent successfully to {email}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}.");              
            }
            return Task.CompletedTask;
        }
    }
}