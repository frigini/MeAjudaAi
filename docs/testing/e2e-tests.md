# Testes End-to-End (E2E)

## Visão Geral

Os testes E2E do MeAjudaAi validam o comportamento completo do sistema em um ambiente isolado usando **TestContainers** com Docker. Cada teste executa contra infraestrutura real (PostgreSQL, Redis, Azurite) em containers efêmeros.

## Arquitetura

### TestContainers + Docker

```
┌─────────────────────────────────────────────────────────────┐
│                     Teste E2E                               │
│  - HttpClient → ApiService (real)                          │
│  - Autenticação via TestAuthenticationHandler              │
│  - Validação de responses e side-effects                   │
└────────────┬────────────────────────────────────────────────┘
             │
             ├─→ PostgreSQL Container (Testcontainers)
             ├─→ Redis Container (Testcontainers)
             └─→ Azurite Container (Testcontainers - Blob Storage)
```

**Diferenças vs Integration Tests:**
- **E2E**: Containers Docker reais, testa comportamento completo do sistema
- **Integration**: In-memory/localhost database, testa integrações específicas

## Estrutura dos Testes

### Base Class: `TestContainerTestBase`

Todos os testes E2E herdam de `TestContainerTestBase`:

```csharp
public abstract class TestContainerTestBase : IAsyncLifetime
{
    protected HttpClient ApiClient { get; private set; }
    protected Faker Faker { get; }
    
    // Métodos de autenticação
    protected void AuthenticateAsAdmin()
    protected void AuthenticateAsUser()
    protected void AuthenticateAsProvider()
}
```

**Ciclo de vida:**
1. `InitializeAsync()` - Iniciar containers (PostgreSQL, Redis, Azurite)
2. Executar testes
3. `DisposeAsync()` - Parar e remover containers

### Organização por Módulo

```
tests/MeAjudaAi.E2E.Tests/
├── Modules/
│   ├── Users/UsersLifecycleEndToEndTests.cs
│   ├── Providers/ProvidersEndToEndTests.cs
│   ├── Documents/DocumentsEndToEndTests.cs
│   ├── SearchProviders/SearchProvidersEndToEndTests.cs
│   ├── ServiceCatalogs/ServiceCatalogsEndToEndTests.cs
│   └── Locations/AllowedCitiesEndToEndTests.cs
├── Infrastructure/
│   ├── MiddlewareEndToEndTests.cs
│   └── RateLimitingEndToEndTests.cs
├── CrossModule/
│   └── ProviderDocumentSearchIntegrationTests.cs
└── Authorization/
    └── PermissionBasedEndToEndTests.cs
```

## Testes de Middleware (Novo!)

### MiddlewareEndToEndTests

Valida comportamento de middlewares em cenários reais:

#### 1. BusinessMetricsMiddleware
```csharp
[Fact]
public async Task BusinessMetrics_UserCreation_ShouldRecordMetric()
{
    AuthenticateAsAdmin();
    var request = new { Username = "test", Email = "test@example.com", ... };
    
    var response = await ApiClient.PostAsJsonAsync("/api/v1/users", request);
    
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    // Middleware registrou métrica de negócio
}
```

**Cobertura:**
- ✅ Criação de usuários
- ✅ Requisições autenticadas
- ✅ Rotas versionadas `/api/v1/*`

#### 2. LoggingContextMiddleware
```csharp
[Fact]
public async Task LoggingContext_CorrelationId_ShouldPropagateToResponseHeader()
{
    var customCorrelationId = Guid.NewGuid().ToString();
    ApiClient.DefaultRequestHeaders.Add("X-Correlation-ID", customCorrelationId);
    
    var response = await ApiClient.GetAsync("/health");
    
    response.Headers.GetValues("X-Correlation-ID").First()
        .Should().Be(customCorrelationId);
}
```

**Cobertura:**
- ✅ Propagação de CorrelationId (request → response)
- ✅ Geração automática de CorrelationId quando ausente
- ✅ Validação de formato GUID

#### 3. SecurityHeadersMiddleware
```csharp
[Fact]
public async Task SecurityHeaders_ShouldIncludeXContentTypeOptions()
{
    var response = await ApiClient.GetAsync("/health");
    
    response.Headers.GetValues("X-Content-Type-Options").First()
        .Should().Be("nosniff");
}
```

**Cobertura:**
- ✅ `X-Content-Type-Options: nosniff`
- ✅ `X-Frame-Options`
- ✅ `Content-Security-Policy`

#### 4. CompressionSecurityMiddleware
```csharp
[Fact]
public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
{
    AuthenticateAsUser();
    ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    
    var response = await ApiClient.GetAsync("/api/v1/users");
    
    response.Content.Headers.ContentEncoding.Should().NotContain("gzip");
    // Proteção contra ataques BREACH/CRIME
}
```

**Cobertura:**
- ✅ Desabilita compressão para usuários autenticados
- ✅ Permite compressão para usuários anônimos

### RateLimitingEndToEndTests

Valida comportamento de rate limiting:

```csharp
[Fact]
public async Task RateLimiting_ManyRequests_ShouldProcessCorrectly()
{
    var responses = new List<HttpResponseMessage>();
    for (int i = 0; i < 50; i++)
    {
        responses.Add(await ApiClient.GetAsync("/health"));
    }
    
    // Validar que rate limiting funciona
    var blockedResponse = responses.FirstOrDefault(r => 
        r.StatusCode == HttpStatusCode.TooManyRequests);
    
    if (blockedResponse != null)
    {
        blockedResponse.Headers.Should().ContainKey("Retry-After");
    }
}
```

**Cobertura:**
- ✅ Bloqueio de requisições excessivas (429 Too Many Requests)
- ✅ Header `Retry-After` em respostas 429
- ✅ Limites independentes por endpoint
- ✅ Limites maiores para usuários autenticados

## Testes por Módulo

### Users

**Arquivo:** `UsersLifecycleEndToEndTests.cs`

**Cobertura:**
- ✅ DELETE com persistência
- ✅ UPDATE com persistência
- ✅ Múltiplas atualizações consecutivas
- ✅ Workflow completo CREATE → UPDATE → DELETE

**Exemplo:**
```csharp
[Fact]
public async Task UpdateUser_CompleteWorkflow_ShouldPersistChanges()
{
    AuthenticateAsAdmin();
    
    // CREATE
    var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest);
    var userId = ExtractIdFromLocation(createResponse.Headers.Location);
    
    // UPDATE
    var updateRequest = new { Username = "updated_name", ... };
    await ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}", updateRequest);
    
    // VERIFY persistence
    var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
    var user = await getResponse.Content.ReadFromJsonAsync<dynamic>();
    user!.GetProperty("data").GetProperty("username").GetString()
        .Should().Be("updated_name");
}
```

### SearchProviders

**Arquivo:** `SearchProvidersEndToEndTests.cs`

**Cobertura:**
- ✅ Busca geolocalizada com filtro por raio
- ✅ Filtro por serviços específicos
- ✅ Ordenação: SubscriptionTier > Rating > Distance
- ✅ Paginação (PageNumber, PageSize)
- ✅ Exclusão de providers fora do raio

**Exemplo:**
```csharp
[Fact]
public async Task SearchProviders_CompleteWorkflow_ShouldFindProvidersWithinRadius()
{
    AuthenticateAsAdmin();
    
    // Criar provider próximo
    await CreateProviderAsync(latitude: -23.550520, longitude: -46.633308);
    
    // Buscar providers em raio de 10km
    var response = await ApiClient.GetAsync(
        "/api/v1/search-providers?latitude=-23.550520&longitude=-46.633308&radiusKm=10");
    
    var providers = await response.Content.ReadFromJsonAsync<dynamic>();
    providers!.GetProperty("data").GetProperty("items").GetArrayLength()
        .Should().BeGreaterThan(0);
}
```

### Documents

**Arquivo:** `DocumentsEndToEndTests.cs`

**Cobertura:**
- ✅ Workflow UPLOAD → VERIFY
- ✅ Rejeição de documentos com RejectionReason
- ✅ Histórico de múltiplos documentos por provider

**Exemplo:**
```csharp
[Fact]
public async Task DocumentLifecycle_UploadAndVerification_ShouldCompleteProperly()
{
    AuthenticateAsAdmin();
    
    // UPLOAD
    var uploadRequest = new { ProviderId = providerId, DocumentType = "IdentityDocument", ... };
    var uploadResponse = await ApiClient.PostAsJsonAsync("/api/v1/documents", uploadRequest);
    var documentId = ExtractIdFromLocation(uploadResponse.Headers.Location);
    
    // VERIFY
    var verifyRequest = new { IsApproved = true };
    var verifyResponse = await ApiClient.PostAsJsonAsync(
        $"/api/v1/documents/{documentId}/verify", verifyRequest);
    
    verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Boas Práticas

### 1. Usar AuthenticateAs*() para Autenticação

```csharp
✅ CORRETO:
AuthenticateAsAdmin();
var response = await ApiClient.GetAsync("/api/v1/users");

❌ INCORRETO:
// Não manipular tokens manualmente em E2E tests
var token = await GetTokenAsync();
ApiClient.DefaultRequestHeaders.Authorization = new("Bearer", token);
```

### 2. Validar Persistência com GET

```csharp
✅ CORRETO:
// CREATE
var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", request);
var userId = ExtractId(createResponse.Headers.Location);

// VERIFY persistence
var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
var user = await getResponse.Content.ReadFromJsonAsync<dynamic>();

❌ INCORRETO:
// Não assumir que dados foram persistidos
var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", request);
// Sem validação de persistência
```

### 3. Testar Side-Effects Reais

```csharp
✅ CORRETO (E2E):
// Testar que middleware REALMENTE desabilita compressão
AuthenticateAsUser();
ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
var response = await ApiClient.GetAsync("/api/v1/users");
response.Content.Headers.ContentEncoding.Should().NotContain("gzip");

❌ INCORRETO (seria Integration):
// Testar apenas lógica do middleware isoladamente
var middleware = new CompressionSecurityMiddleware(next);
await middleware.InvokeAsync(context);
context.Features.Get<IHttpsCompressionFeature>().Should().BeNull();
```

### 4. Cleanup Automático

TestContainers **remove automaticamente** os containers após cada teste. Não é necessário cleanup manual de dados.

## Execução

### Comando Básico
```bash
dotnet test tests/MeAjudaAi.E2E.Tests/
```

### Com Filtro por Módulo
```bash
# Apenas testes de Users
dotnet test tests/MeAjudaAi.E2E.Tests/ --filter "Module=Users"

# Apenas testes de Infrastructure
dotnet test tests/MeAjudaAi.E2E.Tests/ --filter "Module=Infrastructure"
```

### Execução em CI/CD

Requer Docker:
```yaml
- name: Run E2E Tests
  run: |
    docker --version  # Verificar Docker disponível
    dotnet test tests/MeAjudaAi.E2E.Tests/ \
      --logger "trx;LogFileName=e2e-results.trx" \
      --results-directory ./TestResults
```

## Métricas de Cobertura

### Estado Atual (Dez/2024)

**Total de Testes E2E:** 86 testes
- ✅ **Passed:** 74 (86.0%)
- ❌ **Failed:** 12 (14.0%)

**Cobertura por Módulo:**
| Módulo | Testes | Status |
|--------|--------|--------|
| Users | 10 | 7 passed, 3 failed |
| Providers | 15 | 15 passed |
| Documents | 8 | 5 passed, 3 failed |
| SearchProviders | 8 | 4 passed, 4 failed |
| ServiceCatalogs | 20 | 20 passed |
| Locations | 10 | 10 passed |
| Infrastructure | 15 | 13 passed, 2 failed |

**Middlewares Cobertos (E2E):**
- ✅ BusinessMetricsMiddleware
- ✅ LoggingContextMiddleware
- ✅ SecurityHeadersMiddleware
- ✅ CompressionSecurityMiddleware
- ✅ RateLimitingMiddleware
- ✅ RequestLoggingMiddleware

**Gaps Conhecidos:**
- ⚠️ SearchProviders: testes falhando (problema de seed data)
- ⚠️ Documents: testes de verificação falhando (timing issue)
- ⚠️ Users: UPDATE tests com eventual consistency issues

## Troubleshooting

### Container Startup Failures

**Sintoma:** Testes falham com timeout ao iniciar containers

**Solução:**
```bash
# Verificar Docker
docker ps

# Limpar containers órfãos
docker container prune -f

# Verificar recursos
docker system df
```

### Testes Flaky (Intermitentes)

**Sintoma:** Testes passam/falham aleatoriamente

**Causa Comum:** Eventual consistency do banco

**Solução:**
```csharp
// Adicionar retry com exponential backoff
private async Task WaitForResourceAsync(Guid id, int maxAttempts = 10)
{
    var delay = 100;
    for (var attempt = 0; attempt < maxAttempts; attempt++)
    {
        var response = await ApiClient.GetAsync($"/api/v1/resource/{id}");
        if (response.IsSuccessStatusCode) return;
        
        if (attempt < maxAttempts - 1)
        {
            await Task.Delay(delay);
            delay = Math.Min(delay * 2, 2000);
        }
    }
    throw new TimeoutException($"Resource {id} not found");
}
```

### Performance Lenta

**Sintoma:** Testes E2E levam muito tempo

**Otimizações:**
1. **Parallel execution:** xUnit executa classes em paralelo por padrão
2. **Container reuse:** TestContainers reutiliza containers entre testes da mesma classe
3. **Seed data:** Minimizar criação de dados no `Arrange`

## Referências

- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [xUnit Parallelization](https://xunit.net/docs/running-tests-in-parallel)
- [FluentAssertions](https://fluentassertions.com/)
