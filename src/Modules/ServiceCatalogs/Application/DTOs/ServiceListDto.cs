using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;

/// <summary>
/// DTO simplificado para serviço sem detalhes de categoria (para listas).
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive
);
