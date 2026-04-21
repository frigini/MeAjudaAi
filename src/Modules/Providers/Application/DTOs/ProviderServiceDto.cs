using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para serviço de um prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ProviderServiceDto(
    Guid ServiceId,
    string ServiceName
);
