using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para desativar um serviço, removendo-o do uso ativo.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeactivateServiceCommand(Guid Id) : Command<Result>;
