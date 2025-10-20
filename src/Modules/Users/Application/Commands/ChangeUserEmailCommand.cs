using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para alteração do email do usuário com validações de segurança.
/// Operação crítica que pode requerer verificação adicional.
/// </summary>
public sealed record ChangeUserEmailCommand(
    Guid UserId,
    string NewEmail,
    string? UpdatedBy = null,
    bool RequireVerification = true
) : Command<Result<UserDto>>;
