namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// DTO para informações de serviço exposto para outros módulos.
/// </summary>
public sealed record ModuleServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    bool IsActive
);

