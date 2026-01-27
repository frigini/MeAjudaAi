namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes relacionadas ao sistema de autorização
/// </summary>
/// <remarks>
/// Baseado nos valores realmente utilizados no projeto.
/// Evita duplicação com UserRoles.cs existente.
/// </remarks>
public static class AuthConstants
{
    /// <summary>
    /// Nomes das políticas de autorização (baseadas no código existente)
    /// </summary>
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string SelfOrAdmin = "SelfOrAdmin";
        public const string AuthenticatedUser = "AuthenticatedUser";
        public const string SuperAdminOnly = "SuperAdminOnly";
    }

    /// <summary>
    /// Nomes dos claims JWT/OIDC padrão
    /// </summary>
    public static class Claims
    {
        // Claims padrão JWT/OIDC
        public const string Subject = "sub";              // ID do usuário
        public const string Email = "email";
        public const string EmailVerified = "email_verified";
        public const string PreferredUsername = "preferred_username";
        public const string GivenName = "given_name";     // Primeiro nome
        public const string FamilyName = "family_name";   // Sobrenome
        public const string Roles = "roles";              // Array de roles

        // Claims customizados (se necessário)
        public const string UserId = "user_id";
        public const string KeycloakId = "keycloak_id";
        public const string Permission = "permission";
        public const string Module = "module";
        public const string TenantId = "tenant_id";
        public const string Organization = "organization";
        public const string IsSystemAdmin = "is_system_admin";
    }

    /// <summary>
    /// Headers HTTP relacionados à autenticação
    /// </summary>
    public static class Headers
    {
        public const string Authorization = "Authorization";
        public const string Bearer = "Bearer";
        public const string RequestId = "X-Request-Id";
        public const string CorrelationId = "X-Correlation-Id";
        public const string DebugAuthFailure = "X-Debug-Auth-Failure";
        
        // Headers de debug (apenas em ambientes não-produção)
        public const string DebugUser = "X-Debug-User";
        public const string DebugRoles = "X-Debug-Roles";
        public const string DebugPermissionsCount = "X-Debug-Permissions-Count";
        public const string DebugPermissions = "X-Debug-Permissions";
        public const string DebugAuthStatus = "X-Debug-Auth-Status";
    }
}
