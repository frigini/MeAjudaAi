using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests;

public sealed record CreateServiceRequest : Request
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; } = 0;
}
