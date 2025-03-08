using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.API.Filters
{
    public class RequestLoggingFilter : IAsyncActionFilter
    {
        private readonly ILogger<RequestLoggingFilter> _logger;

        public RequestLoggingFilter(ILogger<RequestLoggingFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Log request details before action executes
            var request = context.HttpContext.Request;
            request.EnableBuffering(); // Allow rereading the body

            var method = request.Method;
            var path = request.Path;
            var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

            string body = string.Empty;
            if (request.Body.CanRead)
            {
                using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // Reset position for downstream reading
                }
            }

            _logger.LogInformation("Incoming request: {Method} {Path}, Query: {QueryString}, Body: {Body}", method, path, queryString, body);

            // Proceed to the action
            await next();
        }
    }
}