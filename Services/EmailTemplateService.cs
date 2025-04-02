using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Email_Worker_Service.Services
{
    public interface IEmailTemplateService
    {
        Task<string> GetWelcomeEmailTemplateAsync(string userName);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _logger;
        private const string TemplatesPath = "Templates";

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetWelcomeEmailTemplateAsync(string userName)
        {
            try
            {
                string templatePath = Path.Combine(TemplatesPath, "WelcomeEmail.html");
                string template = await File.ReadAllTextAsync(templatePath);
                return template.Replace("{UserName}", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading welcome email template");
                throw;
            }
        }
    }
} 