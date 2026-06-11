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

        return Results.Problem(detail: error.Message, statusCode: statusCode);
    }
}
