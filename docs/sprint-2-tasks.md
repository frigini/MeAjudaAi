# 📋 Sprint 2: Tarefas Detalhadas

**Período**: 3-16 Dezembro 2025 (2 semanas)  
**Branch**: `improve-tests-coverage` (criada em 25 Nov 2025)  
**Objetivo**: Coverage 35.11% → 80%+ | Criar .bru collections | Atualizar tools/ | Fix skipped tests

---

## 📊 Contexto Atual

- **Coverage**: 35.11% (caiu após migration devido a packages.lock.json + generated code)
- **Tests**: 2,076 total (2,065 passing - 99.5%, 11 skipped - 0.5%)
- **Skipped**: 11 tests (maioria E2E PostGIS/Azurite)
- **Módulos sem .bru**: Providers, Documents, SearchProviders, ServiceCatalogs, Locations
- **Tools desatualizados**: MigrationTool precisa EF Core 10

---

## ✅ Tarefa 1: Criar Bruno API Collections (.bru files)

**Estimativa**: 30 minutos  
**Prioridade**: Alta (facilita manual testing e documentação de APIs)

### Providers Module (tools/api-collections/Providers/)

- [ ] **create-provider.bru** - POST /api/v1/providers
  ```http
  POST {{baseUrl}}/api/v1/providers
  Content-Type: application/json
  Authorization: Bearer {{token}}

  {
    "name": "João Silva Serviços",
    "providerType": "Individual",
    "document": "12345678901",
    "email": "joao@example.com",
    "phoneNumber": "+55 11 98765-4321",
    "businessProfile": {
      "description": "Eletricista especializado em instalações residenciais",
      "yearsOfExperience": 5
    }
  }
  ```

- [ ] **get-provider.bru** - GET /api/v1/providers/{id}
- [ ] **list-providers.bru** - GET /api/v1/providers?status=Active&page=1&pageSize=20
- [ ] **update-provider.bru** - PUT /api/v1/providers/{id}
- [ ] **activate-provider.bru** - POST /api/v1/providers/{id}/activate
- [ ] **reject-provider.bru** - POST /api/v1/providers/{id}/reject

### Documents Module (tools/api-collections/Documents/)

- [ ] **upload-document.bru** - POST /api/v1/documents
- [ ] **get-document.bru** - GET /api/v1/documents/{id}
- [ ] **list-provider-documents.bru** - GET /api/v1/documents/provider/{providerId}
- [ ] **verify-document.bru** - POST /api/v1/documents/{id}/verify
- [ ] **reject-document.bru** - POST /api/v1/documents/{id}/reject

### SearchProviders Module (tools/api-collections/SearchProviders/)

- [ ] **search-providers.bru** - POST /api/v1/search
- [ ] **search-by-radius.bru** - POST /api/v1/search/radius
- [ ] **index-provider.bru** - POST /api/v1/search/providers/{id}/index
- [ ] **remove-provider.bru** - DELETE /api/v1/search/providers/{id}

### ServiceCatalogs Module (tools/api-collections/ServiceCatalogs/)

- [ ] **create-category.bru** - POST /api/v1/catalogs/categories
- [ ] **list-categories.bru** - GET /api/v1/catalogs/categories
- [ ] **create-service.bru** - POST /api/v1/catalogs/services
- [ ] **list-services.bru** - GET /api/v1/catalogs/services
- [ ] **get-services-by-category.bru** - GET /api/v1/catalogs/services/category/{categoryId}

### Locations Module (tools/api-collections/Locations/)

- [ ] **get-address-from-cep.bru** - GET /api/v1/locations/cep/{cep}
- [ ] **validate-city.bru** - POST /api/v1/locations/validate-city
- [ ] **get-city-details.bru** - GET /api/v1/locations/city/{cityName}

**Referência**: Usar tools/api-collections/Users/ como modelo

---

## 🔧 Tarefa 2: Atualizar Tools Projects

**Estimativa**: 15 minutos  
**Prioridade**: Média

### tools/MigrationTool

- [ ] Atualizar TargetFramework para `net10.0`
- [ ] Atualizar EF Core para `10.0.0-rc.1.24451.1`
- [ ] Atualizar Npgsql para `10.0.0-rc.1`
- [ ] Testar: `dotnet run --project tools/MigrationTool`
- [ ] Validar migrations executam sem erros

### tools/SeedDataTool (se existir)

- [ ] Atualizar dependências para .NET 10
- [ ] Adicionar seeders para ServiceCatalogs
  - [ ] 10 categorias padrão
  - [ ] 50 serviços comuns (distribuídos nas categorias)
- [ ] Testar seed data generation

---

## 🐛 Tarefa 3: Corrigir Testes Skipped

**Estimativa**: 45 minutos  
**Prioridade**: Alta

### PostGIS Skipped Tests (5 tests)

**Testes afetados**:
- SearchProviders_GetProvidersByRadius_ReturnsProvidersWithinRadius
- SearchProviders_SearchByCriteria_FiltersCorrectly

**Ações**:
- [ ] Identificar se é problema de setup PostGIS ou teste mal escrito
- [ ] Verificar se extensão PostGIS está habilitada no banco de teste
- [ ] Fix ou documentar como "requires PostGIS extension setup in test database"
- [ ] Considerar usar TestContainers com PostgreSQL+PostGIS pre-configurado

### Azurite Skipped Tests (2 tests)

**Testes afetados**:
- Documents_Upload_ReturnsCreatedDocument
- Documents_Verify_UpdatesStatus

**Ações**:
- [ ] Verificar se Azurite está rodando no CI/CD
- [ ] Adicionar step no workflow GitHub Actions para iniciar Azurite
- [ ] Atualizar connection string de teste
- [ ] Fix ou mover para integration tests locais

### Race Condition Tests (2 tests)

**Testes afetados**:
- Providers_ConcurrentActivation_HandlesCorrectly
- Documents_ConcurrentVerification_HandlesCorrectly

**Ações**:
- [ ] Usar locks ou semaphores para serializar operações críticas
- [ ] Implementar retry logic com backoff exponencial
- [ ] Adicionar idempotency keys para operações duplicadas
- [ ] Usar optimistic concurrency (rowversion/etag)

---

## ✅ Tarefa 4: Unit Tests (Coverage Target: 90%+ em Domain/Application)

**Estimativa**: 2-3 horas  
**Prioridade**: Crítica

### Providers Module

**Domain Layer**:
- [ ] Provider aggregate (state transitions, business rules)
  - [ ] Test: CreateProvider com todos os campos válidos
  - [ ] Test: UpdateBusinessProfile com dados válidos
  - [ ] Test: ActivateProvider valida HasVerifiedDocuments
  - [ ] Test: ActivateProvider bloqueia se HasRejectedDocuments
  - [ ] Test: RejectProvider com motivo obrigatório
  - [ ] Test: State transitions válidas (Draft → PendingVerification → Active)
  - [ ] Test: State transitions inválidas (Draft → Active) deve falhar

- [ ] ProviderBusinessProfile value object
  - [ ] Test: Validação de description mínimo/máximo caracteres
  - [ ] Test: YearsOfExperience não pode ser negativo

**Application Layer**:
- [ ] CreateProviderCommandHandler
  - [ ] Test: Handler cria provider com status Draft
  - [ ] Test: Handler valida documento único (CPF/CNPJ)
  - [ ] Test: Handler publica ProviderCreatedDomainEvent

- [ ] ActivateProviderCommandHandler
  - [ ] Test: Handler valida HasVerifiedDocuments via IDocumentsModuleApi
  - [ ] Test: Handler bloqueia se HasRejectedDocuments
  - [ ] Test: Handler publica ProviderActivatedIntegrationEvent
  - [ ] Test: Handler atualiza status para Active

- [ ] RejectProviderCommandHandler
  - [ ] Test: Handler valida motivo de rejeição não vazio
  - [ ] Test: Handler atualiza status para Rejected
  - [ ] Test: Handler publica ProviderRejectedDomainEvent

### Documents Module

**Domain Layer**:
- [ ] Document aggregate (status transitions)
  - [ ] Test: UploadDocument com FileUrl válido
  - [ ] Test: VerifyDocument atualiza VerifiedAt timestamp
  - [ ] Test: RejectDocument com RejectionReason obrigatório
  - [ ] Test: Status transitions válidas (Uploaded → PendingVerification → Verified)
  - [ ] Test: Status transitions inválidas (Uploaded → Verified) deve falhar

**Application Layer**:
- [ ] UploadDocumentCommandHandler
  - [ ] Test: Handler cria document com status Uploaded
  - [ ] Test: Handler valida ProviderId existe
  - [ ] Test: Handler publica DocumentUploadedDomainEvent

- [ ] VerifyDocumentCommandHandler
  - [ ] Test: Handler atualiza status para Verified
  - [ ] Test: Handler publica DocumentVerifiedDomainEvent
  - [ ] Test: Handler atualiza VerifiedAt timestamp

- [ ] RejectDocumentCommandHandler
  - [ ] Test: Handler valida RejectionReason não vazio
  - [ ] Test: Handler atualiza status para Rejected
  - [ ] Test: Handler publica DocumentRejectedDomainEvent

- [ ] DocumentStatusCountDto calculations
  - [ ] Test: Count por status (Uploaded, Verified, Rejected)
  - [ ] Test: HasVerifiedDocuments retorna true se pelo menos 1 verificado

### SearchProviders Module

**Domain Layer**:
- [ ] SearchableProvider read model
  - [ ] Test: GeoPoint validation (latitude/longitude limites)
  - [ ] Test: ServiceIds array não pode ser nulo

**Application Layer**:
- [ ] SearchProvidersQueryHandler
  - [ ] Test: Filtros por raio (radiusInKm)
  - [ ] Test: Filtros por serviceIds
  - [ ] Test: Filtros por minRating
  - [ ] Test: Filtros por subscriptionTiers
  - [ ] Test: Ranking multi-critério (tier → rating → distance)
  - [ ] Test: Paginação (pageNumber, pageSize)
  - [ ] Test: Raio não-positivo retorna lista vazia

### ServiceCatalogs Module

**Domain Layer**:
- [ ] ServiceCategory aggregate
  - [ ] Test: CreateCategory com nome único
  - [ ] Test: UpdateCategory valida nome não vazio
  - [ ] Test: ActivateCategory/DeactivateCategory toggle IsActive

- [ ] Service aggregate
  - [ ] Test: CreateService valida CategoryId existe
  - [ ] Test: CreateService valida categoria IsActive=true
  - [ ] Test: ChangeCategory valida nova categoria IsActive

**Application Layer**:
- [ ] CreateServiceCommandHandler
  - [ ] Test: Handler valida CategoryId via IServiceCatalogsModuleApi
  - [ ] Test: Handler bloqueia se categoria inativa
  - [ ] Test: Handler publica ServiceCreatedDomainEvent

- [ ] ChangeServiceCategoryCommandHandler
  - [ ] Test: Handler valida nova categoria existe
  - [ ] Test: Handler valida nova categoria IsActive
  - [ ] Test: Handler publica ServiceCategoryChangedDomainEvent

### Users Module

**Domain Layer**:
- [ ] User aggregate
  - [ ] Test: CreateUser valida email formato
  - [ ] Test: UpdateProfile valida campos obrigatórios

**Application Layer**:
- [ ] CreateUserCommandHandler
  - [ ] Test: Handler cria user com Keycloak ID
  - [ ] Test: Handler valida email único

- [ ] UpdateUserProfileCommandHandler
  - [ ] Test: Handler atualiza apenas campos permitidos
  - [ ] Test: Handler publica UserProfileUpdatedDomainEvent

### Locations Module

- [ ] ✅ Já possui 52 tests (manter coverage)
- [ ] Revisar se algum teste adicional pode ser adicionado

---

## 🧪 Tarefa 5: Integration Tests (Coverage Target: 70%+ em Infrastructure/API)

**Estimativa**: 1-2 horas  
**Prioridade**: Alta

### Providers Module

- [ ] **Fluxo completo de registro → verificação → aprovação**
  ```csharp
  [Fact]
  public async Task ProviderRegistrationFlow_FullCycle_Success()
  {
      // 1. POST /providers
      var createResponse = await CreateProviderAsync(validRequest);
      createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
      var providerId = createResponse.Data.Id;

      // 2. Upload documents
      await UploadDocumentAsync(providerId, identityDoc);
      await UploadDocumentAsync(providerId, proofOfResidence);

      // 3. Verify documents (admin action)
      await VerifyDocumentAsync(identityDocId);
      await VerifyDocumentAsync(proofOfResidenceId);

      // 4. Activate provider
      var activateResponse = await ActivateProviderAsync(providerId);
      activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

      // 5. Validate HasVerifiedDocuments blocking
      var provider = await GetProviderAsync(providerId);
      provider.Status.Should().Be(EProviderStatus.Active);
  }
  ```

- [ ] **Validar HasVerifiedDocuments blocking**
  ```csharp
  [Fact]
  public async Task ActivateProvider_WithoutVerifiedDocuments_ReturnsBadRequest()
  {
      var providerId = await CreateProviderWithoutDocuments();
      var response = await ActivateProviderAsync(providerId);
      
      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      response.Errors.Should().Contain(e => e.Contains("verified documents"));
  }
  ```

### Documents Module

- [ ] **Upload → OCR → Verificação → Rejection**
  ```csharp
  [Fact]
  public async Task DocumentWorkflow_VerificationSuccess()
  {
      // 1. POST /documents
      var uploadResponse = await UploadDocumentAsync(providerId, identityDoc);
      var documentId = uploadResponse.Data.Id;

      // 2. Verify document (admin action)
      var verifyResponse = await VerifyDocumentAsync(documentId);
      verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

      // 3. Assert status
      var document = await GetDocumentAsync(documentId);
      document.Status.Should().Be(EDocumentStatus.Verified);
      document.VerifiedAt.Should().NotBeNull();
  }

  [Fact]
  public async Task DocumentWorkflow_RejectionWithReason()
  {
      var documentId = await UploadDocumentAsync(providerId, badQualityDoc);
      
      var rejectResponse = await RejectDocumentAsync(documentId, "Imagem ilegível");
      rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

      var document = await GetDocumentAsync(documentId);
      document.Status.Should().Be(EDocumentStatus.Rejected);
      document.RejectionReason.Should().Be("Imagem ilegível");
  }
  ```

### SearchProviders Module

- [ ] **Busca geolocalizada com filtros + ranking**
  ```csharp
  [Fact]
  public async Task Search_WithFilters_ReturnsCorrectRanking()
  {
      // Arrange: Seed 3 providers
      var platinumProvider = await SeedProviderAsync(tier: Platinum, rating: 4.8, lat: -21.0, lon: -42.0);
      var goldProvider = await SeedProviderAsync(tier: Gold, rating: 4.9, lat: -21.01, lon: -42.01);
      var freeProvider = await SeedProviderAsync(tier: Free, rating: 5.0, lat: -21.02, lon: -42.02);

      // Act: Search by radius
      var searchResponse = await SearchProvidersAsync(
          latitude: -21.0, 
          longitude: -42.0, 
          radiusInKm: 10, 
          minRating: 4.5);

      // Assert: Ranking (tier → rating → distance)
      searchResponse.Data.Results.Should().HaveCount(3);
      searchResponse.Data.Results[0].Id.Should().Be(platinumProvider.Id); // Tier wins
      searchResponse.Data.Results[1].Id.Should().Be(goldProvider.Id);
      searchResponse.Data.Results[2].Id.Should().Be(freeProvider.Id);
  }
  ```

### ServiceCatalogs Module

- [ ] **CRUD de categorias e serviços via API**
  ```csharp
  [Fact]
  public async Task ServiceCatalog_FullCRUD_Success()
  {
      // 1. POST /categories
      var categoryResponse = await CreateCategoryAsync("Elétrica");
      var categoryId = categoryResponse.Data.Id;

      // 2. POST /services
      var serviceResponse = await CreateServiceAsync(categoryId, "Instalação de Tomadas");
      var serviceId = serviceResponse.Data.Id;

      // 3. GET /services/category/{id}
      var servicesResponse = await GetServicesByCategoryAsync(categoryId);
      servicesResponse.Data.Should().ContainSingle(s => s.Id == serviceId);
  }
  ```

---

## 🎯 Tarefa 6: E2E Tests (Scenarios críticos)

**Estimativa**: 1 hora  
**Prioridade**: Alta

### Registro de prestador end-to-end

```csharp
[Fact]
public async Task E2E_ProviderRegistrationToSearch_FullFlow()
{
    // 1. CreateProvider
    var provider = await CreateProviderAsync(validRequest);
    
    // 2. UploadDocuments
    await UploadDocumentAsync(provider.Id, identityDoc);
    await UploadDocumentAsync(provider.Id, proofOfResidence);
    
    // 3. VerifyDocuments (admin)
    await VerifyAllDocumentsAsync(provider.Id);
    
    // 4. ActivateProvider
    await ActivateProviderAsync(provider.Id);
    
    // 5. SearchProviders (assert provider appears)
    await Task.Delay(1000); // Wait for indexing
    var searchResponse = await SearchProvidersAsync(
        provider.Latitude, 
        provider.Longitude, 
        radiusInKm: 10);
    
    searchResponse.Data.Results.Should().Contain(p => p.Id == provider.Id);
}
```

### Busca geolocalizada com filtros

```csharp
[Fact]
public async Task E2E_SearchWithFilters_ReturnsFilteredResults()
{
    // Arrange: Seed providers
    var highRatingProvider = await SeedProviderAsync(rating: 4.8);
    var lowRatingProvider = await SeedProviderAsync(rating: 3.5);
    
    // Act: Search with minRating filter
    var response = await SearchProvidersAsync(
        latitude: -21.0, 
        longitude: -42.0, 
        radiusInKm: 50, 
        minRating: 4.0);
    
    // Assert: Only high rating provider returned
    response.Data.Results.Should().Contain(p => p.Id == highRatingProvider.Id);
    response.Data.Results.Should().NotContain(p => p.Id == lowRatingProvider.Id);
}
```

### Geographic restriction

```csharp
[Fact]
public async Task E2E_GeographicRestriction_BlocksDisallowedCity()
{
    // Arrange: Request with disallowed city
    var request = new CreateProviderRequest
    {
        Name = "Test Provider",
        City = "São Paulo", // Not in allowed list
        State = "SP"
    };
    
    // Act
    var response = await CreateProviderAsync(request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    response.Errors.Should().Contain(e => e.Contains("geographic restriction"));
}
```

### Provider indexing flow (integration event)

```csharp
[Fact]
public async Task E2E_ProviderActivation_TriggersIndexing()
{
    // 1. Create and activate provider
    var provider = await CreateAndActivateProviderAsync();
    
    // 2. Wait for background event processing
    await Task.Delay(2000);
    
    // 3. Assert SearchableProvider created
    var searchResponse = await SearchProvidersAsync(
        provider.Latitude, 
        provider.Longitude, 
        radiusInKm: 1);
    
    searchResponse.Data.Results.Should().Contain(p => p.Id == provider.Id);
    
    var indexed = searchResponse.Data.Results.First(p => p.Id == provider.Id);
    indexed.Name.Should().Be(provider.Name);
    indexed.SubscriptionTier.Should().Be(provider.SubscriptionTier);
}
```

---

## 📈 Objetivo Final

**Coverage Targets**:
- ✅ **Overall**: 35.11% → **80%+**
- ✅ **Domain Layer**: **90%+** (business logic crítico)
- ✅ **Application Layer**: **85%+** (command/query handlers)
- ✅ **Infrastructure Layer**: **70%+** (repositories, external integrations)
- ✅ **API Layer**: **70%+** (endpoints, middleware)

**Test Targets**:
- ✅ **Skipped tests**: 11 → **0-2** (apenas PostGIS/Azurite se não corrigíveis)
- ✅ **Total tests**: 2,076 → **2,300+** (+200 new unit tests)
- ✅ **Pass rate**: 99.5% → **100%** (exceto skipped justificados)

**API Collections**:
- ✅ **.bru files**: Users (existente) + 5 novos módulos = **6 módulos completos**
- ✅ **Total endpoints**: ~35 endpoints documentados

**Tools**:
- ✅ **MigrationTool**: Atualizado para .NET 10 + EF Core 10
- ✅ **SeedDataTool**: Seeders para ServiceCatalogs implementados

---

## 📝 Definition of Done

- [ ] Coverage ≥ 80% (verificar via `dotnet test --collect:"XPlat Code Coverage"`)
- [ ] Skipped tests ≤ 2 (todos justificados em docs/testing/skipped-tests-analysis.md)
- [ ] .bru collections criados para 5 módulos (Providers, Documents, SearchProviders, ServiceCatalogs, Locations)
- [ ] MigrationTool atualizado e testado
- [ ] Todos novos testes passando no CI/CD
- [ ] Code review aprovado
- [ ] Merge para master via PR
- [ ] Documentação atualizada (README.md, docs/testing/)

---

*📅 Criado em: 25 Novembro 2025*  
*📍 Branch: improve-tests-coverage*  
*🔄 Última atualização: 25 Novembro 2025*
