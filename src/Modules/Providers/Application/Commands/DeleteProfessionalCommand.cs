using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Professionals.Application.Commands;

/// <summary>
/// Comando para exclusão lógica do profissional.
/// </summary>
public sealed record DeleteProfessionalCommand(
    Guid ProfessionalId,
    string? DeletedBy = null
) : Command<Result>;
