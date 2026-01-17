using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para adição de qualificação ao prestador de serviços.
/// </summary>
public sealed record AddQualificationCommand(
    Guid ProviderId,
    string Name,
    string? Description,
    string? IssuingOrganization,
    DateTime? IssueDate,
    DateTime? ExpirationDate,
    string? DocumentNumber
) : Command<Result<ProviderDto>>;
