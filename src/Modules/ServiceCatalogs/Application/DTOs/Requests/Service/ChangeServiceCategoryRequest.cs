using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

public sealed record ChangeServiceCategoryRequest : Request
{
    public Guid NewCategoryId { get; init; }
}
