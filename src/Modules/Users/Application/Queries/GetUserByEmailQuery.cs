using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUserByEmailQuery(string Email) : Query<Result<UserDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"user:email:{Email.ToLowerInvariant()}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 15 minutos para busca por email
        return TimeSpan.FromMinutes(15);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["users", $"user-email:{Email.ToLowerInvariant()}"];
    }
}
