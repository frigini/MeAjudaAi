## 🎨 Fase 2: Frontend & Experiência

**Status**: 🔄 Em andamento (Jan–Mar 2026)

### Objetivo
Desenvolver aplicações frontend usando **React + Next.js** (Customer Web App, Admin Portal) + **React Native** (Mobile App).

> **📅 Status Atual**: Sprint 9 (Estabilização) em andamento (26 Mar 2026)  
> **📝 Decisão Técnica**: Cobertura Global de 70% atingida e reforçada no CI/CD.
> **🎉 MIGRAÇÃO CONCLUÍDA**: Admin Portal migrado de Blazor para React + Next.js na Sprint 8D

---

### 📱 Stack Tecnológico ATUALIZADO (21 Mar 2026)

> **📝 Decisão Técnica** (5 Fevereiro 2026):  
> Stack de Customer App definida como **React 19 + Next.js 15 + Tailwind CSS v4**.  
> **Admin Portal**: Migrado de Blazor WASM para React + Next.js na Sprint 8D.
> **Razão**: SEO crítico para Customer App, performance inicial, ecosystem maduro, hiring facilitado.

**Decisão Estratégica**: Stack unificado em **React + Next.js** para todos os apps web

**Justificativa**:
- ✅ **SEO**: Customer App precisa aparecer no Google ("eletricista RJ") - Next.js SSR/SSG resolve
- ✅ **Performance**: Initial load rápido crítico para conversão mobile - code splitting + lazy loading
- ✅ **Ecosystem**: Massivo - geolocation, maps, payments, qualquer problema já resolvido
- ✅ **Hiring**: Fácil escalar time - React devs abundantes
- ✅ **Mobile**: React Native maduro e testado vs MAUI Hybrid ainda novo
- ✅ **Modern Stack**: React 19 + Tailwind v4 é estado da arte (2026)
- ⚠️∩╕Å **Trade-off**: DTOs duplicados (C# backend, TS frontend) - mitigado com OpenAPI TypeScript Generator

**Stack Completa**:

**Admin Portal** (React - migrado na Sprint 8D):
- React 19 + TypeScript 5.7+
- Tailwind CSS v4
- Zustand (state management)
- React Hook Form + Zod
- NextAuth.js (Keycloak OIDC)

**Customer Web App** (novo):
- React 19 (Server Components + Client Components)
- Next.js 15 (App Router, SSR/SSG)
- TypeScript 5.7+ (strict mode)
- Tailwind CSS v4 (@theme, CSS variables)
- Base UI React (@base-ui/react) - headless components
- Zustand (client state) + TanStack Query v5 (server state)
- React Hook Form + Zod (forms & validation)
- Lucide React (icons)

**Mobile Customer App** (novo):
- React Native + Expo
- Compartilha componentes com Customer Web App
- Geolocalização nativa
- Notificações push
- Secure Storage para tokens


**Shared**:
- **OpenAPI TypeScript Generator**: Sincroniza tipos C# → TypeScript automaticamente
  - **Tooling**: `openapi-typescript-codegen` ou `@hey-api/openapi-ts`
  - **Trigger**: CI/CD job on `api/swagger/v1/swagger.json` changes
  - **Output**: `MeAjudaAi.Web.Customer/types/api/generated/`
  - **Versioning**: API versions `v1`, `v2` (breaking changes require version bump)
  - **Breaking Change Gating**: OpenAPI diff in CI fails PR without version bump
- Keycloak OIDC (autenticação unificada)
- PostgreSQL (backend único)

**Code Sharing Strategy (C# Γåö TypeScript)**:

| Artifact | Backend Source | Frontend Output | Sync Method |
|----------|----------------|-----------------|-------------|
| **DTOs** | `Contracts/*.cs` | `types/api/*.ts` | OpenAPI Generator (auto) |
| **Enums** | `Shared.Contracts/Enums/` | `types/enums.ts` | OpenAPI Generator (auto) |
| **Validation** | FluentValidation | Zod schemas | Automated Generation (Sprint 8A) |
| **Constants** | `Shared.Contracts/Constants/` | `lib/constants.ts` | Automated Generation (Sprint 8A) |

**Generation Plan**:
1. Implementar ferramenta CLI para converter `Shared.Contracts` Enums e Constants em `types/enums.ts` e `lib/constants.ts`.
2. Implementar conversor de metadados FluentValidation para Zod schemas em `types/api/validation.ts`.
3. Adicionar tickets no backlog para verificação em CI e versionamento sem├óntico dos artefatos gerados.

**Strategy Note**: We prioritize reusing `MeAjudaAi.Shared.Contracts` for enums and constants to keep the Frontend aligned with the Backend and avoid drift.

**Generated Files Location**:
```text
src/
├── Contracts/                       # Backend DTOs (C#)
└── Web/
    ├── MeAjudaAi.Web.Admin/         # React + Next.js (migrated from Blazor in Sprint 8D)
    ├── MeAjudaAi.Web.Customer/      # Next.js
    │   └── types/api/generated/     # ← OpenAPI generated types
    └── Mobile/
        └── MeAjudaAi.Mobile.Customer/   # React Native
            └── src/types/api/             # ← Same OpenAPI generated types
```

**CI/CD Pipeline** (GitHub Actions):
1. Backend changes → Swagger JSON updated
2. OpenAPI diff check (breaking changes?)
3. If breaking → Require API version bump (`v1` → `v2`)
4. Generate TypeScript types
5. Commit to `types/api/generated/` (auto-commit bot)
6. Frontend tests run with new types

### 🗂️ Estrutura de Projetos Atualizada
```text
src/
├── Web/
│   ├── MeAjudaAi.Web.Admin/          # React + Next.js Admin Portal (Sprint 8D)
│   └── MeAjudaAi.Web.Customer/       # 🚀 Next.js Customer App (Sprint 8A)
├── Mobile/
│   └── MeAjudaAi.Mobile.Customer/    # 🚀 React Native + Expo (Sprint 8B)
└── Shared/
    ├── MeAjudaAi.Shared.DTOs/        # DTOs C# (backend)
    └── MeAjudaAi.Shared.Contracts/   # OpenAPI spec → TypeScript types
```

### 🔐 Autenticação Unificada

**Cross-Platform Authentication Consistency**:

| Aspect | Admin (React) | Customer Web (Next.js) | Customer Mobile (RN) |
|--------|--------------|------------------------|----------------------|
| **Token Storage** | HTTP-only cookies | HTTP-only cookies | Secure Storage |
| **Token Lifetime** | 1h access + 24h refresh | 1h access + 7d refresh | 1h access + 30d refresh |
| **Refresh Strategy** | Automatic (NextAuth) | Middleware refresh | Background refresh |
| **Role Claims** | `role` claim | `role` claim | `role` claim |
| **Logout** | `/api/auth/signout` | `/api/auth/signout` | Revoke + clear storage |

**Keycloak Configuration**:
- **Realm**: `MeAjudaAi`
- **Clients**: `meajudaai-admin` (public), `meajudaai-customer` (public)
- **Roles**: `admin`, `customer`, `provider`
- **Token Format**: JWT (RS256)
- **Token Lifetime**: Access 1h, Refresh 30d (configurable per client: Admin=24h, Customer=7d, Mobile=30d)

**Implementation Details**:
- **Protocolo**: OpenID Connect (OIDC)
- **Identity Provider**: Keycloak
- **Admin Portal**: NextAuth.js v5 (React + Next.js)
- **Customer Web**: NextAuth.js v5 (Next.js)
- **Customer Mobile**: React Native OIDC Client
- **Refresh**: Automático via OIDC interceptor

**Migration Guide**: See `docs/authentication-migration.md` (to be created Sprint 8A)



---

### 🚀 Gestão de Restrições Geográficas

**Resumo**: Restrições geográficas podem ser configuradas via `appsettings.json` (Fase 1, MVP atual) ou gerenciadas dinamicamente via Blazor Admin Portal com banco de dados (Fase 2, planejado Sprint 7+). O middleware `GeographicRestrictionMiddleware` valida cidades/estados permitidos usando IBGE API.

**Contexto**: O middleware `GeographicRestrictionMiddleware` suporta configuração dinâmica via `Microsoft.FeatureManagement`. Este recurso foi implementado em duas fases:

#### ✅ Fase 1: Middleware com appsettings (CONCLUÍDA - Sprint 1 Dia 1, 21 Nov 2025)

**Implementação Atual**: Restrições geográficas baseadas em `appsettings.json` com middleware HTTP e integração IBGE API.

**Decisões de Arquitetura**:

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
     ```ini
     FeatureManagement:GeographicRestriction = true  → Liga TODA validação
         ↓
     allowed_regions.is_active = true              → Ativa cidade ESPECÍFICA
     ```
   - **MVP (Sprint 1)**: Feature toggle + appsettings (hardcoded cities)
   - **Sprint 3**: Migration para database-backed + Admin Portal UI

3. **Remoção de Redund├óncia** ✅ **J├ü REMOVIDO**
   - ❌ **REMOVIDO**: Propriedade `GeographicRestrictionOptions.Enabled` (redundante com feature flag)
   - ❌ **REMOVIDO**: Verificação `|| !_options.Enabled` do middleware
   - ✅ **ÚNICA FONTE DE VERDADE**: `FeatureManagement:GeographicRestriction` (feature toggle)
   - **Justificativa**: Ter duas formas de habilitar/desabilitar causa confusão e potenciais conflitos.
   - **Benefício**: Menos configurações duplicadas, arquitetura mais clara e segura.

**Organização de Pastas** (21 Nov 2025):
```text
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

**Resultado Sprint 1**: Middleware funcional com validação via IBGE API, feature toggle integrado, e lista de cidades configurável via appsettings (requer redeploy para alterações).

---

### 🚀 Fase 2 Shipped (Jan-Feb 2026)

#### ✅ Fase 2: Database-Backed + Admin Portal UI (CONCLUÍDO - Sprint 7, 7 Jan 2026)

**Contexto**: Migrar lista de cidades/estados de `appsettings.json` para banco de dados, permitindo gestão dinâmica via Admin Portal (React) sem necessidade de redeploy.

**Status**: ✅ IMPLEMENTADO - AllowedCities UI completa com CRUD, coordenadas geográficas, e raio de serviço.

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

### 🚀 Fase 2 Follow-ups (Mar 2026+)

**Funcionalidades Admin Portal**:

- [ ] **Visualização de Restrições Atuais** (Admin Portal React)
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
5. **Sprint 3 Semana 2**: Implementar UI no Admin Portal (React)
6. **Sprint 3 Pós-Deploy**: Popular banco com dados iniciais (Muriaé, Itaperuna, Linhares)
7. **Sprint 4**: Remover valores de appsettings.json (obsoleto)

**Testes Necessários**:
- [ ] Unit tests: `AllowedRegionsService` (CRUD + cache invalidation)
- [ ] Integration tests: Middleware com banco populado vs vazio
- [ ] E2E tests: Admin adiciona cidade → Middleware bloqueia outras cidades

**Documentação**:
- [ ] Admin User Guide: Como adicionar/remover cidades piloto
- [ ] Technical Debt: Marcar `AllowedCities` e `AllowedStates` como obsoletos

**⚠️∩╕Å Breaking Changes**:
- ~~`GeographicRestrictionOptions.Enabled` será removido~~ ✅ **J├ü REMOVIDO** (Sprint 1 Dia 1)
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

**Tecnologias (Admin Portal React)**:
- **Framework**: React 19 + TypeScript 5.7+
- **UI**: Tailwind CSS v4 + Base UI
- **State**: Zustand
- **HTTP**: TanStack Query + React Hook Form
- **Charts**: Recharts

**Resultado Esperado**:
- ✅ Admin Portal funcional e responsivo (React)
- ✅ Todas operações CRUD implementadas
- ✅ Dashboard com métricas em tempo real
- ✅ Deploy em Azure Container Apps

---

### 📅 Sprint 8A: Customer App & Nx Setup (2 semanas) ⏳ ATUALIZADO

**Status**: CONCLU├ìDA (5-13 Fev 2026)
**Dependências**: Sprint 7.16 concluído ✅  
**Duração**: 2 semanas

**Contexto**: Sprint dividida em duas partes para acomodar a migração para Nx monorepo.

---

#### 📱 Parte 1: Customer App Development (Focus)

**Home & Busca** (Semana 1):
- [ ] **Landing Page**: Hero section + busca rápida
- [ ] **Busca Geolocalizada**: Campo de endereço/CEP + raio + serviços
- [ ] **Mapa Interativo**: Exibir prestadores no mapa (Leaflet.Blazor)
- [ ] **Listagem de Resultados**: Cards com foto, nome, rating, distância, tier badge
- [ ] **Filtros**: Rating mínimo, tier, disponibilidade
- [ ] **Ordenação**: Distância, Rating, Tier

**Perfil de Prestador** (Semana 1-2):
- [ ] **Visualização**: Foto, nome, descrição, serviços, rating, reviews
- [ ] **Contato**: Botão WhatsApp, telefone, email (MVP: links externos)
- [ ] **Galeria**: Fotos do trabalho (se disponível)
- [ ] **Reviews**: Listar avaliações de outros clientes (read-only, write em Fase 3)
- [ ] **Meu Perfil**: Editar informações básicas

#### 🛠️ Parte 2: Nx Monorepo Setup
**Status**: 🔄 EM PROGRESSO (Março 2026)  
*Nota: Este é um contêiner ampliado que representa múltiplas sprints destinadas à reestruturação modular do front-end web. A "Sprint 8B.2" encapsula a fundação inicial concluída como parte intrínseca deste arco arquitetural.*

### ✅ Sprint 8B.2 - NX Scaffolding & Initial Migration (5 - 18 Mar 2026)
**Branch**: `feature/sprint-8b2-monorepo-cleanup`
**Status**: 🔄 EM REVISÃO
*Nota: A atualização final para "✅ CONCLUÍDA" deve ocorrer somente após o merge do PR ou confirmação explícita de finalização do trabalho na branch.*

**Objectives**:
1. 🔴 **MUST-HAVE**: **NX Monorepo Setup** (Effort: Large)
    - Initialize workspace.
    - **Migrate** existing `MeAjudaAi.Web.Customer` to `apps/customer-web`.
    - **Scaffolding** (empty placeholders): `apps/provider-web` and `apps/admin-portal`.
    - Extract shared libraries: `libs/ui`, `libs/auth`, `libs/api-client`.
2. 🔴 **MUST-HAVE**: **Messaging Unification** (Effort: Medium)
    - Remove Azure Service Bus, unify on RabbitMQ only.
3. 🔴 **MUST-HAVE**: **Technical Excellence Pack** (Effort: Medium)
    - [ ] [**TD**] **Keycloak Automation**: `setup-keycloak-clients.ps1` for local dev.
    - [ ] [**TD**] **Analyzer Cleanup**: Fix SonarLint warnings in React apps & Contracts.
    - [ ] [**TD**] **Refactor Extensions**: Extract `BusinessMetricsMiddlewareExtensions`.
    - [ ] [**TD**] **Polly Logging**: Migrate resilience logging to ILogger (Issue #113).
    - [ ] [**TD**] **Standardization**: Record syntax alignment in `Contracts`.
    *(TODO: Marcar os checkboxes acima como [x] após o merge do PR na branch feature/sprint-8b2-monorepo-cleanup)*

---

**Entregáveis**:
- [ ] Nx workspace com `apps/customer-web` (migrado).
- [ ] Placeholders para `apps/provider-web` e `apps/admin-portal`.
- [ ] Bibliotecas extraídas para `libs/ui`, `libs/auth` e `libs/api-client`.
*(TODO: Marcar os entregáveis como [x] após o merge do PR 153)*

---

## 🔥 Tarefas Técnicas Cross-Module ⏳ ATUALIZADO

**Status**: ✅ CONCLUÍDO (Sprint 5.5 - 19 Dez 2025)

**Contexto Atual**:
- ✅ Lock files regenerados em todos os módulos (37 arquivos atualizados)
- ✅ PR #81 (Aspire 13.1.0) atualizado com lock files corretos
- ✅ PR #82 (FeatureManagement 4.4.0) atualizado com lock files corretos
- ⏳ Aguardando validação CI/CD antes do merge
- 📋 Desenvolvimento frontend aguardando conclusão desta sprint

Tarefas técnicas que devem ser aplicadas em todos os módulos para consistência e melhores práticas.

### Migration Control em Produção

**Issue**: Implementar controle `APPLY_MIGRATIONS` nos módulos restantes

**Contexto**: O módulo Documents já implementa controle via variável de ambiente `APPLY_MIGRATIONS` para desabilitar migrations automáticas em produção.

**Implementação** (padrão estabelecido em `Documents/API/Extensions.cs`):

```csharp
private static void EnsureDatabaseMigrations(WebApplication app)
{
    // Read the environment variable (or from IConfiguration)
    var applyMigrations = app.Configuration["APPLY_MIGRATIONS"] 
                          ?? Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");

    if (!string.IsNullOrEmpty(applyMigrations) && 
        bool.TryParse(applyMigrations, out var shouldApply) && !shouldApply)
    {
        app.Logger.LogInformation("Automatic migrations disabled via APPLY_MIGRATIONS=false");
        return;
    }

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    // Aplicar migrations normalmente
    context.Database.Migrate();
}
```

**Status por Módulo**:
- ✅ **Documents**: Implementado (Sprint 4 - 16 Dez 2025)
- ⏳ **Users**: Pendente
- ⏳ **Providers**: Pendente  
- ⏳ **ServiceCatalogs**: Pendente
- ⏳ **Locations**: Pendente
- ⏳ **SearchProviders**: Pendente

**Esforço Estimado**: 15 minutos por módulo (copiar padrão do Documents)

**Documentação**: Padrão documentado em `docs/database.md` seção "Controle de Migrations em Produção"

**Prioridade**: MÉDIA - Implementar antes do primeiro deployment em produção

---

## 📋 Sprint 5.5: Package Lock Files & Dependency Updates (19 Dez 2025)

**Status**: 🔄 EM ANDAMENTO - Aguardando CI/CD  
**Duração**: 1 dia  
**Objetivo**: Resolver conflitos de package lock files e atualizar dependências

### Contexto

Durante o processo de atualização automática de dependências pelo Dependabot, foram identificados conflitos nos arquivos `packages.lock.json` causados por incompatibilidade de versões do pacote `Microsoft.OpenApi`.

**Problema Raiz**:
- Lock files esperavam versão `[2.3.12, )` 
- Central Package Management especificava `[2.3.0, )`
- Isso causava erros NU1004 em todos os projetos, impedindo build e testes

### Ações Executadas

#### ✅ Correções Implementadas

1. **Branch feature/refactor-and-cleanup**
   - ✅ 37 arquivos `packages.lock.json` regenerados
   - ✅ Commit: "chore: regenerate package lock files to fix version conflicts"
   - ✅ Push para origin concluído

2. **Branch master**
   - ✅ Merge de feature/refactor-and-cleanup → master
   - ✅ Push para origin/master concluído
   - ✅ Todos os lock files atualizados na branch principal

3. **PR #81 - Aspire 13.1.0 Update**
   - Branch: `dependabot/nuget/aspire-f7089cdef2`
   - ✅ Lock files regenerados (37 arquivos)
   - ✅ Commit: "fix: regenerate package lock files after Aspire 13.1.0 update"
   - ✅ Force push concluído
   - ⏳ Aguardando CI/CD (Code Quality Checks, Security Scan)

4. **PR #82 - FeatureManagement 4.4.0 Update**
   - Branch: `dependabot/nuget/Microsoft.FeatureManagement.AspNetCore-4.4.0`
   - ✅ Lock files regenerados (36 arquivos)
   - ✅ Commit: "fix: regenerate package lock files after FeatureManagement update"
   - ✅ Push concluído
   - ⏳ Aguardando CI/CD (Code Quality Checks, Security Scan)

### Próximos Passos

1. ✅ **Merge PRs #81 e #82** - Concluído (19 Dez 2025)
2. ✅ **Atualizar feature branch** - Merge master → feature/refactor-and-cleanup
3. ✅ **Criar PR #83** - Branch feature/refactor-and-cleanup → master
4. ⏳ **Aguardar review e merge PR #83**
5. 📋 **Iniciar Sprint 6** - GitHub Pages Documentation (Q1 2026)
6. 📋 **Planejar Sprint 7** - Blazor Admin Portal (Q1 2026)

#### ✅ Atualizações de Documentação (19 Dez 2025)

**Roadmap**:
- ✅ Atualizada seção Sprint 5.5 com todas as ações executadas
- ✅ Atualizado status de Fase 2 para "Em Planejamento - Q1 2026"
- ✅ Atualizados Sprints 3-5 com dependências e novas timelines
- ✅ Atualizada última modificação para 19 de Dezembro de 2025

**Limpeza de Templates**:
- ✅ Removido `.github/pull-request-template-coverage.md` (template específico de outro PR)
- ✅ Removida pasta `.github/issue-template/` (issues obsoletas: EFCore.NamingConventions, Npgsql já resolvidas)
- ✅ Criado `.github/pull_request_template.md` (template genérico para futuros PRs)
- ✅ Commit: "chore: remove obsolete templates and create proper PR template"

**Pull Request #83**:
- ✅ PR criado: feature/refactor-and-cleanup → master
- ✅ Título: "feat: refactoring and cleanup sprint 5.5"
- ✅ Descrição atualizada refletindo escopo real (documentação + merge PRs #81/#82 + limpeza templates)
- ⏳ Aguardando review e CI/CD validation

### Lições Aprendidas

- **Dependabot**: Regenerar lock files manualmente após updates de versões com conflicts
- **CI/CD**: Validação rigorosa de package locks previne deployments quebrados
- **Central Package Management**: Manter sincronização entre lock files e Directory.Packages.props
- **Template Management**: Manter apenas templates genéricos e reutilizáveis em `.github/`
- **Documentation-First**: Documentar ações executadas imediatamente no roadmap para rastreabilidade

---

### ✅ Sprint 8C - Provider Web App (React + NX) (19 Mar - 21 Mar 2026)
- ✅ **Nx Integration**: `MeAjudaAi.Web.Provider` integrado ao workspace Nx
- ✅ **Onboarding Integration**: 
  - `/onboarding/basic-info` conectado à API (`apiMeGet`/`apiMePut`)
  - `/onboarding/documents` conectado à API (upload via SAS URL para Azure Blob Storage)
- ✅ **Dashboard Real Data**: Página principal (`/`) substituída por dados reais via `apiMeGet`
- ✅ **Provider Public Profile**: Nova rota `/provider/[slug]` para perfis públicos com slugs SEO-friendly
- ✅ **Provider Profile Management**:
  - `/alterar-dados` - Edição completa via `apiMePut`
  - `/configuracoes` - Toggle de visibilidade + delete account com confirmação LGPD
- ✅ **Slug URLs**: Perfis públicos acessíveis via slugs (ex: `/provider/joao-silva-a1b2c3d4`)

### ✅ Sprint 8D - Admin Portal Migration (2 - 24 Mar 2026)

**Status**: ✅ CONCLUÍDA (24 Mar 2026)
**Foco**: Phased migration from Blazor WASM to React.

**Entregáveis**:
- ✅ **Admin Portal React**: Functional `src/Web/MeAjudaAi.Web.Admin/` in React.
- ✅ **Providers CRUD**: Complete provider management.
- ✅ **Document Management**: Document upload and verification.
- ✅ **Service Catalogs**: Service catalog management.
- ✅ **Allowed Cities**: Geographic restrictions management.
- ✅ **Dashboard KPIs**: Admin dashboard with metrics.

### ✅ Sprint 8E - E2E Tests & React Test Infrastructure (23 Mar - 25 Mar 2026)

**Status**: ✅ CONCLUÍDA (25 Mar 2026)
**Foco**: Testes E2E (Playwright) + infraestrutura de testes unitários (Vitest + RTL + MSW) + Governança de Cobertura Global.

**Scope — E2E (Playwright)** ✅:
1. ✅ **Playwright Config**: `playwright.config.ts` com 6 projetos (Chromium, Firefox, WebKit, Mobile, CI)
2. ✅ **Customer E2E** (5 specs): auth, onboarding, performance, profile, search
3. ✅ **Provider E2E** (5 specs): auth, dashboard, onboarding, performance, profile-mgmt
4. ✅ **Admin E2E** (5 specs): auth, configs, dashboard, mobile-responsiveness, providers
5. ✅ **Shared Fixtures**: `src/Web/libs/e2e-support/base.ts` (loginAsAdmin, loginAsProvider, loginAsCustomer, logout)
6. ✅ **CI Integration**: `master-ci-cd.yml` atualizado para gerar especificação OpenAPI e rodar E2E.

**Scope — Testes Unitários (Vitest + RTL)** ✅:
7. ✅ **Infraestrutura**: `libs/test-support/` (test-utils.tsx, customRenderHook), thresholds individuais removidos em favor de Cobertura Global.
8. ✅ **Cobertura Global**: Script `src/Web/scripts/merge-coverage.mjs` consolida relatórios de todos os projetos com threshold de 70%.
9. ✅ **Hardening Admin**: Testes unitários para `Sidebar`, `Button`, `Dashboard`, `Providers` e `Users`. Autenticação centralizada em `auth.ts`.
10. ✅ **Hardening Customer**: `DashboardClient` (DTO compliance), `DocumentUpload` (API assertions) e `SearchFilters` (API category validation).

**Cenários de Teste E2E**:
- [x] Autenticação (login, logout, refresh token)
- [x] Fluxo de onboarding (Customer e Provider)
- [x] CRUD de providers e serviços (Admin)
- [x] Busca e filtros geolocalizados
- [x] Responsividade mobile
- [x] Performance e Core Web Vitals (INP, LCP, CLS)

**Pendências para fechar Sprint**:
- [x] Testes unitários Admin (hooks: providers, categories, dashboard, services, allowed-cities, users; components: sidebar, ui)
- [x] Testes unitários Provider (hooks; components: dashboard cards, profile)
- [x] Configurar MSW handlers para Admin e Provider

### ⏳ Sprint 9 - BUFFER & Risk Mitigation (25 Mar - 11 Mai 2026)

**Status**: ⏳ EM ANDAMENTO
**Duration**: 12 days buffer
- Polishing, Refactoring, and Fixing.
- Move Optional tasks from 8B.2 here if needed.
- Rate limiting and advanced security/monitoring.

**Follow-ups Pendentes**:
- [ ] **OpenAPI Diff Gating**: Adicionar verificação de breaking changes em CI (falhar PR se API mudar sem version bump)
- [ ] **Módulo Comunicações**: Implementar infraestrutura base (outbox pattern, templates, event handlers)

## 🎯 MVP Final Launch: 12 - 16 de Maio de 2026 🎯

### ⚠️ Risk Assessment & Mitigation

#### Risk Mitigation Strategy
- **Contingency Branching**: If major tasks (Admin Migration, NX Setup) slip, we prioritize essential Player flows (Customer/Provider) and fallback to existing Admin solutions.
- **Mobile Apps**: De-scoped from MVP to Phase 2 to ensure web platform stability.
- **Buffer**: Sprint 9 is strictly for stability, no new features.
- Documentação final para MVP

### Cenários de Risco Documentados

### Risk Scenario 1: Keycloak Integration Complexity

- **Problema Potencial**: OIDC flows em Blazor WASM com refresh tokens podem exigir configuração complexa
- **Impacto**: +2-3 dias além do planejado no Sprint 6
- **Mitigação Sprint 9**: 
  - Usar Sprint 9 para refinar authentication flows
  - Implementar proper token refresh handling
  - Adicionar fallback mechanisms

### Risk Scenario 3: React Performance Issues

- **Problema Potencial**: App bundle size > 5MB, lazy loading não configurado corretamente
- **Impacto**: UX ruim, +2-3 dias de otimização
- **Mitigação Sprint 9**:
  - Code splitting with dynamic imports
  - Tree shaking and bundle optimization
  - SSR/SSG via Next.js to improve initial load
  - Lazy load React components
  - Optimize images using next/image and responsive formats

### Risk Scenario 4: MAUI Hybrid Platform-Specific Issues (DE-SCOPED FROM MVP)

> **⚠️ IMPORTANTE**: Este cenário de risco foi removido do escopo do MVP. Os Mobile Apps foram adiados para a Fase 2 conforme.nota acima.

- **Problema Potencial**: Diferenças de comportamento iOS vs Android (permissões, geolocation, file access)
- **Impacto**: +4-5 dias de debugging platform-specific
- **Mitigação Sprint 9**:
  - Criar abstractions para platform-specific APIs
  - Implementar fallbacks para features não suportadas
  - Testes em devices reais (não apenas emuladores)

### Risk Scenario 5: API Integration Edge Cases

- **Problema Potencial**: Casos de erro não cobertos (timeouts, network failures, concurrent updates)
- **Impacto**: +2-3 dias de hardening
- **Mitigação Sprint 9**:
  - Implementar retry policies com Polly
  - Adicionar optimistic concurrency handling
  - Melhorar error messages e user feedback

### Tarefas Sprint 9 (Executar conforme necessário)

#### 1. Work-in-Progress Completion
- [ ] Completar funcionalidades parciais de Sprints 6-8
- [ ] Resolver todos os TODOs/FIXMEs adicionados durante implementação
- [ ] Fechar issues abertas durante desenvolvimento frontend

#### 1.1. ≡ƒº¬ SearchProviders E2E Tests (Movido da Sprint 7.16)
**Prioridade**: MÉDIA - Technical Debt da Sprint 7.16  
**Estimativa**: 1-2 dias

**Objetivo**: Testar busca geolocalizada end-to-end.

**Contexto**: Task 5 da Sprint 7.16 foi marcada como OPCIONAL e movida para Sprint 9 para permitir execução com qualidade sem pressão de deadline. Sprint 7.16 completou 4/4 tarefas obrigatórias.

**Entregáveis**:
- [ ] Teste E2E: Buscar providers por serviço + raio (2km, 5km, 10km)
- [ ] Teste E2E: Validar ordenação por distância crescente
- [ ] Teste E2E: Validar restrição geográfica (AllowedCities) - providers fora da cidade não aparecem
- [ ] Teste E2E: Performance (<500ms para 1000 providers em raio de 10km)
- [ ] Teste E2E: Cenário sem resultados (nenhum provider no raio)
- [ ] Teste E2E: Validar paginação de resultados (10, 20, 50 items por página)

**Infraestrutura**:
- Usar `TestcontainersFixture` com PostGIS 16-3.4
- Seed database com providers em localizações conhecidas (lat/lon)
- Usar `HttpClient` para chamar endpoint `/api/search-providers/search`
- Validar JSON response com FluentAssertions

**Critérios de Aceitação**:
- ✅ 6 testes E2E passando com 100% de cobertura dos cenários
- ✅ Performance validada (95th percentile < 500ms)
- ✅ Documentação em `docs/testing/e2e-tests.md`
- ✅ CI/CD executando testes E2E na pipeline

#### 1.2. 🛠️ Technical Excellence & Debt (Sprint 9)
**Prioridade**: MÉDIA  
**Estimativa**: 3-4 dias

**Objetivo**: Resolver pendências técnicas e melhorar resiliência do sistema.

**Entregáveis**:
- [ ] **Event Handlers**: Implementar handlers para comunicação entre SearchProviders e ServiceCatalogs.
- [ ] **Social Login**: Reintegrar login com Instagram via Keycloak OIDC (Issue #141).
- [ ] **Resilience**: Aplicar `CancellationToken` nos Effects de `ServiceCatalogs`, `Documents` e `Locations`.
- [ ] **Localization (Backend)**: Migrar strings de erro da API para `.resx` e integrar FluentValidation para suporte a multi-idioma via headers.
- [ ] **Localization (Frontend React)**: Implementar infraestrutura com `i18next` (JSON) no Admin e Customer apps, localizando mensagens de validação do **Zod**.
- [ ] **Architectural Tests**: Implementar testes com `NetArchTest` no Módulo de Comunicações para garantir isolamento (evitar referências circulares e acesso direto a DBs externos).
- [ ] **UI Branding**: Aplicar cores oficiais (Blue, Cream, Orange) e padronizar o tema em todo o Admin Portal (React).
- [ ] **Unit Tests (Debt)**: Testes para `LocalizationSubscription` disposal e `PerformanceHelper` LRU eviction.

#### 2. UX/UI Improvements
- [ ] **Loading States**: Skeletons em todas cargas assíncronas
- [ ] **Error Handling**: Mensagens friendly para todos erros (não mostrar stack traces)
#### 3. Security & Performance Hardening
- [ ] **API Rate Limiting**: Aspire middleware (100 req/min por IP, 1000 req/min para authenticated users)
- [ ] **CORS**: Configurar origens permitidas (apenas domínios de produção)
- [ ] **CSRF Protection**: Tokens anti-forgery em forms
- [ ] **Security Headers**: HSTS, X-Frame-Options, CSP
- [ ] **Bundle Optimization**: Lazy loading, AOT compilation, tree shaking
- [ ] **Cache Strategy**: Implementar cache HTTP para assets estáticos

#### 4. Logging & Monitoring
- [ ] **Frontend Logging**: Integração com Application Insights (Blazor WASM)
- [ ] **Error Tracking**: Sentry ou similar para erros em produção
- [ ] **Analytics**: Google Analytics ou Plausible para usage tracking
- [ ] **Performance Monitoring**: Web Vitals tracking (LCP, FID, CLS)

#### 5. Documentação Final MVP
- [ ] **API Documentation**: Swagger/OpenAPI atualizado com exemplos
- [ ] **User Guide**: Guia de uso para Admin Portal e Customer App
- [ ] **Developer Guide**: Como rodar localmente, como contribuir
- [ ] **Deployment Guide**: Deploy em Azure Container Apps (ARM templates ou Bicep)
- [ ] **Lessons Learned**: Documentar decisões de arquitetura e trade-offs

#### 6. Módulo de Comunicações (NOVO - Sprint 9)

**Prioridade**: MÉDIA - Infraestrutura base para funcionalidades pós-MVP  
**Objetivo**: Criar módulo unificado de comunicações (email, SMS, push)  
**Contexto**: Outros módulos (Reviews, Payments, Bookings) dependem de infraestrutura de comunicações. Implementar agora evita refatoração depois.

**Estratégia de Testes (Mandatória)**:

| Tipo | Escopo | Ferramentas |
|------|--------|------------|
| **Unitários** | Lógica de templates, cálculo de retries, mapeamento de DTOs | xUnit + FluentAssertions |
| **Integrados** | Persistência Outbox (PostgreSQL), Handlers de eventos | xUnit + Respawn + Docker |
| **E2E** | Fluxo completo: Evento → Outbox → Envio simulado → Log | Playwright + MSW |
| **Arquiteturais** | Validar que módulos não acessam DB de outros, dependências de contratos | NetArchTest |

---

**Arquitetura Decisão** (diferente dos outros módulos):

| Aspecto | Decisão |
|---------|---------|
| **Padrão** | Full Module + Orchestrator Pattern |
| **Infraestrutura** | Outbox Pattern para garantia de entrega (Reliable Messaging) |
| **Integração** | Event-driven (consome IntegrationEvents) |
| **API Externa** | Abstração via interface (provedor configurável) |
| **Idempotência** | Garantida via CorrelationId/EventId nos Logs |

> **📝 Decisão Técnica** (8 Abr 2026): Provedor de email/sms será definido pós-MVP. Implementar com interface de abstração permitindo troca facil. Sprint 9 foca na infraestrutura de transporte/outbox e templates base.

---

**Arquitetura de Projetos**:
```text
src/
├── Contracts/
│   └── MeAjudaAi.Contracts.Communications/
│       ├── ICommunicationsModuleApi.cs
│       ├── DTOs/
│       │   ├── EmailMessageDto.cs (suporte a Attachments)
│       │   ├── SmsMessageDto.cs
│       │   ├── PushMessageDto.cs
│       │   └── TemplateDto.cs
│       └── Channels/
│           ├── IEmailChannel.cs
│           ├── ISmsChannel.cs
│           └── IPushChannel.cs
│
├── Modules/
│   └── Communications/
│       ├── API/
│       │   ├── MeAjudaAi.Modules.Communications.API.csproj
│       │   └── Endpoints/
│       │       └── CommunicationsModuleEndpoints.cs
│       ├── Application/
│       │   ├── MeAjudaAi.Modules.Communications.Application.csproj
│       │   ├── ModuleApi/
│       │   │   └── CommunicationsModuleApi.cs
│       │   └── Services/
│       │       └── Email/
│       │           └── SendGridEmailService.cs  # Stub inicial
│       ├── Domain/
│       │   ├── MeAjudaAi.Modules.Communications.Domain.csproj
│       │   └── Entities/
│       │       ├── OutboxMessage.cs (ScheduledAt support)
│       │       ├── EmailTemplate.cs (IsSystemTemplate flag)
│       │       └── CommunicationLog.cs (CorrelationId support)
│       └── Infrastructure/
│           ├── MeAjudaAi.Modules.Communications.Infrastructure.csproj
│           └── Persistence/
│               ├── Configurations/
│               └── Migrations/
│
└── Shared/
    └── Communications/
        └── Templates/
            ├── WelcomeEmail.cshtml
            ├── ProviderVerificationApproved.cshtml
            └── ProviderVerificationRejected.cshtml
```

**Localização de Templates**:
- **Source of Truth**: Arquivos `.cshtml` em `Shared/Communications/Templates/` (compilados).
- **Override System**: A entidade `EmailTemplate` no banco permite sobrescrever `Subject` ou trechos do `Body` via Admin Portal sem redeploy.
- **IsSystemTemplate**: Flag para proteger templates críticos de deleção.

---

**Mapeamento de Integração com Eventos**:
| Evento Existing | Ação de Comunicação |
|----------------|-------------------|
| `UserRegisteredIntegrationEvent` | → Enviar email de boas-vindas |
| `ProviderAwaitingVerificationIntegrationEvent` | → Notificar admin |
| `ProviderVerificationStatusUpdatedIntegrationEvent` | → Notificar prestador |
| `DocumentVerifiedIntegrationEvent` | → Notificar prestador |
| `DocumentRejectedIntegrationEvent` | → Notificar prestador |

---

**Interface ICommunicationsModuleApi (Atualizada)**:
```csharp
public enum CommunicationPriority { High, Normal, Low }

public interface ICommunicationsModuleApi : IModuleApi
{
    // Email
    Task<Result<Guid>> SendEmailAsync(
        EmailMessageDto email, 
        CommunicationPriority priority = CommunicationPriority.Normal,
        CancellationToken ct = default);
        
    Task<Result<IReadOnlyList<EmailTemplateDto>>> GetTemplatesAsync(CancellationToken ct = default);
    
    // SMS
    Task<Result<Guid>> SendSmsAsync(SmsMessageDto sms, CancellationToken ct = default);
    
    // Push
    Task<Result<Guid>> SendPushAsync(PushMessageDto push, CancellationToken ct = default);
    
    // Logs (Idempotency check via CorrelationId)
    Task<Result<PagedList<CommunicationLogDto>>> GetLogsAsync(
        CommunicationLogQuery query, 
        CancellationToken ct = default);
}
```

---

**Entidades de Domínio**:

```csharp
// OutboxMessage: Garante entrega e permite agendamento
public class OutboxMessage
{
    public Guid Id { get; }
    public string Type { get; }          // "Email", "Sms", "Push"
    public string Content { get; }       // JSON serializado
    public string Status { get; }        // "Pending", "Sent", "Failed"
    public int RetryCount { get; }
    public DateTime CreatedAt { get; }
    public DateTime? ScheduledAt { get; } // Agendamento futuro
    public DateTime? SentAt { get; }
    public string? ErrorMessage { get; }
}

// EmailTemplate: Templates com sistema de override
public class EmailTemplate
{
    public Guid Id { get; }
    public string Key { get; }            // "welcome", "verification-approved"
    public string Subject { get; }
    public string BodyHtml { get; }
    public string BodyText { get; }
    public bool IsSystemTemplate { get; } // Proteção contra deleção
    public DateTime CreatedAt { get; }
}

// CommunicationLog: Audit trail + Idempotência
public class CommunicationLog
{
    public Guid Id { get; }
    public Guid? CorrelationId { get; }   // Idempotência (EventId original)
    public Guid? UserId { get; }
    public Guid? ProviderId { get; }
    public string Channel { get; }        // "Email", "Sms", "Push"
    public string Type { get; }           // "welcome", "verification-approved"
    public string Status { get; }
    public DateTime SentAt { get; }
    public string? ErrorMessage { get; }
}
```

---

**Infraestrutura - Outbox Pattern**:

Para garantir que comunicações não sejam perdidas em caso de falha:

1. **Processo com Outbox** (garantido):
   ```
   Event Occurred → Save to Outbox Table (Mesma Transação) → Background Worker processa Outbox
   ```

**Melhorias Implementadas**:
- **Retry automático**: Com backoff exponencial via Polly.
- **Priorização**: Mensagens de alta prioridade (ex: Reset Password) furam a fila.
- **Idempotência**: Verificação de `CorrelationId` no Log antes do processamento.

---

**Estimativa de Esforço**:

| Task | Esforço | Dependência |
|------|--------|-----------|
| 1. Criar estrutura de projetos | 2h | - |
| 2. Interfaces ICommunicationsModuleApi | 2h | - |
| 3. Implementar OutboxMessage (Scheduling) | 5h | - |
| 4. Implementar EmailTemplate (Override system) | 3h | - |
| 5. Implementar CommunicationLog (CorrelationId) | 2h | - |
| 6. Implementar ModuleApi + Orchestrator | 6h | Items 1-5 |
| 7. Stub Channel Handlers (Email/Sms/Push) | 5h | - |
| 8. Criar templates básicos (.cshtml) | 3h | - |
| 9. Event handlers (IntegrationEvents) | 4h | Events existentes |
| 10. Configuração DI + Polly Policies | 3h | - |
| **Total** | **~35h (~5 dias)** | - |

---

**Critérios de Aceitação**:
- ✅ Módulo registrado no ModuleApiRegistry
- ✅ Envio garantido via Outbox Pattern
- ✅ Suporte a agendamento de mensagens
- ✅ Sistema de templates híbrido funcional
- ✅ Logs com CorrelationId para evitar duplicidade
- ✅ Integração com 3+ IntegrationEvents
- ✅ Priorização de mensagens funcional

---

**Follow-ups Pendentes**:
- [ ] Definir provedores reais (SendGrid, Twilio, Firebase)
- [ ] Admin UI para gestão dinâmica de templates
- [ ] Suporte a anexos (Attachments) via Blob Storage URLs
- [ ] Dashboard de métricas de entrega e conversão

---

**Resultado Esperado Sprint 9**:
- ✅ MVP production-ready e polished
- ✅ Todos os cenários de risco mitigados ou resolvidos
- ✅ Segurança e performance hardened
- ✅ Documentação completa para usuários e desenvolvedores
- ✅ Monitoring e observabilidade configurados
- ✅ Módulo de Comunicações implementado (infraestrutura base resiliente)
- 🎯 **PRONTO PARA LAUNCH EM 12-16 DE MAIO DE 2026**

> **⚠️∩╕Å CRITICAL**: Se Sprint 9 não for suficiente para completar todos os itens, considerar delay do MVP launch ou reduzir escopo (mover features não-críticas para post-MVP). A qualidade e estabilidade do MVP são mais importantes que a data de lançamento.

---

## 🎯 Fase 3: Qualidade e Monetização

### Objetivo
Introduzir sistema de avaliações para ranking, modelo de assinaturas premium via Stripe, e verificação automatizada de documentos.

### 3.1. Γ¡É Módulo Reviews & Ratings (Planejado)

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

### 3.2. ≡ƒÆ│ Módulo Payments & Billing (Planejado)

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

### 3.3. ≡ƒñû Documents - Verificação Automatizada (Planejado - Fase 2)

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

### 3.4. ≡ƒÅ╖∩╕Å Dynamic Service Tags (Planejado - Fase 3)

**Objetivo**: Exibir tags de serviços baseadas na popularidade real por região.

**Funcionalidades**:
- **Endpoint**: `GET /services/top-region?city=SP` (ou lat/lon)
- **Lógica**: Calcular serviços com maior volume de buscas/contratações na região do usuário.
- **Fallback**: Exibir "Top Globais" se dados regionais insuficientes.
- **Cache**: TTL curto (ex: 1h) para manter relev├óncia sem comprometer performance.

---

## ≡ƒÜÇ Fase 4: Experiência e Engajamento (Post-MVP)

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

### 4.2. 📊 Módulo Analytics & Reporting (Planejado)

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
FROM providers.providers p
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
LEFT JOIN users.users u ON al.actor_id = u.user_id
LEFT JOIN providers.providers p ON al.actor_id = p.provider_id;
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

### ≡ƒ¢í∩╕Å Admin Portal - Módulos Avançados
**Funcionalidades Adicionais (Pós-MVP)**:
- **Recent Activity Dashboard Widget**: Feed de atividades recentes (registros, uploads, verificações, mudanças de status) com atualizações em tempo real via SignalR
- **User & Provider Analytics**: Dashboards avançados com Grafana
- **Fraud Detection**: Sistema de scoring para detectar perfis suspeitos
- **Bulk Operations**: Ações em lote (ex: aprovar múltiplos documentos)
- **Audit Trail**: Histórico completo de todas ações administrativas

#### 📊 Recent Activity Widget (Prioridade: MÉDIA)

**Contexto**: Atualmente o Dashboard exibe apenas gráficos estáticos. Um feed de atividades recentes melhoraria a visibilidade operacional.

**Funcionalidades Core**:
- **Timeline de Eventos**: Feed cronológico de atividades do sistema
- **Tipos de Eventos**:
  - Novos registros de prestadores
  - Uploads de documentos
  - Mudanças de status de verificação
  - Ações administrativas (aprovações/rejeições)
  - Adições/remoções de serviços
- **Filtros**: Por tipo de evento, módulo, data
- **Real-time Updates**: SignalR para atualização automática
- **Paginação**: Carregar mais atividades sob demanda

**Implementação Técnica**:
```csharp
// Domain Events → Integration Events → SignalR Hub
public record ProviderRegisteredEvent(Guid ProviderId, string Name, DateTime Timestamp);
public record DocumentUploadedEvent(Guid DocumentId, string Type, DateTime Timestamp);
public record VerificationStatusChangedEvent(Guid ProviderId, string OldStatus, string NewStatus);

// SignalR Hub
public class ActivityHub : Hub
{
    public async Task BroadcastActivity(ActivityDto activity)
    {
        await Clients.All.SendAsync("ReceiveActivity", activity);
    }
}

// Frontend Component
@inject HubConnection HubConnection

<MudTimeline>
    @foreach (var activity in RecentActivities)
    {
        <MudTimelineItem Color="@GetActivityColor(activity.Type)">
            <MudText>@activity.Description</MudText>
            <MudText Typo="Typo.caption">@activity.Timestamp.ToRelativeTime()</MudText>
        </MudTimelineItem>
    }
</MudTimeline>

@code {
    protected override async Task OnInitializedAsync()
    {
        HubConnection.On<ActivityDto>("ReceiveActivity", activity =>
        {
            RecentActivities.Insert(0, activity);
            StateHasChanged();
        });
        await HubConnection.StartAsync();
    }
}
```

**Estimativa**: 3-5 dias (1 dia backend events, 1 dia SignalR, 2-3 dias frontend)

**Dependências**:
- SignalR configurado no backend
- Event bus consumindo domain events
- ActivityDto contract definido

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

### ⚖️∩╕Å Dispute Resolution System (Média Prioridade)
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
- **Revisão Trimestral**: Ajustes baseados em métricas reais
  - **Percentis monitorados**: P50, P95, P99 (latência de queries)
  - **Frequência**: Análise e ajuste a cada 3 meses
  - **Processo**: Feedback loop → identificar outliers → otimizar queries lentas
- **Monitoramento**: OpenTelemetry + Aspire Dashboard + Application Insights

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

### ✅ **Concluído (Set-Dez 2025)**
1. ✅ Sprint 0: Migration .NET 10 + Aspire 13 (21 Nov 2025 - MERGED to master)
2. ✅ Sprint 1: Geographic Restriction + Module Integration (2 Dez 2025 - MERGED to master)
3. ✅ Sprint 2: Test Coverage 90.56% (10 Dez 2025) - Meta 35% SUPERADA em 55.56pp!
4. ✅ Sprint 5.5: Package Lock Files Fix (19 Dez 2025)
   - Correção conflitos Microsoft.OpenApi (2.3.12 → 2.3.0)
   - 37 arquivos packages.lock.json regenerados
   - PRs #81 e #82 atualizados e aguardando merge
5. ✅ Módulo Users (Concluído)
6. ✅ Módulo Providers (Concluído)
7. ✅ Módulo Documents (Concluído)
8. ✅ Módulo Search & Discovery (Concluído)
9. ✅ Módulo Locations - CEP lookup e geocoding (Concluído)
10. ✅ Módulo ServiceCatalogs - Catálogo admin-managed (Concluído)
11. ✅ CI/CD - GitHub Actions workflows (.NET 10 + Aspire 13)
12. ✅ Feature/refactor-and-cleanup branch - Merged to master (19 Dez 2025)

### 📅 Alta Prioridade (Próximos 3 meses - Q1-Q2 2026)
1. ✅ **Sprint 8B.2: NX Monorepo & Technical Excellence** (Concluída)
2. ✅ **Sprint 8C: Provider Web App (React + NX)** (Concluída - 21 Mar 2026)
3. ✅ **Sprint 8D: Admin Portal Migration** (Concluída - 24 Mar 2026)
4. ✅ **Sprint 8E: E2E Tests React Apps (Playwright)** (Concluída - 25 Mar 2026)
5. ⏳ **Sprint 9: BUFFER & RISK MITIGATION** (Abril/Maio 2026)
6. 🎯 **MVP Final Launch: 12 - 16 de Maio de 2026**
7. 📋 API Collections - Bruno .bru files para todos os módulos

### 🎯 **Alta Prioridade - Pré-MVP**
1. 🎯 Communications - Email notifications
2. 💳 Módulo Payments & Billing (Stripe) - Preparação para monetização

### 🎯 **Média Prioridade (6-12 meses - Fase 2)**
1. 🎉 Módulo Reviews & Ratings
2. 🌍 Documents - Verificação automatizada (OCR + Background checks)
3. 🔄 Search - Indexing worker para integration events (extensão do módulo SearchProviders)
4. 📊 Analytics - Métricas básicas
5. 🏛️ Dispute Resolution System
6. 🔥 Alinhamento de middleware entre UseSharedServices() e UseSharedServicesAsync()

### 🔬 **Testes E2E Frontend (Pós-MVP)**
**Projeto**: `src/Web` (dividido por projeto)
**Estrutura**: Uma pasta para cada projeto frontend
- `src/Web/MeAjudaAi.Web.Customer/e2e/` - Testes E2E para Customer Web App
- `src/Web/MeAjudaAi.Web.Provider/e2e/` - Testes E2E para Provider Web App  
- `src/Web/MeAjudaAi.Web.Admin/e2e/` - Testes E2E para Admin Portal

**Framework**: Playwright
**Cenários a cobrir**:
- [ ] Autenticação (login, logout, refresh token)
- [ ] Fluxo de onboarding (Customer e Provider)
- [ ] CRUD de providers e serviços
- [ ] Busca e filtros
- [ ] Responsividade mobile
- [ ] Performance e Core Web Vitals

