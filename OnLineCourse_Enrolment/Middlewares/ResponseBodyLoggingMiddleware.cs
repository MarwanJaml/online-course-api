

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
            var originalBodyStream = context.Response.Body;
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Only log successful responses
                if (context.Response.StatusCode < 400)
                {
                    await LogResponseBodyAsync(context, responseBody);
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
                responseBody.Dispose();
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