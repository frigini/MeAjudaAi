namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// DTO simplificado de serviço para operações de listagem.
/// </summary>
public sealed record ModuleServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    bool IsActive
);

