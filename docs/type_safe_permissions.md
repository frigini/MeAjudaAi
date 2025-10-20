# Sistema de Permissões Type-Safe e Modular

Este documento demonstra como usar o sistema de permissões type-safe implementado no MeAjudaAi, que suporta arquitetura modular e resolve permissões no servidor.

## Visão Geral

O sistema implementa:
- ✅ **Permissões Type-Safe**: Enum `Permission` com validação em tempo de compilação
- ✅ **Resolução Server-Side**: `IPermissionService` com cache distribuído usando HybridCache
- ✅ **Arquitetura Modular**: Cada módulo pode implementar seu próprio `IModulePermissionResolver`
- ✅ **Extensões para Endpoints**: Métodos fluentes para aplicar autorização
- ✅ **Cache Inteligente**: Cache por usuário e módulo com invalidação por tags

## Estrutura de Arquivos

```
src/
├── Shared/MeAjudai.Shared/Authorization/
│   ├── EPermission.cs                   # Enum type-safe de permissões
│   ├── CustomClaimTypes.cs             # Constantes de claim types
│   ├── PermissionExtensions.cs         # Extensões para enum EPermission
│   ├── IPermissionService.cs           # Interface do serviço de permissões
│   ├── PermissionService.cs            # Implementação modular com cache
│   ├── IModulePermissionResolver.cs    # Interface para resolvers modulares
│   ├── AuthorizationExtensions.cs      # Extensões para DI e ClaimsPrincipal
│   ├── PermissionClaimsTransformation.cs
│   ├── PermissionRequirementHandler.cs
│   └── RequirePermissionAttribute.cs
├── Modules/Users/
│   ├── Application/Authorization/
│   │   ├── UsersPermissionResolver.cs  # Resolver específico do módulo Users
│   │   └── UsersPermissions.cs         # Organizações de permissões modulares
│   └── API/
│       ├── Extensions/
│       │   └── UsersModuleExtensions.cs # Configuração do módulo
│       └── Endpoints/
│           └── UsersEndpoints.cs       # Exemplo de endpoints com permissões
```
## Como Usar

### 1. Configuração no ApiService

```
// Program.cs no ApiService
using MeAjudaAi.Modules.Users.API.Extensions;
using MeAjudaAi.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configura sistema de autorização base
builder.Services.AddPermissionBasedAuthorization();

// Registra módulos com seus resolvers de permissão
builder.Services.AddUsersModule(builder.Configuration);
// builder.Services.AddProvidersModule(builder.Configuration); // Futuros módulos
// builder.Services.AddOrdersModule(builder.Configuration);

var app = builder.Build();

// Mapeia endpoints dos módulos
app.MapUsersEndpoints();

app.Run();
```

### 2. Criando Permissões Type-Safe

```
// As permissões são organizadas por módulo no enum EPermission
public enum EPermission
{
    // Sistema
    [Display(Name = "admin:system", Description = "Administração do sistema")]
    AdminSystem,
    
    // Users Module
    [Display(Name = "users:read", Description = "Visualizar usuários")]
    UsersRead,
    
    [Display(Name = "users:create", Description = "Criar usuários")]
    UsersCreate,
    
    // Providers Module (futuro)
    [Display(Name = "providers:read", Description = "Visualizar prestadores")]
    ProvidersRead,
}
```

### 3. Implementando Resolver Modular

```
// Cada módulo implementa seu próprio resolver
public sealed class UsersPermissionResolver : IModulePermissionResolver
{
    public string ModuleName => "Users";
    
    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        // Lógica específica do módulo para mapear roles/contexto em permissões
        var userRoles = await GetUserRolesAsync(userId, cancellationToken);
        var permissions = new List<EPermission>();
        
        foreach (var role in userRoles)
        {
            var rolePermissions = MapRoleToUserPermissions(role);
            permissions.AddRange(rolePermissions);
        }
        
        return permissions.Distinct().ToList();
    }
    
    public bool CanResolve(EPermission permission)
    {
        return permission.GetModule().Equals("Users", StringComparison.OrdinalIgnoreCase);
    }
    
    private static IEnumerable<EPermission> MapRoleToUserPermissions(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "system-admin" => UsersPermissions.SystemAdmin,
            "user-admin" => UsersPermissions.UserAdmin,
            "basic-user" => UsersPermissions.BasicUser,
            _ => Array.Empty<EPermission>()
        };
    }
}
```
### 4. Organizando Permissões por Módulo

```
// UsersPermissions.cs - organiza permissões por categoria e role
public static class UsersPermissions
{
    // Categorias de permissões
    public static class Read
    {
        public static readonly EPermission[] All = { EPermission.UsersRead, EPermission.UsersList };
        public static readonly EPermission Profile = EPermission.UsersProfile;
    }
    
    public static class Write
    {
        public static readonly EPermission[] All = { EPermission.UsersCreate, EPermission.UsersUpdate };
        public static readonly EPermission Create = EPermission.UsersCreate;
        public static readonly EPermission Update = EPermission.UsersUpdate;
    }
    
    public static class Admin
    {
        public static readonly EPermission[] All = { EPermission.AdminUsers, EPermission.UsersDelete };
        public static readonly EPermission Delete = EPermission.UsersDelete;
        public static readonly EPermission Management = EPermission.AdminUsers;
    }
    
    // Permissões por tipo de usuário
    public static readonly EPermission[] BasicUser = { EPermission.UsersProfile, EPermission.UsersRead };
    public static readonly EPermission[] UserAdmin = { EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersList };
    public static readonly EPermission[] SystemAdmin = { EPermission.AdminSystem, EPermission.AdminUsers, EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersDelete, EPermission.UsersList };
}
```
### 5. Aplicando Permissões em Endpoints

```
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");
        
        // Exemplo 1: Permissão única
        group.MapGet("/", GetUsersAsync)
            .RequirePermission(EPermission.UsersList)
            .RequireAuthorization();
        
        // Exemplo 2: Múltiplas permissões (todas obrigatórias)
        group.MapDelete("/{id:guid}", DeleteUserAsync)
            .RequirePermissions(EPermission.UsersDelete, EPermission.AdminUsers)
            .RequireAuthorization();
        
        // Exemplo 3: Qualquer uma das permissões
        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .RequireAnyPermission(EPermission.UsersRead, EPermission.AdminUsers)
            .RequireAuthorization();
        
        // Exemplo 4: Permissões específicas de módulo
        group.MapGet("/admin", GetAllUsersAdminAsync)
            .RequireModulePermission("Users", EPermission.AdminUsers, EPermission.UsersList)
            .RequireAuthorization();
        
        // Exemplo 5: Admin do sistema
        group.MapPost("/system/reset", ResetSystemUsersAsync)
            .RequireSystemAdmin()
            .RequireAuthorization();
        
        return endpoints;
    }
}
```
### 6. Verificação Server-Side nos Handlers

```
private static async Task<IResult> GetUsersAsync(
    HttpContext context,
    IPermissionService permissionService)
{
    var userId = context.User.FindFirst("sub")?.Value;
    
    // Verificação server-side adicional (redundante mas segura)
    if (!string.IsNullOrEmpty(userId) && 
        await permissionService.HasPermissionAsync(userId, EPermission.UsersList))
    {
        // Lógica do endpoint
        return Results.Ok(new { message = "Lista de usuários" });
    }
    
    return Results.Forbid();
}

// Verificação de múltiplas permissões
private static async Task<IResult> DeleteUserAsync(
    Guid id,
    HttpContext context,
    IPermissionService permissionService)
{
    var userId = context.User.FindFirst("sub")?.Value;
    
    if (!string.IsNullOrEmpty(userId) && 
        await permissionService.HasPermissionsAsync(userId, new[] { EPermission.UsersDelete, EPermission.AdminUsers }))
    {
        // Lógica de remoção
        return Results.Ok(new { id, message = "Usuário removido" });
    }
    
    return Results.Forbid();
}

// Verificação de permissões por módulo
private static async Task<IResult> GetAllUsersAdminAsync(
    HttpContext context,
    IPermissionService permissionService)
{
    var userId = context.User.FindFirst("sub")?.Value;
    
    // Obtém permissões específicas do módulo Users
    var userPermissions = await permissionService.GetUserPermissionsByModuleAsync(userId ?? "", "Users");
    
    if (userPermissions.Contains(EPermission.AdminUsers) && userPermissions.Contains(EPermission.UsersList))
    {
        return Results.Ok(new { message = "Lista administrativa", permissions = userPermissions.Count });
    }
    
    return Results.Forbid();
}
```
### 7. Extensões para ClaimsPrincipal

```
// Verificações no lado do cliente/view
public class SomeController : ControllerBase
{
    public IActionResult SomeAction()
    {
        // Verificação direta no ClaimsPrincipal
        if (User.HasPermission(EPermission.UsersRead))
        {
            // Usuário tem permissão
        }
        
        if (User.HasPermissions(EPermission.UsersRead, EPermission.UsersList))
        {
            // Usuário tem todas as permissões
        }
        
        if (User.HasAnyPermission(EPermission.UsersRead, EPermission.AdminUsers))
        {
            // Usuário tem pelo menos uma das permissões
        }
        
        if (User.IsSystemAdmin())
        {
            // Usuário é admin do sistema
        }
        
        var tenantId = User.GetTenantId();
        var orgId = User.GetOrganizationId();
        var userPermissions = User.GetPermissions();
        
        return Ok();
    }
}
```
## Performance e Cache

O sistema usa cache em múltiplas camadas:

```
// Cache por usuário (30 minutos)
var permissions = await permissionService.GetUserPermissionsAsync(userId);

// Cache por usuário + módulo (30 minutos)
var modulePermissions = await permissionService.GetUserPermissionsByModuleAsync(userId, "Users");

// Invalidação de cache
await permissionService.InvalidateUserPermissionsCacheAsync(userId);
```
**Características do Cache:**
- Cache distribuído usando HybridCache (já disponível no Aspire)
- Cache local (5 minutos) + cache distribuído (30 minutos)
- Invalidação baseada em tags (`user:{userId}`, `module:{module}`)
- Otimização automática para consultas frequentes

## Extensibilidade para Novos Módulos

Para adicionar um novo módulo (ex: Providers):

### 1. Adicionar permissões no enum:
```
public enum EPermission
{
    // Existing permissions...
    
    // Providers Module
    [Display(Name = "providers:read", Description = "Visualizar prestadores")]
    ProvidersRead,
    
    [Display(Name = "providers:create", Description = "Criar prestadores")]
    ProvidersCreate,
}
```
### 2. Criar resolver do módulo:
```
public sealed class ProvidersPermissionResolver : IModulePermissionResolver
{
    public string ModuleName => "Providers";
    
    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Lógica específica do módulo Providers
    }
    
    public bool CanResolve(EPermission permission)
    {
        return permission.GetModule().Equals("Providers", StringComparison.OrdinalIgnoreCase);
    }
}
```
### 3. Registrar no DI:
```
// ProvidersModuleExtensions.cs
public static IServiceCollection AddProvidersModule(this IServiceCollection services, IConfiguration configuration)
{
    services.AddModulePermissionResolver<ProvidersPermissionResolver>();
    return services;
}

// Program.cs
builder.Services.AddProvidersModule(builder.Configuration);
```
### 4. Criar endpoints com permissões:
```
group.MapGet("/", GetProvidersAsync)
    .RequirePermission(EPermission.ProvidersRead)
    .RequireAuthorization();
```
## Vantagens do Sistema

1. **Type-Safety**: Permissões são validadas em tempo de compilação
2. **Performance**: Cache distribuído com invalidação inteligente
3. **Modularidade**: Cada módulo gerencia suas próprias permissões
4. **Extensibilidade**: Fácil adição de novos módulos e permissões
5. **Segurança**: Verificação server-side redundante
6. **Manutenibilidade**: Organizações claras e documentadas
7. **Integração**: Funciona nativamente com Aspire e Keycloak

O sistema está pronto para uso e pode ser facilmente estendido conforme novos módulos são adicionados ao MeAjudaAi!
