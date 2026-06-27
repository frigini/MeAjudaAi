using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

/// <summary>
/// Request para validação de serviços.
/// </summary>
[ExcludeFromCodeCoverage]

public sealed record ValidateServicesRequest
{
    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
}
