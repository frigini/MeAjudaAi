using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Payments.Application.Services;

/// <summary>
/// Resolve e valida URLs de retorno para portais de faturamento.
/// Centraliza aliases (account, billing), validação HTTPS, host confiável e fallback.
/// </summary>
public interface IReturnUrlResolver
{
    /// <summary>
    /// Resolve a URL de retorno informada, aplicando aliases, validação e fallback para ClientBaseUrl.
    /// </summary>
    /// <param name="returnUrl">URL de retorno informada pelo cliente (pode ser alias ou URL absoluta).</param>
    /// <param name="providerId">ID do prestador para logs.</param>
    /// <returns>URL resolvida ou erro de configuração/validação.</returns>
    Result<string> Resolve(string? returnUrl, Guid providerId);
}
