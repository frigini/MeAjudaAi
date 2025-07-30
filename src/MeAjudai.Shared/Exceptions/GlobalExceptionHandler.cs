using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace MeAjudai.Shared.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, errors, extensions) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation errors occurred",
                validationException.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()),
                new Dictionary<string, object?>()),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundException.Message,
                null,
                new Dictionary<string, object?>
                {
                    ["entityName"] = notFoundException.EntityName,
                    ["entityId"] = notFoundException.EntityId
                }),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Authentication is required to access this resource",
                null,
                []),

            ForbiddenAccessException forbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbiddenException.Message,
                null,
                []),

            BusinessRuleException businessException => (
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                businessException.Message,
                null,
                new Dictionary<string, object?>
                {
                    ["ruleName"] = businessException.RuleName
                }),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                domainException.Message,
                null,
                []),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred while processing your request",
                null,
                new Dictionary<string, object?>
                {
                    ["traceId"] = httpContext.TraceIdentifier
                })
        };

        // Log com diferentes níveis baseado no tipo de erro
        if (statusCode >= 500)
        {
            logger.LogError(exception, "Server error occurred: {ErrorType}", exception.GetType().Name);
        }
        else if (statusCode >= 400)
        {
            logger.LogWarning("Client error occurred: {ErrorType} - {Message}", exception.GetType().Name, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = GetProblemTypeUri(statusCode)
        };

        // Adicionar erros de validação
        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // Adicionar extensões específicas
        foreach (var (key, value) in extensions)
        {
            problemDetails.Extensions[key] = value;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static string GetProblemTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231"
    };
}