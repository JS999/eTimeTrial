using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.IdentityModel.Abstractions;

namespace eTimeTrial.Web.Backend
{
   
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _telemetry;

        public GlobalExceptionMiddleware(RequestDelegate next, TelemetryClient telemetry)
        {
            _next = next;
            _telemetry = telemetry;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log to Application Insights with full context
                var telemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };

                telemetry.Properties["Path"] = context.Request.Path;
                telemetry.Properties["Method"] = context.Request.Method;
                telemetry.Properties["User"] = context.User.Identity?.Name ?? "Anonymous";

                _telemetry.TrackException(telemetry);

                // Return clean JSON error response
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var error = new
                {
                    message = "An unexpected error occurred.",
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(error);
            }
        }
    }
}
