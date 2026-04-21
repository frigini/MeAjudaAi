using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.Queries;

[ExcludeFromCodeCoverage]

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
