using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public class GetUserQuery : IQuery<Result<UserDto>>
{
    public Guid CorrelationId => throw new NotImplementedException();
}