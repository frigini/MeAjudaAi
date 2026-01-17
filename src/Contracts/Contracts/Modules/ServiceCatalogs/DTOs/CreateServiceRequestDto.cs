namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para criação de serviço.
/// </summary>
public sealed record CreateServiceRequestDto(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder);
