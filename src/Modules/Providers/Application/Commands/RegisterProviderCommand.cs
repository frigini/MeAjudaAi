using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

[ExcludeFromCodeCoverage]

public record RegisterProviderCommand(
    Guid UserId,
    string Name,
    string Email,
    string? PhoneNumber,
    EProviderType Type,
    string DocumentNumber
) : ICommand<Result<ProviderDto>>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
