using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para desativar um serviço, removendo-o do uso ativo.
/// </summary>
public sealed record DeactivateServiceCommand(Guid Id) : Command<Result>;
