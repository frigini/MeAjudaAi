using MeAjudaAi.Modules.Locations.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Locations.Application.Services;

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
