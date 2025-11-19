using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

public sealed record UpdateServiceCategoryRequest : Request
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }
}
