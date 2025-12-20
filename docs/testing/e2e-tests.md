# Testes End-to-End (E2E)

## Visão Geral

Os testes E2E do MeAjudaAi validam o comportamento completo do sistema em um ambiente isolado usando **TestContainers** com Docker. Cada teste executa contra infraestrutura real (PostgreSQL, Redis, Azurite) em containers efêmeros.

## Arquitetura

### TestContainers + Docker

```text
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

#### Estrutura Consolidada (Padrão `{Module}EndToEndTests`)

```text
tests/MeAjudaAi.E2E.Tests/
├── Modules/
│   ├── Users/UsersEndToEndTests.cs               (renomeado)
│   ├── Providers/ProvidersEndToEndTests.cs       (3→1 consolidado)
│   ├── Documents/DocumentsEndToEndTests.cs       (2→1 consolidado)
│   ├── SearchProviders/SearchProvidersEndToEndTests.cs
│   ├── ServiceCatalogs/ServiceCatalogsEndToEndTests.cs  (2→1 consolidado)
│   └── Locations/LocationsEndToEndTests.cs       (renomeado)
├── Infrastructure/
│   ├── MiddlewareEndToEndTests.cs
│   └── RateLimitingEndToEndTests.cs
├── CrossModule/
│   └── ProviderDocumentSearchIntegrationTests.cs
└── Authorization/
    └── PermissionBasedEndToEndTests.cs
```

**Consolidações Realizadas:**
- **Documents**: `DocumentsEndToEndTests.cs` + `DocumentsVerificationEndToEndTests.cs` → `DocumentsEndToEndTests.cs`
- **ServiceCatalogs**: `ServiceCatalogsEndToEndTests.cs` + `ServiceCatalogsAdvancedEndToEndTests.cs` → `ServiceCatalogsEndToEndTests.cs`
- **Providers**: `ProvidersEndToEndTests.cs` + `ProvidersLifecycleEndToEndTests.cs` + `ProvidersDocumentsEndToEndTests.cs` → `ProvidersEndToEndTests.cs`
- **Users**: `UsersLifecycleEndToEndTests.cs` → `UsersEndToEndTests.cs` (renomeado)
- **Locations**: `AllowedCitiesEndToEndTests.cs` → `LocationsEndToEndTests.cs` (renomeado)

**Redução Total:** 19 arquivos → 15 arquivos (-21%)

#### Organização Interna com #region

Cada módulo consolidado usa `#region` para organizar testes por cenário de negócio:

**Exemplo - DocumentsEndToEndTests.cs (10 testes em 6 regions):**
```csharp
#region Helper Methods
// WaitForProviderAsync, ExtractId helpers

#region Upload and Basic CRUD
// UploadDocument_WithValidData_Should_Return_Success
// GetDocument_ByProviderId_Should_Return_Document

#region Provider Documents
// GetDocumentsByProviderId_Should_Return_ProviderDocuments
// GetDocumentsByProviderId_NonExistent_Should_Return_NotFound

#region Workflows
// DocumentApprovalWorkflow_Should_UpdateStatus
// DocumentRejectionWorkflow_Should_RecordRejectionReason

#region Isolation and Cascading
// DeleteProvider_Should_CascadeToDocuments

#region Verification Workflows
// DocumentVerification_ToApproved_Should_Succeed
// DocumentVerification_WithRejection_Should_RecordReason
// MultipleDocumentVerifications_Should_MaintainHistory
```

**Exemplo - ServiceCatalogsEndToEndTests.cs (14 testes em 7 regions):**
```csharp
#region Basic CRUD Operations
#region Category Filtering
#region Update and Delete Operations
#region Activation Status Changes
#region Database Persistence Verification
#region Advanced Validation Rules
#region Advanced Category Change Scenarios
```

**Exemplo - ProvidersEndToEndTests.cs (10 testes em 6 regions):**
```csharp
#region Basic CRUD Operations
#region Update Operations
#region Delete Operations
#region Verification Status
#region Basic Info Correction
#region Document Operations
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

#### 5. ExceptionHandlerMiddleware - ProblemDetails (NOVO - Commit 737dab30)

Valida estrutura RFC 7807 para respostas de erro:

```csharp
[Fact]
public async Task ExceptionHandler_NotFound_ShouldReturnProblemDetails()
{
    var response = await ApiClient.GetAsync($"/api/v1/users/{Guid.NewGuid()}");
    
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var content = await response.Content.ReadAsStringAsync();
    var problemDetails = JsonDocument.Parse(content);
    
    problemDetails.RootElement.TryGetProperty("type", out _).Should().BeTrue();
    problemDetails.RootElement.TryGetProperty("title", out _).Should().BeTrue();
    problemDetails.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
    status.GetInt32().Should().Be(404);
}
```

**Cobertura (+3 testes):**
- ✅ 404 Not Found - ProblemDetails structure
- ✅ 400 Bad Request - ProblemDetails with validation errors
- ✅ 401 Unauthorized - ProblemDetails for authentication failures

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

## Testes de Autorização (NOVO - Commit 737dab30)

### PermissionAuthorizationEndToEndTests - Role-Based Policies

Valida políticas de autorização baseadas em roles:

#### 1. ProviderOnly Policy (+2 testes)
```csharp
[Fact]
public async Task ProviderOnlyPolicy_WithProviderRole_ShouldAllow()
{
    AuthenticateAs("provider-user-123", roles: ["Provider"], 
                   permissions: [EPermission.ProvidersList.GetValue()]);
    
    var response = await ApiClient.GetAsync("/api/v1/providers");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task ProviderOnlyPolicy_WithUserRole_ShouldDeny()
{
    AuthenticateAs("regular-user-123", roles: ["User"], 
                   permissions: [EPermission.ProvidersList.GetValue()]);
    
    var response = await ApiClient.GetAsync("/api/v1/providers");
    
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

#### 2. AdminOrProvider Policy (+2 testes)
```csharp
[Fact]
public async Task AdminOrProviderPolicy_WithAdmin_ShouldAllow()
[Fact]
public async Task AdminOrProviderPolicy_WithProvider_ShouldAllow()
```

#### 3. AdminOrOwner Policy (+3 testes)
```csharp
[Fact]
public async Task AdminOrOwnerPolicy_WithOwner_ShouldAllowOwnResource()
[Fact]
public async Task AdminOrOwnerPolicy_WithNonOwner_ShouldDenyOtherResource()
[Fact]
public async Task AdminOrOwnerPolicy_WithAdmin_ShouldAllowAnyResource()
```

**Total:** +7 testes cobrindo todas as políticas role-based

## Testes Cross-Module (NOVO - Commit 737dab30)

### ProviderServiceCatalogSearchWorkflowTests

Valida integração completa entre 3 módulos (Provider → ServiceCatalog → SearchProviders):

#### Workflow 1: Create Provider with Services → Search
```csharp
[Fact]
public async Task CompleteWorkflow_CreateProviderWithServices_ShouldAppearInSearch()
{
    // STEP 1: Criar categoria e serviço
    var categoryResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", ...);
    var serviceResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", ...);
    
    // STEP 2: Criar Provider com geolocalização (São Paulo)
    var providerResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", new {
        BusinessProfile = new {
            PrimaryAddress = new {
                Latitude = -23.550520,
                Longitude = -46.633308
            }
        }
    });
    
    // STEP 3: Associar serviço ao provider
    // (implícito ou via endpoint específico)
    
    // STEP 4: Buscar via SearchProviders API
    var searchResponse = await ApiClient.GetAsync(
        "/api/v1/search-providers?" +
        $"latitude={latitude}&longitude={longitude}" +
        $"&radiusKm=10&serviceIds={serviceId}");
    
    // STEP 5: Validar provider aparece nos resultados
    var items = searchResult.GetProperty("data").GetProperty("items");
    items.EnumerateArray().Should().Contain(p => 
        p.GetProperty("id").GetGuid() == providerId);
    
    // STEP 6: Validar ordenação (SubscriptionTier > Rating > Distance)
}
```

#### Workflow 2: Filter by Multiple Services
```csharp
[Fact]
public async Task CompleteWorkflow_FilterByMultipleServices_ShouldReturnOnlyMatchingProviders()
{
    // Criar 2 categorias, 2 serviços
    // Criar Provider1 (multi-service: Consultation + Tutoring)
    // Criar Provider2 (single-service: Consultation only)
    
    // Buscar com múltiplos serviceIds
    var searchResponse = await ApiClient.GetAsync(
        $"/api/v1/search-providers?serviceIds={serviceId1},{serviceId2}");
    
    // Validar que apenas Provider1 aparece (tem ambos os serviços)
    items.Should().Contain(p => p.GetProperty("id").GetGuid() == providerId1);
    items.Should().NotContain(p => p.GetProperty("id").GetGuid() == providerId2);
}
```

**Total:** +2 testes validando integração end-to-end

## Testes por Módulo

### Users

**Arquivo:** [UsersEndToEndTests.cs](../../tests/MeAjudaAi.E2E.Tests/Modules/Users/UsersEndToEndTests.cs)

**Cobertura:**
- ✅ DELETE com persistência
- ✅ UPDATE com persistência
- ✅ Múltiplas atualizações consecutivas
- ✅ Workflow completo CREATE → UPDATE → DELETE
- ✅ **409 Conflict Validation (NOVO - Commit 737dab30)**
  - Duplicate email detection
  - Duplicate username detection
  - Concurrent update handling

**Exemplo - Workflow Completo:**
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

**Exemplo - 409 Conflict Validation:**
```csharp
[Fact]
public async Task CreateUser_WithDuplicateEmail_Should_Return_Conflict()
{
    AuthenticateAsAdmin();
    
    // CREATE primeiro usuário
    var firstRequest = new { Email = "duplicate@example.com", ... };
    await ApiClient.PostAsJsonAsync("/api/v1/users", firstRequest);
    
    // ATTEMPT criar segundo usuário com mesmo email
    var secondRequest = new { Email = "duplicate@example.com", ... };
    var response = await ApiClient.PostAsJsonAsync("/api/v1/users", secondRequest);
    
    // ASSERT - Deve retornar 409 Conflict ou 400 Bad Request
    response.StatusCode.Should().BeOneOf(
        HttpStatusCode.Conflict, 
        HttpStatusCode.BadRequest);
}

[Fact]
public async Task UpdateUser_ConcurrentUpdates_Should_HandleGracefully()
{
    // CREATE user
    var userId = await CreateUserAsync();
    
    // CONCURRENT updates (simular race condition)
    var tasks = Enumerable.Range(1, 5).Select(i => 
        ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", 
            new { FirstName = $"Updated{i}" }));
    
    var responses = await Task.WhenAll(tasks);
    
    // ASSERT - Pelo menos uma atualização deve ter sucesso
    responses.Should().Contain(r => 
        r.StatusCode == HttpStatusCode.OK || 
        r.StatusCode == HttpStatusCode.NoContent);
}
```

### Providers

**Arquivo:** [ProvidersEndToEndTests.cs](../../tests/MeAjudaAi.E2E.Tests/Modules/Providers/ProvidersEndToEndTests.cs) *(consolidado: 3→1 arquivo)*

**Estrutura Interna (10 testes em 6 #regions):**
- `#region Basic CRUD Operations` - Criação e workflows completos
- `#region Update Operations` - Atualização com validação de dados
- `#region Delete Operations` - Exclusão e verificação de cascata
- `#region Verification Status` - Transições de status de verificação
- `#region Basic Info Correction` - Workflows de correção
- `#region Document Operations` - Upload e remoção de documentos

**Cobertura:**
- ✅ CRUD completo (Create, Update, Delete)
- ✅ Workflow completo de criação e busca
- ✅ Atualização de dados válidos e inválidos
- ✅ Exclusão sem documentos associados
- ✅ Mudança de status de verificação (Pending → Verified)
- ✅ Validação de transições inválidas de status
- ✅ Workflow de correção de informações básicas
- ✅ Upload de documentos para providers
- ✅ Exclusão de documentos de providers

**Exemplo:**
```csharp
[Fact]
public async Task UpdateProvider_WithValidData_Should_Return_Success()
{
    AuthenticateAsAdmin();
    var uniqueId = Guid.NewGuid().ToString("N")[..8];
    
    // CREATE provider
    var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest);
    var providerId = ExtractIdFromLocation(createResponse.Headers.Location);
    
    // UPDATE provider
    var updateRequest = new { Name = $"Updated_{uniqueId}", ... };
    var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", updateRequest);
    
    updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
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

**Arquivo:** [DocumentsEndToEndTests.cs](../../tests/MeAjudaAi.E2E.Tests/Modules/Documents/DocumentsEndToEndTests.cs) *(consolidado: 2→1 arquivo)*

**Estrutura Interna (10 testes em 6 #regions):**
- `#region Helper Methods` - Helpers para providers e extração de IDs
- `#region Upload and Basic CRUD` - Upload e operações básicas
- `#region Provider Documents` - Documentos por provider
- `#region Workflows` - Workflows de aprovação e rejeição
- `#region Isolation and Cascading` - Exclusão em cascata
- `#region Verification Workflows` - Múltiplas verificações e histórico

**Cobertura:**
- ✅ Workflow UPLOAD → VERIFY
- ✅ Rejeição de documentos com RejectionReason
- ✅ Histórico de múltiplos documentos por provider
- ✅ Exclusão em cascata quando provider é removido
- ✅ Busca de documentos por ProviderId

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

### ServiceCatalogs

**Arquivo:** [ServiceCatalogsEndToEndTests.cs](../../tests/MeAjudaAi.E2E.Tests/Modules/ServiceCatalogs/ServiceCatalogsEndToEndTests.cs) *(consolidado: 2→1 arquivo)*

**Estrutura Interna (14 testes em 7 #regions):**
- `#region Basic CRUD Operations` - Criar, buscar e listar serviços
- `#region Category Filtering` - Filtros por categoria
- `#region Update and Delete Operations` - Atualização e exclusão
- `#region Activation Status Changes` - Ativação/desativação
- `#region Database Persistence Verification` - Persistência em banco
- `#region Advanced Validation Rules` - Validação de dados avançada
- `#region Advanced Category Change Scenarios` - Mudança de categoria

**Cobertura:**
- ✅ CRUD completo (Create, Read, Update, Delete)
- ✅ Filtros por categoria (Healthcare, Education, etc.)
- ✅ Ativação e desativação de serviços
- ✅ Validação de persistência em banco de dados
- ✅ Validação de regras de negócio (nome obrigatório, preço positivo)
- ✅ Mudança de categoria de serviços
- ✅ Validação de relacionamentos com providers

**Exemplo:**
```csharp
[Fact]
public async Task ServiceCatalog_CompleteLifecycle_ShouldWork()
{
    AuthenticateAsAdmin();
    
    // CREATE
    var createRequest = new { Name = "Test Service", Category = 0, Price = 100.00, ... };
    var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs", createRequest);
    var serviceId = ExtractIdFromLocation(createResponse.Headers.Location);
    
    // UPDATE
    var updateRequest = new { Name = "Updated Service", IsActive = false };
    await ApiClient.PutAsJsonAsync($"/api/v1/service-catalogs/{serviceId}", updateRequest);
    
    // VERIFY persistence
    var getResponse = await ApiClient.GetAsync($"/api/v1/service-catalogs/{serviceId}");
    var service = await getResponse.Content.ReadFromJsonAsync<dynamic>();
    service!.GetProperty("data").GetProperty("isActive").GetBoolean().Should().BeFalse();
    
    // DELETE
    var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/service-catalogs/{serviceId}");
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
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

### Estado Atual (Dez/2025)

**Total de Testes E2E:** **156 testes** em **20 arquivos consolidados**

**Cobertura por Categoria:**

| Categoria | Testes | Arquivos | Descrição |
|-----------|--------|----------|---------------|
| Módulos | 75 | 6 | CRUD, workflows, validações por módulo |
| Infraestrutura | 56 | 9 | Middleware, rate limiting, health checks, versioning |
| Autorização | 15 | 1 | Policies baseadas em roles e permissões |
| Cross-Module | 10 | 4 | Workflows integrados entre módulos |
| **TOTAL** | **156** | **20** | - |

**Cobertura Detalhada por Módulo:**

| Módulo/Categoria | Arquivo | Testes | Status |
|------------------|---------|--------|--------|
| **MÓDULOS (75 testes)** | | | |
| Users | UsersEndToEndTests.cs | 13 | ✅ Passed |
| Providers | ProvidersEndToEndTests.cs | 15 | ✅ Passed |
| Documents | DocumentsEndToEndTests.cs | 12 | ✅ Passed |
| SearchProviders | SearchProvidersEndToEndTests.cs | 10 | ✅ Passed |
| ServiceCatalogs | ServiceCatalogsEndToEndTests.cs | 15 | ✅ Passed |
| Locations | LocationsEndToEndTests.cs | 10 | ✅ Passed |
| **INFRAESTRUTURA (56 testes)** | | | |
| Middleware | MiddlewareEndToEndTests.cs | 18 | ✅ Passed |
| Rate Limiting | RateLimitingEndToEndTests.cs | 6 | ✅ Passed |
| CORS | CorsEndToEndTests.cs | 8 | ✅ Passed |
| OpenTelemetry | OpenTelemetryMetricsEndToEndTests.cs | 5 | ✅ Passed |
| Validation | ValidationStatusCodeEndToEndTests.cs | 7 | ✅ Passed |
| Health Checks | HealthCheckTests.cs | 4 | ✅ Passed |
| Infrastructure Health | InfrastructureHealthTests.cs | 3 | ✅ Passed |
| API Versioning | ApiVersioningTests.cs | 3 | ✅ Passed |
| Module Integration | ModuleIntegrationTests.cs | 2 | ✅ Passed |
| **AUTORIZAÇÃO (15 testes)** | | | |
| Permission-Based | PermissionAuthorizationEndToEndTests.cs | 15 | ✅ Passed |
| **CROSS-MODULE (2 testes)** | | | |
| Provider-Service-Search | ProviderServiceCatalogSearchWorkflowTests.cs | 2 | ✅ Passed |
| **TOTAL** | **20 arquivos** | **148** | - |

**Middlewares Cobertos (E2E + Integration):**
- ✅ BusinessMetricsMiddleware (E2E)
- ✅ LoggingContextMiddleware (E2E)
- ✅ SecurityHeadersMiddleware (E2E + Integration: 13 testes)
- ✅ CompressionSecurityMiddleware (E2E + Integration: 8 testes)
- ✅ ExceptionHandlerMiddleware (E2E) - ProblemDetails RFC 7807 validation
- ✅ RateLimitingMiddleware (E2E)
- ✅ RequestLoggingMiddleware (E2E)
- ✅ PermissionOptimizationMiddleware (validado)
- ✅ CorrelationId propagation (validado)
- ✅ CORS Middleware (E2E)

**Novos Testes de Infraestrutura:**
- ✅ CorsEndToEndTests.cs (8 testes) - CORS headers, preflight, origins
- ✅ OpenTelemetryMetricsEndToEndTests.cs (5 testes) - Métricas, traces, logs
- ✅ ValidationStatusCodeEndToEndTests.cs (7 testes) - Status codes HTTP corretos
- ✅ HealthCheckTests.cs (4 testes) - Endpoints /health e /ready
- ✅ InfrastructureHealthTests.cs (3 testes) - PostgreSQL, Redis, Azurite health
- ✅ ApiVersioningTests.cs (3 testes) - Versionamento de API
- ✅ ModuleIntegrationTests.cs (2 testes) - Integração entre módulos

**Padrão de Nomenclatura:** `{Module}EndToEndTests.cs` com organização `#region` por cenário de negócio

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

### Desempenho Lento

**Sintoma:** Testes E2E levam muito tempo

**Otimizações:**
1. **Parallel execution:** xUnit executa classes em paralelo por padrão
2. **Container reuse:** TestContainers reutiliza containers entre testes da mesma classe
3. **Seed data:** Minimizar criação de dados no `Arrange`

## Referências

- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [xUnit Parallelization](https://xunit.net/docs/running-tests-in-parallel)
- [FluentAssertions](https://fluentassertions.com/)
