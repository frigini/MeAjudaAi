namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para atualização de serviço.
/// Suporta atualizações parciais com campos nullable.
/// </summary>
public sealed record UpdateServiceRequestDto(
    Guid? CategoryId = null,
    string? Name = null,
    string? Description = null,
    int? DisplayOrder = null);
