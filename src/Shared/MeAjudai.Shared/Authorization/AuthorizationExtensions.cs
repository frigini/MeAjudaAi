using MeAjudaAi.Shared.Authorization.HealthChecks;
using MeAjudaAi.Shared.Authorization.Keycloak;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Authorization.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Extensions para configurar o sistema de autorização baseado em permissões.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Configura o sistema de autorização com permissões type-safe.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration for Keycloak integration</param>
    /// <returns>Service collection para chaining</returns>
    public static IServiceCollection AddPermissionBasedAuthorization(
        this IServiceCollection services, 
        IConfiguration? configuration = null)
    {
        // Registra serviços de permissão core
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        // Adiciona métricas e monitoramento
        services.AddPermissionMetrics();
        
        // Adiciona health checks
        services.AddPermissionSystemHealthCheck();
        
        // Adiciona integração com Keycloak se configuração estiver disponível
        if (configuration != null)
        {
            services.AddKeycloakPermissionResolver(configuration);
        }
        
        // Configura políticas de autorização
        services.AddAuthorization(options =>
        {
            // Registra políticas para cada permissão (EPermissions)
            foreach (EPermission permission in Enum.GetValues<EPermission>())
            {
                var policyName = $"RequirePermission:{permission.GetValue()}";
                options.AddPolicy(policyName, policy =>
                {
                    policy.Requirements.Add(new PermissionRequirement(permission));
                });
            }
        });
        
        return services;
    }
    
    /// <summary>
    /// Adiciona resolução de permissões via Keycloak.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration para Keycloak</param>
    /// <returns>Service collection para chaining</returns>
    public static IServiceCollection AddKeycloakPermissionResolver(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        // Registra o resolvedor de permissões do Keycloak
        services.AddScoped<IKeycloakPermissionResolver, KeycloakPermissionResolver>();
        
        // Configura opções do Keycloak a partir da configuração
        services.Configure<KeycloakPermissionOptions>(
            configuration.GetSection("Keycloak"));
        
        return services;
    }
    
    /// <summary>
    /// Adiciona middleware de autorização para a aplicação.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder para chaining</returns>
    public static IApplicationBuilder UsePermissionBasedAuthorization(this IApplicationBuilder app)
    {
        // Middleware de otimização deve vir antes da autenticação
        app.UsePermissionOptimization();
        
        return app;
    }
    
    /// <summary>
    /// Adiciona um resolver de permissões específico de um módulo.
    /// </summary>
    public static IServiceCollection AddModulePermissionResolver<T>(this IServiceCollection services)
        where T : class, IModulePermissionResolver
    {
        services.AddScoped<IModulePermissionResolver, T>();
        return services;
    }
    
    /// <summary>
    /// Verifica se um ClaimsPrincipal possui uma permissão específica.
    /// </summary>
    /// <param name="user">O usuário</param>
    /// <param name="permission">A permissão a verificar</param>
    /// <returns>True se o usuário possui a permissão</returns>
    public static bool HasPermission(this ClaimsPrincipal user, EPermission permission)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.HasClaim(CustomClaimTypes.Permission, permission.GetValue());
    }
    
    /// <summary>
    /// Verifica se um ClaimsPrincipal possui múltiplas permissões.
    /// </summary>
    /// <param name="user">O usuário</param>
    /// <param name="permissions">As permissões a verificar</param>
    /// <param name="requireAll">Se true, requer todas as permissões; se false, requer ao menos uma</param>
    /// <returns>True se o usuário atende aos critérios</returns>
    public static bool HasPermissions(this ClaimsPrincipal user, IEnumerable<EPermission> permissions, bool requireAll = true)
    {
        ArgumentNullException.ThrowIfNull(user);
        var permissionsList = permissions.ToList();
        
        return permissionsList.Count == 0 || (requireAll
            ? permissionsList.All(user.HasPermission)
            : permissionsList.Any(user.HasPermission));
    }
    
    /// <summary>
    /// Verifica se um ClaimsPrincipal é administrador do sistema.
    /// </summary>
    /// <param name="user">O usuário</param>
    /// <returns>True se o usuário é admin</returns>
    public static bool IsSystemAdmin(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.HasClaim(CustomClaimTypes.IsSystemAdmin, "true");
    }
    
    /// <summary>
    /// Obtém todas as permissões de um ClaimsPrincipal.
    /// </summary>
    /// <param name="user">O usuário</param>
    /// <returns>Lista de permissões</returns>
    public static IEnumerable<EPermission> GetPermissions(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var permissionClaims = user.FindAll(CustomClaimTypes.Permission)
            .Where(c => c.Value != "*") // Exclui o marcador de processamento
            .Select(c => PermissionExtensions.FromValue(c.Value))
            .Where(p => p.HasValue)
            .Select(p => p!.Value);
            
        return permissionClaims;
    }
    
    /// <summary>
    /// Extension method para adicionar autorização baseada em permissão a endpoints.
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <param name="permission">Permissão necessária</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, EPermission permission)
        where TBuilder : IEndpointConventionBuilder
    {
        var policyName = $"RequirePermission:{permission.GetValue()}";
        return builder.RequireAuthorization(policyName);
    }
    
    /// <summary>
    /// Extension method para autorização Admin ou Self (para endpoints de usuário).
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequireSelfOrAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization("SelfOrAdmin");
    }
    
    /// <summary>
    /// Extension method para autorização Admin.
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequireAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization("AdminOnly");
    }
    
    /// <summary>
    /// Extension method para autorização Super Admin.
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequireSuperAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization("SuperAdminOnly");
    }
    
    /// <summary>
    /// Extension method para adicionar múltiplas permissões (require ALL).
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <param name="permissions">Permissões necessárias</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params EPermission[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(permissions);
        foreach (var permission in permissions)
        {
            builder.RequirePermission(permission);
        }
        return builder;
    }
    
    /// <summary>
    /// Extension method para adicionar permissão por módulo.
    /// </summary>
    /// <param name="builder">Route handler builder</param>
    /// <param name="module">Nome do módulo</param>
    /// <param name="action">Ação (read, write, delete, etc.)</param>
    /// <returns>Route handler builder para chaining</returns>
    public static TBuilder RequireModulePermission<TBuilder>(this TBuilder builder, string module, string action)
        where TBuilder : IEndpointConventionBuilder
    {
        var permissionValue = $"{module}:{action}";
        var permission = PermissionExtensions.FromValue(permissionValue);
        
        if (permission.HasValue)
        {
            return builder.RequirePermission(permission.Value);
        }
        
        // Fallback para política dinâmica se permissão não existir no enum
        var policyName = $"RequirePermission:{permissionValue}";
        return builder.RequireAuthorization(policyName);
    }
}