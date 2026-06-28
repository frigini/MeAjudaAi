namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para validação de serviços.
/// </summary>
public sealed record ValidateServicesRequestDto(IReadOnlyCollection<Guid> ServiceIds);
