namespace Email_Worker_Service.Models
{
    public class WorkerSettings
    {
        public int IntervalMinutes { get; set; } = 60; // Default to 1 hour
        public int BatchSize { get; set; } = 50; // Default to 50 emails per batch
        public int DelayBetweenEmailsMs { get; set; } = 1000; // Default to 1 second
        public int MaxRetryAttempts { get; set; } = 3; // Default to 3 retry attempts
    }
} 