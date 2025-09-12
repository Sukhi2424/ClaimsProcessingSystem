using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

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
            string apiKey = null;

            // --- MANUAL FILE READING WORKAROUND ---
            try
            {
                // Get the absolute path to the running application's directory
                var basePath = AppContext.BaseDirectory;
                var appSettingsPath = Path.Combine(basePath, "appsettings.Development.json");

                if (File.Exists(appSettingsPath))
                {
                    var jsonText = await File.ReadAllTextAsync(appSettingsPath);
                    using (var doc = JsonDocument.Parse(jsonText))
                    {
                        if (doc.RootElement.TryGetProperty("SendGridKey", out var keyElement))
                        {
                            apiKey = keyElement.GetString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // This will write any file reading errors to the debug console
                System.Diagnostics.Debug.WriteLine($"Error manually reading secrets: {ex.Message}");
            }
            // --- END OF WORKAROUND ---


            if (string.IsNullOrEmpty(apiKey))
            {
                // If it's still null, the file is unreadable or the key is missing from the file
                throw new Exception("SendGridKey is still null. Check appsettings.Development.json and its properties.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("sukhi.m2409@gmail.com", "ClaimsPro System");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = $"<p>{message}</p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}