using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Interfaces;

public interface IKeycloakService
{
    Task<Response<string>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateUserAsync(
        string userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Response<UserDto>> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Response<IEnumerable<UserDto>>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> AssignRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> RemoveRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Response<IEnumerable<string>>> GetUserRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}