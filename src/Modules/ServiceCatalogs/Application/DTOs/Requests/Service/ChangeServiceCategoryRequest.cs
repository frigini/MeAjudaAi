using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

public sealed record ChangeServiceCategoryRequest
{
    public Guid NewCategoryId { get; init; }
}
