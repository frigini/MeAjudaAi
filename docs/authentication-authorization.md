# Sistema de Autenticação e Autorização

Este documento cobre o sistema completo de autenticação e autorização do MeAjudaAi, incluindo integração com Keycloak e o sistema de permissões type-safe.

## 📋 Visão Geral

MeAjudaAi utiliza um sistema robusto de autenticação e autorização com as seguintes características:

- **Autenticação**: Integração com Keycloak usando tokens JWT
- **Autorização**: Sistema type-safe baseado em enums (`EPermission`)
- **Arquitetura Modular**: Cada módulo pode implementar suas próprias regras de permissão
- **Cache Inteligente**: HybridCache para otimização de desempenho
- **Extensibilidade**: Suporte para múltiplos provedores de permissões
## 🏗️ Arquitetura do Sistema

### Componentes Principais

```text
Sistema de Autenticação & Autorização
├── Autenticação (Keycloak + JWT)
│   ├── Validação de Token JWT
│   ├── Transformação de Claims (PermissionClaimsTransformation)
│   └── Gerenciamento de Identidade do Usuário
│
└── Autorização (Permissões Type-Safe)
    ├── Enum EPermission (Type-Safe)
    ├── Serviço de Permissões (Cache + Resolução)
    ├── Resolvedores de Permissão de Módulo
    └── Handlers de Autorização
    └── Health Check do Sistema (PermissionSystemHealthCheck)
```

...

### 3. Componentes de Infraestrutura (Invisíveis)

O sistema de autorização inclui componentes que operam automaticamente no pipeline do ASP.NET Core:

#### 3.1 PermissionClaimsTransformation
Implementa a interface nativa `IClaimsTransformation`.
- **Papel:** Automaticamente intercepta cada requisição autenticada, chama o `IPermissionService` para obter as permissões do usuário e as injeta como *Claims* (`AuthConstants.Claims.Permission` e `AuthConstants.Claims.Module`) no `ClaimsPrincipal`.
- **Vantagem:** Permite o uso de atributos nativos de autorização baseados em políticas (`[Authorize(Policy = "...")]`) sem consultas manuais ao serviço em cada controller.
- **Registro:** Registrado automaticamente via `AddPermissionBasedAuthorization` em `AuthorizationExtensions`.

#### 3.2 PermissionSystemHealthCheck
Implementa `IHealthCheck`.
- **Papel:** Monitora a saúde do subsistema de permissões.
- **Verificações:** 
    - **Funcionalidade:** Testa a resolução de permissões para um usuário de teste.
    - **Performance:** Monitora tempo de resposta e taxa de acerto do cache (`CacheHitRate`).
    - **Cache:** Valida a integridade do cache distribuído.
- **Impacto:** Se o sistema de permissões falhar (ex: falha na conexão com banco/cache), o Health Check marca a API como `Unhealthy`, garantindo que o orquestrador (ex: Kubernetes) remova a instância do tráfego.

...

### Fluxo de Autorização

```mermaid
graph TD
    A[Requisição] --> B[Validação JWT]
    B --> C[Transformação de Claims]
    C --> D[Resolução de Permissões]
    D --> E[Cache de Permissões]
    E --> F{Verificação de Permissão}
    F -->|Permitir| G[Execução do Endpoint]
    F -->|Negar| H[403 Forbidden]
    
    D --> I[Resolvedores de Módulo]
    I --> J[Roles do Keycloak]
    J --> K[Mapeamento de Permissões]
```

## 🔐 Sistema de Permissões Type-Safe

O sistema é baseado em um enum type-safe (`EPermission`), arquitetura modular e resolução server-side.

### 1. Enum EPermission

Um sistema unificado de permissões type-safe:

```csharp
public enum EPermission
{
    // ===== SYSTEM - GLOBAL =====
    [Display(Name = "system:read")]
    SystemRead,
    
    [Display(Name = "system:write")]
    SystemWrite,
    
    [Display(Name = "system:admin")]
    SystemAdmin,
    
    // ===== USERS MODULE =====
    [Display(Name = "users:read")]
    UsersRead,
    
    [Display(Name = "users:create")]
    UsersCreate,
    
    [Display(Name = "users:update")]
    UsersUpdate,
    
    [Display(Name = "users:delete")]
    UsersDelete,
    
    [Display(Name = "users:list")]
    UsersList,
    
    [Display(Name = "users:profile")]
    UsersProfile,
    
    // ===== ADMIN PERMISSIONS =====
    [Display(Name = "admin:system")]
    AdminSystem,
    
    [Display(Name = "admin:users")]
    AdminUsers,
    
    [Display(Name = "admin:reports")]
    AdminReports
}
```

### 2. IPermissionService

Interface principal para resolução de permissões:

```csharp
public interface IPermissionService
{
    Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(string userId, EPermission permission, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionsAsync(string userId, IEnumerable<EPermission> permissions, bool requireAll = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EPermission>> GetUserPermissionsByModuleAsync(string userId, string moduleName, CancellationToken cancellationToken = default);
    Task InvalidateUserPermissionsCacheAsync(string userId, CancellationToken cancellationToken = default);
}
```

### 3. IModulePermissionResolver

Interface para resolução modular de permissões:

```csharp
public interface IModulePermissionResolver
{
    string ModuleName { get; }
    Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default);
    bool CanResolve(EPermission permission);
}
```

## 🚀 Implementação

### 1. Configuração Básica

```csharp
// Program.cs no ApiService
using MeAjudaAi.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configura o sistema completo de autorização
builder.Services.AddPermissionBasedAuthorization(builder.Configuration);

// Registra resolvedores de módulos específicos
builder.Services.AddModulePermissionResolver<UsersPermissionResolver>();

var app = builder.Build();

// Aplica middleware de autorização
app.UsePermissionBasedAuthorization();

app.Run();
```

### 2. Implementação de Resolvedor de Módulo

```csharp
// Modules/Users/Application/Authorization/UsersPermissionResolver.cs
public class UsersPermissionResolver : IModulePermissionResolver
{
    private readonly ILogger<UsersPermissionResolver> _logger;
    
    public UsersPermissionResolver(ILogger<UsersPermissionResolver> logger)
    {
        _logger = logger;
    }
    
    public string ModuleName => "Users";
    
    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Busca roles do usuário (exemplo simplificado)
            var userRoles = await GetUserRolesAsync(userId, cancellationToken);
            
            var permissions = new HashSet<EPermission>();
            
            foreach (var role in userRoles)
            {
                var rolePermissions = MapRoleToUserPermissions(role);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }
            
            return permissions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve permissions for user {UserId}", userId);
            return Array.Empty<EPermission>();
        }
    }
    
    public bool CanResolve(EPermission permission)
    {
        return permission.GetModule().Equals("users", StringComparison.OrdinalIgnoreCase);
    }
    
    private async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken)
    {
        // Simula busca de roles (substituir com lógica real)
        await Task.Delay(10, cancellationToken);
        
        if (userId.Contains("admin", StringComparison.OrdinalIgnoreCase))
            return new[] { "admin", "user" };
        if (userId.Contains("manager", StringComparison.OrdinalIgnoreCase))
            return new[] { "manager", "user" };
        
        return new[] { "user" };
    }
    
    private static IEnumerable<EPermission> MapRoleToUserPermissions(string role)
    {
        return role.ToUpperInvariant() switch
        {
            "ADMIN" => new[]
            {
                EPermission.AdminUsers,
                EPermission.UsersRead, EPermission.UsersCreate, 
                EPermission.UsersUpdate, EPermission.UsersDelete, EPermission.UsersList
            },
            "MANAGER" => new[]
            {
                EPermission.UsersRead, EPermission.UsersUpdate, EPermission.UsersList
            },
            "USER" => new[]
            {
                EPermission.UsersRead, EPermission.UsersProfile
            },
            _ => Array.Empty<EPermission>()
        };
    }
}
```

### 3. Uso em Endpoints

```csharp
// Modules/Users/API/Endpoints/UsersEndpoints.cs
public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");
        
        // GET /api/users - Requer permissão de leitura
        group.MapGet("/", GetUsers)
             .RequirePermission(EPermission.UsersRead)
             .WithName("GetUsers")
             .WithSummary("Lista todos os usuários");
        
        // POST /api/users - Requer permissão de criação
        group.MapPost("/", CreateUser)
             .RequirePermission(EPermission.UsersCreate)
             .WithName("CreateUser")
             .WithSummary("Cria um novo usuário");
    }
}
```

## 🔍 Integração com Keycloak

### Visão Geral

O `UsersPermissionResolver` suporta tanto uma implementação mock (para desenvolvimento/testes) quanto integração com Keycloak (para produção) através de configuração por variável de ambiente.

### Configuração

Defina a variável de ambiente `Authorization:UseKeycloak` no seu `appsettings.json`:

```json
{
  "Authorization": {
    "UseKeycloak": false  // true para usar Keycloak, false para mock
  }
}
```

**Configuração de Produção (Keycloak):**

```json
{
  "Authorization": {
    "UseKeycloak": true
  },
  "Keycloak": {
    "BaseUrl": "https://your-keycloak-instance.com",
    "Realm": "your-realm",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "AdminUsername": "admin-user",
    "AdminPassword": "admin-password"
  }
}
```

### Mapeamento de Roles

| Role | Permissões |
|------|------------|
| `meajudaai-system-admin` | `UsersRead`, `UsersUpdate`, `UsersDelete`, `AdminUsers` |
| `meajudaai-user-admin` | `UsersRead`, `UsersUpdate`, `UsersList` |
| `meajudaai-user` | `UsersRead`, `UsersProfile` |

## 🚀 Performance e Cache

O sistema implementa cache inteligente em múltiplas camadas:

```csharp
// Cache por usuário (30 minutos)
var permissions = await permissionService.GetUserPermissionsAsync(userId);

// Cache por módulo (15 minutos)
var modulePermissions = await permissionService.GetUserPermissionsByModuleAsync(userId, "Users");

// Invalidação seletiva
await permissionService.InvalidateUserPermissionsCacheAsync(userId);
```

## 🧪 Testes

### Handler de Autenticação para Testes

Para testes, use o handler de autenticação dedicado:

```csharp
// Em testes de integração
services.AddTestAuthentication(options =>
{
    options.DefaultUserId = "test-user";
    options.DefaultPermissions = new[] 
    { 
        EPermission.UsersRead, 
        EPermission.UsersCreate 
    };
});
```

## 🛠️ Solução de Problemas

### Problemas Comuns

1. **403 Forbidden inesperado**
   - Verifique se o usuário possui a permissão necessária
   - Confirme que o cache não está desatualizado
   - Valide o mapeamento de roles no Keycloak

2. **Performance lenta**
   - Monitore as métricas de taxa de acerto do cache
   - Verifique se os resolvedores modulares estão otimizados
   - Considere ajustar o TTL do cache

3. **Tokens JWT inválidos**
   - Confirme a configuração do Keycloak
   - Verifique se o realm está correto
   - Valide certificados e chaves
