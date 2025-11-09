using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Command para definir um documento como primário
/// </summary>
public sealed record SetPrimaryDocumentCommand(
    Guid ProviderId,
    EDocumentType DocumentType
) : Command<Result<ProviderDto>>;
