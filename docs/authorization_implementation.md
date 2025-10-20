# Sistema de Autorização Type-Safe - Guia de Implementação

## 📋 Visão Geral

Este documento detalha o sistema de autorização type-safe implementado no MeAjudaAi, baseado em enums (`EPermission`) e arquitetura modular.

### Características do Sistema

✅ **Type-Safe**: Enum `EPermission` com validação em tempo de compilação  
✅ **Modular**: Cada módulo implementa seu próprio `IModulePermissionResolver`  
✅ **Performance**: Cache distribuído com HybridCache  
✅ **Extensível**: Suporte para múltiplos provedores de permissão  
✅ **Monitoramento**: Métricas integradas para observabilidade  

## 🔧 Componentes Principais

### 1. EPermission Enum

Sistema unificado de permissões type-safe:

```
public enum EPermission
{
    // ===== SISTEMA - GLOBAL =====
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

```
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

```
public interface IModulePermissionResolver
{
    string ModuleName { get; }
    Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default);
    bool CanResolve(EPermission permission);
}
```
## 🚀 Implementação

### 1. Configuração Básica

```
// Program.cs no ApiService
using MeAjudaAi.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configura o sistema completo de autorização
builder.Services.AddPermissionBasedAuthorization(builder.Configuration);

// Registra resolvers específicos dos módulos
builder.Services.AddModulePermissionResolver<UsersPermissionResolver>();

var app = builder.Build();

// Aplica middleware de autorização
app.UsePermissionBasedAuthorization();

app.Run();
```
### 2. Implementação de Module Resolver

```
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
        // Simula busca de roles (substitua pela lógica real)
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

```
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
        
        // PUT /api/users/{id} - Requer permissão de atualização
        group.MapPut("/{id:int}", UpdateUser)
             .RequirePermission(EPermission.UsersUpdate)
             .WithName("UpdateUser")
             .WithSummary("Atualiza um usuário");
        
        // DELETE /api/users/{id} - Requer múltiplas permissões
        group.MapDelete("/{id:int}", DeleteUser)
             .RequirePermissions(EPermission.UsersDelete, EPermission.AdminUsers)
             .WithName("DeleteUser")
             .WithSummary("Remove um usuário");
             
        // GET /api/users/profile - Acesso ao próprio perfil
        group.MapGet("/profile", GetMyProfile)
             .RequirePermission(EPermission.UsersProfile)
             .WithName("GetMyProfile")
             .WithSummary("Obtém perfil do usuário atual");
    }
    
    private static async Task<IResult> GetUsers(IUserRepository repository)
    {
        var users = await repository.GetAllAsync();
        return Results.Ok(users);
    }
    
    private static async Task<IResult> CreateUser(
        CreateUserDto dto, 
        IUserRepository repository)
    {
        var user = await repository.CreateAsync(dto);
        return Results.Created($"/api/users/{user.Id}", user);
    }
    
    private static async Task<IResult> UpdateUser(
        int id, 
        UpdateUserDto dto, 
        IUserRepository repository,
        ClaimsPrincipal currentUser,
        IPermissionService permissionService)
    {
        var currentUserId = currentUser.GetUserId();
        
        // Verificação contextual: admin pode editar qualquer usuário
        if (await permissionService.HasPermissionAsync(currentUserId, EPermission.AdminUsers))
        {
            var user = await repository.UpdateAsync(id, dto);
            return Results.Ok(user);
        }
        
        // Usuário pode editar apenas seu próprio perfil
        if (currentUserId == id.ToString() && 
            await permissionService.HasPermissionAsync(currentUserId, EPermission.UsersProfile))
        {
            var user = await repository.UpdateAsync(id, dto);
            return Results.Ok(user);
        }
        
        return Results.Forbid();
    }
    
    private static async Task<IResult> DeleteUser(
        int id, 
        IUserRepository repository)
    {
        await repository.DeleteAsync(id);
        return Results.NoContent();
    }
    
    private static async Task<IResult> GetMyProfile(
        ClaimsPrincipal user,
        IUserRepository repository)
    {
        var userId = user.GetUserId();
        var profile = await repository.GetByIdAsync(int.Parse(userId));
        return Results.Ok(profile);
    }
}
```
### 4. Configuração do Módulo

```
// Modules/Users/API/Extensions/UsersModuleExtensions.cs
public static class UsersModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        // Registra o resolver de permissões do módulo
        services.AddModulePermissionResolver<UsersPermissionResolver>();
        
        // Outros serviços do módulo...
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
    
    public static void MapUsersModule(this IEndpointRouteBuilder app)
    {
        app.MapUsersEndpoints();
    }
}
```
## 🔧 Cache e Performance

### Configuração de Cache

```
// Cache automático por usuário (30 minutos)
var permissions = await permissionService.GetUserPermissionsAsync(userId);

// Cache por módulo (15 minutos)
var modulEPermission = await permissionService.GetUserPermissionsByModuleAsync(userId, "Users");

// Invalidação quando necessário
await permissionService.InvalidateUserPermissionsCacheAsync(userId);
```
### Métricas Automáticas

O sistema coleta automaticamente:

- ⏱️ Tempo de resolução de permissões
- 📊 Taxa de acerto do cache
- ❌ Falhas de autorização
- 📈 Performance por módulo

```
// Métricas são expostas em /metrics para Prometheus
// Configuração automática, sem código adicional necessário
```
## 🧪 Testes

### Configuração para Testes

```
// WebApplicationFactory para testes de integração
public class UsersApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Substitui autenticação por versão de teste
            services.AddTestAuthentication(options =>
            {
                options.DefaultUserId = "test-user";
                options.DefaultPermissions = new[]
                {
                    EPermission.UsersRead,
                    EPermission.UsersCreate
                };
            });
        });
    }
}
```
### Exemplo de Teste

```
[Test]
public async Task GetUsers_WithValidPermission_ShouldReturnUsers()
{
    // Arrange
    using var factory = new UsersApiFactory();
    using var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/users");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
    users.Should().NotBeNull();
}

[Test]
public async Task CreateUser_WithoutPermission_ShouldReturnForbidden()
{
    // Arrange
    using var factory = new UsersApiFactory();
    factory.ConfigureTestPermissions(Array.Empty<EPermission>());
    using var client = factory.CreateClient();
    
    var createDto = new CreateUserDto { Name = "Test User" };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/users", createDto);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```
## 📋 Checklist de Implementação

### ✅ Configuração Inicial
- [ ] Configurar `AddPermissionBasedAuthorization()` no Program.cs
- [ ] Implementar `IModulePermissionResolver` para o módulo
- [ ] Registrar resolver com `AddModulePermissionResolver<T>()`
- [ ] Adicionar middleware com `UsePermissionBasedAuthorization()`

### ✅ Endpoints
- [ ] Aplicar `.RequirePermission()` nos endpoints
- [ ] Usar `.RequirePermissions()` para múltiplas permissões
- [ ] Implementar verificações contextuais quando necessário
- [ ] Adicionar documentação/summary aos endpoints

### ✅ Testes
- [ ] Configurar `TestAuthenticationHandler`
- [ ] Criar testes para cenários com e sem permissão
- [ ] Validar cache e performance
- [ ] Testar invalidação de cache

### ✅ Monitoramento
- [ ] Verificar métricas em /metrics
- [ ] Configurar alertas para falhas de autorização
- [ ] Monitorar performance do cache
- [ ] Validar logs de segurança

## 📖 Documentação Relacionada

- [Sistema de Autenticação Principal](../authentication.md)
- [Type-Safe Permissions System](./type_safe_permissions_system.md)
- [Server-Side Permission Resolution Guide](./server_side_permissions.md)
- [Test Authentication Handler](./development.md#3-test-authentication-handler)

---

**Status**: ✅ **Implementado e Ativo**  
**Última Atualização**: Outubro 2025  
**Sistema**: Type-Safe Authorization com EPermission
