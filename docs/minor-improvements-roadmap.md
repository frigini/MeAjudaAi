# Minor Improvements Roadmap - Blazor Admin Portal

## Overview
Este documento lista melhorias menores sugeridas para as partes 1-5 das implementações do Blazor Admin Portal. Algumas são implementações rápidas, outras requerem planejamento de sprint futuro.

**Status Geral**: Sprint 6 concluída, reorganização para sprints futuras (7+)

---

## ✅ Part 1: FluentValidation (Completed)

### Suggestion 1: Async Validators for CPF/CNPJ Uniqueness ⏳ Backlog

**Status**: Postergado - Dependente de integração API Receita Federal  
**Prioridade**: Baixa (não crítico para MVP)  
**Sprint Estimado**: Backlog (quando integração externa for priorizada)

#### Contexto
Atualmente a validação de CPF/CNPJ é apenas sintática (dígitos verificadores). Para produção, precisamos verificar:
1. Se o CPF/CNPJ já está cadastrado no sistema (unicidade)
2. Se o CPF/CNPJ é válido na Receita Federal (autenticidade)

#### Implementação Proposta

**1. Validação de Unicidade no Banco (Implementar Agora)**
```csharp
// src/Web/MeAjudaAi.Web.Admin/Validators/UniqueCpfCnpjValidator.cs
public class UniqueCpfCnpjValidator : AbstractValidator<string>
{
    public UniqueCpfCnpjValidator(IProvidersApi providersApi)
    {
        RuleFor(doc => doc)
            .MustAsync(async (document, cancellationToken) =>
            {
                var result = await providersApi.CheckDocumentUniquenessAsync(document);
                return result.IsSuccess && result.Value.IsUnique;
            })
            .WithMessage("Este CPF/CNPJ já está cadastrado no sistema");
    }
}

// Uso em CreateProviderRequestDtoValidator
RuleFor(x => x.BusinessProfile.Document.Number)
    .SetAsyncValidator(new UniqueCpfCnpjValidator(_providersApi))
    .When(x => x.BusinessProfile?.Document != null);
```

**2. Validação na Receita Federal (Sprint 7-8)**
```csharp
// src/Modules/Providers/Infrastructure/ExternalServices/ReceitaFederalClient.cs
public interface IReceitaFederalClient
{
    Task<Result<CpfValidationDto>> ValidateCpfAsync(string cpf);
    Task<Result<CnpjValidationDto>> ValidateCnpjAsync(string cnpj);
}

// Configuração com Circuit Breaker e cache agressivo
services.AddHttpClient<IReceitaFederalClient, ReceitaFederalClient>()
    .AddPolicyHandler(GetReceitaFederalPolicy()) // Retry + circuit breaker
    .AddPolicyHandler(Policy.CacheAsync<HttpResponseMessage>(
        TimeProvider.System.GetTimestamp() + TimeSpan.FromHours(24))); // Cache 24h
```

**Considerações**:
- API pública da Receita tem rate limits agressivos
- Alternativas: BrasilAPI, ReceitaWS (serviços terceiros)
- Cache mínimo de 24h para evitar re-consultas
- Validação assíncrona só para create/update, não para list/search
- Implementar fallback: se API estiver indisponível, aceitar documento (validar depois)

**Referências**:
- Ver [docs/future-external-services.md](future-external-services.md#api-receita-federal-cnpjcpf)
- Issue: `#TODO-CREATE-ASYNC-VALIDATORS`

---

### Suggestion 2: Unit Tests for ValidationExtensions ✅ Completed

**Status**: ✅ Implementado  
**Localização**: `tests/MeAjudaAi.Shared.Tests/Unit/Extensions/DocumentExtensionsTests.cs`

#### Cobertura Atual (Completa)
Os testes já cobrem todos os edge cases necessários:

**CPF Tests**:
- ✅ CPFs válidos com/sem formatação
- ✅ Todos zeros (000.000.000-00)
- ✅ Dígitos repetidos (111.111.111-11, 222.222.222-22, etc.)
- ✅ Dígitos verificadores inválidos
- ✅ Tamanho incorreto
- ✅ Valores null/empty/whitespace
- ✅ Caracteres não numéricos

**CNPJ Tests**:
- ✅ CNPJs válidos com/sem formatação
- ✅ Todos zeros (00.000.000/0000-00)
- ✅ Dígitos repetidos (11.111.111/1111-11, etc.)
- ✅ Dígitos verificadores inválidos
- ✅ Tamanho incorreto
- ✅ Valores null/empty/whitespace
- ✅ Caracteres não numéricos

**Testes de Geração**:
- ✅ Gera CPFs válidos
- ✅ Gera CNPJs válidos
- ✅ Valores gerados são diferentes (não determin ísticos)

#### Métricas
- **Cobertura de Código**: ~100% para DocumentExtensions
- **Total de Testes**: 23 testes unitários
- **Casos de Borda**: Todos cobertos

**Nenhuma ação adicional necessária.**

---

## 🔧 Part 2: Centralized Configuration

### Suggestion 1: Configuration Refresh Capability ⏳ Backlog

**Status**: Postergado - Não crítico, complexidade alta vs benefício  
**Prioridade**: Baixa  
**Sprint Estimado**: Backlog (DevEx improvements)

#### Contexto
Atualmente a configuração é buscada apenas no startup do Blazor WASM. Para ambientes de desenvolvimento/staging, seria útil recarregar configuração sem full refresh.

#### Implementação Proposta

**1. Criar serviço de configuração recarregável**
```csharp
// src/Web/MeAjudaAi.Web.Admin/Services/ConfigurationReloadService.cs
public interface IConfigurationReloadService
{
    Task<Result<ClientConfiguration>> ReloadConfigurationAsync();
    event EventHandler<ClientConfiguration>? ConfigurationChanged;
    ClientConfiguration CurrentConfiguration { get; }
}

public class ConfigurationReloadService : IConfigurationReloadService
{
    private readonly HttpClient _httpClient;
    private ClientConfiguration _currentConfig;
    
    public event EventHandler<ClientConfiguration>? ConfigurationChanged;
    public ClientConfiguration CurrentConfiguration => _currentConfig;
    
    public async Task<Result<ClientConfiguration>> ReloadConfigurationAsync()
    {
        try
        {
            var newConfig = await _httpClient.GetFromJsonAsync<ClientConfiguration>(
                "/api/configuration/client");
            
            if (newConfig != null && !newConfig.Equals(_currentConfig))
            {
                var oldConfig = _currentConfig;
                _currentConfig = newConfig;
                ConfigurationChanged?.Invoke(this, newConfig);
                
                // Log mudanças
                LogConfigChanges(oldConfig, newConfig);
            }
            
            return Result<ClientConfiguration>.Success(newConfig);
        }
        catch (Exception ex)
        {
            return Result<ClientConfiguration>.Failure(Error.Internal(ex.Message));
        }
    }
}
```

**2. Adicionar botão de reload no DevTools (apenas development)**
```razor
@* src/Web/MeAjudaAi.Web.Admin/Components/DevTools/ConfigReloadButton.razor *@
@if (Environment.IsDevelopment)
{
    <MudButton OnClick="ReloadConfig" 
               StartIcon="@Icons.Material.Filled.Refresh"
               Variant="Variant.Outlined">
        Reload Config
    </MudButton>
}

@code {
    [Inject] private IConfigurationReloadService ConfigService { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    
    private async Task ReloadConfig()
    {
        var result = await ConfigService.ReloadConfigurationAsync();
        if (result.IsSuccess)
            Snackbar.Add("Configuração recarregada!", Severity.Success);
        else
            Snackbar.Add($"Erro: {result.Error.Message}", Severity.Error);
    }
}
```

**Limitações**:
- Algumas configurações requerem reinicialização de serviços (ex: OIDC)
- API clients precisariam ser recriados com nova BaseUrl
- Feature toggles podem ser recarregados dinamicamente
- **Recomendação**: Implementar apenas para feature flags, não para URLs/autenticação

**Alternativa Simples** (Implementar Agora):
```csharp
// Adicionar endpoint de debug para forçar reload
// src/Modules/Configuration/API/Endpoints/ReloadConfigEndpoint.cs
[RequireHost("localhost")] // Apenas em dev
public static IResult ReloadConfig(IConfiguration config)
{
    (config as IConfigurationRoot)?.Reload();
    return Results.Ok("Configuration reloaded");
}
```

---

### Suggestion 2: Document Environment Variable Overrides ✅ Priority

**Status**: Implementar Agora  
**Prioridade**: Alta  
**Complexidade**: Baixa (documentação)

#### Implementação

**1. Criar guia de deployment**
```markdown
<!-- docs/deployment/environment-variables.md -->
# Environment Variables - Deployment Guide

## Overview
Todas as configurações podem ser sobrescritas via variáveis de ambiente em produção.

## Hierarquia de Configuração
1. appsettings.json (padrões)
2. appsettings.{Environment}.json
3. Azure Key Vault (secrets)
4. **Environment Variables** (maior prioridade)

## Formato de Variáveis

### Convenção .NET
Use `__` (dois underscores) para níveis aninhados:
```bash
Keycloak__Authority=https://keycloak.prod.example.com
Keycloak__ClientId=meajudaai-admin
ApiBaseUrl=https://api.prod.example.com
```

### Docker Compose
```yaml
environment:
  - Keycloak__Authority=https://keycloak.staging.local
  - Features__EnableReduxDevTools=false
```

### Azure App Service
```bash
# Configuração → Application Settings
Keycloak__Authority = https://keycloak.azure.com
Keycloak__PostLogoutRedirectUri = https://admin.azure.com
```

### Kubernetes
```yaml
env:
  - name: Keycloak__Authority
    valueFrom:
      configMapKeyRef:
        name: app-config
        key: keycloak-authority
```

## Variáveis Críticas (Obrigatórias em Produção)

| Variável | Exemplo | Descrição |
|----------|---------|-----------|
| `ApiBaseUrl` | `https://api.prod.com` | URL da API backend |
| `Keycloak__Authority` | `https://auth.prod.com` | Keycloak realm URL |
| `Keycloak__ClientId` | `admin-portal` | Client ID OIDC |
| `Keycloak__PostLogoutRedirectUri` | `https://admin.prod.com` | Redirect após logout |

## Variáveis Opcionais

| Variável | Default | Prod Recommendation |
|----------|---------|---------------------|
| `Features__EnableReduxDevTools` | `true` | `false` |
| `Keycloak__Scope` | `openid profile email` | `+ custom-scope` |

## Validação de Configuração

O app valida configuração no startup:
```csharp
ValidateConfiguration(clientConfig);
// Lança InvalidOperationException se faltar configuração crítica
```

## Exemplos Completos

### Docker Production
```dockerfile
ENV ApiBaseUrl=https://api.meajudaai.com \
    Keycloak__Authority=https://auth.meajudaai.com \
    Keycloak__ClientId=admin-portal \
    Features__EnableReduxDevTools=false
```

### Azure App Service
Ver [deployment/azure-app-service.md](azure-app-service.md#configuration)

### Kubernetes
Ver [infrastructure/k8s/configmap.yaml](../../infrastructure/k8s/configmap.yaml)
```

**2. Adicionar seção no README principal**
```markdown
## Configuration
See [docs/deployment/environment-variables.md](docs/deployment/environment-variables.md) for:
- Environment variable naming conventions
- Production deployment examples
- Docker/Kubernetes/Azure configuration
```

---

## 🔐 Part 4: Authorization & Fluxor

### Suggestion 1: Integration Tests for Authorization ⏳ Backlog

**Status**: Postergado - Baixa prioridade vs esforço  
**Prioridade**: Baixa (testes E2E cobrem casos principais)  
**Sprint Estimado**: Backlog

#### Implementação Proposta

**1. Criar fixtures de teste com roles**
```csharp
// tests/MeAjudaAi.Web.Admin.IntegrationTests/Fixtures/AuthorizationFixture.cs
public class AuthorizationTestFixture : IDisposable
{
    public HttpClient AdminClient { get; }
    public HttpClient ProviderManagerClient { get; }
    public HttpClient UnauthorizedClient { get; }
    
    public AuthorizationTestFixture()
    {
        var factory = new WebApplicationFactory<Program>();
        
        // Criar clients com diferentes claims
        AdminClient = factory.CreateClient(options =>
        {
            options.DefaultRequestHeaders.Authorization = 
                CreateBearerToken(roles: ["SystemAdmin"]);
        });
        
        ProviderManagerClient = factory.CreateClient(options =>
        {
            options.DefaultRequestHeaders.Authorization = 
                CreateBearerToken(roles: ["ProviderManager"]);
        });
    }
}
```

**2. Testes de autorização por endpoint**
```csharp
// tests/MeAjudaAi.Web.Admin.IntegrationTests/Authorization/ProvidersAuthorizationTests.cs
public class ProvidersAuthorizationTests : IClassFixture<AuthorizationTestFixture>
{
    [Fact]
    public async Task LoadProviders_WithProviderManagerRole_ShouldSucceed()
    {
        // Arrange
        var state = new ProvidersState();
        var action = new LoadProvidersAction();
        
        // Act
        var effect = new ProvidersEffects(
            _fixture.ProviderManagerApiClient,
            _fixture.PermissionService,
            _snackbar,
            _logger);
        
        await effect.HandleLoadProvidersAction(action, _dispatcher);
        
        // Assert
        _dispatcher.Verify(d => d.Dispatch(
            It.IsAny<LoadProvidersSuccessAction>()), Times.Once);
    }
    
    [Fact]
    public async Task LoadProviders_WithoutRole_ShouldDispatchFailure()
    {
        // Testa que usuário sem role recebe acesso negado
    }
}
```

**3. Testes E2E com Playwright**
```csharp
// tests/MeAjudaAi.E2E.Tests/Authorization/ProviderManagementE2ETests.cs
[Test]
public async Task ProviderManager_CanAccessProvidersList()
{
    await LoginAs(role: "ProviderManager");
    await Page.GotoAsync("/providers");
    
    // Deve ver a lista
    await Expect(Page.Locator("[data-testid='providers-table']"))
        .ToBeVisibleAsync();
}

[Test]
public async Task RegularUser_CannotAccessProvidersList()
{
    await LoginAs(role: "User");
    await Page.GotoAsync("/providers");
    
    // Deve ver mensagem de acesso negado
    await Expect(Page.Locator("text='Acesso negado'"))
        .ToBeVisibleAsync();
}
```

**Cobertura de Testes Necessária**:
- ✅ ProvidersEffects: Verificação de PolicyNames.ProviderManagerPolicy **já implementado**
- ⏳ ServiceCatalogsEffects: Verificar políticas antes de API calls
- ⏳ LocationsEffects: Verificar políticas
- ⏳ DocumentsEffects: Verificar upload permissions
- ⏳ Testes E2E para fluxos de navegação baseados em roles

---

## 🛡️ Part 5: Security Headers & CSP

### Suggestion 1: Nonce-based CSP ⏳ Backlog

**Status**: Postergado - Incompatível com Blazor WASM, requer Server-Side Rendering  
**Prioridade**: Baixa  
**Sprint Estimado**: Backlog (quando migrar para Blazor Server ou .NET 10 SSR)

#### Contexto
Atualmente usamos `'unsafe-inline'` para styles do MudBlazor. Para produção, nonce-based CSP é mais seguro.

#### Implementação Proposta

**1. Gerar nonce por requisição (Server-side)**
```csharp
// src/Modules/WebApi/Middleware/CspNonceMiddleware.cs
public class CspNonceMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Gera nonce único por request
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items["csp-nonce"] = nonce;
        
        // Injeta no CSP header
        context.Response.Headers["Content-Security-Policy"] = 
            $"default-src 'self'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            $"style-src 'self' 'nonce-{nonce}';";
        
        await next(context);
    }
}
```

**2. Injetar nonce no HTML (Razor Pages/Blazor Server)**
```html
<!-- wwwroot/index.html ou _Host.cshtml -->
<style nonce="@Context.Items["csp-nonce"]">
    /* MudBlazor dynamic styles */
</style>

<script nonce="@Context.Items["csp-nonce"]">
    // Blazor boot script
</script>
```

**Problema com Blazor WASM**:
- Blazor WASM é **client-side only**, sem server-side rendering
- Nonce precisa ser gerado pelo servidor (único por request)
- **Solução**: Mover para Blazor Server ou Blazor United (.NET 8+)

**Alternativa para WASM**:
```csharp
// Usar hashes ao invés de nonce para scripts/styles estáticos
Content-Security-Policy: 
    script-src 'self' 'sha256-{hash-of-blazor-boot-script}';
    style-src 'self' 'sha256-{hash-of-mudblazor-styles}';
```

**Limitações**:
- MudBlazor gera styles dinâmicos em runtime → difícil calcular hash
- Requer refatoração para Blazor Server ou Server-Side Rendering
- **Recomendação**: Aguardar .NET 10 Blazor United estabilizar

**Issue**: `#TODO-CSP-NONCE-BLAZOR-SERVER`

---

### Suggestion 2: CSP Violation Monitoring ⏳ Sprint 6

**Status**: Planejado  
**Prioridade**: Alta  
**Complexidade**: Baixa

#### Implementação Proposta

**1. Adicionar report-uri ao CSP header**
```csharp
// src/Modules/WebApi/Extensions/SecurityHeadersExtensions.cs
context.Response.Headers["Content-Security-Policy"] = 
    "default-src 'self'; " +
    "script-src 'self' https://trusted-cdn.com; " +
    "report-uri /api/csp/violations; " +  // Endpoint interno
    "report-to csp-endpoint";              // Reporting API v1
```

**2. Criar endpoint de CSP violations**
```csharp
// src/Modules/WebApi/Endpoints/CspViolationEndpoint.cs
public static class CspViolationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/csp/violations", HandleCspViolation)
            .AllowAnonymous(); // CSP reports vêm do browser sem auth
    }
    
    private static async Task<IResult> HandleCspViolation(
        HttpContext context,
        ILogger<CspViolationEndpoint> logger,
        [FromServices] ITelemetryClient telemetry)
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var violation = JsonSerializer.Deserialize<CspViolationReport>(body);
        
        // Log estruturado
        logger.LogWarning(
            "CSP Violation: {BlockedUri} violated {ViolatedDirective}. " +
            "Document: {DocumentUri}, Source: {SourceFile}:{LineNumber}",
            violation.BlockedUri,
            violation.ViolatedDirective,
            violation.DocumentUri,
            violation.SourceFile,
            violation.LineNumber);
        
        // Enviar para Application Insights
        telemetry.TrackEvent("CspViolation", new Dictionary<string, string>
        {
            ["BlockedUri"] = violation.BlockedUri,
            ["ViolatedDirective"] = violation.ViolatedDirective,
            ["UserAgent"] = context.Request.Headers.UserAgent
        });
        
        return Results.NoContent();
    }
}

public record CspViolationReport(
    string DocumentUri,
    string BlockedUri,
    string ViolatedDirective,
    string? SourceFile,
    int? LineNumber);
```

**3. Configurar alertas no Application Insights**
```kusto
// Query KQL para dashboard
customEvents
| where name == "CspViolation"
| summarize ViolationCount = count() by BlockedUri, ViolatedDirective
| order by ViolationCount desc
| take 20
```

**Alertas**:
- Email se violation rate > 100/hour
- Slack webhook para violations críticas (ex: script de domínio malicioso)

---

### Suggestion 3: CSP Testing with DevTools 📝 Documentation

**Status**: Implementar Agora (documentação)  
**Prioridade**: Alta  
**Complexidade**: Baixa

#### Criar guia de testes

```markdown
<!-- docs/security/csp-testing-guide.md -->
# CSP Testing Guide

## Overview
Como testar Content Security Policy com Chrome/Edge DevTools para garantir que nenhum recurso legítimo foi bloqueado.

## Step-by-Step Testing

### 1. Abrir DevTools
- F12 ou Ctrl+Shift+I
- Tab **Console**

### 2. Ativar CSP Violations
- Settings (⚙️) → Console → ✅ Show violations

### 3. Testar Fluxos Principais
1. Login/Logout
2. Navegação entre páginas (Providers, Service Catalogs, Locations)
3. Upload de documentos
4. Formulários (Create Provider, Add Document)

### 4. Verificar Violations
**Violations legítimas** (OK, podem ignorar):
```
[Report Only] Refused to load script 'chrome-extension://...'
```
Extensões do Chrome são sempre bloqueadas, esperado.

**Violations críticas** (CORRIGIR):
```
Refused to load stylesheet 'https://cdn.mudblazor.com/...'
Violated directive: style-src 'self'
```
Adicionar `https://cdn.mudblazor.com` ao CSP.

### 5. Testar MudBlazor Themes
- Trocar dark/light mode
- Verificar se nenhum style inline foi bloqueado
- Snackbars, Dialogs, Tooltips devem funcionar

### 6. Checklist de Recursos

| Recurso | Testado | Status |
|---------|---------|--------|
| MudBlazor CSS | ☐ | |
| Fluxor ReduxDevTools | ☐ | |
| Blazor boot script | ☐ | |
| OIDC redirect | ☐ | |
| API calls (fetch) | ☐ | |
| File uploads | ☐ | |

## Automated Testing

### Playwright CSP Test
```csharp
[Test]
public async Task AllPages_ShouldNotHaveCspViolations()
{
    var violations = new List<string>();
    
    Page.Console += (_, msg) =>
    {
        if (msg.Text.Contains("Content Security Policy"))
            violations.Add(msg.Text);
    };
    
    await Page.GotoAsync("/providers");
    await Page.GotoAsync("/service-catalogs");
    
    Assert.IsEmpty(violations, 
        $"CSP violations detected: {string.Join("\n", violations)}");
}
```

## Production Monitoring
Ver [csp-violation-monitoring.md](csp-violation-monitoring.md) para configuração de alertas.
```

---

## Summary

| Suggestion | Status | Priority | Sprint | Complexity |
|------------|--------|----------|--------|------------|
| Part 1: Async CPF/CNPJ validators | ⏳ Planned | Medium | 7-8 | Medium |
| Part 1: Unit tests for ValidationExtensions | ✅ Done | N/A | N/A | N/A |
| Part 2: Config refresh capability | ⏳ Planned | Low | 6 | Medium |
| Part 2: Document env var overrides | 📝 TODO | High | Current | Low |
| Part 4: Authorization integration tests | ⏳ Planned | High | 6 | Medium-High |
| Part 5: Nonce-based CSP | ⏳ Planned | Medium | 7 | High |
| Part 5: CSP violation monitoring | ⏳ Planned | High | 6 | Low |
| Part 5: CSP DevTools testing guide | 📝 TODO | High | Current | Low |

### Immediate Actions (This Sprint)
1. ✅ Document environment variable overrides
2. ✅ Create CSP testing guide

### Next Sprint (6)
1. ⏳ Implement configuration reload service (feature flags only)
2. ⏳ Add CSP violation monitoring endpoint
3. ⏳ Create authorization integration tests

### Future (Sprint 7-8)
1. ⏳ Implement async CPF/CNPJ validators with Receita Federal API
2. ⏳ Evaluate Blazor Server migration for nonce-based CSP
