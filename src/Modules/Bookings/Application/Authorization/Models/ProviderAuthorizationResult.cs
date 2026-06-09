using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Bookings.Application.Enums;

namespace MeAjudaAi.Modules.Bookings.Application.Authorization.Models;


[ExcludeFromCodeCoverage]
public sealed class ProviderAuthorizationResult
{
    public Guid? UserId { get; init; }
    public bool IsAdmin { get; init; }
    public Guid? ProviderId { get; init; }
    public EAuthorizationFailureKind FailureKind { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ErrorStatusCode { get; init; }

    public static ProviderAuthorizationResult Admin(Guid userId) => new() { IsAdmin = true, UserId = userId };
    public static ProviderAuthorizationResult Authorized(Guid userId, Guid providerId) => new() { UserId = userId, ProviderId = providerId };
    public static ProviderAuthorizationResult NotLinked(Guid userId) => new() { UserId = userId, FailureKind = EAuthorizationFailureKind.NotLinked };
    public static ProviderAuthorizationResult Unauthorized(string? message = null) => 
        new() { FailureKind = EAuthorizationFailureKind.Unauthorized, ErrorMessage = message };
    public static ProviderAuthorizationResult UpstreamFailure(string message, int statusCode) => 
        new() { FailureKind = EAuthorizationFailureKind.UpstreamFailure, ErrorMessage = message, ErrorStatusCode = statusCode };
}
