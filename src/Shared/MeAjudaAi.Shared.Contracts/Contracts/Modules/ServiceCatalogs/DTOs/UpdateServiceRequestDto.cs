namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para atualização de serviço.
/// Suporta atualizações parciais com campos nullable.
/// </summary>
public sealed record UpdateServiceRequestDto(
    string? Name = null,
    string? Description = null,
    int? DisplayOrder = null);
