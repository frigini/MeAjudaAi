using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Modules.Professionals.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissionais por tipo.
/// </summary>
public sealed record GetProfessionalsByTypeQuery(ProfessionalType Type) : Query<Result<IReadOnlyList<ProfessionalDto>>>;
