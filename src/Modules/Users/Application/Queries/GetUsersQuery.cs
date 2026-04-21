using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

using MeAjudaAi.Contracts.Models;
using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Users.Application.Queries;

[ExcludeFromCodeCoverage]

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
