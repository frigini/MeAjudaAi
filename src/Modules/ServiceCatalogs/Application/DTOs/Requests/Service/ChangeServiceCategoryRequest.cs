using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

/// <summary>
/// Request para alteração da categoria de um serviço.
/// </summary>
[ExcludeFromCodeCoverage]

public sealed record ChangeServiceCategoryRequest
{
    public Guid NewCategoryId { get; init; }
}
