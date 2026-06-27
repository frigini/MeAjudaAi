namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

/// <summary>
/// Request para atualização de categoria de serviço.
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="DisplayOrder"></param>
public sealed record UpdateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);
