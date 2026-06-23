using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Payments.API.Helpers;

internal static class PaymentAuthorizationHelper
{
    /// <summary>
    /// Valida se o usuário tem autorização para acessar recursos do prestador informado.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP com os claims do usuário.</param>
    /// <param name="providerId">ID do prestador alvo.</param>
    /// <returns>Null se autorizado; IResult com erro de autorização caso contrário.</returns>
    public static IResult? AuthorizeProviderAccess(HttpContext httpContext, Guid providerId)
    {
        var isSystemAdmin = string.Equals(
            httpContext.User?.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (isSystemAdmin)
        {
            return null;
        }

        var userProviderIdClaim = httpContext.User?.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        if (string.IsNullOrEmpty(userProviderIdClaim) ||
            !Guid.TryParse(userProviderIdClaim, out var userProviderId) ||
            userProviderId != providerId)
        {
            return string.IsNullOrEmpty(userProviderIdClaim)
                ? Results.Unauthorized()
                : Results.Forbid();
        }

        return null;
    }
}
