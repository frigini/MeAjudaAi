using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Define tipos de claims customizados utilizados no sistema de autenticação/autorização.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// Claim type para permissões específicas do usuário.
    /// </summary>
    public const string Permission = AuthConstants.Claims.Permission;
    
    /// <summary>
    /// Claim type para o módulo ao qual uma permissão pertence.
    /// </summary>
    public const string Module = AuthConstants.Claims.Module;
    
    /// <summary>
    /// Claim type para o ID do tenant (para futuro suporte multi-tenant).
    /// </summary>
    public const string TenantId = AuthConstants.Claims.TenantId;
    
    /// <summary>
    /// Claim type para o contexto de organização do usuário.
    /// </summary>
    public const string Organization = AuthConstants.Claims.Organization;
    
    /// <summary>
    /// Claim type para indicar se o usuário é administrador do sistema.
    /// </summary>
    public const string IsSystemAdmin = AuthConstants.Claims.IsSystemAdmin;
}