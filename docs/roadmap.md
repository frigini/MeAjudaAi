# 🗺️ Roadmap - MeAjudaAi

Este documento consolida o planejamento estratégico e tático da plataforma MeAjudaAi, definindo fases de implementação, módulos prioritários e funcionalidades futuras.

---

## 📖 Visão Geral

O roadmap está organizado em **três fases principais** para entrega incremental de valor:

1. **Fase 1: Fundação (MVP Core)** - Registro de prestadores, busca geolocalizada, catálogo de serviços
2. **Fase 2: Qualidade e Monetização** - Sistema de avaliações, assinaturas premium, verificação automatizada
3. **Fase 3: Experiência e Engajamento** - Agendamentos, comunicações, analytics avançado

A implementação seguirá os princípios arquiteturais definidos em `architecture.md`: **Modular Monolith**, **DDD**, **CQRS**, e **isolamento schema-per-module**.

---

## 🎯 Fase 1: Fundação (MVP Core)

### Objetivo
Estabelecer as capacidades essenciais da plataforma: registro multi-etapas de prestadores com verificação, busca geolocalizada e catálogo de serviços.

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

### 1.6. ✅ Módulo service_catalogs (Concluído)

**Status**: Implementado e funcional com testes completos

**Objetivo**: Gerenciar tipos de serviços que prestadores podem oferecer através de um catálogo admin-managed.

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
- **Module API**: `service_catalogsModuleApi` para comunicação inter-módulos

**3. Infrastructure Layer** ✅
- `service_catalogsDbContext` com schema isolation (`service_catalogs`)
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
- `Iservice_catalogsModuleApi` - Interface pública
- DTOs: ModuleServiceCategoryDto, ModuleServiceDto, ModuleServiceListDto, ModuleServiceValidationResultDto

#### **API Pública Implementada**
```csharp
public interface Iservice_catalogsModuleApi : IModuleApi
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

## 🎯 Fase 2: Qualidade e Monetização

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

## 🎨 Frontend Applications

### 📱 Stack Tecnológico
- **Framework**: React + TypeScript (Vite)
- **UI Library**: Material-UI (MUI)
- **State Management**: Zustand
- **API Client**: Axios com wrapper customizado
- **Auth**: oidc-client-ts (integração com Keycloak)

### 🗂️ Estrutura de Projetos
```text
web/
├── admin-portal/       # Portal administrativo
├── customer-app/       # App para clientes (futuro)
├── provider-app/       # App para prestadores (futuro)
└── shared/            # Componentes compartilhados
```

### 🔐 Autenticação
- **Protocolo**: OpenID Connect (OIDC)
- **Identity Provider**: Keycloak
- **Token Management**: Automático via oidc-client-ts
- **Storage**: Secure token storage (sessionStorage/localStorage com encryption)

### 🎯 Prioridades de Implementação
1. **Admin Portal** (Fase 1)
   - Gestão de usuários e prestadores
   - Moderação de reviews
   - Dashboard de métricas básico
   - Gestão de catálogo de serviços

2. **Customer Profile** (Fase 1)
   - Edição de perfil
   - Histórico de solicitações
   - Gestão de reviews escritos

3. **Provider Portal** (Fase 2)
   - Dashboard analítico
   - Gestão de documentos
   - Calendário de disponibilidade
   - Gestão de assinatura

---

## 🎯 Funcionalidades Prioritárias Recomendadas

### 🛡️ Admin Portal (Alta Prioridade)
**Por quê**: Operações da plataforma são impossíveis sem back-office.

**Funcionalidades Core**:
- **User & Provider Management**: Visualizar, suspender, verificar manualmente
- **service_catalogs Management**: Aprovar/rejeitar serviços sugeridos
- **Review Moderation**: Lidar com reviews sinalizados
- **Dashboard**: Métricas-chave do módulo Analytics

**Implementação**: App Blazor ou React separado consumindo mesma API (endpoints admin-only)

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
6. 📋 Módulo service_catalogs - Catálogo admin-managed de categorias e serviços
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
