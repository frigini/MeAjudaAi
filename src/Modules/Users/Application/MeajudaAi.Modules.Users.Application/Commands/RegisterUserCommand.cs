using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public class RegisterUserCommand : ICommand<Result<UserId>>
{
    // Herdar de ICommand do Shared
    public Guid CorrelationId => throw new NotImplementedException();
}