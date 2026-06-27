namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

/// <summary>
/// Request para obtenção de todas as categorias de serviço, com opção de filtrar apenas as ativas.
/// </summary>
/// <param name="ActiveOnly"></param>
public sealed record GetAllServiceCategoriesRequest(bool ActiveOnly = false);
