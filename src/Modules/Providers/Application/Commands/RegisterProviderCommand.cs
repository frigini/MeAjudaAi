using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

public record RegisterProviderCommand(
    Guid UserId,
    string Name,
    string Email,
    string? PhoneNumber,
    EProviderType Type,
    string DocumentNumber
) : ICommand<Result<ProviderDto>>;
