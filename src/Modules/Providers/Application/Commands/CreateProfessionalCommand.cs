using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Professionals.Application.Commands;

/// <summary>
/// Comando para criação de um novo profissional no sistema.
/// </summary>
public sealed record CreateProfessionalCommand(
    Guid UserId,
    string Name,
    ProfessionalType Type,
    BusinessProfileDto BusinessProfile
) : Command<Result<ProfessionalDto>>;
