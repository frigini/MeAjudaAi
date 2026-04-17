using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

[ExcludeFromCodeCoverage]

public sealed record DeleteServiceCategoryCommand(Guid Id) : Command<Result>;
