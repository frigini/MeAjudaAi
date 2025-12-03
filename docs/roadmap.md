# 🗺️ Roadmap - MeAjudaAi

Este documento consolida o planejamento estratégico e tático da plataforma MeAjudaAi, definindo fases de implementação, módulos prioritários e funcionalidades futuras.

---

## 📊 Sumário Executivo

**Projeto**: MeAjudaAi - Plataforma de Conexão entre Clientes e Prestadores de Serviços  
**Status Geral**: Fase 1 ✅ | Sprint 0 ✅ | Sprint 1 🔄 (Dia 1) | MVP Target: 31/Março/2025  
**Cobertura de Testes**: 28.69% → Meta 75-80% (Sprint 1)  
**Stack**: .NET 10 LTS + Aspire 13 + PostgreSQL + Blazor WASM + MAUI Hybrid

### Marcos Principais
- ✅ **Janeiro 2025**: Fase 1 concluída - 6 módulos core implementados
- ✅ **Jan 20 - 21 Nov**: Sprint 0 - Migration .NET 10 + Aspire 13 (CONCLUÍDO)
- 🔄 **22 Nov - 2 Dez**: Sprint 1 - Geographic Restriction + Module Integration + Test Coverage (DIAS 1-6 CONCLUÍDOS, FINALIZANDO)
- ⏳ **3 Dez - 16 Dez**: Sprint 2 - Test Coverage 80% + API Collections + Tools Update
- ⏳ **Dezembro 2025**: Sprint 3 - Frontend Blazor (Web)
- ⏳ **Fevereiro-Março 2025**: Sprints 4-6 - Frontend Blazor (Web + Mobile)
- 🎯 **31 Março 2025**: MVP Launch (Admin Portal + Customer App)
- 🔮 **Abril 2025+**: Fase 3 - Reviews, Assinaturas, Agendamentos

---

## 🎯 Status Atual

**✅ Fase 1: CONCLUÍDA** (Janeiro 2025)  
Todos os 6 módulos core implementados, testados e integrados:
- Users • Providers • Documents • Search & Discovery • Locations • ServiceCatalogs

**🔄 Fase 1.5: EM ANDAMENTO** (Novembro-Dezembro 2025)  
Fundação técnica para escalabilidade e produção:
- ✅ Migration .NET 10 + Aspire 13 (Sprint 0 - CONCLUÍDO 21 Nov)
- 🔄 Geographic Restriction + Module Integration (Sprint 1 - DIAS 1-6 CONCLUÍDOS, EM FINALIZAÇÂO)
- ⏳ Test Coverage 80% + API Collections + Tools Update (Sprint 2 - Planejado 3-16 Dez)
- ⏳ Frontend Blazor Admin Portal (Sprint 3 - Planejado)

**⏳ Fase 2: PLANEJADO** (Fevereiro-Março 2025)  
Frontend Blazor WASM + MAUI Hybrid:
- Admin Portal (Sprint 3)
- Customer App (Sprint 4)
- Polishing + Hardening (Sprint 5)

---

## 📖 Visão Geral

O roadmap está organizado em **cinco fases principais** para entrega incremental de valor:

1. **✅ Fase 1: Fundação (MVP Core)** - Registro de prestadores, busca geolocalizada, catálogo de serviços
2. **🔄 Fase 1.5: Fundação Técnica** - Migration .NET 10, integração, testes, observability
3. **🔮 Fase 2: Frontend & Experiência** - Blazor WASM Admin + Customer App
4. **🔮 Fase 3: Qualidade e Monetização** - Sistema de avaliações, assinaturas premium, verificação automatizada
5. **🔮 Fase 4: Experiência e Engajamento** - Agendamentos, comunicações, analytics avançado

A implementação segue os princípios arquiteturais definidos em `architecture.md`: **Modular Monolith**, **DDD**, **CQRS**, e **isolamento schema-per-module**.

---

## 📅 Cronograma de Sprints (Janeiro-Março 2025)

| Sprint | Duração | Período | Objetivo | Status |
|--------|---------|---------|----------|--------|
| **Sprint 0** | 4 semanas | Jan 20 - 21 Nov | Migration .NET 10 + Aspire 13 | ✅ CONCLUÍDO |
| **Sprint 1** | 10 dias | 22 Nov - 2 Dez | Geographic Restriction + Module Integration | 🔄 DIAS 1-6 CONCLUÍDOS |
| **Sprint 2** | 2 semanas | 3 Dez - 16 Dez | Test Coverage 80% + API Collections + Tools Update | ⏳ Planejado |
| **Sprint 3** | 2 semanas | 17 Dez - 31 Dez | Blazor Admin Portal (Web) | ⏳ Planejado |
| **Sprint 4** | 2 semanas | Feb 17 - Mar 2 | Blazor Admin Portal (Web) | ⏳ Planejado |
| **Sprint 5** | 3 semanas | Mar 3 - Mar 23 | Blazor Customer App (Web + Mobile) | ⏳ Planejado |
| **Sprint 6** | 1 semana | Mar 24 - Mar 30 | Polishing & Hardening (MVP Final) | ⏳ Planejado |

**MVP Launch Target**: 31 de Março de 2025 🎯

**Post-MVP (Fase 3+)**: Reviews, Assinaturas, Agendamentos (Abril 2025+)

---

## ✅ Fase 1: Fundação (MVP Core) - CONCLUÍDA

### Objetivo
Estabelecer as capacidades essenciais da plataforma: registro multi-etapas de prestadores com verificação, busca geolocalizada e catálogo de serviços.

### Status: ✅ CONCLUÍDA (Janeiro 2025)

**Todos os 6 módulos implementados, testados e integrados:**
1. ✅ **Users** - Autenticação, perfis, roles
2. ✅ **Providers** - Registro multi-etapas, verificação, gestão
3. ✅ **Documents** - Upload seguro, workflow de verificação
4. ✅ **Search & Discovery** - Busca geolocalizada com PostGIS
5. ✅ **Locations** - Lookup de CEP, geocoding, validações
6. ✅ **ServiceCatalogs** - Catálogo hierárquico de serviços

**Conquistas:**
- 28.69% test coverage (93/100 E2E passing, 296 unit tests)
- ⚠️ Coverage caiu após migration (packages.lock.json + generated code)
- APIs públicas (IModuleApi) implementadas para todos módulos
- Integration events funcionais entre módulos
- Health checks configurados
- CI/CD pipeline completo no GitHub Actions
- Documentação arquitetural completa + skipped tests tracker

### 1.1. ✅ Módulo Users (Concluído)
**Status**: Implementado e em produção

**Funcionalidades Entregues**:
- ✅ Registro e autenticação via Keycloak (OIDC)
- ✅ Gestão de perfil básica
- ✅ Sistema de roles e permissões
- ✅ Health checks e monitoramento
- ✅ API versionada com documentação OpenAPI

---

### 1.2. ✅ Módulo Providers (Concluído)

**Status**: Implementado e em produção

**Funcionalidades Entregues**:
- ✅ Provider aggregate com estados de registro (`EProviderStatus`: Draft, PendingVerification, Active, Suspended, Rejected)
- ✅ Múltiplos tipos de prestador (Individual, Company)
- ✅ Verificação de documentos integrada com módulo Documents
- ✅ BusinessProfile com informações de contato e identidade empresarial
- ✅ Gestão de qualificações e certificações
- ✅ Domain Events (`ProviderRegistered`, `ProviderVerified`, `ProviderRejected`)
- ✅ API pública (IProvidersModuleApi) para comunicação inter-módulos
- ✅ Queries por documento, cidade, estado, tipo e status de verificação
- ✅ Soft delete e auditoria completa

---

### 1.3. ✅ Módulo Documents (Concluído)

**Status**: Implementado e em produção

**Funcionalidades Entregues**:
- ✅ Upload seguro de documentos via Azure Blob Storage
- ✅ Tipos de documento suportados: IdentityDocument, ProofOfResidence, ProfessionalLicense, BusinessLicense
- ✅ Workflow de verificação com estados (`EDocumentStatus`: Uploaded, PendingVerification, Verified, Rejected, Failed)
- ✅ Integração completa com módulo Providers
- ✅ Domain Events (`DocumentUploaded`, `DocumentVerified`, `DocumentRejected`, `DocumentFailed`)
- ✅ API pública (IDocumentsModuleApi) para queries de documentos
- ✅ Verificações de integridade: HasVerifiedDocuments, HasRequiredDocuments, HasPendingDocuments
- ✅ Sistema de contadores por status (DocumentStatusCountDto)
- ✅ Suporte a OCR data extraction (campo OcrData para dados extraídos)
- ✅ Rejection/Failure reasons para auditoria

**Arquitetura Implementada**:
```csharp
// Document: Aggregate Root
public sealed class Document : AggregateRoot<DocumentId>
{
    public Guid ProviderId { get; }
    public EDocumentType DocumentType { get; } 
    public string FileUrl { get; } // Blob name/key no Azure Storage
    public string FileName { get; }
    public EDocumentStatus Status { get; }
    public DateTime UploadedAt { get; }
    public DateTime? VerifiedAt { get; }
    public string? RejectionReason { get; }
    public string? OcrData { get; }
}
```

**API Pública Implementada**:
```csharp
public interface IDocumentsModuleApi : IModuleApi
{
    Task<Result<ModuleDocumentDto?>> GetDocumentByIdAsync(Guid documentId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetProviderDocumentsAsync(Guid providerId, CancellationToken ct = default);
    Task<Result<ModuleDocumentStatusDto?>> GetDocumentStatusAsync(Guid documentId, CancellationToken ct = default);
    Task<Result<bool>> HasVerifiedDocumentsAsync(Guid providerId, CancellationToken ct = default);
    Task<Result<bool>> HasRequiredDocumentsAsync(Guid providerId, CancellationToken ct = default);
    Task<Result<DocumentStatusCountDto>> GetDocumentStatusCountAsync(Guid providerId, CancellationToken ct = default);
    Task<Result<bool>> HasPendingDocumentsAsync(Guid providerId, CancellationToken ct = default);
    Task<Result<bool>> HasRejectedDocumentsAsync(Guid providerId, CancellationToken ct = default);
}
```

**Próximas Melhorias (Fase 2)**:
- 🔄 Background worker para verificação automatizada via OCR
- 🔄 Integração com APIs governamentais para validação
- 🔄 Sistema de scoring automático baseado em qualidade de documentos

---

### 1.4. ✅ Módulo Search & Discovery (Concluído)

**Status**: Implementado e em produção

**Funcionalidades Entregues**:
- ✅ Busca geolocalizada com PostGIS nativo
- ✅ Read model denormalizado otimizado (SearchableProvider)
- ✅ Filtros por raio, serviços, rating mínimo e subscription tiers
- ✅ Ranking multi-critério (tier → rating → distância)
- ✅ Paginação server-side com contagem total
- ✅ Queries espaciais nativas (ST_DWithin, ST_Distance)
- ✅ Hybrid repository (EF Core + Dapper) para performance
- ✅ Validação de raio não-positivo (short-circuit)
- ✅ CancellationToken support para queries longas
- ✅ API pública (ISearchModuleApi)

**Arquitetura Implementada**:
```csharp
// SearchableProvider: Read Model
public sealed class SearchableProvider : AggregateRoot<SearchableProviderId>
{
    public Guid ProviderId { get; }
    public string Name { get; }
    public GeoPoint Location { get; } // Latitude, Longitude com PostGIS
    public decimal AverageRating { get; }
    public int TotalReviews { get; }
    public ESubscriptionTier SubscriptionTier { get; } // Free, Standard, Gold, Platinum
    public Guid[] ServiceIds { get; }
    public bool IsActive { get; }
    public string? Description { get; }
    public string? City { get; }
    public string? State { get; }
}
```

**API Pública Implementada**:
```csharp
public interface ISearchModuleApi
{
    Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        SubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
```

**Lógica de Ranking Implementada**:
1. ✅ Filtrar por raio usando `ST_DWithin` (índice GIST)
2. ✅ Ordenar por tier de assinatura (Platinum > Gold > Standard > Free)
3. ✅ Ordenar por avaliação média (descendente)
4. ✅ Ordenar por distância (crescente) como desempate

**Performance**:
- ✅ Queries espaciais executadas no banco (não in-memory)
- ✅ Índices GIST para geolocalização
- ✅ Paginação eficiente com OFFSET/LIMIT
- ✅ Count query separada para total

**Próximas Melhorias (Opcional)**:
- 🔄 Migração para Elasticsearch para maior escalabilidade (se necessário)
- 🔄 Indexing worker consumindo integration events (atualmente manual)
- 🔄 Caching de resultados para queries frequentes

---

### 1.5. ✅ Módulo Location Management (Concluído)

**Status**: Implementado e testado com integração IBGE ativa

**Objetivo**: Abstrair funcionalidades de geolocalização e lookup de CEP brasileiro.

**Funcionalidades Entregues**:
- ✅ ValueObjects: Cep, Coordinates, Address com validação completa
- ✅ Integração com APIs de CEP: ViaCEP, BrasilAPI, OpenCEP
- ✅ Fallback chain automático (ViaCEP → BrasilAPI → OpenCEP)
- ✅ Resiliência HTTP via ServiceDefaults (retry, circuit breaker, timeout)
- ✅ API pública (ILocationModuleApi) para comunicação inter-módulos
- ✅ **Integração IBGE API** (Sprint 1 Dia 1): Validação geográfica oficial
- ✅ Serviço de geocoding (stub para implementação futura)
- ✅ 52 testes unitários passando (100% coverage em ValueObjects)

**Arquitetura Implementada**:
```csharp
// ValueObjects
public sealed class Cep // Valida e formata CEP brasileiro (12345-678)
public sealed class Coordinates // Latitude/Longitude com validação de limites
public sealed class Address // Endereço completo com CEP, rua, bairro, cidade, UF

// API Pública
public interface ILocationModuleApi : IModuleApi
{
    Task<Result<AddressDto>> GetAddressFromCepAsync(string cep, CancellationToken ct = default);
    Task<Result<CoordinatesDto>> GetCoordinatesFromAddressAsync(string address, CancellationToken ct = default);
}
```

**Serviços Implementados**:
- `CepLookupService`: Implementa chain of responsibility com fallback entre provedores
- `ViaCepClient`, `BrasilApiCepClient`, `OpenCepClient`: Clients HTTP com resiliência
- **`IbgeClient`** (Novo): Cliente HTTP para IBGE Localidades API com normalização de nomes
- **`IbgeService`** (Novo): Validação de municípios com HybridCache (7 dias TTL)
- **`GeographicValidationService`** (Novo): Adapter pattern para integração com middleware
- `GeocodingService`: Stub (TODO: integração com Nominatim ou Google Maps API)

**Integração IBGE Implementada** (Sprint 1 Dia 1):
```csharp
// IbgeClient: Normalização de nomes (remove acentos, lowercase, hífens)
public Task<Municipio?> GetMunicipioByNameAsync(string cityName, CancellationToken ct = default);
public Task<List<Municipio>> GetMunicipiosByUFAsync(string ufSigla, CancellationToken ct = default);
public Task<bool> ValidateCityInStateAsync(string city, string state, CancellationToken ct = default);

// IbgeService: Business logic com cache (HybridCache, TTL: 7 dias)
public Task<bool> ValidateCityInAllowedRegionsAsync(
    string cityName, 
    string stateSigla, 
    List<string> allowedCities, 
    CancellationToken ct = default);
public Task<Municipio?> GetCityDetailsAsync(string cityName, CancellationToken ct = default);

// GeographicValidationService: Adapter para Shared module
public Task<bool> ValidateCityAsync(
    string cityName, 
    string stateSigla, 
    List<string> allowedCities, 
    CancellationToken ct = default);
```

**Observação**: IBGE integration provides city/state validation for geographic restriction; geocoding (lat/lon lookup) via Nominatim is planned for Sprint 3 (optional improvement).

**Modelos IBGE**:
- `Regiao`: Norte, Nordeste, Sudeste, Sul, Centro-Oeste
- `UF`: Unidade da Federação (estado) com região
- `Mesorregiao`: Mesorregião com UF
- `Microrregiao`: Microrregião com mesorregião
- `Municipio`: Município com hierarquia completa + helper methods (GetUF, GetEstadoSigla, GetNomeCompleto)

**API Base IBGE**: `https://servicodados.ibge.gov.br/api/v1/localidades/`

**Próximas Melhorias (Opcional)**:
- 🔄 Implementar GeocodingService com Nominatim (OpenStreetMap) ou Google Maps API
- 🔄 Adicionar caching Redis para reduzir chamadas às APIs externas (TTL: 24h para CEP, 7d para geocoding)
- ✅ ~~Integração com IBGE para lookup de municípios e estados~~ (IMPLEMENTADO)

---

### 1.6. ✅ Módulo ServiceCatalogs (Concluído)

**Status**: Implementado e funcional com testes completos

**Objetivo**: Gerenciar tipos de serviços que prestadores podem oferecer por catálogo gerenciado administrativamente.

#### **Arquitetura Implementada**
- **Padrão**: DDD + CQRS com hierarquia de categorias
- **Schema**: `service_catalogs` (isolado)
- **Naming**: snake_case no banco, PascalCase no código

#### **Entidades de Domínio Implementadas**
```csharp
// ServiceCategory: Aggregate Root
public sealed class ServiceCategory : AggregateRoot<ServiceCategoryId>
{
    public string Name { get; }
    public string? Description { get; }
    public bool IsActive { get; }
    public int DisplayOrder { get; }
    
    // Domain Events: Created, Updated, Activated, Deactivated
    // Business Rules: Nome único, validações de criação/atualização
}

// Service: Aggregate Root
public sealed class Service : AggregateRoot<ServiceId>
{
    public ServiceCategoryId CategoryId { get; }
    public string Name { get; }
    public string? Description { get; }
    public bool IsActive { get; }
    public int DisplayOrder { get; }
    
    // Domain Events: Created, Updated, Activated, Deactivated, CategoryChanged
    // Business Rules: Nome único, categoria ativa, validações
}
```

#### **Camadas Implementadas**

**1. Domain Layer** ✅
- `ServiceCategoryId` e `ServiceId` (strongly-typed IDs)
- Agregados com lógica de negócio completa
- 9 Domain Events (lifecycle completo)
- Repositórios: `IServiceCategoryRepository`, `IServiceRepository`
- Exception: `CatalogDomainException`

**2. Application Layer** ✅
- **DTOs**: ServiceCategoryDto, ServiceDto, ServiceListDto, ServiceCategoryWithCountDto
- **Commands** (11 total):
  - Categories: Create, Update, Activate, Deactivate, Delete
  - Services: Create, Update, ChangeCategory, Activate, Deactivate, Delete
- **Queries** (6 total):
  - Categories: GetById, GetAll, GetWithCount
  - Services: GetById, GetAll, GetByCategory
- **Handlers**: 11 Command Handlers + 6 Query Handlers
- **Module API**: `ServiceCatalogsModuleApi` para comunicação inter-módulos

**3. Infrastructure Layer** ✅
- `ServiceCatalogsDbContext` com schema isolation (`service_catalogs`)
- EF Core Configurations (snake_case, índices otimizados)
- Repositories com SaveChangesAsync integrado
- DI registration com auto-migration support

**4. API Layer** ✅
- **Endpoints REST** usando Minimal APIs pattern:
  - `GET /api/v1/catalogs/categories` - Listar categorias
  - `GET /api/v1/catalogs/categories/{id}` - Buscar categoria
  - `POST /api/v1/catalogs/categories` - Criar categoria
  - `PUT /api/v1/catalogs/categories/{id}` - Atualizar categoria
  - `POST /api/v1/catalogs/categories/{id}/activate` - Ativar
  - `POST /api/v1/catalogs/categories/{id}/deactivate` - Desativar
  - `DELETE /api/v1/catalogs/categories/{id}` - Deletar
  - `GET /api/v1/catalogs/services` - Listar serviços
  - `GET /api/v1/catalogs/services/{id}` - Buscar serviço
  - `GET /api/v1/catalogs/services/category/{categoryId}` - Por categoria
  - `POST /api/v1/catalogs/services` - Criar serviço
  - `PUT /api/v1/catalogs/services/{id}` - Atualizar serviço
  - `POST /api/v1/catalogs/services/{id}/change-category` - Mudar categoria
  - `POST /api/v1/catalogs/services/{id}/activate` - Ativar
  - `POST /api/v1/catalogs/services/{id}/deactivate` - Desativar
  - `DELETE /api/v1/catalogs/services/{id}` - Deletar
- **Autorização**: Todos endpoints requerem role Admin
- **Versionamento**: Sistema unificado via BaseEndpoint

**5. Shared.Contracts** ✅
- `IServiceCatalogsModuleApi` - Interface pública
- DTOs: ModuleServiceCategoryDto, ModuleServiceDto, ModuleServiceListDto, ModuleServiceValidationResultDto

#### **API Pública Implementada**
```csharp
public interface IServiceCatalogsModuleApi : IModuleApi
{
    Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(Guid serviceId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(Guid categoryId, bool activeOnly = true, CancellationToken ct = default);
    Task<Result<bool>> IsServiceActiveAsync(Guid serviceId, CancellationToken ct = default);
    Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(Guid[] serviceIds, CancellationToken ct = default);
}
```

#### **Status de Compilação**
- ✅ **Domain**: BUILD SUCCEEDED (3 warnings XML documentation)
- ✅ **Application**: BUILD SUCCEEDED (18 warnings SonarLint - não críticos)
- ✅ **Infrastructure**: BUILD SUCCEEDED
- ✅ **API**: BUILD SUCCEEDED
- ✅ **Adicionado à Solution**: 4 projetos integrados

#### **Integração com Outros Módulos**
- **Providers Module** (Planejado): Adicionar ProviderServices linking table
- **Search Module** (Planejado): Denormalizar services nos SearchableProvider
- **Admin Portal**: Endpoints prontos para gestão de catálogo

#### **Próximos Passos (Pós-MVP)**
1. **Testes**: Implementar unit tests e integration tests
2. **Migrations**: Criar e aplicar migration inicial do schema `service_catalogs`
3. **Bootstrap**: Integrar no Program.cs e AppHost
4. **Provider Integration**: Estender Providers para suportar ProviderServices
5. **Admin UI**: Interface para gestão de catálogo
6. **Seeders**: Popular catálogo inicial com serviços comuns

#### **Considerações Técnicas**
- **SaveChangesAsync**: Integrado nos repositórios (padrão do projeto)
- **Validações**: Nome único por categoria/serviço, categoria ativa para criar serviço
- **Soft Delete**: Não implementado (hard delete com validação de dependências)
- **Cascata**: DeleteServiceCategory valida se há serviços vinculados

#### **Schema do Banco de Dados**
```sql
-- Schema: service_catalogs
CREATE TABLE service_catalogs.service_categories (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    display_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

CREATE TABLE service_catalogs.services (
    id UUID PRIMARY KEY,
    category_id UUID NOT NULL REFERENCES service_catalogs.service_categories(id),
    name VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    display_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

CREATE INDEX idx_services_category_id ON service_catalogs.services(category_id);
CREATE INDEX idx_services_is_active ON service_catalogs.services(is_active);
CREATE INDEX idx_service_categories_is_active ON service_catalogs.service_categories(is_active);
```

---

## 🔄 Fase 1.5: Fundação Técnica (Em Andamento)

### Objetivo
Fortalecer a base técnica do sistema antes de desenvolver frontend, garantindo escalabilidade, qualidade e compatibilidade com .NET 10 LTS + Aspire 13.

### Justificativa
Com todos os 6 módulos core implementados (Fase 1 ✅), precisamos consolidar a fundação técnica antes de iniciar desenvolvimento frontend:
- **.NET 9 EOL**: Suporte expira em maio 2025, migrar para .NET 10 LTS agora evita migração em produção
- **Aspire 13**: Novas features de observability e orchestration
- **Test Coverage**: Atual 40.51% → objetivo 80%+ para manutenibilidade
- **Integração de Módulos**: IModuleApi implementado mas não utilizado com as regras de negócio reais
- **Restrição Geográfica**: MVP exige operação apenas em cidades piloto (SP, RJ, BH)

---

### 📅 Sprint 0: Migration .NET 10 + Aspire 13 (1-2 semanas)

**Status**: 🔄 EM ANDAMENTO (branch: `migration-to-dotnet-10`)

**Objetivos**:
- Migrar todos projetos para .NET 10 LTS
- Atualizar Aspire para v13
- Atualizar dependências (EF Core 10, Npgsql 10, etc.)
- Validar testes e corrigir breaking changes
- Atualizar CI/CD para usar .NET 10 SDK

**Tarefas**:
- [x] Criar branch `migration-to-dotnet-10`
- [x] Merge master (todos módulos Fase 1) ✅
- [x] Atualizar `Directory.Packages.props` para .NET 10 ✅
- [x] Atualizar todos `.csproj` para `<TargetFramework>net10.0</TargetFramework>` ✅
- [x] Atualizar Aspire packages para v13.x ✅
- [x] Atualizar EF Core para 10.x (RC) ✅
- [x] Atualizar Npgsql para 10.x (RC) ✅
- [x] `dotnet restore` executado com sucesso ✅
- [x] **Verificação Incremental**:
  - [x] Build Domain projects → ✅ sem erros
  - [x] Build Application projects → ✅ sem erros
  - [x] Build Infrastructure projects → ✅ sem erros
  - [x] Build API projects → ✅ sem erros
  - [x] Build completo → ✅ 0 warnings, 0 errors
  - [x] Fix testes Hangfire (Skip para CI/CD) ✅
  - [ ] Run unit tests → validar localmente
  - [ ] Run integration tests → validar localmente (exceto Hangfire que requer Aspire)
- [ ] Atualizar Azure DevOps pipeline YAML
- [ ] Validar Docker images com .NET 10
- [ ] Merge para master após validação completa

**Resultado Esperado**:
- ✅ Sistema rodando em .NET 10 LTS com Aspire 13
- ✅ Todos 296 testes passando
- ✅ CI/CD funcional
- ✅ Documentação atualizada

#### 📦 Pacotes com Versões Não-Estáveis ou Pendentes de Atualização

⚠️ **CRITICAL**: All packages listed below are Release Candidate (RC) or Preview versions.  
**DO NOT deploy to production** until stable versions are released. See [.NET 10 Release Timeline](https://github.com/dotnet/core/releases).

**Status da Migration**: A maioria dos pacotes core já está em .NET 10, mas alguns ainda estão em **RC (Release Candidate)** ou aguardando releases estáveis.

**Pacotes Atualizados (RC/Preview)**:
```xml
<!-- EF Core 10.x - RC -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0-rc.1.24451.1" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-rc.1.24451.1" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0-rc.1.24451.1" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0-rc.1.24451.1" />

<!-- Npgsql 10.x - RC -->
<PackageVersion Include="Npgsql" Version="10.0.0-rc.1" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0-rc.1" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite" Version="10.0.0-rc.1" />

<!-- Aspire 13.x - Preview -->
<PackageVersion Include="Aspire.Hosting" Version="13.0.0-preview.1" />
<PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="13.0.0-preview.1" />
<PackageVersion Include="Aspire.Npgsql" Version="13.0.0-preview.1" />
<PackageVersion Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="13.0.0-preview.1" />
<!-- ... outros pacotes Aspire em preview -->
```

**⚠️ Pacotes a Monitorar para Releases Estáveis**:

| Pacote | Versão Atual | Versão Estável Esperada | Impacto | Ação Requerida |
|--------|--------------|-------------------------|---------|----------------|
| **EF Core 10.x** | `10.0.0-rc.1.24451.1` | `10.0.0` (Nov-Dez 2025) | ALTO | Atualizar após release + testar migrations |
| **Npgsql 10.x** | `10.0.0-rc.1` | `10.0.0` (Nov-Dez 2025) | CRÍTICO | Revalidar Hangfire compatibility |
| **Aspire 13.x** | `13.0.0-preview.1` | `13.0.0` (Dez 2025) | MÉDIO | Atualizar orchestration configs |
| **Aspire.Npgsql.EntityFrameworkCore.PostgreSQL** | `13.0.0-preview.1` | `13.0.0` (Dez 2025) | ALTO | Sincronizar com Aspire 13 stable |
| **Hangfire.PostgreSql** | `1.20.12` | `2.0.0` (timeline desconhecida) | CRÍTICO | Monitorar <https://github.com/frankhommers/Hangfire.PostgreSql> |

**🔔 Monitoramento Automático de Releases**:

Para receber notificações quando novas versões estáveis forem lançadas, configure os seguintes alertas:

1. **GitHub Watch (Repositórios Open Source)**:
   - Acesse: <https://github.com/dotnet/efcore> → Click "Watch" → "Custom" → "Releases"
   - Acesse: <https://github.com/npgsql/npgsql> → Click "Watch" → "Custom" → "Releases"
   - Acesse: <https://github.com/dotnet/aspire> → Click "Watch" → "Custom" → "Releases"
   - Acesse: <https://github.com/frankhommers/Hangfire.PostgreSql> → Click "Watch" → "Custom" → "Releases"
   - **Benefício**: Notificação no GitHub e email quando nova release for publicada

2. **NuGet Package Monitoring (Via GitHub Dependabot)**:
   - Criar `.github/dependabot.yml` no repositório:
     ```yaml
     version: 2
     updates:
       - package-ecosystem: "nuget"
         directory: "/"
         schedule:
           interval: "weekly"
         open-pull-requests-limit: 10
         # Ignorar versões preview/rc se desejar apenas stable
         ignore:
           - dependency-name: "*"
             update-types: ["version-update:semver-major"]
     ```
   - **Benefício**: PRs automáticos quando novas versões forem detectadas

3. **NuGet.org Email Notifications**:
   - Acesse: <https://www.nuget.org/account> → "Change Email Preferences"
   - Habilite "Package update notifications"
   - **Limitação**: Não funciona para todos pacotes, depende do publisher

4. **Visual Studio / Rider IDE Alerts**:
   - **Visual Studio**: Tools → Options → NuGet Package Manager → "Check for updates automatically"
   - **Rider**: Settings → Build, Execution, Deployment → NuGet → "Check for package updates"
   - **Benefício**: Notificação visual no Solution Explorer

5. **dotnet outdated (CLI Tool)**:
   ```powershell
   # Instalar globalmente
   dotnet tool install --global dotnet-outdated-tool
   
   # Verificar pacotes desatualizados
   dotnet outdated
   
   # Verificar apenas pacotes major/minor desatualizados
   dotnet outdated --upgrade:Major
   
   # Automatizar verificação semanal (Task Scheduler / cron)
   # Windows Task Scheduler: Executar semanalmente
   # C:\Code\MeAjudaAi> dotnet outdated > outdated-report.txt
   ```
   - **Benefício**: Script automatizado para verificação periódica

6. **GitHub Actions Workflow (Recomendado)**:
   - Criar `.github/workflows/check-dependencies.yml`:
     ```yaml
     name: Check Outdated Dependencies
     
     on:
       schedule:
         - cron: '0 9 * * 1' # Toda segunda-feira às 9h
       workflow_dispatch: # Manual trigger
     
     jobs:
       check-outdated:
         runs-on: ubuntu-latest
         steps:
           - uses: actions/checkout@v6
           
           - name: Setup .NET
             uses: actions/setup-dotnet@v5
             with:
               dotnet-version: '10.x'
           
           - name: Install dotnet-outdated
             run: dotnet tool install --global dotnet-outdated-tool
           
           - name: Check for outdated packages
             run: |
               dotnet outdated > outdated-report.txt
               cat outdated-report.txt
           
           - name: Create Issue if outdated packages found
             if: success()
             uses: actions/github-script@v7
             with:
               script: |
                 const fs = require('fs');
                 const report = fs.readFileSync('outdated-report.txt', 'utf8');
                 if (report.includes('has newer versions')) {
                   github.rest.issues.create({
                     owner: context.repo.owner,
                     repo: context.repo.repo,
                     title: '[AUTOMATED] Outdated NuGet Packages Detected',
                     body: `\`\`\`\n${report}\n\`\`\``,
                     labels: ['dependencies', 'automated']
                   });
                 }
     ```
   - **Benefício**: Verificação automática semanal + criação de Issue no GitHub

**📋 Checklist de Monitoramento (Recomendado)**:
- [ ] Configurar GitHub Watch para dotnet/efcore
- [ ] Configurar GitHub Watch para npgsql/npgsql
- [ ] Configurar GitHub Watch para dotnet/aspire
- [ ] Configurar GitHub Watch para Hangfire.PostgreSql
- [ ] Instalar `dotnet-outdated-tool` globalmente
- [ ] Criar GitHub Actions workflow para verificação automática (`.github/workflows/check-dependencies.yml`)
- [ ] Configurar Dependabot (`.github/dependabot.yml`)
- [ ] Adicionar lembrete mensal no calendário para verificação manual (backup)

**🔍 Pacotes Críticos Sem Compatibilidade .NET 10 Confirmada**:

1. **Hangfire.PostgreSql 1.20.12**
   - **Status**: Compilado contra Npgsql 6.x
   - **Risco**: Breaking changes em Npgsql 10.x não validados pelo mantenedor
   - **Mitigação Atual**: Testes de integração (marcados como Skip no CI/CD)
   - **Monitoramento**: 
     - GitHub Issues: <https://github.com/frankhommers/Hangfire.PostgreSql/issues>
     - Alternativas: Hangfire.Pro.Redis (pago), Hangfire.SqlServer (outro DB)
   - **Prazo**: Validar localmente ANTES de deploy para produção

2. **Swashbuckle.AspNetCore 10.0.1**
   - **Status**: ExampleSchemaFilter desabilitado (IOpenApiSchema read-only)
   - **Impacto**: Exemplos automáticos não aparecem no Swagger UI
   - **Solução Temporária**: Comentado em DocumentationExtensions.cs
   - **Próximos Passos**: Investigar API do Swashbuckle 10.x ou usar reflexão
   - **Documentação**: `docs/technical-debt.md` seção ExampleSchemaFilter

**📅 Cronograma de Atualizações Futuras**:

```mermaid
gantt
    title Roadmap de Atualizações de Pacotes
    dateFormat  YYYY-MM-DD
    section EF Core
    RC → Stable           :2025-11-20, 2025-12-15
    Atualizar projeto     :2025-12-15, 7d
    section Npgsql
    RC → Stable           :2025-11-20, 2025-12-15
    Revalidar Hangfire    :2025-12-15, 7d
    section Aspire
    Preview → Stable      :2025-11-20, 2025-12-31
    Atualizar configs     :2025-12-31, 3d
    section Hangfire
    Monitorar upstream    :2025-11-20, 2026-06-30
```

**✅ Ações Imediatas Pós-Migration**:
1. ✅ Finalizar validação de testes (unit + integration)
2. ✅ Validar Hangfire localmente (com Aspire)
3. ⏳ Configurar GitHub Watch para monitoramento de releases (EF Core, Npgsql, Aspire)
4. ⏳ Instalar `dotnet-outdated-tool` e criar workflow de verificação automática
5. ⏳ Configurar Dependabot para PRs automáticos de updates
6. ⏳ Criar alerta para Hangfire.PostgreSql 2.0 (se/quando lançar)

**📝 Notas de Compatibilidade**:
- **EF Core 10 RC**: Sem breaking changes conhecidos desde RC.1
- **Npgsql 10 RC**: Breaking changes documentados em <https://www.npgsql.org/doc/release-notes/10.0.html>
- **Aspire 13 Preview**: API estável, apenas features novas em desenvolvimento

---

### 📅 Sprint 1: Geographic Restriction + Module Integration (10 dias)

**Status**: 🔄 DIAS 1-6 CONCLUÍDOS | FINALIZANDO (22-25 Nov 2025)  
**Branches**: `feature/geographic-restriction` (merged ✅), `feature/module-integration` (em review), `improve-tests-coverage` (criada)  
**Documentação**: [docs/testing/skipped-tests-analysis.md](./testing/skipped-tests-analysis.md)

**Conquistas**:
- ✅ Sprint 0 concluído: Migration .NET 10 + Aspire 13 merged (21 Nov)
- ✅ Middleware de restrição geográfica implementado com IBGE API integration
- ✅ 4 Module APIs implementados (Documents, ServiceCatalogs, SearchProviders, Locations)
- ✅ Testes reativados: 28 testes (11 AUTH + 9 IBGE + 2 ServiceCatalogs + 3 IBGE unavailability + 3 duplicates removed)
- ✅ Skipped tests reduzidos: 20 (26%) → 11 (11.5%) ⬇️ **-14.5%**
- ✅ Integration events: Providers → SearchProviders indexing
- ✅ Schema fixes: search_providers standardization
- ✅ CI/CD fix: Workflow secrets validation removido

**Objetivos Alcançados**:
- ✅ Implementar middleware de restrição geográfica (compliance legal)
- ✅ Implementar 4 Module APIs usando IModuleApi entre módulos
- ✅ Reativar 28 testes E2E skipped (auth refactor + race condition fixes)
- ✅ Integração cross-module: Providers ↔ Documents, Providers ↔ SearchProviders
- ⏳ Aumentar coverage: 35.11% → 80%+ (MOVIDO PARA SPRINT 2)

**Estrutura (2 Branches + Próxima Sprint)**:

#### Branch 1: `feature/geographic-restriction` (Dias 1-2) ✅ CONCLUÍDO
- [x] GeographicRestrictionMiddleware (validação cidade/estado) ✅
- [x] GeographicRestrictionOptions (configuration) ✅
- [x] Feature toggle (Development: disabled, Production: enabled) ✅
- [x] Unit tests (29 tests) + Integration tests (8 tests, skipped) ✅
- [x] **Integração IBGE API** (validação oficial de municípios) ✅
  - [x] IbgeClient com normalização de nomes (Muriaé → muriae) ✅
  - [x] IbgeService com HybridCache (7 dias TTL) ✅
  - [x] GeographicValidationService (adapter pattern) ✅
  - [x] 2-layer validation (IBGE primary, simple fallback) ✅
  - [x] 15 unit tests IbgeClient ✅
  - [x] Configuração de APIs (ViaCep, BrasilApi, OpenCep, IBGE) ✅
  - [x] Remoção de hardcoded URLs (enforce configuration) ✅
- [x] **Commit**: feat(locations): Integrate IBGE API for geographic validation (520069a) ✅
- **Target**: 28.69% → 30% coverage ✅ (CONCLUÍDO: 92/104 testes passando)
- **Merged**: 25 Nov 2025 ✅

#### Branch 2: `feature/module-integration` (Dias 3-10) ✅ DIAS 3-6 CONCLUÍDOS | 🔄 DIA 7-10 CODE REVIEW
- [x] **Dia 3**: Refactor ConfigurableTestAuthenticationHandler (reativou 11 AUTH tests) ✅
- [x] **Dia 3**: Fix race conditions (identificados 2 para Sprint 2) ✅
- [x] **Dia 4**: IDocumentsModuleApi implementation (7 métodos) ✅
- [x] **Dia 5**: IServiceCatalogsModuleApi (3 métodos stub) + ISearchModuleApi (2 novos métodos) ✅
- [x] **Dia 6**: Integration events (Providers → SearchProviders indexing) ✅
  - [x] DocumentVerifiedIntegrationEvent + handler ✅
  - [x] ProviderActivatedIntegrationEventHandler ✅
  - [x] SearchProviders schema fix (search → search_providers) ✅
  - [x] Clean InitialCreate migration ✅
- [x] **Dia 7**: Naming standardization (Module APIs) ✅
  - [x] ILocationModuleApi → ILocationsModuleApi ✅
  - [x] ISearchModuleApi → ISearchProvidersModuleApi ✅
  - [x] SearchModuleApi → SearchProvidersModuleApi ✅
  - [x] ProviderIndexingDto → ModuleProviderIndexingDto ✅
- [x] **Dia 7**: Test cleanup (remove diagnostics) ✅
- [ ] **Dia 7-10**: Code review & documentation 🔄
- **Target**: 30% → 35% coverage, 93/100 → 98/100 E2E tests
- **Atual**: 2,076 tests (2,065 passing - 99.5%, 11 skipped - 0.5%)
- **Commits**: 25+ total (583 commits total na branch)
- **Status**: Aguardando code review antes de merge

**Integrações Implementadas**:
- ✅ **Providers → Documents**: ActivateProviderCommandHandler valida documentos (4 checks)
- ✅ **Providers → SearchProviders**: ProviderActivatedIntegrationEventHandler indexa providers
- ✅ **Documents → Providers**: DocumentVerifiedDomainEventHandler publica integration event
- ⏳ **Providers → ServiceCatalogs**: API criada, aguarda implementação de gestão de serviços
- ⏳ **Providers → Locations**: CEP lookup (baixa prioridade)

**Bugs Críticos Corrigidos**:
- ✅ AUTH Race Condition (ConfigurableTestAuthenticationHandler thread-safety)
- ✅ IBGE Fail-Closed Bug (GeographicValidationService + IbgeService)
- ✅ MunicipioNotFoundException criada para fallback correto
- ✅ SearchProviders schema hardcoded (search → search_providers)

#### 🆕 Coverage Improvement: MOVIDO PARA SPRINT 2 ✅
- ⏳ Aumentar coverage 35.11% → 80%+ (+200 unit tests)
- ⏳ E2E test para provider indexing flow
- ⏳ Criar .bru API collections para 5 módulos restantes
- ⏳ Atualizar tools/ projects (MigrationTool, etc.)
- **Justificativa**: Focar em code review de qualidade antes de adicionar novos testes
- **Planejamento**: Sprint 2 dedicada (3-16 Dez) para coverage + collections + tools update

**Tarefas Detalhadas**:

#### 1. Integração Providers ↔ Documents ✅ CONCLUÍDO
- [x] Providers: Validar `HasVerifiedDocuments` antes de aprovar prestador ✅
- [x] Providers: Bloquear ativação se `HasRejectedDocuments` ou `HasPendingDocuments` ✅
- [ ] Documents: Publicar `DocumentVerified` event para atualizar status de Providers
- [ ] Integration test: Fluxo completo de verificação de prestador

#### 2. Integração Providers ↔ ServiceCatalogs ⏳ API CRIADA
- [ ] Providers: Adicionar `ProviderServices` linking table (many-to-many)
- [ ] Providers: Validar services via `IServiceCatalogsModuleApi.ValidateServicesAsync`
- [ ] Providers: Bloquear serviços inativos ou inexistentes
- [ ] Admin Portal: Endpoint para associar serviços a prestadores

#### 3. Integração SearchProviders ↔ Providers ✅ CONCLUÍDO
- [x] Search: Métodos IndexProviderAsync e RemoveProviderAsync implementados ✅
- [x] Search: Background handler consumindo ProviderVerificationStatusUpdated events ✅
- [ ] Search: Implementar full provider data sync via integration events
- [ ] Integration test: Busca retorna apenas prestadores verificados

#### 4. Integração Providers ↔ Locations ⏳ BAIXA PRIORIDADE
- [ ] Providers: Usar `ILocationModuleApi.GetAddressFromCepAsync` no registro
- [ ] Providers: Validar CEP existe antes de salvar endereço
- [ ] Providers: Auto-populate cidade/estado via Locations
- [ ] Unit test: Mock de ILocationModuleApi em Providers.Application

#### 5. Restrição Geográfica (MVP Blocker) ✅ CONCLUÍDO
- [x] Criar `AllowedCities` configuration em appsettings ✅
- [x] GeographicRestrictionMiddleware implementado com IBGE integration ✅
- [x] Fail-open fallback para validação simples quando IBGE unavailable ✅
- [ ] Admin: Endpoint para gerenciar cidades permitidas (Sprint 2)
- [x] Integration test: 24 testes passando ✅

**Resultado Alcançado (Sprint 1)**:
- ✅ Módulos integrados com business rules reais (Providers ↔ Documents, Providers ↔ SearchProviders)
- ✅ Operação restrita a cidades piloto configuradas (IBGE API validation)
- ✅ Background workers consumindo integration events (ProviderActivated, DocumentVerified)
- ✅ Validações cross-module funcionando (HasVerifiedDocuments, HasRejectedDocuments)
- ✅ Naming standardization (ILocationsModuleApi, ISearchProvidersModuleApi)
- ✅ CI/CD fix (secrets validation removido)
- 🔄 Code review pendente antes de merge

---

### 📅 Sprint 2: Test Coverage Improvement - Phase 1 (2 semanas)

**Status**: 🔄 EM ANDAMENTO (26 Nov - 2 Dez 2025)  
**Branches**: `improve-tests-coverage` (merged ✅), `improve-tests-coverage-2` (ativa - 5 commits)

**Conquistas (26 Nov - 2 Dez)**:
- ✅ **improve-tests-coverage** branch merged (39 novos testes Shared)
  - ✅ ValidationBehavior: 9 testes (+2-3% coverage)
  - ✅ TopicStrategySelector: 11 testes (+3% coverage)
  - ✅ Shared core classes: 39 unit tests total
  - ✅ Coverage pipeline habilitado para todos módulos
  - ✅ Roadmap documentado com análise completa de gaps
- ✅ **improve-tests-coverage-2** branch (2 Dez 2025 - 5 commits)
  - ✅ **Task 1 - PermissionMetricsService**: Concurrency fix (Dictionary → ConcurrentDictionary)
    - Commit: aabba3d - 813 testes passando (was 812)
  - ✅ **Task 2 - DbContext Transactions**: 10 testes criados (4 passing, 6 skipped/documented)
    - Commit: 5ff84df - DbContextTransactionTests.cs (458 lines)
    - Helper: ShortId() for 8-char GUIDs (Username max 30 chars)
    - 6 flaky tests documented (TestContainers concurrency issues)
  - ⏭️ **Task 3 - DbContextFactory**: SKIPPED (design-time only, não existe em runtime)
  - ⏭️ **Task 4 - SchemaIsolationInterceptor**: SKIPPED (component doesn't exist)
  - ✅ **Task 5 - Health Checks**: 47 testes totais (4 health checks cobertos)
    - Commit: 88eaef8 - ExternalServicesHealthCheck (9 testes, Keycloak availability)
    - Commit: 1ddbf4d - Refactor reflection removal (3 classes: internal → public)
    - Commit: fbf02b9 - HelpProcessing (9 testes) + DatabasePerformance (9 testes)
    - PerformanceHealthCheck: 20 testes (já existiam anteriormente)
  - ✅ **Code Quality**: Removida reflection de todos health checks (maintainability)
  - ✅ **Warning Fixes**: CA2000 reduzido de 16 → 5 (using statements adicionados)
  - ✅ **Shared Tests**: 841 testes passando (eram 813, +28 novos)

**Progresso Coverage (2 Dez 2025)**:
- Baseline: 45% (antes das branches - incluía código de teste)
- **Atual: 27.9%** (14,504/51,841 lines) - **MEDIÇÃO REAL excluindo código gerado**
  - **Com código gerado**: 28.2% (14,695/52,054 lines) - diferença de -0.3%
  - **Código gerado excluído**: 213 linhas (OpenApi.Generated, Runtime.CompilerServices, RegexGenerator)
  - **Análise Correta**: 27.9% é coverage do **código de produção escrito manualmente**
- **Branch Coverage**: 21.7% (2,264/10,422 branches) - sem código gerado
- **Method Coverage**: 40.9% (2,168/5,294 métodos) - sem código gerado
- **Test Suite**: 1,407 testes totais (1,393 passing - 99.0%, 14 skipped - 1.0%, 0 failing)
- Target Phase 1: 35% (+7.1 percentage points from 27.9% baseline)
- Target Final Sprint 2: 50%+ (revised from 80% - more realistic)

**Coverage por Assembly (Top 5 - Maiores)**:
1. **MeAjudaAi.Modules.Users.Tests**: 0% (test code, expected)
2. **MeAjudaAi.Modules.Users.Application**: 55.6% (handlers, queries, DTOs)
3. **MeAjudaAi.Modules.Users.Infrastructure**: 53.9% (Keycloak, repos, events)
4. **MeAjudaAi.Modules.Users.Domain**: 49.1% (entities, value objects, events)
5. **MeAjudaAi.Shared**: 41.2% (authorization, caching, behaviors)

**Coverage por Assembly (Bottom 5 - Gaps Críticos)**:
1. **MeAjudaAi.ServiceDefaults**: 20.7% (health checks, extensions) ⚠️
2. **MeAjudaAi.Modules.ServiceCatalogs.Domain**: 27.6% (domain events 25-50%)
3. **MeAjudaAi.Shared.Tests**: 7.3% (test infrastructure code)
4. **MeAjudaAi.ApiService**: 55.5% (middlewares, extensions) - better than expected
5. **MeAjudaAi.Modules.Users.API**: 31.8% (endpoints, extensions)

**Gaps Identificados (Coverage < 30%)**:
- ⚠️ **ServiceDefaults.HealthChecks**: 0% (ExternalServicesHealthCheck, PostgresHealthCheck, GeolocationHealth)
  - **Motivo**: Classes estão no ServiceDefaults (AppHost), não no Shared (testado)
  - **Ação**: Mover health checks para Shared.Monitoring ou criar testes no AppHost
- ⚠️ **Shared.Logging**: 0% (SerilogConfigurator, CorrelationIdEnricher, LoggingContextMiddleware)
  - **Ação**: Unit tests para enrichers, integration tests para middleware
- ⚠️ **Shared.Jobs**: 14.8% (HangfireExtensions, HangfireAuthorizationFilter)
  - **Motivo**: Hangfire testes skip no CI/CD (require Aspire DCP/Dashboard)
  - **Ação**: Local tests com Docker, ou mocks para unit tests
- ⚠️ **Shared.Messaging.RabbitMq**: 12% (RabbitMqMessageBus)
  - **Motivo**: Integration tests require RabbitMQ container
  - **Ação**: TestContainers RabbitMQ ou mocks
- ⚠️ **Shared.Database.Exceptions**: 17% (PostgreSqlExceptionProcessor)
  - **Ação**: Unit tests para constraint exception handling

**Progresso Phase 1 (Improve-Tests-Coverage-2)**:
- ✅ **5 Commits**: aabba3d, 5ff84df, 88eaef8, 1ddbf4d, fbf02b9
- ✅ **~40 New Tests**: Task 2 (10 DbContext) + Task 5 (27 health checks) + Task 1 (incremental fixes)
- ✅ **Test Success Rate**: 99.0% (1,393/1,407 passing)
- ✅ **Build Time**: ~25 minutes (full suite with Docker integration tests)
- ✅ **Health Checks Coverage**:
  - ✅ ExternalServicesHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - ✅ HelpProcessingHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - ✅ DatabasePerformanceHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - ✅ PerformanceHealthCheck: 20/20 (Shared/Monitoring) - 100% (pré-existente)
  - ❌ ServiceDefaults.HealthChecks.*: 0% (not in test scope yet)

**Technical Decisions Validated**:
- ✅ **No Reflection**: All health check classes changed from internal → public
  - Reason: "Não é para usar reflection, é difícil manter código com reflection"
  - Result: Direct instantiation `new MeAjudaAiHealthChecks.HealthCheckName(...)`
- ✅ **TestContainers**: Real PostgreSQL for integration tests (no InMemory)
  - Result: 4 core transaction tests passing, 6 advanced scenarios documented
- ✅ **Moq.Protected()**: HttpMessageHandler mocking for HttpClient tests
  - Result: 9 ExternalServicesHealthCheck tests passing
- ✅ **Flaky Test Documentation**: TestContainers concurrency issues documented, not ignored
  - Files: DbContextTransactionTests.cs (lines with Skip attribute + detailed explanations)

**Next Steps (Phase 1 Completion)**:
- ✅ **Coverage Report Generated**: coverage/report/index.html + Summary.txt
- ⏳ **Roadmap Update**: Document actual coverage achieved (IN PROGRESS)
- ⏳ **Commit Warning Fixes**: 11 CA2000 warnings fixed (16 → 5 remaining)
- ⏳ **Merge to Master**: After roadmap update + final code review

**Próximas Tarefas (Phase 2 - Nova Branch ou Continuação)**:
- [ ] **ServiceDefaults Health Checks** (Priority: CRITICAL - 0% coverage)
  - [ ] PostgresHealthCheck tests (connection validation)
  - [ ] GeolocationHealthOptions tests (ViaCEP, BrasilAPI, IBGE)
  - [ ] Mover health checks de ServiceDefaults para Shared (melhor testabilidade)
  - Estimativa: 15-20 testes, +3-5% coverage
  
- [ ] **Logging Infrastructure** (Priority: HIGH - 0% coverage)
  - [ ] SerilogConfigurator tests (configuration validation)
  - [ ] CorrelationIdEnricher tests (correlation ID injection)
  - [ ] LoggingContextMiddleware integration tests
  - Estimativa: 10-12 testes, +2% coverage
  
- [ ] **Messaging Resilience** (Priority: CRITICAL - 12% coverage)
  - [ ] RabbitMqMessageBus tests (TestContainers)
  - [ ] MessageRetryPolicyTests (circuit breaker, exponential backoff)
  - [ ] MessagePublisherTests (integration events)
  - [ ] MessageHandlerMiddlewareTests (retry + dead letter)
  - Estimativa: 20-25 testes, +5-8% coverage
  
- [ ] **Middlewares** (Priority: HIGH)
  - [ ] AuthorizationMiddleware tests (permission checks)
  - [ ] RateLimitingMiddleware tests (429 responses)
  - [ ] RequestValidationMiddleware tests
  - Estimativa: 12-15 testes, +2% coverage
  
- [ ] **Database Exception Handling** (Priority: MEDIUM - 17% coverage)
  - [ ] PostgreSqlExceptionProcessor tests (constraint violations)
  - [ ] UniqueConstraintException tests
  - [ ] ForeignKeyConstraintException tests
  - [ ] NotNullConstraintException tests
  - Estimativa: 15-20 testes, +3-4% coverage
  
- [ ] **Documents Module** (Priority: CRITICAL - needs baseline measurement)
  - [ ] DocumentVerificationWorkflow tests
  - [ ] OcrProcessingService tests (Azure Document Intelligence)
  - [ ] DocumentValidation tests (CPF/CNPJ/CNH formats)
  - Estimativa: 25-30 testes, +8-10% coverage

**Objetivos Fase 1 (Dias 1-7) - ✅ CONCLUÍDO 2 DEZ 2025**:
- ✅ Aumentar coverage Shared de baseline para 28.2% (medição real)
- ✅ Focar em componentes críticos (Health Checks - 4/7 implementados)
- ✅ Documentar testes flaky (6 TestContainers scope issues documented)
- ✅ **NO REFLECTION** - todas classes public para manutenibilidade
- ✅ 50 novos testes criados (5 commits, 1,393/1,407 passing)
- ✅ Coverage report consolidado gerado (HTML + Text)

**Objetivos Fase 2 (Dias 8-14) - ⏳ PLANEJADO**:
- ServiceDefaults: 20.7% → 35%+ (health checks + extensions)
- Shared.Logging: 0% → 30%+ (enrichers + middleware)
- Shared.Messaging: 12% → 40%+ (RabbitMQ + resilience)
- Shared.Database.Exceptions: 17% → 50%+ (constraint handling)
- **Overall Target**: 28.2% → 35%+ (+6.8 percentage points)

**Decisões Técnicas**:
- ✅ TestContainers para PostgreSQL (no InMemory databases)
- ✅ Moq para HttpMessageHandler (HttpClient mocking)
- ✅ FluentAssertions para assertions
- ✅ xUnit 3.1.5 como framework
- ✅ Classes public em vez de internal (no reflection needed)
- ⚠️ Testes flaky com concurrent scopes marcados como Skip (documentados)

**Health Checks Implementation**:
- ✅ **ExternalServicesHealthCheck**: Keycloak availability (9 testes - Shared/Monitoring)
- ✅ **PerformanceHealthCheck**: Memory, GC, thread pool (20 testes - Shared/Monitoring, pré-existente)
- ✅ **HelpProcessingHealthCheck**: Business logic operational (9 testes - Shared/Monitoring)
- ✅ **DatabasePerformanceHealthCheck**: DB metrics configured (9 testes - Shared/Monitoring)
- [ ] **ServiceDefaults.HealthChecks.PostgresHealthCheck**: Connection validation (0% - PENDING)
- [ ] **ServiceDefaults.HealthChecks.ExternalServicesHealthCheck**: Duplicate of Shared? (0% - INVESTIGATE)
- [ ] **Locations**: APIs de CEP health (ViaCEP, BrasilAPI) - PENDING
- [ ] **Documents**: Azure Blob Storage health - PENDING
- [ ] **Search**: PostGIS índices health - PENDING

**Arquitetura de Health Checks**:
- **Shared/Monitoring**: 4 health checks implementados e testados (47 testes, 100% coverage)
- **ServiceDefaults/HealthChecks**: Classes duplicadas ou diferentes? (0% coverage - needs investigation)
- **Decisão Técnica Requerida**: Consolidar health checks em uma única localização (Shared ou ServiceDefaults)

**Data Seeding** (MOVED TO SPRINT 3):
- [ ] Seeder de ServiceCatalogs: 10 categorias + 50 serviços
- [ ] Seeder de Providers: 20 prestadores fictícios
- [ ] Seeder de Users: Admin + 10 customers
- [ ] Script: `dotnet run --seed-dev-data`

**Resultado Esperado Sprint 2**:
- ✅ **Shared coverage**: 41.2% (baseline measurement - production code only)
- ✅ **Overall coverage**: 28.2% line, 22.3% branch, 41.1% method (ReportGenerator consolidated)
- ✅ **Health checks**: 47 testes passando (4/7 componentes cobertos - Shared/Monitoring)
- ✅ **Test suite**: 1,407 testes (1,393 passing - 99.0%, 14 skipped - 1.0%, 0 failing)
- ✅ **Build quality**: 5 CA2000 warnings remaining (down from 16 - using statements fixed)
- ✅ **Code quality**: Zero reflection in health checks (all classes public)
- ⏳ **Phase 2 Target**: 28.2% → 35%+ (ServiceDefaults, Logging, Messaging, Database.Exceptions)
- ⏳ **Phase 2 Tests**: +60-80 testes (ServiceDefaults 15-20, Logging 10-12, Messaging 20-25, DB 15-20, Middlewares 12-15)

### Phase 2 Task Breakdown & Release Gates

#### Coverage Targets (Progressive)
- **Minimum (CI Warning Threshold)**: Line 70%, Branch 60%, Method 70%
- **Recommended**: Line 85%, Branch 75%, Method 85%
- **Excellent**: Line 90%+, Branch 80%+, Method 90%+

**Note**: Current baseline (28.2%) is below minimum. Phase 2 targets (35%+) are intermediate milestones. Critical paths must reach 70%+ before production deployment.

#### Phase 2 Task Matrix

| Task | Priority | Estimated Tests | Target Coverage | Due Date | Definition of Done | Status |
|------|----------|-----------------|-----------------|----------|-------------------|--------|
| ServiceDefaults.HealthChecks | CRITICAL | 15-20 | 35%+ line | 2025-12-09 | All public health checks tested + no reflection | TODO |
| Shared.Logging | CRITICAL | 10-12 | 30%+ line | 2025-12-10 | Core logging scenarios covered | TODO |
| Shared.Messaging.RabbitMq | CRITICAL | 20-25 | 40%+ line | 2025-12-12 | Publish/consume/error handling tested | TODO |
| Shared.Database.Exceptions | HIGH | 15-20 | 50%+ line | 2025-12-13 | All exception types + handlers covered | TODO |
| Shared.Middlewares | HIGH | 12-15 | 45%+ line | 2025-12-16 | Request/response pipelines tested | TODO |

#### Release Gate Criteria

**Phase 2 Merge to Master** (Required):
- [ ] Line Coverage: 35%+ overall
- [ ] Health Checks: 100% for Shared/Monitoring components
- [ ] Test Suite: 1,467+ tests (current 1,407 + ~60 new)
- [ ] All Tests Passing: 99%+ (flaky tests <1%)
- [ ] Code Quality: 0 CRITICAL SonarQube violations, <10 MAJOR violations

**Production Deployment** (Gated - may lag merge by 1-2 days):
- [ ] Critical Paths: 70%+ for Users, Providers, Documents modules
- [ ] End-to-End Tests: All key user flows passing
- [ ] Performance: Health checks <500ms response time
- [ ] Security: No HIGH/CRITICAL vulnerabilities

**Decision**: Phase 2 can merge to master when all "Required" gates pass. Production deployment requires Critical Paths 70%+ threshold.

**Decisões Estratégicas para Sprint 2 Continuação**:
1. **Priorizar componentes críticos com 0% coverage**: ServiceDefaults.HealthChecks, Logging, Messaging.RabbitMq
2. **Investigar duplicação**: ServiceDefaults vs Shared health checks (consolidar arquitetura)
3. **TestContainers para infraestrutura**: RabbitMQ, PostgreSQL, Redis (integration tests)
4. **Documentar flaky tests**: 6 DbContext concurrency tests já documentados, padrão estabelecido
5. **Target realista**: 35% (+6.8pp) em vez de 80% original - base sólida para builds futuros

---

## 🚀 Próximos Passos Imediatos (Sprint 0 - EM ANDAMENTO)

### 1️⃣ Finalizar Migration .NET 10 + Aspire 13

**Branch Atual**: `migration-to-dotnet-10` (commit d7b06bc)

**Checklist de Migration**:
```bash
# 1. Verificar estado atual da migration branch
git log --oneline -5

# 2. Verificar arquivos já modificados (se houver)
git status

# 3. Atualizar Directory.Packages.props
# - .NET 10 packages (Microsoft.Extensions.*, System.*, etc.)
# - Aspire 13.x (Aspire.Hosting.*, Aspire.Npgsql, etc.)
# - EF Core 10.x
# - Npgsql 10.x

# 4. Atualizar todos .csproj para net10.0
# Usar script PowerShell ou find/replace em massa:
# <TargetFramework>net9.0</TargetFramework> → <TargetFramework>net10.0</TargetFramework>

# 5. Rodar build completo
dotnet build

# 6. Rodar todos os testes
dotnet test --no-build

# 7. Verificar erros e breaking changes
# Consultar: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0

# 8. Atualizar Docker images (se aplicável)
# FROM mcr.microsoft.com/dotnet/aspnet:9.0 → FROM mcr.microsoft.com/dotnet/aspnet:10.0

# 9. Atualizar CI/CD pipeline (azure-pipelines.yml)
# - dotnet-version: '10.x'
# - Usar .NET 10 SDK na agent pool

# 10. Merge para master após validação
git checkout master
git merge migration-to-dotnet-10 --no-ff
git push origin master
```

**Recursos**:
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [Aspire 13 Release Notes](https://learn.microsoft.com/en-us/dotnet/aspire/whats-new)
- [EF Core 10 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)

**Estimativa**: 1-2 semanas (20-30 Jan 2025)

---

### 2️⃣ Após Conclusão da Migration

**Próximo Sprint**: Sprint 1 (Integração de Módulos + Restrição Geográfica)

Consultar seção "Sprint 1" acima para checklist detalhado.

---

## 🎨 Fase 2: Frontend & Experiência (Planejado)

### Objetivo
Desenvolver aplicações frontend usando Blazor WebAssembly (Web) e MAUI Blazor Hybrid (Mobile), aproveitando fullstack .NET para máxima reutilização de código.

---

### 📱 Stack Tecnológico ATUALIZADA

> **📝 Nota de Decisão Técnica** (Janeiro 2025):  
> Stack de frontend atualizado de **React + TypeScript** para **Blazor WASM + MAUI Hybrid**.  
> **Razão**: Maximizar reutilização de código entre web e mobile (70%+ de código compartilhado C#), melhor integração com ASP.NET Core Identity + Keycloak, e redução de complexidade DevOps (fullstack .NET). Ver justificativa completa abaixo.

**Decisão Estratégica**: Blazor WASM + MAUI Hybrid (fullstack .NET)

**Justificativa**:
- ✅ **Compartilhamento de Código**: C# end-to-end, compartilhar DTOs, validators, business logic
- ✅ **Integração com Identity**: Melhor integração nativa com ASP.NET Core Identity + Keycloak
- ✅ **Performance**: AOT compilation no Blazor WASM (carregamento rápido)
- ✅ **Mobile Nativo**: MAUI Blazor Hybrid permite usar APIs nativas do device
- ✅ **Ecossistema**: Um único stack .NET reduz complexidade de DevOps
- ✅ **Evolução**: Preparado para futuras features (notificações push, geolocalização nativa)

**Stack Completa**:
- **Web Admin Portal**: Blazor WebAssembly (AOT enabled)
- **Web Customer App**: Blazor WebAssembly (AOT enabled)
- **Mobile Customer App**: .NET MAUI Blazor Hybrid (iOS + Android)
- **UI Library**: MudBlazor (Material Design para Blazor)
- **State Management**: Fluxor (Flux/Redux para Blazor)
- **Auth**: Microsoft.AspNetCore.Components.WebAssembly.Authentication (OIDC)
- **API Client**: Refit + HttpClientFactory
- **Mapping**: AutoMapper compartilhado com backend

### 🗂️ Estrutura de Projetos Atualizada
```text
src/
├── Web/
│   ├── MeAjudaAi.Web.Admin/          # Blazor WASM Admin Portal
│   ├── MeAjudaAi.Web.Customer/       # Blazor WASM Customer App
│   └── MeAjudaAi.Web.Shared/         # Componentes compartilhados
├── Mobile/
│   └── MeAjudaAi.Mobile/             # .NET MAUI Blazor Hybrid
└── Shared/
    ├── MeAjudaAi.Shared.DTOs/        # DTOs compartilhados (backend + frontend)
    ├── MeAjudaAi.Shared.Validators/  # FluentValidation (backend + frontend)
    └── MeAjudaAi.Shared.Contracts/   # Interfaces de API (Refit)
```

### 🔐 Autenticação Atualizada
- **Protocolo**: OpenID Connect (OIDC)
- **Identity Provider**: Keycloak
- **Token Management**: `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
- **Storage**: Tokens em memória (WASM) + Secure Storage (MAUI)
- **Refresh**: Automático via OIDC interceptor

---

### 📅 Sprint 3: Blazor Admin Portal (2 semanas)

**Status**: ⏳ PLANEJADO

**Objetivos**:
- Portal administrativo para gestão de plataforma
- CRUD de prestadores, serviços, moderação
- Dashboard com métricas básicas
- **Gestão de Restrições Geográficas** (Sprint 1 dependency)

**Funcionalidades**:

#### 1. Autenticação e Autorização
- [ ] Login via Keycloak (role: Admin required)
- [ ] Logout
- [ ] Tela de acesso negado (403)

#### 2. Dashboard Principal
- [ ] Cards com KPIs: Total Providers, Pending Verifications, Active Services, Total Reviews
- [ ] Gráfico de registros de prestadores (últimos 30 dias)
- [ ] Lista de ações pendentes (documentos para verificar, reviews flagged)

#### 3. Gestão de Prestadores
- [ ] **Listagem**: Tabela com filtros (status, cidade, tier, services)
- [ ] **Detalhes**: Ver perfil completo + documentos + serviços
- [ ] **Ações**: Aprovar, Rejeitar, Suspender, Reativar
- [ ] **Histórico**: Audit log de alterações

#### 4. Gestão de Documentos
- [ ] **Fila de Verificação**: Listar documentos pendentes (ordered by upload date)
- [ ] **Visualizador**: Exibir documento no browser (PDF/Image viewer)
- [ ] **Ações**: Verificar, Rejeitar (com motivo)
- [ ] **OCR Data**: Exibir dados extraídos (se disponível)

#### 5. Gestão de Catálogo de Serviços
- [ ] **Categorias**: CRUD completo com drag-and-drop para reordenar
- [ ] **Serviços**: CRUD completo com seleção de categoria
- [ ] **Ativar/Desativar**: Toggle switch para cada item
- [ ] **Preview**: Exibir hierarquia completa do catálogo

#### 6. 🆕 Gestão de Restrições Geográficas
> **⚠️ CRITICAL**: Feature implementada no Sprint 1 Dia 1 requer UI administrativa para produção.

**Contexto**: O middleware `GeographicRestrictionMiddleware` suporta configuração dinâmica via `Microsoft.FeatureManagement`, mas atualmente as cidades/estados permitidos são gerenciados via `appsettings.json` (requer redeploy). Esta seção implementa gestão via banco de dados com UI administrativa.

**Decisões de Arquitetura (Sprint 1 Dia 1 - 21 Nov 2025)**:

1. **Localização de Código** ✅ **ATUALIZADO 21 Nov 2025**
   - ✅ **MOVIDO** `GeographicRestrictionMiddleware` para `ApiService/Middlewares` (específico para API HTTP)
   - ✅ **MOVIDO** `GeographicRestrictionOptions` para `ApiService/Options` (configuração lida de appsettings da API)
   - ✅ **MOVIDO** `FeatureFlags.cs` para `Shared/Constants` (constantes globais como AuthConstants, ValidationConstants)
   - ❌ **DELETADO** `Shared/Configuration/` (pasta vazia após movimentações)
   - ❌ **DELETADO** `Shared/Middleware/` (pasta vazia, middleware único movido para ApiService)
   - **Justificativa**: 
     - GeographicRestriction é feature **exclusiva da API HTTP** (não será usada por Workers/Background Jobs)
     - Options são lidas de appsettings que só existem em ApiService
     - FeatureFlags são constantes (similar a `AuthConstants.Claims.*`, `ValidationConstants.MaxLength.*`)
     - Middlewares genéricos já estão em pastas temáticas (Authorization/Middleware, Logging/, Monitoring/)

2. **Propósito da Feature Toggle** ✅
   - ✅ **Feature flag ativa/desativa TODA a restrição geográfica** (on/off global)
   - ✅ **Cidades individuais controladas via banco de dados** (Sprint 3 - tabela `allowed_regions`)
   - ✅ **Arquitetura proposta**:
     ```
     FeatureManagement:GeographicRestriction = true  → Liga TODA validação
         ↓
     allowed_regions.is_active = true              → Ativa cidade ESPECÍFICA
     ```
   - **MVP (Sprint 1)**: Feature toggle + appsettings (hardcoded cities)
   - **Sprint 3**: Migration para database-backed + Admin Portal UI

3. **Remoção de Redundância** ✅ **JÁ REMOVIDO**
   - ❌ **REMOVIDO**: Propriedade `GeographicRestrictionOptions.Enabled` (redundante com feature flag)
   - ❌ **REMOVIDO**: Verificação `|| !_options.Enabled` do middleware
   - ✅ **ÚNICA FONTE DE VERDADE**: `FeatureManagement:GeographicRestriction` (feature toggle)
   - **Justificativa**: Ter duas formas de habilitar/desabilitar causa confusão e potenciais conflitos.
   - **Benefício**: Menos configurações duplicadas, arquitetura mais clara e segura.

**Organização de Pastas** (21 Nov 2025):
```
src/
  Shared/
    Constants/
      FeatureFlags.cs          ← MOVIDO de Configuration/ (constantes globais)
      AuthConstants.cs         (existente)
      ValidationConstants.cs   (existente)
    Authorization/Middleware/  (middlewares de autorização)
    Logging/                   (LoggingContextMiddleware)
    Monitoring/                (BusinessMetricsMiddleware)
    Messaging/Handlers/        (MessageRetryMiddleware)
  
  Bootstrapper/MeAjudaAi.ApiService/
    Middlewares/
      GeographicRestrictionMiddleware.cs  ← MOVIDO de Shared/Middleware/
      RateLimitingMiddleware.cs           (específico HTTP)
      SecurityHeadersMiddleware.cs        (específico HTTP)
    Options/
      GeographicRestrictionOptions.cs     ← MOVIDO de Shared/Configuration/
      RateLimitOptions.cs                 (existente)
      CorsOptions.cs                      (existente)
```

**Arquitetura Proposta**:
```sql
-- Schema: geographic_restrictions (novo)
CREATE TABLE geographic_restrictions.allowed_regions (
    region_id UUID PRIMARY KEY,
    type VARCHAR(10) NOT NULL, -- 'City' ou 'State'
    city_name VARCHAR(200),
    state_code VARCHAR(2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    added_at TIMESTAMP NOT NULL,
    added_by_user_id UUID,
    notes TEXT
);

CREATE INDEX idx_allowed_regions_state ON geographic_restrictions.allowed_regions(state_code);
CREATE INDEX idx_allowed_regions_active ON geographic_restrictions.allowed_regions(is_active);
```

**Funcionalidades Admin Portal**:

- [ ] **Visualização de Restrições Atuais**
  - [ ] Tabela com cidades/estados permitidos
  - [ ] Filtros: Tipo (Cidade/Estado), Estado, Status (Ativo/Inativo)
  - [ ] Ordenação: Alfabética, Data de Adição
  - [ ] Indicador visual: Badgets para "Cidade" vs "Estado"

- [ ] **Adicionar Cidade/Estado**
  - [ ] Form com campos:
    - Tipo: Dropdown (Cidade, Estado)
    - Estado: Dropdown preenchido via IBGE API (27 UFs)
    - Cidade: Autocomplete via IBGE API (se tipo=Cidade)
    - Notas: Campo opcional (ex: "Piloto Beta Q1 2025")
  - [ ] Validações:
    - Estado deve ser sigla válida (RJ, SP, MG, etc.)
    - Cidade deve existir no IBGE (validação server-side)
    - Não permitir duplicatas (cidade+estado único)
  - [ ] Preview: "Você está adicionando: Muriaé/MG"

- [ ] **Editar Região**
  - [ ] Apenas permitir editar "Notas" e "Status"
  - [ ] Cidade/Estado são imutáveis (delete + re-add se necessário)
  - [ ] Confirmação antes de desativar região com prestadores ativos

- [ ] **Ativar/Desativar Região**
  - [ ] Toggle switch inline na tabela
  - [ ] Confirmação: "Desativar [Cidade/Estado] irá bloquear novos registros. Prestadores existentes não serão afetados."
  - [ ] Audit log: Registrar quem ativou/desativou e quando

- [ ] **Remover Região**
  - [ ] Botão de exclusão com confirmação dupla
  - [ ] Validação: Bloquear remoção se houver prestadores registrados nesta região
  - [ ] Mensagem: "Não é possível remover [Cidade]. Existem 15 prestadores registrados."

**Integração com Middleware** (Refactor Necessário):

**Abordagem 1: Database-First (Recomendado)**
```csharp
// GeographicRestrictionOptions (modificado)
public class GeographicRestrictionOptions
{
    public bool Enabled { get; set; }
    public string BlockedMessage { get; set; } = "...";
    
    // DEPRECATED: Remover após migration para database
    [Obsolete("Use database-backed AllowedRegionsService instead")]
    public List<string> AllowedCities { get; set; } = new();
    [Obsolete("Use database-backed AllowedRegionsService instead")]
    public List<string> AllowedStates { get; set; } = new();
}

// Novo serviço
public interface IAllowedRegionsService
{
    Task<List<string>> GetAllowedCitiesAsync(CancellationToken ct = default);
    Task<List<string>> GetAllowedStatesAsync(CancellationToken ct = default);
}

// GeographicRestrictionMiddleware (modificado)
public class GeographicRestrictionMiddleware
{
    private readonly IAllowedRegionsService _regionsService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Buscar listas do banco (com cache)
        var allowedCities = await _regionsService.GetAllowedCitiesAsync(ct);
        var allowedStates = await _regionsService.GetAllowedStatesAsync(ct);
        
        // Lógica de validação permanece igual
        if (!allowedCities.Contains(userCity) && !allowedStates.Contains(userState))
        {
            // Bloquear
        }
    }
}
```

**Abordagem 2: Hybrid (Fallback para appsettings)**
- Se banco estiver vazio, usar `appsettings.json`
- Migração gradual: Admin adiciona regiões no portal, depois remove de appsettings

**Cache Strategy**:
- Usar `HybridCache` (já implementado no `IbgeService`)
- TTL: 5 minutos (balanço entre performance e fresh data)
- Invalidação: Ao adicionar/remover/editar região no admin portal

**Migration Path**:
1. **Sprint 3 Semana 1**: Criar schema `geographic_restrictions` + tabela
2. **Sprint 3 Semana 1**: Implementar `AllowedRegionsService` com cache
3. **Sprint 3 Semana 1**: Refactor middleware para usar serviço (mantém fallback appsettings)
4. **Sprint 3 Semana 2**: Implementar CRUD endpoints no Admin API
5. **Sprint 3 Semana 2**: Implementar UI no Blazor Admin Portal
6. **Sprint 3 Pós-Deploy**: Popular banco com dados iniciais (Muriaé, Itaperuna, Linhares)
7. **Sprint 4**: Remover valores de appsettings.json (obsoleto)

**Testes Necessários**:
- [ ] Unit tests: `AllowedRegionsService` (CRUD + cache invalidation)
- [ ] Integration tests: Middleware com banco populado vs vazio
- [ ] E2E tests: Admin adiciona cidade → Middleware bloqueia outras cidades

**Documentação**:
- [ ] Admin User Guide: Como adicionar/remover cidades piloto
- [ ] Technical Debt: Marcar `AllowedCities` e `AllowedStates` como obsoletos

**⚠️ Breaking Changes**:
- ~~`GeographicRestrictionOptions.Enabled` será removido~~ ✅ **JÁ REMOVIDO** (Sprint 1 Dia 1)
  - **Motivo**: Redundante com feature toggle - fonte de verdade única
  - **Migração**: Usar apenas `FeatureManagement:GeographicRestriction` em appsettings
- `GeographicRestrictionOptions.AllowedCities/AllowedStates` será deprecado (Sprint 3)
  - **Migração**: Admin Portal populará tabela `allowed_regions` via UI

**Estimativa**:
- **Backend (API + Service)**: 2 dias
- **Frontend (Admin Portal UI)**: 2 dias
- **Migration + Testes**: 1 dia
- **Total**: 5 dias (dentro do Sprint 3 de 2 semanas)

#### 7. Moderação de Reviews (Preparação para Fase 3)
- [ ] **Listagem**: Reviews flagged/reportados
- [ ] **Ações**: Aprovar, Remover, Banir usuário
- [ ] Stub para módulo Reviews (a ser implementado na Fase 3)

**Tecnologias**:
- **Framework**: Blazor WebAssembly (.NET 10)
- **UI**: MudBlazor (Material Design)
- **State**: Fluxor (Flux/Redux pattern)
- **HTTP**: Refit + Polly (retry policies)
- **Charts**: ApexCharts.Blazor

**Resultado Esperado**:
- ✅ Admin Portal funcional e responsivo
- ✅ Todas operações CRUD implementadas
- ✅ Dashboard com métricas em tempo real
- ✅ Deploy em Azure Container Apps

---

### 📅 Sprint 4: Blazor Customer App (Web + Mobile) (3 semanas)

**Status**: ⏳ PLANEJADO

**Objetivos**:
- App para clientes (web + mobile)
- Busca de prestadores
- Gestão de perfil
- Histórico de interações

**Funcionalidades**:

#### 1. Blazor WASM (Web) - Semana 1-2

**Home & Busca**:
- [ ] **Landing Page**: Hero section + busca rápida
- [ ] **Busca Geolocalizada**: Campo de endereço/CEP + raio + serviços
- [ ] **Mapa Interativo**: Exibir prestadores no mapa (Leaflet.Blazor)
- [ ] **Listagem de Resultados**: Cards com foto, nome, rating, distância, tier badge
- [ ] **Filtros**: Rating mínimo, tier, disponibilidade
- [ ] **Ordenação**: Distância, Rating, Tier

**Perfil de Prestador**:
- [ ] **Visualização**: Foto, nome, descrição, serviços, rating, reviews
- [ ] **Contato**: Botão WhatsApp, telefone, email (MVP: links externos)
- [ ] **Galeria**: Fotos do trabalho (se disponível)
- [ ] **Reviews**: Listar avaliações de outros clientes (read-only, write em Fase 3)

**Meu Perfil**:
- [ ] **Editar**: Nome, foto, telefone, endereço
- [ ] **Histórico**: Prestadores contatados (tracking básico)
- [ ] **Configurações**: Preferências de notificações (stub para futuro)

#### 2. MAUI Blazor Hybrid (Mobile) - Semana 3

**Diferenças do Web**:
- [ ] **Geolocalização Nativa**: Usar GPS do device para busca automática
- [ ] **Câmera**: Permitir upload de foto de perfil via câmera
- [ ] **Notificações Push**: Stub para futuro (ex: prestador aceitou contato)
- [ ] **Deep Linking**: Abrir prestador via link compartilhado
- [ ] **Offline Mode**: Cache de última busca realizada

**Compartilhamento de Código**:
- [ ] Razor Components compartilhados entre Web e Mobile
- [ ] Services layer compartilhado (ISearchService, IProviderService)
- [ ] DTOs e Validators compartilhados via Shared.DTOs

**Tecnologias Mobile**:
- **Framework**: .NET MAUI 10 + Blazor Hybrid
- **UI**: MudBlazor (funciona em MAUI)
- **Maps**: MAUI Community Toolkit Maps
- **Storage**: Preferences API + Secure Storage

**Resultado Esperado**:
- ✅ Customer App (Web) publicado
- ✅ Customer App (Mobile) disponível em TestFlight (iOS) e Google Play Beta (Android)
- ✅ 70%+ código compartilhado entre Web e Mobile
- ✅ UX otimizada para mobile (gestures, navegação nativa)

---

### 📅 Sprint 5: Polishing & Hardening (1 semana)

**Status**: ⏳ PLANEJADO

**Objetivos**:
- Melhorias de UX/UI
- Rate limiting
- Logging avançado
- Documentação final

**Tarefas**:

#### 1. UX/UI Improvements
- [ ] **Loading States**: Skeletons em todas cargas assíncronas
- [ ] **Error Handling**: Mensagens friendly para todos erros (não mostrar stack traces)
- [ ] **Validação Client-Side**: FluentValidation compartilhado entre frontend e backend
- [ ] **Acessibilidade**: ARIA labels, teclado navigation, screen reader support
- [ ] **Dark Mode**: Suporte a tema escuro (MudBlazor built-in)

#### 2. Rate Limiting & Security
- [ ] **API Rate Limiting**: Aspire middleware (100 req/min por IP, 1000 req/min para authenticated users)
- [ ] **CORS**: Configurar origens permitidas (apenas domínios de produção)
- [ ] **CSRF Protection**: Tokens anti-forgery em forms
- [ ] **Security Headers**: HSTS, X-Frame-Options, CSP

#### 3. Logging & Monitoring
- [ ] **Frontend Logging**: Integração com Application Insights (Blazor WASM)
- [ ] **Error Tracking**: Sentry ou similar para erros em produção
- [ ] **Analytics**: Google Analytics ou Plausible para usage tracking

#### 4. Documentação
- [ ] **API Documentation**: Swagger/OpenAPI atualizado com exemplos
- [ ] **User Guide**: Guia de uso para Admin Portal
- [ ] **Developer Guide**: Como rodar localmente, como contribuir
- [ ] **Deployment Guide**: Deploy em Azure Container Apps (ARM templates ou Bicep)

**Resultado Esperado**:
- ✅ MVP production-ready
- ✅ Segurança hardened
- ✅ Documentação completa
- ✅ Monitoring configurado

---

## 🎯 Fase 3: Qualidade e Monetização

### Objetivo
Introduzir sistema de avaliações para ranking, modelo de assinaturas premium via Stripe, e verificação automatizada de documentos.

### 3.1. ⭐ Módulo Reviews & Ratings (Planejado)

**Objetivo**: Permitir que clientes avaliem prestadores, influenciando ranking de busca.

#### **Arquitetura Proposta**
- **Padrão**: Simple layered architecture
- **Agregação**: Cálculo de `AverageRating` via integration events (não real-time)

#### **Entidades de Domínio**
```csharp
// Review: Aggregate Root
public class Review
{
    public Guid ReviewId { get; }
    public Guid ProviderId { get; }
    public Guid CustomerId { get; }
    public int Rating { get; } // 1-5
    public string? Comment { get; }
    public DateTime CreatedAt { get; }
    public bool IsFlagged { get; } // Para moderação
}

// ProviderRating: Aggregate (ou parte do read model)
public class ProviderRating
{
    public Guid ProviderId { get; }
    public decimal AverageRating { get; }
    public int TotalReviews { get; }
    public DateTime LastUpdated { get; }
}
```

#### **API Pública (IReviewsModuleApi)**
```csharp
public interface IReviewsModuleApi : IModuleApi
{
    Task<Result> SubmitReviewAsync(SubmitReviewRequest request, CancellationToken ct = default);
    Task<Result<PagedList<ReviewDto>>> GetReviewsForProviderAsync(
        Guid providerId, 
        int page, 
        int pageSize, 
        CancellationToken ct = default);
    Task<Result> FlagReviewAsync(Guid reviewId, string reason, CancellationToken ct = default);
}
```

#### **Implementação**
1. **Schema**: Criar `meajudaai_reviews` com `reviews`, `provider_ratings`
2. **Submit Endpoint**: Validar que cliente pode avaliar (serviço contratado?)
3. **Rating Calculation**: Publicar `ReviewAddedIntegrationEvent` → Search module atualiza `AverageRating`
4. **Moderação**: Sistema de flag para reviews inapropriados
5. **Testes**: Unit tests para cálculo de média + integration tests para submission

---

### 3.2. 💳 Módulo Payments & Billing (Planejado)

**Objetivo**: Gerenciar assinaturas de prestadores via Stripe (Free, Standard, Gold, Platinum).

#### **Arquitetura Proposta**
- **Padrão**: Anti-Corruption Layer (ACL) sobre Stripe API
- **Isolamento**: Lógica de domínio protegida de mudanças na Stripe

#### **Entidades de Domínio**
```csharp
// Subscription: Aggregate Root
public class Subscription
{
    public Guid SubscriptionId { get; }
    public Guid ProviderId { get; }
    public string StripeSubscriptionId { get; }
    public ESubscriptionPlan Plan { get; } // Free, Standard, Gold, Platinum
    public ESubscriptionStatus Status { get; } // Active, Canceled, PastDue
    public DateTime StartDate { get; }
    public DateTime? EndDate { get; }
}

// BillingAttempt: Entity
public class BillingAttempt
{
    public Guid AttemptId { get; }
    public Guid SubscriptionId { get; }
    public decimal Amount { get; }
    public bool IsSuccessful { get; }
    public DateTime AttemptedAt { get; }
}
```

#### **API Pública (IBillingModuleApi)**
```csharp
public interface IBillingModuleApi : IModuleApi
{
    Task<Result<string>> CreateCheckoutSessionAsync(
        CreateCheckoutRequest request, 
        CancellationToken ct = default);
    Task<Result<SubscriptionDto>> GetSubscriptionForProviderAsync(
        Guid providerId, 
        CancellationToken ct = default);
}
```

#### **Implementação**
1. **Stripe Setup**: Configurar produtos e pricing plans no dashboard
2. **Webhook Endpoint**: Receber eventos Stripe (`checkout.session.completed`, `invoice.payment_succeeded`, `customer.subscription.deleted`)
3. **Event Handlers**: Atualizar status de `Subscription` baseado em eventos
4. **Checkout Session**: Gerar URL de checkout para frontend
5. **Integration Events**: Publicar `SubscriptionTierChangedIntegrationEvent` → Search module atualiza ranking
6. **Testes**: Integration tests com mock events da Stripe testing library

---

### 3.3. 🤖 Documents - Verificação Automatizada (Planejado - Fase 2)

**Objetivo**: Automatizar verificação de documentos via OCR e APIs governamentais.

**Funcionalidades Planejadas**:
- **OCR Inteligente**: Azure AI Vision para extrair texto de documentos
- **Validação de Dados**: Cross-check com dados fornecidos pelo prestador
- **Background Checks**: Integração com APIs de antecedentes criminais
- **Scoring Automático**: Sistema de pontuação baseado em qualidade de documentos

**Background Jobs**:
1. **DocumentUploadedHandler**: Trigger OCR processing
2. **OcrCompletedHandler**: Validar campos extraídos
3. **VerificationScheduler**: Agendar verificações periódicas

**Nota**: Infraestrutura básica já existe (campo OcrData, estados de verificação), falta implementar workers e integrações.

---

## 🚀 Fase 4: Experiência e Engajamento (Post-MVP)

### Objetivo
Melhorar experiência do usuário com agendamentos, comunicações centralizadas e analytics avançado.

### 4.1. 📅 Módulo Service Requests & Booking (Planejado)

**Objetivo**: Permitir que clientes solicitem serviços e agendem horários com prestadores.

#### **Funcionalidades**
- **Solicitação de Serviço**: Cliente descreve necessidade e localização
- **Matching**: Sistema sugere prestadores compatíveis
- **Agendamento**: Calendário integrado com disponibilidade de prestador
- **Notificações**: Lembretes automáticos via Communications module

---

### 4.2. 📧 Módulo Communications (Planejado)

**Objetivo**: Centralizar e orquestrar todas as comunicações da plataforma (email, SMS, push).

#### **Arquitetura Proposta**
- **Padrão**: Orchestrator Pattern
- **Canais**: Email (SendGrid/Mailgun), SMS (Twilio), Push (Firebase)

#### **API Pública (ICommunicationsModuleApi)**
```csharp
public interface ICommunicationsModuleApi : IModuleApi
{
    Task<Result> SendEmailAsync(EmailRequest request, CancellationToken ct = default);
    Task<Result> SendSmsAsync(SmsRequest request, CancellationToken ct = default);
    Task<Result> SendPushNotificationAsync(PushRequest request, CancellationToken ct = default);
}
```

#### **Event Handlers**
- `UserRegisteredIntegrationEvent` → Email de boas-vindas
- `ProviderVerificationFailedIntegrationEvent` → Notificação de rejeição
- `BookingConfirmedIntegrationEvent` → Lembrete de agendamento

#### **Implementação**
1. **Channel Handlers**: Implementar `IEmailService`, `ISmsService`, `IPushService`
2. **Template Engine**: Sistema de templates para mensagens (Razor, Handlebars)
3. **Queue Processing**: Background worker para processar fila de mensagens
4. **Retry Logic**: Polly para retry com backoff exponencial
5. **Testes**: Unit tests para handlers + integration tests com mock services

---

### 4.3. 📊 Módulo Analytics & Reporting (Planejado)

**Objetivo**: Capturar, processar e visualizar dados de negócio e operacionais.

#### **Arquitetura Proposta**
- **Padrão**: CQRS + Event Sourcing (para audit)
- **Metrics**: Façade sobre OpenTelemetry/Aspire
- **Audit**: Immutable event log de todas as atividades
- **Reporting**: Denormalized read models para queries rápidos

#### **API Pública (IAnalyticsModuleApi)**
```csharp
public interface IAnalyticsModuleApi : IModuleApi
{
    Task<Result<ReportDto>> GetReportAsync(ReportQuery query, CancellationToken ct = default);
    Task<Result<PagedList<AuditLogEntryDto>>> GetAuditHistoryAsync(
        AuditLogQuery query, 
        CancellationToken ct = default);
}
```

#### **Database Views**
```sql
-- vw_provider_summary: Visão holística de cada prestador
CREATE VIEW meajudaai_analytics.vw_provider_summary AS
SELECT 
    p.provider_id,
    p.name,
    p.status,
    p.join_date,
    s.subscription_tier,
    pr.average_rating,
    pr.total_reviews
FROM meajudaai_providers.providers p
LEFT JOIN meajudaai_billing.subscriptions s ON p.provider_id = s.provider_id
LEFT JOIN meajudaai_reviews.provider_ratings pr ON p.provider_id = pr.provider_id;

-- vw_financial_transactions: Consolidação de eventos financeiros
CREATE VIEW meajudaai_analytics.vw_financial_transactions AS
SELECT 
    ba.attempt_id AS transaction_id,
    s.provider_id,
    ba.amount,
    s.plan,
    ba.is_successful AS status,
    ba.attempted_at AS transaction_date
FROM meajudaai_billing.billing_attempts ba
JOIN meajudaai_billing.subscriptions s ON ba.subscription_id = s.subscription_id;

-- vw_audit_log_enriched: Audit log legível
CREATE VIEW meajudaai_analytics.vw_audit_log_enriched AS
SELECT 
    al.log_id,
    al.timestamp,
    al.event_name,
    al.actor_id,
    COALESCE(u.full_name, p.name) AS actor_name,
    al.entity_id,
    al.details_json
FROM meajudaai_analytics.audit_log al
LEFT JOIN meajudaai_users.users u ON al.actor_id = u.user_id
LEFT JOIN meajudaai_providers.providers p ON al.actor_id = p.provider_id;
```

#### **Implementação**
1. **Schema**: Criar `meajudaai_analytics` com `audit_log`, reporting tables
2. **Event Handlers**: Consumir todos integration events relevantes
3. **Metrics Integration**: Expor métricas customizadas via OpenTelemetry
4. **Reporting API**: Endpoints otimizados para leitura de relatórios
5. **Dashboards**: Integração com Aspire Dashboard e Grafana
6. **Testes**: Integration tests para event handlers + performance tests para reporting

---

## 🎯 Funcionalidades Adicionais Recomendadas (Fase 4+)

### 🛡️ Admin Portal - Módulos Avançados
**Funcionalidades Adicionais (Pós-MVP)**:
- **User & Provider Analytics**: Dashboards avançados com Grafana
- **Fraud Detection**: Sistema de scoring para detectar perfis suspeitos
- **Bulk Operations**: Ações em lote (ex: aprovar múltiplos documentos)
- **Audit Trail**: Histórico completo de todas ações administrativas

---

### 👤 Customer Profile Management (Alta Prioridade)
**Por quê**: Plano atual é muito focado em prestadores; clientes também precisam de gestão de perfil.

**Funcionalidades Core**:
- Editar informações básicas (nome, foto)
- Ver histórico de prestadores contatados
- Gerenciar reviews escritos
- Preferências de notificações

**Implementação**: Enhancement ao módulo Users existente

---

### ⚖️ Dispute Resolution System (Média Prioridade)
**Por quê**: Mesmo sem pagamentos in-app, disputas podem ocorrer (reviews injustos, má conduta).

**Funcionalidades Core**:
- Botão "Reportar" em perfis de prestadores e reviews
- Formulário para descrever problema
- Fila no Admin Portal para moderadores

**Implementação**: Novo módulo pequeno ou extensão do módulo Reviews

---

## 📊 Métricas de Sucesso

### 📈 Métricas de Produto
- **Crescimento de usuários**: 20% ao mês
- **Retenção de prestadores**: 85%
- **Satisfação média**: 4.5+ estrelas
- **Taxa de conversão (Free → Paid)**: 15%

### ⚡ Métricas Técnicas (SLOs)

#### **Tiered Performance Targets**

| Categoria | Tempo Alvo | Exemplo |
|-----------|------------|---------|
| **Consultas Simples** | <200ms | Busca por ID, dados em cache |
| **Consultas Médias** | <500ms | Listagens com filtros básicos |
| **Consultas Complexas** | <1000ms | Busca cross-module, agregações |
| **Consultas Analíticas** | <3000ms | Relatórios, dashboards |

#### **Baseline de Desempenho**
- **Assumindo**: Cache distribuído configurado, índices otimizados
- **Revisão**: Ajustes trimestrais baseados em métricas reais (P50, P95, P99)
- **Monitoramento**: OpenTelemetry + Aspire Dashboard

#### **Outros SLOs**
- **Disponibilidade**: 99.9% uptime
- **Segurança**: Zero vulnerabilidades críticas
- **Cobertura de Testes**: >80% para código crítico

---

## 🔄 Processo de Gestão do Roadmap

### 📅 Revisão Trimestral
- Avaliação de progresso contra milestones
- Ajuste de prioridades baseado em métricas
- Análise de feedback de usuários e prestadores

### 💬 Feedback Contínuo
- **Input da comunidade**: Surveys, suporte, analytics
- **Feedback de prestadores**: Portal dedicado para sugestões
- **Necessidades de negócio**: Alinhamento com stakeholders

### 🎯 Critérios de Priorização
1. **Impacto no MVP**: Funcionalidade é crítica para lançamento?
2. **Esforço de Implementação**: Complexidade técnica e tempo estimado
3. **Dependências**: Quais módulos dependem desta funcionalidade?
4. **Valor para Usuário**: Feedback qualitativo e quantitativo

---

## 📋 Sumário Executivo de Prioridades

### ✅ **Alta Prioridade (Próximos 6 meses - Fase 1)**
1. ✅ Módulo Users (Concluído)
2. ✅ Módulo Providers (Concluído)
3. ✅ Módulo Documents (Concluído)
4. ✅ Módulo Search & Discovery (Concluído)
5. 📋 Módulo Location - CEP lookup e geocoding
6. 📋 Módulo ServiceCatalogs - Catálogo admin-managed de categorias e serviços
7. 📋 Admin Portal - Gestão básica
8. 📋 Customer Profile - Gestão de perfil

### 🎯 **Média Prioridade (6-12 meses - Fase 2)**
1. ⭐ Módulo Reviews & Ratings
2. 💳 Módulo Payments & Billing (Stripe)
3. 🤖 Documents - Verificação automatizada (OCR + Background checks)
4. 🔄 Search - Indexing worker para integration events
5. 📊 Analytics - Métricas básicas
6. 📧 Communications - Email notifications
7. 🛡️ Dispute Resolution System

### 🔮 **Baixa Prioridade (12+ meses - Fase 3)**
1. 📅 Service Requests & Booking
2. 📱 Mobile Apps (iOS/Android nativo)
3. 🧠 Recomendações com ML
4. 🎮 Gamificação avançada
5. 💬 Chat interno
6. 🌐 Internacionalização

---

## 📚 Referências e Recursos

### 📖 Documentação Relacionada
- **Arquitetura**: [`docs/architecture.md`](./architecture.md) - Princípios e padrões arquiteturais
- **Desenvolvimento**: [`docs/development.md`](./development.md) - Guia de setup e workflow
- **Autenticação**: [`docs/authentication_and_authorization.md`](./authentication_and_authorization.md) - Keycloak e OIDC
- **CI/CD**: [`docs/ci_cd.md`](./ci_cd.md) - Pipeline e deployment

### 🔧 Ferramentas e Tecnologias
- **.NET 10.0** - Runtime principal (migrado de .NET 9.0)
- **PostgreSQL + PostGIS** - Database com suporte geoespacial
- **Keycloak** - Identity & Access Management
- **Stripe** - Payment processing
- **Azure Blob Storage** - Document storage
- **OpenTelemetry + Aspire** - Observability

### 🌐 APIs Externas
- **IBGE Localidades API** - Validação oficial de municípios brasileiros
  - Base URL: `https://servicodados.ibge.gov.br/api/v1/localidades/`
  - Documentação: <https://servicodados.ibge.gov.br/api/docs/localidades>
  - Uso: Validação geográfica para restrição de cidades piloto
- **Nominatim (OpenStreetMap)** - Planned for Sprint 3 (optional improvement)
  - Geocoding (lat/lon lookup) para cidades/endereços
  - **Note**: Post-MVP feature, not a blocker for initial geographic-restriction release
- **ViaCep API** - Lookup de CEP brasileiro
  - Base URL: `https://viacep.com.br/ws/`
  - Documentação: <https://viacep.com.br/>
- **BrasilApi CEP** - Lookup de CEP (fallback)
  - Base URL: `https://brasilapi.com.br/api/cep/v1/`
  - Documentação: <https://brasilapi.com.br/docs>
- **OpenCep API** - Lookup de CEP (fallback)
  - Base URL: `https://opencep.com/v1/`
  - Documentação: <https://opencep.com/>
- **Nominatim (OpenStreetMap)** - Geocoding (planejado)
  - Base URL: `https://nominatim.openstreetmap.org/`
  - Documentação: <https://nominatim.org/release-docs/latest/>

---

*📅 Última atualização: 21 de Novembro de 2025*  
*🔄 Roadmap em constante evolução baseado em feedback, métricas e aprendizados*
