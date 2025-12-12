using MeAjudaAi.Modules.Locations.Application.DTOs;
using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para buscar cidade permitida por ID
/// </summary>
public sealed record GetAllowedCityByIdQuery(Guid Id) : IRequest<AllowedCityDto?>;
