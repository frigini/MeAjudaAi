using MeAjudaAi.Modules.Professionals.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Professionals.Application.Commands;

/// <summary>
/// Comando para atualização do perfil do profissional.
/// </summary>
public sealed record UpdateProfessionalProfileCommand(
    Guid ProfessionalId,
    string Name,
    BusinessProfileDto BusinessProfile,
    string? UpdatedBy = null
) : Command<Result<ProfessionalDto>>;
