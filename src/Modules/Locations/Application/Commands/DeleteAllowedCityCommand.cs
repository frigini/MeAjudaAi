using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Command para deletar cidade permitida
/// </summary>
public sealed record DeleteAllowedCityCommand(Guid Id) : IRequest<Unit>;
