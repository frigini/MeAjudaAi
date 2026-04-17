using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para ativar uma categoria de serviço.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;
