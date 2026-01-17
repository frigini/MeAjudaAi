using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para ativar um serviço, tornando-o disponível para uso.
/// </summary>
public sealed record ActivateServiceCommand(Guid Id) : Command<Result>;
