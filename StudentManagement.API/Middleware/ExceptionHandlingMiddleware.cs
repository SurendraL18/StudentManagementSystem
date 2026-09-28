using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Common.Exceptions;

namespace StudentManagement.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            ProblemDetails problemDetails;

            if (exception is DuplicateUserEmailException duplicateEmailEx)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;

                problemDetails = new ProblemDetails
                {
                    Type = "/problems/duplicate-user-email",
                    Title = "Email address conflict.",
                    Status = StatusCodes.Status409Conflict,
                    Detail = duplicateEmailEx.Message,
                    Instance = context.Request.Path
                };
            }
            else
            {
                // Fallback baseline handling for generic/unhandled technical system crashes
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                problemDetails = new ProblemDetails
                {
                    Type = "/problems/internal-server-error",
                    Title = "An unexpected error occurred on the server.",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "A generic internal server failure occurred. Please reference the system logs.",
                    Instance = context.Request.Path
                };
            }

            await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
        }
    }
}
