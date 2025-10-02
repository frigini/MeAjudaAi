using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUserByIdQuery(Guid UserId) : Query<Result<UserDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"user:id:{UserId}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 15 minutos para usuários individuais
        return TimeSpan.FromMinutes(15);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["users", $"user:{UserId}"];
    }
}