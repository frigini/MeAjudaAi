using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissional por ID do usuário.
/// </summary>
public sealed record GetProfessionalByUserIdQuery(Guid UserId) : Query<Result<ProfessionalDto?>>;
