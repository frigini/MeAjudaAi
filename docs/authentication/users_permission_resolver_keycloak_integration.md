# UsersPermissionResolver - Keycloak Integration

## 🎯 Overview

O `UsersPermissionResolver` foi atualizado para suportar tanto **implementação mock** (para desenvolvimento/testes) quanto **integração com Keycloak** (para produção) através de configuração por environment variable.

## ⚙️ Configuration

### Environment Variable

Configure a environment variable `Authorization:UseKeycloak` no seu `appsettings.json`:

```json
{
  "Authorization": {
    "UseKeycloak": false  // true para usar Keycloak, false para mock
  }
}
```

### Configuração para Desenvolvimento (Mock)

```json
{
  "Authorization": {
    "UseKeycloak": false
  }
}
```

**Mock Implementation:**
- Usa padrões de `userId` para simular roles
- `admin` → `["meajudaai-system-admin", "meajudaai-user-admin"]`
- `manager` → `["meajudaai-user-admin"]` 
- Outros → `["meajudaai-user"]`
- Simula delay de 10ms para realismo

### Configuração para Produção (Keycloak)

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

## 🔧 Implementation Details

### Dependency Injection

O resolver é injetado automaticamente quando você usa `AddPermissionBasedAuthorization()`:

```csharp
// No Program.cs ou Startup.cs
services.AddPermissionBasedAuthorization(configuration);
```

### Constructor Logic

```csharp
public UsersPermissionResolver(
    ILogger<UsersPermissionResolver> logger, 
    IConfiguration configuration,
    IKeycloakPermissionResolver? keycloakResolver = null)
{
    _useKeycloak = configuration.GetValue<bool>("Authorization:UseKeycloak", false);
    
    // Fallback para mock se Keycloak não estiver disponível
    if (_useKeycloak && keycloakResolver == null)
    {
        _logger.LogWarning("Keycloak integration enabled but resolver not available. Using mock.");
        _useKeycloak = false;
    }
}
```

### Role Resolution Flow

```csharp
public async Task<IReadOnlyList<EPermissions>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken)
{
    // 1. Determina qual implementação usar
    var userRoles = _useKeycloak 
        ? await GetUserRolesFromKeycloakAsync(userId, cancellationToken)
        : await GetUserRolesMockAsync(userId, cancellationToken);
    
    // 2. Mapeia roles para permissões
    var permissions = new List<EPermissions>();
    foreach (var role in userRoles)
    {
        permissions.AddRange(MapRoleToUserPermissions(role));
    }
    
    // 3. Remove duplicatas e retorna
    return permissions.Distinct().ToList();
}
```

## 📊 Role Mapping

### Roles → Permissions Mapping

| Role | Permissions |
|------|-------------|
| `meajudaai-system-admin` | `UsersRead`, `UsersUpdate`, `UsersDelete`, `AdminUsers` |
| `meajudaai-user-admin` | `UsersRead`, `UsersUpdate`, `UsersList` |
| `meajudaai-user` | `UsersRead`, `UsersProfile` |

### Keycloak Integration

O método `GetUserRolesFromKeycloakAsync` usa o `IKeycloakPermissionResolver` existente:

1. **Busca permissões** via Keycloak resolver
2. **Converte para roles** para manter compatibilidade
3. **Fallback para mock** em caso de erro

## 🔍 Logging

### Debug Logs

```
[Debug] UsersPermissionResolver initialized with {Keycloak|Mock} implementation
[Debug] Fetching user roles from Keycloak for user {UserId}
[Debug] Retrieved {RoleCount} roles from {Keycloak|Mock} for user {UserId}: {Roles}
[Debug] Resolved {PermissionCount} Users module permissions for user {UserId} using {ResolverType}
```

### Error Handling

```
[Warning] Keycloak integration enabled but resolver not available. Using mock.
[Error] Failed to fetch roles from Keycloak for user {UserId}, falling back to mock
[Error] Failed to resolve Users module permissions for user {UserId}
```

## 🧪 Testing

### Development Testing (Mock)

```json
{
  "Authorization": { "UseKeycloak": false }
}
```

- Testa com `userId` contendo `"admin"`, `"manager"`, ou outros valores
- Verifica mapeamento de roles mock

### Integration Testing (Keycloak)

```json
{
  "Authorization": { "UseKeycloak": true },
  "Keycloak": { /* configuração real */ }
}
```

- Testa com usuários reais do Keycloak
- Verifica integração completa

## 🚀 Environment Variables

### Docker Compose

```yaml
environment:
  - Authorization__UseKeycloak=true
  - Keycloak__BaseUrl=https://keycloak.company.com
  - Keycloak__Realm=production
```

### Kubernetes

```yaml
env:
  - name: Authorization__UseKeycloak
    value: "true"
  - name: Keycloak__BaseUrl
    valueFrom:
      secretKeyRef:
        name: keycloak-config
        key: base-url
```

## 🔒 Security Considerations

1. **Keycloak Credentials:** Use secrets management para `ClientSecret` e credenciais admin
2. **Caching:** Roles são cached por 15 minutos via `HybridCache`
3. **Fallback:** Sempre falha graciosamente para mock em caso de erro
4. **Logging:** Não loga informações sensíveis, apenas IDs de usuário

## 📈 Performance

- **Mock:** ~10ms de delay simulado
- **Keycloak:** Cache de 15 minutos, 5 minutos local
- **Fallback:** Automático em caso de falha do Keycloak
- **Async:** Completamente assíncrono com `CancellationToken` support

## 🔄 Migration Path

### Fase 1: Development
```json
{ "Authorization": { "UseKeycloak": false } }
```

### Fase 2: Staging  
```json
{ "Authorization": { "UseKeycloak": true } }
```

### Fase 3: Production
```json
{ "Authorization": { "UseKeycloak": true } }
```

Com environment variables específicas por ambiente.