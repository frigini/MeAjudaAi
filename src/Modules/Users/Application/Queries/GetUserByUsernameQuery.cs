using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUserByUsernameQuery(string Username) : Query<Result<UserDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"user:username:{Username.ToLowerInvariant()}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 15 minutos para busca por username
        return TimeSpan.FromMinutes(15);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["users", $"user-username:{Username.ToLowerInvariant()}"];
    }
}
