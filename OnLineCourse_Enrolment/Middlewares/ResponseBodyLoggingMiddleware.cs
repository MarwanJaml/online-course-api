

// ResponseLoggingMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnLineCourse_Enrolment.Middlewares
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger;

        public ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Capture the original response body stream
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    // Replace the response body stream
                    context.Response.Body = responseBody;

                    // Continue with the pipeline
                    await _next(context);

                    // Log response details
                    _logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
                        context.Response.StatusCode,
                        context.Request.Method,
                        context.Request.Path);

                    // Log response body only for errors or if it's small
                    if (context.Response.StatusCode >= 400 || responseBody.Length < 1000)
                    {
                        await LogResponseBodyAsync(context, responseBody);
                    }

                    // Copy the response back to the original stream
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResponseLoggingMiddleware");
                throw; // Re-throw to let other middleware handle it
            }
        }

        private async Task LogResponseBodyAsync(HttpContext context, MemoryStream responseBody)
        {
            try
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                if (!string.IsNullOrEmpty(responseBodyText))
                {
                    _logger.LogInformation("Response Body: {ResponseBody}", responseBodyText);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log response body");
            }
        }
    }
}