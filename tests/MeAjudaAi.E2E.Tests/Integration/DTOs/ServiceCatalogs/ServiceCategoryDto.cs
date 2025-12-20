namespace MeAjudaAi.E2E.Tests.Integration.DTOs.ServiceCatalogs;

/// <summary>
/// DTO para representar categoria de serviço nos testes de integração.
/// </summary>
internal record ServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
