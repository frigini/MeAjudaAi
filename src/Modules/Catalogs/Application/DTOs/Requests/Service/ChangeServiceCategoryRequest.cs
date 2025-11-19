using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests.Service;

public sealed record ChangeServiceCategoryRequest : Request
{
    public Guid NewCategoryId { get; init; }
}
