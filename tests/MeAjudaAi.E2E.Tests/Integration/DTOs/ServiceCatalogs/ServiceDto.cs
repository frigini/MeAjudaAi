namespace MeAjudaAi.E2E.Tests.Integration.DTOs.ServiceCatalogs;

/// <summary>
/// DTO para representar serviço com detalhes completos nos testes de integração.
/// </summary>
internal record ServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
