using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para ativar um prestador de serviços após verificação bem-sucedida de documentos.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="ActivatedBy">Quem está executando a ativação</param>
public sealed record ActivateProviderCommand(
    Guid ProviderId,
    string? ActivatedBy = null
) : Command<Result>;
