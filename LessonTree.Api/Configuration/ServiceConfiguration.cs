using LessonTree.DAL;
using LessonTree.DAL.Repositories;
using LessonTree.BLL.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using LessonTree.API.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using LessonTree.DAL.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using LessonTree.BLL.Services;
using Hangfire;
using Hangfire.Storage.SQLite;

namespace LessonTree.API.Configuration
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Check for production database URL first, fallback to SQLite for local development
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var productionConnection = builder.Configuration.GetConnectionString("ProductionConnection");

            Console.WriteLine($"=== DATABASE CONNECTION DEBUG ===");
            Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrWhiteSpace(databaseUrl)}");
            Console.WriteLine($"DATABASE_URL length: {databaseUrl?.Length ?? 0}");
            Console.WriteLine($"DATABASE_URL value: '{databaseUrl ?? "NULL"}'");
            Console.WriteLine($"ProductionConnection exists: {!string.IsNullOrWhiteSpace(productionConnection)}");
            Console.WriteLine($"ProductionConnection length: {productionConnection?.Length ?? 0}");
            Console.WriteLine($"ProductionConnection value: '{productionConnection ?? "NULL"}'");

            // Use PostgreSQL if we have a valid DATABASE_URL or ProductionConnection
            var usePostgreSQL = !string.IsNullOrWhiteSpace(databaseUrl) || !string.IsNullOrWhiteSpace(productionConnection);

            if (usePostgreSQL)
            {
                // FORCE use ProductionConnection since DATABASE_URL is missing port
                // Prefer ProductionConnection (fixed), fallback to DATABASE_URL only if ProductionConnection missing
                var connectionString = !string.IsNullOrWhiteSpace(productionConnection) ? productionConnection : databaseUrl;

                Console.WriteLine($"Using PostgreSQL database");
                Console.WriteLine($"Connection source: {(!string.IsNullOrWhiteSpace(productionConnection) ? "ProductionConnection config (fixed)" : "DATABASE_URL env var (fallback)")}");
                Console.WriteLine($"Final connection string length: {connectionString?.Length ?? 0}");
                Console.WriteLine($"Final connection string: '{connectionString ?? "NULL"}'");

                builder.Services.AddEntityFrameworkNpgsql()
                    .AddDbContext<LessonTreeContext>(options =>
                    {
                        options.UseNpgsql(connectionString);

                        if (builder.Environment.IsDevelopment())
                        {
                            options.EnableSensitiveDataLogging();
                            options.EnableDetailedErrors();
                        }
                    });
            }
            else
            {
                Console.WriteLine("Using SQLite database for local development");
                builder.Services.AddEntityFrameworkSqlite()
                    .AddDbContext<LessonTreeContext>(options =>
                    {
                        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

                        if (builder.Environment.IsDevelopment())
                        {
                            options.EnableSensitiveDataLogging();
                            options.EnableDetailedErrors();
                        }
                    });
            }

            builder.Services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<LessonTreeContext>()
                .AddDefaultTokenProviders();

            // === EXISTING REPOSITORY REGISTRATIONS (unchanged) ===
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<ICourseRepository, CourseRepository>();
            builder.Services.AddTransient<ITopicRepository, TopicRepository>();
            builder.Services.AddTransient<ISubTopicRepository, SubTopicRepository>();
            builder.Services.AddTransient<ILessonRepository, LessonRepository>();
            builder.Services.AddTransient<IStandardRepository, StandardRepository>();
            builder.Services.AddTransient<IAttachmentRepository, AttachmentRepository>();
            builder.Services.AddTransient<INotesRepository, NotesRepository>();
            builder.Services.AddTransient<IScheduleRepository, ScheduleRepository>();
            builder.Services.AddTransient<IScheduleConfigurationRepository, ScheduleConfigurationRepository>();

            // === EXISTING SERVICE REGISTRATIONS (unchanged) ===
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<ICourseService, CourseService>();
            builder.Services.AddTransient<ITopicService, TopicService>();
            builder.Services.AddTransient<ISubTopicService, SubTopicService>();
            builder.Services.AddTransient<ILessonService, LessonService>();
            builder.Services.AddTransient<IStandardService, StandardService>();
            builder.Services.AddTransient<IAttachmentService, AttachmentService>();
            builder.Services.AddTransient<IScheduleService, ScheduleService>();
            builder.Services.AddTransient<IScheduleConfigurationService, ScheduleConfigurationService>();
            builder.Services.AddTransient<INoteService, NoteService>();

            // === NEW SERVICE REGISTRATION FOR SCHEDULE GENERATION MIGRATION ===
            builder.Services.AddTransient<IScheduleGenerationService, ScheduleGenerationService>();

            // === BACKGROUND SCHEDULE SERVICE ===
            builder.Services.AddTransient<IBackgroundScheduleService, BackgroundScheduleService>();

            // === REPORT GENERATION SERVICE ===
            builder.Services.AddTransient<IReportGenerationService, ReportGenerationService>();

            // === EXISTING HEALTH CHECKS (unchanged) ===
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(builder.Configuration["HealthChecks:Checks:0:Description"]))
                .AddDbContextCheck<LessonTreeContext>(
                    builder.Configuration["HealthChecks:Checks:1:Name"],
                    failureStatus: Enum.Parse<HealthStatus>(builder.Configuration["HealthChecks:Checks:1:FailureStatus"] ?? "Unhealthy"));

            // === EXISTING AUTHENTICATION (unchanged) ===
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization();

            // === EXISTING CORS (unchanged) ===
            builder.Services.AddCors(options =>
            {
                var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
                options.AddPolicy("AllowSwaggerAndUI", policy =>
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });

            builder.Services.AddMemoryCache(); // Keep MemoryCache for other uses

            // === HANGFIRE CONFIGURATION ===
            // ✅ PROPER FIX: Always use separate database file for Hangfire to avoid conflicts
            var hangfireConnectionString = "Data Source=LessonTree-Hangfire.db";

            Console.WriteLine($"Hangfire using database: {hangfireConnectionString}");

            builder.Services.AddHangfire(configuration => configuration
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(hangfireConnectionString));

            builder.Services.AddHangfireServer();

            // === EXISTING CONTROLLERS AND SWAGGER (unchanged) ===
            builder.Services.AddScoped<RequestLoggingFilter>();
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<RequestLoggingFilter>();
                options.OutputFormatters.Insert(0, new SystemTextJsonOutputFormatter(new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles, // Handle circular references without $id/$values overhead
                    MaxDepth = 64, // Increase max depth if needed (default is 32)
                    WriteIndented = true, // Optional: Make JSON readable for debugging
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver(), // Required for .NET 8
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            });
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "LessonTree API", Version = "v1" });

                // Add security definition for JWT Bearer
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' followed by a space and the JWT token."
                });

                // Add security requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] { }
                    }
                });
            });

            builder.Services.AddAutoMapper(typeof(MappingProfile)); // Specify the type containing mappings
        }
    }
}