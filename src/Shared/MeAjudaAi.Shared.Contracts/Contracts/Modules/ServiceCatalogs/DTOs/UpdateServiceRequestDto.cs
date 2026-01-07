namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para atualização de serviço.
/// </summary>
public sealed record UpdateServiceRequestDto(
    string Name,
    string? Description,
    int DisplayOrder);
