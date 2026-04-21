using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;

/// <summary>
/// DTO para informações de serviço.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
