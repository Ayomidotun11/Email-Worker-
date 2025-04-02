using System;
using System.Threading;
using System.Threading.Tasks;
using Email_Worker_Service.Models;
using Email_Worker_Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Linq;

namespace Email_Worker_Service.Worker
{
    public class EmailWorker : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WorkerSettings _workerSettings;
        private readonly IAsyncPolicy _retryPolicy;

        public EmailWorker(
            IServiceProvider serviceProvider,
            IOptions<WorkerSettings> workerSettings,
            ILogger<EmailWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _workerSettings = workerSettings.Value;
            _logger = logger;

            // Configure retry policy
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_workerSettings.MaxRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Error occurred while processing emails. Retry attempt {RetryCount} after {RetryTimeSpan}s",
                            retryCount, timeSpan.TotalSeconds);
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Worker Service is starting. Scanning for users to process.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing emails");
                }

                _logger.LogInformation("Waiting {Minutes} minutes before next scan", _workerSettings.IntervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(_workerSettings.IntervalMinutes), stoppingToken);
            }
        }

        private async Task ProcessEmailsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();

            try
            {
                // Get users who haven't received emails yet
                var usersToProcess = (await databaseService.GetUsersForEmailProcessingAsync())
                    .Take(_workerSettings.BatchSize)
                    .ToList();

                if (!usersToProcess.Any())
                {
                    _logger.LogInformation("No new users to process");
                    return;
                }

                _logger.LogInformation("Found {Count} new users to process", usersToProcess.Count);

                foreach (var user in usersToProcess)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        try
                        {
                            string emailBody = await templateService.GetWelcomeEmailTemplateAsync(user.Name);
                            var emailMessage = new EmailMessage(
                                user.Email,
                                "Welcome to Our Service",
                                emailBody,
                                true);

                            await emailService.SendEmailAsync(emailMessage);
                            await databaseService.UpdateUserEmailStatusAsync(user.Id, DateTime.UtcNow);

                            _logger.LogInformation("Successfully sent welcome email to user {UserId} ({Email})", 
                                user.Id, user.Email);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process email for user {UserId} ({Email})", 
                                user.Id, user.Email);
                            throw;
                        }
                    });

                    // Add delay between emails to avoid overwhelming the SMTP server
                    if (!stoppingToken.IsCancellationRequested)
                        await Task.Delay(_workerSettings.DelayBetweenEmailsMs, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email processing batch");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Worker Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
