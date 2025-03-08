using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Text.Json;
using NWebsec.AspNetCore.Middleware;
using Microsoft.AspNetCore.Diagnostics;

namespace LessonTree.API.Configuration
{
    public static class MiddlewareConfiguration
    {
        public static void ConfigureMiddleware(WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Middleware: Handling {Method} request to {Path}", context.Request.Method, context.Request.Path);
                await next(context);
            });

            app.UseCors("AllowSwaggerAndUI");

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exception = errorFeature.Error;

                        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";

                        var includeStackTrace = bool.Parse(app.Services.GetRequiredService<IConfiguration>()["HealthChecks:IncludeStackTrace"] ?? "true");
                        var errorResponse = new
                        {
                            status = "error",
                            message = exception.Message,
                            stackTrace = includeStackTrace ? exception.StackTrace : null
                        };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                    }
                });
            });

            app.UseHsts();
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opt => opt.NoReferrer());

            // Removed UseIpRateLimiting since rate limiting is no longer needed

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LessonTree API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseSerilogRequestLogging();

            app.UseHealthChecks(app.Configuration["HealthChecks:Endpoint"], new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = app.Services.GetRequiredService<IConfiguration>()["HealthChecks:ResponseFormat"] ?? "application/json";
                    var includeDescription = bool.Parse(app.Services.GetRequiredService<IConfiguration>()["HealthChecks:IncludeDescription"] ?? "true");
                    var includeDuration = bool.Parse(app.Services.GetRequiredService<IConfiguration>()["HealthChecks:IncludeDuration"] ?? "true");

                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = includeDescription ? e.Value.Description : null,
                            duration = includeDuration ? e.Value.Duration.TotalMilliseconds : (double?)null
                        })
                    });
                    await context.Response.WriteAsync(result);
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Health check executed: {Status}", report.Status);
                }
            });

            app.MapControllers();
        }
    }
}