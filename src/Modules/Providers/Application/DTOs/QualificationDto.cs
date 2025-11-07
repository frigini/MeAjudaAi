namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para qualificação.
/// </summary>
public sealed record QualificationDto(
    string Name,
    string? Description,
    string? IssuingOrganization,
    DateTime? IssueDate,
    DateTime? ExpirationDate,
    string? DocumentNumber
);
