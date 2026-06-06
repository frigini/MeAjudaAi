using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.Queries;

[ExcludeFromCodeCoverage]

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

    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Users, CacheTags.UserEmailTag(Email)];
}
