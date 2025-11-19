using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests.Service;

public sealed record ValidateServicesRequest : Request
{
    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
}
