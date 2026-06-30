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
| 9 | GET | `/auth/providers` | AllowAnonymous() | INT ✅ |

**Gaps Users:** Nenhum

---

### 1.2 Módulo: Providers
**Base Path:** `/api/v1/providers`

#### Endpoints Públicos/Autenticados (Public/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/become` | RequireAuthorization() | INT ✅ |
| 2 | GET | `/public/{idOrSlug}` | AllowAnonymous() | INT ✅ |
| 3 | PUT | `/{id:guid}/device-token` | RequireAuthorization() | E2E ✅ |

#### Endpoints do Próprio Prestador (Public/Me/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 4 | GET | `/me` | RequireAuthorization() | E2E ✅ |
| 5 | PUT | `/me` | RequireAuthorization() | INT ✅ |
| 6 | DELETE | `/me` | RequireAuthorization() | INT ✅ |
| 7 | POST | `/me/activate` | RequireAuthorization() | E2E ✅ |
| 8 | POST | `/me/deactivate` | RequireAuthorization() | E2E ✅ |
| 9 | GET | `/me/status` | RequireAuthorization() | INT ✅ |
| 10 | POST | `/me/documents` | RequireAuthorization() | INT ✅ |

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
| 24 | POST | `/{id:guid}/require-correction` | RequireAuthorization() | INT ✅ |
| 25 | GET | `/{id:guid}/verification-events` | RequireAuthorization() | INT ✅ |

#### Endpoints de Serviços (ProviderServices/)
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 26 | POST | `/{providerId:guid}/services/{serviceId:guid}` | RequireAuthorization("SelfOrAdmin") | INT ✅ |
| 27 | DELETE | `/{providerId:guid}/services/{serviceId:guid}` | RequireAuthorization("SelfOrAdmin") | INT ✅ |

**Gaps Providers:** Nenhum

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
| 8 | GET | `/my` | RequireAuthorization() | INT ✅ |
| 9 | GET | `/provider/{providerId:guid}` | RequireAuthorization() | INT ✅ |
| 10 | GET | `/availability/{providerId:guid}` | RequireAuthorization() | INT ✅ |
| 11 | POST | `/schedule` | RequireAuthorization() | INT ✅ |

**Gaps Bookings:** Nenhum

---

### 1.4 Módulo: Payments
**Base Path:** `/api/v1/payments`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/subscriptions` | RequireAuthorization() | INT ✅ |
| 2 | POST | `/subscriptions/billing-portal` | RequireAuthorization() | INT ✅ |
| 3 | POST | `/webhook` (Stripe) | AllowAnonymous() | INT ✅ + E2E ✅ |

**Gaps Payments:** Nenhum

---

### 1.5 Módulo: Communications
**Base Path:** `/api/v1/communications`

#### Endpoints Públicos
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/logs` | RequirePermission(CommunicationsRead) | INT ✅ |
| 2 | GET | `/templates` | RequirePermission(CommunicationsRead) | INT ✅ |

#### Endpoints Admin
| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 3 | POST | `/templates` | RequirePermission(CommunicationsManage) | E2E ✅ |
| 4 | PUT | `/templates/{id:guid}` | RequirePermission(CommunicationsManage) | E2E ✅ |
| 5 | PATCH | `/templates/{id:guid}/activate` | RequirePermission(CommunicationsManage) | E2E ✅ |
| 6 | PATCH | `/templates/{id:guid}/deactivate` | RequirePermission(CommunicationsManage) | E2E ✅ |

**Gaps Communications:** Nenhum

---

### 1.6 Módulo: Documents
**Base Path:** `/api/v1/documents`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/upload` | RequireAuthorization() | E2E ✅ |
| 2 | GET | `/{documentId:guid}` | RequireAdmin() | E2E ✅ |
| 3 | GET | `/provider/{providerId:guid}` | RequireAuthorization() | E2E ✅ |
| 4 | POST | `/{documentId:guid}/request-verification` | RequireAuthorization() | E2E ✅ |
| 5 | POST | `/{documentId:guid}/verify` | RequireAdmin() | E2E ✅ |
| 6 | DELETE | `/{documentId:guid}` | RequireAdmin() | E2E ✅ |

**Gaps Documents:** Nenhum

---

### 1.7 Módulo: Locations
**Base Path:** `/api/v1/locations`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/` | RequireAdmin() | INT ✅ |
| 2 | GET | `/state/{state}` | RequireAdmin() | INT ✅ |
| 3 | GET | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 4 | POST | `/` | RequireAdmin() | INT ✅ |
| 5 | PUT | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 6 | PATCH | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 7 | DELETE | `/{id:guid}` | RequireAdmin() | INT ✅ |
| 8 | GET | `/search` | RequirePermission(LocationsManage) | INT ✅ |

**Gaps Locations:** Nenhum

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
| 12 | PUT | `/services/{id:guid}` | RequireAdmin() | INT ✅ |
| 13 | POST | `/services/{id:guid}/change-category` | RequireAdmin() | INT ✅ |
| 14 | POST | `/services/{id:guid}/activate` | RequireAdmin() | INT ✅ |
| 15 | POST | `/services/{id:guid}/deactivate` | RequireAdmin() | INT ✅ |
| 16 | DELETE | `/services/{id:guid}` | RequireAdmin() | INT ✅ |
| 17 | POST | `/services/validate` | RequireAdmin() | INT ✅ |

**Gaps ServiceCatalogs:** Nenhum

---

### 1.9 Módulo: Ratings
**Base Path:** `/api/v1/ratings`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | POST | `/` | RequireAuthorization() | INT ✅ |
| 2 | GET | `/{id:guid}` | AllowAnonymous() | INT ✅ |
| 3 | GET | `/provider/{providerId:guid}` | AllowAnonymous() | INT ✅ |
| 4 | GET | `/{id:guid}/status` | RequireAuthorization("AdminPolicy") | INT ✅ |

**Gaps Ratings:** Nenhum

---

### 1.10 Módulo: SearchProviders
**Base Path:** `/api/v1/search`

| # | Método | Endpoint | Autorização | Status Teste |
|---|--------|----------|-------------|--------------|
| 1 | GET | `/providers` | (público) | E2E ✅ |

**Gaps SearchProviders:** Nenhum

---

## 2. Resumo de Gaps por Prioridade

### Todos os gaps foram cobertos! 🎉

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

### Fase 1: Providers + Users ✅ COMPLETA
**Objetivo:** Completar coverage do módulo mais utilizado

- [x] Users: GET /auth/providers
- [x] Providers: POST /become
- [x] Providers: GET /me/status
- [x] Providers: PUT /me
- [x] Providers: DELETE /me
- [x] Providers: POST /me/documents
- [x] Providers: GET /{id}/verification-events
- [x] Providers: POST /{id}/require-correction
- [x] Providers: POST /{id}/services/{sid}
- [x] Providers: DELETE /{id}/services/{sid}

### Fase 2: Bookings + Payments ✅ COMPLETA
**Objetivo:** Fluxos críticos de negócio

- [x] Bookings: GET /my (já coberto por `BookingsApiTests` + novos testes de paginação/isolamento)
- [x] Bookings: GET /provider/{id} (já coberto por `BookingsApiTests`)
- [x] Bookings: GET /availability/{id} (já coberto por `BookingsApiTests`)
- [x] Bookings: POST /schedule (já coberto por `BookingsApiTests`)
- [x] Payments: POST /webhook (Stripe) (já coberto por `PaymentsApiTests` + `PaymentsEndToEndTests`)

### Fase 3: Communications + Documents ✅ COMPLETA
**Objetivo:** Módulos de suporte

- [x] Communications: GET /logs (já coberto por `CommunicationsModuleApiTests`)
- [x] Communications: GET /templates (já coberto por `CommunicationsModuleApiTests`)
- [x] Communications: POST /templates (já coberto por `EmailTemplateEndToEndTests`)
- [x] Communications: PUT /templates/{id} (já coberto por `EmailTemplateEndToEndTests`)
- [x] Communications: PATCH /templates/{id}/activate (já coberto por `EmailTemplateEndToEndTests`)
- [x] Communications: PATCH /templates/{id}/deactivate (já coberto por `EmailTemplateEndToEndTests`)
- [x] Documents: GET /{id} (já coberto por `DocumentsEndToEndTests.GetDocumentStatus`)
- [x] Documents: GET /provider/{id} (já coberto por `DocumentsEndToEndTests.GetProviderDocuments`)
- [x] Documents: POST /{id}/request-verification (já coberto por `DocumentsEndToEndTests.RequestDocumentVerification`)

### Fase 4: Locations + Ratings + ServiceCatalogs ✅ COMPLETA
**Objetivo:** Finalizar coverage

- [x] Locations: GET /{id} (já coberto por `AllowedCityApiTests`)
- [x] Locations: PATCH /{id} (já coberto por `AllowedCityApiTests`)
- [x] Locations: GET /search (já coberto por `AllowedCityApiTests` + `LocationsApiIntegrationTests`)
- [x] Ratings: POST / (novo: `RatingsCreateEndpointTests`)
- [x] Ratings: GET /{id} (já coberto por `RatingsEndpointsTests`)
- [x] Ratings: GET /provider/{id} (já coberto por `RatingsEndpointsTests`)
- [x] Ratings: GET /{id}/status (já coberto por `RatingsEndpointsTests`)
- [x] ServiceCatalogs: PUT /services/{id} (já coberto por `ServiceCatalogsApiTests`)
- [x] ServiceCatalogs: POST /services/{id}/activate (já coberto por `ServiceCatalogsApiTests`)
- [x] ServiceCatalogs: POST /services/{id}/deactivate (já coberto por `ServiceCatalogsApiTests`)

### Fase 5: Testes Internos de Integração ✅ COMPLETA
**Objetivo:** Cobrir command/query handlers dos módulos Bookings, Payments e Communications com testes internos

#### 5.1 Bookings - Infraestrutura de Teste
- [x] `TestInfrastructureExtensions.cs` — DI wiring (InMemory DB + mocks)
- [x] `MockProvidersModuleApi.cs` — mock com seed methods
- [x] `MockServiceCatalogsModuleApi.cs` — mock com seed methods
- [x] `BookingsIntegrationTestBase.cs` — base class com helpers

#### 5.2 Bookings - Command Handler Tests
- [x] `CreateBookingCommandHandlerTests.cs` — 4 testes (valid, provider not found, inactive service, overlap)
- [x] `SetProviderScheduleCommandHandlerTests.cs` — 2 testes (new schedule, provider not found)
- [x] `ConfirmBookingCommandHandlerTests.cs` — 4 testes (success, not found, non-owner, already confirmed)
- [x] `RejectBookingCommandHandlerTests.cs` — 3 testes (success, not found, non-owner)
- [x] `CompleteBookingCommandHandlerTests.cs` — 3 testes (success, pending booking, not found)
- [x] `CancelBookingCommandHandlerTests.cs` — 4 testes (success as client, success as provider, not found, unauthorized)
- [x] `BookingRealtimeEventsHandlerTests.cs` (Unit) — 5 testes (created, confirmed, cancelled, rejected, completed)

#### 5.3 Bookings - Query Handler Tests
- [x] `GetBookingByIdQueryHandlerTests.cs` — 3 testes (success, not found, unauthorized)
- [x] `GetBookingsByClientQueryHandlerTests.cs` — 3 testes (success, empty, isolation)
- [x] `GetBookingsByProviderQueryHandlerTests.cs` — 2 testes (success, empty)
- [x] `GetProviderAvailabilityQueryHandlerTests.cs` — 2 testes (success, provider not found)

#### 5.4 Payments - Infraestrutura de Teste
- [x] `TestInfrastructureExtensions.cs` — DI wiring (InMemory DB + mocks)
- [x] `MockPaymentGateway.cs` — mock com setup methods
- [x] `PaymentsIntegrationTestBase.cs` — base class com helpers

#### 5.5 Payments - Handler Tests
- [x] `CreateSubscriptionCommandHandlerTests.cs` — 3 testes (valid, invalid plan, persists)
- [x] `CreateBillingPortalSessionCommandHandlerTests.cs` — 2 testes (valid, no subscription)
- [x] `GetActiveSubscriptionByProviderQueryHandlerTests.cs` — 2 testes (exists, not found)
- [x] `PaymentCommandServiceTests.cs` (Unit) — 8 testes (webhook handling, signature validation, inbox persistence)
- [x] `ProcessInboxJobTests.cs` (Unit) — 13 testes (event mapping, event processing, batch execution)

#### 5.6 Communications - Infraestrutura de Teste
- [x] `TestInfrastructureExtensions.cs` — DI wiring (InMemory DB + mocks)
- [x] `CommunicationsIntegrationTestBase.cs` — base class com helpers

#### 5.7 Communications - Command Handler Tests
- [x] `CreateEmailTemplateCommandHandlerTests.cs` — 2 testes (valid, duplicate key)
- [x] `UpdateEmailTemplateCommandHandlerTests.cs` — 3 testes (success, not found, system template)
- [x] `SetEmailTemplateStatusCommandHandlerTests.cs` — 4 testes (deactivate, activate, system template, not found)

#### 5.8 Communications - Query Handler Tests
- [x] `GetAllEmailTemplatesQueryHandlerTests.cs` — 2 testes (with data, empty)
- [x] `GetEmailTemplateByKeyQueryHandlerTests.cs` — 3 testes (success, not found, inactive)
- [x] `OutboxProcessorServiceTests.cs` (Unit) — 22+ testes (email, SMS, push, templates, error handling)

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

### 8.1 Cobertura de Endpoints (API)

| Módulo | Total Endpoints | Com Teste | Gap | Coverage |
|--------|-----------------|-----------|-----|----------|
| Users | 9 | 9 | 0 | 100% ✅ |
| Providers | 27 | 27 | 0 | 100% ✅ |
| Bookings | 11 | 11 | 0 | 100% ✅ |
| Payments | 3 | 3 | 0 | 100% ✅ |
| Communications | 6 | 6 | 0 | 100% ✅ |
| Documents | 6 | 6 | 0 | 100% ✅ |
| Locations | 8 | 8 | 0 | 100% ✅ |
| ServiceCatalogs | 17 | 17 | 0 | 100% ✅ |
| Ratings | 4 | 4 | 0 | 100% ✅ |
| SearchProviders | 1 | 1 | 0 | 100% ✅ |
| **TOTAL** | **92** | **92** | **0** | **100%** 🎉 |

### 8.2 Testes Internos de Integração + Unitários (Handlers/Services)

| Módulo | Command/Service | Query | Event Handlers | Total Testes |
|--------|----------------|-------|----------------|--------------|
| Bookings | 20 testes (6 handlers) | 10 testes (4 handlers) | 5 testes (1 handler) | 35 |
| Payments | 11 testes (2 handlers + PaymentCommandService) | 2 testes (1 handler) | 13 testes (ProcessInboxJob) | 26 |
| Communications | 9 testes (3 handlers) | 5 testes (2 handlers) | 22+ testes (OutboxProcessorService) | 36+ |
| **TOTAL** | **40** | **17** | **40+** | **97+** |

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