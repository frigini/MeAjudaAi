using MeAjudaAi.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

[ExcludeFromCodeCoverage]

public sealed record ChangeServiceCategoryRequest
{
    public Guid NewCategoryId { get; init; }
}
