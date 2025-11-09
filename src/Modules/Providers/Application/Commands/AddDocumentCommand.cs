using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para adição de documento ao prestador de serviços.
/// </summary>
public sealed record AddDocumentCommand(
    Guid ProviderId,
    string DocumentNumber,
    EDocumentType DocumentType
) : Command<Result<ProviderDto>>;
