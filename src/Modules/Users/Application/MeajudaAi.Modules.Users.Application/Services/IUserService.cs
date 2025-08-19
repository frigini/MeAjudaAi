using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Services;

public interface IUserService
{
    // User Management
    Task<Result<UserDto>> RegisterUserAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ActivateUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeactivateUserAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    
    // Queries
    Task<Result<PagedResponse<IEnumerable<UserDto>>>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default);
    Task<Result<int>> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);
    
    // Role Management
    Task<Result<bool>> AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
}