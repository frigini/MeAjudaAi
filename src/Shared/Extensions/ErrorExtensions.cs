using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblem(this Error error)
    {
        var statusCode = error.StatusCode is >= 400 and <= 599
            ? error.StatusCode
            : StatusCodes.Status500InternalServerError;

        var type = statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        return Results.Problem(
            detail: error.Message,
            title: error.Message,
            type: type,
            statusCode: statusCode);
    }
}
