# Authorization Implementation - MeAjudaAi Web Admin

## Overview

The MeAjudaAi.Web.Admin project implements comprehensive role-based authorization using ASP.NET Core authorization policies adapted for Blazor WASM with Keycloak OIDC authentication.

---

## Architecture

### Roles

Defined in `Authorization/RoleNames.cs`:

| Role | Description | Access Level |
|------|-------------|--------------|
| `admin` | System administrator | Full access to all features |
| `provider-manager` | Provider manager | Manage providers (create, edit, delete) |
| `document-reviewer` | Document reviewer | Review and approve documents |
| `catalog-manager` | Catalog manager | Manage services and categories |
| `viewer` | Viewer | Read-only access |

### Policies

Defined in `Authorization/PolicyNames.cs` and registered in `Program.cs`:

| Policy | Required Roles | Usage |
|--------|---------------|-------|
| `AdminPolicy` | `admin` | Administrative pages (AllowedCities, Settings) |
| `ProviderManagerPolicy` | `provider-manager` or `admin` | Providers page |
| `DocumentReviewerPolicy` | `document-reviewer` or `admin` | Documents page |
| `CatalogManagerPolicy` | `catalog-manager` or `admin` | Services, Categories pages |
| `ViewerPolicy` | Any authenticated user | Dashboard, read-only views |

---

## Configuration

### Program.cs

Authorization policies are configured in `Program.cs`:

```csharp
// Autorização com políticas baseadas em roles
builder.Services.AddAuthorizationCore(options =>
{
    // Política de Admin - requer role "admin"
    options.AddPolicy(PolicyNames.AdminPolicy, policy =>
        policy.RequireRole(RoleNames.Admin));

    // Política de Gerente de Provedores - requer "provider-manager" ou "admin"
    options.AddPolicy(PolicyNames.ProviderManagerPolicy, policy =>
        policy.RequireRole(RoleNames.ProviderManager, RoleNames.Admin));

    // Política de Revisor de Documentos - requer "document-reviewer" ou "admin"
    options.AddPolicy(PolicyNames.DocumentReviewerPolicy, policy =>
        policy.RequireRole(RoleNames.DocumentReviewer, RoleNames.Admin));

    // Política de Gerente de Catálogo - requer "catalog-manager" ou "admin"
    options.AddPolicy(PolicyNames.CatalogManagerPolicy, policy =>
        policy.RequireRole(RoleNames.CatalogManager, RoleNames.Admin));

    // Política de Visualizador - qualquer usuário autenticado
    options.AddPolicy(PolicyNames.ViewerPolicy, policy =>
        policy.RequireAuthenticatedUser());
});

// Registrar serviço de permissões
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

### Keycloak Setup

Roles are managed in Keycloak and included in JWT tokens via the `roles` claim:

1. **Create Realm Roles** in Keycloak:
   - `admin`
   - `provider-manager`
   - `document-reviewer`
   - `catalog-manager`
   - `viewer`

2. **Create Client Scopes** to include roles in tokens:
   - Add "roles" mapper to include realm roles in JWT

3. **Assign Roles to Users** in Keycloak Admin Console

4. **JWT Token Example**:
```json
{
  "sub": "user-123",
  "name": "João Silva",
  "email": "joao@example.com",
  "roles": ["admin", "provider-manager"],
  "preferred_username": "joao.silva"
}
```

---

## Usage

### Page-Level Authorization

Use `@attribute [Authorize(Policy = "PolicyName")]` on pages:

```csharp
@page "/providers"
@attribute [Authorize(Policy = PolicyNames.ProviderManagerPolicy)]
@using MeAjudaAi.Web.Admin.Authorization
```

**Examples:**

- **Providers.razor**: `ProviderManagerPolicy`
- **Documents.razor**: `DocumentReviewerPolicy`
- **Services.razor, Categories.razor**: `CatalogManagerPolicy`
- **AllowedCities.razor**: `AdminPolicy`
- **Dashboard.razor**: `ViewerPolicy`

### Component-Level Authorization

Use `<AuthorizeView>` component to show/hide UI elements:

```razor
<AuthorizeView Policy="@PolicyNames.ProviderManagerPolicy">
    <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@CreateProvider">
        Novo Provedor
    </MudButton>
</AuthorizeView>
```

**With Roles:**

```razor
<AuthorizeView Roles="@(new[] { RoleNames.Admin, RoleNames.ProviderManager })">
    <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="@Edit" />
</AuthorizeView>
```

**With NotAuthorized Fallback:**

```razor
<AuthorizeView Policy="@PolicyNames.AdminPolicy">
    <Authorized>
        <MudButton Color="Color.Error" OnClick="@DeleteAll">Deletar Tudo</MudButton>
    </Authorized>
    <NotAuthorized>
        <MudText Color="Color.Error">Acesso negado - apenas administradores</MudText>
    </NotAuthorized>
</AuthorizeView>
```

### Programmatic Permission Checks

Inject `IPermissionService` and check permissions in code:

```csharp
@inject IPermissionService PermissionService

@code {
    private bool _canEditProviders;

    protected override async Task OnInitializedAsync()
    {
        _canEditProviders = await PermissionService.HasPermissionAsync(PolicyNames.ProviderManagerPolicy);
    }

    private async Task EditProvider()
    {
        if (!_canEditProviders)
        {
            Snackbar.Add("Você não tem permissão para editar provedores", Severity.Error);
            return;
        }

        // Proceed with edit
    }
}
```

**IPermissionService API:**

```csharp
// Check by policy
bool hasPermission = await PermissionService.HasPermissionAsync(PolicyNames.AdminPolicy);

// Check by role (any)
bool hasRole = await PermissionService.HasAnyRoleAsync(RoleNames.Admin, RoleNames.ProviderManager);

// Check by role (all)
bool hasAllRoles = await PermissionService.HasAllRolesAsync(RoleNames.Admin, RoleNames.CatalogManager);

// Get user's roles
IEnumerable<string> roles = await PermissionService.GetUserRolesAsync();

// Check if admin
bool isAdmin = await PermissionService.IsAdminAsync();
```

### Effects (Fluxor State Management)

Add authorization checks in Effects before API calls:

```csharp
[EffectMethod]
public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
{
    try
    {
        // Verify user has permission to view providers
        var hasPermission = await _permissionService.HasPermissionAsync(PolicyNames.ProviderManagerPolicy);
        if (!hasPermission)
        {
            _logger.LogWarning("User attempted to load providers without proper authorization");
            dispatcher.Dispatch(new LoadProvidersFailureAction("Acesso negado: você não tem permissão para visualizar provedores"));
            return;
        }

        var result = await _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize);
        
        if (result.IsSuccess && result.Value is not null)
        {
            dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value));
        }
        else
        {
            dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error?.Message ?? "Erro ao carregar"));
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading providers");
        dispatcher.Dispatch(new LoadProvidersFailureAction(ex.Message));
    }
}
```

---

## Testing

### Unit Tests

Tests are in `tests/MeAjudaAi.Web.Admin.Tests/Services/PermissionServiceTests.cs`.

**Example Test:**

```csharp
[Fact]
public async Task HasPermissionAsync_WithValidPolicy_ReturnsTrue()
{
    // Arrange
    var user = CreateAuthenticatedUser(RoleNames.Admin);
    var authState = new AuthenticationState(user);
    _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
        .ReturnsAsync(authState);

    _authServiceMock.Setup(x => x.AuthorizeAsync(user, PolicyNames.AdminPolicy))
        .ReturnsAsync(AuthorizationResult.Success());

    // Act
    var result = await _permissionService.HasPermissionAsync(PolicyNames.AdminPolicy);

    // Assert
    Assert.True(result);
}
```

**Run Tests:**

```bash
dotnet test tests/MeAjudaAi.Web.Admin.Tests
```

---

## Security Best Practices

### ✅ Do

1. **Always check permissions in Effects** before making API calls
2. **Use policies on pages** with `@attribute [Authorize(Policy = "...")]`
3. **Hide UI elements** users don't have access to using `<AuthorizeView>`
4. **Log authorization failures** for security auditing
5. **Use specific policies** instead of generic `[Authorize]`
6. **Validate on backend** - client-side checks are for UX only

### ❌ Don't

1. **Don't rely solely on UI hiding** - always validate on backend
2. **Don't hardcode role names** - use `RoleNames` constants
3. **Don't expose sensitive features** without proper authorization checks
4. **Don't skip logging** for security events
5. **Don't use anonymous policies** for sensitive operations

---

## Role Assignment Matrix

| Feature | admin | provider-manager | document-reviewer | catalog-manager | viewer |
|---------|-------|------------------|-------------------|-----------------|--------|
| View Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| Manage Providers | ✅ | ✅ | ❌ | ❌ | ❌ |
| Review Documents | ✅ | ❌ | ✅ | ❌ | ❌ |
| Manage Services | ✅ | ❌ | ❌ | ✅ | ❌ |
| Manage Categories | ✅ | ❌ | ❌ | ✅ | ❌ |
| Manage Cities | ✅ | ❌ | ❌ | ❌ | ❌ |
| System Settings | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## Troubleshooting

### User can't access a page

1. **Check Keycloak role assignment** - verify user has required role
2. **Check JWT token** - use jwt.io to decode and verify "roles" claim
3. **Check browser console** - look for authorization failures
4. **Check logs** - look for "User attempted to... without proper authorization"

### Roles not appearing in token

1. **Check Keycloak client scopes** - ensure "roles" mapper is configured
2. **Check realm roles** - verify roles exist in Keycloak
3. **Check user role assignment** - verify roles are assigned to user
4. **Refresh token** - logout and login again to get new token

### AuthorizeView not working

1. **Check `<CascadingAuthenticationState>`** - must wrap Router in App.razor
2. **Inject IPermissionService** - verify it's registered in Program.cs
3. **Check policy names** - use `PolicyNames` constants, not strings
4. **Check component hierarchy** - AuthenticationState must cascade down

---

## Migration Guide

### Updating Existing Pages

**Before:**
```csharp
@page "/mypage"
@attribute [Authorize]
```

**After:**
```csharp
@page "/mypage"
@attribute [Authorize(Policy = PolicyNames.MyPolicy)]
@using MeAjudaAi.Web.Admin.Authorization
```

### Updating Existing Components

**Before:**
```razor
<MudButton OnClick="@Delete">Deletar</MudButton>
```

**After:**
```razor
<AuthorizeView Policy="@PolicyNames.AdminPolicy">
    <MudButton OnClick="@Delete">Deletar</MudButton>
</AuthorizeView>
```

---

## References

- [ASP.NET Core Authorization](https://learn.microsoft.com/aspnet/core/security/authorization/introduction)
- [Blazor Authentication](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
