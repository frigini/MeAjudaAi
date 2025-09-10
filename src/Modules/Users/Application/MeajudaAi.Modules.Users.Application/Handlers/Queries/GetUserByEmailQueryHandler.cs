using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

public sealed class GetUserByEmailQueryHandler(
    IUserRepository userRepository
) : IQueryHandler<GetUserByEmailQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(
            new Email(query.Email), cancellationToken);

        return user == null
            ? Result<UserDto>.Failure("User not found")
            : Result<UserDto>.Success(user.ToDto());
    }
}