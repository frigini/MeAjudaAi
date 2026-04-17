using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para completar o preenchimento de informações básicas e avançar
/// para a etapa de verificação de documentos.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="UpdatedBy">Quem está executando a atualização</param>
[ExcludeFromCodeCoverage]
public sealed record CompleteBasicInfoCommand(
    Guid ProviderId,
    string? UpdatedBy = null
) : Command<Result>;
