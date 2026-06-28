using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

/// <summary>
/// Request para criação de uma nova categoria de serviço.
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="DisplayOrder"></param>
[ExcludeFromCodeCoverage]
public sealed record CreateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);
