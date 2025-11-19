using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

/// <summary>
/// Request to update an existing service's information.
/// </summary>
public sealed record UpdateServiceRequest : Request
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
}
