using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace ClaimsProcessingSystem.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var apiKey = _configuration["SendGridKey"];

            // This new check will tell us if the key is missing.
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("The 'SendGridKey' was not found in the configuration. Please make sure it is set correctly in your user secrets.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("no-reply@claimspro.com", "ClaimsPro System");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = $"<p>{message}</p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}