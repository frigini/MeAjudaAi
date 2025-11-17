using MeAjudaAi.Modules.Location.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Location.Application.Services;

/// <summary>
/// Provedor de CEP para fallback chain.
/// </summary>
public enum ECepProvider
{
    ViaCep,
    BrasilApi,
    OpenCep
}

/// <summary>
/// Serviço de consulta de CEP com fallback automático entre provedores.
/// </summary>
public interface ICepLookupService
{
    /// <summary>
    /// Busca um endereço pelo CEP usando chain of responsibility.
    /// Ordem de fallback: ViaCEP → BrasilAPI → OpenCEP
    /// </summary>
    Task<Address?> LookupAsync(Cep cep, CancellationToken cancellationToken = default);
}
