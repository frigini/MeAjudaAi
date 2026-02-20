namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para serviço de um prestador.
/// </summary>
public sealed record ProviderServiceDto(
    Guid ServiceId,
    string ServiceName
);
