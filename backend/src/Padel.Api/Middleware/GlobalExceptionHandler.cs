using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;

namespace Padel.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                (ProblemDetails)new ValidationProblemDetails(validationException.Errors)
                {
                    Title = "One or more validation failures occurred.",
                    Status = StatusCodes.Status400BadRequest,
                }),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Title = "Resource not found.",
                    Detail = notFoundException.Message,
                    Status = StatusCodes.Status404NotFound,
                }),

            AuthenticationFailedException authException => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Title = "Authentication failed.",
                    Detail = authException.Message,
                    Status = StatusCodes.Status401Unauthorized,
                }),

            SlotUnavailableException slotException => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Title = "One or more selected slots are no longer available.",
                    Detail = slotException.Message,
                    Status = StatusCodes.Status409Conflict,
                }),

            DbUpdateException => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Title = "The request conflicts with existing data.",
                    Status = StatusCodes.Status409Conflict,
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                }),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        // Serialize via the `object` overload so System.Text.Json picks up the runtime type
        // (e.g. ValidationProblemDetails.Errors) instead of only the declared ProblemDetails members.
        await httpContext.Response.WriteAsJsonAsync((object)problemDetails, cancellationToken);

        return true;
    }
}
