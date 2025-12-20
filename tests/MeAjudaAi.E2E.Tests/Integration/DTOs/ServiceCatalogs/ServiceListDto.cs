namespace MeAjudaAi.E2E.Tests.Integration.DTOs.ServiceCatalogs;

/// <summary>
/// DTO para representar serviço em listagens nos testes de integração.
/// </summary>
internal record ServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
);
