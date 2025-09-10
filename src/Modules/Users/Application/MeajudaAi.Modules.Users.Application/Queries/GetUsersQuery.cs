using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUsersQuery(
    int Page,
    int PageSize,
    string? SearchTerm
) : Query<Result<PagedResult<UserDto>>>;