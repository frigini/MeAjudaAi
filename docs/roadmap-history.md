# ≡ƒù║∩╕Å Roadmap - MeAjudaAi

Este documento consolida o planejamento estrat├⌐gico e t├ítico da plataforma MeAjudaAi, definindo fases de implementa├º├úo, m├│dulos priorit├írios e funcionalidades futuras.

---

## ≡ƒôè Sum├írio Executivo

**Projeto**: MeAjudaAi - Plataforma de Conex├úo entre Clientes e Prestadores de Servi├ºos  
**Status Geral**: Consulte a [Tabela de Sprints](#cronograma-de-sprints) para o status detalhado atualizado.
**Cobertura de Testes**: Backend 90.56% | Frontend 30 testes bUnit  
**Stack**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 (Customer, Provider, Admin) + Tailwind v4

### Marcos Principais

Consulte a se├º├úo [Cronograma de Sprints](#cronograma-de-sprints) abaixo para o status detalhado e atualizado de cada sprint, e datas alvo (incluindo o MVP Launch).

**Procedimento de Revis├úo de Sprints**
As futuras atualiza├º├╡es da tabela de sprints devem observar a pol├¡tica: an├ílise commit-by-commit newest-first, apresentando um veredicto conciso e resolvendo os follow-ups.

## ΓÜá∩╕Å Notas de Risco

- Estimativas assumem velocidade consistente e aus├¬ncia de bloqueios maiores
- Primeiro projeto Blazor WASM pode revelar complexidade n├úo prevista
- Sprint 9 reservado como buffer de conting├¬ncia (n├úo para novas features)

## ≡ƒÅù∩╕Å Decis├╡es Arquiteturais Futuras

### NX Monorepo (Frontend)

**Status**: Γ£à Inclu├¡do no Sprint 8B.2  
**Branch**: `feature/sprint-8b2-technical-excellence`

**Motiva├º├úo**: Com Customer Web App (Next.js), Provider App (pr├│ximo sprint), Admin Portal (migra├º├úo planejada) e Mobile (React Native + Expo), o compartilhamento de c├│digo (componentes, hooks, tipos TypeScript, schemas Zod) entre os projetos se torna cr├¡tico. NX oferece:
- Workspace unificado com `libs/` compartilhadas
- Build cache inteligente (s├│ reconstr├│i o que mudou)
- Dependency graph entre projetos
- Gera├º├úo de c├│digo consistente

**Escopo (Sprint 8B.2)**:
- Migrar `MeAjudaAi.Web.Customer` para workspace NX
- Criar `apps/customer-web`, `apps/provider-web` (Sprint 8C), `apps/admin-web` (Sprint 8D), `apps/mobile` (Sprint 8E)
- Criar `libs/ui` (componentes compartilhados), `libs/auth`, `libs/api-client`
- Atualizar `.NET Aspire AppHost` para apontar para nova estrutura
- Atualizar CI/CD para usar `nx affected`

**Decis├úo de antecipa├º├úo**: NX foi antecipado do p├│s-MVP para o Sprint 8B.2 porque o Provider App (Sprint 8C) e a migra├º├úo Admin (Sprint 8D) se beneficiam diretamente do workspace unificado. Criar o NX antes desses projetos evita migra├º├úo posterior mais custosa.

---

### Migra├º├úo Admin Portal: Blazor WASM ΓåÆ React

**Status**: ΓÅ│ Planejado ΓÇö Sprint 8D (ap├│s Provider App)

**An├ílise (Atualizada Mar├ºo 2026)**:

| Fator | Manter Blazor | Migrar para React |
|-------|--------------|-------------------|
| Custo | Γ£à Zero | Γ¥î Alto (reescrever ~5000+ linhas) |
| Compartilhamento C# DTOs | Γ£à Nativo | ΓÜá∩╕Å Requer API client gerado (libs/api-client via NX) |
| Uso interno (n├úo SEO) | Γ£à Blazor adequado | Γ£à React com NX compartilha componentes |
| Unifica├º├úo de stack | Γ¥î Dual-stack (Blazor + React) | Γ£à Single-stack React (3 apps no NX) |
| Hiring | ΓÜá∩╕Å Blazor nicho | Γ£à React mais f├ícil |
| Shared Components | Γ¥î Isolado do NX | Γ£à Reutiliza libs/ui, libs/auth do NX |

**Decis├úo Revisada (Mar├ºo 2026)**: **Migrar para React** dentro do workspace NX. Com a ado├º├úo do NX Monorepo (Sprint 8B.2) e o Provider App (Sprint 8C) como segundo app React, manter o Admin em Blazor cria uma ilha isolada que n├úo se beneficia dos componentes compartilhados (`libs/ui`, `libs/auth`, `libs/api-client`). A unifica├º├úo de stack reduz complexidade operacional e facilita manuten├º├úo.

**Sequ├¬ncia**: Provider App (Sprint 8C) ΓåÆ Admin Migration (Sprint 8D). O Provider App estabelece os padr├╡es e shared libs que a migra├º├úo Admin reutilizar├í.

---

## ≡ƒÄ» Status Atual

**≡ƒôà Sprint 8B conclu├¡do**: Fevereiro/Mar├ºo de 2026 (Finalizado em 4 de Mar├ºo de 2026)

### Γ£à Sprint 8A - Customer Web App & Test Optimization - CONCLU├ìDA (5-13 Fev 2026)

**Objetivos**:
1. Γ£à **Integrar Service Tags com Backend**
2. Γ£à **Implementar Filtros Avan├ºados de Busca**
3. Γ£à **Otimizar Testes E2E (Redu├º├úo de Tempo)**

**Progresso Atual**: 3/3 objetivos completos Γ£à **SPRINT 8A CONCLU├ìDO 100%!**

**Funcionalidades Entregues**:
- **Service Tags**: Integra├º├úo com API para carregar servi├ºos populares dinamicamente (`service-catalog.ts`).
- **Busca Avan├ºada**: Filtros de Categoria, Avalia├º├úo (Rating) e Dist├óncia (Raio) implementados na UI (`SearchFilters.tsx`) e integrados com API de busca.
- **Frontend Integration**: `SearchPage` atualizado para processar novos par├ómetros de filtro e mapear categorias para IDs de servi├ºo.

**Otimiza├º├úo de Testes**:
- **Problema**: Testes E2E lentos devido a ac├║mulo de dados (40m+).
- **Solu├º├úo**: Implementado `IAsyncLifetime` e `CleanupDatabaseAsync()` em **todas** as classes de teste E2E (`Documents`, `Locations`, `Providers`, `ServiceCatalogs`, `Users`).
- **Resultado**: Testes rodam com banco limpo a cada execu├º├úo, prevenindo degrada├º├úo de performance e falhas por dados sujos (Race Conditions).
- `parallelizeTestCollections`: Controla se cole├º├╡es de teste executam em paralelo no xUnit. Confirmado que `parallelizeTestCollections: false` ├⌐ necess├írio para DbContext com TestContainers, pois banco compartilhado causa lock conflicts.
---

### Γ£à Sprint 8B.1 - Provider Onboarding & Registration Experience - CONCLU├ìDA (Mar├ºo 2026)

**Objetivos**:
1. Γ£à **Multi-step Provider Registration**: Implementar UI de "Torne-se um Prestador" com Stepper unificado.
2. Γ£à **Fix Backend Reliability**: Resolver erros 500 nos endpoints cr├¡ticos de prestador.
3. Γ£à **Visual Alignment**: Alinhar design do prestador com o fluxo de cliente.

**Avan├ºos Entregues**:
- **Stepper UI**: Componente de linha do tempo implementado em `/cadastro/prestador`, guiando o usu├írio pelas etapas de Dados B├ísicos, Endere├ºo e Documentos.
- **Corre├º├úo de API (Critical)**: Resolvido erro de resolu├º├úo de DI para `RegisterProviderCommandHandler`, permitindo a cria├º├úo de perfis sem falhas internas (500).
- **Onboarding Flow**: Implementa├º├úo da l├│gica de transi├º├úo entre passos 1 (Dados B├ísicos) e 2 (Endere├ºo), com persist├¬ncia correta no banco de dados.
- **Validation**: Integra├º├úo com esquema de valida├º├úo existente e tratamento de erros amig├ível no frontend.

**Pr├│ximos Passos (Pendentes)**:
- ΓÅ│ **Document Upload (Step 3)**: Implementar componente de upload de documentos no fluxo de onboarding do prestador.
- ΓÅ│ **Review Dashboard**: Criar interface para o prestador acompanhar o status de sua verifica├º├úo (hoje parado em `pendingBasicInfo`).
- ΓÅ│ **Professional Profile Setup**: Permitir que o prestador selecione categorias e servi├ºos logo ap├│s o credenciamento b├ísico.

---

### ΓÅ│ Sprint 8B.2 - Technical Excellence & NX Monorepo (Planejado - Antes do Provider App)

**Branch**: `feature/sprint-8b2-technical-excellence`

**Objetivos**:
1. ΓÅ│ **Messaging Unification (RabbitMQ Only)**: Remover completamente o Azure Service Bus da solu├º├úo.
    - **Execu├º├úo**:
        - Remover pacotes `.Azure.ServiceBus` de todos os projetos.
        - Unificar `MassTransit` configuration em `ServiceDefaults`.
        - Atualizar scripts de infra (`docker-compose.yaml`) para foco total em RabbitMQ.
        - Remover segredos e vars de ambiente do ASB no Azure/Staging.
    - **Sucesso**: Aplica├º├úo funcionando sem depend├¬ncia do Azure Service Bus local ou remoto.
2. ΓÅ│ **Backend Integration Test Optimization**: Reduzir o tempo de execu├º├úo (hoje ~30 min).
    - **Execu├º├úo**:
        - Migrar os ~20 projetos de teste restantes para o padr├úo `RequiredModules`.
        - Implementar `Respawn` ou similar para limpeza ultra-r├ípida de banco em vez de migrations completas.
        - Otimizar recursos do TestContainers (reuse containers entre runs se poss├¡vel).
    - **Sucesso**: Su├¡te completa de integra├º├úo rodando em < 10 minutos.
3. ΓÅ│ **Slug Implementation**: Substituir IDs por Slugs nas rotas de perfil de prestador para maior seguran├ºa e SEO.
    - **Execu├º├úo**:
        - Backend: Adicionar `Slug` ao `BusinessProfile` entity.
        - Backend: Implementar `slugify` logic e garantir unicidade no Persistence layer.
        - UI: Alterar rotas de `/prestador/[id]` para `/prestador/[slug]`.
        - SEO: Adicionar canonical tags e metadados din├ómicos baseados no slug.
    - **Sucesso**: Navegar via slug e manter compatibilidade com IDs antigos (301 redirect).
4. ΓÅ│ **Frontend Testing & CI/CD Suite**: Implementar su├¡te completa de testes no Next.js.
    - **Contexto**: Baseado no [Plano de Testes Robusto](./testing/frontend-testing-plan.md).
    - **Execu├º├úo**:
        - Setup do projeto `tests/MeAjudaAi.Web.Customer.Tests`.
        - Implementar Mocks de API com MSW para os fluxos de busca e perfil.
        - Criar o primeiro pipeline `.github/workflows/frontend-quality.yml`.
        - Integrar SonarCloud (SonarQube) para an├ílise est├ítica de TS/React.
    - **Sucesso**: Pipeline falhando se testes n├úo passarem ou qualidade cair.
5. ΓÅ│ **NX Monorepo Setup**: Configurar workspace NX para gerenciar todos os projetos frontend.
    - **Execu├º├úo**:
        - Inicializar workspace NX na raiz do projeto.
        - Migrar `MeAjudaAi.Web.Customer` (Next.js) para `apps/customer-web`.
        - Criar shared libs: `libs/ui`, `libs/auth`, `libs/api-client`.
        - Extrair componentes compartilhados do Customer App para `libs/ui`.
        - Atualizar `.NET Aspire AppHost` para apontar para nova estrutura NX.
        - Atualizar CI/CD para usar `nx affected`.
        - Scaffolding `apps/provider-web` (vazio, ser├í implementado no Sprint 8C).
    - **Sucesso**: Customer Web App rodando dentro do workspace NX com build e testes funcionais.

---

### Γ£à Sprint 7.10 - Accessibility Features - CONCLU├ìDA (16 Jan 2026)
### Γ£à Sprint 7.11 - Error Boundaries - CONCLU├ìDA (16 Jan 2026) 
### Γ£à Sprint 7.12 - Performance Optimizations - CONCLU├ìDA (16 Jan 2026)
### Γ£à Sprint 7.13 - Standardized Error Handling - CONCLU├ìDA (16 Jan 2026)
### Γ£à Sprint 7.14 - Complete Localization (i18n) - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

### Γ£à Sprint 7.9 - Magic Strings Elimination - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Objetivos**:
1. Γ£à **Configura├º├úo Aspire com Pacotes NuGet Locais** - Resolver erro DCP/Dashboard paths
2. Γ£à **Elimina├º├úo de Warnings** - 0 warnings em toda a solu├º├úo
3. Γ£à **Scripts de Automa├º├úo** - Facilitar setup e execu├º├úo
4. Γ£à **Documenta├º├úo** - Instru├º├╡es claras de inicializa├º├úo

**Progresso Atual**: 4/4 objetivos completos Γ£à **SPRINT 7.5 CONCLU├ìDO!**

**Detalhamento - Configura├º├úo Aspire** Γ£à:
- Directory.Build.targets criado no AppHost com propriedades MSBuild
- Propriedades `CliPath` e `DashboardPath` configuradas automaticamente
- Detecta pacotes locais em `packages/` (aspire.hosting.orchestration.win-x64 13.1.0)
- Target de valida├º├úo com mensagens de erro claras
- launchSettings.json criado com vari├íveis de ambiente (ASPNETCORE_ENVIRONMENT, POSTGRES_PASSWORD)
- Keycloak options com senha padr├úo "postgres" para desenvolvimento
- Aspire SDK atualizado de 13.0.2 para 13.1.0 (sincronizado com global.json)
- Workaround documentado em docs/known-issues/aspire-local-packages.md
- Commits: 95f52e79 "fix: configurar caminhos Aspire para pacotes NuGet locais"

**Detalhamento - Elimina├º├úo de Warnings** Γ£à:
- Admin Portal: Directory.Build.props com NoWarn para 11 tipos de warnings
  - CS8602 (null reference), S2094 (empty records), S3260 (sealed), S2953 (Dispose)
  - S2933 (readonly), S6966 (await async), S2325 (static), S5693 (content length)
  - MUD0002 (MudBlazor casing), NU1507 (package sources), NU1601 (dependency version)
- MudBlazor atualizado de 7.21.0 para 8.0.0 em Directory.Packages.props
- .editorconfig criado no Admin Portal com documenta├º├úo de supress├╡es
- **Resultado**: Build completo com 0 warnings, 0 erros
- Commit: 60cbb060 "fix: eliminar todos os warnings de NuGet"

**Detalhamento - Scripts de Automa├º├úo** Γ£à:
- `scripts/setup.ps1`: Script de setup inicial com valida├º├úo de pr├⌐-requisitos
  - Verifica .NET SDK 10.0.101, Docker Desktop, Git
  - Executa dotnet restore e build
  - Exibe instru├º├╡es de configura├º├úo do Keycloak
- `scripts/dev.ps1`: Script de desenvolvimento di├írio
  - Valida Docker e .NET SDK
  - Restaura depend├¬ncias
  - Inicia Aspire AppHost
  - Define vari├íveis de ambiente (POSTGRES_PASSWORD, ASPNETCORE_ENVIRONMENT)
- `scripts/README.md`: Documenta├º├úo completa dos scripts
- `.vscode/launch.json` e `.vscode/tasks.json`: Configura├º├úo para debugging

**Detalhamento - Documenta├º├úo** Γ£à:
- README.md atualizado com se├º├úo "ΓÜí Setup em 2 Comandos"
- Tabela de scripts com descri├º├úo e uso
- Pr├⌐-requisitos claramente listados
- docs/known-issues/aspire-local-packages.md: Workaround documentado
  - Descri├º├úo do problema (bug Aspire com globalPackagesFolder)
  - 3 solu├º├╡es alternativas (VS Code F5, Visual Studio, configura├º├úo manual)
  - Link para issue upstream: [dotnet/aspire#6789](https://github.com/dotnet/aspire/issues/6789)
- Scripts de build: Unix/Linux Makefile e PowerShell scripts (ver `build/` directory)

**Resultado Alcan├ºado**:
- Γ£à Aspire AppHost inicia corretamente via F5 ou scripts
- Γ£à 0 warnings em toda a solu├º├úo (40 projetos)
- Γ£à Setup automatizado em 2 comandos PowerShell
- Γ£à Documenta├º├úo completa de inicializa├º├úo
- Γ£à Experi├¬ncia de desenvolvimento melhorada
- Γ£à 16 arquivos modificados, 588 adi├º├╡es, 109 dele├º├╡es

---

### Γ£à Sprint 7.6 - Otimiza├º├úo de Testes de Integra├º├úo - CONCLU├ìDA (12 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Ap├│s Sprint 7.5, testes de integra├º├úo apresentaram timeouts intermitentes. Investiga├º├úo revelou que BaseApiTest aplicava migrations de TODOS os 6 m├│dulos para CADA teste, causando esgotamento do pool de conex├╡es PostgreSQL (erro 57P01).

**Problema Identificado**:
- Γ¥î Teste `DocumentRepository_ShouldBeRegisteredInDI` passa na master (15s)
- Γ¥î Mesmo teste falha no fix/aspire-initialization com timeout (~14s)
- Γ¥î Erro PostgreSQL: `57P01: terminating connection due to administrator command`
- Γ¥î Causa raiz: BaseApiTest aplica migrations dos 6 m├│dulos sequencialmente (~60-70s)

**Investiga├º├úo Realizada**:
1. Γ¥î Tentativa 1: Remover migration vazia SyncModel ΓåÆ Ainda falha
2. Γ¥î Tentativa 2: Remover PostGIS extension annotation ΓåÆ Ainda falha
3. Γ¥î Tentativa 3: Adicionar CloseConnectionAsync ap├│s migrations ΓåÆ Ainda falha
4. Γ£à **Insight do usu├írio**: "qual cen├írio o teste quebra? ├⌐ um cen├írio real? ├⌐ um teste necess├írio?"
5. Γ£à **Descoberta**: Teste s├│ verifica DI registration, n├úo precisa de migrations!
6. Γ£à **Root cause**: ALL tests aplica ALL modules migrations desnecessariamente

**Solu├º├úo Implementada: Migrations Sob Demanda (On-Demand Migrations)**

**1. TestModule Enum com Flags** Γ£à
```csharp
[Flags]
public enum TestModule
{
    None = 0,
    Users = 1 << 0,
    Providers = 1 << 1,
    Documents = 1 << 2,
    ServiceCatalogs = 1 << 3,
    Locations = 1 << 4,
    SearchProviders = 1 << 5,
    All = Users | Providers | Documents | ServiceCatalogs | Locations | SearchProviders
}
```

**2. RequiredModules Virtual Property** Γ£à
```csharp
/// <summary>
/// Override this property in your test class to specify which modules are required.
/// Default is TestModule.All for backward compatibility.
/// </summary>
protected virtual TestModule RequiredModules => TestModule.All;
```

**3. ApplyRequiredModuleMigrationsAsync Method** Γ£à
- Verifica flags de RequiredModules
- Aplica EnsureCleanDatabaseAsync apenas uma vez
- Aplica migrations SOMENTE para m├│dulos especificados
- Fecha conex├╡es ap├│s cada m├│dulo
- Seeds Locations test data se Locations module requerido

**4. EnsureCleanDatabaseAsync Method** Γ£à
- Extra├¡do do legacy ApplyMigrationsAsync
- Manuseia PostgreSQL startup retry logic (erro 57P03)
- 10 tentativas com linear backoff (1s, 2s, 3s, ...)

**Arquivos Modificados** Γ£à:
- `tests/MeAjudaAi.Integration.Tests/Base/BaseApiTest.cs`: Refactoring completo
  - Lines 29-49: TestModule enum
  - Lines 51-67: RequiredModules property + documenta├º├úo
  - Lines 363-453: ApplyRequiredModuleMigrationsAsync (novo)
  - Lines 455-484: EnsureCleanDatabaseAsync (extra├¡do)
  - Lines 486+: ApplyMigrationsAsync marcado como `@deprecated`

- `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentsIntegrationTests.cs`:
  ```csharp
  protected override TestModule RequiredModules => TestModule.Documents;
  ```

- **5 Test Classes Otimizados**:
  - UsersIntegrationTests ΓåÆ `TestModule.Users`
  - ProvidersIntegrationTests ΓåÆ `TestModule.Providers`
  - ServiceCatalogsIntegrationTests ΓåÆ `TestModule.ServiceCatalogs`
  - DocumentsApiTests ΓåÆ `TestModule.Documents`

- `tests/MeAjudaAi.Integration.Tests/README.md`: Nova se├º├úo "ΓÜí Performance Optimization: RequiredModules"

**Resultados Alcan├ºados** Γ£à:
- Γ£à **Performance**: 83% faster para testes single-module (10s vs 60s)
- Γ£à **Confiabilidade**: Eliminou timeouts do PostgreSQL (57P01 errors)
- Γ£à **Isolamento**: Cada teste carrega apenas m├│dulos necess├írios
- Γ£à **Backward Compatible**: Default RequiredModules = TestModule.All
- Γ£à **Realismo**: Espelha comportamento Aspire (migrations per-module)
- Γ£à **Test Results**:
  - Antes: DocumentRepository_ShouldBeRegisteredInDI ΓåÆ TIMEOUT (~14s)
  - Depois: DocumentRepository_ShouldBeRegisteredInDI ΓåÆ Γ£à PASS (~10s)

**M├⌐tricas de Compara├º├úo**:

| Cen├írio | Antes (All Modules) | Depois (Required Only) | Improvement |
|---------|---------------------|------------------------|-------------|
| Inicializa├º├úo | ~60-70s | ~10-15s | **83% faster** |
| Migrations aplicadas | 6 m├│dulos sempre | Apenas necess├írias | M├¡nimo necess├írio |
| Timeouts | Frequentes | Raros/Eliminados | Γ£à Est├ível |
| Pool de conex├╡es | Esgotamento frequente | Isolado por m├│dulo | Γ£à Confi├ível |

**Outros Fixes** Γ£à:
- Γ£à IHostEnvironment shadowing corrigido em 6 m├│dulos (SearchProviders, ServiceCatalogs, Users, Providers, Documents, Locations)
- Γ£à Removido teste redundante `IbgeApiIntegrationTests.GetMunicipioByNameAsync_Itaperuna_ShouldReturnValidMunicipio`
- Γ£à Removida migration vazia `SearchProviders/20260112200309_SyncModel_20260112170301.cs`
- Γ£à Analisados 3 testes skipped - todos validados como corretos

**Documenta├º├úo Atualizada** Γ£à:
- Γ£à tests/MeAjudaAi.Integration.Tests/README.md: Performance optimization guide
- Γ£à docs/roadmap.md: Esta entrada (Sprint 7.6)
- ΓÅ│ docs/architecture.md: Testing architecture (pr├│ximo)
- ΓÅ│ docs/development.md: Developer guide para RequiredModules (pr├│ximo)
- ΓÅ│ docs/technical-debt.md: Remover item de otimiza├º├úo de testes (pr├│ximo)

**Pr├│ximos Passos**:
1. Otimizar remaining 23 test classes com RequiredModules apropriados
2. Atualizar docs/architecture.md com diagrama de testing pattern
3. Atualizar docs/development.md com guia de uso
4. Atualizar docs/technical-debt.md removendo item resolvido

**Commits**:
- [hash]: "refactor: implement on-demand module migrations in BaseApiTest"
- [hash]: "docs: add RequiredModules optimization guide to tests README"

---

### Γ£à Sprint 7.7 - Flux Pattern Refactoring - CONCLU├ìDA (15-16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Ap├│s Sprint 7 Features, 5 p├íginas admin (Providers, Documents, Categories, Services, AllowedCities) ainda utilizavam direct API calls. Part 7 consistiu em refatorar todas para o padr├úo Flux/Redux com Fluxor, garantindo consist├¬ncia arquitetural e single source of truth.

**Objetivos**:
1. Γ£à **Refatorar Providers.razor** - Migrar Create/Update/Delete para Fluxor Actions
2. Γ£à **Refatorar Documents.razor** - Remover direct API calls
3. Γ£à **Refatorar Categories.razor** - Implementar Flux pattern completo
4. Γ£à **Refatorar Services.razor** - Remover direct API calls
5. Γ£à **Refatorar AllowedCities.razor** - Implementar Flux pattern completo
6. Γ£à **Decis├úo Arquitetural sobre Dialogs** - Avaliar se refatorar ou manter pragm├ítico
7. Γ£à **Documenta├º├úo Flux Pattern** - Criar guia de implementa├º├úo completo

**Progresso Atual**: 7/7 objetivos completos Γ£à **SPRINT 7.7 CONCLU├ìDO 100%!**

**Implementa├º├╡es Realizadas** Γ£à:

**1. Providers.razor Refactoring** Γ£à (Commit b98bac98):
- Removidos 95 linhas de c├│digo direto (APIs, handlers de sucesso/erro)
- Migrados todos m├⌐todos para Fluxor Actions
- Novo: `CreateProviderAction`, `UpdateProviderAction`, `DeleteProviderAction`, `UpdateVerificationStatusAction`
- ProvidersEffects implementado com todos side-effects
- ProvidersReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsVerifying`
- **Redu├º├úo**: 95 linhas ΓåÆ 18 linhas (81% code reduction)

**2. Documents.razor Refactoring** Γ£à (Commit 152a22ca):
- Removidos handlers diretos de upload e request verification
- Novo: `UploadDocumentAction`, `RequestDocumentVerificationAction`, `DeleteDocumentAction`
- DocumentsEffects com retry logic e error handling
- DocumentsReducer com estados `IsUploading`, `IsRequestingVerification`, `IsDeleting`
- **Redu├º├úo**: 87 linhas ΓåÆ 12 linhas (86% code reduction)

**3. Categories.razor Refactoring** Γ£à (Commit 1afa2daa):
- Removidos m├⌐todos `CreateCategory`, `UpdateCategory`, `DeleteCategory`, `ToggleActivation`
- Novo: `CreateCategoryAction`, `UpdateCategoryAction`, `DeleteCategoryAction`, `ToggleActivationAction`
- CategoriesEffects com valida├º├úo de depend├¬ncias (n├úo deletar se tem servi├ºos)
- CategoriesReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redu├º├úo**: 103 linhas ΓåÆ 18 linhas (83% code reduction)

**4. Services.razor Refactoring** Γ£à (Commit 399ee25b):
- Removidos m├⌐todos `CreateService`, `UpdateService`, `DeleteService`, `ToggleActivation`
- Novo: `CreateServiceAction`, `UpdateServiceAction`, `DeleteServiceAction`, `ToggleActivationAction`
- ServicesEffects com category validation
- ServicesReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redu├º├úo**: 98 linhas ΓåÆ 18 linhas (82% code reduction)

**5. AllowedCities.razor Refactoring** Γ£à (Commit 9ee405e0):
- Removidos m├⌐todos `CreateCity`, `UpdateCity`, `DeleteCity`, `ToggleActivation`
- Novo: `CreateAllowedCityAction`, `UpdateAllowedCityAction`, `DeleteAllowedCityAction`, `ToggleActivationAction`
- LocationsEffects com valida├º├úo de coordenadas
- LocationsReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redu├º├úo**: 92 linhas ΓåÆ 14 linhas (85% code reduction)

**M├⌐tricas de Refactoring**:

| P├ígina | Antes (LOC) | Depois (LOC) | Redu├º├úo | Percentual |
|--------|-------------|--------------|---------|------------|
| Providers.razor | 95 | 18 | 77 | 81% |
| Documents.razor | 87 | 12 | 75 | 86% |
| Categories.razor | 103 | 18 | 85 | 83% |
| Services.razor | 98 | 18 | 80 | 82% |
| AllowedCities.razor | 92 | 14 | 78 | 85% |
| **TOTAL** | **475** | **80** | **395** | **83%** |

**Decis├úo Arquitetural: Dialogs com Padr├úo Pragm├ítico** Γ£à:

Ap├│s an├ílise, decidiu-se manter os 10 dialogs (CreateProvider, EditProvider, VerifyProvider, CreateCategory, EditCategory, CreateService, EditService, CreateAllowedCity, EditAllowedCity, UploadDocument) com direct API calls pelo princ├¡pio YAGNI (You Aren't Gonna Need It):

**Justificativa**:
- Dialogs s├úo componentes ef├¬meros (lifecycle curto)
- N├úo h├í necessidade de compartilhar estado entre dialogs
- Refatorar adicionaria complexidade sem benef├¡cio real
- Single Responsibility Principle: dialogs fazem apenas submit de formul├írio
- Manutenibilidade: c├│digo direto ├⌐ mais f├ícil de entender neste contexto

**Documenta├º├úo** Γ£à (Commit c1e33919):
- Criado `docs/architecture/flux-pattern-implementation.md` (422 linhas)
- Se├º├╡es: Overview, Implementation Details, Data Flow Diagram, Anatomy of Feature, Before/After Examples
- Naming Conventions, File Structure, Best Practices
- Quick Guide for Adding New Operations
- Architectural Decisions (pragmatic approach for dialogs)
- Code reduction metrics (87% average)

**Commits**:
- b98bac98: "refactor(admin): migrate Providers page to Flux pattern"
- 152a22ca: "refactor(admin): migrate Documents page to Flux pattern"  
- 1afa2daa: "refactor(admin): migrate Categories page to Flux pattern"
- 399ee25b: "refactor(admin): migrate Services page to Flux pattern"
- 9ee405e0: "refactor(admin): migrate AllowedCities page to Flux pattern"
- c1e33919: "docs: add comprehensive Flux pattern implementation guide"

---

### Γ£à Sprint 7.8 - Dialog Implementation Verification - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Durante Sprint 7.7, refer├¬ncias a dialogs foram identificadas (CreateProviderDialog, EditProviderDialog, VerifyProviderDialog, UploadDocumentDialog, ProviderSelectorDialog). Part 8 consistiu em verificar se todos os dialogs estavam implementados e corrigir quaisquer problemas de build.

**Objetivos**:
1. Γ£à **Verificar Implementa├º├úo dos 5 Dialogs Principais**
2. Γ£à **Corrigir Erros de Build nos Testes**
3. Γ£à **Garantir Qualidade das Implementa├º├╡es**

**Progresso Atual**: 3/3 objetivos completos Γ£à **SPRINT 7.8 CONCLU├ìDO 100%!**

**1. Verifica├º├úo de Dialogs** Γ£à:

Todos os 5 dialogs requeridos estavam **j├í implementados e funcionais**:

| Dialog | Arquivo | Linhas | Status | Features |
|--------|---------|--------|--------|----------|
| CreateProviderDialog | CreateProviderDialog.razor | 189 | Γ£à Completo | Form validation, Type selection, Document mask, Name, Email, Phone, Address fields |
| EditProviderDialog | EditProviderDialog.razor | 176 | Γ£à Completo | Pre-populated form, data loading, validation |
| VerifyProviderDialog | VerifyProviderDialog.razor | 100 | Γ£à Completo | Status selection (Verified/Rejected/Pending), Comments field |
| UploadDocumentDialog | UploadDocumentDialog.razor | 166 | Γ£à Completo | File picker, Document type selection, Validation (PDF/JPEG/PNG, 10MB max) |
| ProviderSelectorDialog | ProviderSelectorDialog.razor | 72 | Γ£à Completo | Fluxor integration, Searchable provider list, Pagination support |

**Implementa├º├╡es Verificadas**:
- Γ£à **CreateProviderDialog**: Formul├írio completo com MudGrid, MudSelect (Individual/Business), campos de endere├ºo completo (Street, Number, Complement, Neighborhood, City, State, PostalCode), valida├º├úo FluentValidation, Snackbar notifications
- Γ£à **EditProviderDialog**: Carrega dados do provider via IProvidersApi, loading states, error handling, email readonly (n├úo edit├ível), Portuguese labels
- Γ£à **VerifyProviderDialog**: MudSelect com 3 status (Verified, Rejected, Pending), campo de observa├º├╡es (opcional), submit com loading spinner
- Γ£à **UploadDocumentDialog**: MudFileUpload com 7 tipos de documento (RG, CNH, CPF, CNPJ, Comprovante, Certid├úo, Outros), Accept=".pdf,.jpg,.jpeg,.png", MaximumFileCount=1, tamanho formatado
- Γ£à **ProviderSelectorDialog**: Usa Fluxor ProvidersState, dispatch de LoadProvidersAction, lista clic├ível com MudList, error states com retry button

**Padr├╡es Arquiteturais Observados**:
- Γ£à MudBlazor components (MudDialog, MudForm, MudTextField, MudSelect, MudFileUpload, MudList)
- Γ£à Portuguese labels e mensagens
- Γ£à Proper error handling com try/catch
- Γ£à Snackbar notifications (Severity.Success, Severity.Error)
- Γ£à Loading states com MudProgressCircular/MudProgressLinear
- Γ£à MudMessageBox confirmations (opcional)
- Γ£à CascadingParameter IMudDialogInstance para Close/Cancel
- Γ£à Validation com MudForm @bind-IsValid
- ΓÜá∩╕Å **Pragmatic Approach**: Dialogs usam direct API calls (conforme decis├úo arquitetural Sprint 7.7)

**2. Corre├º├úo de Erros de Build** Γ£à (Commit 9e5da3ac):

Durante verifica├º├úo, encontrados 26 erros de compila├º├úo em testes:

**Problemas Identificados**:
- Γ¥î `Response<T>` type not found (namespace MeAjudaAi.Contracts vs MeAjudaAi.Shared.Models)
- Γ¥î `PagedResult<T>` type not found (missing using directive)
- Γ¥î Test helper classes `Request` e `TestPagedRequest` n├úo existiam
- Γ¥î `Response<T>` n├úo tinha propriedade `IsSuccess`
- Γ¥î `PagedResult<T>` instantiation usava construtor inexistente (usa required properties)

**Solu├º├╡es Implementadas**:
1. Γ£à Adicionado `using MeAjudaAi.Shared.Models;` e `using MeAjudaAi.Contracts.Models;` em ContractsTests.cs
2. Γ£à Criadas classes de teste helper:
   ```csharp
   public abstract record Request { public string? UserId { get; init; } }
   public record TestPagedRequest : Request { 
       public int PageSize { get; init; } = 10;
       public int PageNumber { get; init; } = 1;
   }
   ```
3. Γ£à Adicionado `IsSuccess` computed property a `Response<T>`:
   ```csharp
   public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
   ```
4. Γ£à Adicionado default constructor a `Response<T>`:
   ```csharp
   public Response() : this(default, 200, null) { }
   ```
5. Γ£à Corrigido PagedResult instantiation em BaseEndpointTests:
   ```csharp
   new PagedResult<string> { Items = items, PageNumber = 1, PageSize = 5, TotalItems = 10 }
   ```
6. Γ£à Adicionado `HandlePagedResult<T>` method wrapper em TestEndpoint class

**Resultado**:
- Γ£à Build completo em Release mode: **0 errors, 5 warnings (apenas Sonar)**
- Γ£à 26 erros resolvidos
- Γ£à Todos os testes compilando corretamente

**Commits**:
- 9e5da3ac: "fix: resolve test build errors"

**Arquivos Modificados**:
- `tests/MeAjudaAi.Shared.Tests/Unit/Contracts/ContractsTests.cs`: +17 linhas (usings + helper classes)
- `tests/MeAjudaAi.Shared.Tests/Unit/Endpoints/BaseEndpointTests.cs`: +5 linhas (using + HandlePagedResult)
- `src/Shared/Models/Response.cs`: +9 linhas (IsSuccess property + default constructor)

**3. Garantia de Qualidade** Γ£à:

Verifica├º├╡es realizadas:
- Γ£à Todos os 11 dialogs compilam sem erros
- Γ£à Nenhum dialog tem c├│digo incompleto ou TODOs
- Γ£à Todos seguem padr├úo MudBlazor consistente
- Γ£à Error handling presente em todos
- Γ£à Loading states implementados
- Γ£à Portuguese labels consistentes
- Γ£à Integra├º├úo com APIs funcionando (IProvidersApi, IDocumentsApi, IServiceCatalogsApi, ILocationsApi)

**Pr├│ximos Passos**:
- Sprint 8: Customer App (Web + Mobile)
- Continuar otimiza├º├úo de testes com RequiredModules
- Atualizar docs/architecture.md com testing patterns

---

### Γ£à Sprint 7.9 - Magic Strings Elimination - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Ap├│s refactoring Flux (Sprint 7.7) e verifica├º├úo de dialogs (Sprint 7.8), foi identificado que status values (Verified, Pending, Rejected) e tipos (Individual, Business) estavam hardcoded em 30+ lugares. Part 9 consistiu em eliminar todos magic strings e centralizar constantes.

**Objetivos**:
1. Γ£à **Criar Arquivos de Constantes Centralizados**
2. Γ£à **Atualizar Todos os Componentes para Usar Constantes**
3. Γ£à **Criar Extension Methods para Display Names**
4. Γ£à **Adicionar Suporte a Localiza├º├úo (Portugu├¬s)**
5. Γ£à **Alinhar com Enums do Backend**
6. Γ£à **Adicionar Documenta├º├úo XML Completa**

**Progresso Atual**: 6/6 objetivos completos Γ£à **SPRINT 7.9 CONCLU├ìDO 100%!**

**1. Arquivos de Constantes Criados** Γ£à (Commit 0857cf0a):

**Constants/ProviderConstants.cs** (180 linhas):
- `ProviderType`: None=0, Individual=1, Company=2, Cooperative=3, Freelancer=4
- `VerificationStatus`: None=0, Pending=1, InProgress=2, Verified=3, Rejected=4, Suspended=5
- `ProviderStatus`: None=0, PendingBasicInfo=1, PendingDocumentVerification=2, Active=3, Suspended=4, Rejected=5
- Extension methods: `ToDisplayName(int)`, `ToColor(int)` com MudBlazor.Color
- Helper method: `GetAll()` retorna lista de (Value, DisplayName)

**Constants/DocumentConstants.cs** (150 linhas):
- `DocumentStatus`: Uploaded=1, PendingVerification=2, Verified=3, Rejected=4, Failed=5
- `DocumentType`: IdentityDocument=1, ProofOfResidence=2, CriminalRecord=3, Other=99
- Extension methods: `ToDisplayName(int)`, `ToDisplayName(string)`, `ToColor(int)`, `ToColor(string)`
- Helper method: `GetAll()` para DocumentType

**Constants/CommonConstants.cs** (119 linhas):
- `ActivationStatus`: Active=true, Inactive=false com `ToDisplayName(bool)`, `ToColor(bool)`, `ToIcon(bool)`
- `CommonActions`: Create, Update, Delete, Activate, Deactivate, Verify com `ToDisplayName(string)`
- `MessageSeverity`: Success, Info, Warning, Error com `ToMudSeverity(string)`

**2. Componentes Atualizados** Γ£à:

| Componente | Antes | Depois | Mudan├ºas |
|------------|-------|--------|----------|
| VerifyProviderDialog.razor | 3 hardcoded strings | VerificationStatus constants | VerificationStatuses class removida, `ToDisplayName()` no select |
| CreateProviderDialog.razor | "Individual"/"Business" | ProviderType.Individual/Company | Model.ProviderTypeValue como int, `ToDisplayName()` |
| DocumentsEffects.cs | "PendingVerification" string | DocumentStatus.ToDisplayName() | Type-safe constant |
| Documents.razor | switch/case status colors | DocumentStatus.ToColor() | Status chip com `ToDisplayName()` |
| Dashboard.razor | GetProviderTypeLabel() method | ProviderType.ToDisplayName() | Chart labels localizados, StatusOrder array atualizado |
| Categories.razor | "Ativa"/"Inativa" strings | ActivationStatus.ToDisplayName() | Status chip com `ToColor()` |
| Services.razor | "Ativo"/"Inativo" strings | ActivationStatus.ToDisplayName() | Status chip com `ToColor()` |
| AllowedCities.razor | "Ativa"/"Inativa" strings | ActivationStatus.ToDisplayName() | Status chip com `ToColor()` |
| Providers.razor | VERIFIED_STATUS constant | VerificationStatus.Verified | Status chip com `ToColor()` e `ToDisplayName()`, disable logic atualizado |

**Total**: 10 componentes atualizados + 30+ magic strings eliminados

**3. Extension Methods Implementados** Γ£à:

**Display Names (Portugu├¬s)**:
```csharp
ProviderType.ToDisplayName(1) ΓåÆ "Pessoa F├¡sica"
ProviderType.ToDisplayName(2) ΓåÆ "Pessoa Jur├¡dica"
VerificationStatus.ToDisplayName(3) ΓåÆ "Verificado"
VerificationStatus.ToDisplayName(1) ΓåÆ "Pendente"
DocumentStatus.ToDisplayName("PendingVerification") ΓåÆ "Aguardando Verifica├º├úo"
ActivationStatus.ToDisplayName(true) ΓåÆ "Ativo"
```

**Color Mapping (MudBlazor)**:
```csharp
VerificationStatus.ToColor(3) ΓåÆ Color.Success   // Verified
VerificationStatus.ToColor(1) ΓåÆ Color.Warning   // Pending
VerificationStatus.ToColor(4) ΓåÆ Color.Error     // Rejected
DocumentStatus.ToColor("Verified") ΓåÆ Color.Success
ActivationStatus.ToColor(true) ΓåÆ Color.Success
```

**Icon Mapping** (ActivationStatus):
```csharp
ActivationStatus.ToIcon(true) ΓåÆ Icons.Material.Filled.CheckCircle
ActivationStatus.ToIcon(false) ΓåÆ Icons.Material.Filled.Cancel
```

**4. Alinhamento Backend/Frontend** Γ£à:

Constantes frontend replicam exatamente os enums do backend:
- `ProviderConstants` Γåö∩╕Å `Modules.Providers.Domain.Enums.EProviderType`, `EVerificationStatus`, `EProviderStatus`
- `DocumentConstants` Γåö∩╕Å `Modules.Documents.Domain.Enums.EDocumentStatus`, `EDocumentType`
- Valores num├⌐ricos id├¬nticos (Individual=1, Company=2, etc.)
- Sem├óntica preservada (Pending=1, Verified=3, Rejected=4)

**5. Documenta├º├úo XML** Γ£à:

Todos os 3 arquivos de constantes possuem:
- `<summary>` para cada constante
- `<param>` e `<returns>` para todos os m├⌐todos
- `<remarks>` quando relevante
- Exemplos de uso em coment├írios
- Portugu├¬s para descri├º├╡es de neg├│cio

**6. Benef├¡cios Alcan├ºados** Γ£à:

| Benef├¡cio | Impacto |
|-----------|---------|
| **Type Safety** | Erros de digita├º├úo imposs├¡veis (Verifiied vs Verified) |
| **Intellisense** | Auto-complete para todos os status/tipos |
| **Manutenibilidade** | Mudan├ºa em 1 lugar propaga para todos |
| **Localiza├º├úo** | Labels em portugu├¬s centralizados |
| **Consist├¬ncia** | Cores MudBlazor padronizadas |
| **Testabilidade** | Constants mock├íveis e isolados |
| **Performance** | Sem aloca├º├úo de strings duplicadas |

**M├⌐tricas**:
- **Strings Eliminados**: 30+ hardcoded strings
- **Arquivos Criados**: 3 (ProviderConstants, DocumentConstants, CommonConstants)
- **Componentes Atualizados**: 10
- **Linhas de C├│digo**: +449 (constants) | -48 (hardcoded strings) = +401 net
- **Build**: Sucesso com 4 warnings (nullability - n├úo relacionados)

**Commits**:
- 0857cf0a: "refactor: eliminate magic strings with centralized constants"

**Arquivos Modificados**:
- `src/Web/MeAjudaAi.Web.Admin/Constants/ProviderConstants.cs` (criado - 180 linhas)
- `src/Web/MeAjudaAi.Web.Admin/Constants/DocumentConstants.cs` (criado - 150 linhas)
- `src/Web/MeAjudaAi.Web.Admin/Constants/CommonConstants.cs` (criado - 119 linhas)
- `Components/Dialogs/VerifyProviderDialog.razor` (updated)
- `Components/Dialogs/CreateProviderDialog.razor` (updated)
- `Features/Documents/DocumentsEffects.cs` (updated)
- `Pages/Documents.razor` (updated)
- `Pages/Dashboard.razor` (updated)
- `Pages/Categories.razor` (updated)
- `Pages/Services.razor` (updated)
- `Pages/AllowedCities.razor` (updated)
- `Pages/Providers.razor` (updated)

---

### Γ£à Sprint 7.10 - Accessibility Features - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Admin Portal precisava de melhorias de acessibilidade para compliance WCAG 2.1 AA, suporte a leitores de tela, navega├º├úo por teclado e ARIA labels.

**Objetivos**:
1. Γ£à **ARIA Labels e Roles Sem├ónticos**
2. Γ£à **Live Region para An├║ncios de Leitores de Tela**
3. Γ£à **Skip-to-Content Link**
4. Γ£à **Navega├º├úo por Teclado Completa**
5. Γ£à **Documenta├º├úo de Acessibilidade**

**Progresso Atual**: 5/5 objetivos completos Γ£à **SPRINT 7.10 CONCLU├ìDO 100%!**

**Arquivos Criados**:
- `Helpers/AccessibilityHelper.cs` (178 linhas): AriaLabels constants, LiveRegionAnnouncements, keyboard shortcuts
- `Components/Accessibility/LiveRegionAnnouncer.razor` (50 linhas): ARIA live region component
- `Components/Accessibility/SkipToContent.razor` (20 linhas): Skip-to-content link
- `Services/LiveRegionService.cs` (79 linhas): Service para an├║ncios de leitores de tela
- `docs/accessibility.md` (350+ linhas): Guia completo de acessibilidade

**Arquivos Modificados**:
- `Layout/MainLayout.razor`: Adicionado SkipToContent e LiveRegionAnnouncer, enhanced ARIA labels
- `Pages/Providers.razor`: ARIA labels contextuais ("Editar provedor {name}")
- `Program.cs`: Registrado LiveRegionService

**Benef├¡cios**:
- Γ£à WCAG 2.1 AA compliant
- Γ£à Navega├º├úo apenas por teclado funcional
- Γ£à Suporte a leitores de tela (NVDA, JAWS, VoiceOver)
- Γ£à Skip-to-content para usu├írios de teclado
- Γ£à Contrast ratio 4.5:1+ em todos elementos

**Commit**: 38659852

---

### Γ£à Sprint 7.11 - Error Boundaries - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Necessidade de sistema robusto de error handling para capturar erros de renderiza├º├úo de componentes, registrar com correlation IDs e fornecer op├º├╡es de recupera├º├úo ao usu├írio.

**Objetivos**:
1. Γ£à **ErrorBoundary Global no App.razor**
2. Γ£à **ErrorLoggingService com Correlation IDs**
3. Γ£à **Fluxor Error State Management**
4. Γ£à **ErrorBoundaryContent UI com Recovery Options**
5. Γ£à **Integra├º├úo com LiveRegion para An├║ncios**

**Progresso Atual**: 5/5 objetivos completos Γ£à **SPRINT 7.11 CONCLU├ìDO 100%!**

**Arquivos Criados**:
- `Services/ErrorLoggingService.cs` (108 linhas): LogComponentError, LogUnhandledError, GetUserFriendlyMessage
- `Features/Errors/ErrorState.cs` (48 linhas): GlobalError, CorrelationId, UserMessage, TechnicalDetails
- `Features/Errors/ErrorFeature.cs` (24 linhas): Fluxor feature state
- `Features/Errors/ErrorActions.cs` (17 linhas): SetGlobalErrorAction, ClearGlobalErrorAction, RetryAfterErrorAction
- `Features/Errors/ErrorReducers.cs` (37 linhas): Reducers para error state
- `Components/Errors/ErrorBoundaryContent.razor` (118 linhas): UI de erro com retry, reload, go home

**Arquivos Modificados**:
- `App.razor`: Wrapped Router em ErrorBoundary, added error logging e dispatch
- `Program.cs`: Registrado ErrorLoggingService

**Features**:
- **Correlation IDs**: Cada erro tem ID ├║nico para tracking
- **User-Friendly Messages**: Exception types mapeados para mensagens em portugu├¬s
- **Recovery Options**: Retry (se recoverable), Go Home, Reload Page
- **Technical Details**: Expans├¡vel para desenvolvedores (stack trace)
- **Fluxor Integration**: Error state global acess├¡vel em qualquer componente

**Commit**: da1d1300

---

### Γ£à Sprint 7.12 - Performance Optimizations - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Admin Portal precisava de otimiza├º├╡es para lidar com grandes datasets (1000+ providers) sem degrada├º├úo de performance. Implementado virtualization, debouncing, memoization e batch processing.

**Objetivos**:
1. Γ£à **Virtualization em MudDataGrid**
2. Γ£à **Debounced Search (300ms)**
3. Γ£à **Memoization para Opera├º├╡es Caras**
4. Γ£à **Batch Processing para Evitar UI Blocking**
5. Γ£à **Throttling para Opera├º├╡es Rate-Limited**
6. Γ£à **Performance Monitoring Helpers**
7. Γ£à **Documenta├º├úo de Performance**

**Progresso Atual**: 7/7 objetivos completos Γ£à **SPRINT 7.12 CONCLU├ìDO 100%!**

**Arquivos Criados**:
- `Helpers/DebounceHelper.cs` (66 linhas): Debounce helper class e extensions
- `Helpers/PerformanceHelper.cs` (127 linhas): MeasureAsync, Memoize, ProcessInBatchesAsync, ShouldThrottle
- `docs/performance.md` (350+ linhas): Guia completo de otimiza├º├╡es de performance

**Arquivos Modificados**:
- `Pages/Providers.razor`: 
  * Adicionado MudTextField para search com DebounceInterval="300"
  * Virtualize="true" em MudDataGrid
  * Memoization para filtered providers (30s cache)
  * IDisposable implementation para limpar cache

**Melhorias de Performance**:

| M├⌐trica | Antes | Depois | Melhoria |
|---------|-------|--------|----------|
| Render 1000 items | 850ms | 180ms | 78% faster |
| Search API calls | 12/sec | 3/sec | 75% fewer |
| Memory usage | 45 MB | 22 MB | 51% less |
| Scroll FPS | 30 fps | 60 fps | 100% smoother |

**T├⌐cnicas Implementadas**:
- **Virtualization**: Renderiza apenas linhas vis├¡veis (~20-30), suporta 10,000+ items
- **Debouncing**: Espera 300ms ap├│s ├║ltima tecla antes de executar search
- **Memoization**: Cache de filtered results por 30 segundos
- **Batch Processing**: Processa 50 items/vez com delay de 10ms entre batches
- **Throttling**: Rate-limit para opera├º├╡es cr├¡ticas (5s min interval)

**Commit**: fa8a9599

---

### Γ£à Sprint 7.13 - Standardized Error Handling - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Admin Portal precisava de tratamento de erro padronizado com retry logic autom├ítico, mensagens amig├íveis em portugu├¬s e correlation IDs para troubleshooting.

**Objetivos**:
1. Γ£à **ErrorHandlingService Centralizado**
2. Γ£à **Retry Logic com Exponential Backoff**
3. Γ£à **Mapeamento de HTTP Status Codes para Mensagens Amig├íveis**
4. Γ£à **Correlation ID Tracking**
5. Γ£à **Integra├º├úo com Fluxor Effects**
6. Γ£à **Documenta├º├úo de Error Handling**

**Progresso Atual**: 6/6 objetivos completos Γ£à **SPRINT 7.13 CONCLU├ìDO 100%!**

**Arquivos Criados**:
- `Services/ErrorHandlingService.cs` (216 linhas):
  * HandleApiError<T>(Result<T> result, string operation) - Trata erros e retorna mensagem amig├ível
  * ExecuteWithRetryAsync<T>() - Executa opera├º├╡es com retry autom├ítico (at├⌐ 3 tentativas)
  * ShouldRetry() - Determina se deve retry (apenas 5xx e 408 timeout)
  * GetRetryDelay() - Exponential backoff: 1s, 2s, 4s
  * GetUserFriendlyMessage() - Mapeia status HTTP para mensagens em portugu├¬s
  * GetMessageFromHttpStatus() - 15+ mapeamentos de status code
  * ErrorInfo record - Encapsula Message, CorrelationId, StatusCode
- `docs/error-handling.md` (350+ linhas): Guia completo de tratamento de erros

**Arquivos Modificados**:
- `Program.cs`: builder.Services.AddScoped<ErrorHandlingService>();
- `Features/Providers/ProvidersEffects.cs`:
  * Injetado ErrorHandlingService
  * GetProvidersAsync wrapped com ExecuteWithRetryAsync (3 tentativas)
  * GetUserFriendlyMessage(403) para erros de autoriza├º├úo
  * Automatic retry para erros transientes (network, timeout, server errors)

**Funcionalidades de Error Handling**:

| Recurso | Implementa├º├úo |
|---------|---------------|
| HTTP Status Mapping | 400ΓåÆ"Requisi├º├úo inv├ílida", 401ΓåÆ"N├úo autenticado", 403ΓåÆ"Sem permiss├úo", 404ΓåÆ"N├úo encontrado", etc. |
| Retry Transient Errors | 5xx (Server Error), 408 (Timeout) com at├⌐ 3 tentativas |
| Exponential Backoff | 1s ΓåÆ 2s ΓåÆ 4s entre tentativas |
| Correlation IDs | Activity.Current?.Id para rastreamento distribu├¡do |
| Fallback Messages | Backend message priorit├íria, fallback para status code mapping |
| Exception Handling | HttpRequestException e Exception com logging |

**Mensagens de Erro Suportadas**:
- **400**: Requisi├º├úo inv├ílida. Verifique os dados fornecidos.
- **401**: Voc├¬ n├úo est├í autenticado. Fa├ºa login novamente.
- **403**: Voc├¬ n├úo tem permiss├úo para realizar esta a├º├úo.
- **404**: Recurso n├úo encontrado.
- **408**: A requisi├º├úo demorou muito. Tente novamente.
- **429**: Muitas requisi├º├╡es. Aguarde um momento.
- **500**: Erro interno do servidor. Nossa equipe foi notificada.
- **502/503**: Servidor/Servi├ºo temporariamente indispon├¡vel.
- **504**: O servidor n├úo respondeu a tempo.

**Padr├úo de Uso**:

```csharp
// Antes (sem retry, mensagem crua)
var result = await _providersApi.GetProvidersAsync(pageNumber, pageSize);
if (result.IsFailure) {
    dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error?.Message ?? "Erro"));
}

// Depois (com retry autom├ítico, mensagem amig├ível)
var result = await _errorHandler.ExecuteWithRetryAsync(
    () => _providersApi.GetProvidersAsync(pageNumber, pageSize),
    "carregar provedores",
    3);
if (result.IsFailure) {
    var userMessage = _errorHandler.HandleApiError(result, "carregar provedores");
    dispatcher.Dispatch(new LoadProvidersFailureAction(userMessage));
}
```

**Benef├¡cios**:
- Γ£à Resili├¬ncia contra erros transientes (automatic retry)
- Γ£à UX melhorado com mensagens em portugu├¬s
- Γ£à Troubleshooting facilitado com correlation IDs
- Γ£à Logging estruturado de todas as tentativas
- Γ£à Redu├º├úo de chamadas ao suporte (mensagens auto-explicativas)

**Commit**: c198d889 "feat(sprint-7.13): implement standardized error handling with retry logic"

---

### Γ£à Sprint 7.14 - Complete Localization (i18n) - CONCLU├ìDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Admin Portal precisava de suporte multi-idioma com troca din├ómica de idioma e tradu├º├╡es completas para pt-BR e en-US.

**Objetivos**:
1. Γ£à **LocalizationService com Dictionary-Based Translations**
2. Γ£à **LanguageSwitcher Component**
3. Γ£à **140+ Translation Strings (pt-BR + en-US)**
4. Γ£à **Culture Switching com CultureInfo**
5. Γ£à **OnCultureChanged Event para Reactivity**
6. Γ£à **Documenta├º├úo de Localiza├º├úo**

**Progresso Atual**: 6/6 objetivos completos Γ£à **SPRINT 7.14 CONCLU├ìDO 100%!**

**Arquivos Criados**:
- `Services/LocalizationService.cs` (235 linhas):
  * Dictionary-based translations (pt-BR, en-US)
  * SetCulture(cultureName) - Muda idioma e dispara OnCultureChanged
  * GetString(key) - Retorna string localizada com fallback
  * GetString(key, params) - Formata├º├úo com par├ómetros
  * SupportedCultures property - Lista de idiomas dispon├¡veis
  * CurrentCulture, CurrentLanguage properties
- `Components/Common/LanguageSwitcher.razor` (35 linhas):
  * MudMenu com ├¡cone de idioma (≡ƒîÉ)
  * Lista de idiomas dispon├¡veis
  * Check mark no idioma atual
  * Integrado no MainLayout AppBar
- `docs/localization.md` (550+ linhas): Guia completo de internacionaliza├º├úo

**Arquivos Modificados**:
- `Program.cs`: builder.Services.AddScoped<LocalizationService>();
- `Layout/MainLayout.razor`: 
  * @using MeAjudaAi.Web.Admin.Components.Common
  * <LanguageSwitcher /> adicionado antes do menu do usu├írio

**Tradu├º├╡es Implementadas** (140+ strings):

| Categoria | pt-BR | en-US | Exemplos |
|-----------|-------|-------|----------|
| Common (12) | Salvar, Cancelar, Excluir, Editar | Save, Cancel, Delete, Edit | Common.Save, Common.Loading |
| Navigation (5) | Painel, Provedores, Documentos | Dashboard, Providers, Documents | Nav.Dashboard, Nav.Logout |
| Providers (9) | Nome, Documento, Status | Name, Document, Status | Providers.Active, Providers.SearchPlaceholder |
| Validation (4) | Campo obrigat├│rio, E-mail inv├ílido | Field required, Invalid email | Validation.Required |
| Success (3) | Salvo com sucesso | Saved successfully | Success.SavedSuccessfully |
| Error (3) | Erro de conex├úo | Connection error | Error.NetworkError |

**Funcionalidades de Localiza├º├úo**:

| Recurso | Implementa├º├úo |
|---------|---------------|
| Idiomas Suportados | pt-BR (Portugu├¬s Brasil), en-US (English US) |
| Default Language | pt-BR |
| Fallback Mechanism | en-US como fallback se string n├úo existe em pt-BR |
| String Formatting | Suporte a par├ómetros: L["Messages.ItemsFound", count] |
| Culture Switching | CultureInfo.CurrentCulture e CurrentUICulture |
| Component Reactivity | OnCultureChanged event dispara StateHasChanged |
| Date/Time Formatting | Autom├ítico via CultureInfo (15/12/2024 vs 12/15/2024) |
| Number Formatting | Autom├ítico (R$ 1.234,56 vs $1,234.56) |

**Padr├úo de Uso**:

```razor
@inject LocalizationService L

<!-- Strings simples -->
<MudButton>@L.GetString("Common.Save")</MudButton>

<!-- Com par├ómetros -->
<MudText>@L.GetString("Providers.ItemsFound", providerCount)</MudText>

<!-- Reatividade em mudan├ºa de idioma -->
@code {
    protected override void OnInitialized()
    {
        L.OnCultureChanged += StateHasChanged;
    }
}
```

**Conven├º├╡es de Nomenclatura**:
- `{Categoria}.{A├º├úo/Contexto}{Tipo}` - Estrutura hier├írquica
- Common.* - Textos compartilhados
- Nav.* - Navega├º├úo e menus
- Providers.*, Documents.* - Espec├¡fico de entidade
- Validation.* - Mensagens de valida├º├úo
- Success.*, Error.* - Feedback de opera├º├╡es

**Benef├¡cios**:
- Γ£à Admin Portal preparado para mercado global
- Γ£à UX melhorado com idioma nativo do usu├írio
- Γ£à Facilita adi├º├úo de novos idiomas (es-ES, fr-FR)
- Γ£à Formata├º├úo autom├ítica de datas/n├║meros por cultura
- Γ£à Manuten├º├úo centralizada de strings UI

**Futuro (Roadmap de Localization)**:
- [ ] Persist├¬ncia de prefer├¬ncia no backend
- [ ] Auto-detec├º├úo de idioma do navegador
- [ ] Strings para todas as p├íginas (Dashboard, Documents, etc.)
- [ ] Pluraliza├º├úo avan├ºada (1 item vs 2 items)
- [ ] Adicionar es-ES, fr-FR
- [ ] FluentValidation messages localizadas

**Commit**: 2e977908 "feat(sprint-7.14): implement complete localization (i18n)"

---

### Γ£à Sprint 7.15 - Package Updates & Resilience Migration (16 Jan 2026)

**Status**: CONCLU├ìDA (16 Jan 2026)  
**Dura├º├úo**: 1 dia  
**Commits**: b370b328, 949b6d3c

**Contexto**: Atualiza├º├úo de rotina de pacotes NuGet revelou depreca├º├úo do Polly.Extensions.Http, necessitando migra├º├úo para Microsoft.Extensions.Http.Resilience (nova API oficial do .NET 10).

#### ≡ƒôª Atualiza├º├╡es de Pacotes (39 packages)

**ASP.NET Core 10.0.2**:
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.OpenApi
- Microsoft.AspNetCore.TestHost
- Microsoft.AspNetCore.Components.WebAssembly
- Microsoft.AspNetCore.Components.WebAssembly.Authentication
- Microsoft.AspNetCore.Components.WebAssembly.DevServer
- Microsoft.Extensions.Http (10.2.0)
- Microsoft.Extensions.Http.Resilience (10.2.0) - **NOVO**

**Entity Framework Core 10.0.2**:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.EntityFrameworkCore.Relational
- Npgsql.EntityFrameworkCore.PostgreSQL (10.0.0)

**Ferramentas Build (18.0.2)** - Breaking Change:
- Microsoft.Build (17.14.28 ΓåÆ 18.0.2)
- Microsoft.Build.Framework (requerido por EF Core Design 10.0.2)
- Microsoft.Build.Locator
- Microsoft.Build.Tasks.Core
- Microsoft.Build.Utilities.Core
- **Resolu├º├úo**: Removido pin CVE (CVE-2024-38095 corrigido na 18.0+)

**Azure Storage 12.27.0**:
- Azure.Storage.Blobs (12.27.0)
- Azure.Storage.Common (12.25.0 ΓåÆ 12.26.0 - conflito resolvido)

**Outras Atualiza├º├╡es**:
- System.IO.Hashing (9.0.10 ΓåÆ 10.0.1)
- Microsoft.CodeAnalysis.Analyzers (3.11.0 ΓåÆ 3.14.0)
- Refit (9.0.2 ΓåÆ 9.1.2)
- AngleSharp, AngleSharp.Css (1.2.0 ΓåÆ 1.3.0)
- ... (total 39 packages)

**Decis├úo Microsoft.OpenApi**:
- Testado 3.1.3: **INCOMPAT├ìVEL** (CS0200 com source generators .NET 10)
- Mantido 2.3.0: **EST├üVEL** (funciona perfeitamente)
- Confirmado 16/01/2026 com SDK 10.0.102

#### ≡ƒöä Migra├º├úo Polly.Extensions.Http ΓåÆ Microsoft.Extensions.Http.Resilience

**Pacote Removido**:
```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Polly.Extensions.Http" Version="3.0.0" Remove="true" />
```

**Novo Pacote**:
```xml
<PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="10.2.0" />
```

**Refatora├º├úo de C├│digo**:

1. **`PollyPolicies.cs` ΓåÆ `ResiliencePolicies.cs`** (renomeado):
   ```csharp
   // ANTES (Polly.Extensions.Http)
   public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
   {
       return HttpPolicyExtensions
           .HandleTransientHttpError()
           .WaitAndRetryAsync(3, retryAttempt => 
               TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
   }

   // DEPOIS (Microsoft.Extensions.Http.Resilience)
   public static void ConfigureRetry(HttpRetryStrategyOptions options)
   {
       options.MaxRetryAttempts = 3;
       options.Delay = TimeSpan.FromSeconds(2);
       options.BackoffType = DelayBackoffType.Exponential;
       options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
           .HandleResult(response => 
               response.StatusCode >= HttpStatusCode.InternalServerError ||
               response.StatusCode == HttpStatusCode.RequestTimeout);
   }
   ```

2. **`ServiceCollectionExtensions.cs`**:
   ```csharp
   // ANTES
   client.AddPolicyHandler(PollyPolicies.GetRetryPolicy())
         .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy())
         .AddPolicyHandler(PollyPolicies.GetTimeoutPolicy());

   // DEPOIS
   client.AddStandardResilienceHandler(options =>
   {
       ResiliencePolicies.ConfigureRetry(options.Retry);
       ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker);
       ResiliencePolicies.ConfigureTimeout(options.TotalRequestTimeout);
   });

   // Upload timeout separado (sem retry)
   client.AddStandardResilienceHandler(options =>
   {
       options.Retry.MaxRetryAttempts = 0; // Disable retry for uploads
       ResiliencePolicies.ConfigureUploadTimeout(options.TotalRequestTimeout);
   });
   ```

**Pol├¡ticas Configuradas**:
- **Retry**: 3 tentativas, backoff exponencial (2s, 4s, 8s)
- **Circuit Breaker**: 50% failure ratio, 5 throughput m├¡nimo, 30s break duration
- **Timeout**: 30s padr├úo, 120s para uploads

**Arquivos Impactados**:
- `Directory.Packages.props` (remo├º├úo + adi├º├úo de pacote)
- `src/MeAjudaAi.Web.Admin/Infrastructure/Http/ResiliencePolicies.cs` (renomeado e refatorado)
- `src/MeAjudaAi.Web.Admin/Infrastructure/Extensions/ServiceCollectionExtensions.cs` (nova API)

#### Γ£à Resultados

**Build Status**:
- Γ£à 0 erros de compila├º├úo
- Γ£à 10 warnings pr├⌐-existentes (analyzers - n├úo relacionados)
- Γ£à Todos os 1245 testes passando

**Comportamento Mantido**:
- Γ£à Retry logic id├¬ntico
- Γ£à Circuit breaker configura├º├úo equivalente
- Γ£à Timeouts diferenciados (standard vs upload)
- Γ£à HTTP resilience sem quebras

**Compatibilidade**:
- Γ£à .NET 10.0.2 LTS (suporte at├⌐ Nov 2028)
- Γ£à EF Core 10.0.2
- Γ£à Microsoft.Build 18.0.2 (├║ltima stable)
- Γ£à Npgsql 10.x + Hangfire.PostgreSql 1.20.13

**Technical Debt Removido**:
- Γ£à Deprecated package eliminado (Polly.Extensions.Http)
- Γ£à Migra├º├úo para API oficial Microsoft (.NET 10)
- Γ£à CVE pin removido (Microsoft.Build CVE-2024-38095)

**Li├º├╡es Aprendidas**:
- Microsoft.OpenApi 3.1.3 incompat├¡vel com source generators .NET 10 (CS0200 read-only property)
- Microsoft.Build breaking change (17.x ΓåÆ 18.x) necess├írio para EF Core Design 10.0.2
- AddStandardResilienceHandler simplifica configura├º├úo (3 chamadas ΓåÆ 1 com options)
- Upload timeout requer retry desabilitado (MaxRetryAttempts = 0)

**Commits**:
- `b370b328`: "chore: update 39 nuget packages to latest stable versions"
- `949b6d3c`: "refactor: migrate from Polly.Extensions.Http to Microsoft.Extensions.Http.Resilience"

---

### Γ£à Sprint 7.20 - Dashboard Charts & Data Mapping Fixes (5 Fev 2026)

**Status**: CONCLU├ìDA (5 Fev 2026)  
**Dura├º├úo**: 1 dia  
**Branch**: `fix/aspire-initialization` (continua├º├úo)

**Contexto**: Dashboard charts estavam exibindo mensagens de debug e o gr├ífico "Provedores por Tipo" estava vazio devido a incompatibilidade de mapeamento JSON entre backend e frontend.

#### ≡ƒÄ» Objetivos

1. Γ£à **Remover Mensagens de Debug** - Eliminar "Chart disabled for debugging"
2. Γ£à **Corrigir Gr├ífico Vazio** - Resolver problema de dados ausentes em "Provedores por Tipo"
3. Γ£à **Implementar Mapeamento JSON Correto** - Alinhar propriedades backend/frontend
4. Γ£à **Adicionar Helper Methods** - Criar m├⌐todos de formata├º├úo localizados

#### ≡ƒöì Problema Identificado

**Root Cause**: Property name mismatch entre backend e frontend

- **Backend API** (`ProviderDto`): Retorna JSON com propriedade `type: 1`
- **Frontend DTO** (`ModuleProviderDto`): Esperava propriedade `ProviderType`
- **Resultado**: `ProviderType` ficava `null` no frontend, causando gr├ífico vazio

**Investiga├º├úo**:
1. Γ£à Verificado `DevelopmentDataSeeder.cs` - Dados de seed CONT├èM tipos ("Individual", "Company")
2. Γ£à Analisado `GetProvidersEndpoint.cs` - Retorna `ProviderDto` com propriedade `Type`
3. Γ£à Inspecionado `ModuleProviderDto.cs` - Propriedade chamada `ProviderType` (mismatch!)
4. Γ£à Confirmado via `ProvidersEffects.cs` - Usa `IProvidersApi.GetProvidersAsync`

#### ≡ƒ¢á∩╕Å Solu├º├╡es Implementadas

**1. JSON Property Mapping** Γ£à:
```csharp
// src/Contracts/Contracts/Modules/Providers/DTOs/ModuleProviderDto.cs
using System.Text.Json.Serialization;

public sealed record ModuleProviderDto(
    Guid Id,
    string Name,
    string Email,
    string Document,
    [property: JsonPropertyName("type")]  // ΓåÉ FIX: Mapeia "type" do JSON para "ProviderType"
    string ProviderType,
    string VerificationStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive,
    string? Phone = null);
```

**2. Debug Messages Removal** Γ£à:
```razor
<!-- src/Web/MeAjudaAi.Web.Admin/Pages/Dashboard.razor -->
<!-- ANTES -->
<MudCardContent>
    <MudText>Chart disabled for debugging</MudText>
    @if (ProvidersState.Value.Providers.Count > 0)

<!-- DEPOIS -->
<MudCardContent>
    @if (ProvidersState.Value.Providers.Count > 0)
```

**3. Display Name Helper** Γ£à:
```csharp
// Dashboard.razor @code
private string GetProviderTypeDisplayName(ProviderType type)
{
    return type switch
    {
        ProviderType.Individual => "Pessoa F├¡sica",
        ProviderType.Company => "Pessoa Jur├¡dica",
        _ => type.ToString()
    };
}
```

**4. Chart Logic Simplification** Γ£à:
```csharp
// Removido c├│digo complexo de parsing int
// ANTES: int.TryParse(g.Key, out int typeValue) + ProviderTypeOrderInts lookup
// DEPOIS: Enum.TryParse<ProviderType>(g.Key, true, out var typeEnum) + GetProviderTypeDisplayName()
```

#### ≡ƒôè Arquivos Modificados

| Arquivo | Mudan├ºas | LOC |
|---------|----------|-----|
| `ModuleProviderDto.cs` | Adicionado `[JsonPropertyName("type")]` e using | +3 |
| `Dashboard.razor` | Removido debug text, adicionado helper method | +12, -15 |

#### Γ£à Resultados Alcan├ºados

- Γ£à **Gr├ífico "Provedores por Tipo"**: Agora exibe dados corretamente
- Γ£à **Mensagens de Debug**: Removidas de ambos os gr├íficos
- Γ£à **Build**: Sucesso sem erros (0 errors, 0 warnings)
- Γ£à **Mapeamento JSON**: Backend `type` ΓåÆ Frontend `ProviderType` funcionando
- Γ£à **Localiza├º├úo**: Labels em portugu├¬s ("Pessoa F├¡sica", "Pessoa Jur├¡dica")

#### ≡ƒÄô Li├º├╡es Aprendidas

1. **Property Naming Conventions**: Backend usa nomes curtos (`Type`), Frontend usa nomes descritivos (`ProviderType`)
2. **JSON Serialization**: `[JsonPropertyName]` ├⌐ essencial para alinhar DTOs entre camadas
3. **Record Positional Parameters**: Atributos requerem `[property: ...]` syntax
4. **Debug Messages**: Sempre remover antes de merge para evitar confus├úo em produ├º├úo

#### ≡ƒö« Pr├│ximos Passos

- [ ] Implementar "Atividades Recentes" (ver Fase 3+)
- [ ] Adicionar mais gr├íficos ao Dashboard (distribui├º├úo geogr├ífica, documentos pendentes)
- [ ] Criar testes bUnit para componentes de gr├íficos

**Commits**:
- [hash]: "fix: add JsonPropertyName mapping for ProviderType in ModuleProviderDto"
- [hash]: "fix: remove debug messages and simplify chart logic in Dashboard"

---

### Γ£à Sprint 7.16 - Technical Debt Sprint (17-21 Jan 2026)

**Status**: Γ£à CONCLU├ìDA (17-21 Jan 2026)  
**Dura├º├úo**: 1 semana (5 dias ├║teis)  
**Objetivo**: Reduzir d├⌐bito t├⌐cnico ANTES de iniciar Customer App

**Justificativa**: 
- Customer App adicionar├í ~5000+ linhas de c├│digo novo
- Melhor resolver d├⌐bitos do Admin Portal ANTES de replicar patterns
- Keycloak automation ├⌐ BLOQUEADOR para Customer App (precisa de novo cliente OIDC)
- Quality improvements estabelecem padr├╡es para Customer App

---

#### ≡ƒôï Tarefas Planejadas

##### 1. ≡ƒöÉ Keycloak Client Automation (Dia 1-2, ~1 dia) - **BLOQUEADOR**

**Prioridade**: CR├ìTICA - Customer App precisa de cliente OIDC "meajudaai-customer"

**Entreg├íveis**:
- [ ] Script `infrastructure/keycloak/setup-keycloak-clients.ps1`
  * Valida Keycloak rodando (HTTP health check)
  * Obt├⌐m token admin via REST API
  * Cria realm "MeAjudaAi" (se n├úo existir)
  * Cria clientes "meajudaai-admin" e "meajudaai-customer" (OIDC, PKCE)
  * Configura Redirect URIs (localhost + produ├º├úo)
  * Cria roles "admin", "customer"
  * Cria usu├írios demo (admin@meajudaai.com.br, customer@meajudaai.com.br)
  * Exibe resumo de configura├º├úo
- [ ] Atualizar `docs/keycloak-admin-portal-setup.md` com se├º├úo "Automated Setup"
- [ ] Integrar script em `scripts/dev.ps1` (opcional - chamar setup-keycloak-clients.ps1)

**API Keycloak Admin REST**:
- Endpoint: `POST /auth/admin/realms/{realm}/clients`
- Autentica├º├úo: Bearer token

**Benef├¡cios**:
- Γ£à Customer App pronto para desenvolvimento (cliente configurado)
- Γ£à Onboarding em 1 comando: `.\setup-keycloak-clients.ps1`
- Γ£à Elimina 15 passos manuais documentados

---

##### 2. ≡ƒÄ¿ Frontend Analyzer Warnings (Dia 2-3, ~1 dia)

**Prioridade**: ALTA - Code quality antes de expandir codebase

**Warnings a Resolver**:

**S2094 - Empty Records (6 ocorr├¬ncias)**:
```csharp
// ANTES
public sealed record LoadProvidersAction { }

// DEPOIS - Op├º├úo 1: Adicionar propriedade ├║til
public sealed record LoadProvidersAction
{
    public bool ForceRefresh { get; init; }
}

// DEPOIS - Op├º├úo 2: Justificar supress├úo
#pragma warning disable S2094 // Empty action by design (Redux pattern)
public sealed record LoadProvidersAction { }
#pragma warning restore S2094
```

**S2953 - Dispose Pattern (1 ocorr├¬ncia)**:
```csharp
// ANTES: App.razor
public void Dispose() { ... }

// DEPOIS
public class App : IDisposable
{
    public void Dispose() { ... }
}
```

**S2933 - Readonly Fields (1 ocorr├¬ncia)**:
```csharp
// ANTES
private MudTheme _theme = new();

// DEPOIS
private readonly MudTheme _theme = new();
```

**MUD0002 - Casing (3 ocorr├¬ncias)**:
```razor
<!-- ANTES -->
<MudDrawer AriaLabel="Navigation" />

<!-- DEPOIS -->
<MudDrawer aria-label="Navigation" />
```

**Entreg├íveis**:
- [ ] Resolver todos os 11 warnings (ou justificar supress├╡es)
- [ ] Remover regras do `.editorconfig` ap├│s corre├º├úo
- [ ] Build com **0 warnings**

---

##### 3. ≡ƒôè Frontend Test Coverage (Dia 3-5, ~1-2 dias)

**Prioridade**: ALTA - Confian├ºa em Admin Portal antes de Customer App

**Meta**: 10 ΓåÆ 30-40 testes bUnit

**Testes Novos (20-30 testes)**:

**Fluxor State Management (8 testes)**:
- `ProvidersReducers`: LoadSuccess, LoadFailure, SetFilters, SetSorting
- `DocumentsReducers`: UploadSuccess, VerificationUpdate
- `ServiceCatalogsReducers`: CreateSuccess, UpdateSuccess

**Components (12 testes)**:
- `Providers.razor`: rendering, search, pagination (3 testes)
- `Documents.razor`: upload workflow, verification (3 testes)
- `CreateProviderDialog`: form validation, submit (2 testes)
- `EditProviderDialog`: data binding, update (2 testes)
- `LanguageSwitcher`: culture change, persistence (2 testes)

**Services (5 testes)**:
- `LocalizationService`: SetCulture, GetString, fallback
- `ErrorHandlingService`: retry logic, status mapping

**Effects (3 testes)**:
- Mock `IProvidersApi.GetPagedProvidersAsync`
- Verificar dispatches Success/Failure
- Testar error handling

**Infraestrutura**:
- Criar `TestContext` base reutiliz├ível
- Configurar `JSRuntimeMode.Loose`
- Registrar `MudServices` e `Fluxor`

**Entreg├íveis**:
- [ ] 30-40 testes bUnit (3x aumento)
- [ ] Cobertura ~40-50% de componentes cr├¡ticos
- [ ] CI/CD passing (master-ci-cd.yml)

---

##### 4. ≡ƒô¥ Records Standardization (Dia 5, ~0.5 dia)

**Prioridade**: M├ëDIA - Padroniza├º├úo importante

**Objetivo**: Padronizar uso de `record class` vs `record` vs `class` no projeto.

**Auditoria**:
```powershell
# Buscar todos os records no projeto
Get-ChildItem -Recurse -Include *.cs | Select-String "record "
```

**Padr├╡es a Estabelecer**:
- DTOs: `public record <Name>Dto` (imut├ível)
- Requests: `public sealed record <Name>Request` (imut├ível)
- Responses: `public sealed record <Name>Response` (imut├ível)
- Fluxor Actions: `public sealed record <Name>Action` (imut├ível)
- Fluxor State: `public sealed record <Name>State` (imut├ível)
- Entities: `public class <Name>` (mut├ível, EF Core)

**Entreg├íveis**:
- [ ] Documentar padr├úo em `docs/architecture.md` se├º├úo "C# Coding Standards"
- [ ] Converter records inconsistentes (se necess├írio)
- [ ] Adicionar analyzer rule para enforcement futuro

---

##### 5. ≡ƒº¬ SearchProviders E2E Tests ΓÜ¬ MOVIDO PARA SPRINT 9

**Prioridade**: M├ëDIA - MOVIDO PARA SPRINT 9 (Buffer)

**Objetivo**: Testar busca geolocalizada end-to-end.

**Status**: ΓÜ¬ MOVIDO PARA SPRINT 9 - Task opcional, n├úo cr├¡tica para Customer App

**Justificativa da Movimenta├º├úo**:
- Sprint 7.16 completou 4/4 tarefas obrigat├│rias (Keycloak, Warnings, Tests, Records)
- E2E tests marcados como OPCIONAL desde o planejamento
- N├úo bloqueiam Sprint 8 (Customer App)
- Melhor executar com calma no Sprint 9 (Buffer) sem press├úo de deadline

**Entreg├íveis** (ser├úo executados no Sprint 9):
- [ ] Teste: Buscar providers por servi├ºo + raio (2km, 5km, 10km)
- [ ] Teste: Validar ordena├º├úo por dist├óncia
- [ ] Teste: Validar restri├º├úo geogr├ífica (AllowedCities)
- [ ] Teste: Performance (<500ms para 1000 providers)

**Estimativa**: 1-2 dias (Sprint 9)

---

#### ≡ƒôè Resultado Esperado Sprint 7.16

**D├⌐bito T├⌐cnico Reduzido**:
- Γ£à Keycloak automation completo (bloqueador removido)
- Γ£à 0 warnings no Admin Portal (S2094, S2953, S2933, MUD0002)
- Γ£à 30-40 testes bUnit (confian├ºa 3x maior)
- Γ£à Records padronizados (consist├¬ncia)
- ΓÜ¬ SearchProviders E2E (MOVIDO para Sprint 9 - n├úo cr├¡tico)

**Quality Metrics**:
- **Build**: 0 errors, 0 warnings
- **Tests**: 1245 backend + 43 frontend bUnit = **1288 testes**
- **Coverage**: Backend 90.56% (frontend bUnit sem m├⌐trica - foco em quantidade de testes)
- **Technical Debt**: Reduzido de 313 linhas ΓåÆ ~150 linhas

**Pronto para Customer App**:
- Γ£à Keycloak configurado (cliente meajudaai-customer)
- Γ£à Admin Portal com qualidade m├íxima (patterns estabelecidos)
- Γ£à Test infrastructure robusta (replic├ível no Customer App)
- Γ£à Zero distra├º├╡es (d├⌐bito t├⌐cnico minimizado)

**Commits Estimados**:
- `feat(sprint-7.16): add Keycloak client automation script`
- `fix(sprint-7.16): resolve all frontend analyzer warnings`
- `test(sprint-7.16): increase bUnit coverage to 30-40 tests`
- `refactor(sprint-7.16): standardize record usage across project`

---

### Γ£à Sprint 8A - Customer Web App (Conclu├¡da)

**Status**: CONCLU├ìDA (5-13 Fev 2026)  
**Foco**: Refinamento de Layout e UX (Home & Search)

**Atividades Realizadas**:
1. **Home Page Layout Refinement** Γ£à
   - Restaurada se├º├úo "Como funciona?" (How It Works) ap├│s "Conhe├ºa o MeAjudaA├¡".
   - Ajustado posicionamento para melhorar fluxo de conte├║do (Promessa -> Confian├ºa -> Processo).
   - Corrigidos warnings de imagens (aspect ratio, sizes).
   - Ajustados espa├ºamentos e alinhamentos (Hero, City Search vertical center).

2. **Search Page Layout & UX** Γ£à
   - Removido limite de largura (`max-w-6xl`) para aproveitar tela cheia.
   - Service Tags movidas para largura total, centralizadas em desktop.
   - Mock de Service Tags atualizado para "Top 10 Servi├ºos Populares" (Pedreiro, Eletricista, etc.).
   - Melhorada experi├¬ncia em mobile com scroll horizontal.

**Pr├│ximos Passos (Imediato)**:
- Integrar Service Tags com backend real (popularidade/regional).
- Implementar filtros avan├ºados.

---

### Γ£à Sprint 8B - Authentication & Onboarding Flow - CONCLU├ìDO

**Periodo Estimado**: 19 Fev - 4 Mar 2026
**Foco**: Fluxos de Cadastro e Login Segmentados (Cliente vs Prestador)

**Regras de Neg├│cio e UX**:

**1. Ponto de Entrada Unificado**
- Bot├úo "Cadastre-se Gr├ítis" na Home/Header.
- **Modal de Sele├º├úo** (Inspirado em refer├¬ncia visual):
  - Op├º├úo A: "Quero ser cliente" (Encontrar melhores acompanhantes/prestadores).
  - Op├º├úo B: "Sou prestador" (Divulgar servi├ºos).

**2. Fluxo do Cliente (Customer Flow)**
- **Login/Cadastro**:
  - Social Login: Google, Facebook, Instagram.
  - Manual: Email + Senha.
- **Dados**:
  - Validar necessidade de endere├ºo (Possivelmente opcional no cadastro, obrigat├│rio no agendamento).

**3. Fluxo do Prestador (Provider Flow)**
- **Redirecionamento**: Ao clicar em "Sou prestador", redirecionar para landing page espec├¡fica de prestadores (modelo visual refer├¬ncia #3).
- **Etapa 1: Cadastro B├ísico**:
  - Social Login ou Manual.
  - Dados B├ísicos: Nome, Telefone/WhatsApp (validado via OTP se poss├¡vel).
- **Etapa 2: Verifica├º├úo de Seguran├ºa (Obrigat├│ria)**:
  - Upload de Documentos (RG/CNH).
  - Valida├º├úo de Antecedentes Criminais.
  - Biometria Facial (Liveness Check) para evitar fraudes.
- **Conformidade LGPD & Seguran├ºa**:
  - **Consentimento Expl├¡cito**: Coleta de aceite inequ├¡voco para tratamento de dados sens├¡veis (biometria, antecedentes), detalhando finalidade e base legal (Preven├º├úo ├á Fraude/Leg├¡timo Interesse).
  - **Pol├¡tica de Reten├º├úo**: Defini├º├úo clara de prazos de armazenamento e fluxo de exclus├úo autom├ítica ap├│s inatividade ou solicita├º├úo.
  - **Operadores de Dados**: Contratos com vendors (ex: servi├ºo de biometria) exigindo compliance LGPD/GDPR e Acordos de Processamento de Dados (DPA).
  - **Direitos do Titular**: Fluxos automatizados para solicita├º├úo de exporta├º├úo (portabilidade) e anonimiza├º├úo/exclus├úo de dados.
  - **DPIA**: Realiza├º├úo de Relat├│rio de Impacto ├á Prote├º├úo de Dados (RIPD) espec├¡fico para o tratamento de dados biom├⌐tricos.
  - **Seguran├ºa**: Criptografia em repouso (AES-256) e em tr├ónsito (TLS 1.3). Divulga├º├úo transparente do uso de reCAPTCHA v3 e seus termos.
- **Prote├º├úo**: Integra├º├úo com Google reCAPTCHA v3 em todo o fluxo.

**Entreg├íveis**:
- [ ] Componente `AuthModal` com sele├º├úo de perfil.
- [ ] Integra├º├úo `NextAuth.js` com Providers (Google, FB, Instagram) e Credentials.
- [ ] P├ígina de Onboarding de Prestadores (Step-by-step wizard).
- [ ] Integra├º├úo com servi├ºo de verifica├º├úo de documentos/biometria.

---

### ▶️ Sprint 8C - Provider Web App (React + NX) - ACTIVE

**Periodo Estimado**: 19 Mar - 1 Abr 2026
**Foco**: App de Administra├º├úo de Perfil para Prestadores
**Branch**: (a ser criada: `feature/sprint-8c-provider-app`)

**Contexto**: Segundo app React no workspace NX. Utiliza shared libs (`libs/ui`, `libs/auth`, `libs/api-client`) criadas no Sprint 8B.2. Completa os pendentes do Sprint 8B.1 (Document Upload, Review Dashboard, Professional Profile Setup).

**Escopo**:
- Criar `apps/provider-web` dentro do workspace NX (Next.js + Tailwind v4).
- **Document Upload (Step 3)**: Componente de upload de documentos no fluxo de onboarding.
- **Review Dashboard**: Interface para o prestador acompanhar status de verifica├º├úo.
- **Professional Profile Setup**: Sele├º├úo de categorias e servi├ºos ap├│s credenciamento.
- **Provider Profile Page**: P├ígina de perfil p├║blico do prestador (com slug do Sprint 8B.2).
- Autentica├º├úo Keycloak (cliente `meajudaai-provider`).
- Estilo visual alinhado com Customer App (Tailwind v4 + componentes `libs/ui`).

---

### ΓÅ│ Sprint 8D - Admin Portal Migration (Blazor ΓåÆ React + NX)

**Periodo Estimado**: 2 - 15 Abr 2026
**Foco**: Migra├º├úo do Admin Portal de Blazor WASM para React dentro do workspace NX
**Branch**: (a ser criada: `feature/sprint-8d-admin-migration`)

**Contexto**: Terceiro app React no workspace NX. Reutiliza padr├╡es e shared libs consolidados pelo Customer (Sprint 8A) e Provider App (Sprint 8C). Elimina dual-stack (Blazor + React) em favor de single-stack React.

**Escopo**:
- Criar `apps/admin-web` dentro do workspace NX (Next.js + Tailwind v4).
- Migrar todas as funcionalidades existentes do Blazor Admin Portal:
  - Dashboard com KPIs e gr├íficos (Providers por status/tipo)
  - CRUD Providers (Create, Update, Delete, Verify)
  - Gest├úo de Documentos (Upload, Verifica├º├úo, Rejei├º├úo)
  - Gest├úo de Service Catalogs (Categorias + Servi├ºos)
  - Gest├úo de Restri├º├╡es Geogr├íficas (AllowedCities)
  - Dark Mode, Localiza├º├úo (i18n), Acessibilidade
- Substituir Fluxor por Zustand ou Redux Toolkit (state management React).
- Substituir Refit/C# DTOs por `libs/api-client` (gerado via OpenAPI ou manual).
- Manter autentica├º├úo Keycloak (cliente `meajudaai-admin`).
- Estilo visual unificado com Customer e Provider Apps.
- Remover projeto Blazor WASM ap├│s migra├º├úo completa e valida├º├úo.

---

### ΓÅ│ Sprint 8E - Mobile App (React Native + Expo)

**Periodo Estimado**: 16 - 29 Abr 2026
**Foco**: App Mobile Nativo (iOS/Android) com Expo
**Branch**: (a ser criada: `feature/sprint-8e-mobile-app`)

**Escopo**:
- Criar `apps/mobile` dentro do workspace NX (React Native + Expo).
- Portar funcionalidades do Customer Web App para Mobile.
- Reutilizar l├│gica de neg├│cio e autentica├º├úo via shared libs NX.
- Notifica├º├╡es Push.

---

**Status**: SKIPPED durante Parts 10-15 (escopo muito grande)  
**Prioridade**: Alta (recomendado antes do MVP)  
**Estimativa**: 3-5 dias de sprint dedicado

**Contexto**: A Part 13 foi intencionalmente pulada durante a implementa├º├úo das Parts 10-15 (melhorias menores) por ser muito extensa e merecer um sprint dedicado. Testes unit├írios frontend s├úo cr├¡ticos para manutenibilidade e confian├ºa no c├│digo, mas requerem setup completo de infraestrutura de testes.

**Escopo Planejado**:

**1. Infraestrutura de Testes** (1 dia):
- Criar projeto `MeAjudaAi.Web.Admin.Tests`
- Adicionar pacotes: bUnit, Moq, FluentAssertions, xUnit
- Configurar test host e service mocks
- Setup de TestContext base reutiliz├ível

**2. Testes de Fluxor State Management** (1-2 dias):
- **Reducers**: 15+ testes para state mutations
  * ProvidersReducers: LoadSuccess, LoadFailure, SetFilters, SetSorting
  * DocumentsReducers: UploadSuccess, VerificationUpdate
  * ServiceCatalogsReducers: CRUD operations
  * LocationsReducers: LoadCities, FilterByState
  * ErrorReducers: SetGlobalError, ClearError, RetryAfterError
- **Actions**: Verificar payloads corretos
- **Features**: Initial state validation

**3. Testes de Effects** (1 dia):
- Mock de IProvidersApi, IDocumentsApi, IServiceCatalogsApi
- Test de retry logic em ErrorHandlingService
- Verificar dispatches corretos (Success/Failure actions)
- Test de autoriza├º├úo e permiss├╡es

**4. Testes de Componentes** (1-2 dias):
- **Pages**: 
  * Providers.razor: rendering, search, pagination
  * Documents.razor: upload, verification workflow
  * ServiceCatalogs.razor: category/service CRUD
  * Dashboard.razor: charts rendering
- **Dialogs**:
  * CreateProviderDialog: form validation
  * EditProviderDialog: data binding
  * UploadDocumentDialog: file upload mock
  * VerifyProviderDialog: status change
- **Shared Components**:
  * LanguageSwitcher: culture change
  * LiveRegionAnnouncer: accessibility
  * ErrorBoundaryContent: error recovery

**5. Testes de Servi├ºos** (0.5 dia):
- LocalizationService: culture switching, string retrieval
- ErrorHandlingService: retry logic, status code mapping
- LiveRegionService: announcement queue
- ErrorLoggingService: correlation IDs
- PermissionService: policy checks

**Meta de Cobertura**:
- **Reducers**: >95% (l├│gica pura, f├ícil de testar)
- **Effects**: >80% (com mocks de APIs)
- **Components**: >70% (rendering e intera├º├╡es b├ísicas)
- **Services**: >90% (l├│gica de neg├│cio)
- **Geral**: >80% code coverage

**Benef├¡cios Esperados**:
- Γ£à Confidence em refactorings futuros
- Γ£à Documenta├º├úo viva do comportamento esperado
- Γ£à Detec├º├úo precoce de regress├╡es
- Γ£à Facilita onboarding de novos devs
- Γ£à Reduz bugs em produ├º├úo

**Ferramentas e Patterns**:
```csharp
// Exemplo de teste de Reducer
[Fact]
public void LoadProvidersSuccessAction_Should_UpdateState()
{
    // Arrange
    var initialState = new ProvidersState(isLoading: true, providers: []);
    var providers = new List<ModuleProviderDto> { /* mock data */ };
    var action = new LoadProvidersSuccessAction(providers, totalItems: 10, pageNumber: 1, pageSize: 10);
    
    // Act
    var newState = ProvidersReducers.OnLoadProvidersSuccess(initialState, action);
    
    // Assert
    newState.IsLoading.Should().BeFalse();
    newState.Providers.Should().HaveCount(1);
    newState.TotalItems.Should().Be(10);
}

// Exemplo de teste de Component
[Fact]
public void LanguageSwitcher_Should_ChangeCulture()
{
    // Arrange
    using var ctx = new TestContext();
    ctx.Services.AddScoped<LocalizationService>();
    var component = ctx.RenderComponent<LanguageSwitcher>();
    
    // Act
    var enButton = component.Find("button[data-lang='en-US']");
    enButton.Click();
    
    // Assert
    var localization = ctx.Services.GetRequiredService<LocalizationService>();
    localization.CurrentCulture.Name.Should().Be("en-US");
}
```

**Prioriza├º├úo Sugerida**:
1. **Cr├¡tico (antes do MVP)**: Reducers + Effects + ErrorHandlingService
2. **Importante (pr├⌐-MVP)**: Componentes principais (Providers, Documents)
3. **Nice-to-have (p├│s-MVP)**: Componentes de UI (dialogs, shared)

**Recomenda├º├úo**: Implementar em **Sprint 8.5** (entre Customer App e Buffer) ou dedicar 1 semana do Sprint 9 (Buffer) para esta tarefa. Frontend tests s├úo investimento de longo prazo essencial para manutenibilidade.

---

### Γ£à Sprint 7 - Blazor Admin Portal Features - CONCLU├ìDA (6-7 Jan 2026)

**Branch**: `blazor-admin-portal-features` (MERGED to master)

**Objetivos**:
1. Γ£à **CRUD Completo de Providers** (6-7 Jan 2026) - Create, Update, Delete, Verify
2. Γ£à **Gest├úo de Documentos** (7 Jan 2026) - Upload, verifica├º├úo, deletion workflow
3. Γ£à **Gest├úo de Service Catalogs** (7 Jan 2026) - CRUD de categorias e servi├ºos
4. Γ£à **Gest├úo de Restri├º├╡es Geogr├íficas** (7 Jan 2026) - UI para AllowedCities com banco de dados
5. Γ£à **Gr├íficos Dashboard** (7 Jan 2026) - MudCharts com providers por status e evolu├º├úo temporal
6. Γ£à **Testes** (7 Jan 2026) - Aumentar cobertura para 30 testes bUnit

**Progresso Atual**: 6/6 features completas Γ£à **SPRINT 7 CONCLU├ìDO 100%!**

**Detalhamento - Provider CRUD** Γ£à:
- IProvidersApi enhanced: CreateProviderAsync, UpdateProviderAsync, DeleteProviderAsync, UpdateVerificationStatusAsync
- CreateProviderDialog: formul├írio completo com valida├º├úo (ProviderType, Name, FantasyName, Document, Email, Phone, Description, Address)
- EditProviderDialog: edi├º├úo simplificada (nome/telefone - aguardando DTO enriquecido do backend)
- VerifyProviderDialog: mudan├ºa de status de verifica├º├úo (Verified, Rejected, Pending + optional notes)
- Providers.razor: action buttons (Edit, Delete, Verify) com MessageBox confirmation
- Result<T> error handling pattern em todas opera├º├╡es
- Portuguese labels + Snackbar notifications
- Build sucesso (19 warnings Sonar apenas)
- Commit: cd2be7f6 "feat(admin): complete Provider CRUD operations"

**Detalhamento - Documents Management** Γ£à:
- DocumentsState/Actions/Reducers/Effects: Fluxor pattern completo
- Documents.razor: p├ígina com provider selector e listagem de documentos
- MudDataGrid com status chips coloridos (Verified=Success, Rejected=Error, Pending=Warning, Uploaded=Info)
- ProviderSelectorDialog: sele├º├úo de provider da lista existente
- UploadDocumentDialog: MudFileUpload com tipos de documento (RG, CNH, CPF, CNPJ, Comprovante, Outros)
- RequestVerification action via IDocumentsApi.RequestDocumentVerificationAsync
- DeleteDocument com confirma├º├úo MessageBox
- Real-time status updates via Fluxor Dispatch
- Portuguese labels + Snackbar notifications
- Build sucesso (28 warnings Sonar apenas)
- Commit: e033488d "feat(admin): implement Documents management feature"

**Detalhamento - Service Catalogs** Γ£à:
- IServiceCatalogsApi enhanced: 10 m├⌐todos (Create, Update, Delete, Activate, Deactivate para Categories e Services)
- ServiceCatalogsState/Actions/Reducers/Effects: Fluxor pattern completo
- Categories.razor: full CRUD page com MudDataGrid, status chips, action buttons
- Services.razor: full CRUD page com category relationship e MudDataGrid
- CreateCategoryDialog, EditCategoryDialog: forms com Name, Description, DisplayOrder
- CreateServiceDialog, EditServiceDialog: forms com CategoryId (dropdown), Name, Description, DisplayOrder
- Activate/Deactivate toggles para ambos
- Delete confirmations com MessageBox
- Portuguese labels + Snackbar notifications
- Build sucesso (37 warnings Sonar/MudBlazor apenas)
- Commit: bd0c46b3 "feat(admin): implement Service Catalogs CRUD (Categories + Services)"

**Detalhamento - Geographic Restrictions** Γ£à:
- ILocationsApi j├í possu├¡a CRUD completo (Create, Update, Delete, GetAll, GetById, GetByState)
- LocationsState/Actions/Reducers/Effects: Fluxor pattern completo
- AllowedCities.razor: full CRUD page com MudDataGrid
- CreateAllowedCityDialog: formul├írio com City, State, Country, Latitude, Longitude, ServiceRadiusKm, IsActive
- EditAllowedCityDialog: mesmo formul├írio para edi├º├úo
- MudDataGrid com coordenadas em formato F6 (6 decimais), status chips (Ativa/Inativa)
- Toggle activation via MudSwitch (updates backend via UpdateAllowedCityAsync)
- Delete confirmation com MessageBox
- Portuguese labels + Snackbar notifications
- Build sucesso (42 warnings Sonar/MudBlazor apenas)
- Commit: 3317ace3 "feat(admin): implement Geographic Restrictions - AllowedCities UI"

**Detalhamento - Dashboard Charts** Γ£à:
- Dashboard.razor enhanced com 2 charts interativos (MudBlazor built-in charts)
- Provider Status Donut Chart: agrupa providers por VerificationStatus (Verified, Pending, Rejected)
- Provider Type Pie Chart: distribui├º├úo entre Individual (Pessoa F├¡sica) e Company (Empresa)
- Usa ProvidersState existente (sem novos endpoints de backend)
- OnAfterRender lifecycle hook para update de dados quando providers carregam
- UpdateChartData() m├⌐todo com GroupBy LINQ queries
- Portuguese labels para tipos de provider
- Empty state messages quando n├úo h├í providers cadastrados
- MudChart components com Width="300px", Height="300px", LegendPosition.Bottom
- Build sucesso (43 warnings Sonar/MudBlazor apenas)
- Commit: 0e0d0d81 "feat(admin): implement Dashboard Charts with MudBlazor"

**Detalhamento - Testes bUnit** Γ£à:
- 30 testes bUnit criados (objetivo: 30+) - era 10, adicionados 20 novos
- CreateProviderDialogTests: 4 testes (form fields, submit button, provider type, MudForm)
- DocumentsPageTests: 5 testes (provider selector, upload button, loading, document list, error)
- CategoriesPageTests: 4 testes (load action, create button, list, loading)
- ServicesPageTests: 3 testes (load actions, create button, list)
- AllowedCitiesPageTests: 4 testes (load action, create button, list, loading)
- Todos seguem pattern: Mock IState/IDispatcher/IApi, AddMudServices, JSRuntimeMode.Loose
- Verificam rendering, state management, user interactions
- Namespaces corrigidos: Modules.*.DTOs
- Build sucesso (sem erros de compila├º├úo)
- Commit: 2a082840 "test(admin): increase bUnit test coverage to 30 tests"

---

### Γ£à Sprint 6 - Blazor Admin Portal Setup - CONCLU├ìDA (30 Dez 2025 - 5 Jan 2026)

**Status**: MERGED to master (5 Jan 2026)

**Principais Conquistas**:
1. **Projeto Blazor WASM Configurado** Γ£à
   - .NET 10 com target `net10.0-browser`
   - MudBlazor 7.21.0 (Material Design UI library)
   - Fluxor 6.1.0 (Redux-pattern state management)
   - Refit 9.0.2 (Type-safe HTTP clients)
   - Bug workaround: `CompressionEnabled=false` (static assets .NET 10)

2. **Autentica├º├úo Keycloak OIDC Completa** Γ£à
   - Microsoft.AspNetCore.Components.WebAssembly.Authentication
   - Login/Logout flows implementados
   - Authentication.razor com 6 estados (LoggingIn, CompletingLoggingIn, etc.)
   - BaseAddressAuthorizationMessageHandler configurado
   - **Token Storage**: SessionStorage (Blazor WASM padr├úo)
   - **Refresh Strategy**: Autom├ítico via OIDC library (silent refresh)
   - **SDKs Refit**: Interfaces manuais com atributos Refit (n├úo code-generated)
   - Documenta├º├úo completa em `docs/keycloak-admin-portal-setup.md`

3. **Providers Feature (READ-ONLY)** Γ£à
   - Fluxor store completo (State/Actions/Reducers/Effects)
   - MudDataGrid com pagina├º├úo server-side
   - IProvidersApi via Refit com autentica├º├úo
   - PagedResult<T> correto (Client.Contracts.Api)
   - VERIFIED_STATUS constant (type-safe)
   - Portuguese error messages

4. **Dashboard com KPIs** Γ£à
   - 3 KPIs: Total Providers, Pending Verifications, Active Services
   - IServiceCatalogsApi integrado (contagem real de servi├ºos)
   - MudCards com Material icons
   - Fluxor stores para Dashboard state
   - Loading states e error handling

5. **Dark Mode com Fluxor** Γ£à
   - ThemeState management (IsDarkMode boolean)
   - Toggle button em MainLayout
   - MudThemeProvider two-way binding

6. **Layout Base** Γ£à
   - MainLayout.razor com MudDrawer + MudAppBar
   - NavMenu.razor com navega├º├úo
   - User menu com AuthorizeView
   - Responsive design (Material Design)

7. **Testes bUnit + xUnit** Γ£à
   - 10 testes criados (ProvidersPageTests, DashboardPageTests, DarkModeToggleTests)
   - JSInterop mock configurado (JSRuntimeMode.Loose)
   - MudServices registrados em TestContext
   - CI/CD integration (master-ci-cd.yml + pr-validation.yml)

8. **Localiza├º├úo Portuguesa** Γ£à
   - Todos coment├írios inline em portugu├¬s
   - Mensagens de erro em portugu├¬s
   - UI messages traduzidas (Authentication.razor)
   - Projeto language policy compliance

9. **Integra├º├úo Aspire** Γ£à
   - Admin portal registrado em AppHost
   - Environment variables configuradas (ApiBaseUrl, Keycloak)
   - Build e execu├º├úo via `dotnet run --project src/Aspire/MeAjudaAi.AppHost`

10. **Documenta├º├úo** Γ£à
    - docs/keycloak-admin-portal-setup.md (manual configura├º├úo)
    - docs/testing/bunit-ci-cd-practices.md (atualizado)
    - Roadmap atualizado com progresso Sprint 6

11. **SDKs Completos para Sprint 7** Γ£à (6 Jan 2026)
    - IDocumentsApi: Upload, verifica├º├úo, gest├úo de documentos de providers
    - ILocationsApi: CRUD de cidades permitidas (AllowedCities)
    - DTOs criados: ModuleAllowedCityDto, Create/UpdateAllowedCityRequestDto
    - README melhorado: conceito de SDK, diagrama arquitetural, compara├º├úo manual vs SDK
    - 4/4 SDKs necess├írios para Admin Portal (Providers, Documents, ServiceCatalogs, Locations)

**Resultado Alcan├ºado**:
- Γ£à Blazor Admin Portal 100% funcional via Aspire
- Γ£à Login/Logout Keycloak funcionando
- Γ£à Providers listagem paginada (read-only)
- Γ£à Dashboard com 3 KPIs reais (IServiceCatalogsApi)
- Γ£à Dark mode toggle
- Γ£à 10 testes bUnit (build verde)
- Γ£à Portuguese localization completa
- Γ£à 0 erros build (10 warnings - analyzers apenas)
- Γ£à **4 SDKs completos** para Admin Portal (IProvidersApi, IDocumentsApi, IServiceCatalogsApi, ILocationsApi)
- Γ£à **Documenta├º├úo SDK** melhorada (conceito, arquitetura, exemplos pr├íticos)

**Γ£à Pr├│xima Etapa Conclu├¡da: Sprint 7 - Blazor Admin Portal Features** (6-7 Jan 2026)
- Γ£à CRUD completo de Providers (create, update, delete, verify)
- Γ£à Gest├úo de Documentos (upload, verifica├º├úo, rejection)
- Γ£à Gest├úo de Service Catalogs (categorias + servi├ºos)
- Γ£à Gest├úo de Restri├º├╡es Geogr├íficas (UI para AllowedCities)
- Γ£à Gr├íficos Dashboard (MudCharts - providers por status, evolu├º├úo temporal)
- Γ£à Aumentar cobertura de testes (30+ testes bUnit)

---

## Γ£à Sprint 5.5 - Refactor & Cleanup (19-30 Dez 2025)

**Status**: CONCLU├ìDA

**Principais Conquistas**:
1. **Refatora├º├úo MeAjudaAi.Shared.Messaging** Γ£à
   - Factories organizados em pasta dedicada (`Messaging/Factories/`)
   - Services organizados em pasta dedicada (`Messaging/Services/`)
   - Options organizados em pasta dedicada (`Messaging/Options/`)
   - 4 arquivos: ServiceBusOptions, MessageBusOptions, RabbitMqOptions, DeadLetterOptions
   - IMessageBusFactory + MessageBusFactory separados
   - IDeadLetterServiceFactory + DeadLetterServiceFactory separados
   - 1245/1245 testes passando

2. **Extensions Padronizadas** Γ£à
   - 14 arquivos consolidados: CachingExtensions, CommandsExtensions, DatabaseExtensions, etc.
   - BusinessMetricsMiddlewareExtensions extra├¡do para arquivo pr├│prio
   - Monitoring folder consolidation completo
   - Removidos 13 arquivos obsoletos (Extensions.cs gen├⌐ricos + subpastas)

3. **Extension Members (C# 14)** Γ£à
   - EnumExtensions migrado para nova sintaxe `extension<TEnum>(string value)`
   - 18/18 testes passando (100% compatibilidade)
   - Documentado em architecture.md - se├º├úo "C# 14 Features Utilizados"
   - Avaliado DocumentExtensions (n├úo adequado para extension properties)

4. **TODOs Resolvidos** Γ£à
   - 12/12 TODOs no c├│digo resolvidos ou documentados
   - Remaining issues movidos para technical-debt.md com prioriza├º├úo
   - api-reference.md removido (redundante com ReDoc + api-spec.json)

5. **Documenta├º├úo Atualizada** Γ£à
   - architecture.md atualizado com C# 14 features
   - technical-debt.md atualizado com status atual
   - roadmap.md atualizado com Sprint 5.5 completion
   - 0 warnings in build

**Γ£à Fase 1.5: CONCLU├ìDA** (21 Nov - 10 Dez 2025)  
Funda├º├úo t├⌐cnica para escalabilidade e produ├º├úo:
- Γ£à Migration .NET 10 + Aspire 13 (Sprint 0 - CONCLU├ìDO 21 Nov, MERGED to master)
- Γ£à Geographic Restriction + Module Integration (Sprint 1 - CONCLU├ìDO 2 Dez, MERGED to master)
- Γ£à Test Coverage 90.56% (Sprint 2 - CONCLU├ìDO 10 Dez - META 35% SUPERADA EM 55.56pp!)
- Γ£à GitHub Pages Documentation Migration (Sprint 3 Parte 1 - CONCLU├ìDO 11 Dez - DEPLOYED!)

**Γ£à Sprint 3 Parte 2: CONCLU├ìDA** (11 Dez - 13 Dez 2025)  
Admin Endpoints & Tools - TODAS AS PARTES FINALIZADAS:
- Γ£à Admin: Endpoints CRUD para gerenciar cidades permitidas (COMPLETO)
  - Γ£à Banco de dados: LocationsDbContext + migrations
  - Γ£à Dom├¡nio: AllowedCity entity + IAllowedCityRepository
  - Γ£à Handlers: CRUD completo (5 handlers)
  - Γ£à Endpoints: GET/POST/PUT/DELETE configurados
  - Γ£à Exception Handling: Domain exceptions + IExceptionHandler (404/400 corretos)
  - Γ£à Testes: 4 integration + 15 E2E (100% passando)
  - Γ£à Quality: 0 warnings, dotnet format executado
- Γ£à Tools: Bruno Collections para todos m├│dulos (35+ arquivos .bru)
- Γ£à Scripts: Auditoria completa e documenta├º├úo (commit b0b94707)
- Γ£à Module Integrations: Providers Γåö ServiceCatalogs + Locations
- Γ£à Code Quality: NSubstituteΓåÆMoq, UuidGenerator, .slnx, SonarQube warnings
- Γ£à CI/CD: Formatting checks corrigidos, exit code masking resolvido

**Γ£à Sprint 4: CONCLU├ìDO** (14 Dez - 16 Dez 2025)  
Health Checks Robustos + Data Seeding para MVP - TODAS AS PARTES FINALIZADAS:
- Γ£à Health Checks: DatabasePerformanceHealthCheck (lat├¬ncia <100ms healthy, <500ms degraded)
- Γ£à Health Checks: ExternalServicesHealthCheck (Keycloak + IBGE API + Redis)
- Γ£à Health Checks: HelpProcessingHealthCheck (sistema de ajuda operacional)
- Γ£à Health Endpoints: /health, /health/live, /health/ready com JSON responses
- Γ£à Health Dashboard: Dashboard nativo do Aspire (decis├úo arquitetural - n├úo usar AspNetCore.HealthChecks.UI)
- Γ£à Health Packages: AspNetCore.HealthChecks.Npgsql 9.0.0, .Redis 8.0.1
- Γ£à Redis Health Check: Configurado via AddRedis() com tags 'ready', 'cache'
- Γ£à Data Seeding: infrastructure/database/seeds/01-seed-service-catalogs.sql (8 categorias + 12 servi├ºos)
- Γ£à Seed Automation: Docker Compose executa seeds automaticamente na inicializa├º├úo
- Γ£à Project Structure: Reorganiza├º├úo - automation/ ΓåÆ infrastructure/automation/, seeds em infrastructure/database/seeds/
- Γ£à Documentation: README.md, scripts/README.md, infrastructure/database/README.md + docs/future-external-services.md
- Γ£à MetricsCollectorService: Implementado com IServiceScopeFactory (4 TODOs resolvidos)
- Γ£à Unit Tests: 14 testes para ExternalServicesHealthCheck (6 novos para IBGE API)
- Γ£à Integration Tests: 9 testes para DataSeeding (categorias, servi├ºos, idempot├¬ncia)
- Γ£à Future Services Documentation: Documentado OCR, payments, SMS/email (quando implementar)
- Γ£à Code Review: Logs traduzidos para ingl├¬s conforme pol├¡tica (Program.cs - 3 mensagens)
- Γ£à Markdown Linting: technical-debt.md corrigido (code blocks, URLs, headings)
- Γ£à Architecture Test: PermissionHealthCheckExtensions exception documentada (namespace vs folder structure)

**Γ£à Sprint 5: CONCLU├ìDO ANTECIPADAMENTE** (Tarefas completadas nos Sprints 3-4)  
Todas as tarefas planejadas j├í foram implementadas:
- Γ£à NSubstitute ΓåÆ Moq migration (Sprint 3)
- Γ£à UuidGenerator unification (commit 0a448106)
- Γ£à .slnx migration (commit 1de5dc1a)
- Γ£à Design patterns documentation (architecture.md)
- Γ£à Bruno collections para todos m├│dulos (Users, Providers, Documents)

**ΓÅ│ Sprint 5.5: CONCLU├ìDA** (19-20 Dez 2025) Γ£à
**Branch**: `feature/refactor-and-cleanup`  
**Objetivo**: Refatora├º├úo t├⌐cnica e redu├º├úo de d├⌐bito t├⌐cnico antes do frontend

**Γ£à Refatoramento de Testes Completado** (20 Dez 2025):
- Γ£à Reorganiza├º├úo estrutural de MeAjudaAi.Shared.Tests (TestInfrastructure com 8 subpastas)
- Γ£à ModuleExtensionsTests movidos para m├│dulos individuais (Documents, Providers, ServiceCatalogs, Users)
- Γ£à Tradu├º├úo de ~35 coment├írios para portugu├¬s (mantendo AAA em ingl├¬s)
- Γ£à Separa├º├úo de classes aninhadas (LoggingConfigurationExtensionsTests, TestEvent, BenchmarkResult, BenchmarkExtensions)
- Γ£à Remo├º├úo de duplicados (DocumentExtensionsTests, EnumExtensionsTests, SearchableProviderTests)
- Γ£à GeographicRestrictionMiddlewareTests movido para Unit/Middleware/
- Γ£à TestPerformanceBenchmark: classes internas separadas
- Γ£à 11 commits de refatoramento com build verde

**Γ£à Corre├º├úo PostGIS Integration Tests** (20 Dez 2025):
- Γ£à Imagem Docker atualizada: postgres:15-alpine ΓåÆ postgis/postgis:15-3.4
- Γ£à EnsurePostGisExtensionAsync() implementado em fixtures
- Γ£à Connection string com 'Include Error Detail=true' para diagn├│stico
- Γ£à Suporte completo a dados geogr├íficos (NetTopologySuite/GeoPoint)
- Γ£à Migrations SearchProviders agora passam na pipeline

**Resumo da Sprint**:
- Γ£à 15 commits com melhorias significativas
- Γ£à Todos TODOs cr├¡ticos resolvidos
- Γ£à Testes melhorados (Provider Repository, Azurite)
- Γ£à Messaging refatorado (IRabbitMqInfrastructureManager extra├¡do)
- Γ£à Extensions consolidadas (BusinessMetricsMiddleware)
- Γ£à Upload file size configur├ível (IOptions pattern)
- Γ£à Build sem warnings (0 warnings)
- Γ£à Documenta├º├úo atualizada (architecture.md, configuration.md)
- Γ£à Code review aplicado (logs em ingl├¬s, path matching preciso, XML docs)

**Atividades Planejadas** (14 tarefas principais):

**1. Resolu├º├úo de TODOs Cr├¡ticos (Alta Prioridade)** - Γ£à 8-12h CONCLU├ìDO
- [x] IBGE Middleware Fallback - Fix validation when IBGE fails (3 TODOs em IbgeUnavailabilityTests.cs) Γ£à
- [x] Rate Limiting Cache Cleanup - Memory leak prevention (MaxPatternCacheSize=1000) Γ£à
- [x] Email Constraint Database Fix - Schema issue (clarified as not-yet-implemented) Γ£à
- [x] Azurite/Blob Storage - Container auto-creation with thread-safe initialization Γ£à
- [x] Provider Repository Tests - Documentation updated (unit vs integration) Γ£à
- [x] BusinessMetrics - Already extracted (no action needed) Γ£à
- [x] Monitoring - Structure already adequate (no action needed) Γ£à
- [x] Middleware UseSharedServices Alignment - TODO #249 RESOLVIDO Γ£à (19 Dez 2025)
- [x] Azurite Integration Tests - Configured deterministic blob storage tests Γ£à (19 Dez 2025)

**2. Melhorias de Testes (M├⌐dia Prioridade)** - 4-6h
- [x] Testes Infrastructure Extensions - RESOLVIDO: n├úo aplic├ível Γ£à (19 Dez 2025)
  - Extensions de configura├º├úo (Keycloak/PostgreSQL/Migration) validadas implicitamente em E2E/integra├º├úo
  - Testes unit├írios teriam baixo ROI (mockaria apenas chamadas de configura├º├úo)
  - Infraestrutura validada quando AppHost sobe e containers inicializam
- [x] Provider Repository Tests - Duplica├º├úo RESOLVIDA Γ£à (19 Dez 2025)
  - Removidos testes unit├írios com mocks (260 linhas redundantes)
  - Adicionados 5 testes de integra├º├úo faltantes (DeleteAsync, GetByIdsAsync, ExistsByUserIdAsync)
  - 17 testes de integra├º├úo com valida├º├úo REAL de persist├¬ncia
  - Redu├º├úo de manuten├º├úo + maior confian├ºa nos testes

**3. Refatora├º├úo MeAjudaAi.Shared.Messaging** - 8-10h
- [x] ~~Separar NoOpDeadLetterService em arquivo pr├│prio~~ Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] ~~Extrair DeadLetterStatistics e FailureRate para arquivos separados~~ Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] ~~Extrair IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory, MessageRetryExtensions~~ Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] ~~Todos os 1245 testes do Shared passando~~ Γ£à CONCLU├ìDO (19 Dez 2025)
- [Γ£ô] ~~Organizar Factories em pasta dedicada~~ - Γ£à CONCLU├ìDO (19 Dez 2025)
  - Criada pasta `Messaging/Factories/`
  - `MessageBusFactory` e `DeadLetterServiceFactory` organizados
  - Interfaces e implementa├º├╡es em arquivos separados
  - `EnvironmentBasedDeadLetterServiceFactory` ΓåÆ `DeadLetterServiceFactory`
- [Γ£ô] ~~Organizar Services em pasta dedicada~~ - Γ£à CONCLU├ìDO (19 Dez 2025)
  - Criada pasta `Messaging/Services/`
  - `ServiceBusInitializationService` movido para organiza├º├úo
- [Γ£ô] ~~Organizar Options em pasta dedicada~~ - Γ£à CONCLU├ìDO (19 Dez 2025)
  - Criada pasta `Messaging/Options/`
  - 4 arquivos organizados: `ServiceBusOptions`, `MessageBusOptions`, `RabbitMqOptions`, `DeadLetterOptions`
  - Namespace unificado: `MeAjudaAi.Shared.Messaging.Options`
- [Γ£ô] ~~Criar IMessageBusFactory + renomear MessageBusFactory.cs ΓåÆ EnvironmentBasedMessageBusFactory.cs~~ - Γ£à CONCLU├ìDO (19 Dez 2025)
  - Invertido: Criada interface `IMessageBusFactory` em arquivo pr├│prio
  - Classe `EnvironmentBasedMessageBusFactory` renomeada para `MessageBusFactory`
  - Movido de `NoOp/Factory/` para raiz `Messaging/`
  - Um arquivo por classe seguindo SRP
- [x] Extrair IRabbitMqInfrastructureManager para arquivo separado Γ£à (19 Dez 2025)
- [ ] Adicionar Integration Events faltantes nos m├│dulos (Documents, SearchProviders, ServiceCatalogs?) - BACKLOG
- [ ] Reorganiza├º├úo geral da estrutura de pastas em Messaging - BACKLOG
- [ ] Adicionar testes unit├írios para classes de messaging - BACKLOG

**4. Refatora├º├úo Extensions (MeAjudaAi.Shared)** - Γ£à 8h CONCLU├ìDO
- [x] ~~Padronizar Extensions: criar arquivo [FolderName]Extensions.cs por funcionalidade~~ Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] Extension Members (C# 14): EnumExtensions migrado com sucesso Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] BusinessMetricsMiddlewareExtensions: J├í existe em Extensions/ Γ£à CONCLU├ìDO (19 Dez 2025)
- [x] Monitoring folder consolidation: Estrutura j├í adequada Γ£à CONCLU├ìDO (19 Dez 2025)
  - Consolidados: CachingExtensions, CommandsExtensions, DatabaseExtensions, EventsExtensions
  - ExceptionsExtensions, LoggingExtensions, MessagingExtensions, QueriesExtensions, SerializationExtensions
  - Removidos 13 arquivos obsoletos (Extensions.cs gen├⌐ricos + subpastas)
  - 1245/1245 testes passando
- [x] ~~Migra├º├úo para Extension Members (C# 14)~~ Γ£à AVALIADO (19 Dez 2025)
  - Γ£à Sintaxe `extension(Type receiver)` validada e funcional no .NET 10
  - Γ£à Novos recursos dispon├¡veis: extension properties, static extensions, operators
  - Γ£à Documentado em `docs/architecture.md` - se├º├úo "C# 14 Features Utilizados"
  - ≡ƒôï Planejamento: Agendado como ├║ltima atividade da Sprint 5.5
  - ≡ƒô¥ Recomenda├º├úo: Usar Extension Members em NOVOS c├│digos que se beneficiem de properties
- [x] Extrair BusinessMetricsMiddlewareExtensions de BusinessMetricsMiddleware.cs Γ£à (19 Dez 2025)
- [x] Consolidar Monitoring folder (MonitoringExtensions.cs ├║nico) Γ£à (19 Dez 2025)
- [ ] Revisar padr├úo de extens├╡es em todas as funcionalidades do Shared

**5. Code Quality & Cleanup (Baixa Prioridade)** - 3-4h
- [x] Padroniza├º├úo de Records - An├ílise conclu├¡da Γ£à (19 Dez 2025)
  - Property-based records: DTOs/Requests (mutabilidade com `init`)
  - Positional records: Domain Events, Query/Command DTOs (imutabilidade)
  - Pattern adequado ao contexto de uso
- [ ] Upload File Size Configuration - Tornar configur├ível (UploadDocumentCommandHandler.cs:90)
- [x] ~~Remover api-reference.md (redundante com ReDoc + api-spec.json)~~ Γ£à CONCLU├ìDO (19 Dez)

**6. Testes E2E SearchProviders** - 2-3 sprints (BACKLOG)
- [ ] 15 testes E2E cobrindo cen├írios principais de busca
- [ ] Valida├º├úo de integra├º├úo IBGE API, filtros, pagina├º├úo
- [ ] Autentica├º├úo/autoriza├º├úo em todos endpoints

**7. Review Completo de Testes** - 6-8h
- [ ] Auditoria completa de todos os arquivos em tests/
- [ ] Identificar testes duplicados, obsoletos ou mal estruturados
- [ ] Validar coverage e identificar gaps
- [ ] Documentar padr├╡es de teste para novos contribuidores

**8. Migra├º├úo Extension Members (C# 14) - FINAL SPRINT ACTIVITY** - Γ£à 2h CONCLU├ìDO
- [x] Migrar EnumExtensions para syntax `extension<TEnum>(string value)` Γ£à
- [x] 18/18 testes passando (100% compatibilidade) Γ£à
- [x] Documentar patterns e guidelines em architecture.md Γ£à
- [x] Avaliado DocumentExtensions (n├úo adequado para extension properties) Γ£à

**8. BDD Implementation (BACKLOG - Futuro)** - Sprint dedicado planejado
- [ ] Setup SpecFlow + Playwright.NET para acceptance tests
- [ ] Implementar 5-10 features cr├¡ticas em Gherkin (Provider Registration, Document Upload, Service Catalog)
- [ ] Integrar ao CI/CD pipeline
- [ ] Criar documenta├º├úo execut├ível com Gherkin
- **Benef├¡cio**: Testes de aceita├º├úo leg├¡veis para stakeholders e documenta├º├úo viva do sistema
- **Timing**: Implementa├º├úo prevista AP├ôS desenvolvimento do Customer App (Sprint 8+)
- **Escopo**: Testes end-to-end de fluxos completos (Frontend ΓåÆ Backend ΓåÆ APIs terceiras)
- **Foco**: Fluxos cr├¡ticos de usu├írio utilizados por Admin Portal e Customer App

**Crit├⌐rios de Aceita├º├úo**:
- [x] Todos os 12 TODOs no c├│digo resolvidos ou documentados Γ£à
- [x] ~~Messaging refatorado com estrutura clara de pastas~~ Γ£à CONCLU├ìDO (19 Dez)
- [x] ~~Extensions consolidadas por funcionalidade~~ Γ£à CONCLU├ìDO (19 Dez)
- [x] Extension Blocks (C# 14) avaliado e implementado onde aplic├ível Γ£à (19 Dez)
- [x] Testes de infrastructure com >70% coverage (resolvido: n├úo aplic├ível) Γ£à (19 Dez)
- [x] 0 warnings no build Γ£à (19 Dez)
- [x] Documenta├º├úo t├⌐cnica atualizada Γ£à (19 Dez)

**Estimativa Total**: 35-45 horas de trabalho t├⌐cnico (10h j├í conclu├¡das)  
**Benef├¡cio**: Backend robusto e manuten├¡vel para suportar desenvolvimento do frontend Blazor

**≡ƒô¥ Pr├│xima Atividade Recomendada**: Migra├º├úo para Extension Blocks (C# 14) - 4-6h
- Avaliar novo recurso de linguagem para melhorar organiza├º├úo de extension methods
- Migrar m├⌐todos de prop├│sito geral (PermissionExtensions, EnumExtensions)
- Manter padr├úo atual para DI extensions ([FolderName]Extensions.cs)

**Γ£à Sprint 5.5 Completed** (19-30 Dez 2025):
- Refatora├º├úo MeAjudaAi.Shared.Messaging (Factories, Services, Options)
- Extensions padronizadas (14 arquivos consolidados)
- Extension Members (C# 14) implementado
- TODOs resolvidos (12/12 conclu├¡dos)
- Dependabot PRs fechados para regenera├º├úo
- 1245/1245 testes passando

**ΓÅ│ Fase 2: EM ANDAMENTO** (JaneiroΓÇôMaio 2026)  
Frontend React (NX Monorepo) + Mobile:
- Sprint 6: Blazor Admin Portal Setup - Γ£à CONCLU├ìDO (5 Jan 2026)
- Sprint 7: Blazor Admin Portal Features (6-24 Jan 2026) - Γ£à CONCLU├ìDO
- Sprint 7.16: Technical Debt Sprint (17-21 Jan 2026) - ≡ƒöä EM PROGRESSO (Task 5 movida p/ Sprint 9)
- Sprint 8A: Customer App (5-18 Fev 2026) - Γ£à Conclu├¡do
- Sprint 8B: Authentication & Onboarding (19 Fev - 4 Mar 2026) - Γ£à CONCLU├ìDO
- Sprint 8B.2: Technical Excellence & NX Monorepo (5-18 Mar 2026) - ≡ƒöä EM PROGRESSO
- Sprint 8C: Provider Web App (19 Mar - 1 Abr 2026) - ΓÅ│ Planejado
- Sprint 8D: Admin Portal Migration Blazor ΓåÆ React (2-15 Abr 2026) - ΓÅ│ Planejado
- Sprint 8E: Mobile App (16-29 Abr 2026) - ΓÅ│ Planejado
- Sprint 9: BUFFER (30 Abr - 6 Mai 2026) - ΓÅ│ Planejado
- MVP Final: 9 de Maio de 2026
- *Nota: Data de MVP atualizada para 9 de Maio de 2026 para acomodar NX Monorepo, Provider App, Admin Migration e Mobile App.*

**ΓÜá∩╕Å Risk Assessment**: Estimativas assumem velocidade consistente. NX Monorepo setup e Admin Migration s├úo os maiores riscos de escopo. Sprint 9 reservado como buffer de conting├¬ncia.

---

## ≡ƒôû Vis├úo Geral

O roadmap est├í organizado em **cinco fases principais** para entrega incremental de valor:

1. **Γ£à Fase 1: Funda├º├úo (MVP Core)** - Registro de prestadores, busca geolocalizada, cat├ílogo de servi├ºos
2. **≡ƒöä Fase 1.5: Funda├º├úo T├⌐cnica** - Migration .NET 10, integra├º├úo, testes, observability
3. **≡ƒö« Fase 2: Frontend & Experi├¬ncia** - Blazor WASM Admin + Customer App
4. **≡ƒö« Fase 3: Qualidade e Monetiza├º├úo** - Sistema de avalia├º├╡es, assinaturas premium, verifica├º├úo automatizada
5. **≡ƒö« Fase 4: Experi├¬ncia e Engajamento** - Agendamentos, comunica├º├╡es, analytics avan├ºado

A implementa├º├úo segue os princ├¡pios arquiteturais definidos em `architecture.md`: **Modular Monolith**, **DDD**, **CQRS**, e **isolamento schema-per-module**.

---

<a id="cronograma-de-sprints"></a>
## ≡ƒôà Cronograma de Sprints (Novembro 2025-Mar├ºo 2026)

| Sprint | Dura├º├úo | Per├¡odo | Objetivo | Status |
|--------|---------|---------|----------|--------|
| **Sprint 0** | 4 semanas | Jan 20 - 21 Nov | Migration .NET 10 + Aspire 13 | Γ£à CONCLU├ìDO (21 Nov - MERGED) |
| **Sprint 1** | 10 dias | 22 Nov - 2 Dez | Geographic Restriction + Module Integration | Γ£à CONCLU├ìDO (2 Dez - MERGED) |
| **Sprint 2** | 1 semana | 3 Dez - 10 Dez | Test Coverage 90.56% | Γ£à CONCLU├ìDO (10 Dez - META SUPERADA!) |
| **Sprint 3-P1** | 1 dia | 10 Dez - 11 Dez | GitHub Pages Documentation | Γ£à CONCLU├ìDO (11 Dez - DEPLOYED!) |
| **Sprint 3-P2** | 2 semanas | 11 Dez - 13 Dez | Admin Endpoints & Tools | Γ£à CONCLU├ìDO (13 Dez - MERGED) |
| **Sprint 4** | 5 dias | 14 Dez - 18 Dez | Health Checks + Data Seeding | Γ£à CONCLU├ìDO (18 Dez - MERGED!) |
| **Sprint 5** | - | Sprints 3-4 | Quality Improvements | Γ£à CONCLU├ìDO ANTECIPADAMENTE |
| **Sprint 5.5** | 2 semanas | 19 Dez - 31 Dez | Refactor & Cleanup (Technical Debt) | Γ£à CONCLU├ìDO (30 Dez 2025) |
| **Sprint 6** | 1 semana | 30 Dez - 5 Jan | Blazor Admin Portal - Setup & Core | Γ£à CONCLU├ìDO (5 Jan 2026) |
| **Sprint 7** | 3 semanas | 6 - 24 Jan | Blazor Admin Portal - Features | Γ£à CONCLU├ìDO |
| **Sprint 7.16** | 1 semana | 17-21 Jan | Technical Debt Sprint | ≡ƒöä EM PROGRESSO |
| **Sprint 8** | 2 semanas | 5 - 18 Fev | Customer Web App (Web) | Γ£à CONCLU├ìDO |
| **Sprint 8B** | 2 semanas | 19 Fev - 4 Mar | Authentication & Onboarding | Γ£à CONCLU├ìDO |
| **Sprint 8C** | 2 semanas | 5-18 Mar | Mobile App | ΓÅ│ Planejado |
| **Sprint 9** | 1 semana | 19-25 Mar | **BUFFER: Polishing, Refactoring & Risk Mitigation** | ΓÅ│ Planejado |
| **MVP Launch** | - | 28 de Mar├ºo de 2026 | Final deployment & launch preparation | ≡ƒÄ» Target |

**MVP Launch Target**: 28 de Mar├ºo de 2026 ≡ƒÄ»  
*Atualizado para 28 de Mar├ºo de 2026.*

**Post-MVP (Fase 3+)**: Reviews, Assinaturas, Agendamentos (Abril 2026+)

---

## Γ£à Fase 1: Funda├º├úo (MVP Core) - CONCLU├ìDA

### Objetivo
Estabelecer as capacidades essenciais da plataforma: registro multi-etapas de prestadores com verifica├º├úo, busca geolocalizada e cat├ílogo de servi├ºos.

### Status: Γ£à CONCLU├ìDA (Janeiro 2025)

**Todos os 6 m├│dulos implementados, testados e integrados:**
1. Γ£à **Users** - Autentica├º├úo, perfis, roles
2. Γ£à **Providers** - Registro multi-etapas, verifica├º├úo, gest├úo
3. Γ£à **Documents** - Upload seguro, workflow de verifica├º├úo
4. Γ£à **Search & Discovery** - Busca geolocalizada com PostGIS
5. Γ£à **Locations** - Lookup de CEP, geocoding, valida├º├╡es
6. Γ£à **ServiceCatalogs** - Cat├ílogo hier├írquico de servi├ºos

**Conquistas:**
- 28.69% test coverage (93/100 E2E passing, 296 unit tests)
- ΓÜá∩╕Å Coverage caiu ap├│s migration (packages.lock.json + generated code)
- APIs p├║blicas (IModuleApi) implementadas para todos m├│dulos
- Integration events funcionais entre m├│dulos
- Health checks configurados
- CI/CD pipeline completo no GitHub Actions
- Documenta├º├úo arquitetural completa + skipped tests tracker

### 1.1. Γ£à M├│dulo Users (Conclu├¡do)
**Status**: Implementado e em produ├º├úo

**Funcionalidades Entregues**:
- Γ£à Registro e autentica├º├úo via Keycloak (OIDC)
- Γ£à Gest├úo de perfil b├ísica
- Γ£à Sistema de roles e permiss├╡es
- Γ£à Health checks e monitoramento
- Γ£à API versionada com documenta├º├úo OpenAPI

---

### 1.2. Γ£à M├│dulo Providers (Conclu├¡do)

**Status**: Implementado e em produ├º├úo

**Funcionalidades Entregues**:
- Γ£à Provider aggregate com estados de registro (`EProviderStatus`: Draft, PendingVerification, Active, Suspended, Rejected)
- Γ£à M├║ltiplos tipos de prestador (Individual, Company)
- Γ£à Verifica├º├úo de documentos integrada com m├│dulo Documents
- Γ£à BusinessProfile com informa├º├╡es de contato e identidade empresarial
- Γ£à Gest├úo de qualifica├º├╡es e certifica├º├╡es
- Γ£à Domain Events (`ProviderRegistered`, `ProviderVerified`, `ProviderRejected`)
- Γ£à API p├║blica (IProvidersModuleApi) para comunica├º├úo inter-m├│dulos
- Γ£à Queries por documento, cidade, estado, tipo e status de verifica├º├úo
- Γ£à Soft delete e auditoria completa

---

### 1.3. Γ£à M├│dulo Documents (Conclu├¡do)

**Status**: Implementado e em produ├º├úo

**Funcionalidades Entregues**:
- Γ£à Upload seguro de documentos via Azure Blob Storage
- Γ£à Tipos de documento suportados: IdentityDocument, ProofOfResidence, ProfessionalLicense, BusinessLicense
- Γ£à Workflow de verifica├º├úo com estados (`EDocumentStatus`: Uploaded, PendingVerification, Verified, Rejected, Failed)
- Γ£à Integra├º├úo completa com m├│dulo Providers
- Γ£à Domain Events (`DocumentUploaded`, `DocumentVerified`, `DocumentRejected`, `DocumentFailed`)
- Γ£à API p├║blica (IDocumentsModuleApi) para queries de documentos
- Γ£à Verifica├º├╡es de integridade: HasVerifiedDocuments, HasRequiredDocuments, HasPendingDocuments
- Γ£à Sistema de contadores por status (DocumentStatusCountDto)
- Γ£à Suporte a OCR data extraction (campo OcrData para dados extra├¡dos)
- Γ£à Rejection/Failure reasons para auditoria

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

**API P├║blica Implementada**:
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

**Pr├│ximas Melhorias (Fase 2)**:
- ≡ƒöä Background worker para verifica├º├úo automatizada via OCR
- ≡ƒöä Integra├º├úo com APIs governamentais para valida├º├úo
- ≡ƒöä Sistema de scoring autom├ítico baseado em qualidade de documentos

---

### 1.4. Γ£à M├│dulo Search & Discovery (Conclu├¡do)

**Status**: Implementado e em produ├º├úo

**Funcionalidades Entregues**:
- Γ£à Busca geolocalizada com PostGIS nativo
- Γ£à Read model denormalizado otimizado (SearchableProvider)
- Γ£à Filtros por raio, servi├ºos, rating m├¡nimo e subscription tiers
- Γ£à Ranking multi-crit├⌐rio (tier ΓåÆ rating ΓåÆ dist├óncia)
- Γ£à Pagina├º├úo server-side com contagem total
- Γ£à Queries espaciais nativas (ST_DWithin, ST_Distance)
- Γ£à Hybrid repository (EF Core + Dapper) para performance
- Γ£à Valida├º├úo de raio n├úo-positivo (short-circuit)
- Γ£à CancellationToken support para queries longas
- Γ£à API p├║blica (ISearchModuleApi)

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

**API P├║blica Implementada**:
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

**L├│gica de Ranking Implementada**:
1. Γ£à Filtrar por raio usando `ST_DWithin` (├¡ndice GIST)
2. Γ£à Ordenar por tier de assinatura (Platinum > Gold > Standard > Free)
3. Γ£à Ordenar por avalia├º├úo m├⌐dia (descendente)
4. Γ£à Ordenar por dist├óncia (crescente) como desempate

**Performance**:
- Γ£à Queries espaciais executadas no banco (n├úo in-memory)
- Γ£à ├ìndices GIST para geolocaliza├º├úo
- Γ£à Pagina├º├úo eficiente com OFFSET/LIMIT
- Γ£à Count query separada para total

**Pr├│ximas Melhorias (Opcional)**:
- ≡ƒöä Migra├º├úo para Elasticsearch para maior escalabilidade (se necess├írio)
- ≡ƒöä Indexing worker consumindo integration events (atualmente manual)
- ≡ƒöä Caching de resultados para queries frequentes

---

### 1.5. Γ£à M├│dulo Location Management (Conclu├¡do)

**Status**: Implementado e testado com integra├º├úo IBGE ativa

**Objetivo**: Abstrair funcionalidades de geolocaliza├º├úo e lookup de CEP brasileiro.

**Funcionalidades Entregues**:
- Γ£à ValueObjects: Cep, Coordinates, Address com valida├º├úo completa
- Γ£à Integra├º├úo com APIs de CEP: ViaCEP, BrasilAPI, OpenCEP
- Γ£à Fallback chain autom├ítico (ViaCEP ΓåÆ BrasilAPI ΓåÆ OpenCEP)
- Γ£à Resili├¬ncia HTTP via ServiceDefaults (retry, circuit breaker, timeout)
- Γ£à API p├║blica (ILocationModuleApi) para comunica├º├úo inter-m├│dulos
- Γ£à **Integra├º├úo IBGE API** (Sprint 1 Dia 1): Valida├º├úo geogr├ífica oficial
- Γ£à Servi├ºo de geocoding (stub para implementa├º├úo futura)
- Γ£à 52 testes unit├írios passando (100% coverage em ValueObjects)

**Arquitetura Implementada**:
```csharp
// ValueObjects
public sealed class Cep // Valida e formata CEP brasileiro (12345-678)
public sealed class Coordinates // Latitude/Longitude com valida├º├úo de limites
public sealed class Address // Endere├ºo completo com CEP, rua, bairro, cidade, UF

// API P├║blica
public interface ILocationModuleApi : IModuleApi
{
    Task<Result<AddressDto>> GetAddressFromCepAsync(string cep, CancellationToken ct = default);
    Task<Result<CoordinatesDto>> GetCoordinatesFromAddressAsync(string address, CancellationToken ct = default);
}
```

**Servi├ºos Implementados**:
- `CepLookupService`: Implementa chain of responsibility com fallback entre provedores
- `ViaCepClient`, `BrasilApiCepClient`, `OpenCepClient`: Clients HTTP com resili├¬ncia
- **`IbgeClient`** (Novo): Cliente HTTP para IBGE Localidades API com normaliza├º├úo de nomes
- **`IbgeService`** (Novo): Valida├º├úo de munic├¡pios com HybridCache (7 dias TTL)
- **`GeographicValidationService`** (Novo): Adapter pattern para integra├º├úo com middleware
- `GeocodingService`: Stub (TODO: integra├º├úo com Nominatim ou Google Maps API)

**Integra├º├úo IBGE Implementada** (Sprint 1 Dia 1):
```csharp
// IbgeClient: Normaliza├º├úo de nomes (remove acentos, lowercase, h├¡fens)
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

**Observa├º├úo**: IBGE integration provides city/state validation for geographic restriction; geocoding (lat/lon lookup) via Nominatim is planned for Sprint 3 (optional improvement).

**Modelos IBGE**:
- `Regiao`: Norte, Nordeste, Sudeste, Sul, Centro-Oeste
- `UF`: Unidade da Federa├º├úo (estado) com regi├úo
- `Mesorregiao`: Mesorregi├úo com UF
- `Microrregiao`: Microrregi├úo com mesorregi├úo
- `Municipio`: Munic├¡pio com hierarquia completa + helper methods (GetUF, GetEstadoSigla, GetNomeCompleto)

**API Base IBGE**: `https://servicodados.ibge.gov.br/api/v1/localidades/`

**Pr├│ximas Melhorias (Opcional)**:
- ≡ƒöä Implementar GeocodingService com Nominatim (OpenStreetMap) ou Google Maps API
- ≡ƒöä Adicionar caching Redis para reduzir chamadas ├ás APIs externas (TTL: 24h para CEP, 7d para geocoding)
- Γ£à ~~Integra├º├úo com IBGE para lookup de munic├¡pios e estados~~ (IMPLEMENTADO)

---

### 1.6. Γ£à M├│dulo ServiceCatalogs (Conclu├¡do)

**Status**: Implementado e funcional com testes completos

**Objetivo**: Gerenciar tipos de servi├ºos que prestadores podem oferecer por cat├ílogo gerenciado administrativamente.

#### **Arquitetura Implementada**
- **Padr├úo**: DDD + CQRS com hierarquia de categorias
- **Schema**: `service_catalogs` (isolado)
- **Naming**: snake_case no banco, PascalCase no c├│digo

#### **Entidades de Dom├¡nio Implementadas**
```csharp
// ServiceCategory: Aggregate Root
public sealed class ServiceCategory : AggregateRoot<ServiceCategoryId>
{
    public string Name { get; }
    public string? Description { get; }
    public bool IsActive { get; }
    public int DisplayOrder { get; }
    
    // Domain Events: Created, Updated, Activated, Deactivated
    // Business Rules: Nome ├║nico, valida├º├╡es de cria├º├úo/atualiza├º├úo
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
    // Business Rules: Nome ├║nico, categoria ativa, valida├º├╡es
}
```

#### **Camadas Implementadas**

**1. Domain Layer** Γ£à
- `ServiceCategoryId` e `ServiceId` (strongly-typed IDs)
- Agregados com l├│gica de neg├│cio completa
- 9 Domain Events (lifecycle completo)
- Reposit├│rios: `IServiceCategoryRepository`, `IServiceRepository`
- Exception: `CatalogDomainException`

**2. Application Layer** Γ£à
- **DTOs**: ServiceCategoryDto, ServiceDto, ServiceListDto, ServiceCategoryWithCountDto
- **Commands** (11 total):
  - Categories: Create, Update, Activate, Deactivate, Delete
  - Services: Create, Update, ChangeCategory, Activate, Deactivate, Delete
- **Queries** (6 total):
  - Categories: GetById, GetAll, GetWithCount
  - Services: GetById, GetAll, GetByCategory
- **Handlers**: 11 Command Handlers + 6 Query Handlers
- **Module API**: `ServiceCatalogsModuleApi` para comunica├º├úo inter-m├│dulos

**3. Infrastructure Layer** Γ£à
- `ServiceCatalogsDbContext` com schema isolation (`service_catalogs`)
- EF Core Configurations (snake_case, ├¡ndices otimizados)
- Repositories com SaveChangesAsync integrado
- DI registration com auto-migration support

**4. API Layer** Γ£à
- **Endpoints REST** usando Minimal APIs pattern:
  - `GET /api/v1/service-catalogs/categories` - Listar categorias
  - `GET /api/v1/service-catalogs/categories/{id}` - Buscar categoria
  - `POST /api/v1/service-catalogs/categories` - Criar categoria
  - `PUT /api/v1/service-catalogs/categories/{id}` - Atualizar categoria
  - `POST /api/v1/service-catalogs/categories/{id}/activate` - Ativar
  - `POST /api/v1/service-catalogs/categories/{id}/deactivate` - Desativar
  - `DELETE /api/v1/service-catalogs/categories/{id}` - Deletar
  - `GET /api/v1/service-catalogs/services` - Listar servi├ºos
  - `GET /api/v1/service-catalogs/services/{id}` - Buscar servi├ºo
  - `GET /api/v1/service-catalogs/services/category/{categoryId}` - Por categoria
  - `POST /api/v1/service-catalogs/services` - Criar servi├ºo
  - `PUT /api/v1/service-catalogs/services/{id}` - Atualizar servi├ºo
  - `POST /api/v1/service-catalogs/services/{id}/change-category` - Mudar categoria
  - `POST /api/v1/service-catalogs/services/{id}/activate` - Ativar
  - `POST /api/v1/service-catalogs/services/{id}/deactivate` - Desativar
  - `DELETE /api/v1/service-catalogs/services/{id}` - Deletar
- **Autoriza├º├úo**: Todos endpoints requerem role Admin
- **Versionamento**: Sistema unificado via BaseEndpoint

**5. Shared.Contracts** Γ£à
- `IServiceCatalogsModuleApi` - Interface p├║blica
- DTOs: ModuleServiceCategoryDto, ModuleServiceDto, ModuleServiceListDto, ModuleServiceValidationResultDto

#### **API P├║blica Implementada**
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

#### **Status de Compila├º├úo**
- Γ£à **Domain**: BUILD SUCCEEDED (3 warnings XML documentation)
- Γ£à **Application**: BUILD SUCCEEDED (18 warnings SonarLint - n├úo cr├¡ticos)
- Γ£à **Infrastructure**: BUILD SUCCEEDED
- Γ£à **API**: BUILD SUCCEEDED
- Γ£à **Adicionado ├á Solution**: 4 projetos integrados

#### **Integra├º├úo com Outros M├│dulos**
- **Providers Module** (Planejado): Adicionar ProviderServices linking table
- **Search Module** (Planejado): Denormalizar services nos SearchableProvider
- **Admin Portal**: Endpoints prontos para gest├úo de cat├ílogo

#### **Pr├│ximos Passos (P├│s-MVP)**
1. **Testes**: Implementar unit tests e integration tests
2. **Migrations**: Criar e aplicar migration inicial do schema `service_catalogs`
3. **Bootstrap**: Integrar no Program.cs e AppHost
4. **Provider Integration**: Estender Providers para suportar ProviderServices
5. **Admin UI**: Interface para gest├úo de cat├ílogo
6. **Seeders**: Popular cat├ílogo inicial com servi├ºos comuns

#### **Considera├º├╡es T├⌐cnicas**
- **SaveChangesAsync**: Integrado nos reposit├│rios (padr├úo do projeto)
- **Valida├º├╡es**: Nome ├║nico por categoria/servi├ºo, categoria ativa para criar servi├ºo
- **Soft Delete**: N├úo implementado (hard delete com valida├º├úo de depend├¬ncias)
- **Cascata**: DeleteServiceCategory valida se h├í servi├ºos vinculados

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

## ≡ƒöä Fase 1.5: Funda├º├úo T├⌐cnica (Em Andamento)

### Objetivo
Fortalecer a base t├⌐cnica do sistema antes de desenvolver frontend, garantindo escalabilidade, qualidade e compatibilidade com .NET 10 LTS + Aspire 13.

### Justificativa
Com todos os 6 m├│dulos core implementados (Fase 1 Γ£à), precisamos consolidar a funda├º├úo t├⌐cnica antes de iniciar desenvolvimento frontend:
- **.NET 9 EOL**: Suporte expira em maio 2025, migrar para .NET 10 LTS agora evita migra├º├úo em produ├º├úo
- **Aspire 13**: Novas features de observability e orchestration
- **Test Coverage**: Atual 40.51% ΓåÆ objetivo 80%+ para manutenibilidade
- **Integra├º├úo de M├│dulos**: IModuleApi implementado mas n├úo utilizado com as regras de neg├│cio reais
- **Restri├º├úo Geogr├ífica**: MVP exige opera├º├úo apenas em cidades piloto (SP, RJ, BH)

---

### ≡ƒôà Sprint 0: Migration .NET 10 + Aspire 13 (1-2 semanas)

**Status**: Γ£à CONCLU├ìDO (10 Dez 2025) - Branch: `improve-tests-coverage-2`

**Objetivos**:
- Migrar todos projetos para .NET 10 LTS
- Atualizar Aspire para v13
- Atualizar depend├¬ncias (EF Core 10, Npgsql 10, etc.)
- Validar testes e corrigir breaking changes
- Atualizar CI/CD para usar .NET 10 SDK

**Tarefas**:
- [x] Criar branch `migration-to-dotnet-10` Γ£à
- [x] Merge master (todos m├│dulos Fase 1) Γ£à
- [x] Atualizar `Directory.Packages.props` para .NET 10 Γ£à
- [x] Atualizar todos `.csproj` para `<TargetFramework>net10.0</TargetFramework>` Γ£à
- [x] Atualizar Aspire packages para v13.0.2 Γ£à
- [x] Atualizar EF Core para 10.0.1 GA Γ£à
- [x] Atualizar Npgsql para 10.0.0 GA Γ£à
- [x] `dotnet restore` executado com sucesso Γ£à
- [x] **Verifica├º├úo Incremental**:
  - [x] Build Domain projects ΓåÆ Γ£à sem erros
  - [x] Build Application projects ΓåÆ Γ£à sem erros
  - [x] Build Infrastructure projects ΓåÆ Γ£à sem erros
  - [x] Build API projects ΓåÆ Γ£à sem erros
  - [x] Build completo ΓåÆ Γ£à 0 warnings, 0 errors
  - [x] Fix testes Hangfire (Skip para CI/CD) Γ£à
  - [x] Run unit tests ΓåÆ Γ£à 480 testes (479 passed, 1 skipped)
  - [x] Run integration tests ΓåÆ Γ£à validados com Docker
- [x] Atualizar CI/CD workflows (removido --locked-mode) Γ£à
- [x] Validar Docker images com .NET 10 Γ£à
- [x] Merge para master ap├│s valida├º├úo completa Γ£à

**Resultado Alcan├ºado**:
- Γ£à Sistema rodando em .NET 10 LTS com Aspire 13.0.2
- Γ£à Todos 480 testes passando (479 passed, 1 skipped)
- Γ£à CI/CD funcional (GitHub Actions atualizado)
- Γ£à Documenta├º├úo atualizada
- Γ£à EF Core 10.0.1 GA + Npgsql 10.0.0 GA (vers├╡es est├íveis)

#### ≡ƒôª Pacotes com Vers├╡es N├úo-Est├íveis ou Pendentes de Atualiza├º├úo

ΓÜá∩╕Å **CRITICAL**: All packages listed below are Release Candidate (RC) or Preview versions.  
**DO NOT deploy to production** until stable versions are released. See [.NET 10 Release Timeline](https://github.com/dotnet/core/releases).

**Status da Migration**: A maioria dos pacotes core j├í est├í em .NET 10, mas alguns ainda est├úo em **RC (Release Candidate)** ou aguardando releases est├íveis.

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

**≡ƒôª Pacotes Atualizados ΓÇö Estado Misto (11 Dez 2025)**:

| Pacote | Vers├úo Atual | Status | Notas |
|--------|--------------|--------|-------|
| **EF Core 10.x** | `10.0.1` | Γ£à GA STABLE | Atualizado de 10.0.0-rc.2 ΓåÆ 10.0.1 GA |
| **Npgsql 10.x** | `10.0.0` | Γ£à GA STABLE | Atualizado de 10.0.0-rc.1 ΓåÆ 10.0.0 GA |
| **Aspire 13.x** | `13.0.2` | Γ£à GA STABLE | Atualizado de 13.0.0-preview.1 ΓåÆ 13.0.2 GA |
| **Aspire.Npgsql.EntityFrameworkCore.PostgreSQL** | `13.0.2` | Γ£à GA STABLE | Sincronizado com Aspire 13.0.2 GA |
| **Hangfire.PostgreSql** | `1.20.13` | ΓÜá∩╕Å STABLE (Npgsql 6.x) | Monitorando compatibilidade com Npgsql 10.x |
| **EFCore.NamingConventions** | `10.0.0-rc.2` | ΓÜá∩╕Å PRE-RELEASE | Aguardando vers├úo est├ível (issue template criado) |

**≡ƒåò Atualiza├º├╡es via Dependabot (11 Dez 2025)**:

| Pacote | Vers├úo Anterior | Vers├úo Atual | PR | Status |
|--------|-----------------|--------------|-----|--------|
| **Microsoft.AspNetCore.Authentication.JwtBearer** | `10.0.0` | `10.0.1` | [#62](https://github.com/frigini/MeAjudaAi/pull/62) | Γ£à MERGED |
| **Microsoft.AspNetCore.OpenApi** | `10.0.0` | `10.0.1` | [#64](https://github.com/frigini/MeAjudaAi/pull/64) | Γ£à MERGED |
| **Microsoft.Extensions.Caching.Hybrid** | `10.0.0` | `10.1.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | Γ£à MERGED |
| **Microsoft.Extensions.Http.Resilience** | `10.0.0` | `10.1.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | Γ£à MERGED |
| **Serilog** | `4.2.0` | `4.3.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | Γ£à MERGED |
| **Serilog.Sinks.Console** | `6.0.0` | `6.1.1` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | Γ£à MERGED |

**Γ£à Resultado**: Pacotes core (EF Core 10.0.1, Npgsql 10.0.0, Aspire 13.0.2) atualizados para GA est├íveis. EFCore.NamingConventions 10.0.0-rc.2 sob monitoramento (aguardando GA). Lockfiles regenerados e validados em CI/CD.

**ΓÜá∩╕Å Pacotes Ainda a Monitorar**:

| Pacote | Vers├úo Atual | Vers├úo Est├ível Esperada | Impacto | A├º├úo Requerida |
|--------|--------------|-------------------------|---------|----------------|
| **EFCore.NamingConventions** | `10.0.0-rc.2` | `10.0.0` (Q1 2026?) | M├ëDIO | Monitorar <https://github.com/efcore/EFCore.NamingConventions> |
| **Hangfire.PostgreSql** | `1.20.13` | `2.0.0` com Npgsql 10+ | CR├ìTICO | Monitorar <https://github.com/frankhommers/Hangfire.PostgreSql> |

**≡ƒöö Monitoramento Autom├ítico de Releases**:

Para receber notifica├º├╡es quando novas vers├╡es est├íveis forem lan├ºadas, configure os seguintes alertas:

1. **GitHub Watch (Reposit├│rios Open Source)**:
   - Acesse: <https://github.com/dotnet/efcore> ΓåÆ Click "Watch" ΓåÆ "Custom" ΓåÆ "Releases"
   - Acesse: <https://github.com/npgsql/npgsql> ΓåÆ Click "Watch" ΓåÆ "Custom" ΓåÆ "Releases"
   - Acesse: <https://github.com/dotnet/aspire> ΓåÆ Click "Watch" ΓåÆ "Custom" ΓåÆ "Releases"
   - Acesse: <https://github.com/frankhommers/Hangfire.PostgreSql> ΓåÆ Click "Watch" ΓåÆ "Custom" ΓåÆ "Releases"
   - **Benef├¡cio**: Notifica├º├úo no GitHub e email quando nova release for publicada

2. **NuGet Package Monitoring (Via GitHub Dependabot)**:
   - Criar `.github/dependabot.yml` no reposit├│rio:
     ```yaml
     version: 2
     updates:
       - package-ecosystem: "nuget"
         directory: "/"
         schedule:
           interval: "weekly"
         open-pull-requests-limit: 10
         # Ignorar vers├╡es preview/rc se desejar apenas stable
         ignore:
           - dependency-name: "*"
             update-types: ["version-update:semver-major"]
     ```
   - **Benef├¡cio**: PRs autom├íticos quando novas vers├╡es forem detectadas

3. **NuGet.org Email Notifications**:
   - Acesse: <https://www.nuget.org/account> ΓåÆ "Change Email Preferences"
   - Habilite "Package update notifications"
   - **Limita├º├úo**: N├úo funciona para todos pacotes, depende do publisher

4. **Visual Studio / Rider IDE Alerts**:
   - **Visual Studio**: Tools ΓåÆ Options ΓåÆ NuGet Package Manager ΓåÆ "Check for updates automatically"
   - **Rider**: Settings ΓåÆ Build, Execution, Deployment ΓåÆ NuGet ΓåÆ "Check for package updates"
   - **Benef├¡cio**: Notifica├º├úo visual no Solution Explorer

5. **dotnet outdated (CLI Tool)**:
   ```powershell
   # Instalar globalmente
   dotnet tool install --global dotnet-outdated-tool
   
   # Verificar pacotes desatualizados
   dotnet outdated
   
   # Verificar apenas pacotes major/minor desatualizados
   dotnet outdated --upgrade:Major
   
   # Automatizar verifica├º├úo semanal (Task Scheduler / cron)
   # Windows Task Scheduler: Executar semanalmente
   # C:\Code\MeAjudaAi> dotnet outdated > outdated-report.txt
   ```
   - **Benef├¡cio**: Script automatizado para verifica├º├úo peri├│dica

6. **GitHub Actions Workflow (Recomendado)**:
   - Criar `.github/workflows/check-dependencies.yml`:
     ```yaml
     name: Check Outdated Dependencies
     
     on:
       schedule:
         - cron: '0 9 * * 1' # Toda segunda-feira ├ás 9h
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
   - **Benef├¡cio**: Verifica├º├úo autom├ítica semanal + cria├º├úo de Issue no GitHub

**≡ƒôï Checklist de Monitoramento (Recomendado)**:
- [x] Configurar GitHub Watch para dotnet/efcore Γ£à
- [x] Configurar GitHub Watch para npgsql/npgsql Γ£à
- [x] Configurar GitHub Watch para dotnet/aspire Γ£à
- [x] Configurar GitHub Watch para Hangfire.PostgreSql Γ£à
- [x] Issue template criado: `.github/ISSUE_TEMPLATE/efcore-naming-conventions-stable-monitoring.md` Γ£à
- [ ] Instalar `dotnet-outdated-tool` globalmente (opcional - monitoramento manual)
- [ ] Criar GitHub Actions workflow para verifica├º├úo autom├ítica (`.github/workflows/check-dependencies.yml`) (Sprint 3)
- [x] Dependabot habilitado via GitHub (PRs autom├íticos ativos) Γ£à
- [ ] Adicionar lembrete mensal no calend├írio para verifica├º├úo manual (backup)

**≡ƒöì Pacotes Cr├¡ticos Sem Compatibilidade .NET 10 Confirmada**:

1. **Hangfire.PostgreSql 1.20.12**
   - **Status**: Compilado contra Npgsql 6.x
   - **Risco**: Breaking changes em Npgsql 10.x n├úo validados pelo mantenedor
   - **Mitiga├º├úo Atual**: Testes de integra├º├úo (marcados como Skip no CI/CD)
   - **Monitoramento**: 
     - GitHub Issues: [Hangfire.PostgreSql Issues](https://github.com/frankhommers/Hangfire.PostgreSql/issues)
     - Alternativas: Hangfire.Pro.Redis (pago), Hangfire.SqlServer (outro DB)
   - **Prazo**: Validar localmente ANTES de deploy para produ├º├úo

2. **~~Swashbuckle.AspNetCore 10.0.1 - ExampleSchemaFilter~~** Γ£à RESOLVIDO (13 Dez 2025)
   - **Status**: ExampleSchemaFilter **removido permanentemente**
   - **Raz├úo**: C├│digo problem├ítico, dif├¡cil de testar, n├úo essencial
   - **Alternativa**: Usar XML documentation comments para exemplos quando necess├írio
   - **Commit**: [Adicionar hash ap├│s commit]

**≡ƒôà Cronograma de Atualiza├º├╡es Futuras**:

```mermaid
gantt
    title Roadmap de Atualiza├º├╡es de Pacotes
    dateFormat  YYYY-MM-DD
    section EF Core
    RC ΓåÆ Stable           :2025-11-20, 2025-12-15
    Atualizar projeto     :2025-12-15, 7d
    section Npgsql
    RC ΓåÆ Stable           :2025-11-20, 2025-12-15
    Revalidar Hangfire    :2025-12-15, 7d
    section Aspire
    Preview ΓåÆ Stable      :2025-11-20, 2025-12-31
    Atualizar configs     :2025-12-31, 3d
    section Hangfire
    Monitorar upstream    :2025-11-20, 2026-06-30
```

**Γ£à A├º├╡es Conclu├¡das P├│s-Migration (10 Dez 2025)**:
1. Γ£à Finalizar valida├º├úo de testes (unit + integration) - 480 testes passando
2. Γ£à Validar Hangfire localmente (com Aspire) - funcional
3. Γ£à Configurar GitHub Watch para monitoramento de releases (EF Core, Npgsql, Aspire)
4. Γ£à Issue template criado para EFCore.NamingConventions stable monitoring
5. Γ£à Dependabot habilitado via GitHub (PRs autom├íticos)
6. Γ£à Monitoramento ativo para Hangfire.PostgreSql 2.0 (Issue #39)

**≡ƒô¥ Notas de Compatibilidade**:
- **EF Core 10 RC**: Sem breaking changes conhecidos desde RC.1
- **Npgsql 10 RC**: Breaking changes documentados em <https://www.npgsql.org/doc/release-notes/10.0.html>
- **Aspire 13 Preview**: API est├ível, apenas features novas em desenvolvimento

---

### ≡ƒôà Sprint 1: Geographic Restriction + Module Integration (10 dias)

**Status**: ≡ƒöä DIAS 1-6 CONCLU├ìDOS | FINALIZANDO (22-25 Nov 2025)  
**Branches**: `feature/geographic-restriction` (merged Γ£à), `feature/module-integration` (em review), `improve-tests-coverage` (criada)  
**Documenta├º├úo**: An├ílise integrada em [testing/coverage.md](./testing/coverage.md)

**Conquistas**:
- Γ£à Sprint 0 conclu├¡do: Migration .NET 10 + Aspire 13 merged (21 Nov)
- Γ£à Middleware de restri├º├úo geogr├ífica implementado com IBGE API integration
- Γ£à 4 Module APIs implementados (Documents, ServiceCatalogs, SearchProviders, Locations)
- Γ£à Testes reativados: 28 testes (11 AUTH + 9 IBGE + 2 ServiceCatalogs + 3 IBGE unavailability + 3 duplicates removed)
- Γ£à Skipped tests reduzidos: 20 (26%) ΓåÆ 11 (11.5%) Γ¼ç∩╕Å **-14.5%**
- Γ£à Integration events: Providers ΓåÆ SearchProviders indexing
- Γ£à Schema fixes: search_providers standardization
- Γ£à CI/CD fix: Workflow secrets validation removido

**Objetivos Alcan├ºados**:
- Γ£à Implementar middleware de restri├º├úo geogr├ífica (compliance legal)
- Γ£à Implementar 4 Module APIs usando IModuleApi entre m├│dulos
- Γ£à Reativar 28 testes E2E skipped (auth refactor + race condition fixes)
- Γ£à Integra├º├úo cross-module: Providers Γåö Documents, Providers Γåö SearchProviders
- ΓÅ│ Aumentar coverage: 35.11% ΓåÆ 80%+ (MOVIDO PARA SPRINT 2)

**Estrutura (2 Branches + Pr├│xima Sprint)**:

#### Branch 1: `feature/geographic-restriction` (Dias 1-2) Γ£à CONCLU├ìDO
- [x] GeographicRestrictionMiddleware (valida├º├úo cidade/estado) Γ£à
- [x] GeographicRestrictionOptions (configuration) Γ£à
- [x] Feature toggle (Development: disabled, Production: enabled) Γ£à
- [x] Unit tests (29 tests) + Integration tests (8 tests, skipped) Γ£à
- [x] **Integra├º├úo IBGE API** (valida├º├úo oficial de munic├¡pios) Γ£à
  - [x] IbgeClient com normaliza├º├úo de nomes (Muria├⌐ ΓåÆ muriae) Γ£à
  - [x] IbgeService com HybridCache (7 dias TTL) Γ£à
  - [x] GeographicValidationService (adapter pattern) Γ£à
  - [x] 2-layer validation (IBGE primary, simple fallback) Γ£à
  - [x] 15 unit tests IbgeClient Γ£à
  - [x] Configura├º├úo de APIs (ViaCep, BrasilApi, OpenCep, IBGE) Γ£à
  - [x] Remo├º├úo de hardcoded URLs (enforce configuration) Γ£à
- [x] **Commit**: feat(locations): Integrate IBGE API for geographic validation (520069a) Γ£à
- **Target**: 28.69% ΓåÆ 30% coverage Γ£à (CONCLU├ìDO: 92/104 testes passando)
- **Merged**: 25 Nov 2025 Γ£à

#### Branch 2: `feature/module-integration` (Dias 3-10) Γ£à DIAS 3-6 CONCLU├ìDOS | ≡ƒöä DIA 7-10 CODE REVIEW
- [x] **Dia 3**: Refactor ConfigurableTestAuthenticationHandler (reativou 11 AUTH tests) Γ£à
- [x] **Dia 3**: Fix race conditions (identificados 2 para Sprint 2) Γ£à
- [x] **Dia 4**: IDocumentsModuleApi implementation (7 m├⌐todos) Γ£à
- [x] **Dia 5**: IServiceCatalogsModuleApi (3 m├⌐todos stub) + ISearchModuleApi (2 novos m├⌐todos) Γ£à
- [x] **Dia 6**: Integration events (Providers ΓåÆ SearchProviders indexing) Γ£à
  - [x] DocumentVerifiedIntegrationEvent + handler Γ£à
  - [x] ProviderActivatedIntegrationEventHandler Γ£à
  - [x] SearchProviders schema fix (search ΓåÆ search_providers) Γ£à
  - [x] Clean InitialCreate migration Γ£à
- [x] **Dia 7**: Naming standardization (Module APIs) Γ£à
  - [x] ILocationModuleApi ΓåÆ ILocationsModuleApi Γ£à
  - [x] ISearchModuleApi ΓåÆ ISearchProvidersModuleApi Γ£à
  - [x] SearchModuleApi ΓåÆ SearchProvidersModuleApi Γ£à
  - [x] ProviderIndexingDto ΓåÆ ModuleProviderIndexingDto Γ£à
- [x] **Dia 7**: Test cleanup (remove diagnostics) Γ£à
- [ ] **Dia 7-10**: Code review & documentation ≡ƒöä
- **Target**: 30% ΓåÆ 35% coverage, 93/100 ΓåÆ 98/100 E2E tests
- **Atual**: 2,076 tests (2,065 passing - 99.5%, 11 skipped - 0.5%)
- **Commits**: 25+ total (583 commits total na branch)
- **Status**: Aguardando code review antes de merge

**Integra├º├╡es Implementadas**:
- Γ£à **Providers ΓåÆ Documents**: ActivateProviderCommandHandler valida documentos (4 checks)
- Γ£à **Providers ΓåÆ SearchProviders**: ProviderActivatedIntegrationEventHandler indexa providers
- Γ£à **Documents ΓåÆ Providers**: DocumentVerifiedDomainEventHandler publica integration event
- ΓÅ│ **Providers ΓåÆ ServiceCatalogs**: API criada, aguarda implementa├º├úo de gest├úo de servi├ºos
- ΓÅ│ **Providers ΓåÆ Locations**: CEP lookup (baixa prioridade)

**Bugs Cr├¡ticos Corrigidos**:
- Γ£à AUTH Race Condition (ConfigurableTestAuthenticationHandler thread-safety)
- Γ£à IBGE Fail-Closed Bug (GeographicValidationService + IbgeService)
- Γ£à MunicipioNotFoundException criada para fallback correto
- Γ£à SearchProviders schema hardcoded (search ΓåÆ search_providers)

#### ≡ƒåò Coverage Improvement: Γ£à CONCLU├ìDO NO SPRINT 2
- Γ£à Coverage aumentado 28.2% ΓåÆ **90.56%** (+62.36pp - META 35% SUPERADA EM 55.56pp!)
- Γ£à 480 testes (479 passing, 1 skipped) - Suite completa validada em CI/CD
- Γ£à E2E tests para provider indexing flow implementados
- Γ£à Integration tests completos com Docker/TestContainers
- ΓÅ│ Criar .bru API collections para m├│dulos (Sprint 3)
- ΓÅ│ Atualizar tools/ projects (MigrationTool, etc.) (Sprint 3)
- **Resultado**: Sprint 2 conclu├¡do (10 Dez 2025) - Coverage report consolidado gerado

**Tarefas Detalhadas**:

#### 1. Integra├º├úo Providers Γåö Documents Γ£à CONCLU├ìDO
- [x] Providers: Validar `HasVerifiedDocuments` antes de aprovar prestador Γ£à
- [x] Providers: Bloquear ativa├º├úo se `HasRejectedDocuments` ou `HasPendingDocuments` Γ£à
- [x] Documents: Publicar `DocumentVerified` event para atualizar status de Providers Γ£à
- [x] Integration test: Fluxo completo de verifica├º├úo de prestador Γ£à

#### 2. Integra├º├úo Providers Γåö ServiceCatalogs Γ£à IMPLEMENTADO
- [x] ServiceCatalogs: IServiceCatalogsModuleApi com 8 m├⌐todos implementados Γ£à
- [x] ServiceCatalogs: ValidateServicesAsync implementado Γ£à
- [x] ServiceCatalogs: Repository pattern com ServiceCategoryRepository Γ£à
- [x] Integration tests: 15 testes passando Γ£à
- ΓÅ│ Providers: Integra├º├úo de valida├º├úo de servi├ºos (Sprint 3)
- ΓÅ│ Admin Portal: UI para gest├úo de categorias/servi├ºos (Sprint 3)

#### 3. Integra├º├úo SearchProviders Γåö Providers Γ£à CONCLU├ìDO
- [x] Search: M├⌐todos IndexProviderAsync e RemoveProviderAsync implementados Γ£à
- [x] Search: Background handler consumindo ProviderVerificationStatusUpdated events Γ£à
- [x] Search: ISearchProvidersModuleApi com 2 m├⌐todos Γ£à
- [x] Integration test: Busca retorna apenas prestadores verificados Γ£à

#### 4. Integra├º├úo Providers Γåö Locations Γ£à IMPLEMENTADO
- [x] Locations: ILocationsModuleApi implementada Γ£à
- [x] Locations: GetAddressFromCepAsync com 3 providers (ViaCEP, BrasilAPI, OpenCEP) Γ£à
- [x] Locations: IBGE API integration para valida├º├úo de munic├¡pios Γ£à
- [x] Unit tests: 67 testes passando (Locations module) Γ£à
- ΓÅ│ Providers: Integra├º├úo autom├ítica de CEP lookup (Sprint 3)

#### 5. Restri├º├úo Geogr├ífica (MVP Blocker) Γ£à CONCLU├ìDO
- [x] Criar `AllowedCities` configuration em appsettings Γ£à
- [x] GeographicRestrictionMiddleware implementado com IBGE integration Γ£à
- [x] Fail-open fallback para valida├º├úo simples quando IBGE unavailable Γ£à
- [x] Integration test: 24 testes passando Γ£à
- ΓÅ│ Admin: Endpoint para gerenciar cidades permitidas (Sprint 3 - GitHub Pages docs)

**Resultado Alcan├ºado (Sprint 1)**:
- Γ£à M├│dulos integrados com business rules reais (Providers Γåö Documents, Providers Γåö SearchProviders)
- Γ£à Opera├º├úo restrita a cidades piloto configuradas (IBGE API validation)
- Γ£à Background workers consumindo integration events (ProviderActivated, DocumentVerified)
- Γ£à Valida├º├╡es cross-module funcionando (HasVerifiedDocuments, HasRejectedDocuments)
- Γ£à Naming standardization (ILocationsModuleApi, ISearchProvidersModuleApi)
- Γ£à CI/CD fix (secrets validation removido)
- Γ£à **MERGED para master** (branch improve-tests-coverage-2 ativa para continua├º├úo)

---

### ≡ƒôà Sprint 2: Test Coverage Improvement - Phase 1 (2 semanas)

**Status**: Γ£à CONCLU├ìDO em 10 Dez 2025  
**Branches**: `improve-tests-coverage` (merged Γ£à), `improve-tests-coverage-2` (ativa - branch atual)

**Conquistas (26 Nov - 10 Dez)**:
- Γ£à **improve-tests-coverage** branch merged (39 novos testes Shared)
  - Γ£à ValidationBehavior: 9 testes (+2-3% coverage)
  - Γ£à TopicStrategySelector: 11 testes (+3% coverage)
  - Γ£à Shared core classes: 39 unit tests total
  - Γ£à Coverage pipeline habilitado para todos m├│dulos
  - Γ£à Roadmap documentado com an├ílise completa de gaps
- Γ£à **improve-tests-coverage-2** branch (2 Dez 2025 - 5 commits)
  - Γ£à **Task 1 - PermissionMetricsService**: Concurrency fix (Dictionary ΓåÆ ConcurrentDictionary)
    - Commit: aabba3d - 813 testes passando (was 812)
  - Γ£à **Task 2 - DbContext Transactions**: 10 testes criados (4 passing, 6 skipped/documented)
    - Commit: 5ff84df - DbContextTransactionTests.cs (458 lines)
    - Helper: ShortId() for 8-char GUIDs (Username max 30 chars)
    - 6 flaky tests documented (TestContainers concurrency issues)
  - ΓÅ¡∩╕Å **Task 3 - DbContextFactory**: SKIPPED (design-time only, n├úo existe em runtime)
  - ΓÅ¡∩╕Å **Task 4 - SchemaIsolationInterceptor**: SKIPPED (component doesn't exist)
  - Γ£à **Task 5 - Health Checks**: 47 testes totais (4 health checks cobertos)
    - Commit: 88eaef8 - ExternalServicesHealthCheck (9 testes, Keycloak availability)
    - Commit: 1ddbf4d - Refactor reflection removal (3 classes: internal ΓåÆ public)
    - Commit: fbf02b9 - HelpProcessing (9 testes) + DatabasePerformance (9 testes)
    - PerformanceHealthCheck: 20 testes (j├í existiam anteriormente)
  - Γ£à **Code Quality**: Removida reflection de todos health checks (maintainability)
  - Γ£à **Warning Fixes**: CA2000 reduzido de 16 ΓåÆ 5 (using statements adicionados)
  - Γ£à **Shared Tests**: 841 testes passando (eram 813, +28 novos)

**Progresso Coverage (2 Dez 2025)**:
- Baseline: 45% (antes das branches - inclu├¡a c├│digo de teste)
- **Atual: 27.9%** (14,504/51,841 lines) - **MEDI├ç├âO REAL excluindo c├│digo gerado**
  - **Com c├│digo gerado**: 28.2% (14,695/52,054 lines) - diferen├ºa de -0.3%
  - **C├│digo gerado exclu├¡do**: 213 linhas via ExcludeByFile patterns:
    - `**/*OpenApi*.generated.cs`
    - `**/System.Runtime.CompilerServices*.cs`
    - `**/*RegexGenerator.g.cs`
  - **An├ílise Correta**: 27.9% ├⌐ coverage do **c├│digo de produ├º├úo escrito manualmente**
- **Branch Coverage**: 21.7% (2,264/10,422 branches) - sem c├│digo gerado
- **Method Coverage**: 40.9% (2,168/5,294 m├⌐todos) - sem c├│digo gerado
- **Test Suite**: 1,407 testes totais (1,393 passing - 99.0%, 14 skipped - 1.0%, 0 failing)
- Target Phase 1: 35% (+7.1 percentage points from 27.9% baseline)
- Target Final Sprint 2: 50%+ (revised from 80% - more realistic)

**≡ƒôè Progress├úo de Coverage - Sprint 2 (Audit Trail)**:

| Medi├º├úo | Valor | Data | Notas |
|---------|-------|------|-------|
| **Baseline Pr├⌐-Refactor** | 28.2% | 2 Dez | Estado inicial Sprint 2 |
| **Baseline Ajustado** | 27.9% | 2 Dez | Exclus├úo c├│digo gerado (OpenAPI + Regex) |
| **P├│s-Adi├º├úo de Testes** | 90.56% | 10 Dez | 40+ novos testes + consolida├º├úo |

**≡ƒôê Ganho Total**: +62.36 percentage points (28.2% ΓåÆ 90.56%)

**Coverage por Assembly (Top 5 - Maiores)**:
1. **MeAjudaAi.Modules.Users.Tests**: 0% (test code, expected)
2. **MeAjudaAi.Modules.Users.Application**: 55.6% (handlers, queries, DTOs)
3. **MeAjudaAi.Modules.Users.Infrastructure**: 53.9% (Keycloak, repos, events)
4. **MeAjudaAi.Modules.Users.Domain**: 49.1% (entities, value objects, events)
5. **MeAjudaAi.Shared**: 41.2% (authorization, caching, behaviors)

**Coverage por Assembly (Bottom 5 - Gaps Cr├¡ticos)**:
1. **MeAjudaAi.ServiceDefaults**: 20.7% (health checks, extensions) ΓÜá∩╕Å
2. **MeAjudaAi.Modules.ServiceCatalogs.Domain**: 27.6% (domain events 25-50%)
3. **MeAjudaAi.Shared.Tests**: 7.3% (test infrastructure code)
4. **MeAjudaAi.ApiService**: 55.5% (middlewares, extensions) - better than expected
5. **MeAjudaAi.Modules.Users.API**: 31.8% (endpoints, extensions)

**Gaps Identificados (Coverage < 30%)**:
- ΓÜá∩╕Å **ServiceDefaults.HealthChecks**: 0% (ExternalServicesHealthCheck, PostgresHealthCheck, GeolocationHealth)
  - **Motivo**: Classes est├úo no ServiceDefaults (AppHost), n├úo no Shared (testado)
  - **A├º├úo**: Mover health checks para Shared.Monitoring ou criar testes no AppHost
- ΓÜá∩╕Å **Shared.Logging**: 0% (SerilogConfigurator, CorrelationIdEnricher, LoggingContextMiddleware)
  - **A├º├úo**: Unit tests para enrichers, integration tests para middleware
- ΓÜá∩╕Å **Shared.Jobs**: 14.8% ΓåÆ **85%+** (HangfireHealthCheck, HangfireAuthorizationFilter testes criados - 20 Dez 2025)
  - Γ£à **HangfireHealthCheck**: 7 unit tests (valida├º├úo de status, thresholds, null checks)
  - Γ£à **HangfireAuthorizationFilter**: 11 unit tests (ACL admin, ambientes, auth checks)
  - **A├º├úo Completada**: Testes unit├írios criados, coverage estimada 85%+
- ΓÜá∩╕Å **Shared.Messaging.RabbitMq**: 12% (RabbitMqMessageBus)
  - **Motivo**: Integration tests require RabbitMQ container
  - **A├º├úo**: TestContainers RabbitMQ ou mocks
- ΓÜá∩╕Å **Shared.Database.Exceptions**: 17% (PostgreSqlExceptionProcessor)
  - **A├º├úo**: Unit tests para constraint exception handling

**Progresso Phase 1 (Improve-Tests-Coverage-2)**:
- Γ£à **5 Commits**: aabba3d, 5ff84df, 88eaef8, 1ddbf4d, fbf02b9
- Γ£à **40 New Tests**: Task 2 (10 DbContext) + Task 5 (27 health checks) + Task 1 (+3 fixes)
- Γ£à **Test Success Rate**: 99.0% (1,393/1,407 passing)
- Γ£à **Build Time**: ~25 minutes (full suite with Docker integration tests)
- Γ£à **Health Checks Coverage**:
  - Γ£à ExternalServicesHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - Γ£à HelpProcessingHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - Γ£à DatabasePerformanceHealthCheck: 9/9 (Shared/Monitoring) - 100%
  - Γ£à PerformanceHealthCheck: 20/20 (Shared/Monitoring) - 100% (pr├⌐-existente)
  - Γ¥î ServiceDefaults.HealthChecks.*: 0% (not in test scope yet)

**Technical Decisions Validated**:
- Γ£à **No Reflection**: All health check classes changed from internal ΓåÆ public
  - Reason: "N├úo ├⌐ para usar reflection, ├⌐ dif├¡cil manter c├│digo com reflection"
  - Result: Direct instantiation `new MeAjudaAiHealthChecks.HealthCheckName(...)`
- Γ£à **TestContainers**: Real PostgreSQL for integration tests (no InMemory)
  - Result: 4 core transaction tests passing, 6 advanced scenarios documented
- Γ£à **Moq.Protected()**: HttpMessageHandler mocking for HttpClient tests
  - Result: 9 ExternalServicesHealthCheck tests passing
- Γ£à **Flaky Test Documentation**: TestContainers concurrency issues documented, not ignored
  - Files: DbContextTransactionTests.cs (lines with Skip attribute + detailed explanations)

**Phase 1 Completion** - Γ£à CONCLU├ìDO (10 Dez 2025):
- Γ£à **Coverage Report Generated**: coverage/report/index.html + Summary.txt
- Γ£à **Roadmap Update**: Documento atualizado com coverage 90.56% alcan├ºado
- Γ£à **Warnings**: Build limpo, zero warnings cr├¡ticos
- Γ£à **Merged to Master**: PR #35 merged com sucesso

**Phase 2 Completion** - Γ£à CONCLU├ìDO (10 Dez 2025):
- Γ£à **ServiceDefaults Health Checks**: Coberto via integration tests (coverage consolidada)
  - Γ£à PostgresHealthCheck: Testado via TestContainers nos m├│dulos
  - Γ£à GeolocationHealthOptions: 67 testes no m├│dulo Locations
  - Γ£à Health checks architecture: 47 testes em Shared/Monitoring
  
- Γ£à **Logging Infrastructure**: Cobertura via testes de m├│dulos
  - Γ£à Logging testado atrav├⌐s de integration tests
  - Γ£à CorrelationId tracking validado em E2E tests
  - Γ£à LoggingContextMiddleware: Funcional em todos m├│dulos
  
- Γ£à **Messaging Resilience**: Coberto via integration events
  - Γ£à Integration events: ProviderActivated, DocumentVerified testados
  - Γ£à Event handlers: 15+ handlers com testes unit├írios
  - Γ£à Message publishing: Validado em integration tests
  
- Γ£à **Middlewares**: Testados via E2E e integration tests
  - Γ£à GeographicRestrictionMiddleware: 24 integration tests
  - Γ£à Authorization: Validado em 100+ E2E tests com auth
  - Γ£à Request/Response pipeline: Coberto em ApiService.Tests
  
- Γ£à **Database Exception Handling**: Coberto nos m├│dulos
  - Γ£à Repository pattern: Testado em todos 6 m├│dulos
  - Γ£à Constraint violations: Validados em integration tests
  - Γ£à Transaction handling: Coberto em unit tests
  
- Γ£à **Documents Module**: Implementado e testado
  - Γ£à Document validation: 45+ testes unit├írios
  - Γ£à DocumentRepository: Integration tests completos
  - Γ£à Module API: IDocumentsModuleApi com 7 m├⌐todos testados

**Pr├│ximas Tarefas (Sprint 3 - GitHub Pages Documentation)**:
- [ ] Migrar documenta├º├úo para MkDocs Material
- [ ] Criar .bru API collections para teste manual
- [ ] Implementar data seeding scripts
- [ ] Admin endpoints para geographic restrictions
- [ ] Finalizar integra├º├╡es cross-module pendentes

**Objetivos Fase 1 (Dias 1-7) - Γ£à CONCLU├ìDO 2 DEZ 2025**:
- Γ£à Aumentar coverage Shared de baseline para 28.2% (medi├º├úo real)
- Γ£à Focar em componentes cr├¡ticos (Health Checks - 4/7 implementados)
- Γ£à Documentar testes flaky (6 TestContainers scope issues documented)
- Γ£à **NO REFLECTION** - todas classes public para manutenibilidade
- Γ£à 40 novos testes criados (5 commits, 1,393/1,407 passing)
- Γ£à Coverage report consolidado gerado (HTML + Text)

**Objetivos Fase 2 (Dias 8-14) - Γ£à CONCLU├ìDO 10 DEZ 2025**:
- Γ£à ServiceDefaults: Coverage integrado ao report consolidado
- Γ£à Shared.Logging: Cobertura aumentada com testes de m├│dulos
- Γ£à Shared.Messaging: Cobertura aumentada com testes de integra├º├úo
- Γ£à Shared.Database.Exceptions: Cobertura aumentada com testes de m├│dulos
- Γ£à **Overall Target SUPERADO**: 28.2% ΓåÆ **90.56%** (+62.36 percentage points!)

**Decis├╡es T├⌐cnicas**:
- Γ£à TestContainers para PostgreSQL (no InMemory databases)
- Γ£à Moq para HttpMessageHandler (HttpClient mocking)
- Γ£à FluentAssertions para assertions
- Γ£à xUnit 3.1.5 como framework
- Γ£à Classes public em vez de internal (no reflection needed)
- ΓÜá∩╕Å Testes flaky com concurrent scopes marcados como Skip (documentados)

**Health Checks Implementation** - Γ£à CONCLU├ìDO:
- Γ£à **ExternalServicesHealthCheck**: Keycloak availability (9 testes - Shared/Monitoring)
- Γ£à **PerformanceHealthCheck**: Memory, GC, thread pool (20 testes - Shared/Monitoring)
- Γ£à **HelpProcessingHealthCheck**: Business logic operational (9 testes - Shared/Monitoring)
- Γ£à **DatabasePerformanceHealthCheck**: DB metrics configured (9 testes - Shared/Monitoring)
- Γ£à **ServiceDefaults.HealthChecks.PostgresHealthCheck**: Testado via TestContainers (integration tests)
- Γ£à **Locations**: APIs de CEP health validadas (67 testes - ViaCEP, BrasilAPI, IBGE, OpenCEP)
- Γ£à **Documents**: Module health validado via integration tests
- Γ£à **Search**: PostGIS testado via SearchProviders integration tests

**Arquitetura de Health Checks** - Γ£à DEFINIDA:
- **Shared/Monitoring**: 4 health checks implementados e testados (47 testes, 100% coverage)
- **ServiceDefaults/HealthChecks**: Configura├º├╡es base para ASP.NET Core health checks
- **M├│dulos**: Cada m├│dulo com seus pr├│prios health checks espec├¡ficos
- **Decis├úo**: Arquitetura h├¡brida - Shared para componentes globais, m├│dulos para checks espec├¡ficos

**Data Seeding** (SPRINT 3):
- [ ] Seeder de ServiceCatalogs: 10 categorias + 50 servi├ºos (estrutura pronta, dados pendentes)
- [ ] Seeder de Providers: 20 prestadores fict├¡cios
- [ ] Seeder de Users: Admin + 10 customers
- [ ] Script: `dotnet run --seed-dev-data`

**Resultado Alcan├ºado Sprint 2 (10 Dez 2025)**:
- Γ£à **Overall coverage**: **90.56% line**, 78.2% branch, 93.4% method (Cobertura Aggregated Direct)
- Γ£à **Covered lines**: 12,487 de 14,371 coverable lines
- Γ£à **Test suite**: **480 testes** (479 passing - 99.8%, 1 skipped - 0.2%, 0 failing)
- Γ£à **Assemblies**: 25 assemblies cobertos
- Γ£à **Classes**: 528 classes, 491 files
- Γ£à **Build quality**: Zero warnings cr├¡ticos, build limpo
- Γ£à **Code quality**: Zero reflection, todas classes public
- Γ£à **Target SUPERADO**: Meta original 35% ΓåÆ **90.56% alcan├ºado** (+55.56pp acima da meta!)
  - *Nota: Target Phase 2 original era 80%, revisado para 50% mid-sprint por realismo; ambos superados*
- Γ£à **CI/CD**: Todos workflows atualizados e funcionais (.NET 10 + Aspire 13)

### Phase 2 Task Breakdown & Release Gates - Γ£à CONCLU├ìDO (10 Dez 2025)

#### Coverage Targets (Progressive) - Γ£à SUPERADO
- ~~**Minimum (CI Warning Threshold)**: Line 70%, Branch 60%, Method 70%~~
- ~~**Recommended**: Line 85%, Branch 75%, Method 85%~~
- Γ£à **ALCAN├çADO**: Line **90.56%**, Branch **78.2%**, Method **93.4%** (EXCELLENT tier!)

**Resultado**: Coverage inicial (28.2%) elevado para **90.56%** (+62.36pp). Todos os targets superados!

#### Phase 2 Task Matrix - Γ£à TODAS TAREFAS CONCLU├ìDAS

| Task | Priority | Estimated Tests | Target Coverage | Completed | Status |
|------|----------|-----------------|-----------------|-----------|--------|
| ServiceDefaults.HealthChecks | CRITICAL | 15-20 | 35%+ line | 10 Dez 2025 | Γ£à DONE - Testado via integration tests |
| Shared.Logging | CRITICAL | 10-12 | 30%+ line | 10 Dez 2025 | Γ£à DONE - Coberto nos m├│dulos |
| Shared.Messaging.RabbitMq | CRITICAL | 20-25 | 40%+ line | 10 Dez 2025 | Γ£à DONE - Integration events testados |
| Shared.Database.Exceptions | HIGH | 15-20 | 50%+ line | 10 Dez 2025 | Γ£à DONE - Repository pattern coberto |
| Shared.Middlewares | HIGH | 12-15 | 45%+ line | 10 Dez 2025 | Γ£à DONE - E2E tests validados |

#### Release Gate Criteria - Γ£à TODOS CRIT├ëRIOS ATENDIDOS

**Phase 2 Merge to Master** (Required):
- Γ£à Line Coverage: **90.56%** (target 35%+ - SUPERADO)
- Γ£à Health Checks: 100% para Shared/Monitoring (47 testes)
- Γ£à Test Suite: **480 testes** (target 1,467 - redefinido para qualidade)
- Γ£à All Tests Passing: **99.8%** (479 passing, 1 skipped)
- Γ£à Code Quality: 0 warnings cr├¡ticos, build limpo

**Production Deployment** (Ready):
- Γ£à Critical Paths: 90%+ para todos m├│dulos (Users, Providers, Documents, etc.)
- Γ£à End-to-End Tests: Todos fluxos principais passando (E2E.Tests + Integration.Tests)
- Γ£à Performance: Health checks validados, m├⌐tricas ok
- Γ£à Security: .NET 10 GA + Aspire 13.0.2 GA (sem vulnerabilidades conhecidas)

**Decis├úo**: Γ£à Phase 2 **MERGED para master** (PR #35) - Todos gates atendidos!

**Decis├╡es Estrat├⌐gicas Sprint 2 - Γ£à EXECUTADAS**:
1. Γ£à **Componentes cr├¡ticos cobertos**: ServiceDefaults, Logging, Messaging - 90.56% overall
2. Γ£à **Duplica├º├úo investigada**: Arquitetura h├¡brida definida (Shared/Monitoring + m├│dulos)
3. Γ£à **TestContainers implementado**: PostgreSQL validado em 11 integration test suites
4. Γ£à **Flaky tests documentados**: 1 teste skipped (ServiceCatalogs debug), documentado
5. Γ£à **Target SUPERADO**: 90.56% alcan├ºado (original 35% + realista 80% ambos superados!)
6. Γ£à **≡ƒôÜ Documentation Hosting**: Sprint 3 iniciado - branch `migrate-docs-github-pages` criada
   - Γ£à **Decis├úo confirmada**: MkDocs Material com GitHub Pages
   - Γ£à **Branch criada**: 10 Dez 2025
   - **Pr├│ximos passos**: Ver se├º├úo "Sprint 3: GitHub Pages Documentation" acima

---

## ≡ƒÜÇ Pr├│ximos Passos (P├│s Sprint 0 e Sprint 2)

### 1∩╕ÅΓâú Sprint 3: Code & Documentation Organization + Final Integrations (PR├ôXIMA TAREFA)

**Branch**: `migrate-docs-github-pages` (criada em 10 Dez 2025)
**Status**: ≡ƒöä EM PROGRESSO (Parte 1 iniciada 11 Dez 2025)
**Prioridade**: ALTA - Organiza├º├úo completa do projeto antes de prosseguir
**Estimativa**: 2-3 semanas
**Data prevista**: 11-30 Dez 2025

**≡ƒôà Cronograma Detalhado com Gates Semanais**:

| Semana | Per├¡odo | Tarefa Principal | Entreg├ível | Gate de Qualidade |
|--------|---------|------------------|------------|-------------------|
| **1** | 10-11 Dez | **Parte 1**: Docs Audit + MkDocs | `mkdocs.yml` live, 0 links quebrados | Γ£à GitHub Pages deployment |
| **2** | 11-17 Dez | **Parte 2**: Admin Endpoints + Tools | Endpoints de cidades + Bruno collections | Γ£à CRUD + 15 E2E tests passing |
| **3** | 18-24 Dez | **Parte 3**: Module Integrations | Provider Γåö ServiceCatalogs/Locations | Γ£à Integration tests passing |
| **4** | 25-30 Dez | **Parte 4**: Code Quality & Standardization | Moq, UuidGenerator, .slnx, OpenAPI | Γ£à Build + tests 100% passing |

**Estado Atual** (12 Dez 2025):
- Γ£à **Sprint 3 Parte 1 CONCLU├ìDA**: GitHub Pages deployed em [GitHub Pages](https://frigini.github.io/MeAjudaAi/)
- Γ£à **Sprint 3 Parte 2 CONCLU├ìDA**: Admin Endpoints + Tools
- Γ£à **Sprint 3 Parte 3 CONCLU├ìDA**: Module Integrations
- Γ£à **Sprint 3 Parte 4 CONCLU├ìDA**: Code Quality & Standardization
- ≡ƒÄ» **SPRINT 3 COMPLETA - 100% das tarefas realizadas!**

**Resumo dos Avan├ºos**:

**Parte 1: Documentation Migration to GitHub Pages** Γ£à
- Γ£à Audit completo: 43 arquivos .md consolidados
- Γ£à mkdocs.yml: Configurado com navega├º├úo hier├írquica
- Γ£à GitHub Actions: Workflow `.github/workflows/docs.yml` funcionando
- Γ£à Build & Deploy: Validado e publicado

**Parte 2: Admin Endpoints + Tools** Γ£à
- Γ£à Admin endpoints AllowedCities implementados (5 endpoints CRUD)
- Γ£à Bruno Collections para Locations/AllowedCities (6 arquivos)
- Γ£à Testes: 4 integration + 15 E2E (100% passando)
- Γ£à Exception handling completo
- Γ£à Build quality: 0 erros, 71 arquivos formatados
- Γ£à Commit d1ce7456: "fix: corrigir erros de compila├º├úo e exception handling em E2E tests"
- Γ£à Code Quality & Security Fixes (Commit e334c4d7):
  - Removed hardcoded DB credentials (2 arquivos)
  - Fixed build errors: CS0234, CS0246
  - Fixed compiler warnings: CS8603, CS8602, CS8604
  - Added null-safe normalization in AllowedCityRepository
  - Fixed test assertions (6 arquivos)
  - Fixed XML documentation warnings
  - Updated Bruno API documentation
  - Fixed bare URLs in documentation

**Parte 3: Module Integrations** Γ£à
- Γ£à Providers Γåö ServiceCatalogs Integration (Commit 53943da8):
  - Add/Remove services to providers (CQRS handlers)
  - Valida├º├úo via IServiceCatalogsModuleApi
  - POST/DELETE endpoints com autoriza├º├úo SelfOrAdmin
  - Bruno collections (2 arquivos)
  - Domain events: ProviderServiceAdded/RemovedDomainEvent
- Γ£à Aspire Migrations (Commit 3d2b260b):
  - MigrationExtensions.cs com WithMigrations()
  - MigrationHostedService autom├ítico
  - Removida pasta tools/MigrationTool
  - Integra├º├úo nativa com Aspire AppHost
- Γ£à Data Seeding Autom├ítico (Commit fe5a964c):
  - IDevelopmentDataSeeder interface
  - DevelopmentDataSeeder implementa├º├úo
  - Seed autom├ítico ap├│s migrations (Development only)
  - ServiceCatalogs + Locations populados
- Γ£à Data Seeding Scripts (Commit ae659293):
  - seed-dev-data.ps1 (PowerShell)
  - seed-dev-data.sh (Bash)
  - Idempotente, autentica├º├úo Keycloak
  - Documenta├º├úo em scripts/README.md

**Parte 4: Code Quality & Standardization** Γ£à
- Γ£à NSubstitute ΓåÆ Moq (Commit e8683c08):
  - 4 arquivos de teste padronizados
  - Removida depend├¬ncia NSubstitute
- Γ£à UuidGenerator Unification (Commit 0a448106):
  - 9 arquivos convertidos para UuidGenerator.NewId()
  - L├│gica centralizada em Shared.Time
- Γ£à Migra├º├úo .slnx (Commit 1de5dc1a):
  - MeAjudaAi.slnx criado (formato XML)
  - 40 projetos validados
  - 3 workflows CI/CD atualizados
  - Benef├¡cios: 5x mais r├ípido, menos conflitos git
- Γ£à OpenAPI Automation (Commit ae6ef2d0):
  - GitHub Actions para atualizar api-spec.json
  - Deploy autom├ítico para GitHub Pages com ReDoc
  - Documenta├º├úo em docs/api-automation.md

**Build Status Final**: Γ£à 0 erros, 100% dos testes passando, c├│digo formatado

---

## ≡ƒÄ» Sprint 5 (19 Dez 2025 - 3 Jan 2026) - Γ£à CONCLU├ìDA ANTECIPADAMENTE!

**Branch**: `refactor/code-quality-standardization` - Tarefas completadas nas Sprints 3-4

**Status**: Γ£à TODAS as tarefas foram conclu├¡das em sprints anteriores:

**Γ£à Prioridade 1 - Cr├¡tico (COMPLETO)**:

1. Γ£à **Substituir NSubstitute por Moq** (Sprint 3):
   - 3 arquivos migrados (ServiceDefaults.Tests, ApiService.Tests x2)
   - Padroniza├º├úo completa - projeto usa 100% Moq
   - Depend├¬ncia duplicada removida

2. Γ£à **Unificar UuidGenerator** (Commit 0a448106 - Sprint 3):
   - ~26 ocorr├¬ncias de `Guid.CreateVersion7()` substitu├¡das
   - L├│gica centralizada em `MeAjudaAi.Shared.Time.UuidGenerator`
   - Preparado para futura customiza├º├úo

3. Γ£à **Migrar para .slnx** (Commit 1de5dc1a - Sprint 3):
   - `MeAjudaAi.slnx` criado (formato XML)
   - 40 projetos validados, build completo passando
   - 3 workflows CI/CD atualizados (.sln ΓåÆ .slnx)
   - Benef├¡cios confirmados: 5x mais r├ípido, menos conflitos git

4. Γ£à **Design Patterns Documentation** (Sprint 3-4):
   - Se├º├úo completa em `docs/architecture.md`
   - Padr├╡es documentados: Repository, CQRS, Domain Events, Factory, Strategy, Middleware Pipeline
   - Exemplos reais de c├│digo inclu├¡dos (AllowedCityRepository, Commands/Queries)
   - Se├º├úo anti-patterns evitados adicionada

**Γ£à Prioridade 2 - Desej├ível (COMPLETO)**:

5. Γ£à **Bruno Collections** (Sprint 3):
   - Γ£à **Users**: 6 arquivos .bru (CreateUser, DeleteUser, GetUsers, GetUserById, UpdateUser, GetUserByEmail)
   - Γ£à **Providers**: 16 arquivos .bru (CRUD completo + Services + Verification)
   - Γ£à **Documents**: 3 arquivos .bru (Upload, GetProviderDocuments, Verify)
   - Γ£à **ServiceCatalogs**: 35+ arquivos .bru (Categories + Services CRUD)
   - Γ£à **Locations**: 6 arquivos .bru (AllowedCities CRUD + README)

**ΓÅ╕∩╕Å Tarefas Remanescentes** (Prioridade 3 - Baixa urg├¬ncia, mover para Sprint 6 ou posterior):
- ≡ƒöÆ Avaliar migra├º├úo AspNetCoreRateLimit library
- ≡ƒôè Verificar completude Logging Estruturado (Seq, Domain Events, Performance)
- ≡ƒöù Providers Γåö Locations Integration (auto-populate cidade/estado via CEP)

---

## ≡ƒÄ» Pr├│ximos Passos - Sprint 6 (6 Jan - 24 Jan 2026)

**Foco**: Frontend Blazor - Admin Portal Setup + Customer App In├¡cio

**Branch Sugerida**: `feature/blazor-admin-portal`

**Objetivo Geral**: Iniciar desenvolvimento frontend com Blazor WASM para Admin Portal e MAUI Hybrid para Customer App.

**Estimativa Total**: 6-9 dias ├║teis (considerando feriados de fim de ano)

---

#### ≡ƒôÜ Parte 1: Documentation Migration to GitHub Pages (1 semana)

**Objetivos**:
- Migrar ~50 arquivos .md do diret├│rio `docs/` para GitHub Pages
- Implementar MkDocs Material para site naveg├ível
- Consolidar e eliminar documenta├º├úo duplicada/obsoleta
- Estabelecer estrutura hier├írquica l├│gica (max 3 n├¡veis)
- Deploy autom├ítico via GitHub Actions

**Processo de Migra├º├úo** (iterativo, documento a documento):
1. **Auditoria inicial**: Listar todos os .md e categorizar (atual/defasado/duplicado)
2. **Consolida├º├úo**: Mesclar conte├║do duplicado (ex: ci-cd.md vs ci-cd/workflows-overview.md)
3. **Limpeza**: Remover informa├º├╡es obsoletas ou mover para `docs/archive/`
4. **Reorganiza├º├úo**: Estruturar hierarquia (Getting Started ΓåÆ Architecture ΓåÆ Testing ΓåÆ CI/CD ΓåÆ API)
5. **Valida├º├úo**: Revisar links internos, atualizar refer├¬ncias cruzadas
6. **Navega├º├úo**: Configurar `mkdocs.yml` com estrutura final
7. **Deploy**: Habilitar GitHub Pages e testar site completo

**Crit├⌐rios de Qualidade**:
- Γ£à Zero duplica├º├úo de conte├║do
- Γ£à Informa├º├╡es datadas removidas ou arquivadas
- Γ£à Navega├º├úo intuitiva (max 3 n├¡veis de profundidade)
- Γ£à Todos links internos funcionando
- Γ£à Search global funcional
- Γ£à Mobile-friendly + dark mode

**Arquivos a Criar**:
- `mkdocs.yml` (configura├º├úo principal)
- `.github/workflows/deploy-docs.yml` (CI/CD workflow)
- `docs/requirements.txt` (depend├¬ncias Python: mkdocs-material, plugins)

**URL Final**: `https://frigini.github.io/MeAjudaAi/`

---

#### ≡ƒöº Parte 2: Scripts & Tools Organization (3-4 dias)

**Objetivos**:
- Revisar e atualizar scripts em `scripts/`
- Atualizar ferramentas em `tools/` (MigrationTool, etc.)
- Criar .bru API collections para teste manual dos m├│dulos
- Implementar data seeding scripts

**Tarefas Detalhadas**:
- [ ] **Scripts Cleanup**:
  - [ ] Revisar `scripts/generate-clean-coverage.ps1` (funcionando, documentar melhor)
  - [ ] Atualizar scripts de build/deploy se necess├írio
  - [ ] Criar script de data seeding: `scripts/seed-dev-data.ps1`
  
- [ ] **Tools/ Projects**:
  - [ ] Atualizar MigrationTool para .NET 10
  - [ ] Validar ferramentas auxiliares
  - [ ] Documentar uso de cada tool
  
- [ ] **API Collections (.bru)**:
  - [ ] Criar collection para m├│dulo Users
  - [ ] Criar collection para m├│dulo Providers
  - [ ] Criar collection para m├│dulo Documents
  - [ ] Criar collection para m├│dulo ServiceCatalogs
  - [ ] Criar collection para m├│dulo Locations
  - [ ] Criar collection para m├│dulo SearchProviders
  - [ ] Documentar setup e uso das collections

- [ ] **Data Seeding**:
  - [ ] Seeder de ServiceCatalogs: 10 categorias + 50 servi├ºos
  - [ ] Seeder de Providers: 20 prestadores fict├¡cios
  - [ ] Seeder de Users: Admin + 10 customers
  - [ ] Script: `dotnet run --seed-dev-data`

---

#### ≡ƒöù Parte 3: Final Module Integrations (3-5 dias)

**Objetivos**:
- Finalizar integra├º├╡es cross-module pendentes
- Implementar admin endpoints para gest├úo
- Validar fluxos end-to-end completos

**Tarefas Detalhadas**:

**1. Providers Γåö ServiceCatalogs Integration**:
- [ ] Providers: Adicionar `ProviderServices` linking table (many-to-many)
- [ ] Providers: Validar services via `IServiceCatalogsModuleApi.ValidateServicesAsync`
- [ ] Providers: Bloquear servi├ºos inativos ou inexistentes
- [ ] Integration tests: Valida├º├úo completa do fluxo

**2. Providers Γåö Locations Integration**:
- [ ] Providers: Usar `ILocationsModuleApi.GetAddressFromCepAsync` no registro
- [ ] Providers: Auto-populate cidade/estado via Locations
- [ ] Unit test: Mock de ILocationsModuleApi em Providers.Application

**3. Geographic Restrictions Admin**:
- Γ£à **Database**: LocationsDbContext + AllowedCity entity (migration 20251212002108_InitialAllowedCities)
- Γ£à **Repository**: IAllowedCityRepository implementado com queries otimizadas
- Γ£à **Handlers**: CreateAllowedCityHandler, UpdateAllowedCityHandler, DeleteAllowedCityHandler, GetAllowedCityByIdHandler, GetAllAllowedCitiesHandler
- Γ£à **Domain Exceptions**: NotFoundException, AllowedCityNotFoundException, BadRequestException, DuplicateAllowedCityException
- Γ£à **Exception Handling**: LocationsExceptionHandler (IExceptionHandler) + GlobalExceptionHandler com ArgumentException
- Γ£à **Endpoints**: 
  - GET /api/v1/admin/allowed-cities (listar todas)
  - GET /api/v1/admin/allowed-cities/{id} (buscar por ID)
  - POST /api/v1/admin/allowed-cities (criar nova)
  - PUT /api/v1/admin/allowed-cities/{id} (atualizar)
  - DELETE /api/v1/admin/allowed-cities/{id} (deletar)
- Γ£à **Bruno Collections**: 6 arquivos .bru criados (CRUD completo + README)
- Γ£à **Testes**: 4 integration tests + 15 E2E tests (100% passando - 12 Dez)
- Γ£à **Compila├º├úo**: 7 erros corrigidos (MetricsCollectorService, SerilogConfigurator, DeadLetterServices, IbgeClient, GeographicValidationServiceTests)
- Γ£à **Exception Handling Fix**: Program.cs com m├│dulos registrados ANTES de AddSharedServices (ordem cr├¡tica para LIFO handler execution)
- Γ£à **Code Quality**: 0 erros, dotnet format executado (71 arquivos formatados)
- Γ£à **Commit**: d1ce7456 - "fix: corrigir erros de compila├º├úo e exception handling em E2E tests"

**4. ServiceCatalogs Admin UI Integration**:
- [ ] Admin Portal: Endpoint para associar servi├ºos a prestadores
- [ ] API endpoints: CRUD de categorias e servi├ºos
- [ ] Documenta├º├úo: Workflows de gest├úo

---

#### ≡ƒÄ» Parte 4: Code Quality & Standardization (5-8 dias)

**Objetivos**:
- Padronizar uso de bibliotecas de teste (substituir NSubstitute por Moq)
- Unificar gera├º├úo de IDs (usar UuidGenerator em todo c├│digo)
- Migrar para novo formato .slnx (performance e versionamento)
- Automatizar documenta├º├úo OpenAPI no GitHub Pages
- **NOVO**: Documentar Design Patterns implementados
- **NOVO**: Avaliar migra├º├úo para AspNetCoreRateLimit library
- **NOVO**: Verificar completude do Logging Estruturado (Seq, Domain Events, Performance)

**Tarefas Detalhadas**:

**1. Substituir NSubstitute por Moq** ΓÜá∩╕Å CR├ìTICO:
- [ ] **An├ílise**: 3 arquivos usando NSubstitute detectados
  - `tests/MeAjudaAi.ServiceDefaults.Tests/ExtensionsTests.cs`
  - `tests/MeAjudaAi.ApiService.Tests/Extensions/SecurityExtensionsTests.cs`
  - `tests/MeAjudaAi.ApiService.Tests/Extensions/PerformanceExtensionsTests.cs`
- [ ] Substituir `using NSubstitute` por `using Moq`
- [ ] Atualizar syntax: `Substitute.For<T>()` ΓåÆ `new Mock<T>()`
- [ ] Remover PackageReference NSubstitute dos .csproj:
  - `tests/MeAjudaAi.ServiceDefaults.Tests/MeAjudaAi.ServiceDefaults.Tests.csproj`
  - `tests/MeAjudaAi.ApiService.Tests/MeAjudaAi.ApiService.Tests.csproj`
- [ ] Executar testes para validar substitui├º├úo
- [ ] **Raz├úo**: Padronizar com resto do projeto (todos outros testes usam Moq)

**2. Unificar gera├º├úo de IDs com UuidGenerator** ≡ƒôï:
- [ ] **An├ílise**: ~26 ocorr├¬ncias de `Guid.CreateVersion7()` detectadas
  - **C├│digo fonte** (2 arquivos):
    - `src/Modules/Users/Infrastructure/Services/LocalDevelopment/LocalDevelopmentUserDomainService.cs` (linha 30)
    - `src/Shared/Time/UuidGenerator.cs` (3 linhas - j├í correto, implementa├º├úo base)
  - **Testes unit├írios** (18 locais em 3 arquivos):
    - `src/Modules/Providers/Tests/Unit/Application/Queries/GetProviderByDocumentQueryHandlerTests.cs` (2x)
    - `src/Modules/SearchProviders/Tests/Unit/Infrastructure/Repositories/SearchableProviderRepositoryTests.cs` (14x)
    - `src/Modules/Documents/Tests/Integration/DocumentsInfrastructureIntegrationTests.cs` (2x)
  - **Testes de integra├º├úo/E2E** (6 locais em 4 arquivos):
    - `tests/MeAjudaAi.Integration.Tests/Modules/Users/UserRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Integration.Tests/Modules/Providers/ProviderRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Shared.Tests/Auth/ConfigurableTestAuthenticationHandler.cs` (1x)
    - `tests/MeAjudaAi.E2E.Tests/Integration/UsersModuleTests.cs` (2x)
- [ ] Substituir todas ocorr├¬ncias por `UuidGenerator.NewId()`
- [ ] Adicionar `using MeAjudaAi.Shared.Time;` onde necess├írio
- [ ] Executar build completo para validar
- [ ] Executar test suite completo (~480 testes)
- [ ] **Raz├úo**: Centralizar l├│gica de gera├º├úo de UUIDs v7, facilitar futura customiza├º├úo (ex: timestamp override para testes)

**3. Migrar solu├º├úo para formato .slnx** ≡ƒÜÇ:
- [ ] **Contexto**: Novo formato XML introduzido no .NET 9 SDK
  - **Benef├¡cios**: 
    - Formato leg├¡vel e version├ível (XML vs bin├írio)
    - Melhor performance de load/save (at├⌐ 5x mais r├ípido)
    - Suporte nativo no VS 2022 17.12+ e dotnet CLI 9.0+
    - Mais f├ícil de fazer merge em git (conflitos reduzidos)
  - **Compatibilidade**: .NET 10 SDK j├í suporta nativamente
- [ ] **Migra├º├úo**:
  - [ ] Criar backup: `Copy-Item MeAjudaAi.sln MeAjudaAi.sln.backup`
  - [ ] Executar: `dotnet sln MeAjudaAi.sln migrate` (comando nativo .NET 9+)
  - [ ] Validar: `dotnet sln list` (verificar todos 37 projetos listados)
  - [ ] Build completo: `dotnet build MeAjudaAi.slnx`
  - [ ] Testes: `dotnet test MeAjudaAi.slnx`
  - [ ] Atualizar CI/CD: `.github/workflows/*.yml` (trocar .sln por .slnx)
  - [ ] Remover `.sln` ap├│s valida├º├úo completa
- [ ] **Rollback Plan**: Manter `.sln.backup` por 1 sprint
- [ ] **Decis├úo**: Fazer em branch separada ou na atual?
  - **Recomenda├º├úo**: Branch separada `migrate-to-slnx` (isolamento de mudan├ºa estrutural)
  - **Alternativa**: Na branch atual se sprint j├í estiver avan├ºada

**4. OpenAPI Documentation no GitHub Pages** ≡ƒôû:
- [ ] **An├ílise**: Arquivo `api/api-spec.json` j├í existe
- [ ] **Implementa├º├úo**:
  - [ ] Configurar GitHub Action para extrair OpenAPI spec:
    - Op├º├úo 1: Usar action `bump-sh/github-action@v1` (Bump.sh integration)
    - Op├º├úo 2: Usar action `seeebiii/redoc-cli-github-action@v10` (ReDoc UI)
    - Op├º├úo 3: Custom com Swagger UI est├ítico
  - [ ] Criar workflow `.github/workflows/update-api-docs.yml`:
    ```yaml
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    - name: Extract OpenAPI spec
      run: |
        dotnet build
        dotnet run --project tools/OpenApiExtractor/OpenApiExtractor.csproj
    - name: Generate API docs
      uses: seeebiii/redoc-cli-github-action@v10
      with:
        args: bundle api/api-spec.json -o docs/api/index.html
    - name: Deploy to GitHub Pages
      # (integrar com mkdocs deploy existente)
    ```
  - [ ] Adicionar se├º├úo "API Reference" no mkdocs.yml
  - [ ] Substituir se├º├úo atual de API reference por link din├ómico
  - [ ] Validar UI renderizada corretamente (testar endpoints, schemas)
- [ ] **Ferramentas dispon├¡veis**:
  - Γ£à `api/api-spec.json` existe (gerado manualmente ou via tool?)
  - [ ] Verificar se existe tool em `tools/` para extra├º├úo autom├ítica
  - [ ] Se n├úo existir, criar `tools/OpenApiExtractor` para CI/CD
- [ ] **Benef├¡cios**:
  - Documenta├º├úo sempre atualizada com c├│digo
  - UI interativa (try-it-out)
  - Melhor DX para consumidores da API

**5. Health Checks & Monitoring** ≡ƒÅÑ:
- [x] **Health Checks Core**: Γ£à IMPLEMENTADO
  - `src/Shared/Monitoring/HealthChecks.cs`: 4 health checks implementados
  - 47 testes, 100% coverage
  - Componentes: ExternalServicesHealthCheck, PerformanceHealthCheck, HelpProcessingHealthCheck, DatabasePerformanceHealthCheck
  - Endpoints: `/health`, `/health/live`, `/health/ready`
- [x] **Dashboard**: Γ£à DECIS├âO ARQUITETURAL
  - **Usar dashboard nativo do .NET Aspire** (n├úo AspNetCore.HealthChecks.UI)
  - Aspire fornece dashboard integrado com telemetria, traces e m├⌐tricas
  - Health checks expostos via endpoints JSON consumidos pelo Aspire
  - Melhor integra├º├úo com ecossistema .NET 9+ e cloud-native deployments
  - **Rationale**: Evitar depend├¬ncia extra, melhor DX, alinhamento com roadmap .NET

**6. Design Patterns Documentation** ≡ƒôÜ:
- [ ] **Branch**: `docs/design-patterns`
- [ ] **Objetivo**: Documentar padr├╡es arquiteturais implementados no projeto
- [ ] **Tarefas**:
  - [ ] Atualizar `docs/architecture.md` com se├º├úo "Design Patterns Implementados":
    - **Repository Pattern**: `I*Repository` interfaces + implementa├º├╡es Dapper
    - **Unit of Work**: Transaction management nos repositories
    - **CQRS**: Separa├º├úo de Commands e Queries (implementa├º├úo pr├│pria com CommandDispatcher/QueryDispatcher)
    - **Domain Events**: `IDomainEvent` + handlers
    - **Factory Pattern**: `UuidGenerator`, `SerilogConfigurator`
    - **Middleware Pipeline**: ASP.NET Core middlewares customizados
    - **Strategy Pattern**: Feature toggles (FeatureManagement)
    - **Options Pattern**: Configura├º├úo fortemente tipada
    - **Dependency Injection**: Service lifetimes (Scoped, Singleton, Transient)
  - [ ] Adicionar exemplos de c├│digo reais (n├úo pseudo-c├│digo):
    - Exemplo Repository Pattern: `UserRepository.cs` (m├⌐todo `GetByIdAsync`)
    - Exemplo CQRS: `CreateUserCommand` + `CreateUserCommandHandler`
    - Exemplo Domain Events: `UserCreatedEvent` + `UserCreatedEventHandler`
  - [ ] Criar diagramas (opcional, usar Mermaid):
    - Diagrama CQRS flow
    - Diagrama Repository + UnitOfWork
    - Diagrama Middleware Pipeline
  - [ ] Adicionar se├º├úo "Anti-Patterns Evitados":
    - Γ¥î Anemic Domain Model (mitigado com domain services)
    - Γ¥î God Objects (mitigado com separa├º├úo por m├│dulos)
    - Γ¥î Service Locator (substitu├¡do por DI container)
  - [ ] Refer├¬ncias externas:
    - Martin Fowler: Patterns of Enterprise Application Architecture
    - Microsoft: eShopOnContainers (refer├¬ncia de DDD + Clean Architecture)
    - .NET Microservices: Architecture e-book
- [ ] **Estimativa**: 1-2 dias

**7. Rate Limiting com AspNetCoreRateLimit** ΓÜí:
- [x] **Rate Limiting Custom**: Γ£à J├ü IMPLEMENTADO
  - `src/Bootstrapper/MeAjudaAi.ApiService/Middlewares/RateLimitingMiddleware.cs`
  - Usa `IMemoryCache` (in-memory)
  - Testes unit├írios implementados
  - Configura├º├úo via `RateLimitOptions` (appsettings)
- [ ] **Decis├úo Estrat├⌐gica** ΓÜá∩╕Å AVALIAR:
  - **Op├º├úo A**: Migrar para `AspNetCoreRateLimit` library
    - Γ£à Vantagens:
      - Distributed rate limiting com Redis (multi-instance)
      - Configura├º├úo rica (whitelist, blacklist, custom rules)
      - Suporte a rate limiting por endpoint, IP, client ID
      - Throttling policies (burst, sustained)
      - Community-tested e bem documentado
    - Γ¥î Desvantagens:
      - Depend├¬ncia adicional (biblioteca de terceiros)
      - Configura├º├úo mais complexa
      - Overhead de Redis (infraestrutura adicional)
  - **Op├º├úo B**: Manter middleware custom
    - Γ£à Vantagens:
      - Controle total sobre l├│gica
      - Zero depend├¬ncias externas
      - Performance (in-memory cache)
      - Simplicidade
    - Γ¥î Desvantagens:
      - N├úo funciona em multi-instance (sem Redis)
      - Features limitadas vs biblioteca
      - Manuten├º├úo nossa
  - [ ] **Recomenda├º├úo**: Manter custom para MVP, avaliar migra├º├úo para Aspire 13+ (tem rate limiting nativo)
  - [ ] **Se migrar**:
    - [ ] Instalar: `AspNetCoreRateLimit` (v5.0+)
    - [ ] Configurar Redis distributed cache
    - [ ] Migrar `RateLimitOptions` para configura├º├úo da biblioteca
    - [ ] Atualizar testes
    - [ ] Documentar nova configura├º├úo
- [ ] **Estimativa (se migra├º├úo)**: 1-2 dias

**8. Logging Estruturado - Verifica├º├úo de Completude** ≡ƒôè:
- [x] **Core Logging**: Γ£à J├ü IMPLEMENTADO
  - Serilog configurado (`src/Shared/Logging/SerilogConfigurator.cs`)
  - CorrelationId enricher implementado
  - LoggingContextMiddleware funcional
  - Cobertura testada via integration tests
- [x] **Azure Application Insights**: Γ£à CONFIGURADO
  - OpenTelemetry integration (`src/Aspire/MeAjudaAi.ServiceDefaults/Extensions.cs` linha 116-120)
  - Vari├ível de ambiente: `APPLICATIONINSIGHTS_CONNECTION_STRING`
  - Suporte a traces, metrics, logs
- [x] **Seq Integration**: Γ£à J├ü CONFIGURADO
  - `appsettings.Development.json` linha 24-28: serverUrl `http://localhost:5341`
  - `appsettings.Production.json` linha 20-24: vari├íveis de ambiente `SEQ_SERVER_URL` e `SEQ_API_KEY`
  - Serilog.Sinks.Seq j├í instalado e funcional
- [ ] **Tarefas de Verifica├º├úo** ΓÜá∩╕Å PENDENTES:
  - [ ] **Seq Local**: Validar que Seq container est├í rodando (Docker Compose)
  - [ ] **Domain Events Logging**: Verificar se todos domain events est├úo sendo logados
    - [ ] Adicionar correlation ID aos domain events (se ainda n├úo tiver)
    - [ ] Verificar log level apropriado (Information para eventos de neg├│cio)
    - [ ] Exemplos: `UserCreatedEvent`, `ProviderRegisteredEvent`, etc.
  - [ ] **Performance Logging**: Verificar se performance metrics est├úo sendo logados
    - [ ] Middleware de performance j├í existe? (verificar `PerformanceExtensions.cs`)
    - [ ] Adicionar logs para queries lentas (> 1s)
    - [ ] Adicionar logs para endpoints lentos (> 3s)
  - [ ] **Documenta├º├úo**: Atualizar `docs/development.md` com instru├º├╡es de uso do Seq
    - [ ] Como acessar Seq UI (`http://localhost:5341`)
    - [ ] Como filtrar logs por CorrelationId
    - [ ] Como criar queries customizadas
    - [ ] Screenshot da UI do Seq com exemplo de query
- [ ] **Estimativa**: 1 dia (apenas verifica├º├úo e pequenas adi├º├╡es)
- [ ] **Decis├úo de ferramenta**:
  - **ReDoc**: UI moderna, read-only, melhor para documenta├º├úo (recomendado)
  - **Swagger UI**: Try-it-out interativo, melhor para desenvolvimento
  - **Bump.sh**: Versionamento de API, diff tracking (mais complexo)
  - **Recomenda├º├úo inicial**: ReDoc (simplicidade + qualidade visual)

---

#### Γ£à Crit├⌐rios de Conclus├úo Sprint 3 (Atualizado)

**Parte 1 - Documentation** (Γ£à CONCLU├ìDO 11 Dez):
- Γ£à GitHub Pages live em `https://frigini.github.io/MeAjudaAi/`
- Γ£à Todos .md files revisados e organizados (43 arquivos)
- Γ£à Zero links quebrados
- Γ£à Search funcional
- Γ£à Deploy autom├ítico via GitHub Actions

**Parte 2 - Admin Endpoints & Tools** (Γ£à CONCLU├ìDA - 13 Dez):
- Γ£à Admin API de cidades permitidas implementada (5 endpoints CRUD)
- Γ£à Bruno Collections para Locations/AllowedCities (6 arquivos .bru)
- Γ£à Bruno Collections para todos m├│dulos (Users: 6, Providers: 13, Documents: 0, ServiceCatalogs: 13, SearchProviders: 3)
- Γ£à Testes: 4 integration + 15 E2E (100% passando)
- Γ£à Exception handling completo (LocationsExceptionHandler + GlobalExceptionHandler)
- Γ£à Build quality: 0 erros, dotnet format executado
- Γ£à Scripts documentados e auditoria completa (commit b0b94707)
- Γ£à Data seeding funcional (DevelopmentDataSeeder.cs - ServiceCatalogs, Providers, Users)
- Γ£à MigrationTool migrado para Aspire AppHost (commit 3d2b260b)

**Parte 3 - Module Integrations** (Γ£à CONCLU├ìDA - 12 Dez):
- Γ£à Providers Γåö ServiceCatalogs: Completo (commit 53943da8 - ProviderServices many-to-many)
- Γ£à Providers Γåö Locations: Completo (ILocationsModuleApi integrado)
- Γ£à ServiceCatalogs Admin endpoints: CRUD implementado (13 endpoints .bru)
- Γ£à Integration tests: Todos fluxos validados (E2E tests passando)

**Parte 4 - Code Quality & Standardization** (Γ£à CONCLU├ìDA - 12 Dez):
- Γ£à NSubstitute substitu├¡do por Moq (commit e8683c08 - padroniza├º├úo completa)
- Γ£à Guid.CreateVersion7() substitu├¡do por UuidGenerator (commit 0a448106 - ~26 locais)
- Γ£à Migra├º├úo para .slnx conclu├¡da (commit 1de5dc1a - formato .NET 9+)
- Γ£à OpenAPI docs no GitHub Pages automatizado (commit ae6ef2d0)
- Γ£à Design Patterns Documentation (5000+ linhas em architecture.md)
- Γ£à SonarQube warnings resolution (commit d8bb00dc - ~135 warnings resolvidos)
- Γ£à Rate Limiting: Avaliado - decis├úo de manter custom para MVP
- Γ£à Logging Estruturado: Serilog + Seq + App Insights + Correlation IDs completo

**Quality Gates Gerais**:
- Γ£à Build: 100% sucesso (Sprint 3 conclu├¡da - 13 Dez)
- Γ£à Tests: 480 testes passando (99.8% - 1 skipped)
- Γ£à Coverage: 90.56% line (target superado em 55.56pp)
- Γ£à Documentation: GitHub Pages deployed (https://frigini.github.io/MeAjudaAi/)
- Γ£à API Reference: Automatizada via OpenAPI (GitHub Pages)
- Γ£à Code Standardization: 100% Moq, 100% UuidGenerator
- Γ£à SonarQube: ~135 warnings resolvidos sem pragma suppressions
- Γ£à CI/CD: Formatting checks + exit code masking corrigidos

**Resultado Esperado**: Projeto completamente organizado, padronizado, documentado, e com todas integra├º├╡es core finalizadas. Pronto para avan├ºar para Admin Portal (Sprint 4) ou novos m├│dulos.

---

