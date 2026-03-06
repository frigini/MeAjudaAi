# 🗺️ Roadmap - MeAjudaAi

Este documento consolida o planejamento estratégico e tático da plataforma MeAjudaAi, definindo fases de implementação, módulos prioritários e funcionalidades futuras.

---

## 📊 Sumário Executivo

**Projeto**: MeAjudaAi - Plataforma de Conexão entre Clientes e Prestadores de Serviços  
**Status Geral**: Consulte a [Tabela de Sprints](#cronograma-de-sprints) para o status detalhado atualizado.
**Cobertura de Testes**: Backend 90.56% | Frontend 30 testes bUnit  
**Stack**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 (Customer, Provider, Admin) + Tailwind v4

### Marcos Principais

Consulte a seção [Cronograma de Sprints](#cronograma-de-sprints) abaixo para o status detalhado e atualizado de cada sprint, e datas alvo (incluindo o MVP Launch).

**Procedimento de Revisão de Sprints**
As futuras atualizações da tabela de sprints devem observar a política: análise commit-by-commit newest-first, apresentando um veredicto conciso e resolvendo os follow-ups.

## ⚠️ Notas de Risco

- Estimativas assumem velocidade consistente e ausência de bloqueios maiores
- Primeiro projeto Blazor WASM pode revelar complexidade não prevista
- Sprint 9 reservado como buffer de contingência (não para novas features)

## 🏗️ Decisões Arquiteturais Futuras

### NX Monorepo (Frontend)

**Status**: ✅ Incluído no Sprint 8B.2  
**Branch**: `feature/sprint-8b2-technical-excellence`

**Motivação**: Com Customer Web App (Next.js), Provider App (próximo sprint), Admin Portal (migração planejada) e Mobile (React Native + Expo), o compartilhamento de código (componentes, hooks, tipos TypeScript, schemas Zod) entre os projetos se torna crítico. NX oferece:
- Workspace unificado com `libs/` compartilhadas
- Build cache inteligente (só reconstrói o que mudou)
- Dependency graph entre projetos
- Geração de código consistente

**Escopo (Sprint 8B.2)**:
- Migrar `MeAjudaAi.Web.Customer` para workspace NX
- Criar `apps/customer-web`, `apps/provider-web` (Sprint 8C), `apps/admin-web` (Sprint 8D), `apps/mobile` (Sprint 8E)
- Criar `libs/ui` (componentes compartilhados), `libs/auth`, `libs/api-client`
- Atualizar `.NET Aspire AppHost` para apontar para nova estrutura
- Atualizar CI/CD para usar `nx affected`

**Decisão de antecipação**: NX foi antecipado do pós-MVP para o Sprint 8B.2 porque o Provider App (Sprint 8C) e a migração Admin (Sprint 8D) se beneficiam diretamente do workspace unificado. Criar o NX antes desses projetos evita migração posterior mais custosa.

---

### Migração Admin Portal: Blazor WASM → React

**Status**: ⏳ Planejado — Sprint 8D (após Provider App)

**Análise (Atualizada Março 2026)**:

| Fator | Manter Blazor | Migrar para React |
|-------|--------------|-------------------|
| Custo | ✅ Zero | ❌ Alto (reescrever ~5000+ linhas) |
| Compartilhamento C# DTOs | ✅ Nativo | ⚠️ Requer API client gerado (libs/api-client via NX) |
| Uso interno (não SEO) | ✅ Blazor adequado | ✅ React com NX compartilha componentes |
| Unificação de stack | ❌ Dual-stack (Blazor + React) | ✅ Single-stack React (3 apps no NX) |
| Hiring | ⚠️ Blazor nicho | ✅ React mais fácil |
| Shared Components | ❌ Isolado do NX | ✅ Reutiliza libs/ui, libs/auth do NX |

**Decisão Revisada (Março 2026)**: **Migrar para React** dentro do workspace NX. Com a adoção do NX Monorepo (Sprint 8B.2) e o Provider App (Sprint 8C) como segundo app React, manter o Admin em Blazor cria uma ilha isolada que não se beneficia dos componentes compartilhados (`libs/ui`, `libs/auth`, `libs/api-client`). A unificação de stack reduz complexidade operacional e facilita manutenção.

**Sequência**: Provider App (Sprint 8C) → Admin Migration (Sprint 8D). O Provider App estabelece os padrões e shared libs que a migração Admin reutilizará.

---

## 🎯 Status Atual

**📅 Sprint 8B concluído**: Fevereiro/Março de 2026 (Finalizado em 4 de Março de 2026)

### ✅ Sprint 8A - Customer Web App & Test Optimization - CONCLUÍDA (5-13 Fev 2026)

**Objetivos**:
1. ✅ **Integrar Service Tags com Backend**
2. ✅ **Implementar Filtros Avançados de Busca**
3. ✅ **Otimizar Testes E2E (Redução de Tempo)**

**Progresso Atual**: 3/3 objetivos completos ✅ **SPRINT 8A CONCLUÍDO 100%!**

**Funcionalidades Entregues**:
- **Service Tags**: Integração com API para carregar serviços populares dinamicamente (`service-catalog.ts`).
- **Busca Avançada**: Filtros de Categoria, Avaliação (Rating) e Distância (Raio) implementados na UI (`SearchFilters.tsx`) e integrados com API de busca.
- **Frontend Integration**: `SearchPage` atualizado para processar novos parâmetros de filtro e mapear categorias para IDs de serviço.

**Otimização de Testes**:
- **Problema**: Testes E2E lentos devido a acúmulo de dados (40m+).
- **Solução**: Implementado `IAsyncLifetime` e `CleanupDatabaseAsync()` em **todas** as classes de teste E2E (`Documents`, `Locations`, `Providers`, `ServiceCatalogs`, `Users`).
- **Resultado**: Testes rodam com banco limpo a cada execução, prevenindo degradação de performance e falhas por dados sujos (Race Conditions).
- `parallelizeTestCollections`: Controla se coleções de teste executam em paralelo no xUnit. Confirmado que `parallelizeTestCollections: false` é necessário para DbContext com TestContainers, pois banco compartilhado causa lock conflicts.
---

### ✅ Sprint 8B.1 - Provider Onboarding & Registration Experience - CONCLUÍDA (Março 2026)

**Objetivos**:
1. ✅ **Multi-step Provider Registration**: Implementar UI de "Torne-se um Prestador" com Stepper unificado.
2. ✅ **Fix Backend Reliability**: Resolver erros 500 nos endpoints críticos de prestador.
3. ✅ **Visual Alignment**: Alinhar design do prestador com o fluxo de cliente.

**Avanços Entregues**:
- **Stepper UI**: Componente de linha do tempo implementado em `/cadastro/prestador`, guiando o usuário pelas etapas de Dados Básicos, Endereço e Documentos.
- **Correção de API (Critical)**: Resolvido erro de resolução de DI para `RegisterProviderCommandHandler`, permitindo a criação de perfis sem falhas internas (500).
- **Onboarding Flow**: Implementação da lógica de transição entre passos 1 (Dados Básicos) e 2 (Endereço), com persistência correta no banco de dados.
- **Validation**: Integração com esquema de validação existente e tratamento de erros amigável no frontend.

**Próximos Passos (Pendentes)**:
- ⏳ **Document Upload (Step 3)**: Implementar componente de upload de documentos no fluxo de onboarding do prestador.
- ⏳ **Review Dashboard**: Criar interface para o prestador acompanhar o status de sua verificação (hoje parado em `pendingBasicInfo`).
- ⏳ **Professional Profile Setup**: Permitir que o prestador selecione categorias e serviços logo após o credenciamento básico.

---

### ⏳ Sprint 8B.2 - Technical Excellence & NX Monorepo (Planejado - Antes do Provider App)

**Branch**: `feature/sprint-8b2-technical-excellence`

**Objetivos**:
1. ⏳ **Messaging Unification (RabbitMQ Only)**: Remover completamente o Azure Service Bus da solução.
    - **Execução**:
        - Remover pacotes `.Azure.ServiceBus` de todos os projetos.
        - Unificar `MassTransit` configuration em `ServiceDefaults`.
        - Atualizar scripts de infra (`docker-compose.yaml`) para foco total em RabbitMQ.
        - Remover segredos e vars de ambiente do ASB no Azure/Staging.
    - **Sucesso**: Aplicação funcionando sem dependência do Azure Service Bus local ou remoto.
2. ⏳ **Backend Integration Test Optimization**: Reduzir o tempo de execução (hoje ~30 min).
    - **Execução**:
        - Migrar os ~20 projetos de teste restantes para o padrão `RequiredModules`.
        - Implementar `Respawn` ou similar para limpeza ultra-rápida de banco em vez de migrations completas.
        - Otimizar recursos do TestContainers (reuse containers entre runs se possível).
    - **Sucesso**: Suíte completa de integração rodando em < 10 minutos.
3. ⏳ **Slug Implementation**: Substituir IDs por Slugs nas rotas de perfil de prestador para maior segurança e SEO.
    - **Execução**:
        - Backend: Adicionar `Slug` ao `BusinessProfile` entity.
        - Backend: Implementar `slugify` logic e garantir unicidade no Persistence layer.
        - UI: Alterar rotas de `/prestador/[id]` para `/prestador/[slug]`.
        - SEO: Adicionar canonical tags e metadados dinâmicos baseados no slug.
    - **Sucesso**: Navegar via slug e manter compatibilidade com IDs antigos (301 redirect).
4. ⏳ **Frontend Testing & CI/CD Suite**: Implementar suíte completa de testes no Next.js.
    - **Contexto**: Baseado no [Plano de Testes Robusto](./testing/frontend-testing-plan.md).
    - **Execução**:
        - Setup do projeto `tests/MeAjudaAi.Web.Customer.Tests`.
        - Implementar Mocks de API com MSW para os fluxos de busca e perfil.
        - Criar o primeiro pipeline `.github/workflows/frontend-quality.yml`.
        - Integrar SonarCloud (SonarQube) para análise estática de TS/React.
    - **Sucesso**: Pipeline falhando se testes não passarem ou qualidade cair.
5. ⏳ **NX Monorepo Setup**: Configurar workspace NX para gerenciar todos os projetos frontend.
    - **Execução**:
        - Inicializar workspace NX na raiz do projeto.
        - Migrar `MeAjudaAi.Web.Customer` (Next.js) para `apps/customer-web`.
        - Criar shared libs: `libs/ui`, `libs/auth`, `libs/api-client`.
        - Extrair componentes compartilhados do Customer App para `libs/ui`.
        - Atualizar `.NET Aspire AppHost` para apontar para nova estrutura NX.
        - Atualizar CI/CD para usar `nx affected`.
        - Scaffolding `apps/provider-web` (vazio, será implementado no Sprint 8C).
    - **Sucesso**: Customer Web App rodando dentro do workspace NX com build e testes funcionais.

---

### ✅ Sprint 7.10 - Accessibility Features - CONCLUÍDA (16 Jan 2026)
### ✅ Sprint 7.11 - Error Boundaries - CONCLUÍDA (16 Jan 2026) 
### ✅ Sprint 7.12 - Performance Optimizations - CONCLUÍDA (16 Jan 2026)
### ✅ Sprint 7.13 - Standardized Error Handling - CONCLUÍDA (16 Jan 2026)
### ✅ Sprint 7.14 - Complete Localization (i18n) - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

### ✅ Sprint 7.9 - Magic Strings Elimination - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Objetivos**:
1. ✅ **Configuração Aspire com Pacotes NuGet Locais** - Resolver erro DCP/Dashboard paths
2. ✅ **Eliminação de Warnings** - 0 warnings em toda a solução
3. ✅ **Scripts de Automação** - Facilitar setup e execução
4. ✅ **Documentação** - Instruções claras de inicialização

**Progresso Atual**: 4/4 objetivos completos ✅ **SPRINT 7.5 CONCLUÍDO!**

**Detalhamento - Configuração Aspire** ✅:
- Directory.Build.targets criado no AppHost com propriedades MSBuild
- Propriedades `CliPath` e `DashboardPath` configuradas automaticamente
- Detecta pacotes locais em `packages/` (aspire.hosting.orchestration.win-x64 13.1.0)
- Target de validação com mensagens de erro claras
- launchSettings.json criado com variáveis de ambiente (ASPNETCORE_ENVIRONMENT, POSTGRES_PASSWORD)
- Keycloak options com senha padrão "postgres" para desenvolvimento
- Aspire SDK atualizado de 13.0.2 para 13.1.0 (sincronizado com global.json)
- Workaround documentado em docs/known-issues/aspire-local-packages.md
- Commits: 95f52e79 "fix: configurar caminhos Aspire para pacotes NuGet locais"

**Detalhamento - Eliminação de Warnings** ✅:
- Admin Portal: Directory.Build.props com NoWarn para 11 tipos de warnings
  - CS8602 (null reference), S2094 (empty records), S3260 (sealed), S2953 (Dispose)
  - S2933 (readonly), S6966 (await async), S2325 (static), S5693 (content length)
  - MUD0002 (MudBlazor casing), NU1507 (package sources), NU1601 (dependency version)
- MudBlazor atualizado de 7.21.0 para 8.0.0 em Directory.Packages.props
- .editorconfig criado no Admin Portal com documentação de supressões
- **Resultado**: Build completo com 0 warnings, 0 erros
- Commit: 60cbb060 "fix: eliminar todos os warnings de NuGet"

**Detalhamento - Scripts de Automação** ✅:
- `scripts/setup.ps1`: Script de setup inicial com validação de pré-requisitos
  - Verifica .NET SDK 10.0.101, Docker Desktop, Git
  - Executa dotnet restore e build
  - Exibe instruções de configuração do Keycloak
- `scripts/dev.ps1`: Script de desenvolvimento diário
  - Valida Docker e .NET SDK
  - Restaura dependências
  - Inicia Aspire AppHost
  - Define variáveis de ambiente (POSTGRES_PASSWORD, ASPNETCORE_ENVIRONMENT)
- `scripts/README.md`: Documentação completa dos scripts
- `.vscode/launch.json` e `.vscode/tasks.json`: Configuração para debugging

**Detalhamento - Documentação** ✅:
- README.md atualizado com seção "⚡ Setup em 2 Comandos"
- Tabela de scripts com descrição e uso
- Pré-requisitos claramente listados
- docs/known-issues/aspire-local-packages.md: Workaround documentado
  - Descrição do problema (bug Aspire com globalPackagesFolder)
  - 3 soluções alternativas (VS Code F5, Visual Studio, configuração manual)
  - Link para issue upstream: [dotnet/aspire#6789](https://github.com/dotnet/aspire/issues/6789)
- Scripts de build: Unix/Linux Makefile e PowerShell scripts (ver `build/` directory)

**Resultado Alcançado**:
- ✅ Aspire AppHost inicia corretamente via F5 ou scripts
- ✅ 0 warnings em toda a solução (40 projetos)
- ✅ Setup automatizado em 2 comandos PowerShell
- ✅ Documentação completa de inicialização
- ✅ Experiência de desenvolvimento melhorada
- ✅ 16 arquivos modificados, 588 adições, 109 deleções

---

### ✅ Sprint 7.6 - Otimização de Testes de Integração - CONCLUÍDA (12 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Após Sprint 7.5, testes de integração apresentaram timeouts intermitentes. Investigação revelou que BaseApiTest aplicava migrations de TODOS os 6 módulos para CADA teste, causando esgotamento do pool de conexões PostgreSQL (erro 57P01).

**Problema Identificado**:
- ❌ Teste `DocumentRepository_ShouldBeRegisteredInDI` passa na master (15s)
- ❌ Mesmo teste falha no fix/aspire-initialization com timeout (~14s)
- ❌ Erro PostgreSQL: `57P01: terminating connection due to administrator command`
- ❌ Causa raiz: BaseApiTest aplica migrations dos 6 módulos sequencialmente (~60-70s)

**Investigação Realizada**:
1. ❌ Tentativa 1: Remover migration vazia SyncModel → Ainda falha
2. ❌ Tentativa 2: Remover PostGIS extension annotation → Ainda falha
3. ❌ Tentativa 3: Adicionar CloseConnectionAsync após migrations → Ainda falha
4. ✅ **Insight do usuário**: "qual cenário o teste quebra? é um cenário real? é um teste necessário?"
5. ✅ **Descoberta**: Teste só verifica DI registration, não precisa de migrations!
6. ✅ **Root cause**: ALL tests aplica ALL modules migrations desnecessariamente

**Solução Implementada: Migrations Sob Demanda (On-Demand Migrations)**

**1. TestModule Enum com Flags** ✅
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

**2. RequiredModules Virtual Property** ✅
```csharp
/// <summary>
/// Override this property in your test class to specify which modules are required.
/// Default is TestModule.All for backward compatibility.
/// </summary>
protected virtual TestModule RequiredModules => TestModule.All;
```

**3. ApplyRequiredModuleMigrationsAsync Method** ✅
- Verifica flags de RequiredModules
- Aplica EnsureCleanDatabaseAsync apenas uma vez
- Aplica migrations SOMENTE para módulos especificados
- Fecha conexões após cada módulo
- Seeds Locations test data se Locations module requerido

**4. EnsureCleanDatabaseAsync Method** ✅
- Extraído do legacy ApplyMigrationsAsync
- Manuseia PostgreSQL startup retry logic (erro 57P03)
- 10 tentativas com linear backoff (1s, 2s, 3s, ...)

**Arquivos Modificados** ✅:
- `tests/MeAjudaAi.Integration.Tests/Base/BaseApiTest.cs`: Refactoring completo
  - Lines 29-49: TestModule enum
  - Lines 51-67: RequiredModules property + documentação
  - Lines 363-453: ApplyRequiredModuleMigrationsAsync (novo)
  - Lines 455-484: EnsureCleanDatabaseAsync (extraído)
  - Lines 486+: ApplyMigrationsAsync marcado como `@deprecated`

- `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentsIntegrationTests.cs`:
  ```csharp
  protected override TestModule RequiredModules => TestModule.Documents;
  ```

- **5 Test Classes Otimizados**:
  - UsersIntegrationTests → `TestModule.Users`
  - ProvidersIntegrationTests → `TestModule.Providers`
  - ServiceCatalogsIntegrationTests → `TestModule.ServiceCatalogs`
  - DocumentsApiTests → `TestModule.Documents`

- `tests/MeAjudaAi.Integration.Tests/README.md`: Nova seção "⚡ Performance Optimization: RequiredModules"

**Resultados Alcançados** ✅:
- ✅ **Performance**: 83% faster para testes single-module (10s vs 60s)
- ✅ **Confiabilidade**: Eliminou timeouts do PostgreSQL (57P01 errors)
- ✅ **Isolamento**: Cada teste carrega apenas módulos necessários
- ✅ **Backward Compatible**: Default RequiredModules = TestModule.All
- ✅ **Realismo**: Espelha comportamento Aspire (migrations per-module)
- ✅ **Test Results**:
  - Antes: DocumentRepository_ShouldBeRegisteredInDI → TIMEOUT (~14s)
  - Depois: DocumentRepository_ShouldBeRegisteredInDI → ✅ PASS (~10s)

**Métricas de Comparação**:

| Cenário | Antes (All Modules) | Depois (Required Only) | Improvement |
|---------|---------------------|------------------------|-------------|
| Inicialização | ~60-70s | ~10-15s | **83% faster** |
| Migrations aplicadas | 6 módulos sempre | Apenas necessárias | Mínimo necessário |
| Timeouts | Frequentes | Raros/Eliminados | ✅ Estável |
| Pool de conexões | Esgotamento frequente | Isolado por módulo | ✅ Confiável |

**Outros Fixes** ✅:
- ✅ IHostEnvironment shadowing corrigido em 6 módulos (SearchProviders, ServiceCatalogs, Users, Providers, Documents, Locations)
- ✅ Removido teste redundante `IbgeApiIntegrationTests.GetMunicipioByNameAsync_Itaperuna_ShouldReturnValidMunicipio`
- ✅ Removida migration vazia `SearchProviders/20260112200309_SyncModel_20260112170301.cs`
- ✅ Analisados 3 testes skipped - todos validados como corretos

**Documentação Atualizada** ✅:
- ✅ tests/MeAjudaAi.Integration.Tests/README.md: Performance optimization guide
- ✅ docs/roadmap.md: Esta entrada (Sprint 7.6)
- ⏳ docs/architecture.md: Testing architecture (próximo)
- ⏳ docs/development.md: Developer guide para RequiredModules (próximo)
- ⏳ docs/technical-debt.md: Remover item de otimização de testes (próximo)

**Próximos Passos**:
1. Otimizar remaining 23 test classes com RequiredModules apropriados
2. Atualizar docs/architecture.md com diagrama de testing pattern
3. Atualizar docs/development.md com guia de uso
4. Atualizar docs/technical-debt.md removendo item resolvido

**Commits**:
- [hash]: "refactor: implement on-demand module migrations in BaseApiTest"
- [hash]: "docs: add RequiredModules optimization guide to tests README"

---

### ✅ Sprint 7.7 - Flux Pattern Refactoring - CONCLUÍDA (15-16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Após Sprint 7 Features, 5 páginas admin (Providers, Documents, Categories, Services, AllowedCities) ainda utilizavam direct API calls. Part 7 consistiu em refatorar todas para o padrão Flux/Redux com Fluxor, garantindo consistência arquitetural e single source of truth.

**Objetivos**:
1. ✅ **Refatorar Providers.razor** - Migrar Create/Update/Delete para Fluxor Actions
2. ✅ **Refatorar Documents.razor** - Remover direct API calls
3. ✅ **Refatorar Categories.razor** - Implementar Flux pattern completo
4. ✅ **Refatorar Services.razor** - Remover direct API calls
5. ✅ **Refatorar AllowedCities.razor** - Implementar Flux pattern completo
6. ✅ **Decisão Arquitetural sobre Dialogs** - Avaliar se refatorar ou manter pragmático
7. ✅ **Documentação Flux Pattern** - Criar guia de implementação completo

**Progresso Atual**: 7/7 objetivos completos ✅ **SPRINT 7.7 CONCLUÍDO 100%!**

**Implementações Realizadas** ✅:

**1. Providers.razor Refactoring** ✅ (Commit b98bac98):
- Removidos 95 linhas de código direto (APIs, handlers de sucesso/erro)
- Migrados todos métodos para Fluxor Actions
- Novo: `CreateProviderAction`, `UpdateProviderAction`, `DeleteProviderAction`, `UpdateVerificationStatusAction`
- ProvidersEffects implementado com todos side-effects
- ProvidersReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsVerifying`
- **Redução**: 95 linhas → 18 linhas (81% code reduction)

**2. Documents.razor Refactoring** ✅ (Commit 152a22ca):
- Removidos handlers diretos de upload e request verification
- Novo: `UploadDocumentAction`, `RequestDocumentVerificationAction`, `DeleteDocumentAction`
- DocumentsEffects com retry logic e error handling
- DocumentsReducer com estados `IsUploading`, `IsRequestingVerification`, `IsDeleting`
- **Redução**: 87 linhas → 12 linhas (86% code reduction)

**3. Categories.razor Refactoring** ✅ (Commit 1afa2daa):
- Removidos métodos `CreateCategory`, `UpdateCategory`, `DeleteCategory`, `ToggleActivation`
- Novo: `CreateCategoryAction`, `UpdateCategoryAction`, `DeleteCategoryAction`, `ToggleActivationAction`
- CategoriesEffects com validação de dependências (não deletar se tem serviços)
- CategoriesReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redução**: 103 linhas → 18 linhas (83% code reduction)

**4. Services.razor Refactoring** ✅ (Commit 399ee25b):
- Removidos métodos `CreateService`, `UpdateService`, `DeleteService`, `ToggleActivation`
- Novo: `CreateServiceAction`, `UpdateServiceAction`, `DeleteServiceAction`, `ToggleActivationAction`
- ServicesEffects com category validation
- ServicesReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redução**: 98 linhas → 18 linhas (82% code reduction)

**5. AllowedCities.razor Refactoring** ✅ (Commit 9ee405e0):
- Removidos métodos `CreateCity`, `UpdateCity`, `DeleteCity`, `ToggleActivation`
- Novo: `CreateAllowedCityAction`, `UpdateAllowedCityAction`, `DeleteAllowedCityAction`, `ToggleActivationAction`
- LocationsEffects com validação de coordenadas
- LocationsReducer com estados `IsCreating`, `IsUpdating`, `IsDeleting`, `IsTogglingActivation`
- **Redução**: 92 linhas → 14 linhas (85% code reduction)

**Métricas de Refactoring**:

| Página | Antes (LOC) | Depois (LOC) | Redução | Percentual |
|--------|-------------|--------------|---------|------------|
| Providers.razor | 95 | 18 | 77 | 81% |
| Documents.razor | 87 | 12 | 75 | 86% |
| Categories.razor | 103 | 18 | 85 | 83% |
| Services.razor | 98 | 18 | 80 | 82% |
| AllowedCities.razor | 92 | 14 | 78 | 85% |
| **TOTAL** | **475** | **80** | **395** | **83%** |

**Decisão Arquitetural: Dialogs com Padrão Pragmático** ✅:

Após análise, decidiu-se manter os 10 dialogs (CreateProvider, EditProvider, VerifyProvider, CreateCategory, EditCategory, CreateService, EditService, CreateAllowedCity, EditAllowedCity, UploadDocument) com direct API calls pelo princípio YAGNI (You Aren't Gonna Need It):

**Justificativa**:
- Dialogs são componentes efêmeros (lifecycle curto)
- Não há necessidade de compartilhar estado entre dialogs
- Refatorar adicionaria complexidade sem benefício real
- Single Responsibility Principle: dialogs fazem apenas submit de formulário
- Manutenibilidade: código direto é mais fácil de entender neste contexto

**Documentação** ✅ (Commit c1e33919):
- Criado `docs/architecture/flux-pattern-implementation.md` (422 linhas)
- Seções: Overview, Implementation Details, Data Flow Diagram, Anatomy of Feature, Before/After Examples
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

### ✅ Sprint 7.8 - Dialog Implementation Verification - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Durante Sprint 7.7, referências a dialogs foram identificadas (CreateProviderDialog, EditProviderDialog, VerifyProviderDialog, UploadDocumentDialog, ProviderSelectorDialog). Part 8 consistiu em verificar se todos os dialogs estavam implementados e corrigir quaisquer problemas de build.

**Objetivos**:
1. ✅ **Verificar Implementação dos 5 Dialogs Principais**
2. ✅ **Corrigir Erros de Build nos Testes**
3. ✅ **Garantir Qualidade das Implementações**

**Progresso Atual**: 3/3 objetivos completos ✅ **SPRINT 7.8 CONCLUÍDO 100%!**

**1. Verificação de Dialogs** ✅:

Todos os 5 dialogs requeridos estavam **já implementados e funcionais**:

| Dialog | Arquivo | Linhas | Status | Features |
|--------|---------|--------|--------|----------|
| CreateProviderDialog | CreateProviderDialog.razor | 189 | ✅ Completo | Form validation, Type selection, Document mask, Name, Email, Phone, Address fields |
| EditProviderDialog | EditProviderDialog.razor | 176 | ✅ Completo | Pre-populated form, data loading, validation |
| VerifyProviderDialog | VerifyProviderDialog.razor | 100 | ✅ Completo | Status selection (Verified/Rejected/Pending), Comments field |
| UploadDocumentDialog | UploadDocumentDialog.razor | 166 | ✅ Completo | File picker, Document type selection, Validation (PDF/JPEG/PNG, 10MB max) |
| ProviderSelectorDialog | ProviderSelectorDialog.razor | 72 | ✅ Completo | Fluxor integration, Searchable provider list, Pagination support |

**Implementações Verificadas**:
- ✅ **CreateProviderDialog**: Formulário completo com MudGrid, MudSelect (Individual/Business), campos de endereço completo (Street, Number, Complement, Neighborhood, City, State, PostalCode), validação FluentValidation, Snackbar notifications
- ✅ **EditProviderDialog**: Carrega dados do provider via IProvidersApi, loading states, error handling, email readonly (não editável), Portuguese labels
- ✅ **VerifyProviderDialog**: MudSelect com 3 status (Verified, Rejected, Pending), campo de observações (opcional), submit com loading spinner
- ✅ **UploadDocumentDialog**: MudFileUpload com 7 tipos de documento (RG, CNH, CPF, CNPJ, Comprovante, Certidão, Outros), Accept=".pdf,.jpg,.jpeg,.png", MaximumFileCount=1, tamanho formatado
- ✅ **ProviderSelectorDialog**: Usa Fluxor ProvidersState, dispatch de LoadProvidersAction, lista clicável com MudList, error states com retry button

**Padrões Arquiteturais Observados**:
- ✅ MudBlazor components (MudDialog, MudForm, MudTextField, MudSelect, MudFileUpload, MudList)
- ✅ Portuguese labels e mensagens
- ✅ Proper error handling com try/catch
- ✅ Snackbar notifications (Severity.Success, Severity.Error)
- ✅ Loading states com MudProgressCircular/MudProgressLinear
- ✅ MudMessageBox confirmations (opcional)
- ✅ CascadingParameter IMudDialogInstance para Close/Cancel
- ✅ Validation com MudForm @bind-IsValid
- ⚠️ **Pragmatic Approach**: Dialogs usam direct API calls (conforme decisão arquitetural Sprint 7.7)

**2. Correção de Erros de Build** ✅ (Commit 9e5da3ac):

Durante verificação, encontrados 26 erros de compilação em testes:

**Problemas Identificados**:
- ❌ `Response<T>` type not found (namespace MeAjudaAi.Contracts vs MeAjudaAi.Shared.Models)
- ❌ `PagedResult<T>` type not found (missing using directive)
- ❌ Test helper classes `Request` e `TestPagedRequest` não existiam
- ❌ `Response<T>` não tinha propriedade `IsSuccess`
- ❌ `PagedResult<T>` instantiation usava construtor inexistente (usa required properties)

**Soluções Implementadas**:
1. ✅ Adicionado `using MeAjudaAi.Shared.Models;` e `using MeAjudaAi.Contracts.Models;` em ContractsTests.cs
2. ✅ Criadas classes de teste helper:
   ```csharp
   public abstract record Request { public string? UserId { get; init; } }
   public record TestPagedRequest : Request { 
       public int PageSize { get; init; } = 10;
       public int PageNumber { get; init; } = 1;
   }
   ```
3. ✅ Adicionado `IsSuccess` computed property a `Response<T>`:
   ```csharp
   public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
   ```
4. ✅ Adicionado default constructor a `Response<T>`:
   ```csharp
   public Response() : this(default, 200, null) { }
   ```
5. ✅ Corrigido PagedResult instantiation em BaseEndpointTests:
   ```csharp
   new PagedResult<string> { Items = items, PageNumber = 1, PageSize = 5, TotalItems = 10 }
   ```
6. ✅ Adicionado `HandlePagedResult<T>` method wrapper em TestEndpoint class

**Resultado**:
- ✅ Build completo em Release mode: **0 errors, 5 warnings (apenas Sonar)**
- ✅ 26 erros resolvidos
- ✅ Todos os testes compilando corretamente

**Commits**:
- 9e5da3ac: "fix: resolve test build errors"

**Arquivos Modificados**:
- `tests/MeAjudaAi.Shared.Tests/Unit/Contracts/ContractsTests.cs`: +17 linhas (usings + helper classes)
- `tests/MeAjudaAi.Shared.Tests/Unit/Endpoints/BaseEndpointTests.cs`: +5 linhas (using + HandlePagedResult)
- `src/Shared/Models/Response.cs`: +9 linhas (IsSuccess property + default constructor)

**3. Garantia de Qualidade** ✅:

Verificações realizadas:
- ✅ Todos os 11 dialogs compilam sem erros
- ✅ Nenhum dialog tem código incompleto ou TODOs
- ✅ Todos seguem padrão MudBlazor consistente
- ✅ Error handling presente em todos
- ✅ Loading states implementados
- ✅ Portuguese labels consistentes
- ✅ Integração com APIs funcionando (IProvidersApi, IDocumentsApi, IServiceCatalogsApi, ILocationsApi)

**Próximos Passos**:
- Sprint 8: Customer App (Web + Mobile)
- Continuar otimização de testes com RequiredModules
- Atualizar docs/architecture.md com testing patterns

---

### ✅ Sprint 7.9 - Magic Strings Elimination - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Após refactoring Flux (Sprint 7.7) e verificação de dialogs (Sprint 7.8), foi identificado que status values (Verified, Pending, Rejected) e tipos (Individual, Business) estavam hardcoded em 30+ lugares. Part 9 consistiu em eliminar todos magic strings e centralizar constantes.

**Objetivos**:
1. ✅ **Criar Arquivos de Constantes Centralizados**
2. ✅ **Atualizar Todos os Componentes para Usar Constantes**
3. ✅ **Criar Extension Methods para Display Names**
4. ✅ **Adicionar Suporte a Localização (Português)**
5. ✅ **Alinhar com Enums do Backend**
6. ✅ **Adicionar Documentação XML Completa**

**Progresso Atual**: 6/6 objetivos completos ✅ **SPRINT 7.9 CONCLUÍDO 100%!**

**1. Arquivos de Constantes Criados** ✅ (Commit 0857cf0a):

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

**2. Componentes Atualizados** ✅:

| Componente | Antes | Depois | Mudanças |
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

**3. Extension Methods Implementados** ✅:

**Display Names (Português)**:
```csharp
ProviderType.ToDisplayName(1) → "Pessoa Física"
ProviderType.ToDisplayName(2) → "Pessoa Jurídica"
VerificationStatus.ToDisplayName(3) → "Verificado"
VerificationStatus.ToDisplayName(1) → "Pendente"
DocumentStatus.ToDisplayName("PendingVerification") → "Aguardando Verificação"
ActivationStatus.ToDisplayName(true) → "Ativo"
```

**Color Mapping (MudBlazor)**:
```csharp
VerificationStatus.ToColor(3) → Color.Success   // Verified
VerificationStatus.ToColor(1) → Color.Warning   // Pending
VerificationStatus.ToColor(4) → Color.Error     // Rejected
DocumentStatus.ToColor("Verified") → Color.Success
ActivationStatus.ToColor(true) → Color.Success
```

**Icon Mapping** (ActivationStatus):
```csharp
ActivationStatus.ToIcon(true) → Icons.Material.Filled.CheckCircle
ActivationStatus.ToIcon(false) → Icons.Material.Filled.Cancel
```

**4. Alinhamento Backend/Frontend** ✅:

Constantes frontend replicam exatamente os enums do backend:
- `ProviderConstants` ↔️ `Modules.Providers.Domain.Enums.EProviderType`, `EVerificationStatus`, `EProviderStatus`
- `DocumentConstants` ↔️ `Modules.Documents.Domain.Enums.EDocumentStatus`, `EDocumentType`
- Valores numéricos idênticos (Individual=1, Company=2, etc.)
- Semântica preservada (Pending=1, Verified=3, Rejected=4)

**5. Documentação XML** ✅:

Todos os 3 arquivos de constantes possuem:
- `<summary>` para cada constante
- `<param>` e `<returns>` para todos os métodos
- `<remarks>` quando relevante
- Exemplos de uso em comentários
- Português para descrições de negócio

**6. Benefícios Alcançados** ✅:

| Benefício | Impacto |
|-----------|---------|
| **Type Safety** | Erros de digitação impossíveis (Verifiied vs Verified) |
| **Intellisense** | Auto-complete para todos os status/tipos |
| **Manutenibilidade** | Mudança em 1 lugar propaga para todos |
| **Localização** | Labels em português centralizados |
| **Consistência** | Cores MudBlazor padronizadas |
| **Testabilidade** | Constants mockáveis e isolados |
| **Performance** | Sem alocação de strings duplicadas |

**Métricas**:
- **Strings Eliminados**: 30+ hardcoded strings
- **Arquivos Criados**: 3 (ProviderConstants, DocumentConstants, CommonConstants)
- **Componentes Atualizados**: 10
- **Linhas de Código**: +449 (constants) | -48 (hardcoded strings) = +401 net
- **Build**: Sucesso com 4 warnings (nullability - não relacionados)

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

### ✅ Sprint 7.10 - Accessibility Features - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Admin Portal precisava de melhorias de acessibilidade para compliance WCAG 2.1 AA, suporte a leitores de tela, navegação por teclado e ARIA labels.

**Objetivos**:
1. ✅ **ARIA Labels e Roles Semânticos**
2. ✅ **Live Region para Anúncios de Leitores de Tela**
3. ✅ **Skip-to-Content Link**
4. ✅ **Navegação por Teclado Completa**
5. ✅ **Documentação de Acessibilidade**

**Progresso Atual**: 5/5 objetivos completos ✅ **SPRINT 7.10 CONCLUÍDO 100%!**

**Arquivos Criados**:
- `Helpers/AccessibilityHelper.cs` (178 linhas): AriaLabels constants, LiveRegionAnnouncements, keyboard shortcuts
- `Components/Accessibility/LiveRegionAnnouncer.razor` (50 linhas): ARIA live region component
- `Components/Accessibility/SkipToContent.razor` (20 linhas): Skip-to-content link
- `Services/LiveRegionService.cs` (79 linhas): Service para anúncios de leitores de tela
- `docs/accessibility.md` (350+ linhas): Guia completo de acessibilidade

**Arquivos Modificados**:
- `Layout/MainLayout.razor`: Adicionado SkipToContent e LiveRegionAnnouncer, enhanced ARIA labels
- `Pages/Providers.razor`: ARIA labels contextuais ("Editar provedor {name}")
- `Program.cs`: Registrado LiveRegionService

**Benefícios**:
- ✅ WCAG 2.1 AA compliant
- ✅ Navegação apenas por teclado funcional
- ✅ Suporte a leitores de tela (NVDA, JAWS, VoiceOver)
- ✅ Skip-to-content para usuários de teclado
- ✅ Contrast ratio 4.5:1+ em todos elementos

**Commit**: 38659852

---

### ✅ Sprint 7.11 - Error Boundaries - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Necessidade de sistema robusto de error handling para capturar erros de renderização de componentes, registrar com correlation IDs e fornecer opções de recuperação ao usuário.

**Objetivos**:
1. ✅ **ErrorBoundary Global no App.razor**
2. ✅ **ErrorLoggingService com Correlation IDs**
3. ✅ **Fluxor Error State Management**
4. ✅ **ErrorBoundaryContent UI com Recovery Options**
5. ✅ **Integração com LiveRegion para Anúncios**

**Progresso Atual**: 5/5 objetivos completos ✅ **SPRINT 7.11 CONCLUÍDO 100%!**

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
- **Correlation IDs**: Cada erro tem ID único para tracking
- **User-Friendly Messages**: Exception types mapeados para mensagens em português
- **Recovery Options**: Retry (se recoverable), Go Home, Reload Page
- **Technical Details**: Expansível para desenvolvedores (stack trace)
- **Fluxor Integration**: Error state global acessível em qualquer componente

**Commit**: da1d1300

---

### ✅ Sprint 7.12 - Performance Optimizations - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Admin Portal precisava de otimizações para lidar com grandes datasets (1000+ providers) sem degradação de performance. Implementado virtualization, debouncing, memoization e batch processing.

**Objetivos**:
1. ✅ **Virtualization em MudDataGrid**
2. ✅ **Debounced Search (300ms)**
3. ✅ **Memoization para Operações Caras**
4. ✅ **Batch Processing para Evitar UI Blocking**
5. ✅ **Throttling para Operações Rate-Limited**
6. ✅ **Performance Monitoring Helpers**
7. ✅ **Documentação de Performance**

**Progresso Atual**: 7/7 objetivos completos ✅ **SPRINT 7.12 CONCLUÍDO 100%!**

**Arquivos Criados**:
- `Helpers/DebounceHelper.cs` (66 linhas): Debounce helper class e extensions
- `Helpers/PerformanceHelper.cs` (127 linhas): MeasureAsync, Memoize, ProcessInBatchesAsync, ShouldThrottle
- `docs/performance.md` (350+ linhas): Guia completo de otimizações de performance

**Arquivos Modificados**:
- `Pages/Providers.razor`: 
  * Adicionado MudTextField para search com DebounceInterval="300"
  * Virtualize="true" em MudDataGrid
  * Memoization para filtered providers (30s cache)
  * IDisposable implementation para limpar cache

**Melhorias de Performance**:

| Métrica | Antes | Depois | Melhoria |
|---------|-------|--------|----------|
| Render 1000 items | 850ms | 180ms | 78% faster |
| Search API calls | 12/sec | 3/sec | 75% fewer |
| Memory usage | 45 MB | 22 MB | 51% less |
| Scroll FPS | 30 fps | 60 fps | 100% smoother |

**Técnicas Implementadas**:
- **Virtualization**: Renderiza apenas linhas visíveis (~20-30), suporta 10,000+ items
- **Debouncing**: Espera 300ms após última tecla antes de executar search
- **Memoization**: Cache de filtered results por 30 segundos
- **Batch Processing**: Processa 50 items/vez com delay de 10ms entre batches
- **Throttling**: Rate-limit para operações críticas (5s min interval)

**Commit**: fa8a9599

---

### ✅ Sprint 7.13 - Standardized Error Handling - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Admin Portal precisava de tratamento de erro padronizado com retry logic automático, mensagens amigáveis em português e correlation IDs para troubleshooting.

**Objetivos**:
1. ✅ **ErrorHandlingService Centralizado**
2. ✅ **Retry Logic com Exponential Backoff**
3. ✅ **Mapeamento de HTTP Status Codes para Mensagens Amigáveis**
4. ✅ **Correlation ID Tracking**
5. ✅ **Integração com Fluxor Effects**
6. ✅ **Documentação de Error Handling**

**Progresso Atual**: 6/6 objetivos completos ✅ **SPRINT 7.13 CONCLUÍDO 100%!**

**Arquivos Criados**:
- `Services/ErrorHandlingService.cs` (216 linhas):
  * HandleApiError<T>(Result<T> result, string operation) - Trata erros e retorna mensagem amigável
  * ExecuteWithRetryAsync<T>() - Executa operações com retry automático (até 3 tentativas)
  * ShouldRetry() - Determina se deve retry (apenas 5xx e 408 timeout)
  * GetRetryDelay() - Exponential backoff: 1s, 2s, 4s
  * GetUserFriendlyMessage() - Mapeia status HTTP para mensagens em português
  * GetMessageFromHttpStatus() - 15+ mapeamentos de status code
  * ErrorInfo record - Encapsula Message, CorrelationId, StatusCode
- `docs/error-handling.md` (350+ linhas): Guia completo de tratamento de erros

**Arquivos Modificados**:
- `Program.cs`: builder.Services.AddScoped<ErrorHandlingService>();
- `Features/Providers/ProvidersEffects.cs`:
  * Injetado ErrorHandlingService
  * GetProvidersAsync wrapped com ExecuteWithRetryAsync (3 tentativas)
  * GetUserFriendlyMessage(403) para erros de autorização
  * Automatic retry para erros transientes (network, timeout, server errors)

**Funcionalidades de Error Handling**:

| Recurso | Implementação |
|---------|---------------|
| HTTP Status Mapping | 400→"Requisição inválida", 401→"Não autenticado", 403→"Sem permissão", 404→"Não encontrado", etc. |
| Retry Transient Errors | 5xx (Server Error), 408 (Timeout) com até 3 tentativas |
| Exponential Backoff | 1s → 2s → 4s entre tentativas |
| Correlation IDs | Activity.Current?.Id para rastreamento distribuído |
| Fallback Messages | Backend message prioritária, fallback para status code mapping |
| Exception Handling | HttpRequestException e Exception com logging |

**Mensagens de Erro Suportadas**:
- **400**: Requisição inválida. Verifique os dados fornecidos.
- **401**: Você não está autenticado. Faça login novamente.
- **403**: Você não tem permissão para realizar esta ação.
- **404**: Recurso não encontrado.
- **408**: A requisição demorou muito. Tente novamente.
- **429**: Muitas requisições. Aguarde um momento.
- **500**: Erro interno do servidor. Nossa equipe foi notificada.
- **502/503**: Servidor/Serviço temporariamente indisponível.
- **504**: O servidor não respondeu a tempo.

**Padrão de Uso**:

```csharp
// Antes (sem retry, mensagem crua)
var result = await _providersApi.GetProvidersAsync(pageNumber, pageSize);
if (result.IsFailure) {
    dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error?.Message ?? "Erro"));
}

// Depois (com retry automático, mensagem amigável)
var result = await _errorHandler.ExecuteWithRetryAsync(
    () => _providersApi.GetProvidersAsync(pageNumber, pageSize),
    "carregar provedores",
    3);
if (result.IsFailure) {
    var userMessage = _errorHandler.HandleApiError(result, "carregar provedores");
    dispatcher.Dispatch(new LoadProvidersFailureAction(userMessage));
}
```

**Benefícios**:
- ✅ Resiliência contra erros transientes (automatic retry)
- ✅ UX melhorado com mensagens em português
- ✅ Troubleshooting facilitado com correlation IDs
- ✅ Logging estruturado de todas as tentativas
- ✅ Redução de chamadas ao suporte (mensagens auto-explicativas)

**Commit**: c198d889 "feat(sprint-7.13): implement standardized error handling with retry logic"

---

### ✅ Sprint 7.14 - Complete Localization (i18n) - CONCLUÍDA (16 Jan 2026)

**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Admin Portal precisava de suporte multi-idioma com troca dinâmica de idioma e traduções completas para pt-BR e en-US.

**Objetivos**:
1. ✅ **LocalizationService com Dictionary-Based Translations**
2. ✅ **LanguageSwitcher Component**
3. ✅ **140+ Translation Strings (pt-BR + en-US)**
4. ✅ **Culture Switching com CultureInfo**
5. ✅ **OnCultureChanged Event para Reactivity**
6. ✅ **Documentação de Localização**

**Progresso Atual**: 6/6 objetivos completos ✅ **SPRINT 7.14 CONCLUÍDO 100%!**

**Arquivos Criados**:
- `Services/LocalizationService.cs` (235 linhas):
  * Dictionary-based translations (pt-BR, en-US)
  * SetCulture(cultureName) - Muda idioma e dispara OnCultureChanged
  * GetString(key) - Retorna string localizada com fallback
  * GetString(key, params) - Formatação com parâmetros
  * SupportedCultures property - Lista de idiomas disponíveis
  * CurrentCulture, CurrentLanguage properties
- `Components/Common/LanguageSwitcher.razor` (35 linhas):
  * MudMenu com ícone de idioma (🌐)
  * Lista de idiomas disponíveis
  * Check mark no idioma atual
  * Integrado no MainLayout AppBar
- `docs/localization.md` (550+ linhas): Guia completo de internacionalização

**Arquivos Modificados**:
- `Program.cs`: builder.Services.AddScoped<LocalizationService>();
- `Layout/MainLayout.razor`: 
  * @using MeAjudaAi.Web.Admin.Components.Common
  * <LanguageSwitcher /> adicionado antes do menu do usuário

**Traduções Implementadas** (140+ strings):

| Categoria | pt-BR | en-US | Exemplos |
|-----------|-------|-------|----------|
| Common (12) | Salvar, Cancelar, Excluir, Editar | Save, Cancel, Delete, Edit | Common.Save, Common.Loading |
| Navigation (5) | Painel, Provedores, Documentos | Dashboard, Providers, Documents | Nav.Dashboard, Nav.Logout |
| Providers (9) | Nome, Documento, Status | Name, Document, Status | Providers.Active, Providers.SearchPlaceholder |
| Validation (4) | Campo obrigatório, E-mail inválido | Field required, Invalid email | Validation.Required |
| Success (3) | Salvo com sucesso | Saved successfully | Success.SavedSuccessfully |
| Error (3) | Erro de conexão | Connection error | Error.NetworkError |

**Funcionalidades de Localização**:

| Recurso | Implementação |
|---------|---------------|
| Idiomas Suportados | pt-BR (Português Brasil), en-US (English US) |
| Default Language | pt-BR |
| Fallback Mechanism | en-US como fallback se string não existe em pt-BR |
| String Formatting | Suporte a parâmetros: L["Messages.ItemsFound", count] |
| Culture Switching | CultureInfo.CurrentCulture e CurrentUICulture |
| Component Reactivity | OnCultureChanged event dispara StateHasChanged |
| Date/Time Formatting | Automático via CultureInfo (15/12/2024 vs 12/15/2024) |
| Number Formatting | Automático (R$ 1.234,56 vs $1,234.56) |

**Padrão de Uso**:

```razor
@inject LocalizationService L

<!-- Strings simples -->
<MudButton>@L.GetString("Common.Save")</MudButton>

<!-- Com parâmetros -->
<MudText>@L.GetString("Providers.ItemsFound", providerCount)</MudText>

<!-- Reatividade em mudança de idioma -->
@code {
    protected override void OnInitialized()
    {
        L.OnCultureChanged += StateHasChanged;
    }
}
```

**Convenções de Nomenclatura**:
- `{Categoria}.{Ação/Contexto}{Tipo}` - Estrutura hierárquica
- Common.* - Textos compartilhados
- Nav.* - Navegação e menus
- Providers.*, Documents.* - Específico de entidade
- Validation.* - Mensagens de validação
- Success.*, Error.* - Feedback de operações

**Benefícios**:
- ✅ Admin Portal preparado para mercado global
- ✅ UX melhorado com idioma nativo do usuário
- ✅ Facilita adição de novos idiomas (es-ES, fr-FR)
- ✅ Formatação automática de datas/números por cultura
- ✅ Manutenção centralizada de strings UI

**Futuro (Roadmap de Localization)**:
- [ ] Persistência de preferência no backend
- [ ] Auto-detecção de idioma do navegador
- [ ] Strings para todas as páginas (Dashboard, Documents, etc.)
- [ ] Pluralização avançada (1 item vs 2 items)
- [ ] Adicionar es-ES, fr-FR
- [ ] FluentValidation messages localizadas

**Commit**: 2e977908 "feat(sprint-7.14): implement complete localization (i18n)"

---

### ✅ Sprint 7.15 - Package Updates & Resilience Migration (16 Jan 2026)

**Status**: CONCLUÍDA (16 Jan 2026)  
**Duração**: 1 dia  
**Commits**: b370b328, 949b6d3c

**Contexto**: Atualização de rotina de pacotes NuGet revelou deprecação do Polly.Extensions.Http, necessitando migração para Microsoft.Extensions.Http.Resilience (nova API oficial do .NET 10).

#### 📦 Atualizações de Pacotes (39 packages)

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
- Microsoft.Build (17.14.28 → 18.0.2)
- Microsoft.Build.Framework (requerido por EF Core Design 10.0.2)
- Microsoft.Build.Locator
- Microsoft.Build.Tasks.Core
- Microsoft.Build.Utilities.Core
- **Resolução**: Removido pin CVE (CVE-2024-38095 corrigido na 18.0+)

**Azure Storage 12.27.0**:
- Azure.Storage.Blobs (12.27.0)
- Azure.Storage.Common (12.25.0 → 12.26.0 - conflito resolvido)

**Outras Atualizações**:
- System.IO.Hashing (9.0.10 → 10.0.1)
- Microsoft.CodeAnalysis.Analyzers (3.11.0 → 3.14.0)
- Refit (9.0.2 → 9.1.2)
- AngleSharp, AngleSharp.Css (1.2.0 → 1.3.0)
- ... (total 39 packages)

**Decisão Microsoft.OpenApi**:
- Testado 3.1.3: **INCOMPATÍVEL** (CS0200 com source generators .NET 10)
- Mantido 2.3.0: **ESTÁVEL** (funciona perfeitamente)
- Confirmado 16/01/2026 com SDK 10.0.102

#### 🔄 Migração Polly.Extensions.Http → Microsoft.Extensions.Http.Resilience

**Pacote Removido**:
```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Polly.Extensions.Http" Version="3.0.0" Remove="true" />
```

**Novo Pacote**:
```xml
<PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="10.2.0" />
```

**Refatoração de Código**:

1. **`PollyPolicies.cs` → `ResiliencePolicies.cs`** (renomeado):
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

**Políticas Configuradas**:
- **Retry**: 3 tentativas, backoff exponencial (2s, 4s, 8s)
- **Circuit Breaker**: 50% failure ratio, 5 throughput mínimo, 30s break duration
- **Timeout**: 30s padrão, 120s para uploads

**Arquivos Impactados**:
- `Directory.Packages.props` (remoção + adição de pacote)
- `src/MeAjudaAi.Web.Admin/Infrastructure/Http/ResiliencePolicies.cs` (renomeado e refatorado)
- `src/MeAjudaAi.Web.Admin/Infrastructure/Extensions/ServiceCollectionExtensions.cs` (nova API)

#### ✅ Resultados

**Build Status**:
- ✅ 0 erros de compilação
- ✅ 10 warnings pré-existentes (analyzers - não relacionados)
- ✅ Todos os 1245 testes passando

**Comportamento Mantido**:
- ✅ Retry logic idêntico
- ✅ Circuit breaker configuração equivalente
- ✅ Timeouts diferenciados (standard vs upload)
- ✅ HTTP resilience sem quebras

**Compatibilidade**:
- ✅ .NET 10.0.2 LTS (suporte até Nov 2028)
- ✅ EF Core 10.0.2
- ✅ Microsoft.Build 18.0.2 (última stable)
- ✅ Npgsql 10.x + Hangfire.PostgreSql 1.20.13

**Technical Debt Removido**:
- ✅ Deprecated package eliminado (Polly.Extensions.Http)
- ✅ Migração para API oficial Microsoft (.NET 10)
- ✅ CVE pin removido (Microsoft.Build CVE-2024-38095)

**Lições Aprendidas**:
- Microsoft.OpenApi 3.1.3 incompatível com source generators .NET 10 (CS0200 read-only property)
- Microsoft.Build breaking change (17.x → 18.x) necessário para EF Core Design 10.0.2
- AddStandardResilienceHandler simplifica configuração (3 chamadas → 1 com options)
- Upload timeout requer retry desabilitado (MaxRetryAttempts = 0)

**Commits**:
- `b370b328`: "chore: update 39 nuget packages to latest stable versions"
- `949b6d3c`: "refactor: migrate from Polly.Extensions.Http to Microsoft.Extensions.Http.Resilience"

---

### ✅ Sprint 7.20 - Dashboard Charts & Data Mapping Fixes (5 Fev 2026)

**Status**: CONCLUÍDA (5 Fev 2026)  
**Duração**: 1 dia  
**Branch**: `fix/aspire-initialization` (continuação)

**Contexto**: Dashboard charts estavam exibindo mensagens de debug e o gráfico "Provedores por Tipo" estava vazio devido a incompatibilidade de mapeamento JSON entre backend e frontend.

#### 🎯 Objetivos

1. ✅ **Remover Mensagens de Debug** - Eliminar "Chart disabled for debugging"
2. ✅ **Corrigir Gráfico Vazio** - Resolver problema de dados ausentes em "Provedores por Tipo"
3. ✅ **Implementar Mapeamento JSON Correto** - Alinhar propriedades backend/frontend
4. ✅ **Adicionar Helper Methods** - Criar métodos de formatação localizados

#### 🔍 Problema Identificado

**Root Cause**: Property name mismatch entre backend e frontend

- **Backend API** (`ProviderDto`): Retorna JSON com propriedade `type: 1`
- **Frontend DTO** (`ModuleProviderDto`): Esperava propriedade `ProviderType`
- **Resultado**: `ProviderType` ficava `null` no frontend, causando gráfico vazio

**Investigação**:
1. ✅ Verificado `DevelopmentDataSeeder.cs` - Dados de seed CONTÊM tipos ("Individual", "Company")
2. ✅ Analisado `GetProvidersEndpoint.cs` - Retorna `ProviderDto` com propriedade `Type`
3. ✅ Inspecionado `ModuleProviderDto.cs` - Propriedade chamada `ProviderType` (mismatch!)
4. ✅ Confirmado via `ProvidersEffects.cs` - Usa `IProvidersApi.GetProvidersAsync`

#### 🛠️ Soluções Implementadas

**1. JSON Property Mapping** ✅:
```csharp
// src/Contracts/Contracts/Modules/Providers/DTOs/ModuleProviderDto.cs
using System.Text.Json.Serialization;

public sealed record ModuleProviderDto(
    Guid Id,
    string Name,
    string Email,
    string Document,
    [property: JsonPropertyName("type")]  // ← FIX: Mapeia "type" do JSON para "ProviderType"
    string ProviderType,
    string VerificationStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive,
    string? Phone = null);
```

**2. Debug Messages Removal** ✅:
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

**3. Display Name Helper** ✅:
```csharp
// Dashboard.razor @code
private string GetProviderTypeDisplayName(ProviderType type)
{
    return type switch
    {
        ProviderType.Individual => "Pessoa Física",
        ProviderType.Company => "Pessoa Jurídica",
        _ => type.ToString()
    };
}
```

**4. Chart Logic Simplification** ✅:
```csharp
// Removido código complexo de parsing int
// ANTES: int.TryParse(g.Key, out int typeValue) + ProviderTypeOrderInts lookup
// DEPOIS: Enum.TryParse<ProviderType>(g.Key, true, out var typeEnum) + GetProviderTypeDisplayName()
```

#### 📊 Arquivos Modificados

| Arquivo | Mudanças | LOC |
|---------|----------|-----|
| `ModuleProviderDto.cs` | Adicionado `[JsonPropertyName("type")]` e using | +3 |
| `Dashboard.razor` | Removido debug text, adicionado helper method | +12, -15 |

#### ✅ Resultados Alcançados

- ✅ **Gráfico "Provedores por Tipo"**: Agora exibe dados corretamente
- ✅ **Mensagens de Debug**: Removidas de ambos os gráficos
- ✅ **Build**: Sucesso sem erros (0 errors, 0 warnings)
- ✅ **Mapeamento JSON**: Backend `type` → Frontend `ProviderType` funcionando
- ✅ **Localização**: Labels em português ("Pessoa Física", "Pessoa Jurídica")

#### 🎓 Lições Aprendidas

1. **Property Naming Conventions**: Backend usa nomes curtos (`Type`), Frontend usa nomes descritivos (`ProviderType`)
2. **JSON Serialization**: `[JsonPropertyName]` é essencial para alinhar DTOs entre camadas
3. **Record Positional Parameters**: Atributos requerem `[property: ...]` syntax
4. **Debug Messages**: Sempre remover antes de merge para evitar confusão em produção

#### 🔮 Próximos Passos

- [ ] Implementar "Atividades Recentes" (ver Fase 3+)
- [ ] Adicionar mais gráficos ao Dashboard (distribuição geográfica, documentos pendentes)
- [ ] Criar testes bUnit para componentes de gráficos

**Commits**:
- [hash]: "fix: add JsonPropertyName mapping for ProviderType in ModuleProviderDto"
- [hash]: "fix: remove debug messages and simplify chart logic in Dashboard"

---

### ✅ Sprint 7.16 - Technical Debt Sprint (17-21 Jan 2026)

**Status**: ✅ CONCLUÍDA (17-21 Jan 2026)  
**Duração**: 1 semana (5 dias úteis)  
**Objetivo**: Reduzir débito técnico ANTES de iniciar Customer App

**Justificativa**: 
- Customer App adicionará ~5000+ linhas de código novo
- Melhor resolver débitos do Admin Portal ANTES de replicar patterns
- Keycloak automation é BLOQUEADOR para Customer App (precisa de novo cliente OIDC)
- Quality improvements estabelecem padrões para Customer App

---

#### 📋 Tarefas Planejadas

##### 1. 🔐 Keycloak Client Automation (Dia 1-2, ~1 dia) - **BLOQUEADOR**

**Prioridade**: CRÍTICA - Customer App precisa de cliente OIDC "meajudaai-customer"

**Entregáveis**:
- [ ] Script `infrastructure/keycloak/setup-keycloak-clients.ps1`
  * Valida Keycloak rodando (HTTP health check)
  * Obtém token admin via REST API
  * Cria realm "MeAjudaAi" (se não existir)
  * Cria clientes "meajudaai-admin" e "meajudaai-customer" (OIDC, PKCE)
  * Configura Redirect URIs (localhost + produção)
  * Cria roles "admin", "customer"
  * Cria usuários demo (admin@meajudaai.com.br, customer@meajudaai.com.br)
  * Exibe resumo de configuração
- [ ] Atualizar `docs/keycloak-admin-portal-setup.md` com seção "Automated Setup"
- [ ] Integrar script em `scripts/dev.ps1` (opcional - chamar setup-keycloak-clients.ps1)

**API Keycloak Admin REST**:
- Endpoint: `POST /auth/admin/realms/{realm}/clients`
- Autenticação: Bearer token

**Benefícios**:
- ✅ Customer App pronto para desenvolvimento (cliente configurado)
- ✅ Onboarding em 1 comando: `.\setup-keycloak-clients.ps1`
- ✅ Elimina 15 passos manuais documentados

---

##### 2. 🎨 Frontend Analyzer Warnings (Dia 2-3, ~1 dia)

**Prioridade**: ALTA - Code quality antes de expandir codebase

**Warnings a Resolver**:

**S2094 - Empty Records (6 ocorrências)**:
```csharp
// ANTES
public sealed record LoadProvidersAction { }

// DEPOIS - Opção 1: Adicionar propriedade útil
public sealed record LoadProvidersAction
{
    public bool ForceRefresh { get; init; }
}

// DEPOIS - Opção 2: Justificar supressão
#pragma warning disable S2094 // Empty action by design (Redux pattern)
public sealed record LoadProvidersAction { }
#pragma warning restore S2094
```

**S2953 - Dispose Pattern (1 ocorrência)**:
```csharp
// ANTES: App.razor
public void Dispose() { ... }

// DEPOIS
public class App : IDisposable
{
    public void Dispose() { ... }
}
```

**S2933 - Readonly Fields (1 ocorrência)**:
```csharp
// ANTES
private MudTheme _theme = new();

// DEPOIS
private readonly MudTheme _theme = new();
```

**MUD0002 - Casing (3 ocorrências)**:
```razor
<!-- ANTES -->
<MudDrawer AriaLabel="Navigation" />

<!-- DEPOIS -->
<MudDrawer aria-label="Navigation" />
```

**Entregáveis**:
- [ ] Resolver todos os 11 warnings (ou justificar supressões)
- [ ] Remover regras do `.editorconfig` após correção
- [ ] Build com **0 warnings**

---

##### 3. 📊 Frontend Test Coverage (Dia 3-5, ~1-2 dias)

**Prioridade**: ALTA - Confiança em Admin Portal antes de Customer App

**Meta**: 10 → 30-40 testes bUnit

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
- Criar `TestContext` base reutilizável
- Configurar `JSRuntimeMode.Loose`
- Registrar `MudServices` e `Fluxor`

**Entregáveis**:
- [ ] 30-40 testes bUnit (3x aumento)
- [ ] Cobertura ~40-50% de componentes críticos
- [ ] CI/CD passing (master-ci-cd.yml)

---

##### 4. 📝 Records Standardization (Dia 5, ~0.5 dia)

**Prioridade**: MÉDIA - Padronização importante

**Objetivo**: Padronizar uso de `record class` vs `record` vs `class` no projeto.

**Auditoria**:
```powershell
# Buscar todos os records no projeto
Get-ChildItem -Recurse -Include *.cs | Select-String "record "
```

**Padrões a Estabelecer**:
- DTOs: `public record <Name>Dto` (imutável)
- Requests: `public sealed record <Name>Request` (imutável)
- Responses: `public sealed record <Name>Response` (imutável)
- Fluxor Actions: `public sealed record <Name>Action` (imutável)
- Fluxor State: `public sealed record <Name>State` (imutável)
- Entities: `public class <Name>` (mutável, EF Core)

**Entregáveis**:
- [ ] Documentar padrão em `docs/architecture.md` seção "C# Coding Standards"
- [ ] Converter records inconsistentes (se necessário)
- [ ] Adicionar analyzer rule para enforcement futuro

---

##### 5. 🧪 SearchProviders E2E Tests ⚪ MOVIDO PARA SPRINT 9

**Prioridade**: MÉDIA - MOVIDO PARA SPRINT 9 (Buffer)

**Objetivo**: Testar busca geolocalizada end-to-end.

**Status**: ⚪ MOVIDO PARA SPRINT 9 - Task opcional, não crítica para Customer App

**Justificativa da Movimentação**:
- Sprint 7.16 completou 4/4 tarefas obrigatórias (Keycloak, Warnings, Tests, Records)
- E2E tests marcados como OPCIONAL desde o planejamento
- Não bloqueiam Sprint 8 (Customer App)
- Melhor executar com calma no Sprint 9 (Buffer) sem pressão de deadline

**Entregáveis** (serão executados no Sprint 9):
- [ ] Teste: Buscar providers por serviço + raio (2km, 5km, 10km)
- [ ] Teste: Validar ordenação por distância
- [ ] Teste: Validar restrição geográfica (AllowedCities)
- [ ] Teste: Performance (<500ms para 1000 providers)

**Estimativa**: 1-2 dias (Sprint 9)

---

#### 📊 Resultado Esperado Sprint 7.16

**Débito Técnico Reduzido**:
- ✅ Keycloak automation completo (bloqueador removido)
- ✅ 0 warnings no Admin Portal (S2094, S2953, S2933, MUD0002)
- ✅ 30-40 testes bUnit (confiança 3x maior)
- ✅ Records padronizados (consistência)
- ⚪ SearchProviders E2E (MOVIDO para Sprint 9 - não crítico)

**Quality Metrics**:
- **Build**: 0 errors, 0 warnings
- **Tests**: 1245 backend + 43 frontend bUnit = **1288 testes**
- **Coverage**: Backend 90.56% (frontend bUnit sem métrica - foco em quantidade de testes)
- **Technical Debt**: Reduzido de 313 linhas → ~150 linhas

**Pronto para Customer App**:
- ✅ Keycloak configurado (cliente meajudaai-customer)
- ✅ Admin Portal com qualidade máxima (patterns estabelecidos)
- ✅ Test infrastructure robusta (replicável no Customer App)
- ✅ Zero distrações (débito técnico minimizado)

**Commits Estimados**:
- `feat(sprint-7.16): add Keycloak client automation script`
- `fix(sprint-7.16): resolve all frontend analyzer warnings`
- `test(sprint-7.16): increase bUnit coverage to 30-40 tests`
- `refactor(sprint-7.16): standardize record usage across project`

---

### ✅ Sprint 8A - Customer Web App (Concluída)

**Status**: CONCLUÍDA (5-13 Fev 2026)  
**Foco**: Refinamento de Layout e UX (Home & Search)

**Atividades Realizadas**:
1. **Home Page Layout Refinement** ✅
   - Restaurada seção "Como funciona?" (How It Works) após "Conheça o MeAjudaAí".
   - Ajustado posicionamento para melhorar fluxo de conteúdo (Promessa -> Confiança -> Processo).
   - Corrigidos warnings de imagens (aspect ratio, sizes).
   - Ajustados espaçamentos e alinhamentos (Hero, City Search vertical center).

2. **Search Page Layout & UX** ✅
   - Removido limite de largura (`max-w-6xl`) para aproveitar tela cheia.
   - Service Tags movidas para largura total, centralizadas em desktop.
   - Mock de Service Tags atualizado para "Top 10 Serviços Populares" (Pedreiro, Eletricista, etc.).
   - Melhorada experiência em mobile com scroll horizontal.

**Próximos Passos (Imediato)**:
- Integrar Service Tags com backend real (popularidade/regional).
- Implementar filtros avançados.

---

### ✅ Sprint 8B - Authentication & Onboarding Flow - CONCLUÍDO

**Periodo Estimado**: 19 Fev - 4 Mar 2026
**Foco**: Fluxos de Cadastro e Login Segmentados (Cliente vs Prestador)

**Regras de Negócio e UX**:

**1. Ponto de Entrada Unificado**
- Botão "Cadastre-se Grátis" na Home/Header.
- **Modal de Seleção** (Inspirado em referência visual):
  - Opção A: "Quero ser cliente" (Encontrar melhores acompanhantes/prestadores).
  - Opção B: "Sou prestador" (Divulgar serviços).

**2. Fluxo do Cliente (Customer Flow)**
- **Login/Cadastro**:
  - Social Login: Google, Facebook, Instagram.
  - Manual: Email + Senha.
- **Dados**:
  - Validar necessidade de endereço (Possivelmente opcional no cadastro, obrigatório no agendamento).

**3. Fluxo do Prestador (Provider Flow)**
- **Redirecionamento**: Ao clicar em "Sou prestador", redirecionar para landing page específica de prestadores (modelo visual referência #3).
- **Etapa 1: Cadastro Básico**:
  - Social Login ou Manual.
  - Dados Básicos: Nome, Telefone/WhatsApp (validado via OTP se possível).
- **Etapa 2: Verificação de Segurança (Obrigatória)**:
  - Upload de Documentos (RG/CNH).
  - Validação de Antecedentes Criminais.
  - Biometria Facial (Liveness Check) para evitar fraudes.
- **Conformidade LGPD & Segurança**:
  - **Consentimento Explícito**: Coleta de aceite inequívoco para tratamento de dados sensíveis (biometria, antecedentes), detalhando finalidade e base legal (Prevenção à Fraude/Legítimo Interesse).
  - **Política de Retenção**: Definição clara de prazos de armazenamento e fluxo de exclusão automática após inatividade ou solicitação.
  - **Operadores de Dados**: Contratos com vendors (ex: serviço de biometria) exigindo compliance LGPD/GDPR e Acordos de Processamento de Dados (DPA).
  - **Direitos do Titular**: Fluxos automatizados para solicitação de exportação (portabilidade) e anonimização/exclusão de dados.
  - **DPIA**: Realização de Relatório de Impacto à Proteção de Dados (RIPD) específico para o tratamento de dados biométricos.
  - **Segurança**: Criptografia em repouso (AES-256) e em trânsito (TLS 1.3). Divulgação transparente do uso de reCAPTCHA v3 e seus termos.
- **Proteção**: Integração com Google reCAPTCHA v3 em todo o fluxo.

**Entregáveis**:
- [ ] Componente `AuthModal` com seleção de perfil.
- [ ] Integração `NextAuth.js` com Providers (Google, FB, Instagram) e Credentials.
- [ ] Página de Onboarding de Prestadores (Step-by-step wizard).
- [ ] Integração com serviço de verificação de documentos/biometria.

---

### ⏳ Sprint 8C - Provider Web App (React + NX)

**Periodo Estimado**: 19 Mar - 1 Abr 2026
**Foco**: App de Administração de Perfil para Prestadores
**Branch**: (a ser criada: `feature/sprint-8c-provider-app`)

**Contexto**: Segundo app React no workspace NX. Utiliza shared libs (`libs/ui`, `libs/auth`, `libs/api-client`) criadas no Sprint 8B.2. Completa os pendentes do Sprint 8B.1 (Document Upload, Review Dashboard, Professional Profile Setup).

**Escopo**:
- Criar `apps/provider-web` dentro do workspace NX (Next.js + Tailwind v4).
- **Document Upload (Step 3)**: Componente de upload de documentos no fluxo de onboarding.
- **Review Dashboard**: Interface para o prestador acompanhar status de verificação.
- **Professional Profile Setup**: Seleção de categorias e serviços após credenciamento.
- **Provider Profile Page**: Página de perfil público do prestador (com slug do Sprint 8B.2).
- Autenticação Keycloak (cliente `meajudaai-provider`).
- Estilo visual alinhado com Customer App (Tailwind v4 + componentes `libs/ui`).

---

### ⏳ Sprint 8D - Admin Portal Migration (Blazor → React + NX)

**Periodo Estimado**: 2 - 15 Abr 2026
**Foco**: Migração do Admin Portal de Blazor WASM para React dentro do workspace NX
**Branch**: (a ser criada: `feature/sprint-8d-admin-migration`)

**Contexto**: Terceiro app React no workspace NX. Reutiliza padrões e shared libs consolidados pelo Customer (Sprint 8A) e Provider App (Sprint 8C). Elimina dual-stack (Blazor + React) em favor de single-stack React.

**Escopo**:
- Criar `apps/admin-web` dentro do workspace NX (Next.js + Tailwind v4).
- Migrar todas as funcionalidades existentes do Blazor Admin Portal:
  - Dashboard com KPIs e gráficos (Providers por status/tipo)
  - CRUD Providers (Create, Update, Delete, Verify)
  - Gestão de Documentos (Upload, Verificação, Rejeição)
  - Gestão de Service Catalogs (Categorias + Serviços)
  - Gestão de Restrições Geográficas (AllowedCities)
  - Dark Mode, Localização (i18n), Acessibilidade
- Substituir Fluxor por Zustand ou Redux Toolkit (state management React).
- Substituir Refit/C# DTOs por `libs/api-client` (gerado via OpenAPI ou manual).
- Manter autenticação Keycloak (cliente `meajudaai-admin`).
- Estilo visual unificado com Customer e Provider Apps.
- Remover projeto Blazor WASM após migração completa e validação.

---

### ⏳ Sprint 8E - Mobile App (React Native + Expo)

**Periodo Estimado**: 16 - 29 Abr 2026
**Foco**: App Mobile Nativo (iOS/Android) com Expo
**Branch**: (a ser criada: `feature/sprint-8e-mobile-app`)

**Escopo**:
- Criar `apps/mobile` dentro do workspace NX (React Native + Expo).
- Portar funcionalidades do Customer Web App para Mobile.
- Reutilizar lógica de negócio e autenticação via shared libs NX.
- Notificações Push.

---

**Status**: SKIPPED durante Parts 10-15 (escopo muito grande)  
**Prioridade**: Alta (recomendado antes do MVP)  
**Estimativa**: 3-5 dias de sprint dedicado

**Contexto**: A Part 13 foi intencionalmente pulada durante a implementação das Parts 10-15 (melhorias menores) por ser muito extensa e merecer um sprint dedicado. Testes unitários frontend são críticos para manutenibilidade e confiança no código, mas requerem setup completo de infraestrutura de testes.

**Escopo Planejado**:

**1. Infraestrutura de Testes** (1 dia):
- Criar projeto `MeAjudaAi.Web.Admin.Tests`
- Adicionar pacotes: bUnit, Moq, FluentAssertions, xUnit
- Configurar test host e service mocks
- Setup de TestContext base reutilizável

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
- Test de autorização e permissões

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

**5. Testes de Serviços** (0.5 dia):
- LocalizationService: culture switching, string retrieval
- ErrorHandlingService: retry logic, status code mapping
- LiveRegionService: announcement queue
- ErrorLoggingService: correlation IDs
- PermissionService: policy checks

**Meta de Cobertura**:
- **Reducers**: >95% (lógica pura, fácil de testar)
- **Effects**: >80% (com mocks de APIs)
- **Components**: >70% (rendering e interações básicas)
- **Services**: >90% (lógica de negócio)
- **Geral**: >80% code coverage

**Benefícios Esperados**:
- ✅ Confidence em refactorings futuros
- ✅ Documentação viva do comportamento esperado
- ✅ Detecção precoce de regressões
- ✅ Facilita onboarding de novos devs
- ✅ Reduz bugs em produção

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

**Priorização Sugerida**:
1. **Crítico (antes do MVP)**: Reducers + Effects + ErrorHandlingService
2. **Importante (pré-MVP)**: Componentes principais (Providers, Documents)
3. **Nice-to-have (pós-MVP)**: Componentes de UI (dialogs, shared)

**Recomendação**: Implementar em **Sprint 8.5** (entre Customer App e Buffer) ou dedicar 1 semana do Sprint 9 (Buffer) para esta tarefa. Frontend tests são investimento de longo prazo essencial para manutenibilidade.

---

### ✅ Sprint 7 - Blazor Admin Portal Features - CONCLUÍDA (6-7 Jan 2026)

**Branch**: `blazor-admin-portal-features` (MERGED to master)

**Objetivos**:
1. ✅ **CRUD Completo de Providers** (6-7 Jan 2026) - Create, Update, Delete, Verify
2. ✅ **Gestão de Documentos** (7 Jan 2026) - Upload, verificação, deletion workflow
3. ✅ **Gestão de Service Catalogs** (7 Jan 2026) - CRUD de categorias e serviços
4. ✅ **Gestão de Restrições Geográficas** (7 Jan 2026) - UI para AllowedCities com banco de dados
5. ✅ **Gráficos Dashboard** (7 Jan 2026) - MudCharts com providers por status e evolução temporal
6. ✅ **Testes** (7 Jan 2026) - Aumentar cobertura para 30 testes bUnit

**Progresso Atual**: 6/6 features completas ✅ **SPRINT 7 CONCLUÍDO 100%!**

**Detalhamento - Provider CRUD** ✅:
- IProvidersApi enhanced: CreateProviderAsync, UpdateProviderAsync, DeleteProviderAsync, UpdateVerificationStatusAsync
- CreateProviderDialog: formulário completo com validação (ProviderType, Name, FantasyName, Document, Email, Phone, Description, Address)
- EditProviderDialog: edição simplificada (nome/telefone - aguardando DTO enriquecido do backend)
- VerifyProviderDialog: mudança de status de verificação (Verified, Rejected, Pending + optional notes)
- Providers.razor: action buttons (Edit, Delete, Verify) com MessageBox confirmation
- Result<T> error handling pattern em todas operações
- Portuguese labels + Snackbar notifications
- Build sucesso (19 warnings Sonar apenas)
- Commit: cd2be7f6 "feat(admin): complete Provider CRUD operations"

**Detalhamento - Documents Management** ✅:
- DocumentsState/Actions/Reducers/Effects: Fluxor pattern completo
- Documents.razor: página com provider selector e listagem de documentos
- MudDataGrid com status chips coloridos (Verified=Success, Rejected=Error, Pending=Warning, Uploaded=Info)
- ProviderSelectorDialog: seleção de provider da lista existente
- UploadDocumentDialog: MudFileUpload com tipos de documento (RG, CNH, CPF, CNPJ, Comprovante, Outros)
- RequestVerification action via IDocumentsApi.RequestDocumentVerificationAsync
- DeleteDocument com confirmação MessageBox
- Real-time status updates via Fluxor Dispatch
- Portuguese labels + Snackbar notifications
- Build sucesso (28 warnings Sonar apenas)
- Commit: e033488d "feat(admin): implement Documents management feature"

**Detalhamento - Service Catalogs** ✅:
- IServiceCatalogsApi enhanced: 10 métodos (Create, Update, Delete, Activate, Deactivate para Categories e Services)
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

**Detalhamento - Geographic Restrictions** ✅:
- ILocationsApi já possuía CRUD completo (Create, Update, Delete, GetAll, GetById, GetByState)
- LocationsState/Actions/Reducers/Effects: Fluxor pattern completo
- AllowedCities.razor: full CRUD page com MudDataGrid
- CreateAllowedCityDialog: formulário com City, State, Country, Latitude, Longitude, ServiceRadiusKm, IsActive
- EditAllowedCityDialog: mesmo formulário para edição
- MudDataGrid com coordenadas em formato F6 (6 decimais), status chips (Ativa/Inativa)
- Toggle activation via MudSwitch (updates backend via UpdateAllowedCityAsync)
- Delete confirmation com MessageBox
- Portuguese labels + Snackbar notifications
- Build sucesso (42 warnings Sonar/MudBlazor apenas)
- Commit: 3317ace3 "feat(admin): implement Geographic Restrictions - AllowedCities UI"

**Detalhamento - Dashboard Charts** ✅:
- Dashboard.razor enhanced com 2 charts interativos (MudBlazor built-in charts)
- Provider Status Donut Chart: agrupa providers por VerificationStatus (Verified, Pending, Rejected)
- Provider Type Pie Chart: distribuição entre Individual (Pessoa Física) e Company (Empresa)
- Usa ProvidersState existente (sem novos endpoints de backend)
- OnAfterRender lifecycle hook para update de dados quando providers carregam
- UpdateChartData() método com GroupBy LINQ queries
- Portuguese labels para tipos de provider
- Empty state messages quando não há providers cadastrados
- MudChart components com Width="300px", Height="300px", LegendPosition.Bottom
- Build sucesso (43 warnings Sonar/MudBlazor apenas)
- Commit: 0e0d0d81 "feat(admin): implement Dashboard Charts with MudBlazor"

**Detalhamento - Testes bUnit** ✅:
- 30 testes bUnit criados (objetivo: 30+) - era 10, adicionados 20 novos
- CreateProviderDialogTests: 4 testes (form fields, submit button, provider type, MudForm)
- DocumentsPageTests: 5 testes (provider selector, upload button, loading, document list, error)
- CategoriesPageTests: 4 testes (load action, create button, list, loading)
- ServicesPageTests: 3 testes (load actions, create button, list)
- AllowedCitiesPageTests: 4 testes (load action, create button, list, loading)
- Todos seguem pattern: Mock IState/IDispatcher/IApi, AddMudServices, JSRuntimeMode.Loose
- Verificam rendering, state management, user interactions
- Namespaces corrigidos: Modules.*.DTOs
- Build sucesso (sem erros de compilação)
- Commit: 2a082840 "test(admin): increase bUnit test coverage to 30 tests"

---

### ✅ Sprint 6 - Blazor Admin Portal Setup - CONCLUÍDA (30 Dez 2025 - 5 Jan 2026)

**Status**: MERGED to master (5 Jan 2026)

**Principais Conquistas**:
1. **Projeto Blazor WASM Configurado** ✅
   - .NET 10 com target `net10.0-browser`
   - MudBlazor 7.21.0 (Material Design UI library)
   - Fluxor 6.1.0 (Redux-pattern state management)
   - Refit 9.0.2 (Type-safe HTTP clients)
   - Bug workaround: `CompressionEnabled=false` (static assets .NET 10)

2. **Autenticação Keycloak OIDC Completa** ✅
   - Microsoft.AspNetCore.Components.WebAssembly.Authentication
   - Login/Logout flows implementados
   - Authentication.razor com 6 estados (LoggingIn, CompletingLoggingIn, etc.)
   - BaseAddressAuthorizationMessageHandler configurado
   - **Token Storage**: SessionStorage (Blazor WASM padrão)
   - **Refresh Strategy**: Automático via OIDC library (silent refresh)
   - **SDKs Refit**: Interfaces manuais com atributos Refit (não code-generated)
   - Documentação completa em `docs/keycloak-admin-portal-setup.md`

3. **Providers Feature (READ-ONLY)** ✅
   - Fluxor store completo (State/Actions/Reducers/Effects)
   - MudDataGrid com paginação server-side
   - IProvidersApi via Refit com autenticação
   - PagedResult<T> correto (Client.Contracts.Api)
   - VERIFIED_STATUS constant (type-safe)
   - Portuguese error messages

4. **Dashboard com KPIs** ✅
   - 3 KPIs: Total Providers, Pending Verifications, Active Services
   - IServiceCatalogsApi integrado (contagem real de serviços)
   - MudCards com Material icons
   - Fluxor stores para Dashboard state
   - Loading states e error handling

5. **Dark Mode com Fluxor** ✅
   - ThemeState management (IsDarkMode boolean)
   - Toggle button em MainLayout
   - MudThemeProvider two-way binding

6. **Layout Base** ✅
   - MainLayout.razor com MudDrawer + MudAppBar
   - NavMenu.razor com navegação
   - User menu com AuthorizeView
   - Responsive design (Material Design)

7. **Testes bUnit + xUnit** ✅
   - 10 testes criados (ProvidersPageTests, DashboardPageTests, DarkModeToggleTests)
   - JSInterop mock configurado (JSRuntimeMode.Loose)
   - MudServices registrados em TestContext
   - CI/CD integration (master-ci-cd.yml + pr-validation.yml)

8. **Localização Portuguesa** ✅
   - Todos comentários inline em português
   - Mensagens de erro em português
   - UI messages traduzidas (Authentication.razor)
   - Projeto language policy compliance

9. **Integração Aspire** ✅
   - Admin portal registrado em AppHost
   - Environment variables configuradas (ApiBaseUrl, Keycloak)
   - Build e execução via `dotnet run --project src/Aspire/MeAjudaAi.AppHost`

10. **Documentação** ✅
    - docs/keycloak-admin-portal-setup.md (manual configuração)
    - docs/testing/bunit-ci-cd-practices.md (atualizado)
    - Roadmap atualizado com progresso Sprint 6

11. **SDKs Completos para Sprint 7** ✅ (6 Jan 2026)
    - IDocumentsApi: Upload, verificação, gestão de documentos de providers
    - ILocationsApi: CRUD de cidades permitidas (AllowedCities)
    - DTOs criados: ModuleAllowedCityDto, Create/UpdateAllowedCityRequestDto
    - README melhorado: conceito de SDK, diagrama arquitetural, comparação manual vs SDK
    - 4/4 SDKs necessários para Admin Portal (Providers, Documents, ServiceCatalogs, Locations)

**Resultado Alcançado**:
- ✅ Blazor Admin Portal 100% funcional via Aspire
- ✅ Login/Logout Keycloak funcionando
- ✅ Providers listagem paginada (read-only)
- ✅ Dashboard com 3 KPIs reais (IServiceCatalogsApi)
- ✅ Dark mode toggle
- ✅ 10 testes bUnit (build verde)
- ✅ Portuguese localization completa
- ✅ 0 erros build (10 warnings - analyzers apenas)
- ✅ **4 SDKs completos** para Admin Portal (IProvidersApi, IDocumentsApi, IServiceCatalogsApi, ILocationsApi)
- ✅ **Documentação SDK** melhorada (conceito, arquitetura, exemplos práticos)

**✅ Próxima Etapa Concluída: Sprint 7 - Blazor Admin Portal Features** (6-7 Jan 2026)
- ✅ CRUD completo de Providers (create, update, delete, verify)
- ✅ Gestão de Documentos (upload, verificação, rejection)
- ✅ Gestão de Service Catalogs (categorias + serviços)
- ✅ Gestão de Restrições Geográficas (UI para AllowedCities)
- ✅ Gráficos Dashboard (MudCharts - providers por status, evolução temporal)
- ✅ Aumentar cobertura de testes (30+ testes bUnit)

---

## ✅ Sprint 5.5 - Refactor & Cleanup (19-30 Dez 2025)

**Status**: CONCLUÍDA

**Principais Conquistas**:
1. **Refatoração MeAjudaAi.Shared.Messaging** ✅
   - Factories organizados em pasta dedicada (`Messaging/Factories/`)
   - Services organizados em pasta dedicada (`Messaging/Services/`)
   - Options organizados em pasta dedicada (`Messaging/Options/`)
   - 4 arquivos: ServiceBusOptions, MessageBusOptions, RabbitMqOptions, DeadLetterOptions
   - IMessageBusFactory + MessageBusFactory separados
   - IDeadLetterServiceFactory + DeadLetterServiceFactory separados
   - 1245/1245 testes passando

2. **Extensions Padronizadas** ✅
   - 14 arquivos consolidados: CachingExtensions, CommandsExtensions, DatabaseExtensions, etc.
   - BusinessMetricsMiddlewareExtensions extraído para arquivo próprio
   - Monitoring folder consolidation completo
   - Removidos 13 arquivos obsoletos (Extensions.cs genéricos + subpastas)

3. **Extension Members (C# 14)** ✅
   - EnumExtensions migrado para nova sintaxe `extension<TEnum>(string value)`
   - 18/18 testes passando (100% compatibilidade)
   - Documentado em architecture.md - seção "C# 14 Features Utilizados"
   - Avaliado DocumentExtensions (não adequado para extension properties)

4. **TODOs Resolvidos** ✅
   - 12/12 TODOs no código resolvidos ou documentados
   - Remaining issues movidos para technical-debt.md com priorização
   - api-reference.md removido (redundante com ReDoc + api-spec.json)

5. **Documentação Atualizada** ✅
   - architecture.md atualizado com C# 14 features
   - technical-debt.md atualizado com status atual
   - roadmap.md atualizado com Sprint 5.5 completion
   - 0 warnings in build

**✅ Fase 1.5: CONCLUÍDA** (21 Nov - 10 Dez 2025)  
Fundação técnica para escalabilidade e produção:
- ✅ Migration .NET 10 + Aspire 13 (Sprint 0 - CONCLUÍDO 21 Nov, MERGED to master)
- ✅ Geographic Restriction + Module Integration (Sprint 1 - CONCLUÍDO 2 Dez, MERGED to master)
- ✅ Test Coverage 90.56% (Sprint 2 - CONCLUÍDO 10 Dez - META 35% SUPERADA EM 55.56pp!)
- ✅ GitHub Pages Documentation Migration (Sprint 3 Parte 1 - CONCLUÍDO 11 Dez - DEPLOYED!)

**✅ Sprint 3 Parte 2: CONCLUÍDA** (11 Dez - 13 Dez 2025)  
Admin Endpoints & Tools - TODAS AS PARTES FINALIZADAS:
- ✅ Admin: Endpoints CRUD para gerenciar cidades permitidas (COMPLETO)
  - ✅ Banco de dados: LocationsDbContext + migrations
  - ✅ Domínio: AllowedCity entity + IAllowedCityRepository
  - ✅ Handlers: CRUD completo (5 handlers)
  - ✅ Endpoints: GET/POST/PUT/DELETE configurados
  - ✅ Exception Handling: Domain exceptions + IExceptionHandler (404/400 corretos)
  - ✅ Testes: 4 integration + 15 E2E (100% passando)
  - ✅ Quality: 0 warnings, dotnet format executado
- ✅ Tools: Bruno Collections para todos módulos (35+ arquivos .bru)
- ✅ Scripts: Auditoria completa e documentação (commit b0b94707)
- ✅ Module Integrations: Providers ↔ ServiceCatalogs + Locations
- ✅ Code Quality: NSubstitute→Moq, UuidGenerator, .slnx, SonarQube warnings
- ✅ CI/CD: Formatting checks corrigidos, exit code masking resolvido

**✅ Sprint 4: CONCLUÍDO** (14 Dez - 16 Dez 2025)  
Health Checks Robustos + Data Seeding para MVP - TODAS AS PARTES FINALIZADAS:
- ✅ Health Checks: DatabasePerformanceHealthCheck (latência <100ms healthy, <500ms degraded)
- ✅ Health Checks: ExternalServicesHealthCheck (Keycloak + IBGE API + Redis)
- ✅ Health Checks: HelpProcessingHealthCheck (sistema de ajuda operacional)
- ✅ Health Endpoints: /health, /health/live, /health/ready com JSON responses
- ✅ Health Dashboard: Dashboard nativo do Aspire (decisão arquitetural - não usar AspNetCore.HealthChecks.UI)
- ✅ Health Packages: AspNetCore.HealthChecks.Npgsql 9.0.0, .Redis 8.0.1
- ✅ Redis Health Check: Configurado via AddRedis() com tags 'ready', 'cache'
- ✅ Data Seeding: infrastructure/database/seeds/01-seed-service-catalogs.sql (8 categorias + 12 serviços)
- ✅ Seed Automation: Docker Compose executa seeds automaticamente na inicialização
- ✅ Project Structure: Reorganização - automation/ → infrastructure/automation/, seeds em infrastructure/database/seeds/
- ✅ Documentation: README.md, scripts/README.md, infrastructure/database/README.md + docs/future-external-services.md
- ✅ MetricsCollectorService: Implementado com IServiceScopeFactory (4 TODOs resolvidos)
- ✅ Unit Tests: 14 testes para ExternalServicesHealthCheck (6 novos para IBGE API)
- ✅ Integration Tests: 9 testes para DataSeeding (categorias, serviços, idempotência)
- ✅ Future Services Documentation: Documentado OCR, payments, SMS/email (quando implementar)
- ✅ Code Review: Logs traduzidos para inglês conforme política (Program.cs - 3 mensagens)
- ✅ Markdown Linting: technical-debt.md corrigido (code blocks, URLs, headings)
- ✅ Architecture Test: PermissionHealthCheckExtensions exception documentada (namespace vs folder structure)

**✅ Sprint 5: CONCLUÍDO ANTECIPADAMENTE** (Tarefas completadas nos Sprints 3-4)  
Todas as tarefas planejadas já foram implementadas:
- ✅ NSubstitute → Moq migration (Sprint 3)
- ✅ UuidGenerator unification (commit 0a448106)
- ✅ .slnx migration (commit 1de5dc1a)
- ✅ Design patterns documentation (architecture.md)
- ✅ Bruno collections para todos módulos (Users, Providers, Documents)

**⏳ Sprint 5.5: CONCLUÍDA** (19-20 Dez 2025) ✅
**Branch**: `feature/refactor-and-cleanup`  
**Objetivo**: Refatoração técnica e redução de débito técnico antes do frontend

**✅ Refatoramento de Testes Completado** (20 Dez 2025):
- ✅ Reorganização estrutural de MeAjudaAi.Shared.Tests (TestInfrastructure com 8 subpastas)
- ✅ ModuleExtensionsTests movidos para módulos individuais (Documents, Providers, ServiceCatalogs, Users)
- ✅ Tradução de ~35 comentários para português (mantendo AAA em inglês)
- ✅ Separação de classes aninhadas (LoggingConfigurationExtensionsTests, TestEvent, BenchmarkResult, BenchmarkExtensions)
- ✅ Remoção de duplicados (DocumentExtensionsTests, EnumExtensionsTests, SearchableProviderTests)
- ✅ GeographicRestrictionMiddlewareTests movido para Unit/Middleware/
- ✅ TestPerformanceBenchmark: classes internas separadas
- ✅ 11 commits de refatoramento com build verde

**✅ Correção PostGIS Integration Tests** (20 Dez 2025):
- ✅ Imagem Docker atualizada: postgres:15-alpine → postgis/postgis:15-3.4
- ✅ EnsurePostGisExtensionAsync() implementado em fixtures
- ✅ Connection string com 'Include Error Detail=true' para diagnóstico
- ✅ Suporte completo a dados geográficos (NetTopologySuite/GeoPoint)
- ✅ Migrations SearchProviders agora passam na pipeline

**Resumo da Sprint**:
- ✅ 15 commits com melhorias significativas
- ✅ Todos TODOs críticos resolvidos
- ✅ Testes melhorados (Provider Repository, Azurite)
- ✅ Messaging refatorado (IRabbitMqInfrastructureManager extraído)
- ✅ Extensions consolidadas (BusinessMetricsMiddleware)
- ✅ Upload file size configurável (IOptions pattern)
- ✅ Build sem warnings (0 warnings)
- ✅ Documentação atualizada (architecture.md, configuration.md)
- ✅ Code review aplicado (logs em inglês, path matching preciso, XML docs)

**Atividades Planejadas** (14 tarefas principais):

**1. Resolução de TODOs Críticos (Alta Prioridade)** - ✅ 8-12h CONCLUÍDO
- [x] IBGE Middleware Fallback - Fix validation when IBGE fails (3 TODOs em IbgeUnavailabilityTests.cs) ✅
- [x] Rate Limiting Cache Cleanup - Memory leak prevention (MaxPatternCacheSize=1000) ✅
- [x] Email Constraint Database Fix - Schema issue (clarified as not-yet-implemented) ✅
- [x] Azurite/Blob Storage - Container auto-creation with thread-safe initialization ✅
- [x] Provider Repository Tests - Documentation updated (unit vs integration) ✅
- [x] BusinessMetrics - Already extracted (no action needed) ✅
- [x] Monitoring - Structure already adequate (no action needed) ✅
- [x] Middleware UseSharedServices Alignment - TODO #249 RESOLVIDO ✅ (19 Dez 2025)
- [x] Azurite Integration Tests - Configured deterministic blob storage tests ✅ (19 Dez 2025)

**2. Melhorias de Testes (Média Prioridade)** - 4-6h
- [x] Testes Infrastructure Extensions - RESOLVIDO: não aplicável ✅ (19 Dez 2025)
  - Extensions de configuração (Keycloak/PostgreSQL/Migration) validadas implicitamente em E2E/integração
  - Testes unitários teriam baixo ROI (mockaria apenas chamadas de configuração)
  - Infraestrutura validada quando AppHost sobe e containers inicializam
- [x] Provider Repository Tests - Duplicação RESOLVIDA ✅ (19 Dez 2025)
  - Removidos testes unitários com mocks (260 linhas redundantes)
  - Adicionados 5 testes de integração faltantes (DeleteAsync, GetByIdsAsync, ExistsByUserIdAsync)
  - 17 testes de integração com validação REAL de persistência
  - Redução de manutenção + maior confiança nos testes

**3. Refatoração MeAjudaAi.Shared.Messaging** - 8-10h
- [x] ~~Separar NoOpDeadLetterService em arquivo próprio~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair DeadLetterStatistics e FailureRate para arquivos separados~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory, MessageRetryExtensions~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Todos os 1245 testes do Shared passando~~ ✅ CONCLUÍDO (19 Dez 2025)
- [✓] ~~Organizar Factories em pasta dedicada~~ - ✅ CONCLUÍDO (19 Dez 2025)
  - Criada pasta `Messaging/Factories/`
  - `MessageBusFactory` e `DeadLetterServiceFactory` organizados
  - Interfaces e implementações em arquivos separados
  - `EnvironmentBasedDeadLetterServiceFactory` → `DeadLetterServiceFactory`
- [✓] ~~Organizar Services em pasta dedicada~~ - ✅ CONCLUÍDO (19 Dez 2025)
  - Criada pasta `Messaging/Services/`
  - `ServiceBusInitializationService` movido para organização
- [✓] ~~Organizar Options em pasta dedicada~~ - ✅ CONCLUÍDO (19 Dez 2025)
  - Criada pasta `Messaging/Options/`
  - 4 arquivos organizados: `ServiceBusOptions`, `MessageBusOptions`, `RabbitMqOptions`, `DeadLetterOptions`
  - Namespace unificado: `MeAjudaAi.Shared.Messaging.Options`
- [✓] ~~Criar IMessageBusFactory + renomear MessageBusFactory.cs → EnvironmentBasedMessageBusFactory.cs~~ - ✅ CONCLUÍDO (19 Dez 2025)
  - Invertido: Criada interface `IMessageBusFactory` em arquivo próprio
  - Classe `EnvironmentBasedMessageBusFactory` renomeada para `MessageBusFactory`
  - Movido de `NoOp/Factory/` para raiz `Messaging/`
  - Um arquivo por classe seguindo SRP
- [x] Extrair IRabbitMqInfrastructureManager para arquivo separado ✅ (19 Dez 2025)
- [ ] Adicionar Integration Events faltantes nos módulos (Documents, SearchProviders, ServiceCatalogs?) - BACKLOG
- [ ] Reorganização geral da estrutura de pastas em Messaging - BACKLOG
- [ ] Adicionar testes unitários para classes de messaging - BACKLOG

**4. Refatoração Extensions (MeAjudaAi.Shared)** - ✅ 8h CONCLUÍDO
- [x] ~~Padronizar Extensions: criar arquivo [FolderName]Extensions.cs por funcionalidade~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] Extension Members (C# 14): EnumExtensions migrado com sucesso ✅ CONCLUÍDO (19 Dez 2025)
- [x] BusinessMetricsMiddlewareExtensions: Já existe em Extensions/ ✅ CONCLUÍDO (19 Dez 2025)
- [x] Monitoring folder consolidation: Estrutura já adequada ✅ CONCLUÍDO (19 Dez 2025)
  - Consolidados: CachingExtensions, CommandsExtensions, DatabaseExtensions, EventsExtensions
  - ExceptionsExtensions, LoggingExtensions, MessagingExtensions, QueriesExtensions, SerializationExtensions
  - Removidos 13 arquivos obsoletos (Extensions.cs genéricos + subpastas)
  - 1245/1245 testes passando
- [x] ~~Migração para Extension Members (C# 14)~~ ✅ AVALIADO (19 Dez 2025)
  - ✅ Sintaxe `extension(Type receiver)` validada e funcional no .NET 10
  - ✅ Novos recursos disponíveis: extension properties, static extensions, operators
  - ✅ Documentado em `docs/architecture.md` - seção "C# 14 Features Utilizados"
  - 📋 Planejamento: Agendado como última atividade da Sprint 5.5
  - 📝 Recomendação: Usar Extension Members em NOVOS códigos que se beneficiem de properties
- [x] Extrair BusinessMetricsMiddlewareExtensions de BusinessMetricsMiddleware.cs ✅ (19 Dez 2025)
- [x] Consolidar Monitoring folder (MonitoringExtensions.cs único) ✅ (19 Dez 2025)
- [ ] Revisar padrão de extensões em todas as funcionalidades do Shared

**5. Code Quality & Cleanup (Baixa Prioridade)** - 3-4h
- [x] Padronização de Records - Análise concluída ✅ (19 Dez 2025)
  - Property-based records: DTOs/Requests (mutabilidade com `init`)
  - Positional records: Domain Events, Query/Command DTOs (imutabilidade)
  - Pattern adequado ao contexto de uso
- [ ] Upload File Size Configuration - Tornar configurável (UploadDocumentCommandHandler.cs:90)
- [x] ~~Remover api-reference.md (redundante com ReDoc + api-spec.json)~~ ✅ CONCLUÍDO (19 Dez)

**6. Testes E2E SearchProviders** - 2-3 sprints (BACKLOG)
- [ ] 15 testes E2E cobrindo cenários principais de busca
- [ ] Validação de integração IBGE API, filtros, paginação
- [ ] Autenticação/autorização em todos endpoints

**7. Review Completo de Testes** - 6-8h
- [ ] Auditoria completa de todos os arquivos em tests/
- [ ] Identificar testes duplicados, obsoletos ou mal estruturados
- [ ] Validar coverage e identificar gaps
- [ ] Documentar padrões de teste para novos contribuidores

**8. Migração Extension Members (C# 14) - FINAL SPRINT ACTIVITY** - ✅ 2h CONCLUÍDO
- [x] Migrar EnumExtensions para syntax `extension<TEnum>(string value)` ✅
- [x] 18/18 testes passando (100% compatibilidade) ✅
- [x] Documentar patterns e guidelines em architecture.md ✅
- [x] Avaliado DocumentExtensions (não adequado para extension properties) ✅

**8. BDD Implementation (BACKLOG - Futuro)** - Sprint dedicado planejado
- [ ] Setup SpecFlow + Playwright.NET para acceptance tests
- [ ] Implementar 5-10 features críticas em Gherkin (Provider Registration, Document Upload, Service Catalog)
- [ ] Integrar ao CI/CD pipeline
- [ ] Criar documentação executável com Gherkin
- **Benefício**: Testes de aceitação legíveis para stakeholders e documentação viva do sistema
- **Timing**: Implementação prevista APÓS desenvolvimento do Customer App (Sprint 8+)
- **Escopo**: Testes end-to-end de fluxos completos (Frontend → Backend → APIs terceiras)
- **Foco**: Fluxos críticos de usuário utilizados por Admin Portal e Customer App

**Critérios de Aceitação**:
- [x] Todos os 12 TODOs no código resolvidos ou documentados ✅
- [x] ~~Messaging refatorado com estrutura clara de pastas~~ ✅ CONCLUÍDO (19 Dez)
- [x] ~~Extensions consolidadas por funcionalidade~~ ✅ CONCLUÍDO (19 Dez)
- [x] Extension Blocks (C# 14) avaliado e implementado onde aplicável ✅ (19 Dez)
- [x] Testes de infrastructure com >70% coverage (resolvido: não aplicável) ✅ (19 Dez)
- [x] 0 warnings no build ✅ (19 Dez)
- [x] Documentação técnica atualizada ✅ (19 Dez)

**Estimativa Total**: 35-45 horas de trabalho técnico (10h já concluídas)  
**Benefício**: Backend robusto e manutenível para suportar desenvolvimento do frontend Blazor

**📝 Próxima Atividade Recomendada**: Migração para Extension Blocks (C# 14) - 4-6h
- Avaliar novo recurso de linguagem para melhorar organização de extension methods
- Migrar métodos de propósito geral (PermissionExtensions, EnumExtensions)
- Manter padrão atual para DI extensions ([FolderName]Extensions.cs)

**✅ Sprint 5.5 Completed** (19-30 Dez 2025):
- Refatoração MeAjudaAi.Shared.Messaging (Factories, Services, Options)
- Extensions padronizadas (14 arquivos consolidados)
- Extension Members (C# 14) implementado
- TODOs resolvidos (12/12 concluídos)
- Dependabot PRs fechados para regeneração
- 1245/1245 testes passando

**⏳ Fase 2: EM ANDAMENTO** (Janeiro–Maio 2026)  
Frontend React (NX Monorepo) + Mobile:
- Sprint 6: Blazor Admin Portal Setup - ✅ CONCLUÍDO (5 Jan 2026)
- Sprint 7: Blazor Admin Portal Features (6-24 Jan 2026) - ✅ CONCLUÍDO
- Sprint 7.16: Technical Debt Sprint (17-21 Jan 2026) - 🔄 EM PROGRESSO (Task 5 movida p/ Sprint 9)
- Sprint 8A: Customer App (5-18 Fev 2026) - ✅ Concluído
- Sprint 8B: Authentication & Onboarding (19 Fev - 4 Mar 2026) - ✅ CONCLUÍDO
- Sprint 8B.2: Technical Excellence & NX Monorepo (5-18 Mar 2026) - 🔄 EM PROGRESSO
- Sprint 8C: Provider Web App (19 Mar - 1 Abr 2026) - ⏳ Planejado
- Sprint 8D: Admin Portal Migration Blazor → React (2-15 Abr 2026) - ⏳ Planejado
- Sprint 8E: Mobile App (16-29 Abr 2026) - ⏳ Planejado
- Sprint 9: BUFFER (30 Abr - 6 Mai 2026) - ⏳ Planejado
- MVP Final: 9 de Maio de 2026
- *Nota: Data de MVP atualizada para 9 de Maio de 2026 para acomodar NX Monorepo, Provider App, Admin Migration e Mobile App.*

**⚠️ Risk Assessment**: Estimativas assumem velocidade consistente. NX Monorepo setup e Admin Migration são os maiores riscos de escopo. Sprint 9 reservado como buffer de contingência.

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

<a id="cronograma-de-sprints"></a>
## 📅 Cronograma de Sprints (Novembro 2025-Março 2026)

| Sprint | Duração | Período | Objetivo | Status |
|--------|---------|---------|----------|--------|
| **Sprint 0** | 4 semanas | Jan 20 - 21 Nov | Migration .NET 10 + Aspire 13 | ✅ CONCLUÍDO (21 Nov - MERGED) |
| **Sprint 1** | 10 dias | 22 Nov - 2 Dez | Geographic Restriction + Module Integration | ✅ CONCLUÍDO (2 Dez - MERGED) |
| **Sprint 2** | 1 semana | 3 Dez - 10 Dez | Test Coverage 90.56% | ✅ CONCLUÍDO (10 Dez - META SUPERADA!) |
| **Sprint 3-P1** | 1 dia | 10 Dez - 11 Dez | GitHub Pages Documentation | ✅ CONCLUÍDO (11 Dez - DEPLOYED!) |
| **Sprint 3-P2** | 2 semanas | 11 Dez - 13 Dez | Admin Endpoints & Tools | ✅ CONCLUÍDO (13 Dez - MERGED) |
| **Sprint 4** | 5 dias | 14 Dez - 18 Dez | Health Checks + Data Seeding | ✅ CONCLUÍDO (18 Dez - MERGED!) |
| **Sprint 5** | - | Sprints 3-4 | Quality Improvements | ✅ CONCLUÍDO ANTECIPADAMENTE |
| **Sprint 5.5** | 2 semanas | 19 Dez - 31 Dez | Refactor & Cleanup (Technical Debt) | ✅ CONCLUÍDO (30 Dez 2025) |
| **Sprint 6** | 1 semana | 30 Dez - 5 Jan | Blazor Admin Portal - Setup & Core | ✅ CONCLUÍDO (5 Jan 2026) |
| **Sprint 7** | 3 semanas | 6 - 24 Jan | Blazor Admin Portal - Features | ✅ CONCLUÍDO |
| **Sprint 7.16** | 1 semana | 17-21 Jan | Technical Debt Sprint | 🔄 EM PROGRESSO |
| **Sprint 8** | 2 semanas | 5 - 18 Fev | Customer Web App (Web) | ✅ CONCLUÍDO |
| **Sprint 8B** | 2 semanas | 19 Fev - 4 Mar | Authentication & Onboarding | ✅ CONCLUÍDO |
| **Sprint 8C** | 2 semanas | 5-18 Mar | Mobile App | ⏳ Planejado |
| **Sprint 9** | 1 semana | 19-25 Mar | **BUFFER: Polishing, Refactoring & Risk Mitigation** | ⏳ Planejado |
| **MVP Launch** | - | 28 de Março de 2026 | Final deployment & launch preparation | 🎯 Target |

**MVP Launch Target**: 28 de Março de 2026 🎯  
*Atualizado para 28 de Março de 2026.*

**Post-MVP (Fase 3+)**: Reviews, Assinaturas, Agendamentos (Abril 2026+)

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
  - `GET /api/v1/service-catalogs/categories` - Listar categorias
  - `GET /api/v1/service-catalogs/categories/{id}` - Buscar categoria
  - `POST /api/v1/service-catalogs/categories` - Criar categoria
  - `PUT /api/v1/service-catalogs/categories/{id}` - Atualizar categoria
  - `POST /api/v1/service-catalogs/categories/{id}/activate` - Ativar
  - `POST /api/v1/service-catalogs/categories/{id}/deactivate` - Desativar
  - `DELETE /api/v1/service-catalogs/categories/{id}` - Deletar
  - `GET /api/v1/service-catalogs/services` - Listar serviços
  - `GET /api/v1/service-catalogs/services/{id}` - Buscar serviço
  - `GET /api/v1/service-catalogs/services/category/{categoryId}` - Por categoria
  - `POST /api/v1/service-catalogs/services` - Criar serviço
  - `PUT /api/v1/service-catalogs/services/{id}` - Atualizar serviço
  - `POST /api/v1/service-catalogs/services/{id}/change-category` - Mudar categoria
  - `POST /api/v1/service-catalogs/services/{id}/activate` - Ativar
  - `POST /api/v1/service-catalogs/services/{id}/deactivate` - Desativar
  - `DELETE /api/v1/service-catalogs/services/{id}` - Deletar
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

**Status**: ✅ CONCLUÍDO (10 Dez 2025) - Branch: `improve-tests-coverage-2`

**Objetivos**:
- Migrar todos projetos para .NET 10 LTS
- Atualizar Aspire para v13
- Atualizar dependências (EF Core 10, Npgsql 10, etc.)
- Validar testes e corrigir breaking changes
- Atualizar CI/CD para usar .NET 10 SDK

**Tarefas**:
- [x] Criar branch `migration-to-dotnet-10` ✅
- [x] Merge master (todos módulos Fase 1) ✅
- [x] Atualizar `Directory.Packages.props` para .NET 10 ✅
- [x] Atualizar todos `.csproj` para `<TargetFramework>net10.0</TargetFramework>` ✅
- [x] Atualizar Aspire packages para v13.0.2 ✅
- [x] Atualizar EF Core para 10.0.1 GA ✅
- [x] Atualizar Npgsql para 10.0.0 GA ✅
- [x] `dotnet restore` executado com sucesso ✅
- [x] **Verificação Incremental**:
  - [x] Build Domain projects → ✅ sem erros
  - [x] Build Application projects → ✅ sem erros
  - [x] Build Infrastructure projects → ✅ sem erros
  - [x] Build API projects → ✅ sem erros
  - [x] Build completo → ✅ 0 warnings, 0 errors
  - [x] Fix testes Hangfire (Skip para CI/CD) ✅
  - [x] Run unit tests → ✅ 480 testes (479 passed, 1 skipped)
  - [x] Run integration tests → ✅ validados com Docker
- [x] Atualizar CI/CD workflows (removido --locked-mode) ✅
- [x] Validar Docker images com .NET 10 ✅
- [x] Merge para master após validação completa ✅

**Resultado Alcançado**:
- ✅ Sistema rodando em .NET 10 LTS com Aspire 13.0.2
- ✅ Todos 480 testes passando (479 passed, 1 skipped)
- ✅ CI/CD funcional (GitHub Actions atualizado)
- ✅ Documentação atualizada
- ✅ EF Core 10.0.1 GA + Npgsql 10.0.0 GA (versões estáveis)

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

**📦 Pacotes Atualizados — Estado Misto (11 Dez 2025)**:

| Pacote | Versão Atual | Status | Notas |
|--------|--------------|--------|-------|
| **EF Core 10.x** | `10.0.1` | ✅ GA STABLE | Atualizado de 10.0.0-rc.2 → 10.0.1 GA |
| **Npgsql 10.x** | `10.0.0` | ✅ GA STABLE | Atualizado de 10.0.0-rc.1 → 10.0.0 GA |
| **Aspire 13.x** | `13.0.2` | ✅ GA STABLE | Atualizado de 13.0.0-preview.1 → 13.0.2 GA |
| **Aspire.Npgsql.EntityFrameworkCore.PostgreSQL** | `13.0.2` | ✅ GA STABLE | Sincronizado com Aspire 13.0.2 GA |
| **Hangfire.PostgreSql** | `1.20.13` | ⚠️ STABLE (Npgsql 6.x) | Monitorando compatibilidade com Npgsql 10.x |
| **EFCore.NamingConventions** | `10.0.0-rc.2` | ⚠️ PRE-RELEASE | Aguardando versão estável (issue template criado) |

**🆕 Atualizações via Dependabot (11 Dez 2025)**:

| Pacote | Versão Anterior | Versão Atual | PR | Status |
|--------|-----------------|--------------|-----|--------|
| **Microsoft.AspNetCore.Authentication.JwtBearer** | `10.0.0` | `10.0.1` | [#62](https://github.com/frigini/MeAjudaAi/pull/62) | ✅ MERGED |
| **Microsoft.AspNetCore.OpenApi** | `10.0.0` | `10.0.1` | [#64](https://github.com/frigini/MeAjudaAi/pull/64) | ✅ MERGED |
| **Microsoft.Extensions.Caching.Hybrid** | `10.0.0` | `10.1.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | ✅ MERGED |
| **Microsoft.Extensions.Http.Resilience** | `10.0.0` | `10.1.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | ✅ MERGED |
| **Serilog** | `4.2.0` | `4.3.0` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | ✅ MERGED |
| **Serilog.Sinks.Console** | `6.0.0` | `6.1.1` | [#63](https://github.com/frigini/MeAjudaAi/pull/63) | ✅ MERGED |

**✅ Resultado**: Pacotes core (EF Core 10.0.1, Npgsql 10.0.0, Aspire 13.0.2) atualizados para GA estáveis. EFCore.NamingConventions 10.0.0-rc.2 sob monitoramento (aguardando GA). Lockfiles regenerados e validados em CI/CD.

**⚠️ Pacotes Ainda a Monitorar**:

| Pacote | Versão Atual | Versão Estável Esperada | Impacto | Ação Requerida |
|--------|--------------|-------------------------|---------|----------------|
| **EFCore.NamingConventions** | `10.0.0-rc.2` | `10.0.0` (Q1 2026?) | MÉDIO | Monitorar <https://github.com/efcore/EFCore.NamingConventions> |
| **Hangfire.PostgreSql** | `1.20.13` | `2.0.0` com Npgsql 10+ | CRÍTICO | Monitorar <https://github.com/frankhommers/Hangfire.PostgreSql> |

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
- [x] Configurar GitHub Watch para dotnet/efcore ✅
- [x] Configurar GitHub Watch para npgsql/npgsql ✅
- [x] Configurar GitHub Watch para dotnet/aspire ✅
- [x] Configurar GitHub Watch para Hangfire.PostgreSql ✅
- [x] Issue template criado: `.github/ISSUE_TEMPLATE/efcore-naming-conventions-stable-monitoring.md` ✅
- [ ] Instalar `dotnet-outdated-tool` globalmente (opcional - monitoramento manual)
- [ ] Criar GitHub Actions workflow para verificação automática (`.github/workflows/check-dependencies.yml`) (Sprint 3)
- [x] Dependabot habilitado via GitHub (PRs automáticos ativos) ✅
- [ ] Adicionar lembrete mensal no calendário para verificação manual (backup)

**🔍 Pacotes Críticos Sem Compatibilidade .NET 10 Confirmada**:

1. **Hangfire.PostgreSql 1.20.12**
   - **Status**: Compilado contra Npgsql 6.x
   - **Risco**: Breaking changes em Npgsql 10.x não validados pelo mantenedor
   - **Mitigação Atual**: Testes de integração (marcados como Skip no CI/CD)
   - **Monitoramento**: 
     - GitHub Issues: [Hangfire.PostgreSql Issues](https://github.com/frankhommers/Hangfire.PostgreSql/issues)
     - Alternativas: Hangfire.Pro.Redis (pago), Hangfire.SqlServer (outro DB)
   - **Prazo**: Validar localmente ANTES de deploy para produção

2. **~~Swashbuckle.AspNetCore 10.0.1 - ExampleSchemaFilter~~** ✅ RESOLVIDO (13 Dez 2025)
   - **Status**: ExampleSchemaFilter **removido permanentemente**
   - **Razão**: Código problemático, difícil de testar, não essencial
   - **Alternativa**: Usar XML documentation comments para exemplos quando necessário
   - **Commit**: [Adicionar hash após commit]

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

**✅ Ações Concluídas Pós-Migration (10 Dez 2025)**:
1. ✅ Finalizar validação de testes (unit + integration) - 480 testes passando
2. ✅ Validar Hangfire localmente (com Aspire) - funcional
3. ✅ Configurar GitHub Watch para monitoramento de releases (EF Core, Npgsql, Aspire)
4. ✅ Issue template criado para EFCore.NamingConventions stable monitoring
5. ✅ Dependabot habilitado via GitHub (PRs automáticos)
6. ✅ Monitoramento ativo para Hangfire.PostgreSql 2.0 (Issue #39)

**📝 Notas de Compatibilidade**:
- **EF Core 10 RC**: Sem breaking changes conhecidos desde RC.1
- **Npgsql 10 RC**: Breaking changes documentados em <https://www.npgsql.org/doc/release-notes/10.0.html>
- **Aspire 13 Preview**: API estável, apenas features novas em desenvolvimento

---

### 📅 Sprint 1: Geographic Restriction + Module Integration (10 dias)

**Status**: 🔄 DIAS 1-6 CONCLUÍDOS | FINALIZANDO (22-25 Nov 2025)  
**Branches**: `feature/geographic-restriction` (merged ✅), `feature/module-integration` (em review), `improve-tests-coverage` (criada)  
**Documentação**: Análise integrada em [testing/coverage.md](./testing/coverage.md)

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

#### 🆕 Coverage Improvement: ✅ CONCLUÍDO NO SPRINT 2
- ✅ Coverage aumentado 28.2% → **90.56%** (+62.36pp - META 35% SUPERADA EM 55.56pp!)
- ✅ 480 testes (479 passing, 1 skipped) - Suite completa validada em CI/CD
- ✅ E2E tests para provider indexing flow implementados
- ✅ Integration tests completos com Docker/TestContainers
- ⏳ Criar .bru API collections para módulos (Sprint 3)
- ⏳ Atualizar tools/ projects (MigrationTool, etc.) (Sprint 3)
- **Resultado**: Sprint 2 concluído (10 Dez 2025) - Coverage report consolidado gerado

**Tarefas Detalhadas**:

#### 1. Integração Providers ↔ Documents ✅ CONCLUÍDO
- [x] Providers: Validar `HasVerifiedDocuments` antes de aprovar prestador ✅
- [x] Providers: Bloquear ativação se `HasRejectedDocuments` ou `HasPendingDocuments` ✅
- [x] Documents: Publicar `DocumentVerified` event para atualizar status de Providers ✅
- [x] Integration test: Fluxo completo de verificação de prestador ✅

#### 2. Integração Providers ↔ ServiceCatalogs ✅ IMPLEMENTADO
- [x] ServiceCatalogs: IServiceCatalogsModuleApi com 8 métodos implementados ✅
- [x] ServiceCatalogs: ValidateServicesAsync implementado ✅
- [x] ServiceCatalogs: Repository pattern com ServiceCategoryRepository ✅
- [x] Integration tests: 15 testes passando ✅
- ⏳ Providers: Integração de validação de serviços (Sprint 3)
- ⏳ Admin Portal: UI para gestão de categorias/serviços (Sprint 3)

#### 3. Integração SearchProviders ↔ Providers ✅ CONCLUÍDO
- [x] Search: Métodos IndexProviderAsync e RemoveProviderAsync implementados ✅
- [x] Search: Background handler consumindo ProviderVerificationStatusUpdated events ✅
- [x] Search: ISearchProvidersModuleApi com 2 métodos ✅
- [x] Integration test: Busca retorna apenas prestadores verificados ✅

#### 4. Integração Providers ↔ Locations ✅ IMPLEMENTADO
- [x] Locations: ILocationsModuleApi implementada ✅
- [x] Locations: GetAddressFromCepAsync com 3 providers (ViaCEP, BrasilAPI, OpenCEP) ✅
- [x] Locations: IBGE API integration para validação de municípios ✅
- [x] Unit tests: 67 testes passando (Locations module) ✅
- ⏳ Providers: Integração automática de CEP lookup (Sprint 3)

#### 5. Restrição Geográfica (MVP Blocker) ✅ CONCLUÍDO
- [x] Criar `AllowedCities` configuration em appsettings ✅
- [x] GeographicRestrictionMiddleware implementado com IBGE integration ✅
- [x] Fail-open fallback para validação simples quando IBGE unavailable ✅
- [x] Integration test: 24 testes passando ✅
- ⏳ Admin: Endpoint para gerenciar cidades permitidas (Sprint 3 - GitHub Pages docs)

**Resultado Alcançado (Sprint 1)**:
- ✅ Módulos integrados com business rules reais (Providers ↔ Documents, Providers ↔ SearchProviders)
- ✅ Operação restrita a cidades piloto configuradas (IBGE API validation)
- ✅ Background workers consumindo integration events (ProviderActivated, DocumentVerified)
- ✅ Validações cross-module funcionando (HasVerifiedDocuments, HasRejectedDocuments)
- ✅ Naming standardization (ILocationsModuleApi, ISearchProvidersModuleApi)
- ✅ CI/CD fix (secrets validation removido)
- ✅ **MERGED para master** (branch improve-tests-coverage-2 ativa para continuação)

---

### 📅 Sprint 2: Test Coverage Improvement - Phase 1 (2 semanas)

**Status**: ✅ CONCLUÍDO em 10 Dez 2025  
**Branches**: `improve-tests-coverage` (merged ✅), `improve-tests-coverage-2` (ativa - branch atual)

**Conquistas (26 Nov - 10 Dez)**:
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
  - **Código gerado excluído**: 213 linhas via ExcludeByFile patterns:
    - `**/*OpenApi*.generated.cs`
    - `**/System.Runtime.CompilerServices*.cs`
    - `**/*RegexGenerator.g.cs`
  - **Análise Correta**: 27.9% é coverage do **código de produção escrito manualmente**
- **Branch Coverage**: 21.7% (2,264/10,422 branches) - sem código gerado
- **Method Coverage**: 40.9% (2,168/5,294 métodos) - sem código gerado
- **Test Suite**: 1,407 testes totais (1,393 passing - 99.0%, 14 skipped - 1.0%, 0 failing)
- Target Phase 1: 35% (+7.1 percentage points from 27.9% baseline)
- Target Final Sprint 2: 50%+ (revised from 80% - more realistic)

**📊 Progressão de Coverage - Sprint 2 (Audit Trail)**:

| Medição | Valor | Data | Notas |
|---------|-------|------|-------|
| **Baseline Pré-Refactor** | 28.2% | 2 Dez | Estado inicial Sprint 2 |
| **Baseline Ajustado** | 27.9% | 2 Dez | Exclusão código gerado (OpenAPI + Regex) |
| **Pós-Adição de Testes** | 90.56% | 10 Dez | 40+ novos testes + consolidação |

**📈 Ganho Total**: +62.36 percentage points (28.2% → 90.56%)

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
- ⚠️ **Shared.Jobs**: 14.8% → **85%+** (HangfireHealthCheck, HangfireAuthorizationFilter testes criados - 20 Dez 2025)
  - ✅ **HangfireHealthCheck**: 7 unit tests (validação de status, thresholds, null checks)
  - ✅ **HangfireAuthorizationFilter**: 11 unit tests (ACL admin, ambientes, auth checks)
  - **Ação Completada**: Testes unitários criados, coverage estimada 85%+
- ⚠️ **Shared.Messaging.RabbitMq**: 12% (RabbitMqMessageBus)
  - **Motivo**: Integration tests require RabbitMQ container
  - **Ação**: TestContainers RabbitMQ ou mocks
- ⚠️ **Shared.Database.Exceptions**: 17% (PostgreSqlExceptionProcessor)
  - **Ação**: Unit tests para constraint exception handling

**Progresso Phase 1 (Improve-Tests-Coverage-2)**:
- ✅ **5 Commits**: aabba3d, 5ff84df, 88eaef8, 1ddbf4d, fbf02b9
- ✅ **40 New Tests**: Task 2 (10 DbContext) + Task 5 (27 health checks) + Task 1 (+3 fixes)
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

**Phase 1 Completion** - ✅ CONCLUÍDO (10 Dez 2025):
- ✅ **Coverage Report Generated**: coverage/report/index.html + Summary.txt
- ✅ **Roadmap Update**: Documento atualizado com coverage 90.56% alcançado
- ✅ **Warnings**: Build limpo, zero warnings críticos
- ✅ **Merged to Master**: PR #35 merged com sucesso

**Phase 2 Completion** - ✅ CONCLUÍDO (10 Dez 2025):
- ✅ **ServiceDefaults Health Checks**: Coberto via integration tests (coverage consolidada)
  - ✅ PostgresHealthCheck: Testado via TestContainers nos módulos
  - ✅ GeolocationHealthOptions: 67 testes no módulo Locations
  - ✅ Health checks architecture: 47 testes em Shared/Monitoring
  
- ✅ **Logging Infrastructure**: Cobertura via testes de módulos
  - ✅ Logging testado através de integration tests
  - ✅ CorrelationId tracking validado em E2E tests
  - ✅ LoggingContextMiddleware: Funcional em todos módulos
  
- ✅ **Messaging Resilience**: Coberto via integration events
  - ✅ Integration events: ProviderActivated, DocumentVerified testados
  - ✅ Event handlers: 15+ handlers com testes unitários
  - ✅ Message publishing: Validado em integration tests
  
- ✅ **Middlewares**: Testados via E2E e integration tests
  - ✅ GeographicRestrictionMiddleware: 24 integration tests
  - ✅ Authorization: Validado em 100+ E2E tests com auth
  - ✅ Request/Response pipeline: Coberto em ApiService.Tests
  
- ✅ **Database Exception Handling**: Coberto nos módulos
  - ✅ Repository pattern: Testado em todos 6 módulos
  - ✅ Constraint violations: Validados em integration tests
  - ✅ Transaction handling: Coberto em unit tests
  
- ✅ **Documents Module**: Implementado e testado
  - ✅ Document validation: 45+ testes unitários
  - ✅ DocumentRepository: Integration tests completos
  - ✅ Module API: IDocumentsModuleApi com 7 métodos testados

**Próximas Tarefas (Sprint 3 - GitHub Pages Documentation)**:
- [ ] Migrar documentação para MkDocs Material
- [ ] Criar .bru API collections para teste manual
- [ ] Implementar data seeding scripts
- [ ] Admin endpoints para geographic restrictions
- [ ] Finalizar integrações cross-module pendentes

**Objetivos Fase 1 (Dias 1-7) - ✅ CONCLUÍDO 2 DEZ 2025**:
- ✅ Aumentar coverage Shared de baseline para 28.2% (medição real)
- ✅ Focar em componentes críticos (Health Checks - 4/7 implementados)
- ✅ Documentar testes flaky (6 TestContainers scope issues documented)
- ✅ **NO REFLECTION** - todas classes public para manutenibilidade
- ✅ 40 novos testes criados (5 commits, 1,393/1,407 passing)
- ✅ Coverage report consolidado gerado (HTML + Text)

**Objetivos Fase 2 (Dias 8-14) - ✅ CONCLUÍDO 10 DEZ 2025**:
- ✅ ServiceDefaults: Coverage integrado ao report consolidado
- ✅ Shared.Logging: Cobertura aumentada com testes de módulos
- ✅ Shared.Messaging: Cobertura aumentada com testes de integração
- ✅ Shared.Database.Exceptions: Cobertura aumentada com testes de módulos
- ✅ **Overall Target SUPERADO**: 28.2% → **90.56%** (+62.36 percentage points!)

**Decisões Técnicas**:
- ✅ TestContainers para PostgreSQL (no InMemory databases)
- ✅ Moq para HttpMessageHandler (HttpClient mocking)
- ✅ FluentAssertions para assertions
- ✅ xUnit 3.1.5 como framework
- ✅ Classes public em vez de internal (no reflection needed)
- ⚠️ Testes flaky com concurrent scopes marcados como Skip (documentados)

**Health Checks Implementation** - ✅ CONCLUÍDO:
- ✅ **ExternalServicesHealthCheck**: Keycloak availability (9 testes - Shared/Monitoring)
- ✅ **PerformanceHealthCheck**: Memory, GC, thread pool (20 testes - Shared/Monitoring)
- ✅ **HelpProcessingHealthCheck**: Business logic operational (9 testes - Shared/Monitoring)
- ✅ **DatabasePerformanceHealthCheck**: DB metrics configured (9 testes - Shared/Monitoring)
- ✅ **ServiceDefaults.HealthChecks.PostgresHealthCheck**: Testado via TestContainers (integration tests)
- ✅ **Locations**: APIs de CEP health validadas (67 testes - ViaCEP, BrasilAPI, IBGE, OpenCEP)
- ✅ **Documents**: Module health validado via integration tests
- ✅ **Search**: PostGIS testado via SearchProviders integration tests

**Arquitetura de Health Checks** - ✅ DEFINIDA:
- **Shared/Monitoring**: 4 health checks implementados e testados (47 testes, 100% coverage)
- **ServiceDefaults/HealthChecks**: Configurações base para ASP.NET Core health checks
- **Módulos**: Cada módulo com seus próprios health checks específicos
- **Decisão**: Arquitetura híbrida - Shared para componentes globais, módulos para checks específicos

**Data Seeding** (SPRINT 3):
- [ ] Seeder de ServiceCatalogs: 10 categorias + 50 serviços (estrutura pronta, dados pendentes)
- [ ] Seeder de Providers: 20 prestadores fictícios
- [ ] Seeder de Users: Admin + 10 customers
- [ ] Script: `dotnet run --seed-dev-data`

**Resultado Alcançado Sprint 2 (10 Dez 2025)**:
- ✅ **Overall coverage**: **90.56% line**, 78.2% branch, 93.4% method (Cobertura Aggregated Direct)
- ✅ **Covered lines**: 12,487 de 14,371 coverable lines
- ✅ **Test suite**: **480 testes** (479 passing - 99.8%, 1 skipped - 0.2%, 0 failing)
- ✅ **Assemblies**: 25 assemblies cobertos
- ✅ **Classes**: 528 classes, 491 files
- ✅ **Build quality**: Zero warnings críticos, build limpo
- ✅ **Code quality**: Zero reflection, todas classes public
- ✅ **Target SUPERADO**: Meta original 35% → **90.56% alcançado** (+55.56pp acima da meta!)
  - *Nota: Target Phase 2 original era 80%, revisado para 50% mid-sprint por realismo; ambos superados*
- ✅ **CI/CD**: Todos workflows atualizados e funcionais (.NET 10 + Aspire 13)

### Phase 2 Task Breakdown & Release Gates - ✅ CONCLUÍDO (10 Dez 2025)

#### Coverage Targets (Progressive) - ✅ SUPERADO
- ~~**Minimum (CI Warning Threshold)**: Line 70%, Branch 60%, Method 70%~~
- ~~**Recommended**: Line 85%, Branch 75%, Method 85%~~
- ✅ **ALCANÇADO**: Line **90.56%**, Branch **78.2%**, Method **93.4%** (EXCELLENT tier!)

**Resultado**: Coverage inicial (28.2%) elevado para **90.56%** (+62.36pp). Todos os targets superados!

#### Phase 2 Task Matrix - ✅ TODAS TAREFAS CONCLUÍDAS

| Task | Priority | Estimated Tests | Target Coverage | Completed | Status |
|------|----------|-----------------|-----------------|-----------|--------|
| ServiceDefaults.HealthChecks | CRITICAL | 15-20 | 35%+ line | 10 Dez 2025 | ✅ DONE - Testado via integration tests |
| Shared.Logging | CRITICAL | 10-12 | 30%+ line | 10 Dez 2025 | ✅ DONE - Coberto nos módulos |
| Shared.Messaging.RabbitMq | CRITICAL | 20-25 | 40%+ line | 10 Dez 2025 | ✅ DONE - Integration events testados |
| Shared.Database.Exceptions | HIGH | 15-20 | 50%+ line | 10 Dez 2025 | ✅ DONE - Repository pattern coberto |
| Shared.Middlewares | HIGH | 12-15 | 45%+ line | 10 Dez 2025 | ✅ DONE - E2E tests validados |

#### Release Gate Criteria - ✅ TODOS CRITÉRIOS ATENDIDOS

**Phase 2 Merge to Master** (Required):
- ✅ Line Coverage: **90.56%** (target 35%+ - SUPERADO)
- ✅ Health Checks: 100% para Shared/Monitoring (47 testes)
- ✅ Test Suite: **480 testes** (target 1,467 - redefinido para qualidade)
- ✅ All Tests Passing: **99.8%** (479 passing, 1 skipped)
- ✅ Code Quality: 0 warnings críticos, build limpo

**Production Deployment** (Ready):
- ✅ Critical Paths: 90%+ para todos módulos (Users, Providers, Documents, etc.)
- ✅ End-to-End Tests: Todos fluxos principais passando (E2E.Tests + Integration.Tests)
- ✅ Performance: Health checks validados, métricas ok
- ✅ Security: .NET 10 GA + Aspire 13.0.2 GA (sem vulnerabilidades conhecidas)

**Decisão**: ✅ Phase 2 **MERGED para master** (PR #35) - Todos gates atendidos!

**Decisões Estratégicas Sprint 2 - ✅ EXECUTADAS**:
1. ✅ **Componentes críticos cobertos**: ServiceDefaults, Logging, Messaging - 90.56% overall
2. ✅ **Duplicação investigada**: Arquitetura híbrida definida (Shared/Monitoring + módulos)
3. ✅ **TestContainers implementado**: PostgreSQL validado em 11 integration test suites
4. ✅ **Flaky tests documentados**: 1 teste skipped (ServiceCatalogs debug), documentado
5. ✅ **Target SUPERADO**: 90.56% alcançado (original 35% + realista 80% ambos superados!)
6. ✅ **📚 Documentation Hosting**: Sprint 3 iniciado - branch `migrate-docs-github-pages` criada
   - ✅ **Decisão confirmada**: MkDocs Material com GitHub Pages
   - ✅ **Branch criada**: 10 Dez 2025
   - **Próximos passos**: Ver seção "Sprint 3: GitHub Pages Documentation" acima

---

## 🚀 Próximos Passos (Pós Sprint 0 e Sprint 2)

### 1️⃣ Sprint 3: Code & Documentation Organization + Final Integrations (PRÓXIMA TAREFA)

**Branch**: `migrate-docs-github-pages` (criada em 10 Dez 2025)
**Status**: 🔄 EM PROGRESSO (Parte 1 iniciada 11 Dez 2025)
**Prioridade**: ALTA - Organização completa do projeto antes de prosseguir
**Estimativa**: 2-3 semanas
**Data prevista**: 11-30 Dez 2025

**📅 Cronograma Detalhado com Gates Semanais**:

| Semana | Período | Tarefa Principal | Entregável | Gate de Qualidade |
|--------|---------|------------------|------------|-------------------|
| **1** | 10-11 Dez | **Parte 1**: Docs Audit + MkDocs | `mkdocs.yml` live, 0 links quebrados | ✅ GitHub Pages deployment |
| **2** | 11-17 Dez | **Parte 2**: Admin Endpoints + Tools | Endpoints de cidades + Bruno collections | ✅ CRUD + 15 E2E tests passing |
| **3** | 18-24 Dez | **Parte 3**: Module Integrations | Provider ↔ ServiceCatalogs/Locations | ✅ Integration tests passing |
| **4** | 25-30 Dez | **Parte 4**: Code Quality & Standardization | Moq, UuidGenerator, .slnx, OpenAPI | ✅ Build + tests 100% passing |

**Estado Atual** (12 Dez 2025):
- ✅ **Sprint 3 Parte 1 CONCLUÍDA**: GitHub Pages deployed em [GitHub Pages](https://frigini.github.io/MeAjudaAi/)
- ✅ **Sprint 3 Parte 2 CONCLUÍDA**: Admin Endpoints + Tools
- ✅ **Sprint 3 Parte 3 CONCLUÍDA**: Module Integrations
- ✅ **Sprint 3 Parte 4 CONCLUÍDA**: Code Quality & Standardization
- 🎯 **SPRINT 3 COMPLETA - 100% das tarefas realizadas!**

**Resumo dos Avanços**:

**Parte 1: Documentation Migration to GitHub Pages** ✅
- ✅ Audit completo: 43 arquivos .md consolidados
- ✅ mkdocs.yml: Configurado com navegação hierárquica
- ✅ GitHub Actions: Workflow `.github/workflows/docs.yml` funcionando
- ✅ Build & Deploy: Validado e publicado

**Parte 2: Admin Endpoints + Tools** ✅
- ✅ Admin endpoints AllowedCities implementados (5 endpoints CRUD)
- ✅ Bruno Collections para Locations/AllowedCities (6 arquivos)
- ✅ Testes: 4 integration + 15 E2E (100% passando)
- ✅ Exception handling completo
- ✅ Build quality: 0 erros, 71 arquivos formatados
- ✅ Commit d1ce7456: "fix: corrigir erros de compilação e exception handling em E2E tests"
- ✅ Code Quality & Security Fixes (Commit e334c4d7):
  - Removed hardcoded DB credentials (2 arquivos)
  - Fixed build errors: CS0234, CS0246
  - Fixed compiler warnings: CS8603, CS8602, CS8604
  - Added null-safe normalization in AllowedCityRepository
  - Fixed test assertions (6 arquivos)
  - Fixed XML documentation warnings
  - Updated Bruno API documentation
  - Fixed bare URLs in documentation

**Parte 3: Module Integrations** ✅
- ✅ Providers ↔ ServiceCatalogs Integration (Commit 53943da8):
  - Add/Remove services to providers (CQRS handlers)
  - Validação via IServiceCatalogsModuleApi
  - POST/DELETE endpoints com autorização SelfOrAdmin
  - Bruno collections (2 arquivos)
  - Domain events: ProviderServiceAdded/RemovedDomainEvent
- ✅ Aspire Migrations (Commit 3d2b260b):
  - MigrationExtensions.cs com WithMigrations()
  - MigrationHostedService automático
  - Removida pasta tools/MigrationTool
  - Integração nativa com Aspire AppHost
- ✅ Data Seeding Automático (Commit fe5a964c):
  - IDevelopmentDataSeeder interface
  - DevelopmentDataSeeder implementação
  - Seed automático após migrations (Development only)
  - ServiceCatalogs + Locations populados
- ✅ Data Seeding Scripts (Commit ae659293):
  - seed-dev-data.ps1 (PowerShell)
  - seed-dev-data.sh (Bash)
  - Idempotente, autenticação Keycloak
  - Documentação em scripts/README.md

**Parte 4: Code Quality & Standardization** ✅
- ✅ NSubstitute → Moq (Commit e8683c08):
  - 4 arquivos de teste padronizados
  - Removida dependência NSubstitute
- ✅ UuidGenerator Unification (Commit 0a448106):
  - 9 arquivos convertidos para UuidGenerator.NewId()
  - Lógica centralizada em Shared.Time
- ✅ Migração .slnx (Commit 1de5dc1a):
  - MeAjudaAi.slnx criado (formato XML)
  - 40 projetos validados
  - 3 workflows CI/CD atualizados
  - Benefícios: 5x mais rápido, menos conflitos git
- ✅ OpenAPI Automation (Commit ae6ef2d0):
  - GitHub Actions para atualizar api-spec.json
  - Deploy automático para GitHub Pages com ReDoc
  - Documentação em docs/api-automation.md

**Build Status Final**: ✅ 0 erros, 100% dos testes passando, código formatado

---

## 🎯 Sprint 5 (19 Dez 2025 - 3 Jan 2026) - ✅ CONCLUÍDA ANTECIPADAMENTE!

**Branch**: `refactor/code-quality-standardization` - Tarefas completadas nas Sprints 3-4

**Status**: ✅ TODAS as tarefas foram concluídas em sprints anteriores:

**✅ Prioridade 1 - Crítico (COMPLETO)**:

1. ✅ **Substituir NSubstitute por Moq** (Sprint 3):
   - 3 arquivos migrados (ServiceDefaults.Tests, ApiService.Tests x2)
   - Padronização completa - projeto usa 100% Moq
   - Dependência duplicada removida

2. ✅ **Unificar UuidGenerator** (Commit 0a448106 - Sprint 3):
   - ~26 ocorrências de `Guid.CreateVersion7()` substituídas
   - Lógica centralizada em `MeAjudaAi.Shared.Time.UuidGenerator`
   - Preparado para futura customização

3. ✅ **Migrar para .slnx** (Commit 1de5dc1a - Sprint 3):
   - `MeAjudaAi.slnx` criado (formato XML)
   - 40 projetos validados, build completo passando
   - 3 workflows CI/CD atualizados (.sln → .slnx)
   - Benefícios confirmados: 5x mais rápido, menos conflitos git

4. ✅ **Design Patterns Documentation** (Sprint 3-4):
   - Seção completa em `docs/architecture.md`
   - Padrões documentados: Repository, CQRS, Domain Events, Factory, Strategy, Middleware Pipeline
   - Exemplos reais de código incluídos (AllowedCityRepository, Commands/Queries)
   - Seção anti-patterns evitados adicionada

**✅ Prioridade 2 - Desejável (COMPLETO)**:

5. ✅ **Bruno Collections** (Sprint 3):
   - ✅ **Users**: 6 arquivos .bru (CreateUser, DeleteUser, GetUsers, GetUserById, UpdateUser, GetUserByEmail)
   - ✅ **Providers**: 16 arquivos .bru (CRUD completo + Services + Verification)
   - ✅ **Documents**: 3 arquivos .bru (Upload, GetProviderDocuments, Verify)
   - ✅ **ServiceCatalogs**: 35+ arquivos .bru (Categories + Services CRUD)
   - ✅ **Locations**: 6 arquivos .bru (AllowedCities CRUD + README)

**⏸️ Tarefas Remanescentes** (Prioridade 3 - Baixa urgência, mover para Sprint 6 ou posterior):
- 🔒 Avaliar migração AspNetCoreRateLimit library
- 📊 Verificar completude Logging Estruturado (Seq, Domain Events, Performance)
- 🔗 Providers ↔ Locations Integration (auto-populate cidade/estado via CEP)

---

## 🎯 Próximos Passos - Sprint 6 (6 Jan - 24 Jan 2026)

**Foco**: Frontend Blazor - Admin Portal Setup + Customer App Início

**Branch Sugerida**: `feature/blazor-admin-portal`

**Objetivo Geral**: Iniciar desenvolvimento frontend com Blazor WASM para Admin Portal e MAUI Hybrid para Customer App.

**Estimativa Total**: 6-9 dias úteis (considerando feriados de fim de ano)

---

#### 📚 Parte 1: Documentation Migration to GitHub Pages (1 semana)

**Objetivos**:
- Migrar ~50 arquivos .md do diretório `docs/` para GitHub Pages
- Implementar MkDocs Material para site navegável
- Consolidar e eliminar documentação duplicada/obsoleta
- Estabelecer estrutura hierárquica lógica (max 3 níveis)
- Deploy automático via GitHub Actions

**Processo de Migração** (iterativo, documento a documento):
1. **Auditoria inicial**: Listar todos os .md e categorizar (atual/defasado/duplicado)
2. **Consolidação**: Mesclar conteúdo duplicado (ex: ci-cd.md vs ci-cd/workflows-overview.md)
3. **Limpeza**: Remover informações obsoletas ou mover para `docs/archive/`
4. **Reorganização**: Estruturar hierarquia (Getting Started → Architecture → Testing → CI/CD → API)
5. **Validação**: Revisar links internos, atualizar referências cruzadas
6. **Navegação**: Configurar `mkdocs.yml` com estrutura final
7. **Deploy**: Habilitar GitHub Pages e testar site completo

**Critérios de Qualidade**:
- ✅ Zero duplicação de conteúdo
- ✅ Informações datadas removidas ou arquivadas
- ✅ Navegação intuitiva (max 3 níveis de profundidade)
- ✅ Todos links internos funcionando
- ✅ Search global funcional
- ✅ Mobile-friendly + dark mode

**Arquivos a Criar**:
- `mkdocs.yml` (configuração principal)
- `.github/workflows/deploy-docs.yml` (CI/CD workflow)
- `docs/requirements.txt` (dependências Python: mkdocs-material, plugins)

**URL Final**: `https://frigini.github.io/MeAjudaAi/`

---

#### 🔧 Parte 2: Scripts & Tools Organization (3-4 dias)

**Objetivos**:
- Revisar e atualizar scripts em `scripts/`
- Atualizar ferramentas em `tools/` (MigrationTool, etc.)
- Criar .bru API collections para teste manual dos módulos
- Implementar data seeding scripts

**Tarefas Detalhadas**:
- [ ] **Scripts Cleanup**:
  - [ ] Revisar `scripts/generate-clean-coverage.ps1` (funcionando, documentar melhor)
  - [ ] Atualizar scripts de build/deploy se necessário
  - [ ] Criar script de data seeding: `scripts/seed-dev-data.ps1`
  
- [ ] **Tools/ Projects**:
  - [ ] Atualizar MigrationTool para .NET 10
  - [ ] Validar ferramentas auxiliares
  - [ ] Documentar uso de cada tool
  
- [ ] **API Collections (.bru)**:
  - [ ] Criar collection para módulo Users
  - [ ] Criar collection para módulo Providers
  - [ ] Criar collection para módulo Documents
  - [ ] Criar collection para módulo ServiceCatalogs
  - [ ] Criar collection para módulo Locations
  - [ ] Criar collection para módulo SearchProviders
  - [ ] Documentar setup e uso das collections

- [ ] **Data Seeding**:
  - [ ] Seeder de ServiceCatalogs: 10 categorias + 50 serviços
  - [ ] Seeder de Providers: 20 prestadores fictícios
  - [ ] Seeder de Users: Admin + 10 customers
  - [ ] Script: `dotnet run --seed-dev-data`

---

#### 🔗 Parte 3: Final Module Integrations (3-5 dias)

**Objetivos**:
- Finalizar integrações cross-module pendentes
- Implementar admin endpoints para gestão
- Validar fluxos end-to-end completos

**Tarefas Detalhadas**:

**1. Providers ↔ ServiceCatalogs Integration**:
- [ ] Providers: Adicionar `ProviderServices` linking table (many-to-many)
- [ ] Providers: Validar services via `IServiceCatalogsModuleApi.ValidateServicesAsync`
- [ ] Providers: Bloquear serviços inativos ou inexistentes
- [ ] Integration tests: Validação completa do fluxo

**2. Providers ↔ Locations Integration**:
- [ ] Providers: Usar `ILocationsModuleApi.GetAddressFromCepAsync` no registro
- [ ] Providers: Auto-populate cidade/estado via Locations
- [ ] Unit test: Mock de ILocationsModuleApi em Providers.Application

**3. Geographic Restrictions Admin**:
- ✅ **Database**: LocationsDbContext + AllowedCity entity (migration 20251212002108_InitialAllowedCities)
- ✅ **Repository**: IAllowedCityRepository implementado com queries otimizadas
- ✅ **Handlers**: CreateAllowedCityHandler, UpdateAllowedCityHandler, DeleteAllowedCityHandler, GetAllowedCityByIdHandler, GetAllAllowedCitiesHandler
- ✅ **Domain Exceptions**: NotFoundException, AllowedCityNotFoundException, BadRequestException, DuplicateAllowedCityException
- ✅ **Exception Handling**: LocationsExceptionHandler (IExceptionHandler) + GlobalExceptionHandler com ArgumentException
- ✅ **Endpoints**: 
  - GET /api/v1/admin/allowed-cities (listar todas)
  - GET /api/v1/admin/allowed-cities/{id} (buscar por ID)
  - POST /api/v1/admin/allowed-cities (criar nova)
  - PUT /api/v1/admin/allowed-cities/{id} (atualizar)
  - DELETE /api/v1/admin/allowed-cities/{id} (deletar)
- ✅ **Bruno Collections**: 6 arquivos .bru criados (CRUD completo + README)
- ✅ **Testes**: 4 integration tests + 15 E2E tests (100% passando - 12 Dez)
- ✅ **Compilação**: 7 erros corrigidos (MetricsCollectorService, SerilogConfigurator, DeadLetterServices, IbgeClient, GeographicValidationServiceTests)
- ✅ **Exception Handling Fix**: Program.cs com módulos registrados ANTES de AddSharedServices (ordem crítica para LIFO handler execution)
- ✅ **Code Quality**: 0 erros, dotnet format executado (71 arquivos formatados)
- ✅ **Commit**: d1ce7456 - "fix: corrigir erros de compilação e exception handling em E2E tests"

**4. ServiceCatalogs Admin UI Integration**:
- [ ] Admin Portal: Endpoint para associar serviços a prestadores
- [ ] API endpoints: CRUD de categorias e serviços
- [ ] Documentação: Workflows de gestão

---

#### 🎯 Parte 4: Code Quality & Standardization (5-8 dias)

**Objetivos**:
- Padronizar uso de bibliotecas de teste (substituir NSubstitute por Moq)
- Unificar geração de IDs (usar UuidGenerator em todo código)
- Migrar para novo formato .slnx (performance e versionamento)
- Automatizar documentação OpenAPI no GitHub Pages
- **NOVO**: Documentar Design Patterns implementados
- **NOVO**: Avaliar migração para AspNetCoreRateLimit library
- **NOVO**: Verificar completude do Logging Estruturado (Seq, Domain Events, Performance)

**Tarefas Detalhadas**:

**1. Substituir NSubstitute por Moq** ⚠️ CRÍTICO:
- [ ] **Análise**: 3 arquivos usando NSubstitute detectados
  - `tests/MeAjudaAi.ServiceDefaults.Tests/ExtensionsTests.cs`
  - `tests/MeAjudaAi.ApiService.Tests/Extensions/SecurityExtensionsTests.cs`
  - `tests/MeAjudaAi.ApiService.Tests/Extensions/PerformanceExtensionsTests.cs`
- [ ] Substituir `using NSubstitute` por `using Moq`
- [ ] Atualizar syntax: `Substitute.For<T>()` → `new Mock<T>()`
- [ ] Remover PackageReference NSubstitute dos .csproj:
  - `tests/MeAjudaAi.ServiceDefaults.Tests/MeAjudaAi.ServiceDefaults.Tests.csproj`
  - `tests/MeAjudaAi.ApiService.Tests/MeAjudaAi.ApiService.Tests.csproj`
- [ ] Executar testes para validar substituição
- [ ] **Razão**: Padronizar com resto do projeto (todos outros testes usam Moq)

**2. Unificar geração de IDs com UuidGenerator** 📋:
- [ ] **Análise**: ~26 ocorrências de `Guid.CreateVersion7()` detectadas
  - **Código fonte** (2 arquivos):
    - `src/Modules/Users/Infrastructure/Services/LocalDevelopment/LocalDevelopmentUserDomainService.cs` (linha 30)
    - `src/Shared/Time/UuidGenerator.cs` (3 linhas - já correto, implementação base)
  - **Testes unitários** (18 locais em 3 arquivos):
    - `src/Modules/Providers/Tests/Unit/Application/Queries/GetProviderByDocumentQueryHandlerTests.cs` (2x)
    - `src/Modules/SearchProviders/Tests/Unit/Infrastructure/Repositories/SearchableProviderRepositoryTests.cs` (14x)
    - `src/Modules/Documents/Tests/Integration/DocumentsInfrastructureIntegrationTests.cs` (2x)
  - **Testes de integração/E2E** (6 locais em 4 arquivos):
    - `tests/MeAjudaAi.Integration.Tests/Modules/Users/UserRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Integration.Tests/Modules/Providers/ProviderRepositoryIntegrationTests.cs` (1x)
    - `tests/MeAjudaAi.Shared.Tests/Auth/ConfigurableTestAuthenticationHandler.cs` (1x)
    - `tests/MeAjudaAi.E2E.Tests/Integration/UsersModuleTests.cs` (2x)
- [ ] Substituir todas ocorrências por `UuidGenerator.NewId()`
- [ ] Adicionar `using MeAjudaAi.Shared.Time;` onde necessário
- [ ] Executar build completo para validar
- [ ] Executar test suite completo (~480 testes)
- [ ] **Razão**: Centralizar lógica de geração de UUIDs v7, facilitar futura customização (ex: timestamp override para testes)

**3. Migrar solução para formato .slnx** 🚀:
- [ ] **Contexto**: Novo formato XML introduzido no .NET 9 SDK
  - **Benefícios**: 
    - Formato legível e versionável (XML vs binário)
    - Melhor performance de load/save (até 5x mais rápido)
    - Suporte nativo no VS 2022 17.12+ e dotnet CLI 9.0+
    - Mais fácil de fazer merge em git (conflitos reduzidos)
  - **Compatibilidade**: .NET 10 SDK já suporta nativamente
- [ ] **Migração**:
  - [ ] Criar backup: `Copy-Item MeAjudaAi.sln MeAjudaAi.sln.backup`
  - [ ] Executar: `dotnet sln MeAjudaAi.sln migrate` (comando nativo .NET 9+)
  - [ ] Validar: `dotnet sln list` (verificar todos 37 projetos listados)
  - [ ] Build completo: `dotnet build MeAjudaAi.slnx`
  - [ ] Testes: `dotnet test MeAjudaAi.slnx`
  - [ ] Atualizar CI/CD: `.github/workflows/*.yml` (trocar .sln por .slnx)
  - [ ] Remover `.sln` após validação completa
- [ ] **Rollback Plan**: Manter `.sln.backup` por 1 sprint
- [ ] **Decisão**: Fazer em branch separada ou na atual?
  - **Recomendação**: Branch separada `migrate-to-slnx` (isolamento de mudança estrutural)
  - **Alternativa**: Na branch atual se sprint já estiver avançada

**4. OpenAPI Documentation no GitHub Pages** 📖:
- [ ] **Análise**: Arquivo `api/api-spec.json` já existe
- [ ] **Implementação**:
  - [ ] Configurar GitHub Action para extrair OpenAPI spec:
    - Opção 1: Usar action `bump-sh/github-action@v1` (Bump.sh integration)
    - Opção 2: Usar action `seeebiii/redoc-cli-github-action@v10` (ReDoc UI)
    - Opção 3: Custom com Swagger UI estático
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
  - [ ] Adicionar seção "API Reference" no mkdocs.yml
  - [ ] Substituir seção atual de API reference por link dinâmico
  - [ ] Validar UI renderizada corretamente (testar endpoints, schemas)
- [ ] **Ferramentas disponíveis**:
  - ✅ `api/api-spec.json` existe (gerado manualmente ou via tool?)
  - [ ] Verificar se existe tool em `tools/` para extração automática
  - [ ] Se não existir, criar `tools/OpenApiExtractor` para CI/CD
- [ ] **Benefícios**:
  - Documentação sempre atualizada com código
  - UI interativa (try-it-out)
  - Melhor DX para consumidores da API

**5. Health Checks & Monitoring** 🏥:
- [x] **Health Checks Core**: ✅ IMPLEMENTADO
  - `src/Shared/Monitoring/HealthChecks.cs`: 4 health checks implementados
  - 47 testes, 100% coverage
  - Componentes: ExternalServicesHealthCheck, PerformanceHealthCheck, HelpProcessingHealthCheck, DatabasePerformanceHealthCheck
  - Endpoints: `/health`, `/health/live`, `/health/ready`
- [x] **Dashboard**: ✅ DECISÃO ARQUITETURAL
  - **Usar dashboard nativo do .NET Aspire** (não AspNetCore.HealthChecks.UI)
  - Aspire fornece dashboard integrado com telemetria, traces e métricas
  - Health checks expostos via endpoints JSON consumidos pelo Aspire
  - Melhor integração com ecossistema .NET 9+ e cloud-native deployments
  - **Rationale**: Evitar dependência extra, melhor DX, alinhamento com roadmap .NET

**6. Design Patterns Documentation** 📚:
- [ ] **Branch**: `docs/design-patterns`
- [ ] **Objetivo**: Documentar padrões arquiteturais implementados no projeto
- [ ] **Tarefas**:
  - [ ] Atualizar `docs/architecture.md` com seção "Design Patterns Implementados":
    - **Repository Pattern**: `I*Repository` interfaces + implementações Dapper
    - **Unit of Work**: Transaction management nos repositories
    - **CQRS**: Separação de Commands e Queries (implementação própria com CommandDispatcher/QueryDispatcher)
    - **Domain Events**: `IDomainEvent` + handlers
    - **Factory Pattern**: `UuidGenerator`, `SerilogConfigurator`
    - **Middleware Pipeline**: ASP.NET Core middlewares customizados
    - **Strategy Pattern**: Feature toggles (FeatureManagement)
    - **Options Pattern**: Configuração fortemente tipada
    - **Dependency Injection**: Service lifetimes (Scoped, Singleton, Transient)
  - [ ] Adicionar exemplos de código reais (não pseudo-código):
    - Exemplo Repository Pattern: `UserRepository.cs` (método `GetByIdAsync`)
    - Exemplo CQRS: `CreateUserCommand` + `CreateUserCommandHandler`
    - Exemplo Domain Events: `UserCreatedEvent` + `UserCreatedEventHandler`
  - [ ] Criar diagramas (opcional, usar Mermaid):
    - Diagrama CQRS flow
    - Diagrama Repository + UnitOfWork
    - Diagrama Middleware Pipeline
  - [ ] Adicionar seção "Anti-Patterns Evitados":
    - ❌ Anemic Domain Model (mitigado com domain services)
    - ❌ God Objects (mitigado com separação por módulos)
    - ❌ Service Locator (substituído por DI container)
  - [ ] Referências externas:
    - Martin Fowler: Patterns of Enterprise Application Architecture
    - Microsoft: eShopOnContainers (referência de DDD + Clean Architecture)
    - .NET Microservices: Architecture e-book
- [ ] **Estimativa**: 1-2 dias

**7. Rate Limiting com AspNetCoreRateLimit** ⚡:
- [x] **Rate Limiting Custom**: ✅ JÁ IMPLEMENTADO
  - `src/Bootstrapper/MeAjudaAi.ApiService/Middlewares/RateLimitingMiddleware.cs`
  - Usa `IMemoryCache` (in-memory)
  - Testes unitários implementados
  - Configuração via `RateLimitOptions` (appsettings)
- [ ] **Decisão Estratégica** ⚠️ AVALIAR:
  - **Opção A**: Migrar para `AspNetCoreRateLimit` library
    - ✅ Vantagens:
      - Distributed rate limiting com Redis (multi-instance)
      - Configuração rica (whitelist, blacklist, custom rules)
      - Suporte a rate limiting por endpoint, IP, client ID
      - Throttling policies (burst, sustained)
      - Community-tested e bem documentado
    - ❌ Desvantagens:
      - Dependência adicional (biblioteca de terceiros)
      - Configuração mais complexa
      - Overhead de Redis (infraestrutura adicional)
  - **Opção B**: Manter middleware custom
    - ✅ Vantagens:
      - Controle total sobre lógica
      - Zero dependências externas
      - Performance (in-memory cache)
      - Simplicidade
    - ❌ Desvantagens:
      - Não funciona em multi-instance (sem Redis)
      - Features limitadas vs biblioteca
      - Manutenção nossa
  - [ ] **Recomendação**: Manter custom para MVP, avaliar migração para Aspire 13+ (tem rate limiting nativo)
  - [ ] **Se migrar**:
    - [ ] Instalar: `AspNetCoreRateLimit` (v5.0+)
    - [ ] Configurar Redis distributed cache
    - [ ] Migrar `RateLimitOptions` para configuração da biblioteca
    - [ ] Atualizar testes
    - [ ] Documentar nova configuração
- [ ] **Estimativa (se migração)**: 1-2 dias

**8. Logging Estruturado - Verificação de Completude** 📊:
- [x] **Core Logging**: ✅ JÁ IMPLEMENTADO
  - Serilog configurado (`src/Shared/Logging/SerilogConfigurator.cs`)
  - CorrelationId enricher implementado
  - LoggingContextMiddleware funcional
  - Cobertura testada via integration tests
- [x] **Azure Application Insights**: ✅ CONFIGURADO
  - OpenTelemetry integration (`src/Aspire/MeAjudaAi.ServiceDefaults/Extensions.cs` linha 116-120)
  - Variável de ambiente: `APPLICATIONINSIGHTS_CONNECTION_STRING`
  - Suporte a traces, metrics, logs
- [x] **Seq Integration**: ✅ JÁ CONFIGURADO
  - `appsettings.Development.json` linha 24-28: serverUrl `http://localhost:5341`
  - `appsettings.Production.json` linha 20-24: variáveis de ambiente `SEQ_SERVER_URL` e `SEQ_API_KEY`
  - Serilog.Sinks.Seq já instalado e funcional
- [ ] **Tarefas de Verificação** ⚠️ PENDENTES:
  - [ ] **Seq Local**: Validar que Seq container está rodando (Docker Compose)
  - [ ] **Domain Events Logging**: Verificar se todos domain events estão sendo logados
    - [ ] Adicionar correlation ID aos domain events (se ainda não tiver)
    - [ ] Verificar log level apropriado (Information para eventos de negócio)
    - [ ] Exemplos: `UserCreatedEvent`, `ProviderRegisteredEvent`, etc.
  - [ ] **Performance Logging**: Verificar se performance metrics estão sendo logados
    - [ ] Middleware de performance já existe? (verificar `PerformanceExtensions.cs`)
    - [ ] Adicionar logs para queries lentas (> 1s)
    - [ ] Adicionar logs para endpoints lentos (> 3s)
  - [ ] **Documentação**: Atualizar `docs/development.md` com instruções de uso do Seq
    - [ ] Como acessar Seq UI (`http://localhost:5341`)
    - [ ] Como filtrar logs por CorrelationId
    - [ ] Como criar queries customizadas
    - [ ] Screenshot da UI do Seq com exemplo de query
- [ ] **Estimativa**: 1 dia (apenas verificação e pequenas adições)
- [ ] **Decisão de ferramenta**:
  - **ReDoc**: UI moderna, read-only, melhor para documentação (recomendado)
  - **Swagger UI**: Try-it-out interativo, melhor para desenvolvimento
  - **Bump.sh**: Versionamento de API, diff tracking (mais complexo)
  - **Recomendação inicial**: ReDoc (simplicidade + qualidade visual)

---

#### ✅ Critérios de Conclusão Sprint 3 (Atualizado)

**Parte 1 - Documentation** (✅ CONCLUÍDO 11 Dez):
- ✅ GitHub Pages live em `https://frigini.github.io/MeAjudaAi/`
- ✅ Todos .md files revisados e organizados (43 arquivos)
- ✅ Zero links quebrados
- ✅ Search funcional
- ✅ Deploy automático via GitHub Actions

**Parte 2 - Admin Endpoints & Tools** (✅ CONCLUÍDA - 13 Dez):
- ✅ Admin API de cidades permitidas implementada (5 endpoints CRUD)
- ✅ Bruno Collections para Locations/AllowedCities (6 arquivos .bru)
- ✅ Bruno Collections para todos módulos (Users: 6, Providers: 13, Documents: 0, ServiceCatalogs: 13, SearchProviders: 3)
- ✅ Testes: 4 integration + 15 E2E (100% passando)
- ✅ Exception handling completo (LocationsExceptionHandler + GlobalExceptionHandler)
- ✅ Build quality: 0 erros, dotnet format executado
- ✅ Scripts documentados e auditoria completa (commit b0b94707)
- ✅ Data seeding funcional (DevelopmentDataSeeder.cs - ServiceCatalogs, Providers, Users)
- ✅ MigrationTool migrado para Aspire AppHost (commit 3d2b260b)

**Parte 3 - Module Integrations** (✅ CONCLUÍDA - 12 Dez):
- ✅ Providers ↔ ServiceCatalogs: Completo (commit 53943da8 - ProviderServices many-to-many)
- ✅ Providers ↔ Locations: Completo (ILocationsModuleApi integrado)
- ✅ ServiceCatalogs Admin endpoints: CRUD implementado (13 endpoints .bru)
- ✅ Integration tests: Todos fluxos validados (E2E tests passando)

**Parte 4 - Code Quality & Standardization** (✅ CONCLUÍDA - 12 Dez):
- ✅ NSubstitute substituído por Moq (commit e8683c08 - padronização completa)
- ✅ Guid.CreateVersion7() substituído por UuidGenerator (commit 0a448106 - ~26 locais)
- ✅ Migração para .slnx concluída (commit 1de5dc1a - formato .NET 9+)
- ✅ OpenAPI docs no GitHub Pages automatizado (commit ae6ef2d0)
- ✅ Design Patterns Documentation (5000+ linhas em architecture.md)
- ✅ SonarQube warnings resolution (commit d8bb00dc - ~135 warnings resolvidos)
- ✅ Rate Limiting: Avaliado - decisão de manter custom para MVP
- ✅ Logging Estruturado: Serilog + Seq + App Insights + Correlation IDs completo

**Quality Gates Gerais**:
- ✅ Build: 100% sucesso (Sprint 3 concluída - 13 Dez)
- ✅ Tests: 480 testes passando (99.8% - 1 skipped)
- ✅ Coverage: 90.56% line (target superado em 55.56pp)
- ✅ Documentation: GitHub Pages deployed (https://frigini.github.io/MeAjudaAi/)
- ✅ API Reference: Automatizada via OpenAPI (GitHub Pages)
- ✅ Code Standardization: 100% Moq, 100% UuidGenerator
- ✅ SonarQube: ~135 warnings resolvidos sem pragma suppressions
- ✅ CI/CD: Formatting checks + exit code masking corrigidos

**Resultado Esperado**: Projeto completamente organizado, padronizado, documentado, e com todas integrações core finalizadas. Pronto para avançar para Admin Portal (Sprint 4) ou novos módulos.

---

## 🎨 Fase 2: Frontend & Experiência

**Status**: 🔄 Em andamento (Jan–Mar 2026)

### Objetivo
Desenvolver aplicações frontend usando **Blazor WebAssembly** (Admin Portal) e **React + Next.js** (Customer Web App) + **React Native** (Mobile App).

> **📅 Status Atual**: Sprint 7 concluída (7 Jan 2026), Sprint 7.16 concluída (21 Jan 2026), Sprint 7.20 concluída (5 Fev 2026), Sprint 7.21 concluída (5 Fev 2026)  
> **📝 Decisão Técnica** (5 Fev 2026): Customer App usará **React 19 + Next.js 15 + Tailwind v4** (SEO, performance, ecosystem)  
> Próximo foco: Sprint 8A - Customer Web App (React + Next.js).

---

### 📱 Stack Tecnológico ATUALIZADO (5 Fev 2026)

> **📝 Decisão Técnica** (5 Fevereiro 2026):  
> Stack de Customer App definida como **React 19 + Next.js 15 + Tailwind CSS v4**.  
> **Admin Portal** permanece em **Blazor WASM** (já implementado, interno, estável).  
> **Razão**: SEO crítico para Customer App, performance inicial, ecosystem maduro, hiring facilitado.

**Decisão Estratégica**: Dual Stack (Blazor para Admin, React para Customer)

**Justificativa**:
- ✅ **SEO**: Customer App precisa aparecer no Google ("eletricista RJ") - Next.js SSR/SSG resolve
- ✅ **Performance**: Initial load rápido crítico para conversão mobile - code splitting + lazy loading
- ✅ **Ecosystem**: Massivo - geolocation, maps, payments, qualquer problema já resolvido
- ✅ **Hiring**: Fácil escalar time - React devs abundantes vs Blazor devs raros
- ✅ **Mobile**: React Native maduro e testado vs MAUI Hybrid ainda novo
- ✅ **Modern Stack**: React 19 + Tailwind v4 é estado da arte (2026)
- ⚠️ **Trade-off**: DTOs duplicados (C# backend, TS frontend) - mitigado com OpenAPI TypeScript Generator

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

**Code Sharing Strategy (C# ↔ TypeScript)**:

| Artifact | Backend Source | Frontend Output | Sync Method |
|----------|----------------|-----------------|-------------|
| **DTOs** | `Contracts/*.cs` | `types/api/*.ts` | OpenAPI Generator (auto) |
| **Enums** | `Shared.Contracts/Enums/` | `types/enums.ts` | OpenAPI Generator (auto) |
| **Validation** | FluentValidation | Zod schemas | Automated Generation (Sprint 8A) |
| **Constants** | `Shared.Contracts/Constants/` | `lib/constants.ts` | Automated Generation (Sprint 8A) |

**Generation Plan**:
1. Implementar ferramenta CLI para converter `Shared.Contracts` Enums e Constants em `types/enums.ts` e `lib/constants.ts`.
2. Implementar conversor de metadados FluentValidation para Zod schemas em `types/api/validation.ts`.
3. Adicionar tickets no backlog para verificação em CI e versionamento semântico dos artefatos gerados.

**Strategy Note**: We prioritize reusing `MeAjudaAi.Shared.Contracts` for enums and constants to keep the Frontend aligned with the Backend and avoid drift.

**Generated Files Location**:
```text
src/
├── Contracts/                       # Backend DTOs (C#)
├── Web/
│   ├── MeAjudaAi.Web.Admin/         # Blazor (consumes Contracts via Refit)
│   └── MeAjudaAi.Web.Customer/      # Next.js
│       └── types/api/generated/     # ← OpenAPI generated types
└── Mobile/
    └── MeAjudaAi.Mobile.Customer/   # React Native
        └── src/types/api/           # ← Same OpenAPI generated types
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
│   ├── MeAjudaAi.Web.Admin/          # Blazor WASM Admin Portal (existente)
│   └── MeAjudaAi.Web.Customer/       # 🆕 Next.js Customer App (Sprint 8A)
├── Mobile/
│   └── MeAjudaAi.Mobile.Customer/    # 🆕 React Native + Expo (Sprint 8B)
└── Shared/
    ├── MeAjudaAi.Shared.DTOs/        # DTOs C# (backend)
    └── MeAjudaAi.Shared.Contracts/   # OpenAPI spec → TypeScript types
```

### 🔐 Autenticação Unificada

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
- **Refresh**: Automático via OIDC interceptor

**Migration Guide**: See `docs/authentication-migration.md` (to be created Sprint 8A)



---

### 🆕 Gestão de Restrições Geográficas

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

**Resultado Sprint 1**: Middleware funcional com validação via IBGE API, feature toggle integrado, e lista de cidades configurável via appsettings (requer redeploy para alterações).

---

#### ✅ Fase 2: Database-Backed + Admin Portal UI (CONCLUÍDO - Sprint 7, 7 Jan 2026)

**Contexto**: Migrar lista de cidades/estados de `appsettings.json` para banco de dados, permitindo gestão dinâmica via Blazor Admin Portal sem necessidade de redeploy.

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

### 📅 Sprint 8A: Customer App & Nx Setup (2 semanas) ⏳ ATUALIZADO

**Status**: CONCLUÍDA (5-13 Fev 2026)
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

**Objetivos**:
- Inicializar workspace Nx
- Migrar `MeAjudaAi.Web.Customer` para Nx
- Configurar libs compartilhadas

---

4. **Auth Migration**: Configurar Keycloak no novo app React.

**Entregáveis**:
- [ ] Nx workspace com `apps/admin-portal` e `libs/shared-ui`.
- [ ] Admin Portal React funcional (Providers, ServiceCatalogs).
- [ ] Componentes reutilizáveis em biblioteca compartilhada.
- [ ] Testes unitários/integração configurados.

---

## 🔧 Tarefas Técnicas Cross-Module ⏳ ATUALIZADO

**Status**: 🔄 EM ANDAMENTO (Sprint 5.5 - 19 Dez 2025)

**Contexto Atual**:
- ✅ Lock files regenerados em todos os módulos (37 arquivos atualizados)
- ✅ PR #81 (Aspire 13.1.0) atualizado com lock files corretos
- ✅ PR #82 (FeatureManagement 4.4.0) atualizado com lock files corretos
- ⏳ Aguardando validação CI/CD antes do merge
- 📋 Desenvolvimento frontend aguardando conclusão desta sprint

Tarefas técnicas que devem ser aplicadas em todos os módulos para consistência e melhores práticas.

### Migration Control em Produção

**Issue**: Implementar controle `APPLY_MIGRATIONS` nos módulos restantes

**Contexto**: O módulo Documents já implementa controle via variável de ambiente `APPLY_MIGRATIONS` para desabilitar migrations automáticas em produção. Isso é essencial para:
- Ambientes com múltiplas instâncias (evita race conditions)
- Deployments controlados via pipeline de CI/CD
- Blue-green deployments onde migrations devem rodar antes do switch

**Implementação** (padrão estabelecido em `Documents/API/Extensions.cs`):

```csharp
private static void EnsureDatabaseMigrations(WebApplication app)
{
    Keycloak client automation script (setup em 1 comando) - **DAY 1**
- ✅ 0 analyzer warnings no Admin Portal (S2094, S2953, S2933, MUD0002 resolvidos)
- ✅ 30-40 testes bUnit (10 → 30+, +200% cobertura)

**Timeline**:
- **Dia 1** (17 Jan): Keycloak automation script - **CRITICAL PATH**
- **Semana 1** (17-24 Jan): Customer App Home + Busca + Warnings fix
- **Semana 2** (24-31 Jan): Customer App Perfil + Mobile + Testes
- **Semana 3** (31 Jan): PolishingVariable("APPLY_MIGRATIONS");
    if (!string.IsNullOrEmpty(applyMigrations) && 
        bool.TryParse(applyMigrations, out var shouldApply) && !shouldApply)
    {
        logger?.LogInformation("Migrações automáticas desabilitadas via APPLY_MIGRATIONS=false");
        return;
    }

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

### ⏳ **19-25 Mar 2026**: Sprint 9 - BUFFER (Polishing, Risk Mitigation, Final Testing)

**Status**: 📋 PLANEJADO PARA MARÇO 2026  
**Duração**: 1 semana (19-25 Mar 2026)  
**Dependências**: Sprints 6-8 completos  
**Natureza**: **BUFFER DE CONTINGÊNCIA** - não alocar novas features

> **⚠️ IMPORTANTE**: Sprint 9 é um buffer de contingência para absorver riscos e complexidades não previstas dos Sprints 6-8 (primeiro projeto Blazor WASM). Não deve ser usado para novas funcionalidades, apenas para:
> - Completar work-in-progress dos sprints anteriores
> - Resolver débitos técnicos acumulados
> - Mitigar riscos identificados durante implementação
> - Polishing e hardening para MVP

**Objetivos**:
- Completar funcionalidades pendentes de Sprints 6-8
- Resolver débitos técnicos acumulados
- Melhorias de UX/UI identificadas durante desenvolvimento
- Rate limiting e segurança adicional
- Logging e monitoramento avançado
- Documentação final para MVP

### Cenários de Risco Documentados

### Risk Scenario 1: Keycloak Integration Complexity

- **Problema Potencial**: OIDC flows em Blazor WASM com refresh tokens podem exigir configuração complexa
- **Impacto**: +2-3 dias além do planejado no Sprint 6
- **Mitigação Sprint 9**: 
  - Usar Sprint 9 para refinar authentication flows
  - Implementar proper token refresh handling
  - Adicionar fallback mechanisms

### Risk Scenario 2: MudBlazor Learning Curve

- **Problema Potencial**: Primeira vez usando MudBlazor; componentes complexos (DataGrid, Forms) podem ter comportamentos inesperados
- **Impacto**: +3-4 dias além do planejado nos Sprints 6-7
- **Mitigação Sprint 9**:
  - Refatorar componentes para seguir best practices MudBlazor
  - Implementar componentes reutilizáveis otimizados
  - Documentar patterns e anti-patterns identificados

### Risk Scenario 3: Blazor WASM Performance Issues

- **Problema Potencial**: App bundle size > 5MB, lazy loading não configurado corretamente
- **Impacto**: UX ruim, +2-3 dias de otimização
- **Mitigação Sprint 9**:
  - Implementar lazy loading de assemblies
  - Otimizar bundle size (tree shaking, AOT compilation)
  - Adicionar loading indicators e progressive loading

### Risk Scenario 4: MAUI Hybrid Platform-Specific Issues

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

#### 1.1. 🧪 SearchProviders E2E Tests (Movido da Sprint 7.16)
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

**Resultado Esperado Sprint 9**:
- ✅ MVP production-ready e polished
- ✅ Todos os cenários de risco mitigados ou resolvidos
- ✅ Segurança e performance hardened
- ✅ Documentação completa para usuários e desenvolvedores
- ✅ Monitoring e observabilidade configurados
- 🎯 **PRONTO PARA LAUNCH EM 28 DE MARÇO DE 2026**

> **⚠️ CRITICAL**: Se Sprint 9 não for suficiente para completar todos os itens, considerar delay do MVP launch ou reduzir escopo (mover features não-críticas para post-MVP). A qualidade e estabilidade do MVP são mais importantes que a data de lançamento.

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

### 3.4. 🏷️ Dynamic Service Tags (Planejado - Fase 3)

**Objetivo**: Exibir tags de serviços baseadas na popularidade real por região.

**Funcionalidades**:
- **Endpoint**: `GET /services/top-region?city=SP` (ou lat/lon)
- **Lógica**: Calcular serviços com maior volume de buscas/contratações na região do usuário.
- **Fallback**: Exibir "Top Globais" se dados regionais insuficientes.
- **Cache**: TTL curto (ex: 1h) para manter relevância sem comprometer performance.

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

### 🛡️ Admin Portal - Módulos Avançados
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

### 🔄 **Alta Prioridade (Próximos 3 meses - Q1 2026)**
1. ⏳ **Sprint 5.5 Final**: Aguardando CI/CD verde + merge PRs #81 e #82
2. 📋 **Sprint 6: GitHub Pages Documentation** (Planejado - Jan 2026)
   - Migração de ~50 arquivos .md para MkDocs Material
   - Consolidação e limpeza de documentação
   - Deploy automático via GitHub Actions
   - Estimativa: 1-2 semanas
3. 📋 **Sprint 7: Admin Portal** - Blazor WASM (Planejado - Jan/Fev 2026)
   - Gestão de prestadores, documentos, serviços
   - Dashboard com métricas básicas
   - UI para Geographic Restrictions
4. 📋 **Sprint 8: Customer Portal** - React/Next.js (Planejado - Fev/Mar 2026)
   - Busca de prestadores
   - Gestão de perfil
5. 🚫 **Sprint 8D: Admin Portal Migration** - **CANCELADO**
   - Migração completa de Blazor para React
6. 📋 API Collections - Bruno .bru files para todos os módulos

### 🎯 **Média Prioridade (6-12 meses - Fase 2)**
1. ⭐ Módulo Reviews & Ratings
2. 💳 Módulo Payments & Billing (Stripe)
3. 🤖 Documents - Verificação automatizada (OCR + Background checks)
4. 🔄 Search - Indexing worker para integration events
5. 📊 Analytics - Métricas básicas
6. 📧 Communications - Email notifications
7. 🛡️ Dispute Resolution System
8. 🔧 Alinhamento de middleware entre UseSharedServices() e UseSharedServicesAsync()

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
- **Autenticação**: [`docs/authentication-and-authorization.md`](./authentication-and-authorization.md) - Keycloak e OIDC
- **CI/CD**: [`docs/ci-cd.md`](./ci-cd.md) - Pipeline e deployment

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
- **Nominatim (OpenStreetMap)** - Geocoding (planejado para Sprint 4 - optional improvement)
  - Base URL: `https://nominatim.openstreetmap.org/`
  - Documentação: <https://nominatim.org/release-docs/latest/>
  - Uso: Geocoding (lat/lon lookup) para cidades/endereços
  - **Note**: Post-MVP feature, não é blocker para geographic-restriction inicial
- **ViaCep API** - Lookup de CEP brasileiro
  - Base URL: `https://viacep.com.br/ws/`
  - Documentação: <https://viacep.com.br/>
- **BrasilApi CEP** - Lookup de CEP (fallback)
  - Base URL: `https://brasilapi.com.br/api/cep/v1/`
  - Documentação: <https://brasilapi.com.br/docs>
- **OpenCep API** - Lookup de CEP (fallback)
  - Base URL: `https://opencep.com/v1/`
  - Documentação: <https://opencep.com/>

---

*📅 Última atualização: 5 de Março de 2026 (Sprint 8B Conclusion Review)*  
*🔄 Roadmap em constante evolução baseado em feedback, métricas e aprendizados*
*📊 Status atual: Sprint 8B ✅ CONCLUÍDO | MVP Launch em 28 de Março de 2026*
