using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Bookings.Application.Common;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public static class ProviderAuthorizationResultExtensions
{
    public static IResult? ToProblemResult(this ProviderAuthorizationResult result)
    {
        return result.FailureKind switch
        {
            AuthorizationFailureKind.UpstreamFailure => 
                Results.Problem(result.ErrorMessage, statusCode: result.ErrorStatusCode ?? StatusCodes.Status500InternalServerError),
            AuthorizationFailureKind.Unauthorized => 
                Results.Problem(result.ErrorMessage ?? "Acesso não autorizado.", statusCode: StatusCodes.Status401Unauthorized),
            AuthorizationFailureKind.NotLinked => 
                Results.Problem("Usuário não possui prestador vinculado.", statusCode: StatusCodes.Status404NotFound),
            _ => null
        };
    }
}
