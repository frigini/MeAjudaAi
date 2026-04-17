using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para remoção de qualificação do prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record RemoveQualificationCommand(
    Guid ProviderId,
    string QualificationName
) : Command<Result<ProviderDto>>;
