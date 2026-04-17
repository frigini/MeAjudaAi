using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

[ExcludeFromCodeCoverage]

public sealed record UpdateServiceCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }
}
