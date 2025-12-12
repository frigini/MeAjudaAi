using MeAjudaAi.Modules.Locations.Application.DTOs;
using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para buscar todas as cidades permitidas
/// </summary>
public sealed record GetAllAllowedCitiesQuery(bool OnlyActive = false) : IRequest<IReadOnlyList<AllowedCityDto>>;
