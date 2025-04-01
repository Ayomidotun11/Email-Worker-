# Email Worker Service

## Overview
A robust Windows Service built with .NET that manages and automates email distribution to registered users. This service implements enterprise-level patterns and practices for reliable email handling in a production environment.

## Features
- ✉️ Automated email sending with configurable intervals
- 🔄 Batch processing with customizable batch sizes
- ⚡ Asynchronous email processing
- 🔒 Secure SMTP authentication
- 🔁 Automatic retry mechanism for failed attempts
- 📊 Logging and monitoring capabilities
- 💾 Database integration for email queue management

## Technology Stack
- .NET 8.0+
- Entity Framework Core
- Microsoft.Extensions.Hosting
- SMTP Client Integration
- SQL Server (LocalDB)

## Configuration
The service can be configured through `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true
  },
  "WorkerSettings": {
    "IntervalMinutes": 1,
    "BatchSize": 10,
    "DelayBetweenEmailsMs": 1000,
    "MaxRetryAttempts": 3
  }
}
```

## Installation
1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Build the solution
4. Install the Windows Service using:
   ```powershell
   sc.exe create "EmailWorkerService" binpath="path_to_your_executable"
   ```

## Architecture
The service follows SOLID principles and implements the following patterns:
- Repository Pattern for data access
- Dependency Injection for loose coupling
- Background Service Pattern for continuous processing
- Unit of Work Pattern for transaction management

## Development
To run the service in development mode:
1. Open the solution in Visual Studio
2. Set up your local database
3. Update the connection string
4. Run the application

##
Your corrections and contrinution will be deeply appreciated 