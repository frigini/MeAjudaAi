using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Authorization.Middleware;

[ExcludeFromCodeCoverage]

public static class PermissionOptimizationConstants
{
    public const string ExpectedPermissions = "ExpectedPermissions";
    public const string UseAggressivePermissionCache = "UseAggressivePermissionCache";
    public const string PermissionCacheDuration = "PermissionCacheDuration";
}
