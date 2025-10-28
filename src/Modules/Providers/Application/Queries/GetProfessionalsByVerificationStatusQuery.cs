using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Modules.Professionals.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Professionals.Application.Queries;

/// <summary>
/// Query para buscar profissionais por status de verificação.
/// </summary>
public sealed record GetProfessionalsByVerificationStatusQuery(VerificationStatus Status) : Query<Result<IReadOnlyList<ProfessionalDto>>>;
