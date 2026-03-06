## ≡ƒÄ¿ Fase 2: Frontend & Experi├¬ncia

**Status**: ≡ƒöä Em andamento (JanΓÇôMar 2026)

### Objetivo
Desenvolver aplica├º├╡es frontend usando **Blazor WebAssembly** (Admin Portal) e **React + Next.js** (Customer Web App) + **React Native** (Mobile App).

> **≡ƒôà Status Atual**: Sprint 7 conclu├¡da (7 Jan 2026), Sprint 7.16 conclu├¡da (21 Jan 2026), Sprint 7.20 conclu├¡da (5 Fev 2026), Sprint 7.21 conclu├¡da (5 Fev 2026)  
> **≡ƒô¥ Decis├úo T├⌐cnica** (5 Fev 2026): Customer App usar├í **React 19 + Next.js 15 + Tailwind v4** (SEO, performance, ecosystem)  
> Pr├│ximo foco: Sprint 8A - Customer Web App (React + Next.js).

---

### ≡ƒô▒ Stack Tecnol├│gico ATUALIZADO (5 Fev 2026)

> **≡ƒô¥ Decis├úo T├⌐cnica** (5 Fevereiro 2026):  
> Stack de Customer App definida como **React 19 + Next.js 15 + Tailwind CSS v4**.  
> **Admin Portal** permanece em **Blazor WASM** (j├í implementado, interno, est├ível).  
> *Migration to React planned for Sprint 8D to unify the stack.*
> **Raz├úo**: SEO cr├¡tico para Customer App, performance inicial, ecosystem maduro, hiring facilitado.

**Decis├úo Estrat├⌐gica**: Dual Stack (Blazor para Admin, React para Customer)

**Justificativa**:
- Γ£à **SEO**: Customer App precisa aparecer no Google ("eletricista RJ") - Next.js SSR/SSG resolve
- Γ£à **Performance**: Initial load r├ípido cr├¡tico para convers├úo mobile - code splitting + lazy loading
- Γ£à **Ecosystem**: Massivo - geolocation, maps, payments, qualquer problema j├í resolvido
- Γ£à **Hiring**: F├ícil escalar time - React devs abundantes vs Blazor devs raros
- Γ£à **Mobile**: React Native maduro e testado vs MAUI Hybrid ainda novo
- Γ£à **Modern Stack**: React 19 + Tailwind v4 ├⌐ estado da arte (2026)
- ΓÜá∩╕Å **Trade-off**: DTOs duplicados (C# backend, TS frontend) - mitigado com OpenAPI TypeScript Generator

**Stack Completa**:

**Admin Portal** (mantido):
- Blazor WebAssembly 10.0 (AOT enabled)
- MudBlazor 8.15.0 (Material Design)
- Fluxor 6.9.0 (Redux state management)
- Refit (API client)

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
- Geolocaliza├º├úo nativa
- Notifica├º├╡es push
- Secure Storage para tokens


**Shared**:
- **OpenAPI TypeScript Generator**: Sincroniza tipos C# ΓåÆ TypeScript automaticamente
  - **Tooling**: `openapi-typescript-codegen` ou `@hey-api/openapi-ts`
  - **Trigger**: CI/CD job on `api/swagger/v1/swagger.json` changes
  - **Output**: `MeAjudaAi.Web.Customer/types/api/generated/`
  - **Versioning**: API versions `v1`, `v2` (breaking changes require version bump)
  - **Breaking Change Gating**: OpenAPI diff in CI fails PR without version bump
- Keycloak OIDC (autentica├º├úo unificada)
- PostgreSQL (backend ├║nico)

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
3. Adicionar tickets no backlog para verifica├º├úo em CI e versionamento sem├óntico dos artefatos gerados.

**Strategy Note**: We prioritize reusing `MeAjudaAi.Shared.Contracts` for enums and constants to keep the Frontend aligned with the Backend and avoid drift.

**Generated Files Location**:
```text
src/
Γö£ΓöÇΓöÇ Contracts/                       # Backend DTOs (C#)
Γö£ΓöÇΓöÇ Web/
Γöé   Γö£ΓöÇΓöÇ MeAjudaAi.Web.Admin/         # Blazor (consumes Contracts via Refit)
Γöé   ΓööΓöÇΓöÇ MeAjudaAi.Web.Customer/      # Next.js
Γöé       ΓööΓöÇΓöÇ types/api/generated/     # ΓåÉ OpenAPI generated types
ΓööΓöÇΓöÇ Mobile/
    ΓööΓöÇΓöÇ MeAjudaAi.Mobile.Customer/   # React Native
        ΓööΓöÇΓöÇ src/types/api/           # ΓåÉ Same OpenAPI generated types
```

**CI/CD Pipeline** (GitHub Actions):
1. Backend changes ΓåÆ Swagger JSON updated
2. OpenAPI diff check (breaking changes?)
3. If breaking ΓåÆ Require API version bump (`v1` ΓåÆ `v2`)
4. Generate TypeScript types
5. Commit to `types/api/generated/` (auto-commit bot)
6. Frontend tests run with new types

### ≡ƒùé∩╕Å Estrutura de Projetos Atualizada
```text
src/
Γö£ΓöÇΓöÇ Web/
Γöé   Γö£ΓöÇΓöÇ MeAjudaAi.Web.Admin/          # Blazor WASM Admin Portal (existente)
Γöé   ΓööΓöÇΓöÇ MeAjudaAi.Web.Customer/       # ≡ƒåò Next.js Customer App (Sprint 8A)
Γö£ΓöÇΓöÇ Mobile/
Γöé   ΓööΓöÇΓöÇ MeAjudaAi.Mobile.Customer/    # ≡ƒåò React Native + Expo (Sprint 8B)
ΓööΓöÇΓöÇ Shared/
    Γö£ΓöÇΓöÇ MeAjudaAi.Shared.DTOs/        # DTOs C# (backend)
    ΓööΓöÇΓöÇ MeAjudaAi.Shared.Contracts/   # OpenAPI spec ΓåÆ TypeScript types
```

### ≡ƒöÉ Autentica├º├úo Unificada

**Cross-Platform Authentication Consistency**:

| Aspect | Admin (Blazor) | Customer Web (Next.js) | Customer Mobile (RN) |
|--------|----------------|------------------------|----------------------|
| **Token Storage** | In-memory | HTTP-only cookies | Secure Storage |
| **Token Lifetime** | 1h access + 24h refresh | 1h access + 7d refresh | 1h access + 30d refresh |
| **Refresh Strategy** | Automatic (OIDC lib) | Middleware refresh | Background refresh |
| **Role Claims** | `role` claim | `role` claim | `role` claim |
| **Logout** | `/bff/logout` | `/api/auth/signout` | Revoke + clear storage |

**Keycloak Configuration**:
- **Realm**: `MeAjudaAi`
- **Clients**: `meajudaai-admin` (public), `meajudaai-customer` (public)
- **Roles**: `admin`, `customer`, `provider`
- **Token Format**: JWT (RS256)
- **Token Lifetime**: Access 1h, Refresh 30d (configurable per client: Admin=24h, Customer=7d, Mobile=30d)

**Implementation Details**:
- **Protocolo**: OpenID Connect (OIDC)
- **Identity Provider**: Keycloak
- **Admin Portal**: `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (Blazor)
- **Customer Web**: NextAuth.js v5 (Next.js)
- **Customer Mobile**: React Native OIDC Client
- **Refresh**: Autom├ítico via OIDC interceptor

**Migration Guide**: See `docs/authentication-migration.md` (to be created Sprint 8A)



---

### ≡ƒåò Gest├úo de Restri├º├╡es Geogr├íficas

**Resumo**: Restri├º├╡es geogr├íficas podem ser configuradas via `appsettings.json` (Fase 1, MVP atual) ou gerenciadas dinamicamente via Blazor Admin Portal com banco de dados (Fase 2, planejado Sprint 7+). O middleware `GeographicRestrictionMiddleware` valida cidades/estados permitidos usando IBGE API.

**Contexto**: O middleware `GeographicRestrictionMiddleware` suporta configura├º├úo din├ómica via `Microsoft.FeatureManagement`. Este recurso foi implementado em duas fases:

#### Γ£à Fase 1: Middleware com appsettings (CONCLU├ìDA - Sprint 1 Dia 1, 21 Nov 2025)

**Implementa├º├úo Atual**: Restri├º├╡es geogr├íficas baseadas em `appsettings.json` com middleware HTTP e integra├º├úo IBGE API.

**Decis├╡es de Arquitetura**:

1. **Localiza├º├úo de C├│digo** Γ£à **ATUALIZADO 21 Nov 2025**
   - Γ£à **MOVIDO** `GeographicRestrictionMiddleware` para `ApiService/Middlewares` (espec├¡fico para API HTTP)
   - Γ£à **MOVIDO** `GeographicRestrictionOptions` para `ApiService/Options` (configura├º├úo lida de appsettings da API)
   - Γ£à **MOVIDO** `FeatureFlags.cs` para `Shared/Constants` (constantes globais como AuthConstants, ValidationConstants)
   - Γ¥î **DELETADO** `Shared/Configuration/` (pasta vazia ap├│s movimenta├º├╡es)
   - Γ¥î **DELETADO** `Shared/Middleware/` (pasta vazia, middleware ├║nico movido para ApiService)
   - **Justificativa**: 
     - GeographicRestriction ├⌐ feature **exclusiva da API HTTP** (n├úo ser├í usada por Workers/Background Jobs)
     - Options s├úo lidas de appsettings que s├│ existem em ApiService
     - FeatureFlags s├úo constantes (similar a `AuthConstants.Claims.*`, `ValidationConstants.MaxLength.*`)
     - Middlewares gen├⌐ricos j├í est├úo em pastas tem├íticas (Authorization/Middleware, Logging/, Monitoring/)

2. **Prop├│sito da Feature Toggle** Γ£à
   - Γ£à **Feature flag ativa/desativa TODA a restri├º├úo geogr├ífica** (on/off global)
   - Γ£à **Cidades individuais controladas via banco de dados** (Sprint 3 - tabela `allowed_regions`)
   - Γ£à **Arquitetura proposta**:
     ```
     FeatureManagement:GeographicRestriction = true  ΓåÆ Liga TODA valida├º├úo
         Γåô
     allowed_regions.is_active = true              ΓåÆ Ativa cidade ESPEC├ìFICA
     ```
   - **MVP (Sprint 1)**: Feature toggle + appsettings (hardcoded cities)
   - **Sprint 3**: Migration para database-backed + Admin Portal UI

3. **Remo├º├úo de Redund├óncia** Γ£à **J├ü REMOVIDO**
   - Γ¥î **REMOVIDO**: Propriedade `GeographicRestrictionOptions.Enabled` (redundante com feature flag)
   - Γ¥î **REMOVIDO**: Verifica├º├úo `|| !_options.Enabled` do middleware
   - Γ£à **├ÜNICA FONTE DE VERDADE**: `FeatureManagement:GeographicRestriction` (feature toggle)
   - **Justificativa**: Ter duas formas de habilitar/desabilitar causa confus├úo e potenciais conflitos.
   - **Benef├¡cio**: Menos configura├º├╡es duplicadas, arquitetura mais clara e segura.

**Organiza├º├úo de Pastas** (21 Nov 2025):
```
src/
  Shared/
    Constants/
      FeatureFlags.cs          ΓåÉ MOVIDO de Configuration/ (constantes globais)
      AuthConstants.cs         (existente)
      ValidationConstants.cs   (existente)
    Authorization/Middleware/  (middlewares de autoriza├º├úo)
    Logging/                   (LoggingContextMiddleware)
    Monitoring/                (BusinessMetricsMiddleware)
    Messaging/Handlers/        (MessageRetryMiddleware)
  
  Bootstrapper/MeAjudaAi.ApiService/
    Middlewares/
      GeographicRestrictionMiddleware.cs  ΓåÉ MOVIDO de Shared/Middleware/
      RateLimitingMiddleware.cs           (espec├¡fico HTTP)
      SecurityHeadersMiddleware.cs        (espec├¡fico HTTP)
    Options/
      GeographicRestrictionOptions.cs     ΓåÉ MOVIDO de Shared/Configuration/
      RateLimitOptions.cs                 (existente)
      CorsOptions.cs                      (existente)
```

**Resultado Sprint 1**: Middleware funcional com valida├º├úo via IBGE API, feature toggle integrado, e lista de cidades configur├ível via appsettings (requer redeploy para altera├º├╡es).

---

#### Γ£à Fase 2: Database-Backed + Admin Portal UI (CONCLU├ìDO - Sprint 7, 7 Jan 2026)

**Contexto**: Migrar lista de cidades/estados de `appsettings.json` para banco de dados, permitindo gest├úo din├ómica via Blazor Admin Portal sem necessidade de redeploy.

**Status**: Γ£à IMPLEMENTADO - AllowedCities UI completa com CRUD, coordenadas geogr├íficas, e raio de servi├ºo.

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

- [ ] **Visualiza├º├úo de Restri├º├╡es Atuais**
  - [ ] Tabela com cidades/estados permitidos
  - [ ] Filtros: Tipo (Cidade/Estado), Estado, Status (Ativo/Inativo)
  - [ ] Ordena├º├úo: Alfab├⌐tica, Data de Adi├º├úo
  - [ ] Indicador visual: Badgets para "Cidade" vs "Estado"

- [ ] **Adicionar Cidade/Estado**
  - [ ] Form com campos:
    - Tipo: Dropdown (Cidade, Estado)
    - Estado: Dropdown preenchido via IBGE API (27 UFs)
    - Cidade: Autocomplete via IBGE API (se tipo=Cidade)
    - Notas: Campo opcional (ex: "Piloto Beta Q1 2025")
  - [ ] Valida├º├╡es:
    - Estado deve ser sigla v├ílida (RJ, SP, MG, etc.)
    - Cidade deve existir no IBGE (valida├º├úo server-side)
    - N├úo permitir duplicatas (cidade+estado ├║nico)
  - [ ] Preview: "Voc├¬ est├í adicionando: Muria├⌐/MG"

- [ ] **Editar Regi├úo**
  - [ ] Apenas permitir editar "Notas" e "Status"
  - [ ] Cidade/Estado s├úo imut├íveis (delete + re-add se necess├írio)
  - [ ] Confirma├º├úo antes de desativar regi├úo com prestadores ativos

- [ ] **Ativar/Desativar Regi├úo**
  - [ ] Toggle switch inline na tabela
  - [ ] Confirma├º├úo: "Desativar [Cidade/Estado] ir├í bloquear novos registros. Prestadores existentes n├úo ser├úo afetados."
  - [ ] Audit log: Registrar quem ativou/desativou e quando

- [ ] **Remover Regi├úo**
  - [ ] Bot├úo de exclus├úo com confirma├º├úo dupla
  - [ ] Valida├º├úo: Bloquear remo├º├úo se houver prestadores registrados nesta regi├úo
  - [ ] Mensagem: "N├úo ├⌐ poss├¡vel remover [Cidade]. Existem 15 prestadores registrados."

**Integra├º├úo com Middleware** (Refactor Necess├írio):

**Abordagem 1: Database-First (Recomendado)**
```csharp
// GeographicRestrictionOptions (modificado)
public class GeographicRestrictionOptions
{
    public bool Enabled { get; set; }
    public string BlockedMessage { get; set; } = "...";
    
    // DEPRECATED: Remover ap├│s migration para database
    [Obsolete("Use database-backed AllowedRegionsService instead")]
    public List<string> AllowedCities { get; set; } = new();
    [Obsolete("Use database-backed AllowedRegionsService instead")]
    public List<string> AllowedStates { get; set; } = new();
}

// Novo servi├ºo
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
        
        // L├│gica de valida├º├úo permanece igual
        if (!allowedCities.Contains(userCity) && !allowedStates.Contains(userState))
        {
            // Bloquear
        }
    }
}
```

**Abordagem 2: Hybrid (Fallback para appsettings)**
- Se banco estiver vazio, usar `appsettings.json`
- Migra├º├úo gradual: Admin adiciona regi├╡es no portal, depois remove de appsettings

**Cache Strategy**:
- Usar `HybridCache` (j├í implementado no `IbgeService`)
- TTL: 5 minutos (balan├ºo entre performance e fresh data)
- Invalida├º├úo: Ao adicionar/remover/editar regi├úo no admin portal

**Migration Path**:
1. **Sprint 3 Semana 1**: Criar schema `geographic_restrictions` + tabela
2. **Sprint 3 Semana 1**: Implementar `AllowedRegionsService` com cache
3. **Sprint 3 Semana 1**: Refactor middleware para usar servi├ºo (mant├⌐m fallback appsettings)
4. **Sprint 3 Semana 2**: Implementar CRUD endpoints no Admin API
5. **Sprint 3 Semana 2**: Implementar UI no Blazor Admin Portal
6. **Sprint 3 P├│s-Deploy**: Popular banco com dados iniciais (Muria├⌐, Itaperuna, Linhares)
7. **Sprint 4**: Remover valores de appsettings.json (obsoleto)

**Testes Necess├írios**:
- [ ] Unit tests: `AllowedRegionsService` (CRUD + cache invalidation)
- [ ] Integration tests: Middleware com banco populado vs vazio
- [ ] E2E tests: Admin adiciona cidade ΓåÆ Middleware bloqueia outras cidades

**Documenta├º├úo**:
- [ ] Admin User Guide: Como adicionar/remover cidades piloto
- [ ] Technical Debt: Marcar `AllowedCities` e `AllowedStates` como obsoletos

**ΓÜá∩╕Å Breaking Changes**:
- ~~`GeographicRestrictionOptions.Enabled` ser├í removido~~ Γ£à **J├ü REMOVIDO** (Sprint 1 Dia 1)
  - **Motivo**: Redundante com feature toggle - fonte de verdade ├║nica
  - **Migra├º├úo**: Usar apenas `FeatureManagement:GeographicRestriction` em appsettings
- `GeographicRestrictionOptions.AllowedCities/AllowedStates` ser├í deprecado (Sprint 3)
  - **Migra├º├úo**: Admin Portal popular├í tabela `allowed_regions` via UI

**Estimativa**:
- **Backend (API + Service)**: 2 dias
- **Frontend (Admin Portal UI)**: 2 dias
- **Migration + Testes**: 1 dia
- **Total**: 5 dias (dentro do Sprint 3 de 2 semanas)

#### 7. Modera├º├úo de Reviews (Prepara├º├úo para Fase 3)
- [ ] **Listagem**: Reviews flagged/reportados
- [ ] **A├º├╡es**: Aprovar, Remover, Banir usu├írio
- [ ] Stub para m├│dulo Reviews (a ser implementado na Fase 3)

**Tecnologias**:
- **Framework**: Blazor WebAssembly (.NET 10)
- **UI**: MudBlazor (Material Design)
- **State**: Fluxor (Flux/Redux pattern)
- **HTTP**: Refit + Polly (retry policies)
- **Charts**: ApexCharts.Blazor

**Resultado Esperado**:
- Γ£à Admin Portal funcional e responsivo
- Γ£à Todas opera├º├╡es CRUD implementadas
- Γ£à Dashboard com m├⌐tricas em tempo real
- Γ£à Deploy em Azure Container Apps

---

### ≡ƒôà Sprint 8A: Customer App & Nx Setup (2 semanas) ΓÅ│ ATUALIZADO

**Status**: CONCLU├ìDA (5-13 Fev 2026)
**Depend├¬ncias**: Sprint 7.16 conclu├¡do Γ£à  
**Dura├º├úo**: 2 semanas

**Contexto**: Sprint dividida em duas partes para acomodar a migra├º├úo para Nx monorepo.

---

#### ≡ƒô▒ Parte 1: Customer App Development (Focus)

**Home & Busca** (Semana 1):
- [ ] **Landing Page**: Hero section + busca r├ípida
- [ ] **Busca Geolocalizada**: Campo de endere├ºo/CEP + raio + servi├ºos
- [ ] **Mapa Interativo**: Exibir prestadores no mapa (Leaflet.Blazor)
- [ ] **Listagem de Resultados**: Cards com foto, nome, rating, dist├óncia, tier badge
- [ ] **Filtros**: Rating m├¡nimo, tier, disponibilidade
- [ ] **Ordena├º├úo**: Dist├óncia, Rating, Tier

**Perfil de Prestador** (Semana 1-2):
- [ ] **Visualiza├º├úo**: Foto, nome, descri├º├úo, servi├ºos, rating, reviews
- [ ] **Contato**: Bot├úo WhatsApp, telefone, email (MVP: links externos)
- [ ] **Galeria**: Fotos do trabalho (se dispon├¡vel)
- [ ] **Reviews**: Listar avalia├º├╡es de outros clientes (read-only, write em Fase 3)
- [ ] **Meu Perfil**: Editar informa├º├╡es b├ísicas

#### ≡ƒ¢á∩╕Å Parte 2: Nx Monorepo Setup
**Status**: 🔄 EM PROGRESSO (Março 2026)

### 🔄 Sprint 8B.2 - Technical Excellence & NX Monorepo (5 - 18 Mar 2026)
**Branch**: `feature/sprint-8b2-technical-excellence`
**Status**: 🔄 EM PROGRESSO

**Objectives**:
1. 🔴 **MUST-HAVE**: **NX Monorepo Setup** (Effort: Large)
    - Initialize workspace.
    - **Migrate** existing `MeAjudaAi.Web.Customer` to `apps/customer-web`.
    - **Scaffolding** (empty placeholders): `apps/provider-web` (8C), `apps/admin-web` (8D).
    - Extract `libs/ui`, `libs/auth`, `libs/api-client`.
2. 🔴 **MUST-HAVE**: **Messaging Unification** (Effort: Medium)
    - Remove Azure Service Bus, unify on RabbitMQ only.
3. 🟡 **RECOMMENDED**: **Slug Implementation** (MOVED TO SPRINT 8C)
4. 🟢 **OPTIONAL**: **Backend Integration Test Optimization** (MOVED TO SPRINT 9)
5. 🟢 **OPTIONAL**: **Frontend Testing & CI/CD Suite** (MOVED TO SPRINT 9)
    - *Dependency*: NX Setup must precede Frontend Testing changes.

---

4. **Auth Migration**: Configurar Keycloak no novo app React.

**Entreg├íveis**:
- [ ] Nx workspace com `apps/admin-portal` e `libs/shared-ui`.
- [ ] Admin Portal React funcional (Providers, ServiceCatalogs).
- [ ] Componentes reutiliz├íveis em biblioteca compartilhada.
- [ ] Testes unit├írios/integra├º├úo configurados.

---

## ≡ƒöº Tarefas T├⌐cnicas Cross-Module ΓÅ│ ATUALIZADO

**Status**: ≡ƒöä EM ANDAMENTO (Sprint 5.5 - 19 Dez 2025)

**Contexto Atual**:
- Γ£à Lock files regenerados em todos os m├│dulos (37 arquivos atualizados)
- Γ£à PR #81 (Aspire 13.1.0) atualizado com lock files corretos
- Γ£à PR #82 (FeatureManagement 4.4.0) atualizado com lock files corretos
- ΓÅ│ Aguardando valida├º├úo CI/CD antes do merge
- ≡ƒôï Desenvolvimento frontend aguardando conclus├úo desta sprint

Tarefas t├⌐cnicas que devem ser aplicadas em todos os m├│dulos para consist├¬ncia e melhores pr├íticas.

### Migration Control em Produ├º├úo

**Issue**: Implementar controle `APPLY_MIGRATIONS` nos m├│dulos restantes

**Contexto**: O m├│dulo Documents j├í implementa controle via vari├ível de ambiente `APPLY_MIGRATIONS` para desabilitar migrations autom├íticas em produ├º├úo. Isso ├⌐ essencial para:
- Ambientes com m├║ltiplas inst├óncias (evita race conditions)
- Deployments controlados via pipeline de CI/CD
- Blue-green deployments onde migrations devem rodar antes do switch

**Implementa├º├úo** (padr├úo estabelecido em `Documents/API/Extensions.cs`):

```csharp
private static void EnsureDatabaseMigrations(WebApplication app)
{
    Keycloak client automation script (setup em 1 comando) - **DAY 1**
- Γ£à 0 analyzer warnings no Admin Portal (S2094, S2953, S2933, MUD0002 resolvidos)
- Γ£à 30-40 testes bUnit (10 ΓåÆ 30+, +200% cobertura)

**Timeline**:
- **Dia 1** (17 Jan): Keycloak automation script - **CRITICAL PATH**
- **Semana 1** (17-24 Jan): Customer App Home + Busca + Warnings fix
- **Semana 2** (24-31 Jan): Customer App Perfil + Mobile + Testes
- **Semana 3** (31 Jan): PolishingVariable("APPLY_MIGRATIONS");
    if (!string.IsNullOrEmpty(applyMigrations) && 
        bool.TryParse(applyMigrations, out var shouldApply) && !shouldApply)
    {
        logger?.LogInformation("Migra├º├╡es autom├íticas desabilitadas via APPLY_MIGRATIONS=false");
        return;
    }

    // Aplicar migrations normalmente
    context.Database.Migrate();
}
```

**Status por M├│dulo**:
- Γ£à **Documents**: Implementado (Sprint 4 - 16 Dez 2025)
- ΓÅ│ **Users**: Pendente
- ΓÅ│ **Providers**: Pendente  
- ΓÅ│ **ServiceCatalogs**: Pendente
- ΓÅ│ **Locations**: Pendente
- ΓÅ│ **SearchProviders**: Pendente

**Esfor├ºo Estimado**: 15 minutos por m├│dulo (copiar padr├úo do Documents)

**Documenta├º├úo**: Padr├úo documentado em `docs/database.md` se├º├úo "Controle de Migrations em Produ├º├úo"

**Prioridade**: M├ëDIA - Implementar antes do primeiro deployment em produ├º├úo

---

## ≡ƒôï Sprint 5.5: Package Lock Files & Dependency Updates (19 Dez 2025)

**Status**: ≡ƒöä EM ANDAMENTO - Aguardando CI/CD  
**Dura├º├úo**: 1 dia  
**Objetivo**: Resolver conflitos de package lock files e atualizar depend├¬ncias

### Contexto

Durante o processo de atualiza├º├úo autom├ítica de depend├¬ncias pelo Dependabot, foram identificados conflitos nos arquivos `packages.lock.json` causados por incompatibilidade de vers├╡es do pacote `Microsoft.OpenApi`.

**Problema Raiz**:
- Lock files esperavam vers├úo `[2.3.12, )` 
- Central Package Management especificava `[2.3.0, )`
- Isso causava erros NU1004 em todos os projetos, impedindo build e testes

### A├º├╡es Executadas

#### Γ£à Corre├º├╡es Implementadas

1. **Branch feature/refactor-and-cleanup**
   - Γ£à 37 arquivos `packages.lock.json` regenerados
   - Γ£à Commit: "chore: regenerate package lock files to fix version conflicts"
   - Γ£à Push para origin conclu├¡do

2. **Branch master**
   - Γ£à Merge de feature/refactor-and-cleanup ΓåÆ master
   - Γ£à Push para origin/master conclu├¡do
   - Γ£à Todos os lock files atualizados na branch principal

3. **PR #81 - Aspire 13.1.0 Update**
   - Branch: `dependabot/nuget/aspire-f7089cdef2`
   - Γ£à Lock files regenerados (37 arquivos)
   - Γ£à Commit: "fix: regenerate package lock files after Aspire 13.1.0 update"
   - Γ£à Force push conclu├¡do
   - ΓÅ│ Aguardando CI/CD (Code Quality Checks, Security Scan)

4. **PR #82 - FeatureManagement 4.4.0 Update**
   - Branch: `dependabot/nuget/Microsoft.FeatureManagement.AspNetCore-4.4.0`
   - Γ£à Lock files regenerados (36 arquivos)
   - Γ£à Commit: "fix: regenerate package lock files after FeatureManagement update"
   - Γ£à Push conclu├¡do
   - ΓÅ│ Aguardando CI/CD (Code Quality Checks, Security Scan)

### Pr├│ximos Passos

1. Γ£à **Merge PRs #81 e #82** - Conclu├¡do (19 Dez 2025)
2. Γ£à **Atualizar feature branch** - Merge master ΓåÆ feature/refactor-and-cleanup
3. Γ£à **Criar PR #83** - Branch feature/refactor-and-cleanup ΓåÆ master
4. ΓÅ│ **Aguardar review e merge PR #83**
5. ≡ƒôï **Iniciar Sprint 6** - GitHub Pages Documentation (Q1 2026)
6. ≡ƒôï **Planejar Sprint 7** - Blazor Admin Portal (Q1 2026)

#### Γ£à Atualiza├º├╡es de Documenta├º├úo (19 Dez 2025)

**Roadmap**:
- Γ£à Atualizada se├º├úo Sprint 5.5 com todas as a├º├╡es executadas
- Γ£à Atualizado status de Fase 2 para "Em Planejamento - Q1 2026"
- Γ£à Atualizados Sprints 3-5 com depend├¬ncias e novas timelines
- Γ£à Atualizada ├║ltima modifica├º├úo para 19 de Dezembro de 2025

**Limpeza de Templates**:
- Γ£à Removido `.github/pull-request-template-coverage.md` (template espec├¡fico de outro PR)
- Γ£à Removida pasta `.github/issue-template/` (issues obsoletas: EFCore.NamingConventions, Npgsql j├í resolvidas)
- Γ£à Criado `.github/pull_request_template.md` (template gen├⌐rico para futuros PRs)
- Γ£à Commit: "chore: remove obsolete templates and create proper PR template"

**Pull Request #83**:
- Γ£à PR criado: feature/refactor-and-cleanup ΓåÆ master
- Γ£à T├¡tulo: "feat: refactoring and cleanup sprint 5.5"
- Γ£à Descri├º├úo atualizada refletindo escopo real (documenta├º├úo + merge PRs #81/#82 + limpeza templates)
- ΓÅ│ Aguardando review e CI/CD validation

### Li├º├╡es Aprendidas

- **Dependabot**: Regenerar lock files manualmente ap├│s updates de vers├╡es com conflicts
- **CI/CD**: Valida├º├úo rigorosa de package locks previne deployments quebrados
- **Central Package Management**: Manter sincroniza├º├úo entre lock files e Directory.Packages.props
- **Template Management**: Manter apenas templates gen├⌐ricos e reutiliz├íveis em `.github/`
- **Documentation-First**: Documentar a├º├╡es executadas imediatamente no roadmap para rastreabilidade

---

### ⏳ Sprint 8C - Provider Web App (React + NX) (19 Mar - 1 Abr 2026)
- Create `apps/provider-web`.
- Implement registration steps (Upload, Dashboard).
- **Slug Implementation**: Replace IDs with Slugs for SEO/Security.

### ⏳ Sprint 8D - Admin Portal Migration (2 - 22 Abr 2026)
**Status**: ⏳ Planned (+1 week buffer added)
**Foco**: Phased migration from Blazor WASM to React.

**Scope (Prioritized)**:
- Providers CRUD + Document Management (Critical).
- Service Catalogs + Allowed Cities.
- Dashboard with KPIs.

> [!IMPORTANT]
> **Fallback Plan**: If Admin Migration slips >5 days by April 10th, choose:
> 1. Ship MVP with current Blazor Admin.
> 2. Reduce scope to only Providers CRUD.

### ⌛ Sprint 9 - BUFFER & Risk Mitigation (23 Abr - 11 Mai 2026)
**Duration**: 12 days buffer (Extended)
- Polishing, Refactoring, and Fixing.
- Move Optional tasks from 8B.2 here if needed.

## 🎯 MVP Final Launch: 12 - 16 de Maio de 2026 🎯

### ⚠️ Risk Assessment & Mitigation

#### Risk Mitigation Strategy
- **Contingency Branching**: If major tasks (Admin Migration, NX Setup) slip, we prioritize essential Player flows (Customer/Provider) and fallback to existing Admin solutions.
- **Sprint 8E (Mobile)**: De-scoped from MVP to Phase 2 to ensure web platform stability.
- **Buffer**: Sprint 9 is strictly for stability, no new features.

---

### ΓÅ│ **19-25 Mar 2026**: Sprint 9 - BUFFER (Polishing, Risk Mitigation, Final Testing)

**Status**: ≡ƒôï PLANEJADO PARA MAR├çO 2026  
**Dura├º├úo**: 1 semana (19-25 Mar 2026)  
**Depend├¬ncias**: Sprints 6-8 completos  
**Natureza**: **BUFFER DE CONTING├èNCIA** - n├úo alocar novas features

> **ΓÜá∩╕Å IMPORTANTE**: Sprint 9 ├⌐ um buffer de conting├¬ncia para absorver riscos e complexidades n├úo previstas dos Sprints 6-8 (primeiro projeto Blazor WASM). N├úo deve ser usado para novas funcionalidades, apenas para:
> - Completar work-in-progress dos sprints anteriores
> - Resolver d├⌐bitos t├⌐cnicos acumulados
> - Mitigar riscos identificados durante implementa├º├úo
> - Polishing e hardening para MVP

**Objetivos**:
- Completar funcionalidades pendentes de Sprints 6-8
- Resolver d├⌐bitos t├⌐cnicos acumulados
- Melhorias de UX/UI identificadas durante desenvolvimento
- Rate limiting e seguran├ºa adicional
- Logging e monitoramento avan├ºado
- Documenta├º├úo final para MVP

### Cen├írios de Risco Documentados

### Risk Scenario 1: Keycloak Integration Complexity

- **Problema Potencial**: OIDC flows em Blazor WASM com refresh tokens podem exigir configura├º├úo complexa
- **Impacto**: +2-3 dias al├⌐m do planejado no Sprint 6
- **Mitiga├º├úo Sprint 9**: 
  - Usar Sprint 9 para refinar authentication flows
  - Implementar proper token refresh handling
  - Adicionar fallback mechanisms

### Risk Scenario 2: MudBlazor Learning Curve

- **Problema Potencial**: Primeira vez usando MudBlazor; componentes complexos (DataGrid, Forms) podem ter comportamentos inesperados
- **Impacto**: +3-4 dias al├⌐m do planejado nos Sprints 6-7
- **Mitiga├º├úo Sprint 9**:
  - Refatorar componentes para seguir best practices MudBlazor
  - Implementar componentes reutiliz├íveis otimizados
  - Documentar patterns e anti-patterns identificados

### Risk Scenario 3: Blazor WASM Performance Issues

- **Problema Potencial**: App bundle size > 5MB, lazy loading n├úo configurado corretamente
- **Impacto**: UX ruim, +2-3 dias de otimiza├º├úo
- **Mitiga├º├úo Sprint 9**:
  - Implementar lazy loading de assemblies
  - Otimizar bundle size (tree shaking, AOT compilation)
  - Adicionar loading indicators e progressive loading

### Risk Scenario 4: MAUI Hybrid Platform-Specific Issues

- **Problema Potencial**: Diferen├ºas de comportamento iOS vs Android (permiss├╡es, geolocation, file access)
- **Impacto**: +4-5 dias de debugging platform-specific
- **Mitiga├º├úo Sprint 9**:
  - Criar abstractions para platform-specific APIs
  - Implementar fallbacks para features n├úo suportadas
  - Testes em devices reais (n├úo apenas emuladores)

### Risk Scenario 5: API Integration Edge Cases

- **Problema Potencial**: Casos de erro n├úo cobertos (timeouts, network failures, concurrent updates)
- **Impacto**: +2-3 dias de hardening
- **Mitiga├º├úo Sprint 9**:
  - Implementar retry policies com Polly
  - Adicionar optimistic concurrency handling
  - Melhorar error messages e user feedback

### Tarefas Sprint 9 (Executar conforme necess├írio)

#### 1. Work-in-Progress Completion
- [ ] Completar funcionalidades parciais de Sprints 6-8
- [ ] Resolver todos os TODOs/FIXMEs adicionados durante implementa├º├úo
- [ ] Fechar issues abertas durante desenvolvimento frontend

#### 1.1. ≡ƒº¬ SearchProviders E2E Tests (Movido da Sprint 7.16)
**Prioridade**: M├ëDIA - Technical Debt da Sprint 7.16  
**Estimativa**: 1-2 dias

**Objetivo**: Testar busca geolocalizada end-to-end.

**Contexto**: Task 5 da Sprint 7.16 foi marcada como OPCIONAL e movida para Sprint 9 para permitir execu├º├úo com qualidade sem press├úo de deadline. Sprint 7.16 completou 4/4 tarefas obrigat├│rias.

**Entreg├íveis**:
- [ ] Teste E2E: Buscar providers por servi├ºo + raio (2km, 5km, 10km)
- [ ] Teste E2E: Validar ordena├º├úo por dist├óncia crescente
- [ ] Teste E2E: Validar restri├º├úo geogr├ífica (AllowedCities) - providers fora da cidade n├úo aparecem
- [ ] Teste E2E: Performance (<500ms para 1000 providers em raio de 10km)
- [ ] Teste E2E: Cen├írio sem resultados (nenhum provider no raio)
- [ ] Teste E2E: Validar pagina├º├úo de resultados (10, 20, 50 items por p├ígina)

**Infraestrutura**:
- Usar `TestcontainersFixture` com PostGIS 16-3.4
- Seed database com providers em localiza├º├╡es conhecidas (lat/lon)
- Usar `HttpClient` para chamar endpoint `/api/search-providers/search`
- Validar JSON response com FluentAssertions

**Crit├⌐rios de Aceita├º├úo**:
- Γ£à 6 testes E2E passando com 100% de cobertura dos cen├írios
- Γ£à Performance validada (95th percentile < 500ms)
- Γ£à Documenta├º├úo em `docs/testing/e2e-tests.md`
- Γ£à CI/CD executando testes E2E na pipeline

#### 2. UX/UI Improvements
- [ ] **Loading States**: Skeletons em todas cargas ass├¡ncronas
- [ ] **Error Handling**: Mensagens friendly para todos erros (n├úo mostrar stack traces)
#### 3. Security & Performance Hardening
- [ ] **API Rate Limiting**: Aspire middleware (100 req/min por IP, 1000 req/min para authenticated users)
- [ ] **CORS**: Configurar origens permitidas (apenas dom├¡nios de produ├º├úo)
- [ ] **CSRF Protection**: Tokens anti-forgery em forms
- [ ] **Security Headers**: HSTS, X-Frame-Options, CSP
- [ ] **Bundle Optimization**: Lazy loading, AOT compilation, tree shaking
- [ ] **Cache Strategy**: Implementar cache HTTP para assets est├íticos

#### 4. Logging & Monitoring
- [ ] **Frontend Logging**: Integra├º├úo com Application Insights (Blazor WASM)
- [ ] **Error Tracking**: Sentry ou similar para erros em produ├º├úo
- [ ] **Analytics**: Google Analytics ou Plausible para usage tracking
- [ ] **Performance Monitoring**: Web Vitals tracking (LCP, FID, CLS)

#### 5. Documenta├º├úo Final MVP
- [ ] **API Documentation**: Swagger/OpenAPI atualizado com exemplos
- [ ] **User Guide**: Guia de uso para Admin Portal e Customer App
- [ ] **Developer Guide**: Como rodar localmente, como contribuir
- [ ] **Deployment Guide**: Deploy em Azure Container Apps (ARM templates ou Bicep)
- [ ] **Lessons Learned**: Documentar decis├╡es de arquitetura e trade-offs

**Resultado Esperado Sprint 9**:
- Γ£à MVP production-ready e polished
- Γ£à Todos os cen├írios de risco mitigados ou resolvidos
- Γ£à Seguran├ºa e performance hardened
- Γ£à Documenta├º├úo completa para usu├írios e desenvolvedores
- Γ£à Monitoring e observabilidade configurados
- ≡ƒÄ» **PRONTO PARA LAUNCH EM 12-16 DE MAIO DE 2026**

> **ΓÜá∩╕Å CRITICAL**: Se Sprint 9 n├úo for suficiente para completar todos os itens, considerar delay do MVP launch ou reduzir escopo (mover features n├úo-cr├¡ticas para post-MVP). A qualidade e estabilidade do MVP s├úo mais importantes que a data de lan├ºamento.

---

## ≡ƒÄ» Fase 3: Qualidade e Monetiza├º├úo

### Objetivo
Introduzir sistema de avalia├º├╡es para ranking, modelo de assinaturas premium via Stripe, e verifica├º├úo automatizada de documentos.

### 3.1. Γ¡É M├│dulo Reviews & Ratings (Planejado)

**Objetivo**: Permitir que clientes avaliem prestadores, influenciando ranking de busca.

#### **Arquitetura Proposta**
- **Padr├úo**: Simple layered architecture
- **Agrega├º├úo**: C├ílculo de `AverageRating` via integration events (n├úo real-time)

#### **Entidades de Dom├¡nio**
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
    public bool IsFlagged { get; } // Para modera├º├úo
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

#### **API P├║blica (IReviewsModuleApi)**
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

#### **Implementa├º├úo**
1. **Schema**: Criar `meajudaai_reviews` com `reviews`, `provider_ratings`
2. **Submit Endpoint**: Validar que cliente pode avaliar (servi├ºo contratado?)
3. **Rating Calculation**: Publicar `ReviewAddedIntegrationEvent` ΓåÆ Search module atualiza `AverageRating`
4. **Modera├º├úo**: Sistema de flag para reviews inapropriados
5. **Testes**: Unit tests para c├ílculo de m├⌐dia + integration tests para submission

---

### 3.2. ≡ƒÆ│ M├│dulo Payments & Billing (Planejado)

**Objetivo**: Gerenciar assinaturas de prestadores via Stripe (Free, Standard, Gold, Platinum).

#### **Arquitetura Proposta**
- **Padr├úo**: Anti-Corruption Layer (ACL) sobre Stripe API
- **Isolamento**: L├│gica de dom├¡nio protegida de mudan├ºas na Stripe

#### **Entidades de Dom├¡nio**
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

#### **API P├║blica (IBillingModuleApi)**
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

#### **Implementa├º├úo**
1. **Stripe Setup**: Configurar produtos e pricing plans no dashboard
2. **Webhook Endpoint**: Receber eventos Stripe (`checkout.session.completed`, `invoice.payment_succeeded`, `customer.subscription.deleted`)
3. **Event Handlers**: Atualizar status de `Subscription` baseado em eventos
4. **Checkout Session**: Gerar URL de checkout para frontend
5. **Integration Events**: Publicar `SubscriptionTierChangedIntegrationEvent` ΓåÆ Search module atualiza ranking
6. **Testes**: Integration tests com mock events da Stripe testing library

---

### 3.3. ≡ƒñû Documents - Verifica├º├úo Automatizada (Planejado - Fase 2)

**Objetivo**: Automatizar verifica├º├úo de documentos via OCR e APIs governamentais.

**Funcionalidades Planejadas**:
- **OCR Inteligente**: Azure AI Vision para extrair texto de documentos
- **Valida├º├úo de Dados**: Cross-check com dados fornecidos pelo prestador
- **Background Checks**: Integra├º├úo com APIs de antecedentes criminais
- **Scoring Autom├ítico**: Sistema de pontua├º├úo baseado em qualidade de documentos

**Background Jobs**:
1. **DocumentUploadedHandler**: Trigger OCR processing
2. **OcrCompletedHandler**: Validar campos extra├¡dos
3. **VerificationScheduler**: Agendar verifica├º├╡es peri├│dicas

**Nota**: Infraestrutura b├ísica j├í existe (campo OcrData, estados de verifica├º├úo), falta implementar workers e integra├º├╡es.

---

### 3.4. ≡ƒÅ╖∩╕Å Dynamic Service Tags (Planejado - Fase 3)

**Objetivo**: Exibir tags de servi├ºos baseadas na popularidade real por regi├úo.

**Funcionalidades**:
- **Endpoint**: `GET /services/top-region?city=SP` (ou lat/lon)
- **L├│gica**: Calcular servi├ºos com maior volume de buscas/contrata├º├╡es na regi├úo do usu├írio.
- **Fallback**: Exibir "Top Globais" se dados regionais insuficientes.
- **Cache**: TTL curto (ex: 1h) para manter relev├óncia sem comprometer performance.

---

## ≡ƒÜÇ Fase 4: Experi├¬ncia e Engajamento (Post-MVP)

### Objetivo
Melhorar experi├¬ncia do usu├írio com agendamentos, comunica├º├╡es centralizadas e analytics avan├ºado.

### 4.1. ≡ƒôà M├│dulo Service Requests & Booking (Planejado)

**Objetivo**: Permitir que clientes solicitem servi├ºos e agendem hor├írios com prestadores.

#### **Funcionalidades**
- **Solicita├º├úo de Servi├ºo**: Cliente descreve necessidade e localiza├º├úo
- **Matching**: Sistema sugere prestadores compat├¡veis
- **Agendamento**: Calend├írio integrado com disponibilidade de prestador
- **Notifica├º├╡es**: Lembretes autom├íticos via Communications module

---

### 4.2. ≡ƒôº M├│dulo Communications (Planejado)

**Objetivo**: Centralizar e orquestrar todas as comunica├º├╡es da plataforma (email, SMS, push).

#### **Arquitetura Proposta**
- **Padr├úo**: Orchestrator Pattern
- **Canais**: Email (SendGrid/Mailgun), SMS (Twilio), Push (Firebase)

#### **API P├║blica (ICommunicationsModuleApi)**
```csharp
public interface ICommunicationsModuleApi : IModuleApi
{
    Task<Result> SendEmailAsync(EmailRequest request, CancellationToken ct = default);
    Task<Result> SendSmsAsync(SmsRequest request, CancellationToken ct = default);
    Task<Result> SendPushNotificationAsync(PushRequest request, CancellationToken ct = default);
}
```

#### **Event Handlers**
- `UserRegisteredIntegrationEvent` ΓåÆ Email de boas-vindas
- `ProviderVerificationFailedIntegrationEvent` ΓåÆ Notifica├º├úo de rejei├º├úo
- `BookingConfirmedIntegrationEvent` ΓåÆ Lembrete de agendamento

#### **Implementa├º├úo**
1. **Channel Handlers**: Implementar `IEmailService`, `ISmsService`, `IPushService`
2. **Template Engine**: Sistema de templates para mensagens (Razor, Handlebars)
3. **Queue Processing**: Background worker para processar fila de mensagens
4. **Retry Logic**: Polly para retry com backoff exponencial
5. **Testes**: Unit tests para handlers + integration tests com mock services

---

### 4.3. ≡ƒôè M├│dulo Analytics & Reporting (Planejado)

**Objetivo**: Capturar, processar e visualizar dados de neg├│cio e operacionais.

#### **Arquitetura Proposta**
- **Padr├úo**: CQRS + Event Sourcing (para audit)
- **Metrics**: Fa├ºade sobre OpenTelemetry/Aspire
- **Audit**: Immutable event log de todas as atividades
- **Reporting**: Denormalized read models para queries r├ípidos

#### **API P├║blica (IAnalyticsModuleApi)**
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
-- vw_provider_summary: Vis├úo hol├¡stica de cada prestador
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

-- vw_financial_transactions: Consolida├º├úo de eventos financeiros
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

-- vw_audit_log_enriched: Audit log leg├¡vel
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

#### **Implementa├º├úo**
1. **Schema**: Criar `meajudaai_analytics` com `audit_log`, reporting tables
2. **Event Handlers**: Consumir todos integration events relevantes
3. **Metrics Integration**: Expor m├⌐tricas customizadas via OpenTelemetry
4. **Reporting API**: Endpoints otimizados para leitura de relat├│rios
5. **Dashboards**: Integra├º├úo com Aspire Dashboard e Grafana
6. **Testes**: Integration tests para event handlers + performance tests para reporting

---

## ≡ƒÄ» Funcionalidades Adicionais Recomendadas (Fase 4+)

### ≡ƒ¢í∩╕Å Admin Portal - M├│dulos Avan├ºados
**Funcionalidades Adicionais (P├│s-MVP)**:
- **Recent Activity Dashboard Widget**: Feed de atividades recentes (registros, uploads, verifica├º├╡es, mudan├ºas de status) com atualiza├º├╡es em tempo real via SignalR
- **User & Provider Analytics**: Dashboards avan├ºados com Grafana
- **Fraud Detection**: Sistema de scoring para detectar perfis suspeitos
- **Bulk Operations**: A├º├╡es em lote (ex: aprovar m├║ltiplos documentos)
- **Audit Trail**: Hist├│rico completo de todas a├º├╡es administrativas

#### ≡ƒôè Recent Activity Widget (Prioridade: M├ëDIA)

**Contexto**: Atualmente o Dashboard exibe apenas gr├íficos est├íticos. Um feed de atividades recentes melhoraria a visibilidade operacional.

**Funcionalidades Core**:
- **Timeline de Eventos**: Feed cronol├│gico de atividades do sistema
- **Tipos de Eventos**:
  - Novos registros de prestadores
  - Uploads de documentos
  - Mudan├ºas de status de verifica├º├úo
  - A├º├╡es administrativas (aprova├º├╡es/rejei├º├╡es)
  - Adi├º├╡es/remo├º├╡es de servi├ºos
- **Filtros**: Por tipo de evento, m├│dulo, data
- **Real-time Updates**: SignalR para atualiza├º├úo autom├ítica
- **Pagina├º├úo**: Carregar mais atividades sob demanda

**Implementa├º├úo T├⌐cnica**:
```csharp
// Domain Events ΓåÆ Integration Events ΓåÆ SignalR Hub
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

**Depend├¬ncias**:
- SignalR configurado no backend
- Event bus consumindo domain events
- ActivityDto contract definido

---

### ≡ƒæñ Customer Profile Management (Alta Prioridade)
**Por qu├¬**: Plano atual ├⌐ muito focado em prestadores; clientes tamb├⌐m precisam de gest├úo de perfil.

**Funcionalidades Core**:
- Editar informa├º├╡es b├ísicas (nome, foto)
- Ver hist├│rico de prestadores contatados
- Gerenciar reviews escritos
- Prefer├¬ncias de notifica├º├╡es

**Implementa├º├úo**: Enhancement ao m├│dulo Users existente

---

### ΓÜû∩╕Å Dispute Resolution System (M├⌐dia Prioridade)
**Por qu├¬**: Mesmo sem pagamentos in-app, disputas podem ocorrer (reviews injustos, m├í conduta).

**Funcionalidades Core**:
- Bot├úo "Reportar" em perfis de prestadores e reviews
- Formul├írio para descrever problema
- Fila no Admin Portal para moderadores

**Implementa├º├úo**: Novo m├│dulo pequeno ou extens├úo do m├│dulo Reviews

---

## ≡ƒôè M├⌐tricas de Sucesso

### ≡ƒôê M├⌐tricas de Produto
- **Crescimento de usu├írios**: 20% ao m├¬s
- **Reten├º├úo de prestadores**: 85%
- **Satisfa├º├úo m├⌐dia**: 4.5+ estrelas
- **Taxa de convers├úo (Free ΓåÆ Paid)**: 15%

### ΓÜí M├⌐tricas T├⌐cnicas (SLOs)

#### **Tiered Performance Targets**

| Categoria | Tempo Alvo | Exemplo |
|-----------|------------|---------|
| **Consultas Simples** | <200ms | Busca por ID, dados em cache |
| **Consultas M├⌐dias** | <500ms | Listagens com filtros b├ísicos |
| **Consultas Complexas** | <1000ms | Busca cross-module, agrega├º├╡es |
| **Consultas Anal├¡ticas** | <3000ms | Relat├│rios, dashboards |

#### **Baseline de Desempenho**
- **Assumindo**: Cache distribu├¡do configurado, ├¡ndices otimizados
- **Revis├úo Trimestral**: Ajustes baseados em m├⌐tricas reais
  - **Percentis monitorados**: P50, P95, P99 (lat├¬ncia de queries)
  - **Frequ├¬ncia**: An├ílise e ajuste a cada 3 meses
  - **Processo**: Feedback loop ΓåÆ identificar outliers ΓåÆ otimizar queries lentas
- **Monitoramento**: OpenTelemetry + Aspire Dashboard + Application Insights

#### **Outros SLOs**
- **Disponibilidade**: 99.9% uptime
- **Seguran├ºa**: Zero vulnerabilidades cr├¡ticas
- **Cobertura de Testes**: >80% para c├│digo cr├¡tico

---

## ≡ƒöä Processo de Gest├úo do Roadmap

### ≡ƒôà Revis├úo Trimestral
- Avalia├º├úo de progresso contra milestones
- Ajuste de prioridades baseado em m├⌐tricas
- An├ílise de feedback de usu├írios e prestadores

### ≡ƒÆ¼ Feedback Cont├¡nuo
- **Input da comunidade**: Surveys, suporte, analytics
- **Feedback de prestadores**: Portal dedicado para sugest├╡es
- **Necessidades de neg├│cio**: Alinhamento com stakeholders

### ≡ƒÄ» Crit├⌐rios de Prioriza├º├úo
1. **Impacto no MVP**: Funcionalidade ├⌐ cr├¡tica para lan├ºamento?
2. **Esfor├ºo de Implementa├º├úo**: Complexidade t├⌐cnica e tempo estimado
3. **Depend├¬ncias**: Quais m├│dulos dependem desta funcionalidade?
4. **Valor para Usu├írio**: Feedback qualitativo e quantitativo

---

## ≡ƒôï Sum├írio Executivo de Prioridades

### Γ£à **Conclu├¡do (Set-Dez 2025)**
1. Γ£à Sprint 0: Migration .NET 10 + Aspire 13 (21 Nov 2025 - MERGED to master)
2. Γ£à Sprint 1: Geographic Restriction + Module Integration (2 Dez 2025 - MERGED to master)
3. Γ£à Sprint 2: Test Coverage 90.56% (10 Dez 2025) - Meta 35% SUPERADA em 55.56pp!
4. Γ£à Sprint 5.5: Package Lock Files Fix (19 Dez 2025)
   - Corre├º├úo conflitos Microsoft.OpenApi (2.3.12 ΓåÆ 2.3.0)
   - 37 arquivos packages.lock.json regenerados
   - PRs #81 e #82 atualizados e aguardando merge
5. Γ£à M├│dulo Users (Conclu├¡do)
6. Γ£à M├│dulo Providers (Conclu├¡do)
7. Γ£à M├│dulo Documents (Conclu├¡do)
8. Γ£à M├│dulo Search & Discovery (Conclu├¡do)
9. Γ£à M├│dulo Locations - CEP lookup e geocoding (Conclu├¡do)
10. Γ£à M├│dulo ServiceCatalogs - Cat├ílogo admin-managed (Conclu├¡do)
11. Γ£à CI/CD - GitHub Actions workflows (.NET 10 + Aspire 13)
12. Γ£à Feature/refactor-and-cleanup branch - Merged to master (19 Dez 2025)

### 📅 Alta Prioridade (Próximos 3 meses - Q1-Q2 2026)
1. 🔄 **Sprint 8B.2: NX Monorepo & Technical Excellence** (Em andamento)
2. ⏳ **Sprint 8C: Provider Web App (React + NX)** (Abril 2026)
3. ⏳ **Sprint 8D: Admin Portal Migration** (Abril 2026)
4. ⏳ **Sprint 9: BUFFER & RISK MITIGATION** (Abril/Maio 2026)
5. 🎯 **MVP Final Launch: 12 - 16 de Maio de 2026**
6. ≡ƒôï API Collections - Bruno .bru files para todos os m├│dulos

### ≡ƒÄ» **M├⌐dia Prioridade (6-12 meses - Fase 2)**
1. Γ¡É M├│dulo Reviews & Ratings
2. ≡ƒÆ│ M├│dulo Payments & Billing (Stripe)
3. ≡ƒñû Documents - Verifica├º├úo automatizada (OCR + Background checks)
4. ≡ƒöä Search - Indexing worker para integration events
5. ≡ƒôè Analytics - M├⌐tricas b├ísicas
6. ≡ƒôº Communications - Email notifications
7. ≡ƒ¢í∩╕Å Dispute Resolution System
8. ≡ƒöº Alinhamento de middleware entre UseSharedServices() e UseSharedServicesAsync()

