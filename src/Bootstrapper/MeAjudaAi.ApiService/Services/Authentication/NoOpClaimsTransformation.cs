using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MeAjudaAi.ApiService.Services.Authentication;

/// <summary>
/// Implementação no-op de IClaimsTransformation para casos onde transformação mínima é necessária.
/// Usada em ambientes de teste onde não há necessidade de transformações de claims.
/// </summary>
public class NoOpClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        return Task.FromResult(principal);
    }
}
