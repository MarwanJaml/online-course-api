// RequestLoggingMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnLineCourse_Enrolment.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Log basic request info
                _logger.LogInformation("Request: {Method} {Path}{QueryString}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString);

                // Only log body for POST/PUT requests to avoid performance issues
                if (context.Request.Method == "POST" || context.Request.Method == "PUT")
                {
                    await LogRequestBodyAsync(context);
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RequestLoggingMiddleware");
                throw; // Re-throw to let other middleware handle it
            }
        }

        private async Task LogRequestBodyAsync(HttpContext context)
        {
            try
            {
                // Enable buffering so the request can be read multiple times
                context.Request.EnableBuffering();

                // Read the request body
                var buffer = new byte[Convert.ToInt32(context.Request.ContentLength ?? 0)];
                if (buffer.Length > 0)
                {
                    await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                    var requestBody = Encoding.UTF8.GetString(buffer);

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        _logger.LogInformation("Request Body: {RequestBody}", requestBody);
                    }
                }

                // Reset the stream position so it can be read again
                context.Request.Body.Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log request body");
            }
        }
    }
}