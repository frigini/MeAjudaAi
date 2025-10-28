using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissionais por cidade.
/// </summary>
public sealed record GetProfessionalsByCityQuery(string City) : Query<Result<IReadOnlyList<ProfessionalDto>>>;
