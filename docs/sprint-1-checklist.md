# 📋 Sprint 1 - Checklist Detalhado

**Período**: 22 Nov - 29 Nov 2025 (1 semana)  
**Objetivo**: Fundação Crítica para MVP - Restrição Geográfica + Integração de Módulos  
**Pré-requisito**: ✅ Migration .NET 10 + Aspire 13 merged para `master`

---

## 🎯 Visão Geral

| Branch | Duração | Prioridade | Testes Skipped Resolvidos |
|--------|---------|------------|---------------------------|
| `feature/geographic-restriction` | 1-2 dias | 🚨 CRÍTICA | N/A |
| `feature/module-integration` | 3-5 dias | 🚨 CRÍTICA | 8/8 (auth + isolation) |

**Total**: 7 dias úteis (com buffer para code review)

---

## 🗓️ Branch 1: `feature/geographic-restriction` (Dias 1-2)

### 📅 Dia 1 (22 Nov) - Setup & Middleware Core

#### Morning (4h)
- [ ] **Criar branch e estrutura**
  ```bash
  git checkout master
  git pull origin master
  git checkout -b feature/geographic-restriction
  ```

- [ ] **Criar GeographicRestrictionMiddleware**
  - [ ] Arquivo: `src/Shared/Middleware/GeographicRestrictionMiddleware.cs`
  - [ ] Implementar lógica de validação de cidade/estado
  - [ ] Suportar whitelist via `appsettings.json`
  - [ ] Retornar 451 Unavailable For Legal Reasons quando bloqueado
  - [ ] Logs estruturados (Serilog) com cidade/estado rejeitados

  **Exemplo de estrutura**:
  ```csharp
  public class GeographicRestrictionMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ILogger<GeographicRestrictionMiddleware> _logger;
      private readonly GeographicRestrictionOptions _options;

      public async Task InvokeAsync(HttpContext context)
      {
          // Extrair localização do IP ou header X-User-Location
          // Validar contra AllowedCities/AllowedStates
          // Bloquear ou permitir com log
      }
  }
  ```

- [ ] **Criar GeographicRestrictionOptions**
  - [ ] Arquivo: `src/Shared/Configuration/GeographicRestrictionOptions.cs`
  - [ ] Propriedades:
    - `bool Enabled { get; set; }`
    - `List<string> AllowedStates { get; set; }`
    - `List<string> AllowedCities { get; set; }`
    - `string BlockedMessage { get; set; }`

#### Afternoon (4h)
- [ ] **Configurar appsettings**
  - [ ] `src/Bootstrapper/MeAjudaAi.ApiService/appsettings.Development.json`:
    ```json
    "GeographicRestriction": {
      "Enabled": false,
      "AllowedStates": ["SP", "RJ", "MG"],
      "AllowedCities": ["São Paulo", "Rio de Janeiro", "Belo Horizonte"],
      "BlockedMessage": "Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}"
    }
    ```
  - [ ] `appsettings.Production.json`: `"Enabled": true`
  - [ ] `appsettings.Staging.json`: `"Enabled": true`

- [ ] **Registrar middleware no Program.cs**
  - [ ] Adicionar antes de `app.UseRouting()`:
    ```csharp
    app.UseMiddleware<GeographicRestrictionMiddleware>();
    ```
  - [ ] Configurar options no DI:
    ```csharp
    builder.Services.Configure<GeographicRestrictionOptions>(
        builder.Configuration.GetSection("GeographicRestriction")
    );
    ```

- [ ] **Feature Toggle (LaunchDarkly ou AppSettings)**
  - [ ] Implementar flag `geographic-restriction-enabled`
  - [ ] Permitir desabilitar via environment variable

---

### 📅 Dia 2 (23 Nov) - Testes & Documentação

#### Morning (4h)
- [ ] **Unit Tests**
  - [ ] Arquivo: `tests/MeAjudaAi.Shared.Tests/Middleware/GeographicRestrictionMiddlewareTests.cs`
  - [ ] Testar cenários:
    - [ ] Estado permitido → 200 OK
    - [ ] Cidade permitida → 200 OK
    - [ ] Estado bloqueado → 451 Unavailable
    - [ ] Cidade bloqueada → 451 Unavailable
    - [ ] Feature disabled → sempre 200 OK
    - [ ] IP sem localização → default behavior (permitir ou bloquear?)

- [ ] **Integration Tests**
  - [ ] Arquivo: `tests/MeAjudaAi.Integration.Tests/Middleware/GeographicRestrictionIntegrationTests.cs`
  - [ ] Testar com TestServer:
    - [ ] Header `X-User-Location: São Paulo, SP` → 200
    - [ ] Header `X-User-Location: Porto Alegre, RS` → 451
    - [ ] Sem header → default behavior

#### Afternoon (4h)
- [ ] **Documentação**
  - [ ] Atualizar `docs/configuration.md`:
    - [ ] Seção "Geographic Restriction"
    - [ ] Exemplos de configuração
    - [ ] Comportamento em cada ambiente
  - [ ] Criar `docs/middleware/geographic-restriction.md`:
    - [ ] Como funciona
    - [ ] Como configurar
    - [ ] Como testar localmente
    - [ ] Como desabilitar em emergency

- [ ] **Code Review Prep**
  - [ ] Rodar `dotnet format`
  - [ ] Rodar testes localmente: `dotnet test`
  - [ ] Verificar cobertura: `dotnet test --collect:"XPlat Code Coverage"`
  - [ ] Commit final e push:
    ```bash
    git add .
    git commit -m "feat: Add geographic restriction middleware

    - GeographicRestrictionMiddleware validates city/state
    - Feature toggle via appsettings
    - Returns 451 for blocked regions
    - Unit + integration tests (100% coverage)
    - Documented in docs/middleware/geographic-restriction.md"
    git push origin feature/geographic-restriction
    ```

- [ ] **Criar Pull Request**
  - [ ] Título: `feat: Geographic Restriction Middleware (Sprint 1)`
  - [ ] Descrição com checklist:
    - [ ] Middleware implementado
    - [ ] Testes passando (unit + integration)
    - [ ] Documentação completa
    - [ ] Feature toggle configurado
  - [ ] Assignar revisor
  - [ ] Aguardar CI/CD passar (GitHub Actions)

---

## 🗓️ Branch 2: `feature/module-integration` (Dias 3-7)

### 📅 Dia 3 (24 Nov) - Auth Handler Refactor + Setup

#### Morning (4h)
- [ ] **Criar branch**
  ```bash
  git checkout master
  git pull origin master
  git checkout -b feature/module-integration
  ```

- [ ] **🔧 CRÍTICO: Refatorar ConfigurableTestAuthenticationHandler**
  - [ ] Arquivo: `tests/MeAjudaAi.Shared.Tests/Auth/ConfigurableTestAuthenticationHandler.cs`
  - [ ] **Problema atual**: `SetAllowUnauthenticated(true)` força TODOS requests como Admin
  - [ ] **Solução**: Tornar comportamento granular
    ```csharp
    public static void SetAllowUnauthenticated(bool allow, UserRole defaultRole = UserRole.Anonymous)
    {
        _allowUnauthenticated = allow;
        _defaultRole = defaultRole; // Novo campo
    }
    ```
  - [ ] Modificar lógica em `HandleAuthenticateAsync`:
    ```csharp
    if (_currentConfigKey == null || !_userConfigs.TryGetValue(_currentConfigKey, out _))
    {
        if (!_allowUnauthenticated)
            return Task.FromResult(AuthenticateResult.Fail("No auth config"));
        
        // NOVO: Usar role configurável em vez de sempre Admin
        if (_defaultRole == UserRole.Anonymous)
            return Task.FromResult(AuthenticateResult.NoResult()); // Sem autenticação
        else
            ConfigureUser("anonymous", "anonymous@test.com", [], _defaultRole); // Authenticated mas sem permissões
    }
    ```

#### Afternoon (4h)
- [ ] **Reativar testes de autenticação**
  - [ ] Remover `Skip` de 5 testes auth-related:
    - [ ] `PermissionAuthorizationE2ETests.UserWithoutCreatePermission_CannotCreateUser`
    - [ ] `PermissionAuthorizationE2ETests.UserWithMultiplePermissions_HasAppropriateAccess`
    - [ ] `PermissionAuthorizationE2ETests.UserWithCreatePermission_CanCreateUser` ⚠️ NOVO (descoberto 21 Nov)
    - [ ] `ApiVersioningTests.ApiVersioning_ShouldWork_ForDifferentModules`
    - [ ] `ModuleIntegrationTests.CreateUser_ShouldTriggerDomainEvents` ⚠️ NOVO (descoberto 21 Nov)
  - [ ] Atualizar `TestContainerTestBase.cs`:
    ```csharp
    static TestContainerTestBase()
    {
        // CI/CD: Permitir não-autenticado mas NÃO forçar Admin
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(true, UserRole.Anonymous);
    }
    ```
  - [ ] Rodar testes localmente e validar que passam

- [ ] **Resolver race condition em CrossModuleCommunicationE2ETests**
  - [ ] Remover `Skip` dos 3 testes
  - [ ] Adicionar `await Task.Delay(100)` após `CreateUserAsync` (workaround temporário)
  - [ ] Investigar se TestContainers precisa de flush explícito
  - [ ] Rodar testes 10x consecutivas para garantir estabilidade

---

### 📅 Dia 4 (25 Nov) - Provider → Documents Integration

#### Morning (4h)
- [ ] **Criar IDocumentsModuleApi interface pública**
  - [ ] Arquivo: `src/Modules/Documents/API/IDocumentsModuleApi.cs`
  - [ ] Métodos:
    ```csharp
    Task<Result<bool>> HasVerifiedDocumentsAsync(Guid providerId, CancellationToken ct);
    Task<Result<List<DocumentDto>>> GetProviderDocumentsAsync(Guid providerId, CancellationToken ct);
    Task<Result<DocumentVerificationStatus>> GetDocumentStatusAsync(Guid documentId, CancellationToken ct);
    ```

- [ ] **Implementar DocumentsModuleApi**
  - [ ] Arquivo: `src/Modules/Documents/API/DocumentsModuleApi.cs`
  - [ ] Injetar `IDocumentsRepository` e implementar métodos
  - [ ] Adicionar logs estruturados (Serilog)
  - [ ] Retornar `Result<T>` para error handling consistente

#### Afternoon (4h)
- [ ] **Integrar em ProvidersModule**
  - [ ] Injetar `IDocumentsModuleApi` via DI
  - [ ] Adicionar validação em `CreateProviderCommandHandler`:
    ```csharp
    // Validar que provider tem documentos verificados antes de ativar
    var hasVerifiedDocs = await _documentsApi.HasVerifiedDocumentsAsync(providerId, ct);
    if (!hasVerifiedDocs.IsSuccess || !hasVerifiedDocs.Value)
        return Result.Failure("Provider precisa ter documentos verificados");
    ```

- [ ] **Integration Tests**
  - [ ] Arquivo: `tests/MeAjudaAi.Integration.Tests/Modules/ProviderDocumentsIntegrationTests.cs`
  - [ ] Cenários:
    - [ ] Provider com documentos verificados → pode ser ativado
    - [ ] Provider sem documentos → não pode ser ativado
    - [ ] Provider com documentos pendentes → não pode ser ativado

---

### 📅 Dia 5 (26 Nov) - Provider → ServiceCatalogs + Search Integration

#### Morning (4h)
- [ ] **Provider → ServiceCatalogs: Validação de serviços oferecidos**
  - [ ] Criar `IServiceCatalogsModuleApi.ValidateServicesAsync(List<Guid> serviceIds)`
  - [ ] Integrar em `CreateProviderCommandHandler`:
    ```csharp
    var validServices = await _serviceCatalogsApi.ValidateServicesAsync(provider.OfferedServiceIds, ct);
    if (validServices.FailedServiceIds.Any())
        return Result.Failure($"Serviços inválidos: {string.Join(", ", validServices.FailedServiceIds)}");
    ```
  - [ ] Integration tests para validação de serviços

#### Afternoon (4h)
- [ ] **Search → Providers: Sincronização de dados**
  - [ ] Criar `ProviderCreatedIntegrationEvent`
  - [ ] Criar `ProviderCreatedIntegrationEventHandler` no SearchModule:
    ```csharp
    public async Task Handle(ProviderCreatedIntegrationEvent evt, CancellationToken ct)
    {
        // Indexar provider no search index (Elasticsearch ou PostgreSQL FTS)
        await _searchRepository.IndexProviderAsync(evt.ProviderId, evt.Name, evt.Services, evt.Location);
    }
    ```
  - [ ] Publicar evento em `CreateProviderCommandHandler`
  - [ ] Integration test: criar provider → verificar que aparece no search

---

### 📅 Dia 6 (27 Nov) - Providers → Location Integration + E2E Tests

#### Morning (4h)
- [ ] **Providers → Location: Geocoding de endereços**
  - [ ] Criar `ILocationModuleApi.GeocodeAddressAsync(string address)`
  - [ ] Integrar em `CreateProviderCommandHandler`:
    ```csharp
    var geocoded = await _locationApi.GeocodeAddressAsync(provider.Address, ct);
    if (!geocoded.IsSuccess)
        return Result.Failure("Endereço inválido - não foi possível geocodificar");
    
    provider.SetCoordinates(geocoded.Value.Latitude, geocoded.Value.Longitude);
    ```
  - [ ] Mock de API externa (Google Maps/OpenStreetMap)
  - [ ] Fallback se geocoding falhar (usar coordenadas default da cidade)

#### Afternoon (4h)
- [ ] **Integration Tests End-to-End**
  - [ ] Arquivo: `tests/MeAjudaAi.E2E.Tests/Integration/ModuleIntegrationE2ETests.cs`
  - [ ] Cenário completo:
    ```csharp
    [Fact]
    public async Task CompleteProviderOnboarding_WithAllModuleIntegrations_Should_Succeed()
    {
        // 1. Criar provider (Providers module)
        var provider = await CreateProviderAsync();
        
        // 2. Upload documentos (Documents module)
        await UploadDocumentAsync(provider.Id, documentData);
        
        // 3. Associar serviços (ServiceCatalogs module)
        await AssociateServicesAsync(provider.Id, [serviceId1, serviceId2]);
        
        // 4. Geocodificar endereço (Location module)
        await GeocodeProviderAddressAsync(provider.Id);
        
        // 5. Ativar provider (trigger de sincronização)
        await ActivateProviderAsync(provider.Id);
        
        // 6. Verificar que aparece no search (Search module)
        var searchResults = await SearchProvidersAsync("São Paulo");
        searchResults.Should().Contain(p => p.Id == provider.Id);
    }
    ```

---

### 📅 Dia 7 (28-29 Nov) - Documentação, Code Review & Merge

#### Dia 7 Morning (4h)
- [ ] **Documentação completa**
  - [ ] Atualizar `docs/modules/README.md`:
    - [ ] Diagramas de integração entre módulos
    - [ ] Fluxo de dados cross-module
  - [ ] Criar `docs/integration/module-apis.md`:
    - [ ] Lista de todas as `IModuleApi` interfaces
    - [ ] Contratos e responsabilidades
    - [ ] Exemplos de uso
  - [ ] Atualizar `docs/architecture.md`:
    - [ ] Seção "Module Integration Patterns"
    - [ ] Event-driven communication
    - [ ] Direct API calls vs Events

#### Dia 7 Afternoon (4h)
- [ ] **Validação final**
  - [ ] Rodar todos os testes: `dotnet test --no-build`
  - [ ] Verificar cobertura: Deve estar > 45% (subiu de 40.51%)
  - [ ] Rodar testes E2E localmente com Aspire: `dotnet run --project src/Aspire/MeAjudaAi.AppHost`
  - [ ] Verificar logs estruturados (Serilog + Seq)
  - [ ] Performance test básico: criar 100 providers concorrentemente

- [ ] **Code Quality**
  - [ ] Rodar `dotnet format`
  - [ ] Rodar `dotnet build -warnaserror` (zero warnings)
  - [ ] Revisar TODO comments e documentá-los

- [ ] **Commit & Push**
  ```bash
  git add .
  git commit -m "feat: Module integration - Provider lifecycle with cross-module validation

  **Module APIs Implemented:**
  - IDocumentsModuleApi: Document verification for providers
  - IServiceCatalogsModuleApi: Service validation
  - ILocationModuleApi: Address geocoding
  - ISearchModuleApi: Provider indexing

  **Integration Events:**
  - ProviderCreatedIntegrationEvent → Search indexing
  - DocumentVerifiedIntegrationEvent → Provider activation

  **Tests Fixed:**
  - ✅ Refactored ConfigurableTestAuthenticationHandler (5 auth tests reactivated)
  - ✅ Fixed race condition in CrossModuleCommunicationE2ETests (3 tests reactivated)
  - ✅ Total: 98/100 E2E tests passing (98.0%)
  - ⚠️ Remaining: 2 skipped (DocumentsVerification + 1 race condition edge case)

  **Documentation:**
  - docs/integration/module-apis.md
  - docs/modules/README.md updated
  - Architecture diagrams added

  Closes #TBD (E2E test failures)
  Related to Sprint 1 - Foundation"
  
  git push origin feature/module-integration
  ```

#### Dia 7 Final (2h)
- [ ] **Criar Pull Request**
  - [ ] Título: `feat: Module Integration - Cross-module validation & sync (Sprint 1)`
  - [ ] Descrição detalhada:
    ```markdown
    ## 📋 Summary
    Implementa integração crítica entre módulos para validar lifecycle de Providers:
    - Provider → Documents: Verificação de documentos
    - Provider → ServiceCatalogs: Validação de serviços
    - Search → Providers: Sincronização de indexação
    - Providers → Location: Geocoding de endereços

    ## ✅ Checklist
    - [x] 4 Module APIs implementadas
    - [x] Integration events configurados
    - [x] 8 testes E2E reativados (98/100 passing)
    - [x] Documentação completa
    - [x] Code coverage > 45%

    ## 🧪 Tests
    - Unit: 100% coverage nos novos handlers
    - Integration: 15 novos testes
    - E2E: 98/100 passing (98.0%)

    ## 📚 Documentation
    - [x] docs/integration/module-apis.md
    - [x] docs/architecture.md updated
    - [x] API contracts documented
    ```
  - [ ] Assignar revisor
  - [ ] Marcar como "Ready for review"

---

## 📊 Métricas de Sucesso - Sprint 1

| Métrica | Antes (Sprint 0) | Meta Sprint 1 | Como Validar |
|---------|------------------|---------------|-------------|
| **E2E Tests Passing** | 93/100 (93.0%) | 98/100 (98.0%) | GitHub Actions PR |
| **E2E Tests Skipped** | 7 (auth + infra) | 2 (infra only) | dotnet test output |
| **Code Coverage** | 40.51% | > 45% | Coverlet report |
| **Build Warnings** | 0 | 0 | `dotnet build -warnaserror` |
| **Module APIs** | 0 | 4 | Code review |
| **Integration Events** | 0 | 2+ | Event handlers count |
| **Documentation Pages** | 15 | 18+ | `docs/` folder |

---

## 🚨 Bloqueadores Potenciais & Mitigação

| Bloqueador | Probabilidade | Impacto | Mitigação |
|------------|---------------|---------|-----------|
| Auth handler refactor quebra outros testes | Média | Alto | Rodar TODOS os testes após refactor |
| Race condition persiste em CI/CD | Média | Médio | Adicionar retry logic nos testes |
| Geocoding API externa falha | Baixa | Baixo | Implementar mock + fallback |
| Code review demora > 1 dia | Alta | Médio | Self-review rigoroso + CI/CD automático |

---

## 📝 Notas Importantes

### ⚠️ Testes Ainda Skipped (1/103)

Após Sprint 1, apenas **1 teste** permanecerá skipped:
- `RequestDocumentVerification_Should_UpdateStatus` (Azurite networking)
- **Plano**: Resolver no Sprint 2-3 quando implementar document verification completa

### 🔄 Dependências Externas

- **Geocoding API**: Usar mock em desenvolvimento, real em production
- **Elasticsearch**: Opcional para Sprint 1 (pode usar PostgreSQL FTS)
- **Aspire Dashboard**: Recomendado rodar localmente para debug

### 📅 Cronograma Realista

| Dia | Data | Atividades | Horas |
|-----|------|------------|-------|
| 1 | 22 Nov | Geographic Restriction (setup + middleware) | 8h |
| 2 | 23 Nov | Geographic Restriction (testes + docs) | 8h |
| 3 | 24 Nov | Module Integration (auth refactor + setup) | 8h |
| 4 | 25 Nov | Provider → Documents integration | 8h |
| 5 | 26 Nov | Provider → ServiceCatalogs + Search | 8h |
| 6 | 27 Nov | Providers → Location + E2E tests | 8h |
| 7 | 28-29 Nov | Documentação + Code Review | 6h |
| **Total** | | | **54h (7 dias úteis)** |

---

## ✅ Definition of Done - Sprint 1

### Branch 1: `feature/geographic-restriction`
- [ ] Middleware implementado e testado
- [ ] Feature toggle configurado
- [ ] Documentação completa
- [ ] CI/CD passa (0 warnings, 0 errors)
- [ ] Code review aprovado
- [ ] Merged para `master`

### Branch 2: `feature/module-integration`
- [ ] 4 Module APIs implementadas
- [ ] 6 testes E2E reativados e passando
- [ ] Integration events funcionando
- [ ] Cobertura de testes > 45%
- [ ] Documentação de integração completa
- [ ] CI/CD passa (102/103 testes)
- [ ] Code review aprovado
- [ ] Merged para `master`

---

**🎯 Meta Final**: Ao final do Sprint 1, o projeto deve estar com:
- ✅ Restrição geográfica funcional
- ✅ Módulos integrados via APIs + Events
- ✅ 99% dos testes E2E passando
- ✅ Fundação sólida para Sprint 2 (Frontend)

**Pronto para começar! 🚀**
