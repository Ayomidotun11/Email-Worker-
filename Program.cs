using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Email_Worker_Service.Data;
using Email_Worker_Service.Services;
using Email_Worker_Service.Worker;
using Email_Worker_Service.Data.UnitOfWork;
using Email_Worker_Service.Models;
using Email_Worker_Service;
using Microsoft.EntityFrameworkCore.SqlServer; // Add this using directive

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // Configure Settings
        services.Configure<EmailSettings>(hostContext.Configuration.GetSection("EmailSettings"));
        services.Configure<WorkerSettings>(hostContext.Configuration.GetSection("WorkerSettings"));

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Services
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Add Worker
        services.AddHostedService<Email_Worker_Service.Worker.EmailWorker>();
    })
    .Build();

// Initialize the database
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

await host.RunAsync();