using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Modules.Users.Application.DTOs;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public class GetUserProfileQuery : IQuery<Result<UserProfileDto>>
{
    public Guid UserId { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    public GetUserProfileQuery(Guid userId)
    {
        UserId = userId;
    }
}
