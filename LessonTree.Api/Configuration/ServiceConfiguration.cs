using LessonTree.DAL;
using LessonTree.DAL.Repositories;
using LessonTree.BLL.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
using LessonTree.Service.Service.SystemConfig;
using Hangfire;
using Hangfire.PostgreSql;

namespace LessonTree.API.Configuration
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Always use PostgreSQL - migrated from SQLite
            // Check for Render DATABASE_URL via .NET Configuration system (production)
            var connectionString = builder.Configuration["DATABASE_URL"];

            if (!string.IsNullOrEmpty(connectionString))
            {
                // Convert PostgreSQL URI format to Npgsql connection string format
                connectionString = ConvertPostgresUriToConnectionString(connectionString);
            }
            else
            {
                // Fall back to appsettings.json connection string (development)
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            Console.WriteLine($"Using PostgreSQL database");
            Console.WriteLine($"Connection string configured: {!string.IsNullOrEmpty(connectionString)}");

            builder.Services.AddEntityFrameworkNpgsql()
                .AddDbContext<LessonTreeContext>(options =>
                {
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        // Configure for PostgreSQL operations
                        npgsqlOptions.CommandTimeout(120);
                    });

                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    }
                });

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

            // === SYSTEM CONFIG SERVICE ===
            builder.Services.AddTransient<ISystemConfigService, SystemConfigService>();

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
            // Use PostgreSQL for Hangfire - use same processed connection string
            var hangfireConnectionString = connectionString;

            Console.WriteLine($"Hangfire using PostgreSQL database: {hangfireConnectionString}");

            builder.Services.AddHangfire(configuration => configuration
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(hangfireConnectionString));

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

        /// <summary>
        /// Converts PostgreSQL URI format to Npgsql connection string format
        /// Example: postgres://user:pass@host:port/db?sslmode=require
        /// Becomes: Host=host;Port=port;Database=db;Username=user;Password=pass;SSL Mode=Require;
        /// </summary>
        private static string ConvertPostgresUriToConnectionString(string databaseUrl)
        {
            try
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo[0];
                var password = userInfo.Length > 1 ? userInfo[1] : "";

                var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={username};Password={password};";

                // Parse query parameters
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var queryParams = uri.Query.TrimStart('?').Split('&');
                    foreach (var param in queryParams)
                    {
                        var keyValue = param.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = keyValue[0].ToLower();
                            var value = keyValue[1];

                            if (key == "sslmode")
                            {
                                connectionString += $"SSL Mode={value.Replace("require", "Require")};";
                            }
                            else if (key == "channel_binding")
                            {
                                // Skip channel_binding as it's not supported by Npgsql connection string format
                            }
                            else
                            {
                                connectionString += $"{key}={value};";
                            }
                        }
                    }
                }

                return connectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting DATABASE_URL: {ex.Message}");
                return databaseUrl; // Return original if conversion fails
            }
        }
    }
}