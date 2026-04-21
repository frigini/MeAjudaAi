using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;

/// <summary>
/// DTO para informações de categoria de serviço.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
