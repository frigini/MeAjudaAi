using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissionais por estado.
/// </summary>
public sealed record GetProfessionalsByStateQuery(string State) : Query<Result<IReadOnlyList<ProfessionalDto>>>;
