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
> **Razão**: SEO crítico para Customer App, performance inicial, ecossistema maduro, contratação facilitada.

**Decisão Estratégica**: Stack unificado em **React + Next.js** para todos os apps web

**Justificativa**:
- ✅ **SEO**: Customer App precisa aparecer no Google ("eletricista RJ") - Next.js SSR/SSG resolve
- ✅ **Performance**: Initial load rápido crítico para conversão mobile - code splitting + lazy loading
- ✅ **Ecossistema**: Massivo - geolocalização, mapas, pagamentos, qualquer problema já resolvido
- ✅ **Contratação**: Fácil escalar time - React devs abundantes
- ✅ **Mobile**: React Native maduro e testado vs MAUI Hybrid ainda novo
- ✅ **Modern Stack**: React 19 + Tailwind v4 é estado da arte (2026)
- ⚠️ **Trade-off**: DTOs duplicados (C# backend, TS frontend) - mitigado com OpenAPI TypeScript Generator

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

**Estratégia de Compartilhamento de Código (C# → TypeScript)**:

| Artefato | Fonte Backend | Saída Frontend | Método de Sincronização |
|----------|----------------|-----------------|-------------|
| **DTOs** | `Contracts/*.cs` | `types/api/*.ts` | OpenAPI Generator (auto) |
| **Enums** | `MeAjudaAi.Contracts/Enums/` | `types/enums.ts` | OpenAPI Generator (auto) |
| **Validação** | FluentValidation | Zod schemas | Geração Automática (Sprint 8A) |
| **Constantes** | `MeAjudaAi.Contracts/Constants/` | `lib/constants.ts` | Geração Automática (Sprint 8A) |

**Plano de Geração**:
1. Implementar ferramenta CLI para converter `MeAjudaAi.Contracts` Enums e Constants em `types/enums.ts` e `lib/constants.ts`.
2. Implementar conversor de metadados FluentValidation para Zod schemas em `types/api/validation.ts`.
3. Adicionar tickets no backlog para verificação em CI e versionamento semântico dos artefatos gerados.

**Nota de Estratégia**: Priorizamos o reuso de `MeAjudaAi.Contracts` para enums e constantes para manter o Frontend alinhado com o Backend e evitar desvios.

**Localização dos Arquivos Gerados**:
```text
src/
├── Contracts/                       # Backend DTOs (C#)
└── Web/
    ├── MeAjudaAi.Web.Admin/         # React + Next.js (migrado do Blazor na Sprint 8D)
    ├── MeAjudaAi.Web.Customer/      # Next.js
    │   └── types/api/generated/     # ← OpenAPI gerado types
    └── Mobile/
        └── MeAjudaAi.Mobile.Customer/   # React Native
            └── src/types/api/             # ← Mesmo OpenAPI gerado types
```

**Pipeline CI/CD** (GitHub Actions):
1. Mudanças no Backend → Swagger JSON atualizado
2. Verificação de OpenAPI diff (breaking changes?)
3. Se houver quebra → Requerer bump de versão da API (`v1` → `v2`)
4. Gerar tipos TypeScript
5. Commit para `types/api/generated/` (auto-commit bot)
6. Testes do Frontend rodam com novos tipos

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
    └── MeAjudaAi.Contracts/         # OpenAPI spec → TypeScript types
```

### 🔐 Autenticação Unificada

**Consistência de Autenticação Cross-Platform**:

| Aspecto | Admin (React) | Customer Web (Next.js) | Customer Mobile (RN) |
|--------|--------------|------------------------|----------------------|
| **Armazenamento de Token** | HTTP-only cookies | HTTP-only cookies | Secure Storage |
| **Vida útil do Token** | 1h acesso + 24h refresh | 1h acesso + 7d refresh | 1h acesso + 30d refresh |
| **Estratégia de Refresh** | Automática (NextAuth) | Middleware refresh | Background refresh |
| **Role Claims** | `role` claim | `role` claim | `role` claim |
| **Sair (Logout)** | `/api/auth/signout` | `/api/auth/signout` | Revoke + clear storage |

**Configuração Keycloak**:
- **Realm**: `MeAjudaAi`
- **Clientes**: `meajudaai-admin` (público), `meajudaai-customer` (público)
- **Roles**: `admin`, `customer`, `provider`
- **Formato Token**: JWT (RS256)
- **Vida útil Token**: Acesso 1h, Refresh 30d (configurável por cliente: Admin=24h, Customer=7d, Mobile=30d)

**Detalhes de Implementação**:
- **Protocolo**: OpenID Connect (OIDC)
- **Provedor de Identidade**: Keycloak
- **Admin Portal**: NextAuth.js v5 (React + Next.js)
- **Customer Web**: NextAuth.js v5 (Next.js)
- **Customer Mobile**: React Native OIDC Client
- **Refresh**: Automático via OIDC interceptor

**Guia de Migração**: Veja `docs/authentication-migration.md` (a ser criado na Sprint 8A)



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
     - GeographicRestriction é funcionalidade **exclusiva da API HTTP** (não será usada por Workers/Background Jobs)
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
   - **MVP (Sprint 1)**: Feature toggle + appsettings (cidades hardcoded)
   - **Sprint 3**: Migração para database-backed + Admin Portal UI

3. **Remoção de Redundância** ✅ **JÁ REMOVIDO**
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

### 🚀 Fase 2 Seguimentos (Mar 2026+)

**Funcionalidades Admin Portal**:

- [ ] **Visualização de Restrições Atuais** (Admin Portal React)
  - [ ] Tabela com cidades/estados permitidos
  - [ ] Filtros: Tipo (Cidade/Estado), Estado, Status (Ativo/Inativo)
  - [ ] Ordenação: Alfabética, Data de Adição
  - [ ] Indicador visual: Badges para "Cidade" vs "Estado"

- [ ] **Adicionar Cidade/Estado**
  - [ ] Formulário com campos:
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
  - [ ] Cidade/Estado são imutáveis (deletar + re-adicionar se necessário)
  - [ ] Confirmação antes de desativar região com prestadores ativos

- [ ] **Ativar/Desativar Região**
  - [ ] Toggle switch inline na tabela
  - [ ] Confirmação: "Desativar [Cidade/Estado] irá bloquear novos registros. Prestadores existentes não serão afetados."
  - [ ] Audit log: Registrar quem ativou/desativou e quando

- [ ] **Remover Região**
  - [ ] Botão de exclusão com confirmação dupla
  - [ ] Validação: Bloquear remoção se houver prestadores registrados nesta região
  - [ ] Mensagem: "Não é possível remover [Cidade]. Existem 15 prestadores registrados."

**Integração com Middleware** (Refatoração Necessária):

**Abordagem 1: Database-First (Recomendado)**
```csharp
// GeographicRestrictionOptions (modificado)
public class GeographicRestrictionOptions
{
    public bool Enabled { get; set; }
    public string BlockedMessage { get; set; } = "...";
    
    // DEPRECATED: Remover após migração para database
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

**Abordagem 2: Híbrida (Fallback para appsettings)**
- Se banco estiver vazio, usar `appsettings.json`
- Migração gradual: Admin adiciona regiões no portal, depois remove de appsettings

**Estratégia de Cache**:
- Usar `HybridCache` (já implementado no `IbgeService`)
- TTL: 5 minutos (balanço entre performance e dados frescos)
- Invalidação: Ao adicionar/remover/editar região no portal administrativo

**Caminho de Migração**:
1. **Sprint 3 Semana 1**: Criar schema `geographic_restrictions` + tabela
2. **Sprint 3 Semana 1**: Implementar `AllowedRegionsService` com cache
3. **Sprint 3 Semana 1**: Refatoração do middleware para usar serviço (mantém fallback appsettings)
4. **Sprint 3 Semana 2**: Implementar endpoints CRUD na API administrativa
5. **Sprint 3 Semana 2**: Implementar UI no Admin Portal (React)
6. **Sprint 3 Pós-Deploy**: Popular banco com dados iniciais (Muriaé, Itaperuna, Linhares)
7. **Sprint 4**: Remover valores de appsettings.json (obsoleto)

**Testes Necessários**:
- [ ] Unit tests: `AllowedRegionsService` (CRUD + invalidação de cache)
- [ ] Integration tests: Middleware com banco populado vs vazio
- [ ] E2E tests: Admin adiciona cidade → Middleware bloqueia outras cidades

**Documentação**:
- [ ] Guia do Usuário Admin: Como adicionar/remover cidades piloto
- [ ] Débito Técnico: Marcar `AllowedCities` e `AllowedStates` como obsoletos

**⚠️ Breaking Changes**:
- ~~`GeographicRestrictionOptions.Enabled` será removido~~ ✅ **JÁ REMOVIDO** (Sprint 1 Dia 1)
  - **Motivo**: Redundante com feature toggle - fonte de verdade única
  - **Migração**: Usar apenas `FeatureManagement:GeographicRestriction` em appsettings
- `GeographicRestrictionOptions.AllowedCities/AllowedStates` será deprecado (Sprint 3)
  - **Migração**: Admin Portal populará tabela `allowed_regions` via UI

**Estimativa**:
- **Backend (API + Serviço)**: 2 dias
- **Frontend (Admin Portal UI)**: 2 dias
- **Migração + Testes**: 1 dia
- **Total**: 5 dias (dentro da Sprint 3 de 2 semanas)

#### 7. Moderação de Avaliações (Preparação para Fase 3)
- [ ] **Listagem**: Avaliações sinalizadas/reportadas
- [ ] **Ações**: Aprovar, Remover, Banir usuário
- [ ] Stub para módulo de Avaliações (a ser implementado na Fase 3)

**Tecnologias (Admin Portal React)**:
- **Framework**: React 19 + TypeScript 5.7+
- **UI**: Tailwind CSS v4 + Base UI
- **Estado**: Zustand
- **HTTP**: TanStack Query + React Hook Form
- **Gráficos**: Recharts

**Resultado Esperado**:
- ✅ Admin Portal funcional e responsivo (React)
- ✅ Todas operações CRUD implementadas
- ✅ Dashboard com métricas em tempo real
- ✅ Deploy no Azure Container Apps

---

### 📅 Sprint 8A: Customer App & Nx Setup (2 semanas) ⏳ ATUALIZADO

**Status**: CONCLUÍDA (5-13 Fev 2026)
**Dependências**: Sprint 7.16 concluído ✅  
**Duração**: 2 semanas

**Contexto**: Sprint dividida em duas partes para acomodar a migração para Nx monorepo.

---

#### 📱 Parte 1: Desenvolvimento do Customer App (Foco)

**Home & Busca** (Semana 1):
- [ ] **Página de Destino**: Seção Hero + busca rápida
- [ ] **Busca Geolocalizada**: Campo de endereço/CEP + raio + serviços
- [ ] **Mapa Interativo**: Exibir prestadores no mapa (Leaflet.Blazor)
- [ ] **Listagem de Resultados**: Cards com foto, nome, nota, distância, tier badge
- [ ] **Filtros**: Nota mínima, tier, disponibilidade
- [ ] **Ordenação**: Distância, Nota, Tier

**Perfil de Prestador** (Semana 1-2):
- [ ] **Visualização**: Foto, nome, descrição, serviços, nota, avaliações
- [ ] **Contato**: Botão WhatsApp, telefone, e-mail (MVP: links externos)
- [ ] **Galeria**: Fotos do trabalho (se disponível)
- [ ] **Avaliações**: Listar avaliações de outros clientes (apenas leitura, escrita na Fase 3)
- [ ] **Meu Perfil**: Editar informações básicas

#### 🛠️ Parte 2: Configuração do Nx Monorepo
**Status**: 🔄 EM PROGRESSO (Março 2026)  
*Nota: Este é um contêiner ampliado que representa múltiplas sprints destinadas à reestruturação modular do front-end web. A "Sprint 8B.2" encapsula a fundação inicial concluída como parte intrínseca deste arco arquitetural.*

### ✅ Sprint 8B.2 - NX Scaffolding & Migração Inicial (5 - 18 Mar 2026)
**Branch**: `feature/sprint-8b2-monorepo-cleanup`
**Status**: 🔄 EM REVISÃO
*Nota: A atualização final para "✅ CONCLUÍDA" deve ocorrer somente após o merge do PR ou confirmação explícita de finalização do trabalho na branch.*

**Objetivos**:
1. 🔴 **MUST-HAVE**: **Configuração do NX Monorepo** (Esforço: Grande)
    - Inicializar workspace.
    - **Migrar** `MeAjudaAi.Web.Customer` existente para `apps/customer-web`.
    - **Andaime (Scaffolding)** (placeholders vazios): `apps/provider-web` e `apps/admin-portal`.
    - Extrair bibliotecas compartilhadas: `libs/ui`, `libs/auth`, `libs/api-client`.
2. 🔴 **MUST-HAVE**: **Unificação de Mensageria** (Esforço: Médio)
    - Remover Azure Service Bus, unificar apenas no RabbitMQ.
3. 🔴 **MUST-HAVE**: **Pacote de Excelência Técnica** (Esforço: Médio)
    - [ ] [**TD**] **Automação Keycloak**: `setup-keycloak-clients.ps1` para desenvolvimento local.
    - [ ] [**TD**] **Limpeza de Analisadores**: Corrigir avisos SonarLint nos apps React e Contratos.
    - [ ] [**TD**] **Refatoração de Extensões**: Extrair `BusinessMetricsMiddlewareExtensions`.
    - [ ] [**TD**] **Logging Polly**: Migrar logs de resiliência para ILogger (Issue #113).
    - [ ] [**TD**] **Padronização**: Alinhamento de sintaxe de Record em `Contracts`.
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

### Controle de Migração em Produção

**Problema**: Implementar controle `APPLY_MIGRATIONS` nos módulos restantes

**Contexto**: O módulo Documents já implementa controle via variável de ambiente `APPLY_MIGRATIONS` para desabilitar migrations automáticas em produção.

**Implementação** (padrão estabelecido em `Documents/API/Extensions.cs`):

```csharp
private static void EnsureDatabaseMigrations(WebApplication app)
{
    // Lê a variável de ambiente (ou de IConfiguration)
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

## 📋 Sprint 5.5: Arquivos de Lock de Pacote e Atualizações de Dependência (19 Dez 2025)

**Status**: 🔄 EM ANDAMENTO - Aguardando CI/CD  
**Duração**: 1 dia  
**Objetivo**: Resolver conflitos de arquivos de lock de pacote e atualizar dependências

### Contexto

Durante o processo de atualização automática de dependências pelo Dependabot, foram identificados conflitos nos arquivos `packages.lock.json` causados por incompatibilidade de versões do pacote `Microsoft.OpenApi`.

**Problema Raiz**:
- Arquivos de lock esperavam versão `[2.3.12, )` 
- Central Package Management especificava `[2.3.0, )`
- Isso causava erros NU1004 em todos os projetos, impedindo compilação e testes

### Ações Executadas

#### ✅ Correções Implementadas

1. **Branch feature/refactor-and-cleanup**
   - ✅ 37 arquivos `packages.lock.json` regenerados
   - ✅ Commit: "chore: regenerate package lock files to fix version conflicts"
   - ✅ Push para origin concluído

2. **Branch master**
   - ✅ Merge de feature/refactor-and-cleanup → master
   - ✅ Push para origin/master concluído
   - ✅ Todos os arquivos de lock atualizados na branch principal

3. **PR #81 - Atualização do Aspire 13.1.0**
   - Branch: `dependabot/nuget/aspire-f7089cdef2`
   - ✅ Arquivos de lock regenerados (37 arquivos)
   - ✅ Commit: "fix: regenerate package lock files after Aspire 13.1.0 update"
   - ✅ Force push concluído
   - ⏳ Aguardando CI/CD (Verificações de Qualidade de Código, Escaneamento de Segurança)

4. **PR #82 - Atualização do FeatureManagement 4.4.0**
   - Branch: `dependabot/nuget/Microsoft.FeatureManagement.AspNetCore-4.4.0`
   - ✅ Arquivos de lock regenerados (36 arquivos)
   - ✅ Commit: "fix: regenerate package lock files after FeatureManagement update"
   - ✅ Push concluído
   - ⏳ Aguardando CI/CD (Verificações de Qualidade de Código, Escaneamento de Segurança)

### Próximos Passos

1. ✅ **Merge dos PRs #81 e #82** - Concluído (19 Dez 2025)
2. ✅ **Atualizar branch de funcionalidade** - Merge master → feature/refactor-and-cleanup
3. ✅ **Criar PR #83** - Branch feature/refactor-and-cleanup → master
4. ⏳ **Aguardar revisão e merge do PR #83**
5. 📋 **Iniciar Sprint 6** - Documentação do GitHub Pages (Q1 2026)
6. 📋 **Planejar Sprint 7** - Blazor Admin Portal (Q1 2026)

#### ✅ Atualizações de Documentação (19 Dez 2025)

**Roadmap**:
- ✅ Atualizada seção Sprint 5.5 com todas as ações executadas
- ✅ Atualizado status de Fase 2 para "Em Planejamento - Q1 2026"
- ✅ Atualizados Sprints 3-5 com dependências e novas timelines
- ✅ Atualizada última modificação para 19 de Dezembro de 2025

**Limpeza de Modelos**:
- ✅ Removido `.github/pull-request-template-coverage.md` (modelo específico de outro PR)
- ✅ Removida pasta `.github/issue-template/` (issues obsoletas: EFCore.NamingConventions, Npgsql já resolvidas)
- ✅ Criado `.github/pull_request_template.md` (modelo genérico para futuros PRs)
- ✅ Commit: "chore: remove obsolete templates and create proper PR template"

**Pull Request #83**:
- ✅ PR criado: feature/refactor-and-cleanup → master
- ✅ Título: "feat: refactoring and cleanup sprint 5.5"
- ✅ Descrição atualizada refletindo escopo real (documentação + merge dos PRs #81/#82 + limpeza de modelos)
- ⏳ Aguardando revisão e validação do CI/CD

### Lições Aprendidas

- **Dependabot**: Regenerar arquivos de lock manualmente após atualizações de versões com conflitos
- **CI/CD**: Validação rigorosa dos locks de pacote previne implantações quebradas
- **Central Package Management**: Manter sincronização entre arquivos de lock e Directory.Packages.props
- **Gestão de Modelos**: Manter apenas modelos genéricos e reutilizáveis em `.github/`
- **Documentação Primeiro**: Documentar ações executadas imediatamente no roadmap para rastreabilidade

---

### ✅ Sprint 8C - Provider Web App (React + NX) (19 Mar - 21 Mar 2026)
- ✅ **Nx Integration**: `MeAjudaAi.Web.Provider` integrado ao workspace Nx
- ✅ **Integração de Onboarding**: 
  - `/onboarding/basic-info` conectado à API (`apiMeGet`/`apiMePut`)
  - `/onboarding/documents` conectado à API (upload via SAS URL para Azure Blob Storage)
- ✅ **Painel com Dados Reais**: Página principal (`/`) substituída por dados reais via `apiMeGet`
- ✅ **Perfil Público do Prestador**: Nova rota `/provider/[slug]` para perfis públicos com slugs amigáveis ao SEO
- ✅ **Gestão do Perfil do Prestador**:
  - `/alterar-dados` - Edição completa via `apiMePut`
  - `/configuracoes` - Alternância de visibilidade + exclusão de conta com confirmação LGPD
- ✅ **URLs amigáveis (Slug)**: Perfis públicos acessíveis via slugs (ex: `/provider/joao-silva-a1b2c3d4`)

### ✅ Sprint 8D - Migração do Portal Administrativo (2 - 24 Mar 2026)

**Status**: ✅ CONCLUÍDA (24 Mar 2026)
**Foco**: Migração em fases do Blazor WASM para React.

**Entregáveis**:
- ✅ **Admin Portal React**: `src/Web/MeAjudaAi.Web.Admin/` funcional em React.
- ✅ **CRUD de Prestadores**: Gestão completa de prestadores.
- ✅ **Gestão de Documentos**: Envio e verificação de documentos.
- ✅ **Catálogo de Serviços**: Gestão do catálogo de serviços.
- ✅ **Cidades Permitidas**: Gestão de restrições geográficas.
- ✅ **KPIs do Painel**: Painel administrativo com métricas.

### ✅ Sprint 8E - Testes E2E e Infraestrutura de Teste React (23 Mar - 25 Mar 2026)

**Status**: ✅ CONCLUÍDA (25 Mar 2026)
**Foco**: Testes E2E (Playwright) + infraestrutura de testes unitários (Vitest + RTL + MSW) + Governança de Cobertura Global.

**Escopo — E2E (Playwright)** ✅:
1. ✅ **Configuração Playwright**: `playwright.config.ts` com 6 projetos (Chromium, Firefox, WebKit, Mobile, CI)
2. ✅ **Customer E2E** (5 especificações): auth, onboarding, performance, profile, search
3. ✅ **Provider E2E** (5 especificações): auth, dashboard, onboarding, performance, profile-mgmt
4. ✅ **Admin E2E** (5 especificações): auth, configs, dashboard, mobile-responsiveness, providers
5. ✅ **Fixtures Compartilhadas**: `src/Web/libs/e2e-support/base.ts` (loginAsAdmin, loginAsProvider, loginAsCustomer, logout)
6. ✅ **Integração CI**: `master-ci-cd.yml` atualizado para gerar especificação OpenAPI e rodar E2E.

**Escopo — Testes Unitários (Vitest + RTL)** ✅:
7. ✅ **Infraestrutura**: `libs/test-support/` (test-utils.tsx, customRenderHook), limites individuais removidos em favor de Cobertura Global.
8. ✅ **Cobertura Global**: Script `src/Web/scripts/merge-coverage.mjs` consolida relatórios de todos os projetos com limite de 70%.
9. ✅ **Hardening Admin**: Testes unitários para `Sidebar`, `Button`, `Dashboard`, `Providers` e `Users`. Autenticação centralizada em `auth.ts`.
10. ✅ **Hardening Customer**: `DashboardClient` (conformidade com DTO), `DocumentUpload` (asserções da API) e `SearchFilters` (validação de categoria da API).

**Cenários de Teste E2E**:
- [x] Autenticação (login, logout, refresh token)
- [x] Fluxo de onboarding (Cliente e Prestador)
- [x] CRUD de prestadores e serviços (Admin)
- [x] Busca e filtros geolocalizados
- [x] Responsividade mobile
- [x] Desempenho e Core Web Vitals (INP, LCP, CLS)

**Pendências para fechar Sprint**:
- [x] Testes unitários Admin (hooks: providers, categories, dashboard, services, allowed-cities, users; components: sidebar, ui)
- [x] Testes unitários Prestador (hooks; components: dashboard cards, profile)
- [x] Configurar MSW handlers para Admin e Prestador

### ✅ Sprint 9 - BUFFER & Mitigação de Risco (25 Mar - 11 Abr 2026)

**Status**: ✅ ENCERRADA (Snapshot: encerrada em 11 Abr 2026)
**Duração**: 12 dias de buffer (finalização do MVP)
- Polimento, Refatoração e Correção.
- Mover tarefas Opcionais da 8B.2 para cá se necessário.
- Limite de taxa (Rate limiting) e segurança/monitoramento avançado.

**Seguimentos Pendentes**:
- [ ] **Gating de Diff OpenAPI**: Adicionar verificação de mudanças de quebra em CI (falhar PR se API mudar sem bump de versão)
- [x] **Módulo de Comunicações**: ~~Implementar infraestrutura base (outbox pattern, modelos, handlers de evento)~~ ✅ Implementado (exceção BUFFER — infraestrutura entregue nesta sprint)

## 🎯 MVP Final Launch: 12 - 16 de Maio de 2026 🎯

### ⚠️ Avaliação e Mitigação de Risco

#### Estratégia de Mitigação de Risco
- **Contingência de Ramificação**: Se as tarefas principais (Migração Admin, Configuração NX) atrasarem, priorizaremos os fluxos essenciais do Jogador (Cliente/Prestador) e recairemos sobre as soluções Admin existentes.
- **Aplicativos Móveis**: Retirados do escopo do MVP para a Fase 2 para garantir a estabilidade da plataforma web.
- **Buffer**: A Sprint 9 é estritamente para estabilidade, sem novas funcionalidades (Exceção: infraestrutura do Módulo de Comunicações).
- Documentação final para o MVP

### Cenários de Risco Documentados

### Cenário de Risco 1: Complexidade de Integração do Keycloak

- **Problema Potencial**: Fluxos OIDC em Blazor WASM com tokens de atualização podem exigir configuração complexa
- **Impacto**: +2-3 dias além do planejado na Sprint 6
- **Mitigação Sprint 9**: 
  - Usar a Sprint 9 para refinar os fluxos de autenticação
  - Implementar o tratamento adequado de atualização de token
  - Adicionar mecanismos de fallback

### Cenário de Risco 2: Problemas de Desempenho do React

- **Problema Potencial**: Tamanho do pacote do aplicativo > 5MB, carregamento lento não configurado corretamente
- **Impacto**: Experiência do Usuário (UX) ruim, +2-3 dias de otimização
- **Mitigação Sprint 9**:
  - Divisão de código com importações dinâmicas
  - Tree shaking e otimização de pacotes
  - SSR/SSG via Next.js para melhorar o carregamento inicial
  - Carregamento lento de componentes React
  - Otimizar imagens usando next/image e formatos responsivos

### Cenário de Risco 3: Problemas Específicos da Plataforma MAUI Hybrid (REMOVIDO DO ESCOPO DO MVP)

> **⚠️ IMPORTANTE**: Este cenário de risco foi removido do escopo do MVP. Os Aplicativos Móveis foram adiados para a Fase 2 conforme nota acima.

- **Problema Potencial**: Diferenças de comportamento iOS vs Android (permissões, geolocalização, acesso a arquivos)
- **Impacto**: +4-5 dias de depuração específica da plataforma
- **Mitigação Sprint 9**:
  - Criar abstrações para APIs específicas da plataforma
  - Implementar fallbacks para funcionalidades não suportadas
  - Testes em dispositivos reais (não apenas emuladores)

### Cenário de Risco 4: Casos de Borda da Integração de API

- **Problema Potencial**: Casos de erro não cobertos (tempos limite, falhas de rede, atualizações simultâneas)
- **Impacto**: +2-3 dias de endurecimento (hardening)
- **Mitigação Sprint 9**:
  - Implementar políticas de nova tentativa com Polly
  - Adicionar tratamento de concorrência otimista
  - Melhorar as mensagens de erro e o feedback do usuário

### Tarefas Sprint 9 (Executar conforme necessário)

#### 1. Conclusão do Trabalho em Andamento
- [ ] Completar funcionalidades parciais das Sprints 6-8
- [ ] Resolver todos os TODOs/FIXMEs adicionados durante a implementação
- [ ] Fechar issues abertas durante o desenvolvimento frontend

#### 1.1. ✅ 🧪 SearchProviders Testes E2E (Concluído)
- [x] Teste E2E: Buscar prestadores por serviço + raio (2km, 5km, 10km)
- [x] Teste E2E: Validar ordenação por distância crescente
- [x] Teste E2E: Validar restrição geográfica (AllowedCities) - prestadores fora da cidade não aparecem
- [x] Teste E2E: Desempenho (<1500ms em ambiente de teste)
- [x] Teste E2E: Cenário sem resultados (nenhum prestador no raio)
- [x] Teste E2E: Validar paginação de resultados (10, 20, 50 itens por página)

**Infraestrutura**:
- Usar `TestcontainersFixture` com PostGIS 16-3.4
- Semear banco de dados com prestadores em localizações conhecidas (lat/lon)
- Usar `HttpClient` para chamar o ponto de extremidade `/api/search-providers/search`
- Validar a resposta JSON com FluentAssertions

**Critérios de Aceitação**:
- ✅ 6 testes E2E passando com 100% de cobertura dos cenários
- ✅ Desempenho validado
- ✅ Documentação em `docs/testing/e2e-tests.md`
- ✅ CI/CD executando testes E2E na pipeline

#### 1.2. 🛠️ Excelência Técnica e Débito (Sprint 9)
**Prioridade**: MÉDIA
**Estimativa**: 3-4 dias

**Objetivo**: Resolver pendências técnicas e melhorar a resiliência do sistema.

**Entregáveis**:
- [x] **Generalização do Outbox Pattern**: Mover estrutura base (entidade, repositório, worker base) para `MeAjudaAi.Shared` para reuso em futuros módulos (Payments, Bookings).
- [ ] **Handlers de Evento**: Implementar handlers para comunicação entre SearchProviders e ServiceCatalogs.

- [ ] **Login Social**: Reintegrar login com Instagram via Keycloak OIDC (Issue #141).
- [ ] **Resiliência**: Aplicar `CancellationToken` nos Effects de `ServiceCatalogs`, `Documents` e `Locations`.
- [ ] **Localização (Backend)**: Migrar strings de erro da API para `.resx` e integrar FluentValidation para suporte a multi-idioma via cabeçalhos.
- [ ] **Localização (Frontend React)**: Implementar infraestrutura com `i18next` (JSON) nos apps Admin e Customer, localizando mensagens de validação do **Zod**.
- [ ] **Testes Arquiteturais**: Implementar testes com `NetArchTest` no Módulo de Comunicações para garantir isolamento (evitar referências circulares e acesso direto a DBs externos).
- [ ] **Identidade Visual da Interface**: Aplicar cores oficiais (Azul, Creme, Laranja) e padronizar o tema em todo o Portal Administrativo (React).
- [ ] **Testes Unitários (Débito)**: Testes para descarte de `LocalizationSubscription` e despejo LRU do `PerformanceHelper`.

#### 2. Melhorias de UX/UI
- [ ] **Estados de Carregamento**: Skeletons em todas as cargas assíncronas
- [ ] **Tratamento de Erros**: Mensagens amigáveis para todos os erros (não mostrar stack traces)

#### 3. Endurecimento de Segurança e Desempenho
- [ ] **Limite de Taxa da API**: Middleware Aspire (100 req/min por IP, 1000 req/min para usuários autenticados)
- [ ] **CORS**: Configurar origens permitidas (apenas domínios de produção)
- [ ] **Proteção CSRF**: Tokens anti-falsificação em formulários
- [ ] **Cabeçalhos de Segurança**: HSTS, X-Frame-Options, CSP
- [ ] **Otimização do Pacote**: Carregamento lento (lazy loading), compilação AOT, tree shaking
- [ ] **Estratégia de Cache**: Implementar cache HTTP para ativos estáticos

#### 4. Registro e Monitoramento
- [ ] **Registro do Frontend**: Integração com Application Insights (React + Next.js)
- [ ] **Rastreamento de Erros**: Sentry ou similar para erros em produção
- [ ] **Análises (Analytics)**: Google Analytics ou Plausible para rastreamento de uso
- [ ] **Monitoramento de Desempenho**: Rastreamento de Web Vitals (LCP, FID, CLS)

#### 5. Documentação Final do MVP
- [ ] **Documentação da API**: Swagger/OpenAPI atualizado com exemplos
- [ ] **Guia do Usuário**: Guia de uso para o Portal Administrativo e o Aplicativo do Cliente
- [ ] **Guia do Desenvolvedor**: Como rodar localmente, como contribuir
- [ ] **Guia de Implantação**: Implantação nos Aplicativos de Contêiner do Azure (modelos ARM ou Bicep)
- [ ] **Lições Aprendidas**: Documentar decisões de arquitetura e compensações (trade-offs)

#### 6. Módulo de Comunicações (NOVO - Sprint 9)

**Prioridade**: MÉDIA - Infraestrutura base para funcionalidades pós-MVP  
**Objetivo**: Criar módulo unificado de comunicações (e-mail, SMS, push)  
**Contexto**: Outros módulos (Avaliações, Pagamentos, Reservas) dependem de infraestrutura de comunicações. Implementar agora evita refatoração depois.

**Estratégia de Testes (Mandatória)**:

| Tipo | Escopo | Ferramentas |
|------|--------|------------|
| **Unitários** | Lógica de modelos, cálculo de tentativas (retries), mapeamento de DTOs | ✅ xUnit + FluentAssertions |
| **Integrados** | Persistência do Outbox (PostgreSQL), Handlers de eventos | ✅ xUnit + Respawn + Docker |
| **E2E** | Fluxo completo: Evento → Outbox → Envio simulado → Registro | ✅ Playwright + MSW |
| **Arquiteturais** | Validar que módulos não acessam DB de outros, dependências de contratos | ✅ NetArchTest |

---

**Decisão de Arquitetura** (diferente dos outros módulos):

| Aspecto | Decisão |
|---------|---------|
| **Padrão** | Módulo Completo + Padrão Orquestrador |
| **Infraestrutura** | Padrão Outbox para garantia de entrega (Mensageria Confiável) |
| **Integração** | Baseada em eventos (consome IntegrationEvents) |
| **API Externa** | Abstração via interface (provedor configurável) |
| **Idempotência** | Garantida via CorrelationId/EventId nos Registros |

---

**Arquitetura de Projetos**:
```text
src/
├── Shared/
│   └── MeAjudaAi.Contracts/Modules/Communications/
│       ├── ICommunicationsModuleApi.cs
│       ├── DTOs/
│       │   ├── EmailMessageDto.cs
│       │   ├── EmailTemplateDto.cs
│       │   ├── PushMessageDto.cs
│       │   ├── SmsMessageDto.cs
│       │   └── CommunicationLogDto.cs
│       ├── Channels/
│       │   ├── IEmailChannel.cs
│       │   ├── IPushChannel.cs
│       │   └── ISmsChannel.cs
│       └── Queries/
│           └── CommunicationLogQuery.cs
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
│       │           └── StubEmailService.cs  # Provedor atual (Stubs)
│       ├── Domain/
│       │   ├── MeAjudaAi.Modules.Communications.Domain.csproj
│       │   └── Entities/
│       │       ├── OutboxMessage.cs (suporte a ScheduledAt)
│       │       ├── EmailTemplate.cs (flag IsSystemTemplate)
│       │       └── CommunicationLog.cs (suporte a CorrelationId)
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

**Localização de Modelos**:
- **Fonte de Verdade**: Arquivos `.cshtml` em `Shared/Communications/Templates/` (compilados).
- **Sistema de Sobreposição (Override)**: A entidade `EmailTemplate` no banco permite sobrescrever o Assunto (`Subject`) ou trechos do Corpo (`Body`) via Portal Administrativo sem nova implantação.
- **IsSystemTemplate**: Flag para proteger modelos críticos de deleção (Garantido via validação em `EmailTemplateRepository.DeleteAsync`).

---

**Mapeamento de Integração com Eventos**:

| Evento existente | Ação de Comunicação |
|----------------|-------------------|
| `UserRegisteredIntegrationEvent` | ✅ Enviar e-mail de boas-vindas |
| `ProviderAwaitingVerificationIntegrationEvent` | ✅ Notificar administrador |
| `ProviderVerificationStatusUpdatedIntegrationEvent` | ✅ Notificar prestador |
| `DocumentVerifiedIntegrationEvent` | [ ] Notificar prestador |
| `DocumentRejectedIntegrationEvent` | [ ] Notificar prestador |

---

**Interface ICommunicationsModuleApi (Atualizada)**:
> **Nota**: `ECommunicationPriority` é proveniente de `MeAjudaAi.Contracts.Shared` (não redeclare o enum — use o tipo compartilhado `ECommunicationPriority` diretamente).

```csharp
public interface ICommunicationsModuleApi : IModuleApi
{
    // E-mail
    Task<Result<Guid>> SendEmailAsync(
        EmailMessageDto email, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default);
        
    Task<Result<IReadOnlyList<EmailTemplateDto>>> GetTemplatesAsync(CancellationToken ct = default);
    
    // SMS
    Task<Result<Guid>> SendSmsAsync(
        SmsMessageDto sms, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default);
    
    // Push
    Task<Result<Guid>> SendPushAsync(
        PushMessageDto push, 
        ECommunicationPriority priority = ECommunicationPriority.Normal,
        CancellationToken ct = default);
    
    // Registros (Verificação de idempotência via Identificador de Correlação)
    Task<Result<PagedResult<CommunicationLogDto>>> GetLogsAsync(
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
    public string? CorrelationId { get; } // Idempotência
    public ECommunicationChannel Channel { get; }
    public string Payload { get; }       // JSON serializado
    public EOutboxMessageStatus Status { get; } // Pending, Processing, Sent, Failed
    public int RetryCount { get; }
    public DateTime CreatedAt { get; }
    public DateTime? ScheduledAt { get; } // Agendamento futuro
    public DateTime? SentAt { get; }
    public string? ErrorMessage { get; }
}

// EmailTemplate: Modelos com sistema de sobreposição
public class EmailTemplate
{
    public Guid Id { get; }
    public string TemplateKey { get; }    // "welcome", "verification-approved"
    public string? OverrideKey { get; }   // Contexto opcional
    public string Subject { get; }
    public string HtmlBody { get; }
    public string TextBody { get; }
    public string Language { get; }       // pt-br, en-us
    public bool IsSystemTemplate { get; } // Proteção contra exclusão
    public DateTime CreatedAt { get; }
}

// CommunicationLog: Trilha de auditoria + Idempotência
public class CommunicationLog
{
    public Guid Id { get; }
    public string CorrelationId { get; }  // Idempotência (identificador único)
    public ECommunicationChannel Channel { get; }
    public string Recipient { get; }
    public string? TemplateKey { get; }
    public bool IsSuccess { get; }
    public DateTime CreatedAt { get; }
    public string? ErrorMessage { get; }
}

---

**Infraestrutura - Padrão Outbox**:

Para garantir que as comunicações não sejam perdidas em caso de falha:

1. **Processo com Outbox** (garantido):
   ```text
   Evento Ocorrido → Salvar na Tabela de Outbox (Mesma Transação) → Trabalhador de Background processa Outbox
   ```

**Melhorias Implementadas**:
- ✅ **Nova tentativa automática**: Com recuo exponencial via Polly.
- ✅ **Priorização**: Mensagens de alta prioridade (ex: Redefinição de Senha) furam a fila.
- ✅ **Idempotência**: Verificação de `CorrelationId` no Registro antes do processamento.
- ✅ **Recuperação**: Mecanismo para resetar mensagens travadas no estado Processing.

---

**Estimativa de Esforço**:

| Tarefa | Esforço | Status |
|------|--------|-----------|
| 1. Criar estrutura de projetos | 2h | ✅ |
| 2. Interfaces ICommunicationsModuleApi | 2h | ✅ |
| 3. Implementar OutboxMessage (Agendamento) | 5h | ✅ |
| 4. Implementar EmailTemplate (Sistema de sobreposição) | 3h | ✅ |
| 5. Implementar CommunicationLog (CorrelationId) | 2h | ✅ |
| 6. Implementar ModuleApi + Orquestrador | 6h | ✅ |
| 7. Handlers de Canal de Stub (E-mail/Sms/Push) | 5h | ✅ |
| 8. Integração com Eventos Existentes (3/5) | 4h | ✅ |
| 9. Criar modelos básicos (.cshtml) | 3h | ✅ |
| 10. Configuração de DI + Políticas Polly | 3h | ✅ |
| **Total** | **~35h (~5 dias)** | ✅ |

---

**Critérios de Aceitação**:
- ✅ Módulo registrado no ModuleApiRegistry
- ✅ Envio garantido via Padrão Outbox
- ✅ Suporte ao agendamento de mensagens
- ✅ Sistema de modelos híbrido funcional
- ✅ Registros com CorrelationId para evitar duplicidade
- ✅ Integração com mais de 3 IntegrationEvents
- ✅ Priorização de mensagens funcional

---

**Seguimentos Pendentes**:
- [ ] Definir provedores reais (SendGrid, Twilio, Firebase) → **Veja Débito Técnico em docs/technical-debt.md**
- [ ] Interface administrativa para gestão dinâmica de modelos
- [ ] Suporte a anexos via URLs do Armazenamento de Blobs
- [ ] Painel de métricas de entrega e conversão

---

**Resultado Esperado Sprint 9**:
- ✅ MVP pronto para produção e polido
- ✅ Todos os cenários de risco mitigados ou resolvidos
- ✅ Segurança e desempenho endurecidos
- ✅ Documentação completa para usuários e desenvolvedores
- ✅ Monitoramento e observabilidade configurados
- ✅ Módulo de Comunicações implementado (infraestrutura base resiliente)
- 🎯 **PRONTO PARA LANÇAMENTO EM 12-16 DE MAIO DE 2026**

> **⚠️ CRITICAL**: Se a Sprint 9 não for suficiente para completar todos os itens, considerar atraso no lançamento do MVP ou reduzir o escopo (mover recursos não críticos para o pós-MVP). A qualidade e a estabilidade do MVP são mais importantes que a data de lançamento.

---

## 🎯 Fase 3: Qualidade e Monetização

### Objetivo
Introduzir sistema de avaliações para ranking, modelo de assinaturas premium via Stripe, e verificação automatizada de documentos.

### 3.1. 🌟 Módulo Reviews & Ratings (Planejado)

**Objetivo**: Permitir que clientes avaliem prestadores, influenciando o ranking de busca.

#### **Arquitetura Proposta**
- **Padrão**: Arquitetura em camadas simples
- **Agregação**: Cálculo de `AverageRating` via eventos de integração (não em tempo real)

#### **Entidades de Domínio**
```csharp
// Review: Raiz de Agregado
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

// ProviderRating: Agregado (ou parte do modelo de leitura)
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
1. **Esquema**: Criar `meajudaai_reviews` com `reviews`, `provider_ratings`
2. **Ponto de Extremidade de Envio**: Validar que o cliente pode avaliar (serviço contratado?)
3. **Cálculo de Classificação**: Publicar `ReviewAddedIntegrationEvent` → Módulo de busca atualiza `AverageRating`
4. **Moderação**: Sistema de sinalização para avaliações inapropriadas
5. **Testes**: Testes unitários para cálculo de média + testes de integração para envio

---

### 3.2. 💳 Módulo Payments & Billing (Planejado)

**Objetivo**: Gerenciar assinaturas de prestadores via Stripe (Free, Standard, Gold, Platinum).

#### **Arquitetura Proposta**
- **Padrão**: Camada Anti-Corrupção (ACL) sobre a API do Stripe
- **Isolamento**: Lógica de domínio protegida de mudanças no Stripe

#### **Entidades de Domínio**
```csharp
// Subscription: Raiz de Agregado
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

// BillingAttempt: Entidade
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
1. **Configuração do Stripe**: Configurar produtos e planos de preços no painel
2. **Ponto de Extremidade de Webhook**: Receber eventos do Stripe (`checkout.session.completed`, `invoice.payment_succeeded`, `customer.subscription.deleted`)
3. **Handlers de Evento**: Atualizar o status da `Subscription` baseado em eventos
4. **Sessão de Checkout**: Gerar URL de checkout para o frontend
5. **Eventos de Integração**: Publicar `SubscriptionTierChangedIntegrationEvent` → Módulo de busca atualiza o ranking
6. **Testes**: Testes de integração com eventos simulados da biblioteca de testes do Stripe

---

### 3.3. 🤖 Documents - Verificação Automatizada (Planejado - Fase 2)

**Objetivo**: Automatizar a verificação de documentos via OCR e APIs governamentais.

**Funcionalidades Planejadas**:
- **OCR Inteligente**: Azure AI Vision para extrair texto de documentos
- **Validação de Dados**: Verificação cruzada com dados fornecidos pelo prestador
- **Verificações de Antecedentes**: Integração com APIs de antecedentes criminais
- **Pontuação Automática**: Sistema de pontuação baseado na qualidade dos documentos

**Trabalhos em Background**:
1. **DocumentUploadedHandler**: Aciona o processamento de OCR
2. **OcrCompletedHandler**: Valida os campos extraídos
3. **VerificationScheduler**: Agenda verificações periódicas

**Nota**: A infraestrutura básica já existe (campo OcrData, estados de verificação), falta implementar trabalhadores e integrações.

---

### 3.4. 🏷️ Dynamic Service Tags (Planejado - Fase 3)

**Objetivo**: Exibir tags de serviços baseadas na popularidade real por região.

**Funcionalidades**:
- **Ponto de extremidade**: `GET /services/top-region?city=SP` (ou lat/lon)
- **Lógica**: Calcular serviços com maior volume de buscas/contratações na região do usuário.
- **Recuo (Fallback)**: Exibir "Top Globais" se os dados regionais forem insuficientes.
- **Cache**: TTL curto (ex: 1h) para manter a relevância sem comprometer o desempenho.

---

## 🚀 Fase 4: Experiência e Engajamento (Pós-MVP)

### Objetivo
Melhorar a experiência do usuário com agendamentos, comunicações centralizadas e análises avançadas.

### 4.1. 📅 Módulo Service Requests & Booking (Planejado)

**Objetivo**: Permitir que clientes solicitem serviços e agendem horários com prestadores.

#### **Funcionalidades**
- **Solicitação de Serviço**: Cliente descreve necessidade e localização
- **Correspondência (Matching)**: O sistema sugere prestadores compatíveis
- **Agendamento**: Calendário integrado com disponibilidade do prestador
- **Notificações**: Lembretes automáticos via módulo de Comunicações

---

### 4.2. 📊 Módulo Analytics & Reporting (Planejado)

**Objetivo**: Capturar, processar e visualizar dados de negócio e operacionais.

#### **Arquitetura Proposta**
- **Padrão**: CQRS + Event Sourcing (para auditoria)
- **Métricas**: Fachada sobre OpenTelemetry/Aspire
- **Auditoria**: Log de eventos imutável de todas as atividades
- **Relatórios**: Modelos de leitura desnormalizados para consultas rápidas

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

#### **Exibições de Banco de Dados**
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

-- vw_audit_log_enriched: Log de auditoria legível
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
1. **Esquema**: Criar `meajudaai_analytics` com `audit_log`, tabelas de relatórios
2. **Handlers de Evento**: Consumir todos os eventos de integração relevantes
3. **Integração de Métricas**: Expor métricas personalizadas via OpenTelemetry
4. **API de Relatórios**: Pontos de extremidade otimizados para leitura de relatórios
5. **Painéis**: Integração com Aspire Dashboard e Grafana
6. **Testes**: Testes de integração para handlers de evento + testes de desempenho para relatórios

---

## 🎯 Funcionalidades Adicionais Recomendadas (Fase 4+)

### 🛡️ Admin Portal - Módulos Avançados
**Funcionalidades Adicionais (Pós-MVP)**:
- **Widget de Painel de Atividades Recentes**: Feed cronológico de atividades (registros, envios, verificações, mudanças de status) com atualizações em tempo real via SignalR
- **Análises de Usuário e Prestador**: Painéis avançados com Grafana
- **Detecção de Fraude**: Sistema de pontuação para detectar perfis suspeitos
- **Operações em Lote**: Ações em lote (ex: aprovar múltiplos documentos)
- **Trilha de Auditoria**: Histórico completo de todas as ações administrativas

#### 📊 Widget de Atividade Recente (Prioridade: MÉDIA)

**Contexto**: Atualmente, o Painel exibe apenas gráficos estáticos. Um feed de atividades recentes melhoraria a visibilidade operacional.

**Funcionalidades Core**:
- **Linha do Tempo de Eventos**: Feed cronológico de atividades do sistema
- **Tipos de Eventos**:
  - Novos registros de prestadores
  - Envios de documentos
  - Mudanças de status de verificação
  - Ações administrativas (aprovações/rejeições)
  - Adições/remoções de serviços
- **Filtros**: Por tipo de evento, módulo, data
- **Atualizações em Tempo Real**: SignalR para atualização automática
- **Paginação**: Carregar mais atividades sob demanda

**Implementação Técnica**:
```csharp
// Eventos de Domínio → Eventos de Integração → SignalR Hub
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
```

```typescript
// Componente Frontend (React)
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function ActivityTimeline() {
  const [activities, setActivities] = useState<ActivityDto[]>([]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/activity")
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveActivity", (activity: ActivityDto) => {
      setActivities(prev => [activity, ...prev]);
    });

    connection.start();
    return () => { connection.stop(); };
  }, []);

  return (
    <Timeline>
      {activities.map(activity => (
        <TimelineItem key={activity.id} color={getActivityColor(activity.type)}>
          <Typography>{activity.description}</Typography>
          <Typography variant="caption">{formatRelative(activity.timestamp)}</Typography>
        </TimelineItem>
      ))}
    </Timeline>
  );
}
```

**Estimativa**: 3-5 dias (1 dia para eventos de backend, 1 dia para SignalR, 2-3 dias para frontend)

**Dependências**:
- SignalR configurado no backend
- Barramento de eventos consumindo eventos de domínio
- Contrato ActivityDto definido

---

### 👤 Gestão de Perfil do Cliente (Alta Prioridade)
**Por quê**: O plano atual é muito focado em prestadores; os clientes também precisam de gestão de perfil.

**Funcionalidades Core**:
- Editar informações básicas (nome, foto)
- Ver histórico de prestadores contatados
- Gerenciar avaliações escritas
- Preferências de notificações

**Implementação**: Melhoria (Enhancement) no módulo Users existente

---

### ⚖️ Sistema de Resolução de Disputas (Média Prioridade)
**Por quê**: Mesmo sem pagamentos no aplicativo, podem ocorrer disputas (avaliações injustas, má conduta).

**Funcionalidades Core**:
- Botão "Reportar" em perfis de prestadores e avaliações
- Formulário para descrever o problema
- Fila no Portal Administrativo para moderadores

**Implementação**: Novo módulo pequeno ou extensão do módulo de Avaliações

---

## 📊 Métricas de Sucesso

### 📈 Métricas de Produto
- **Crescimento de usuários**: 20% ao mês
- **Retenção de prestadores**: 85%
- **Satisfação média**: 4.5+ estrelas
- **Taxa de conversão (Grátis → Pago)**: 15%

### ⚡ Métricas Técnicas (SLOs)

#### **Metas de Desempenho em Camadas**

| Categoria | Tempo Alvo | Exemplo |
|-----------|------------|---------|
| **Consultas Simples** | <200ms | Busca por ID, dados em cache |
| **Consultas Médias** | <500ms | Listagens com filtros básicos |
| **Consultas Complexas** | <1000ms | Busca entre módulos, agregações |
| **Consultas Analíticas** | <3000ms | Relatórios, painéis |

#### **Linha de Base de Desempenho**
- **Assumindo**: Cache distribuído configurado, índices otimizados
- **Revisão Trimestral**: Ajustes baseados em métricas reais
  - **Percentis monitorados**: P50, P95, P99 (latência de consultas)
  - **Frequência**: Análise e ajuste a cada 3 meses
  - **Processo**: Ciclo de feedback → identificar outliers → otimizar consultas lentas
- **Monitoramento**: OpenTelemetry + Aspire Dashboard + Application Insights

#### **Outros SLOs**
- **Disponibilidade**: 99.9% de tempo de atividade (uptime)
- **Segurança**: Zero vulnerabilidades críticas
- **Cobertura de Testes**: >80% para código crítico

---

## 🔄 Processo de Gestão do Roadmap

### 📅 Revisão Trimestral
- Avaliação de progresso em relação aos marcos (milestones)
- Ajuste de prioridades baseado em métricas
- Análise de feedback de usuários e prestadores

### 💬 Feedback Contínuo
- **Contribuição da comunidade**: Pesquisas, suporte, análises
- **Feedback de prestadores**: Portal dedicado para sugestões
- **Necessidades de negócio**: Alinhamento com as partes interessadas (stakeholders)

### 🎯 Critérios de Priorização
1. **Impacto no MVP**: A funcionalidade é crítica para o lançamento?
2. **Esforço de Implementação**: Complexidade técnica e tempo estimado
3. **Dependências**: Quais módulos dependem desta funcionalidade?
4. **Valor para o Usuário**: Feedback qualitativo e quantitativo

---

## 📋 Sumário Executivo de Prioridades

### ✅ **Concluído (Set-Dez 2025)**
1. ✅ Sprint 0: Migração .NET 10 + Aspire 13 (21 Nov 2025 - MERGE para master)
2. ✅ Sprint 1: Restrição Geográfica + Integração de Módulos (2 Dez 2025 - MERGE para master)
3. ✅ Sprint 2: Cobertura de Testes de 90.56% (10 Dez 2025) - Meta de 35% SUPERADA em 55.56pp!
4. ✅ Sprint 5.5: Correção de Arquivos de Lock de Pacote (19 Dez 2025)
   - Correção de conflitos Microsoft.OpenApi (2.3.12 → 2.3.0)
   - 37 arquivos packages.lock.json regenerados
   - PRs #81 e #82 atualizados e aguardando merge
5. ✅ Módulo Users (Concluído)
6. ✅ Módulo Providers (Concluído)
7. ✅ Módulo Documents (Concluído)
8. ✅ Módulo Search & Discovery (Concluído)
9. ✅ Módulo Locations - Busca de CEP e geocodificação (Concluído)
10. ✅ Módulo ServiceCatalogs - Catálogo gerenciado por admin (Concluído)
11. ✅ CI/CD - Fluxos de trabalho do GitHub Actions (.NET 10 + Aspire 13)
12. ✅ Ramo (branch) feature/refactor-and-cleanup - Mesclado para master (19 Dez 2025)

### 📅 Alta Prioridade (Próximos 3 meses - Q1-Q2 2026)
1. ✅ **Sprint 8B.2: NX Monorepo e Excelência Técnica** (Concluída)
2. ✅ **Sprint 8C: App Web do Prestador (React + NX)** (Concluída - 21 Mar 2026)
3. ✅ **Sprint 8D: Migração do Portal Administrativo** (Concluída - 24 Mar 2026)
4. ✅ **Sprint 8E: Testes E2E Apps React (Playwright)** (Concluída - 25 Mar 2026)
5. ⏳ **Sprint 9: BUFFER E MITIGAÇÃO DE RISCO** (25 Mar - 11 Abr 2026)
6. 🎯 **MVP Final Launch: 12 - 16 de Maio de 2026**
7. 📋 Coleções de API - Arquivos .bru do Bruno para todos os módulos

### 🎯 **Alta Prioridade - Pré-MVP**
1. 🎯 Communications - Notificações por e-mail
2. 💳 Módulo Payments & Billing (Stripe) - Preparação para monetização

### 🎯 **Média Prioridade (6-12 meses - Fase 2)**
1. 🎉 Módulo Reviews & Ratings
2. 🌍 Documents - Verificação automatizada (OCR + Verificações de antecedentes)
3. 🔄 Search - Trabalhador de indexação para eventos de integração (extensão do módulo SearchProviders)
4. 📊 Analytics - Métricas básicas
5. 🏛️ Sistema de Resolução de Disputas
6. 🔥 Alinhamento de middleware entre UseSharedServices() e UseSharedServicesAsync()

### 🔬 **Testes E2E Frontend (Pós-MVP)**
**Projeto**: `src/Web` (dividido por projeto)
**Estrutura**: Uma pasta para cada projeto frontend
- `src/Web/MeAjudaAi.Web.Customer/e2e/` - Testes E2E para App Web do Cliente
- `src/Web/MeAjudaAi.Web.Provider/e2e/` - Testes E2E para App Web do Prestador  
- `src/Web/MeAjudaAi.Web.Admin/e2e/` - Testes E2E para Portal Administrativo

**Framework**: Playwright
**Cenários a cobrir**:
- [ ] Autenticação (login, logout, refresh token)
- [ ] Fluxo de onboarding (Cliente e Prestador)
- [ ] CRUD de prestadores e serviços
- [ ] Busca e filtros
- [ ] Responsividade mobile
- [ ] Desempenho e Core Web Vitals
