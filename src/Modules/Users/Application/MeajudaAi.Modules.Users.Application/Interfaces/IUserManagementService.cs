using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Interfaces;

public interface IUserManagementService
{
    Task<Result<IEnumerable<UserDto>>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<int>> GetTotalUsersCountAsync(
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}