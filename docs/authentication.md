# Authentication and Authorization System

Este documento cobre o sistema completo de autenticação e autorização do MeAjudaAi, incluindo integração com Keycloak e sistema de permissões type-safe.

## 📋 Visão Geral

O MeAjudaAi utiliza um sistema robusto de autenticação e autorização com as seguintes características:

- **Autenticação**: Integração com Keycloak usando JWT tokens
- **Autorização**: Sistema type-safe baseado em enums (`EPermissions`)
- **Arquitetura Modular**: Cada módulo pode implementar suas próprias regras de permissão
- **Cache Inteligente**: HybridCache para otimização de performance
- **Extensibilidade**: Suporte para múltiplos provedores de permissão

## 🏗️ Arquitetura do Sistema

### Componentes Principais

```text
Authentication & Authorization System
├── Authentication (Keycloak + JWT)
│   ├── JWT Token Validation
│   ├── Claims Transformation
│   └── User Identity Management
│
└── Authorization (Type-Safe Permissions)
    ├── EPermissions Enum (Type-Safe)
    ├── Permission Service (Caching + Resolution)
    ├── Module Permission Resolvers
    └── Authorization Handlers
```csharp
### Fluxo de Autorização

```mermaid
graph TD
    A[Request] --> B[JWT Validation]
    B --> C[Claims Transformation]
    C --> D[Permission Resolution]
    D --> E[Permission Cache]
    E --> F{Permission Check}
    F -->|Allow| G[Endpoint Execution]
    F -->|Deny| H[403 Forbidden]
    
    D --> I[Module Resolvers]
    I --> J[Keycloak Roles]
    J --> K[Permission Mapping]
```text
## 🔐 Sistema de Permissões

### EPermissions Enum

O sistema utiliza um enum type-safe para definir todas as permissões:

```csharp
public enum EPermissions
{
    // Sistema
    [Display(Name = "system:read")]
    SystemRead,
    
    [Display(Name = "system:admin")]
    SystemAdmin,
    
    // Usuários
    [Display(Name = "users:read")]
    UsersRead,
    
    [Display(Name = "users:create")]
    UsersCreate,
    
    [Display(Name = "users:update")]
    UsersUpdate,
    
    [Display(Name = "users:delete")]
    UsersDelete,
    
    // Administração
    [Display(Name = "admin:system")]
    AdminSystem,
    
    [Display(Name = "admin:users")]
    AdminUsers
}
```csharp
### Uso em Endpoints

```csharp
// Extension methods fluentes
app.MapGet("/api/users", GetUsers)
   .RequirePermission(EPermissions.UsersRead);

app.MapPost("/api/users", CreateUser)
   .RequirePermissions(EPermissions.UsersCreate, EPermissions.UsersWrite);

app.MapDelete("/api/users/{id}", DeleteUser)
   .RequirePermission(EPermissions.UsersDelete);
```csharp
### Verificação Programática

```csharp
// Em controladores ou services
public async Task<IResult> GetUserData(
    ClaimsPrincipal user,
    IPermissionService permissionService)
{
    // Verificação simples
    if (!user.HasPermission(EPermissions.UsersRead))
        return Results.Forbid();
    
    // Verificação assíncrona com service
    var userId = user.GetUserId();
    if (!await permissionService.HasPermissionAsync(userId, EPermissions.UsersRead))
        return Results.Forbid();
        
    // Múltiplas permissões
    var hasAnyPermission = await permissionService.HasPermissionsAsync(
        userId, 
        [EPermissions.UsersRead, EPermissions.AdminUsers], 
        requireAll: false);
    
    return Results.Ok(/* data */);
}
```yaml
## ⚙️ Configuração

### 1. Configuração Básica

```csharp
// Program.cs
using MeAjudaAi.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o sistema completo de autorização
builder.Services.AddPermissionBasedAuthorization(builder.Configuration);

// Adiciona resolvers específicos de módulos
builder.Services.AddModulePermissionResolver<UsersPermissionResolver>();

var app = builder.Build();

// Aplica middleware de autorização
app.UsePermissionBasedAuthorization();
```csharp
### 2. Configuração do Keycloak

```json
// appsettings.json
{
  "Keycloak": {
    "BaseUrl": "http://localhost:8080",
    "Realm": "meajudaai",
    "AdminClientId": "admin-cli",
    "AdminClientSecret": "your-client-secret"
  },
  "Authentication": {
    "Keycloak": {
      "Authority": "http://localhost:8080/realms/meajudaai",
      "Audience": "account",
      "MetadataAddress": "http://localhost:8080/realms/meajudaai/.well-known/openid_configuration",
      "RequireHttpsMetadata": false
    }
  }
}
```yaml
### 3. Configuração de Autenticação JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/meajudaai";
        options.Audience = "meajudaai-client";
        options.RequireHttpsMetadata = false; // Apenas para desenvolvimento
    });
```bash
### 4. Setup Local com Docker

```bash
# Quick setup com Keycloak standalone
docker compose -f infrastructure/compose/standalone/keycloak-only.yml up -d

# Ou ambiente completo de desenvolvimento
docker compose -f infrastructure/compose/environments/development.yml up -d
```yaml
## 🏗️ Implementação Modular

### Permission Resolver por Módulo

Cada módulo pode implementar sua própria lógica de resolução de permissões:

```csharp
public class UsersPermissionResolver : IModulePermissionResolver
{
    public string ModuleName => "Users";
    
    public async Task<IReadOnlyList<EPermissions>> ResolvePermissionsAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        // Lógica específica do módulo para resolver permissões
        var userRoles = await GetUserRolesAsync(userId, cancellationToken);
        
        var permissions = new List<EPermissions>();
        
        foreach (var role in userRoles)
        {
            permissions.AddRange(MapRoleToPermissions(role));
        }
        
        return permissions;
    }
    
    public bool CanResolve(EPermissions permission)
    {
        // Verifica se este resolver pode lidar com a permissão
        return permission.GetModule().Equals("users", StringComparison.OrdinalIgnoreCase);
    }
    
    private IEnumerable<EPermissions> MapRoleToPermissions(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "user-admin" => new[] { 
                EPermissions.UsersRead, 
                EPermissions.UsersCreate, 
                EPermissions.UsersUpdate, 
                EPermissions.UsersDelete 
            },
            "user-operator" => new[] { 
                EPermissions.UsersRead, 
                EPermissions.UsersUpdate 
            },
            "user" => new[] { EPermissions.UsersRead },
            _ => Array.Empty<EPermissions>()
        };
    }
}
```yaml
### Registro do Resolver

```csharp
// Na configuração do módulo
public static class UsersModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        // Registra o resolver de permissões do módulo
        services.AddModulePermissionResolver<UsersPermissionResolver>();
        
        return services;
    }
}
```csharp
## 🚀 Performance e Cache

### Sistema de Cache

O sistema implementa cache inteligente em múltiplas camadas:

```csharp
// Cache por usuário (30 minutos)
var permissions = await permissionService.GetUserPermissionsAsync(userId);

// Cache por módulo (15 minutos)  
var modulePermissions = await permissionService.GetUserPermissionsByModuleAsync(userId, "Users");

// Invalidação seletiva
await permissionService.InvalidateUserPermissionsCacheAsync(userId);
```csharp
### Métricas e Monitoramento

O sistema coleta métricas detalhadas:

- Tempo de resolução de permissões
- Taxa de acerto do cache
- Falhas de autorização
- Performance por módulo

```csharp
// Métricas são coletadas automaticamente
// Consulte /metrics para Prometheus ou Application Insights
```yaml
## 🔍 Keycloak Integration

### Setup do Realm

O realm do Keycloak inclui:
- **Realm**: `meajudaai`
- **Client ID**: `meajudaai-client`
- **Redirect URIs**: `http://localhost:*`
- **Usuários padrão**:
  - Admin: `admin@meajudaai.com` / `admin123`
  - User: `user@meajudaai.com` / `user123`

### Mapeamento de Roles

Roles do Keycloak são automaticamente mapeados para permissões:

```csharp
// Configuração no KeycloakPermissionResolver
private static IEnumerable<EPermissions> MapKeycloakRoleToPermissions(string roleName)
{
    return roleName.ToLowerInvariant() switch
    {
        "meajudaai-system-admin" => new[]
        {
            EPermissions.AdminSystem,
            EPermissions.AdminUsers,
            EPermissions.UsersRead,
            EPermissions.UsersCreate,
            EPermissions.UsersUpdate,
            EPermissions.UsersDelete
        },
        "meajudaai-user-admin" => new[]
        {
            EPermissions.AdminUsers,
            EPermissions.UsersRead,
            EPermissions.UsersCreate,
            EPermissions.UsersUpdate
        },
        "meajudaai-user" => new[]
        {
            EPermissions.UsersRead
        },
        _ => Array.Empty<EPermissions>()
    };
}
```csharp
### Claims Mapping

O sistema mapeia claims do Keycloak:
- `sub` → User ID
- `email` → Email address
- `preferred_username` → Username
- `realm_access.roles` → User roles

## 🧪 Testing

### Test Authentication Handler

Para testes, utilize o handler de autenticação dedicado:

```csharp
// Em testes de integração
services.AddTestAuthentication(options =>
{
    options.DefaultUserId = "test-user";
    options.DefaultPermissions = new[] 
    { 
        EPermissions.UsersRead, 
        EPermissions.UsersCreate 
    };
});
```yaml
### Testes Unitários

```csharp
[Test]
public async Task ShouldAllowUserWithPermission()
{
    // Arrange
    var user = CreateTestUser(EPermissions.UsersRead);
    
    // Act
    var result = await endpoint.HandleAsync(user);
    
    // Assert
    result.Should().BeOfType<Ok<UserDto>>();
}
```csharp
## 📚 Exemplos Avançados

### Permissões Contextuais

```csharp
public async Task<IResult> UpdateUser(
    int userId,
    UpdateUserDto dto,
    ClaimsPrincipal currentUser,
    IPermissionService permissionService)
{
    var currentUserId = currentUser.GetUserId();
    
    // Admin pode editar qualquer usuário
    if (await permissionService.HasPermissionAsync(currentUserId, EPermissions.AdminUsers))
        return await UpdateUserInternal(userId, dto);
    
    // Usuário pode editar apenas seu próprio perfil
    if (currentUserId == userId.ToString() && 
        await permissionService.HasPermissionAsync(currentUserId, EPermissions.UsersProfile))
        return await UpdateUserInternal(userId, dto);
    
    return Results.Forbid();
}
```text
### Extension Methods Customizados

```csharp
public static class CustomPermissionExtensions
{
    public static bool CanManageUser(this ClaimsPrincipal user, string targetUserId)
    {
        // Admin pode gerenciar qualquer usuário
        if (user.HasPermission(EPermissions.AdminUsers))
            return true;
        
        // Usuário pode gerenciar apenas a si mesmo
        return user.GetUserId() == targetUserId && 
               user.HasPermission(EPermissions.UsersProfile);
    }
}
```csharp
## 🛠️ Troubleshooting

### Problemas Comuns

1. **403 Forbidden inesperado**
   - Verifique se o usuário possui a permissão necessária
   - Confirme se o cache não está desatualizado
   - Valide o mapeamento de roles no Keycloak

2. **Performance lenta**
   - Monitore métricas de cache hit ratio
   - Verifique se resolvers modulares estão otimizados
   - Considere ajustar TTL do cache

3. **Tokens JWT inválidos**
   - Confirme configuração do Keycloak
   - Verifique se o realm está correto
   - Valide certificados e chaves

### Debug e Logs

```csharp
// Habilitar logs detalhados
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("MeAjudaAi.Shared.Authorization", LogLevel.Trace);
```text
## 📋 Checklist de Implementação

- [ ] Configurar Keycloak realm
- [ ] Implementar Permission Resolver do módulo
- [ ] Adicionar permissões nos endpoints
- [ ] Configurar cache e métricas
- [ ] Implementar testes de autorização
- [ ] Validar performance em produção

---

## 📖 Documentação Relacionada

- [Server-Side Permission Resolution Guide](./authentication/server_side_permission_resolution_guide.md)
- [Test Authentication Handler](./testing/test_authentication_handler.md)
- [Development Guidelines](./development-guidelines.md)
- [Test Configuration](../testing/test_auth_configuration.md)
- [Test Examples](../testing/test_auth_examples.md)

## Production Deployment

### Environment Configuration

In production, ensure the following environment variables are set:

```bash
Authentication__Keycloak__Authority=https://your-keycloak-domain/realms/meajudaai
Authentication__Keycloak__RequireHttpsMetadata=true
Authentication__Keycloak__Audience=account
```csharp
### Security Considerations

1. **HTTPS Required**: Always use HTTPS in production
2. **Token Validation**: Ensure proper token signature validation
3. **Audience Validation**: Validate the token audience claim
4. **Issuer Validation**: Validate the token issuer claim

### SSL/TLS Configuration

For production deployments, configure SSL certificates:
- Use valid SSL certificates for Keycloak
- Configure proper trust store if using custom certificates
- Ensure certificate chain validation

## Troubleshooting

### Common Issues

1. **Token Validation Errors**
   - Check authority URL configuration
   - Verify metadata endpoint accessibility
   - Ensure proper audience configuration

2. **CORS Issues**
   - Configure allowed origins in Keycloak client
   - Set proper CORS headers in application

3. **Certificate Issues**
   - Verify SSL certificate validity
   - Check certificate trust chain
   - Configure proper certificate validation

### Debug Logging

Enable authentication debug logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```csharp
### Health Checks

The application includes authentication health checks:
- Keycloak connectivity
- Token validation endpoint
- Metadata endpoint accessibility

## 📖 Documentação Relacionada

### Documentação Especializada
- **[Guia de Implementação de Autorização](./authentication/authorization_system_implementation.md)** - Guia completo para implementar autorização type-safe
- **[Sistema de Permissões Type-Safe](./authentication/type_safe_permissions_system.md)** - Detalhes do sistema baseado em EPermissions
- **[Resolução Server-Side de Permissões](./authentication/server_side_permission_resolution_guide.md)** - Guia para resolução de permissões no servidor

### Desenvolvimento e Testes
- **[Test Authentication Handler](./testing/test_authentication_handler.md)** - Handler configurável para cenários de teste
- **[Exemplos de Teste de Auth](../testing/test_auth_examples.md)** - Exemplos práticos de autenticação em testes

### Arquitetura e Operações
- **[Guias de Desenvolvimento](./development-guidelines.md)** - Diretrizes gerais de desenvolvimento
- **[Arquitetura do Sistema](./architecture.md)** - Visão geral da arquitetura
- **[CI/CD e Infraestrutura](./ci_cd.md)** - Configuração de pipeline e deploy

## Troubleshooting

### Common Issues

1. **Token Validation Errors**
   - Check authority URL configuration
   - Verify metadata endpoint accessibility
   - Ensure proper audience configuration

2. **CORS Issues**
   - Configure allowed origins in Keycloak client
   - Set proper CORS headers in application

3. **Certificate Issues**
   - Verify SSL certificate validity
   - Check certificate trust chain
   - Configure proper certificate validation

4. **Permission Resolution Errors**
   - Verify module permission resolvers are registered
   - Check EPermissions enum mapping
   - Validate cache configuration

### Debug Logging

Enable authentication debug logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug",
      "MeAjudaAi.Shared.Authorization": "Debug"
    }
  }
}
```text
### Health Checks

The application includes authentication health checks:
- Keycloak connectivity
- Token validation endpoint
- Metadata endpoint accessibility
- Permission service availability

## API Documentation

The Swagger UI includes authentication support:
1. Click "Authorize" button
2. Enter JWT token in format: `Bearer <token>`
3. Test authenticated endpoints

For obtaining tokens during development, see the [testing documentation](../testing/test_auth_examples.md).