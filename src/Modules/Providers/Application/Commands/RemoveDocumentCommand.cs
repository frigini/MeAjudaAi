using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para remoção de documento do prestador de serviços.
/// </summary>
public sealed record RemoveDocumentCommand(
    Guid ProviderId,
    EDocumentType DocumentType
) : Command<Result<ProviderDto>>;
