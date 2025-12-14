using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Filters;

/// <summary>
/// Handler que captura exceções de domínio e converte para respostas HTTP adequadas.
/// </summary>
public sealed class LocationsExceptionHandler(ILogger<LocationsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problemDetails = exception switch
        {
            NotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            BadRequestException badRequestEx => HandleBadRequestException(badRequestEx),
            _ => null
        };

        if (problemDetails is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private ProblemDetails HandleNotFoundException(NotFoundException exception)
    {
        logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = exception.Message
        };
    }

    private ProblemDetails HandleBadRequestException(BadRequestException exception)
    {
        logger.LogWarning(exception, "Bad request: {Message}", exception.Message);
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad request",
            Detail = exception.Message
        };
    }
}
