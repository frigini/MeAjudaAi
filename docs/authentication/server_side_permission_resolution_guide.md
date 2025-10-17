# Configuração Completa do Sistema de Permissões Server-Side

Este documento detalha como configurar e usar o sistema completo de permissões type-safe com resolução server-side, métricas, cache e integração com Keycloak.

## 1. Configuração Básica no Program.cs

### ApiService/Program.cs
```csharp
using MeAjudaAi.Modules.Users.API.Extensions;
using MeAjudaAi.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configuração básica do Aspire (já existente)
builder.AddServiceDefaults();

// ⭐ Adiciona sistema de permissões completo
builder.Services.AddPermissionBasedAuthorization(builder.Configuration);

// Registra módulos específicos
builder.Services.AddUsersModule();
// builder.Services.AddProvidersModule(); // Futuros módulos
// builder.Services.AddOrdersModule();

// Configuração de autenticação (Keycloak)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.Audience = builder.Configuration["Authentication:Keycloak:Audience"];
        options.RequireHttpsMetadata = false; // Apenas para desenvolvimento
    });

var app = builder.Build();

// ⭐ Middleware de otimização ANTES da autenticação
app.UsePermissionBasedAuthorization();

// Pipeline padrão
app.UseAuthentication();
app.UseAuthorization();

// Mapeia endpoints dos módulos
app.MapUsersEndpoints();

// Health checks incluem verificação de permissões
app.MapHealthChecks("/health");

app.Run();
```

## 2. Configuração no appsettings.json

### appsettings.json
```json
{
  "Authentication": {
    "Keycloak": {
      "BaseUrl": "http://localhost:8080",
      "Realm": "meajudaai",
      "AdminClientId": "meajudaai-admin",
      "AdminClientSecret": "your-admin-client-secret",
      "Authority": "http://localhost:8080/realms/meajudaai",
      "Audience": "meajudaai-api"
    }
  },
  "Logging": {
    "LogLevel": {
      "MeAjudaAi.Shared.Authorization": "Debug"
    }
  }
}
```

### appsettings.Production.json
```json
{
  "Authentication": {
    "Keycloak": {
      "BaseUrl": "https://auth.meajudaai.com",
      "Realm": "meajudaai",
      "AdminClientId": "meajudaai-admin",
      "AdminClientSecret": "{{KEYCLOAK_ADMIN_SECRET}}",
      "Authority": "https://auth.meajudaai.com/realms/meajudaai",
      "Audience": "meajudaai-api"
    }
  }
}
```

## 3. Estrutura de Roles no Keycloak

### Roles Recomendados
```bash
Realm Roles:
├── meajudaai-system-admin     # Administrador completo
├── meajudaai-user-admin       # Administrador de usuários
├── meajudaai-user-operator    # Operador de usuários
├── meajudaai-user             # Usuário básico
├── meajudaai-provider-admin   # Admin de prestadores
├── meajudaai-provider         # Prestador
├── meajudaai-order-admin      # Admin de pedidos
├── meajudaai-order-operator   # Operador de pedidos
├── meajudaai-report-admin     # Admin de relatórios
└── meajudaai-report-viewer    # Visualizador de relatórios
```

### Mapeamento Automático
O `KeycloakPermissionResolver` mapeia automaticamente estes roles para permissões:

```csharp
// Sistema
"meajudaai-system-admin" → Todas as permissões
"meajudaai-user-admin" → AdminUsers + CRUD usuários

// Módulos específicos  
"meajudaai-provider-admin" → CRUD prestadores
"meajudaai-order-admin" → CRUD pedidos
"meajudaai-report-admin" → Criar/exportar relatórios
```

## 4. Uso em Endpoints

### Endpoints com Permissões Type-Safe
```csharp
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");
        
        // ⭐ Permissão única
        group.MapGet("/", GetUsersAsync)
            .RequirePermission(Permission.UsersList)
            .RequireAuthorization();
        
        // ⭐ Múltiplas permissões (todas obrigatórias)
        group.MapDelete("/{id:guid}", DeleteUserAsync)
            .RequirePermissions(Permission.UsersDelete, Permission.AdminUsers)
            .RequireAuthorization();
        
        // ⭐ Qualquer uma das permissões
        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .RequireAnyPermission(Permission.UsersRead, Permission.AdminUsers)
            .RequireAuthorization();
        
        // ⭐ Permissões específicas de módulo
        group.MapGet("/admin", GetAdminViewAsync)
            .RequireModulePermission("Users", Permission.AdminUsers, Permission.UsersList)
            .RequireAuthorization();
        
        // ⭐ Admin do sistema
        group.MapPost("/system/reset", ResetSystemAsync)
            .RequireSystemAdmin()
            .RequireAuthorization();
        
        return endpoints;
    }
}
```

### Handlers com Verificação Server-Side
```csharp
private static async Task<IResult> GetUsersAsync(
    HttpContext context,
    IPermissionService permissionService)
{
    var userId = context.User.FindFirst("sub")?.Value;
    
    // ⭐ Verificação server-side adicional (redundante mas segura)
    if (!string.IsNullOrEmpty(userId) && 
        await permissionService.HasPermissionAsync(userId, Permission.UsersList))
    {
        // Lógica do endpoint
        return Results.Ok(new { message = "Lista de usuários" });
    }
    
    return Results.Forbid();
}

// ⭐ Verificação de múltiplas permissões
private static async Task<IResult> DeleteUserAsync(
    Guid id,
    HttpContext context,
    IPermissionService permissionService)
{
    var userId = context.User.FindFirst("sub")?.Value;
    
    if (!string.IsNullOrEmpty(userId) && 
        await permissionService.HasPermissionsAsync(userId, new[] 
        { 
            Permission.UsersDelete, 
            Permission.AdminUsers 
        }))
    {
        // Lógica de remoção
        return Results.Ok(new { id, message = "Usuário removido" });
    }
    
    return Results.Forbid();
}
```

## 5. Verificações Client-Side (Controllers/Views)

### Em Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    public IActionResult GetProfile()
    {
        // ⭐ Verificações diretas no ClaimsPrincipal
        if (!User.HasPermission(Permission.UsersProfile))
        {
            return Forbid();
        }
        
        if (User.IsSystemAdmin())
        {
            // Lógica para admin
        }
        
        var tenantId = User.GetTenantId();
        var permissions = User.GetPermissions();
        
        return Ok(new { profile = "data", permissions });
    }
}
```

### Em Views/Components
```csharp
@using MeAjudaAi.Shared.Authorization
@if (User.HasPermission(Permission.UsersCreate))
{
    <button class="btn btn-primary">Criar Usuário</button>
}

@if (User.HasAnyPermission(Permission.AdminUsers, Permission.AdminSystem))
{
    <div class="admin-panel">
        <!-- Conteúdo administrativo -->
    </div>
}
```

## 6. Monitoramento e Observabilidade

### Métricas Automáticas
O sistema coleta automaticamente:
- `meajudaai_permission_resolutions_total` - Resoluções de permissão
- `meajudaai_permission_checks_total` - Verificações de permissão
- `meajudaai_permission_cache_hits_total` - Cache hits
- `meajudaai_authorization_failures_total` - Falhas de autorização
- `meajudaai_permission_resolution_duration_seconds` - Duração das operações

### Health Checks
Endpoint `/health` inclui verificação automática:
- ✅ Funcionalidade básica
- ✅ Performance (cache hit rate, tempo de resposta)
- ✅ Integridade do cache
- ✅ Registros de resolvers modulares

### Logs Estruturados
```
// Logs automáticos incluem:
[INF] Added 7 permission claims for user user-123
[WRN] Authorization failure: User user-456 denied users:delete - Permission not granted
[DBG] Resolved 5 permissions from 2 Keycloak roles for user user-789
```

## 7. Cache e Performance

### Configuração Automática
- **Cache Local**: 5 minutos (HybridCache local)
- **Cache Distribuído**: 30 minutos (Redis/SQL Server)
- **Invalidação**: Por tags (`user:{userId}`, `module:{module}`)

### Otimizações Automáticas
- **Middleware de Otimização**: Identifica permissões necessárias por rota
- **Cache Agressivo**: Para operações de leitura em endpoints específicos
- **Bypass**: Para endpoints públicos (/health, /metrics, etc.)

### API de Cache Manual
```csharp
// Invalida cache de usuário específico
await permissionService.InvalidateUserPermissionsCacheAsync("user-123");

// Métricas de cache
var stats = metricsService.GetSystemStats();
Console.WriteLine($"Cache hit rate: {stats.CacheHitRate:P1}");
```

## 8. Desenvolvimento e Testes

### Testes Automatizados
```bash
# Testes unitários
dotnet test --filter "Category=Unit"

# Testes de integração com autenticação simulada
dotnet test --filter "Category=Integration"

# Testes E2E com fluxos completos
dotnet test --filter "Category=E2E"

# Testes de arquitetura
dotnet test --filter "Category=Architecture"
```

### Ambiente de Desenvolvimento
```csharp
// TestAuthenticationHandler para testes
services.AddAuthentication("Test")
    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options =>
    {
        options.Claims = new[]
        {
            new Claim("sub", "test-user"),
            new Claim(CustomClaimTypes.Permission, Permission.UsersRead.GetValue())
        };
    });
```

## 9. Extensibilidade para Novos Módulos

### Adicionando Módulo Providers
```csharp
// 1. Adicionar permissões no enum Permission
public enum Permission
{
    // ... existing permissions
    
    [Display(Name = "providers:read", Description = "Visualizar prestadores")]
    ProvidersRead,
    
    [Display(Name = "providers:create", Description = "Criar prestadores")]
    ProvidersCreate,
}

// 2. Criar resolver específico
public sealed class ProvidersPermissionResolver : IModulePermissionResolver
{
    public string ModuleName => "Providers";
    
    public async Task<IReadOnlyList<Permission>> ResolvePermissionsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        // Lógica específica do módulo
    }
}

// 3. Registrar no DI
public static IServiceCollection AddProvidersModule(this IServiceCollection services)
{
    services.AddModulePermissionResolver<ProvidersPermissionResolver>();
    return services;
}

// 4. Configurar no Program.cs
builder.Services.AddProvidersModule();
```

## 10. Troubleshooting

### Problemas Comuns

**Cache não funciona**
```bash
# Verifique se HybridCache está configurado no Aspire
# Logs devem mostrar: "Added X permission claims for user Y"
```

**Permissões não carregam**
```bash
# Verifique configuração Keycloak
# Teste endpoint: GET /health - deve mostrar resolver_count > 0
```

**Performance degradada**
```bash
# Monitore métricas
curl /metrics | grep meajudaai_permission
# Cache hit rate deve estar > 70%
```

**Roles não mapeiam**
```bash
# Verifique nomes exatos no Keycloak
# Logs devem mostrar: "Retrieved X roles from Keycloak for user Y"
```

O sistema está agora **completo e pronto para produção** com:
- ✅ Permissões type-safe
- ✅ Resolução server-side com cache
- ✅ Integração Keycloak
- ✅ Métricas e monitoramento
- ✅ Health checks
- ✅ Otimizações de performance
- ✅ Extensibilidade modular