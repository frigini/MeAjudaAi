namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para atualização de serviço.
/// Requer todos os campos (full-update pattern).
/// Para alterar a categoria, use o endpoint ChangeServiceCategory.
/// </summary>
public sealed record UpdateServiceRequestDto(
    string Name,
    string? Description = null,
    int DisplayOrder = 0);
