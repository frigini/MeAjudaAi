using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissional por ID.
/// </summary>
public sealed record GetProfessionalByIdQuery(Guid ProfessionalId) : Query<Result<ProfessionalDto?>>;
