namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para alterar a categoria de um serviço.
/// </summary>
public sealed record ChangeServiceCategoryRequestDto(Guid NewCategoryId);
