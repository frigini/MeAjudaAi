using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

public sealed class GetUsersQueryHandler(
    IUserRepository userRepository
) : IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    public async Task<Result<PagedResult<UserDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (users, totalCount) = await userRepository.GetPagedAsync(
                query.Page, query.PageSize, cancellationToken);

            var userDtos = users.Select(u => u.ToDto()).ToList().AsReadOnly();

            var pagedResult = PagedResult<UserDto>.Create(
                userDtos, query.Page, query.PageSize, totalCount);

            return Result<PagedResult<UserDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<UserDto>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}