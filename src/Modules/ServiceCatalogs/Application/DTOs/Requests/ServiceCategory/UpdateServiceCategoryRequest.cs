namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

public record UpdateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);
