using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUsersQuery(
    int Page,
    int PageSize,
    string? SearchTerm
) : Query<Result<PagedResult<UserDto>>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        var searchKey = string.IsNullOrEmpty(SearchTerm) ? "all" : SearchTerm.ToLowerInvariant();
        return $"users:page:{Page}:size:{PageSize}:search:{searchKey}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 5 minutos para listas de usuários
        return TimeSpan.FromMinutes(5);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["users", "users-list"];
    }
}