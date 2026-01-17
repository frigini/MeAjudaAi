namespace MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// DTO de localização geográfica para a API do módulo.
/// </summary>
public sealed record ModuleLocationDto(
    double Latitude,
    double Longitude);

