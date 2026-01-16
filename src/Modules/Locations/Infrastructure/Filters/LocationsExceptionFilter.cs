using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Filters;

/// <summary>
/// Filter que captura exceções de domínio e converte para respostas HTTP adequadas.
/// </summary>
public sealed class LocationsExceptionFilter(ILogger<LocationsExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException notFoundEx:
                logger.LogWarning(notFoundEx, "Resource not found: {Message}", notFoundEx.Message);
                context.Result = new NotFoundObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFoundEx.Message
                });
                context.ExceptionHandled = true;
                break;

            case BadRequestException badRequestEx:
                logger.LogWarning(badRequestEx, "Bad request: {Message}", badRequestEx.Message);
                context.Result = new BadRequestObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = badRequestEx.Message
                });
                context.ExceptionHandled = true;
                break;
        }
    }
}
