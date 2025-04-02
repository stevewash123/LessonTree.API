using LessonTree.API.Configuration;
using LessonTree.DAL;
using LessonTree.DAL.Domain;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
#if DEBUG
try
{
    string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    if (Directory.Exists(logDirectory))
    {
        var logFiles = Directory.EnumerateFiles(logDirectory, "log*.txt");
        foreach (var file in logFiles)
        {
            File.Delete(file);
            Console.WriteLine($"Deleted log file: {file}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error deleting log files: {ex.Message}");
}
#endif

// Ensure the logs directory exists
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
Directory.CreateDirectory(logsPath);

// Debug configuration loading
Console.WriteLine("Configuration Sources:");
foreach (var source in builder.Configuration.Sources)
{
    Console.WriteLine($"- {source.GetType().Name}");
}
Console.WriteLine("appsettings.json ConnectionString: " + builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("appsettings.json Jwt:Key: " + builder.Configuration["Jwt:Key"]);

// Configure Serilog from appsettings.json with explicit debugging
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(logsPath, "log.txt"), // Use absolute path
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            retainedFileCountLimit: 7 // Optional: Keep last 7 days of logs
        );
    config.MinimumLevel.Debug(); // Ensure minimum level is set explicitly
});

ServiceConfiguration.ConfigureServices(builder);

// Configure JSON serialization to handle circular references
builder.Services.Configure<JsonOptions>(options =>
{
    // options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // Handle circular references
    // options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth if needed (default is 32)
    // options.JsonSerializerOptions.WriteIndented = true; // Optional: Make JSON readable for debugging
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<LessonTreeContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>(); // Added RoleManager
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        logger.LogInformation("Seeding database...");
        try
        {
            // Updated call: Added roleManager parameter
            await DatabaseSeeder.SeedDatabaseAsync(context, userManager, roleManager, logger, env);
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed database: {Message}", ex.Message);
            // Optionally, exit the application if seeding is critical
            // Environment.Exit(1);
        }
    }
}
else
{
    logger.LogInformation("Starting API without seeding...");
}

MiddlewareConfiguration.ConfigureMiddleware(app);

app.Run();