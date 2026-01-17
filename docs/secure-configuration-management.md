# Secure Configuration Management for Blazor WASM

## Overview

The MeAjudaAi.Web.Admin Blazor WASM application uses a **secure configuration pattern** that fetches sensitive configuration from the backend API at startup instead of exposing it in publicly accessible `wwwroot/appsettings.json` files.

## Architecture

```text
┌─────────────────────┐
│  Blazor WASM App    │
│  (Browser)          │
│                     │
│  1. Fetch config    │───────┐
│     from backend    │       │
└─────────────────────┘       │
                               │ HTTP GET /api/configuration/client
                               │ (Anonymous - no auth required)
                               │
┌─────────────────────┐       │
│  Backend API        │◄──────┘
│                     │
│  2. Return config   │
│     (non-sensitive) │
│                     │
│  Keycloak:          │
│    Authority        │
│    ClientId         │
│  ApiBaseUrl         │
│  FeatureFlags       │
└─────────────────────┘
```

### Flow

1. **App Startup**: Blazor WASM loads minimal `appsettings.json` (only fallback API URL)
2. **Fetch Configuration**: Makes anonymous HTTP GET to `/api/configuration/client`
3. **Validate**: Validates received configuration (URLs, required fields)
4. **Initialize Services**: Uses backend config to set up OIDC, API clients, etc.
5. **Run Application**: App starts with secure configuration

---

## Configuration Files

### Frontend: wwwroot/appsettings.json

**Minimal, public configuration** (safe to expose):

```json
{
  "ApiBaseUrl": "https://localhost:7001",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**What was removed:**
- ❌ Keycloak Authority URL
- ❌ Keycloak Client ID
- ❌ OAuth scopes
- ❌ Post-logout redirect URIs

**What remains:**
- ✅ Fallback API URL (used only if backend fetch fails)
- ✅ Client-side logging config

---

### Backend: appsettings.json

**Server-side configuration** (not exposed to browser):

```json
{
  "ApiBaseUrl": "https://api.meajudaai.com",
  "ClientBaseUrl": "https://admin.meajudaai.com",
  "Keycloak": {
    "BaseUrl": "https://auth.meajudaai.com",
    "Realm": "meajudaai",
    "ClientId": "meajudaai-api",
    "RequireHttpsMetadata": true,
    "Authority": "https://auth.meajudaai.com/realms/meajudaai"
  },
  "External": {
    "DocumentationUrl": "https://docs.meajudaai.com",
    "SupportUrl": "https://support.meajudaai.com"
  }
}
```

---

## Environment Variables

### Backend API (ApiService)

Configure these environment variables for deployment:

| Variable | Description | Example | Required |
|----------|-------------|---------|----------|
| `ApiBaseUrl` | Public URL of the API | `https://api.meajudaai.com` | ✅ |
| `ClientBaseUrl` | URL of the Blazor WASM app | `https://admin.meajudaai.com` | ✅ |
| `Keycloak__Authority` | Keycloak realm URL | `https://auth.meajudaai.com/realms/meajudaai` | ✅ |
| `Keycloak__ClientId` | Keycloak client ID for admin portal | `admin-portal` | ✅ |
| `Keycloak__BaseUrl` | Keycloak base URL | `https://auth.meajudaai.com` | ✅ |
| `Keycloak__Realm` | Keycloak realm name | `meajudaai` | ✅ |
| `Keycloak__RequireHttpsMetadata` | Enforce HTTPS for Keycloak | `true` (prod), `false` (dev) | ✅ |
| `External__DocumentationUrl` | Help/docs URL | `https://docs.meajudaai.com` | ❌ |
| `External__SupportUrl` | Support portal URL | `https://support.meajudaai.com` | ❌ |

**Environment Variable Syntax:**
- Use `__` (double underscore) for nested properties
- Example: `Keycloak__Authority` → `Keycloak:Authority`

---

## Deployment Configurations

### Development (localhost)

**Backend (ApiService):**
```json
{
  "ApiBaseUrl": "https://localhost:7001",
  "ClientBaseUrl": "http://localhost:5165",
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",
    "ClientId": "admin-portal",
    "RequireHttpsMetadata": false
  }
}
```

**Frontend (Web.Admin wwwroot/appsettings.json):**
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

---

### Staging (Azure Container Apps)

**Backend Environment Variables:**
```bash
ApiBaseUrl=https://api-staging.meajudaai.com
ClientBaseUrl=https://admin-staging.meajudaai.com
Keycloak__Authority=https://auth-staging.meajudaai.com/realms/meajudaai
Keycloak__ClientId=admin-portal-staging
Keycloak__RequireHttpsMetadata=true
```

**Frontend (Web.Admin wwwroot/appsettings.json):**
```json
{
  "ApiBaseUrl": "https://api-staging.meajudaai.com"
}
```

---

### Production (Azure Container Apps)

**Backend Environment Variables:**
```bash
ApiBaseUrl=https://api.meajudaai.com
ClientBaseUrl=https://admin.meajudaai.com
Keycloak__Authority=https://auth.meajudaai.com/realms/meajudaai
Keycloak__ClientId=admin-portal
Keycloak__RequireHttpsMetadata=true
External__DocumentationUrl=https://docs.meajudaai.com
External__SupportUrl=https://support.meajudaai.com
```

**Frontend (Web.Admin wwwroot/appsettings.json):**
```json
{
  "ApiBaseUrl": "https://api.meajudaai.com"
}
```

---

## Configuration Endpoint

### GET /api/configuration/client

**Description**: Returns non-sensitive client configuration for Blazor WASM initialization.

**Authentication**: Anonymous (must be accessible before auth)

**Response Model:**
```json
{
  "apiBaseUrl": "https://api.meajudaai.com",
  "keycloak": {
    "authority": "https://auth.meajudaai.com/realms/meajudaai",
    "clientId": "admin-portal",
    "responseType": "code",
    "scope": "openid profile email",
    "postLogoutRedirectUri": "https://admin.meajudaai.com/"
  },
  "external": {
    "documentationUrl": "https://docs.meajudaai.com",
    "supportUrl": "https://support.meajudaai.com"
  },
  "features": {
    "enableReduxDevTools": false,
    "enableDebugMode": false
  }
}
```

**Error Responses:**
- `500 Internal Server Error`: Missing required configuration (Keycloak:Authority, ClientId)
- `503 Service Unavailable`: Backend not ready

---

## Validation

### Backend Validation

The configuration endpoint validates:
- ✅ `Keycloak:Authority` is configured
- ✅ `Keycloak:ClientId` is configured
- ✅ Throws `InvalidOperationException` if missing

### Frontend Validation

Program.cs validates received configuration:
- ✅ `ApiBaseUrl` is not empty
- ✅ `Keycloak.Authority` is not empty
- ✅ `Keycloak.ClientId` is not empty
- ✅ `Keycloak.PostLogoutRedirectUri` is not empty
- ✅ All URLs are valid absolute URIs

**Error Messages:**

```text
❌❌❌ CONFIGURATION VALIDATION FAILED ❌❌❌

❌ Keycloak Authority is missing
❌ ApiBaseUrl is not a valid absolute URI

Please check your backend configuration and ensure all required settings are properly configured.
```

---

## Security Considerations

### ✅ What is Secure Now

1. **Keycloak URLs not in wwwroot**: Cannot be tampered with by users
2. **Client ID not exposed**: Prevents client spoofing attempts
3. **OAuth scopes from backend**: Centralized scope management
4. **Post-logout URIs from backend**: Prevents open redirect attacks
5. **Feature flags from backend**: Enable/disable features server-side

### ⚠️ What is Still Public (By Design)

1. **Fallback API URL in wwwroot**: Needed for initial bootstrap
   - ✅ Safe: Only used if backend fetch fails
   - ✅ Not sensitive: Just points to API endpoint

2. **Configuration endpoint is anonymous**: Needed before authentication
   - ✅ Safe: Returns only public client config, no secrets
   - ✅ Protected: Validated and controlled by backend

### 🔒 Production Hardening Recommendations

**Rate Limiting & DDoS Protection:**
- ⚠️ **Rate Limiting**: Apply rate limiting (e.g., 100 requests/minute per IP) to prevent DDoS and reconnaissance attacks
- ⚠️ **Caching**: Set appropriate `Cache-Control` headers (e.g., `public, max-age=300`) to reduce backend load
- ⚠️ **Monitoring**: Set up alerts for abnormal access patterns (traffic spikes, unusual geographic sources)
- ⚠️ **CDN/WAF**: Consider placing endpoint behind a CDN with WAF rules for additional protection

---

## Testing

### Local Development

1. Start backend API:
   ```bash
   cd src/Aspire/MeAjudaAi.AppHost
   dotnet run
   ```

2. Verify configuration endpoint:
   ```bash
   curl https://localhost:7001/api/configuration/client
   ```

3. Start Blazor WASM:
   ```bash
   cd src/Web/MeAjudaAi.Web.Admin
   dotnet run
   ```

4. Check browser console for config messages:
   ```text
   🔧 Fetching configuration from: https://localhost:7001/api/configuration/client
   ✅ Configuration loaded successfully
      API Base URL: https://localhost:7001
      Keycloak Authority: http://localhost:8080/realms/meajudaai
      Keycloak Client ID: admin-portal
   ✅ Configuration validation passed
   🚀 Starting MeAjudaAi Admin Portal
   ```

### Integration Tests

Mock the configuration endpoint:

```csharp
[Fact]
public async Task Program_Should_Fetch_Configuration_From_Backend()
{
    // Arrange
    var mockConfig = new ClientConfiguration
    {
        ApiBaseUrl = "https://api.test.com",
        Keycloak = new KeycloakConfiguration
        {
            Authority = "https://auth.test.com/realms/test",
            ClientId = "test-client",
            ResponseType = "code",
            Scope = "openid profile",
            PostLogoutRedirectUri = "https://app.test.com/"
        }
    };

    var mockHttp = new Mock<HttpMessageHandler>();
    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ...)
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(mockConfig)
        });

    // Act & Assert
    // Test configuration fetch logic
}
```

---

## Troubleshooting

### Error: "Cannot connect to the backend API"

**Cause**: Backend API is not running or URL is incorrect

**Solution**:
1. Verify backend is running: `curl https://localhost:7001/api/configuration/client`
2. Check `ApiBaseUrl` in wwwroot/appsettings.json
3. Ensure CORS allows the frontend origin

### Error: "Configuration endpoint returned null"

**Cause**: Backend endpoint not returning proper JSON

**Solution**:
1. Check backend logs for errors
2. Verify `/api/configuration/client` endpoint is registered
3. Test endpoint directly: `curl https://localhost:7001/api/configuration/client`

### Error: "Keycloak Authority is missing"

**Cause**: Backend missing `Keycloak:Authority` configuration

**Solution**:
1. Add to appsettings.json:
   ```json
   {
     "Keycloak": {
       "Authority": "http://localhost:8080/realms/meajudaai"
     }
   }
   ```
2. Or set environment variable: `Keycloak__Authority=...`

---

## Migration Guide

### Before (Insecure)

wwwroot/appsettings.json:
```json
{
  "ApiBaseUrl": "https://localhost:7001",
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",  ← EXPOSED
    "ClientId": "admin-portal"  ← EXPOSED
  }
}
```

### After (Secure)

**Frontend (wwwroot/appsettings.json):**
```json
{
  "ApiBaseUrl": "https://localhost:7001"  ← Only fallback URL
}
```

**Backend (appsettings.json):**
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",  ← SECURE
    "ClientId": "admin-portal"  ← SECURE
  },
  "ClientBaseUrl": "http://localhost:5165"
}
```

**Configuration served via API**: `/api/configuration/client`

---

## Best Practices

1. ✅ **Never store secrets in wwwroot**: Publicly accessible
2. ✅ **Use environment variables for deployment**: Overrides appsettings.json
3. ✅ **Validate configuration on startup**: Fail fast with clear errors
4. ✅ **Log configuration sources**: For debugging (but not values)
5. ✅ **Use HTTPS in production**: Keycloak `RequireHttpsMetadata=true`
6. ✅ **Keep fallback URLs minimal**: Only what's absolutely needed for bootstrap

---

## See Also

- [ASP.NET Core Configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Blazor WASM Security](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/)
- [Keycloak OIDC](https://www.keycloak.org/securing-apps/oidc-layers)
