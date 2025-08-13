namespace MeAjudaAi.Modules.Users.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Response<AuthResponseDto>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Response<UserInfoDto>> GetUserInfoAsync(
        string token,
        CancellationToken cancellationToken = default);
}