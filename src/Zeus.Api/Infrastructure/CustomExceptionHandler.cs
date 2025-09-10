using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Diagnostics;
using Zeus.Api.Models;

namespace Zeus.Api.Infrastructure
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        // Returning a Stack trace is userful, nice thing to have in development.
        // But we don't want to return that level of detail in production
        // We can detect the current envirnment at runtime
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ErrorCodeMessages _errorCodeMessages;

        public CustomExceptionHandler(IHostEnvironment hostEnvironment, ErrorCodeMessages errorCodeMessages)
        {
             Guard.Against.Null(hostEnvironment);
             Guard.Against.Null(errorCodeMessages);

            _hostEnvironment = hostEnvironment;
            _errorCodeMessages = errorCodeMessages;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var isDevelopment = _hostEnvironment.IsDevelopment();
            var errorCodeInfo = _errorCodeMessages.GetErrorCodeInfo(ErrorCodes.INTERNAL_SERVER_ERROR);
            var title = !isDevelopment ? errorCodeInfo.Title ?? "Unkown error." : exception.Message;
            var message = !isDevelopment ? errorCodeInfo.Detail ?? "A unknown error occurred. Please try again later." : exception.StackTrace;

            // We don't need to log here
            // Unhanled exceptions should be logged at the service level and the http request level
            var error = new CustomProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Title = title,
                Detail = message,
                Type = "Error"
            };

            await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);
            return true;
        }
    }
}
