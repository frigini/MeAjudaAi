using MeAjudaAi.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

[ExcludeFromCodeCoverage]

public sealed record CreateServiceRequest
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; } = 0;
}
