using MeAjudaAi.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

[ExcludeFromCodeCoverage]

public sealed record ValidateServicesRequest
{
    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
}
