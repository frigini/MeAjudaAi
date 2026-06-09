using MeAjudaAi.Modules.Bookings.Application.Authorization.Models;
using MeAjudaAi.Modules.Bookings.Application.Enums;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Bookings.Application.Authorization;

/// <summary>
/// Métodos de extensão para converter resultados de autorização em respostas de problema (Problem Details).
/// </summary>
public static class ProviderAuthorizationResultExtensions
{
    /// <summary>
    /// Converte um resultado de autorização de prestador para uma resposta de problema (IResult).
    /// </summary>
    /// <param name="result">O resultado da autorização.</param>
    /// <returns>Um IResult contendo a resposta de erro apropriada ou null se o resultado for bem-sucedido.</returns>
    public static IResult? ToProblemResult(this ProviderAuthorizationResult result) => result.FailureKind switch
    {
        EAuthorizationFailureKind.UpstreamFailure =>
            Results.Problem(result.ErrorMessage, statusCode: result.ErrorStatusCode ?? StatusCodes.Status500InternalServerError),
        EAuthorizationFailureKind.Unauthorized =>
            Results.Problem(result.ErrorMessage ?? "Acesso não autorizado.", statusCode: StatusCodes.Status401Unauthorized),
        EAuthorizationFailureKind.NotLinked =>
            Results.Problem("Usuário não possui prestador vinculado.", statusCode: StatusCodes.Status404NotFound),
        EAuthorizationFailureKind.None => null,
        _ => null
    };
}
