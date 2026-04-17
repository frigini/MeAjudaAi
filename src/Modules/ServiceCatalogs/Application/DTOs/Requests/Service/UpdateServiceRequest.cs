using MeAjudaAi.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

/// <summary>
/// Request to update an existing service's information.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UpdateServiceRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
}
