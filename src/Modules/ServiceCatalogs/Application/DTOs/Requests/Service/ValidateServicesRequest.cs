using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

public sealed record ValidateServicesRequest : Request
{
    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
}
