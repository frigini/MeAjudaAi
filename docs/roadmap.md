# 🗺️ Roadmap - MeAjudaAi

Este documento consolida o planejamento estratégico e tático da plataforma MeAjudaAi, definindo fases de implementação, módulos prioritários e funcionalidades futuras.

---

## 📊 Sumário Executivo

**Projeto**: MeAjudaAi - Plataforma de Conexão entre Clientes e Prestadores de Serviços  
**Status Geral**: Fase 1 ✅ | Fase 1.5 🔄 (Sprint 0) | MVP Target: 31/Março/2025  
**Test Coverage**: 40.51% → Meta 80%+  
**Stack**: .NET 10 LTS + Aspire 13 + PostgreSQL + Blazor WASM + MAUI Hybrid

### Marcos Principais
- ✅ **Janeiro 2025**: Fase 1 concluída - 6 módulos core implementados
- 🔄 **Jan 20 - Feb 2**: Sprint 0 - Migration .NET 10 + Aspire 13
- ⏳ **Fevereiro 2025**: Sprints 1-2 - Integração + Testes + Hardening
- ⏳ **Fevereiro-Março 2025**: Sprints 3-5 - Frontend Blazor (Web + Mobile)
- 🎯 **31 Março 2025**: MVP Launch (Admin Portal + Customer App)
- 🔮 **Abril 2025+**: Fase 3 - Reviews, Assinaturas, Agendamentos

---

## 🎯 Status Atual

**✅ Fase 1: CONCLUÍDA** (Janeiro 2025)  
Todos os 6 módulos core implementados, testados e integrados:
- Users • Providers • Documents • Search & Discovery • Locations • ServiceCatalogs

**🔄 Fase 1.5: EM ANDAMENTO** (Janeiro-Fevereiro 2025)  
Fundação técnica para escalabilidade e produção:
- Migration .NET 10 + Aspire 13 (Sprint 0)
- Integração de módulos + Restrições geográficas (Sprint 1)
- Test coverage 80%+ + Health checks + Data seeding (Sprint 2)

**⏳ Fase 2: PLANEJADO** (Fevereiro-Março 2025)  
Frontend Blazor WASM + MAUI Hybrid:
- Admin Portal (Sprint 3)
- Customer App (Sprint 4)
- Polishing + Hardening (Sprint 5)

---

## 📖 Visão Geral

O roadmap está organizado em **quatro fases principais** para entrega incremental de valor:

1. **✅ Fase 1: Fundação (MVP Core)** - Registro de prestadores, busca geolocalizada, catálogo de serviços
2. **🔄 Fase 1.5: Fundação Técnica** - Migration .NET 10, integração, testes, hardening
3. **⏳ Fase 2: Frontend & Experiência** - Blazor WASM Admin Portal + Customer App
4. **🔮 Fase 3: Qualidade e Monetização** - Sistema de avaliações, assinaturas premium, verificação automatizada
5. **🔮 Fase 4: Experiência e Engajamento** - Agendamentos, comunicações, analytics avançado

A implementação segue os princípios arquiteturais definidos em `architecture.md`: **Modular Monolith**, **DDD**, **CQRS**, e **isolamento schema-per-module**.

---

## 📅 Cronograma de Sprints (Janeiro-Março 2025)

| Sprint | Duração | Período | Objetivo | Status |
|--------|---------|---------|----------|--------|
| **Sprint 0** | 1-2 semanas | Jan 20 - Feb 2 | Migration .NET 10 + Aspire 13 | 🔄 EM ANDAMENTO |
| **Sprint 1** | 1 semana | Feb 3 - Feb 9 | Integração de Módulos + Restrição Geográfica | ⏳ Planejado |
| **Sprint 2** | 1 semana | Feb 10 - Feb 16 | Test Coverage 80% + Hardening | ⏳ Planejado |
| **Sprint 3** | 2 semanas | Feb 17 - Mar 2 | Blazor Admin Portal (Web) | ⏳ Planejado |
| **Sprint 4** | 3 semanas | Mar 3 - Mar 23 | Blazor Customer App (Web + Mobile) | ⏳ Planejado |
| **Sprint 5** | 1 semana | Mar 24 - Mar 30 | Polishing & Hardening (MVP Final) | ⏳ Planejado |

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
- 40.51% test coverage (296 testes passando)
- APIs públicas (IModuleApi) implementadas para todos módulos
- Integration events funcionais entre módulos
- Health checks configurados
- CI/CD pipeline completo no Azure DevOps
- Documentação arquitetural completa

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

**Status**: Implementado e testado

**Objetivo**: Abstrair funcionalidades de geolocalização e lookup de CEP brasileiro.

**Funcionalidades Entregues**:
- ✅ ValueObjects: Cep, Coordinates, Address com validação completa
- ✅ Integração com APIs de CEP: ViaCEP, BrasilAPI, OpenCEP
- ✅ Fallback chain automático (ViaCEP → BrasilAPI → OpenCEP)
- ✅ Resiliência HTTP via ServiceDefaults (retry, circuit breaker, timeout)
- ✅ API pública (ILocationModuleApi) para comunicação inter-módulos
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
- `GeocodingService`: Stub (TODO: integração com Nominatim ou Google Maps API)

**Próximas Melhorias (Opcional)**:
- 🔄 Implementar GeocodingService com Nominatim (OpenStreetMap) ou Google Maps API
- 🔄 Adicionar caching Redis para reduzir chamadas às APIs externas (TTL: 24h para CEP, 7d para geocoding)
- 🔄 Integração com IBGE para lookup de municípios e estados

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
- [ ] **Verificação Incremental**:
  - [ ] Build Domain projects → sem erros
  - [ ] Build Application projects → sem erros
  - [ ] Build Infrastructure projects → sem erros
  - [ ] Build API projects → sem erros
  - [ ] Run unit tests → todos passando
  - [ ] Run integration tests → todos passando
- [ ] Atualizar Azure DevOps pipeline YAML
- [ ] Validar Docker images com .NET 10
- [ ] Merge para master após validação completa

**Resultado Esperado**:
- ✅ Sistema rodando em .NET 10 LTS com Aspire 13
- ✅ Todos 296 testes passando
- ✅ CI/CD funcional
- ✅ Documentação atualizada

---

### 📅 Sprint 1: Integração de Módulos + Restrição Geográfica (1 semana)

**Status**: ⏳ PLANEJADO

**Pré-Requisitos (decidir no Sprint 0)**:
- ✅ **Contratos de Módulos**: Finalizar interfaces IModuleApi para cada módulo
- ✅ **Cache de Cidades**: Implementar caching da lista AllowedCities para evitar impacto de performance no SearchModule
- ✅ **Background Workers**: Definir arquitetura (threading, retry logic, poison queue handling) para integration events

**Objetivos**:
- Implementar regras de negócio reais usando IModuleApi entre módulos
- Adicionar restrição geográfica (operação apenas em cidades piloto)
- Melhorar validações e business rules cross-module

**Tarefas**:

#### 1. Integração Providers ↔ Documents
- [ ] Providers: Validar `HasVerifiedDocuments` antes de aprovar prestador
- [ ] Providers: Bloquear ativação se `HasRejectedDocuments` ou `HasPendingDocuments`
- [ ] Documents: Publicar `DocumentVerified` event para atualizar status de Providers
- [ ] Integration test: Fluxo completo de verificação de prestador

#### 2. Integração Providers ↔ ServiceCatalogs
- [ ] Providers: Adicionar `ProviderServices` linking table (many-to-many)
- [ ] Providers: Validar services via `IServiceCatalogsModuleApi.ValidateServicesAsync`
- [ ] Providers: Bloquear serviços inativos ou inexistentes
- [ ] Admin Portal: Endpoint para associar serviços a prestadores

#### 3. Integração Search ↔ Providers + ServiceCatalogs
- [ ] Search: Denormalizar `ServiceIds` no `SearchableProvider` read model
- [ ] Search: Background worker consumindo `ProviderVerified`, `ProviderUpdated` events
- [ ] Search: Filtrar busca por `ServiceIds` array (query otimizada)
- [ ] Integration test: Busca retorna apenas prestadores com serviços ativos

#### 4. Integração Providers ↔ Locations
- [ ] Providers: Usar `ILocationModuleApi.GetAddressFromCepAsync` no registro
- [ ] Providers: Validar CEP existe antes de salvar endereço
- [ ] Providers: Auto-populate cidade/estado via Locations
- [ ] Unit test: Mock de ILocationModuleApi em Providers.Application

#### 5. Restrição Geográfica (MVP Blocker)
- [ ] Criar `AllowedCities` configuration em appsettings
- [ ] Providers: Validar cidade permitida no registro (`São Paulo`, `Rio de Janeiro`, `Belo Horizonte`)
- [ ] Search: Filtrar automaticamente por cidades permitidas
- [ ] Admin: Endpoint para gerenciar cidades permitidas
- [ ] Integration test: Rejeitar registro fora de cidades piloto

**Resultado Esperado**:
- ✅ Módulos totalmente integrados com business rules reais
- ✅ Operação restrita a cidades piloto (SP, RJ, BH)
- ✅ Background workers consumindo integration events
- ✅ Validações cross-module funcionando

---

### 📅 Sprint 2: Test Coverage 80% + Hardening (1 semana)

**Status**: ⏳ PLANEJADO

**Objetivos**:
- Aumentar test coverage de 40.51% para 80%+
- Implementar health checks customizados
- Data seeding para ambientes de desenvolvimento/staging
- Melhorias de observability

**Tarefas**:

#### 1. Unit Tests (Coverage Target: 90%+ em Domain/Application)
- [ ] **Users**: Unit tests para domain entities + commands/queries
- [ ] **Providers**: Unit tests para agregados + business rules + validation
- [ ] **Documents**: Unit tests para workflow de verificação
- [ ] **Search**: Unit tests para ranking logic
- [ ] **Locations**: ✅ Já possui 52 tests (manter coverage)
- [ ] **ServiceCatalogs**: Unit tests para categorias + serviços + validações

#### 2. Integration Tests (Coverage Target: 70%+ em Infrastructure/API)
- [ ] **Providers**: Fluxo completo de registro → verificação → aprovação
- [ ] **Documents**: Upload → OCR → Verificação → Rejection
- [ ] **Search**: Busca geolocalizada com filtros + ranking
- [ ] **ServiceCatalogs**: CRUD de categorias e serviços via API

#### 3. E2E Tests (Scenarios críticos)
- [ ] Registro de prestador end-to-end (com documents + services)
- [ ] Busca geolocalizada com filtros (raio, rating, tier, services)
- [ ] Admin: Moderação de prestador (aprovar/rejeitar)
- [ ] Geographic restriction: Bloquear registro fora de cidades piloto

#### 4. Health Checks Customizados
- [ ] Providers: Check se Keycloak está acessível
- [ ] Documents: Check se Azure Blob Storage está acessível
- [ ] Locations: Check se APIs de CEP estão respondendo (ViaCEP, BrasilAPI)
- [ ] Search: Check se índices PostGIS estão íntegros
- [ ] ServiceCatalogs: Check de integridade do catálogo (categorias ativas)

#### 5. Data Seeding (Desenvolvimento/Staging)
- [ ] Seeder de ServiceCatalogs: 10 categorias + 50 serviços comuns
- [ ] Seeder de Providers: 20 prestadores fictícios (variedade de tiers/serviços)
- [ ] Seeder de Users: Admin padrão + 10 customers
- [ ] Seeder de Reviews: 50 avaliações (planejado para Fase 2)
- [ ] Script: `dotnet run --seed-dev-data`

#### 6. Observability Improvements
- [ ] Adicionar métricas customizadas (ex: provider_registrations_total)
- [ ] Logging estruturado com correlation IDs em todos módulos
- [ ] Distributed tracing via OpenTelemetry em cross-module calls

**Resultado Esperado**:
- ✅ Test coverage ≥ 80% (atual: 40.51%)
- ✅ Health checks prontos para monitoramento em produção
- ✅ Ambiente de desenvolvimento populado com dados realistas
- ✅ Observability completa (metrics, logs, traces)

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

#### 6. Moderação de Reviews (Preparação para Fase 3)
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

### 2.1. ⭐ Módulo Reviews & Ratings (Planejado)

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

### 2.2. 💳 Módulo Payments & Billing (Planejado)

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

### 2.3. 🤖 Documents - Verificação Automatizada (Planejado - Fase 2)

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

## 🚀 Fase 3: Experiência e Engajamento (Post-MVP)

### Objetivo
Melhorar experiência do usuário com agendamentos, comunicações centralizadas e analytics avançado.

### 3.1. 📅 Módulo Service Requests & Booking (Planejado)

**Objetivo**: Permitir que clientes solicitem serviços e agendem horários com prestadores.

#### **Funcionalidades**
- **Solicitação de Serviço**: Cliente descreve necessidade e localização
- **Matching**: Sistema sugere prestadores compatíveis
- **Agendamento**: Calendário integrado com disponibilidade de prestador
- **Notificações**: Lembretes automáticos via Communications module

---

### 3.2. 📧 Módulo Communications (Planejado)

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

### 3.3. 📊 Módulo Analytics & Reporting (Planejado)

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
- **.NET 9.0** - Runtime principal
- **PostgreSQL + PostGIS** - Database com suporte geoespacial
- **Keycloak** - Identity & Access Management
- **Stripe** - Payment processing
- **Azure Blob Storage** - Document storage
- **OpenTelemetry + Aspire** - Observability

---

*📅 Última atualização: Novembro 2025*  
*🔄 Roadmap em constante evolução baseado em feedback, métricas e aprendizados*
