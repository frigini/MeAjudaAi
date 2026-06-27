namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

public record CreateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);
