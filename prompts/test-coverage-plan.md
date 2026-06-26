# Plano de Cobertura de Testes - Todos os Módulos

**Data:** 23/06/2026  
**Status:** Proposto  
**Módulos:** Users, Providers, Bookings, Payments, Communications, Documents, Locations, ServiceCatalogs, Ratings, SearchProviders

---

## 1. Inventário de Endpoints por Módulo

### 1.1 Módulo: Users
**Base Path:** `/api/v1/users`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/` | RequireAdmin() | E2E ✅ |
| 2 | GET | `/` | RequirePermission(UsersList) | E2E ✅ |
| 3 | GET | `/{id:guid}` | RequireAuthorization() | E2E ✅ |
| 4 | GET | `/by-email/{email}` | RequireAdmin() | E2E ✅ |
| 5 | PUT | `/{id:guid}/profile` | RequireAuthorization() | E2E ✅ |
| 6 | DELETE | `/{id:guid}` | RequireAdmin() | E2E ✅ |
| 7 | POST | `/register` | AllowAnonymous() | INT ✅ |
| 8 | PUT | `/{id:guid}/device-token` | RequireAuthorization() | E2E ✅ |
| 9 | GET | `/auth/providers` | AllowAnonymous() | **NOVO** |

**Gaps:** Endpoint de auth providers sem teste

---

### 1.2 Módulo: Providers
**Base Path:** `/api/v1/providers`

#### Endpoints Públicos/Autenticados (Public/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/become` | RequireAuthorization() | **NOVO** |
| 2 | GET | `/public/{idOrSlug}` | AllowAnonymous() | **NOVO** |
| 3 | PUT | `/{id:guid}/device-token` | RequireAuthorization() | E2E ✅ |

#### Endpoints do Próprio Prestador (Public/Me/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 4 | GET | `/me` | RequireAuthorization() | E2E ✅ |
| 5 | PUT | `/me` | RequireAuthorization() | **NOVO** |
| 6 | DELETE | `/me` | RequireAuthorization() | **NOVO** |
| 7 | POST | `/me/activate` | RequireAuthorization() | E2E ✅ |
| 8 | POST | `/me/deactivate` | RequireAuthorization() | E2E ✅ |
| 9 | GET | `/me/status` | RequireAuthorization() | **NOVO** |
| 10 | POST | `/me/documents` | RequireAuthorization() | **NOVO** |

#### Endpoints Admin (ProviderAdmin/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 11 | POST | `/` | RequirePermission(ProvidersCreate) | E2E ✅ |
| 12 | GET | `/` | RequirePermission(ProvidersList) | INT ✅ |
| 13 | GET | `/{id:guid}` | RequirePermission(ProvidersRead) | E2E ✅ |
| 14 | GET | `/by-user/{userId:guid}` | RequirePermission(ProvidersRead) | E2E ✅ |
| 15 | GET | `/by-city/{city}` | RequirePermission(ProvidersRead) | INT ✅ |
| 16 | GET | `/by-state/{state}` | RequirePermission(ProvidersRead) | INT ✅ |
| 17 | GET | `/by-type/{type}` | RequirePermission(ProvidersRead) | E2E ✅ |
| 18 | GET | `/verification-status/{status}` | RequirePermission(ProvidersRead) | INT ✅ |
| 19 | PUT | `/{id:guid}` | RequirePermission(ProvidersUpdate) | E2E ✅ |
| 20 | PUT | `/{id:guid}/verification-status` | RequirePermission(ProvidersApprove) | E2E ✅ |
| 21 | DELETE | `/{id:guid}` | RequirePermission(ProvidersDelete) | E2E ✅ |
| 22 | POST | `/{id:guid}/documents` | RequirePermission(ProvidersUpdate) | E2E ✅ |
| 23 | DELETE | `/{id:guid}/documents/{docId}` | RequireAuthorization("SelfOrAdmin") | E2E ✅ |
| 24 | POST | `/{id:guid}/require-correction` | RequireAuthorization() | **NOVO** |
| 25 | GET | `/{id:guid}/verification-events` | RequireAuthorization() | **NOVO** |

#### Endpoints de Serviços (ProviderServices/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 26 | POST | `/{providerId:guid}/services/{serviceId:guid}` | RequireAuthorization("SelfOrAdmin") | **NOVO** |
| 27 | DELETE | `/{providerId:guid}/services/{serviceId:guid}` | RequireAuthorization("SelfOrAdmin") | **NOVO** |

**Gaps Providers:** 9 endpoints sem teste

---

### 1.3 Módulo: Bookings
**Base Path:** `/api/v1/bookings`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/` | RequireAuthorization() | INT ✅ |
| 2 | PUT | `/{id:guid}/confirm` | RequireAuthorization() | INT ✅ |
| 3 | PUT | `/{id:guid}/cancel` | RequireAuthorization() | INT ✅ |
| 4 | PUT | `/{id:guid}/reject` | RequireAuthorization() | INT ✅ |
| 5 | PUT | `/{id:guid}/complete` | RequireAuthorization() | INT ✅ |
| 6 | GET | `/{id:guid}` | RequireAuthorization() | INT ✅ |
| 7 | GET | `/{id:guid}/events` | RequireAuthorization() | E2E ✅ |
| 8 | GET | `/my` | RequireAuthorization() | **NOVO** |
| 9 | GET | `/provider/{providerId:guid}` | RequireAuthorization() | **NOVO** |
| 10 | GET | `/availability/{providerId:guid}` | RequireAuthorization() | **NOVO** |
| 11 | POST | `/schedule` | RequireAuthorization() | **NOVO** |

**Gaps Bookings:** 5 endpoints sem teste

---

### 1.4 Módulo: Payments
**Base Path:** `/api/v1/payments`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/subscriptions` | RequireAuthorization() | INT ✅ |
| 2 | POST | `/subscriptions/billing-portal` | RequireAuthorization() | INT ✅ |
| 3 | POST | `/webhook` (Stripe) | AllowAnonymous() | **NOVO** |

**Gaps Payments:** 1 endpoint sem teste (webhook)

---

### 1.5 Módulo: Communications
**Base Path:** `/api/v1/communications`

#### Endpoints Públicos
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/logs` | RequirePermission(CommunicationsRead) | **NOVO** |
| 2 | GET | `/templates` | RequirePermission(CommunicationsRead) | **NOVO** |

#### Endpoints Admin
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 3 | POST | `/templates` | RequirePermission(CommunicationsManage) | **NOVO** |
| 4 | PUT | `/templates/{id:guid}` | RequirePermission(CommunicationsManage) | **NOVO** |
| 5 | PATCH | `/templates/{id:guid}/activate` | RequirePermission(CommunicationsManage) | **NOVO** |
| 6 | PATCH | `/templates/{id:guid}/deactivate` | RequirePermission(CommunicationsManage) | **NOVO** |

**Gaps Communications:** 6 endpoints sem teste

---

### 1.6 Módulo: Documents
**Base Path:** `/api/v1/documents`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/upload` | RequireAuthorization() | E2E ✅ |
| 2 | GET | `/{documentId:guid}` | RequireAdmin() | **NOVO** |
| 3 | GET | `/provider/{providerId:guid}` | RequireAuthorization() | **NOVO** |
| 4 | POST | `/{documentId:guid}/request-verification` | RequireAuthorization() | **NOVO** |
| 5 | POST | `/{documentId:guid}/verify` | RequireAdmin() | E2E ✅ |
| 6 | DELETE | `/{documentId:guid}` | RequireAdmin() | E2E ✅ |

**Gaps Documents:** 3 endpoints sem teste

---

### 1.7 Módulo: Locations
**Base Path:** `/api/v1/locations`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/` | RequireAdmin() | INT ✅ |
| 2 | GET | `/state/{state}` | RequireAdmin() | INT ✅ |
| 3 | GET | `/{id:guid}` | RequireAdmin() | **NOVO** |
| 4 | POST | `/` | RequireAdmin() | INT ✅ |
| 5 | PUT | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 6 | PATCH | `/{id:guid}` | RequireAdmin() | **NOVO** |
| 7 | DELETE | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 8 | GET | `/search` | RequirePermission(LocationsManage) | **NOVO** |

**Gaps Locations:** 3 endpoints sem teste

---

### 1.8 Módulo: ServiceCatalogs
**Base Path:** `/api/v1/service-catalogs`

#### Categories
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/categories` | RequirePermission(ServiceCatalogsRead) | INT ✅ |
| 2 | GET | `/categories/{id:guid}` | RequireAuthorization() | INT ✅ |
| 3 | POST | `/categories` | RequireAdmin() | INT ✅ |
| 4 | PUT | `/categories/{id:guid}` | RequireAdmin() | INT ✅ |
| 5 | POST | `/categories/{id:guid}/activate` | RequireAdmin() | INT ✅ |
| 6 | POST | `/categories/{id:guid}/deactivate` | RequireAdmin() | INT ✅ |
| 7 | DELETE | `/categories/{id:guid}` | RequireAdmin() | INT ✅ |

#### Services
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 8 | GET | `/services` | AllowAnonymous() | INT ✅ |
| 9 | GET | `/services/{id:guid}` | RequireAuthorization() | INT ✅ |
| 10 | GET | `/services/category/{categoryId:guid}` | RequireAuthorization() | INT ✅ |
| 11 | POST | `/services` | RequireAdmin() | INT ✅ |
| 12 | PUT | `/services/{id:guid}` | RequireAdmin() | **NOVO** |
| 13 | POST | `/services/{id:guid}/change-category` | RequireAdmin() | INT ✅ |
| 14 | POST | `/services/{id:guid}/activate` | RequireAdmin() | **NOVO** |
| 15 | POST | `/services/{id:guid}/deactivate` | RequireAdmin() | **NOVO** |
| 16 | DELETE | `/services/{id:guid}` | RequireAdmin() | INT ✅ |
| 17 | POST | `/services/validate` | RequireAdmin() | INT ✅ |

**Gaps ServiceCatalogs:** 3 endpoints sem teste

---

### 1.9 Módulo: Ratings
**Base Path:** `/api/v1/ratings`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/` | RequireAuthorization() | **NOVO** |
| 2 | GET | `/{id:guid}` | AllowAnonymous() | **NOVO** |
| 3 | GET | `/provider/{providerId:guid}` | AllowAnonymous() | **NOVO** |
| 4 | GET | `/{id:guid}/status` | RequireAuthorization("AdminPolicy") | **NOVO** |

**Gaps Ratings:** 4 endpoints sem teste (módulo inteiro)

---

### 1.10 Módulo: SearchProviders
**Base Path:** `/api/v1/search`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/providers` | (público) | E2E ✅ |

**Gaps SearchProviders:** Nenhum

---

## 2. Resumo de Gaps por Prioridade

### Prioridade Alta (Críticos para negócio)
| Módulo | Endpoint | Tipo |
|--------|----------|------|
| Providers | POST /me/documents | INT |
| Providers | GET /me/status | INT |
| Providers | POST /become | INT |
| Providers | GET /{id}/verification-events | INT |
| Bookings | GET /my | INT |
| Bookings | GET /provider/{id} | INT |
| Payments | POST /webhook (Stripe) | E2E |

### Prioridade Média
| Módulo | Endpoint | Tipo |
|--------|----------|------|
| Users | GET /auth/providers | INT |
| Providers | PUT /me | INT |
| Providers | DELETE /me | INT |
| Providers | POST /{id}/require-correction | INT |
| Providers | POST /{id}/services/{sid} | INT |
| Providers | DELETE /{id}/services/{sid} | INT |
| Bookings | GET /availability/{id} | INT |
| Bookings | POST /schedule | INT |
| Communications | Todos (6) | INT |
| Documents | GET /{id} | INT |
| Documents | GET /provider/{id} | INT |
| Documents | POST /{id}/request-verification | INT |
| Locations | GET /{id} | INT |
| Locations | PATCH /{id} | INT |
| Locations | GET /search | INT |
| Ratings | Todos (4) | INT+E2E |

### Prioridade Baixa (Infraestrutura)
| Módulo | Endpoint | Tipo |
|--------|----------|------|
| ServiceCatalogs | PUT /services/{id} | INT |
| ServiceCatalogs | POST /services/{id}/activate | INT |
| ServiceCatalogs | POST /services/{id}/deactivate | INT |

---

## 3. Templates de Testes

### 3.1 Template Teste de Integração (INT)

```csharp
namespace MeAjudaAi.Integration.Tests.Modules.{Module};

[Collection("Database")]
public class {EndpointName}IntegrationTests : BaseApiTest
{
    public {EndpointName}IntegrationTests(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task {MethodName}_{Scenario}_ShouldReturnExpectedStatusCode()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // Act
        var response = await Client.{MethodName}Async("{endpoint}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.{Expected});
        
        if (response.IsSuccessStatusCode)
        {
            var content = await ReadJsonAsync<JsonElement>(response.Content);
            // assertions...
        }
    }
    
    [Fact]
    public async Task {MethodName}_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();
        
        // Act
        var response = await Client.{MethodName}Async("{endpoint}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### 3.2 Template Teste E2E

```csharp
namespace MeAjudaAi.E2E.Tests.Modules.{Module};

[Collection("TestContainer")]
public class {EndpointName}EndToEndTests : BaseTestContainerTest
{
    private readonly TestContainerFixture _fixture;
    
    public {EndpointName}EndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }
    
    public async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _fixture.AuthenticateAsAdmin();
    }
    
    [Fact]
    public async Task {MethodName}_{Scenario}_ShouldWork()
    {
        // Arrange
        var request = new 
        {
            Property1 = "value",
            Property2 = 123
        };
        
        // Act
        var response = await _fixture.PostJsonAsync("{endpoint}", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Optional: Verify database state
        await _fixture.WithServiceScopeAsync(async services =>
        {
            // var context = services.GetRequiredService<SomeDbContext>();
            // assertions...
        });
    }
}
```

---

## 4. Plano de Execução

### Fase 1: Providers + Users (Sprint Atual)
**Objetivo:** Completar coverage do módulo mais utilizado

- [ ] Users: GET /auth/providers
- [ ] Providers: POST /become
- [ ] Providers: GET /me/status
- [ ] Providers: PUT /me
- [ ] Providers: DELETE /me
- [ ] Providers: POST /me/documents
- [ ] Providers: GET /{id}/verification-events
- [ ] Providers: POST /{id}/require-correction
- [ ] Providers: POST /{id}/services/{sid}
- [ ] Providers: DELETE /{id}/services/{sid}

### Fase 2: Bookings + Payments
**Objetivo:** Fluxos críticos de negócio

- [ ] Bookings: GET /my
- [ ] Bookings: GET /provider/{id}
- [ ] Bookings: GET /availability/{id}
- [ ] Bookings: POST /schedule
- [ ] Payments: POST /webhook (Stripe)

### Fase 3: Communications + Documents
**Objetivo:** Módulos de suporte

- [ ] Communications: GET /logs
- [ ] Communications: GET /templates
- [ ] Communications: POST /templates
- [ ] Communications: PUT /templates/{id}
- [ ] Communications: PATCH /templates/{id}/activate
- [ ] Communications: PATCH /templates/{id}/deactivate
- [ ] Documents: GET /{id}
- [ ] Documents: GET /provider/{id}
- [ ] Documents: POST /{id}/request-verification

### Fase 4: Locations + Ratings + ServiceCatalogs
**Objetivo:** Finalizar coverage

- [ ] Locations: GET /{id}
- [ ] Locations: PATCH /{id}
- [ ] Locations: GET /search
- [ ] Ratings: POST /
- [ ] Ratings: GET /{id}
- [ ] Ratings: GET /provider/{id}
- [ ] Ratings: GET /{id}/status
- [ ] ServiceCatalogs: PUT /services/{id}
- [ ] ServiceCatalogs: POST /services/{id}/activate
- [ ] ServiceCatalogs: POST /services/{id}/deactivate

---

## 5. Estratégia de Dados de Teste

### 5.1 Dados Compartilhados (Fixtures)

```csharp
// DatabaseFixture.cs ou TestContainerFixture.cs

// Providers
protected async Task<Guid> CreateTestProviderAsync(string name = "Test Provider")
{
    var provider = Provider.Create(
        ProviderId.Create(Guid.NewGuid()),
        UserId.Create(Guid.NewGuid()),
        name,
        EProviderType.Individual,
        BusinessProfile.Create(/* ... */)
    );
    // ... save and return
}

// Bookings
protected async Task<Guid> CreateTestBookingAsync(Guid clientId, Guid providerId)
{
    // ...
}

// ServiceCatalogs
protected async Task<Guid> CreateTestCategoryAsync(string name = "Test Category")
{
    // ...
}
```

### 5.2 Dados Anônimos (Bogus/Faker)

```csharp
private static readonly Faker _faker = new("pt_BR");

private static string GenerateTestName() => 
    $"Test_{_faker.Person.FullName}_{Guid.NewGuid().ToString("N")[..8]}";

private static string GenerateEmail() => 
    $"test_{Guid.NewGuid().ToString("N")[..8]}@test.com";
```

---

## 6. Autenticação em Testes

### 6.1 Integração (BaseApiTest)

```csharp
// Configurar admin
AuthConfig.ConfigureAdmin();

// Configurar usuário regular
AuthConfig.ConfigureRegularUser();

// Permitir anônimo
AuthConfig.SetAllowUnauthenticated(true);

// Limpar
AuthConfig.ClearConfiguration();
```

### 6.2 E2E (BaseTestContainerTest)

```csharp
// Admin
TestContainerFixture.AuthenticateAsAdmin();

// Usuário específico
TestContainerFixture.AuthenticateAsUser(userId, username);

// Com permissões customizadas
ConfigurableTestAuthenticationHandler.ConfigureUser(
    userId: userId,
    userName: "testuser",
    email: "test@test.com",
    permissions: new[] { EPermission.ProvidersRead.GetValue() },
    isSystemAdmin: false,
    roles: Array.Empty<string>()
);

// Provider context
ConfigurableTestAuthenticationHandler.ConfigureProvider(providerId);

// Anônimo
TestContainerFixture.AuthenticateAsAnonymous();
```

---

## 7. Arquivos de Referência

### Base Classes
- `tests/MeAjudaAi.Integration.Tests/Base/BaseApiTest.cs`
- `tests/MeAjudaAi.E2E.Tests/Base/BaseTestContainerTest.cs`
- `tests/MeAjudaAi.E2E.Tests/Base/TestContainerFixture.cs`

### Helpers
- `tests/MeAjudaAi.Integration.Tests/Base/BaseApiTest.cs` → `ReadJsonAsync<T>()`, `GetResponseData()`
- `tests/MeAjudaAi.E2E.Tests/Base/BaseTestContainerTest.cs` → `PostJsonAsync<T>()`, `ExtractIdFromLocation()`

### Handlers de Auth
- `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Handlers/ConfigurableTestAuthenticationHandler.cs`
- `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Handlers/TestAuthenticationConfiguration.cs`

---

## 8. Métricas

| Módulo | Total Endpoints | Com Teste | Gap | Coverage |
|--------|-----------------|-----------|-----|----------|
| Users | 9 | 8 | 1 | 89% |
| Providers | 27 | 18 | 9 | 67% |
| Bookings | 11 | 6 | 5 | 55% |
| Payments | 3 | 2 | 1 | 67% |
| Communications | 6 | 0 | 6 | 0% |
| Documents | 6 | 3 | 3 | 50% |
| Locations | 8 | 5 | 3 | 63% |
| ServiceCatalogs | 17 | 14 | 3 | 82% |
| Ratings | 4 | 0 | 4 | 0% |
| SearchProviders | 1 | 1 | 0 | 100% |
| **TOTAL** | **92** | **57** | **35** | **62%** |

---

## 9. Critérios de Aceitação

- [ ] Todos os endpoints com teste de sucesso (2xx)
- [ ] Todos os endpoints com teste de não autorizado (401/403)
- [ ] Todos os endpoints admin com teste de forbidden para non-admin
- [ ] Fluxos end-to-end completos (criar → ler → atualizar → deletar)
- [ ] Testes de validação de input (400)
- [ ] Testes de não encontrado (404) onde aplicável
- [ ] Cobertura de código > 70% por módulo

---

## 10. Notas

1. **Webhooks (Stripe):** Requer mocking do Stripe ou uso de Stripe CLI para testes locais
2. **SSE Endpoints:** Requer cliente HTTP especial ou teste via E2E com timeout
3. **Geographic Restrictions:** Alguns endpoints têm restrições baseadas em geolocalização (HTTP 451)
4. **Rate Limiting:** Endpoints públicos podem ter rate limiting que afeta testes

---

*Documento gerado automaticamente - Revise e ajuste conforme necessidade do projeto*