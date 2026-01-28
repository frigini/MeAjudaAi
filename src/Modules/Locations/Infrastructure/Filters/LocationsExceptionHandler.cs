using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Exceptions;
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
            Shared.Exceptions.NotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            DuplicateAllowedCityException duplicateEx => HandleDuplicateException(duplicateEx),
            ArgumentException argumentEx => HandleArgumentException(argumentEx),
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

    private ProblemDetails HandleNotFoundException(Shared.Exceptions.NotFoundException exception)
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

    private ProblemDetails HandleDuplicateException(DuplicateAllowedCityException exception)
    {
        logger.LogWarning(exception, "Duplicate resource: {Message}", exception.Message);
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Duplicate resource",
            Detail = exception.Message
        };
    }

    private ProblemDetails HandleArgumentException(ArgumentException exception)
    {
        logger.LogWarning(exception, "Invalid argument: {Message}", exception.Message);
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid argument",
            Detail = exception.Message
        };
    }
}
