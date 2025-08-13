using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.DTOs.Responses;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Result<UserInfoDto>> GetUserInfoAsync(
        string token,
        CancellationToken cancellationToken = default);
}