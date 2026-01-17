# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **apenas débitos técnicos PENDENTES**. Itens resolvidos são removidos deste documento.

---

## ✅ Melhorias Recentes (Sprint 7.6 - Jan 2026)

### ⚡ Otimização de Desempenho de Testes de Integração - CONCLUÍDA

**Sprint**: Sprint 7.6 (12 Jan 2026)  
**Severidade**: ALTA (bloqueava testes em CI/CD)  
**Status**: ✅ RESOLVIDO

**Problema Identificado**:
- ❌ Testes de integração aplicavam migrations de TODOS os 6 módulos para CADA teste
- ❌ Timeout frequente (~60-70s de inicialização, vs esperado ~10-15s)
- ❌ PostgreSQL pool exhaustion (erro `57P01: terminating connection due to administrator command`)
- ❌ Teste `DocumentRepository_ShouldBeRegisteredInDI` falhava na branch fix/aspire-initialization
- ❌ Race conditions causavam falhas intermitentes sem mudança de código

**Solução Implementada**:
- ✅ **TestModule enum com Flags**: Permite especificar quais módulos cada teste precisa
- ✅ **RequiredModules property**: Override virtual para declarar dependências por teste
- ✅ **ApplyRequiredModuleMigrationsAsync**: Aplica migrations apenas dos módulos necessários
- ✅ **EnsureCleanDatabaseAsync**: Extraído para melhor reusabilidade
- ✅ **Backward compatible**: Default RequiredModules = TestModule.All

**Resultados Alcançados**:
- ✅ **Desempenho**: 83% faster para testes single-module (10s vs 60s)
- ✅ **Confiabilidade**: Eliminou timeouts e errors 57P01
- ✅ **Isolamento**: Cada teste carrega apenas módulos necessários
- ✅ **26 test classes otimizados**: Users (4), Providers (5), Documents (4), ServiceCatalogs (7), Locations (5), SearchProviders (1)
- ✅ **Test Results**: DocumentRepository_ShouldBeRegisteredInDI agora PASSA em 10s
- ✅ **Code Quality**: Método legado marcado como [Obsolete], testes renomeados, terminologia em português

**Arquivos Modificados**:
- `tests/MeAjudaAi.Integration.Tests/Base/BaseApiTest.cs`: Refactoring completo com TestModule pattern + [Obsolete] em método legado
- `tests/MeAjudaAi.Integration.Tests/README.md`: Guia de uso com terminologia em português (Desempenho)
- 26 test classes com `RequiredModules` override
- `AllowedCityExceptionHandlingTests.cs`: Testes renomeados para refletir comportamento real

**Documentação Atualizada**:
- ✅ [tests/README.md](../tests/MeAjudaAi.Integration.Tests/README.md): Guia de otimização de desempenho
- ✅ [docs/architecture.md](architecture.md#integration-test-infrastructure): Testing architecture
- ✅ [docs/development.md](development.md#testes-de-integração): Developer guide
- ✅ [docs/roadmap.md](roadmap.md#sprint-76): Sprint 7.6 implementation details

**Metrics**:

| Cenário | Antes | Depois | Improvement |
|---------|-------|--------|-------------|
| Inicialização | ~60-70s | ~10-15s | **83% faster** ⚡ |
| Migrations aplicadas | 6 módulos | Apenas necessárias | Mínimo |
| Timeouts | Frequentes | Eliminados | ✅ |

**Sprint Completion**: 12 Janeiro 2026  
**Issue**: fix/aspire-initialization (continuação)

---

## 🆕 Sprint 6 - Débitos Técnicos (BAIXA PRIORIDADE)

**Sprint**: Sprint 6 Concluída (30 Dez 2025 - 5 Jan 2026)  
**Status**: Itens de baixa prioridade, não bloqueiam Sprint 7

### 🎨 Frontend - Warnings de Analyzers (BAIXA)

**Severidade**: BAIXA (code quality)  
**Sprint**: BACKLOG (não afeta funcionalidade)

**Descrição**: Build do Admin Portal gera 10 warnings de analyzers (SonarLint + MudBlazor):

**Warnings SonarLint**:
1. **S2094** (6 ocorrências): Empty records em Actions
   - `DashboardActions.cs`: `LoadDashboardStatsAction` (record vazio)
   - `ProvidersActions.cs`: `LoadProvidersAction`, `GoToPageAction` (records vazios)
   - `ThemeActions.cs`: `ToggleDarkModeAction`, `SetDarkModeAction` (records vazios)
   - **Recomendação**: Converter para `interface` ou adicionar propriedades quando houver parâmetros
   
2. **S2953** (1 ocorrência): `App.razor:58` - Método `Dispose()` não implementa `IDisposable`
   - **Recomendação**: Renomear método ou implementar interface corretamente

3. **S2933** (1 ocorrência): `App.razor:41` - Campo `_theme` deve ser `readonly`
   - **Recomendação**: Adicionar modificador `readonly`

**Warnings MudBlazor**:
4. **MUD0002** (3 ocorrências): Atributos com casing incorreto em `MainLayout.razor`
   - `AriaLabel` → `aria-label` (lowercase)
   - `Direction` → `direction` (lowercase)
   - **Recomendação**: Atualizar para lowercase conforme padrão HTML

**Ações Recomendadas** (Sprint 7):
- [ ] Converter Actions vazias para interfaces (ThemeActions, DashboardActions)
- [ ] Corrigir Dispose() em App.razor (implementar IDisposable ou renomear)
- [ ] Adicionar readonly em _theme (App.razor)
- [ ] Corrigir casing de atributos MudBlazor (MainLayout.razor)

**Impacto**: Nenhum - build continua 100% funcional

---

### 📊 Frontend - Cobertura de Testes (MÉDIA)

**Severidade**: MÉDIA (quality assurance)  
**Sprint**: Sprint 7 (aumentar cobertura)

**Descrição**: Admin Portal tem apenas 10 testes bUnit criados. Coverage atual é baixo para produção.

**Testes Existentes**:
1. **ProvidersPageTests** (4 testes):
   - Dispatch LoadProvidersAction
   - Loading state display
   - Error message display
   - Data grid rendering
   
2. **DashboardPageTests** (4 testes):
   - Dispatch LoadDashboardStatsAction
   - Loading state display
   - KPI values display
   - Error message display
   
3. **DarkModeToggleTests** (2 testes):
   - Toggle dark mode action
   - Initial state rendering

**Gaps de Cobertura**:
- ❌ **Authentication flows**: Login/Logout/Callbacks não testados
- ❌ **Pagination**: GoToPageAction não validado em testes
- ❌ **API error scenarios**: Apenas erro genérico testado
- ❌ **MudBlazor interactions**: Clicks, inputs não validados
- ❌ **Fluxor Effects**: Chamadas API não mockadas completamente

**Ações Recomendadas** (Sprint 7):
- [ ] Criar 10+ testes adicionais (meta: 30 testes totais)
- [ ] Testar fluxos de autenticação (Authentication.razor)
- [ ] Testar paginação (GoToPageAction com diferentes páginas)
- [ ] Testar interações MudBlazor (button clicks, input changes)
- [ ] Aumentar coverage de error scenarios (API failures, network errors)
- [ ] Documentar patterns de teste em `docs/testing/bunit-patterns.md`

**Meta de Coverage**: 70-85% (padrão indústria para frontend)

---

### 🔐 Keycloak Client - Configuração Manual (MÉDIA)

**Severidade**: MÉDIA (developer experience)  
**Sprint**: Sprint 7 (automação desejável)

**Descrição**: Client `admin-portal` precisa ser criado MANUALMENTE no Keycloak realm `meajudaai`.

**Situação Atual**:
- ✅ Documentação completa: `docs/keycloak-admin-portal-setup.md`
- ✅ Passos detalhados (General Settings, Capability config, Login settings)
- ✅ Exemplo de usuário admin de teste
- ❌ Processo manual (8-10 passos via Admin Console)

**Problemas**:
1. **Onboarding lento**: Novo desenvolvedor precisa seguir ~10 passos
2. **Erro humano**: Fácil esquecer redirect URIs ou roles
3. **Reprodutibilidade**: Ambiente local pode divergir de dev/staging

**Ações Recomendadas** (Sprint 7):
- [ ] Criar script de automação: `scripts/setup-keycloak-clients.ps1`
- [ ] Usar Keycloak Admin REST API para criar client programaticamente
- [ ] Integrar script em `dotnet run --project src/Aspire/MeAjudaAi.AppHost`
- [ ] Adicionar validação: verificar se client já existe antes de criar
- [ ] Documentar script em `docs/keycloak-admin-portal-setup.md`

**Referências**:
- Keycloak Admin REST API: <https://www.keycloak.org/docs-api/latest/rest-api/index.html>
- Client Representation: <https://www.keycloak.org/docs-api/latest/rest-api/index.html#ClientRepresentation>

**Impacto**: Developer experience - não bloqueia produção

---

## 🔄 Sprint 5.5 - Itens Pendentes (BACKLOG)

**Branch**: `feature/refactor-and-cleanup`  
**Status**: Itens de baixa prioridade, não críticos para MVP

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging - Restante (BACKLOG)

**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: BACKLOG (não crítico para MVP)

**Descrição**: Continuar refatoração iniciada em 19/Dez/2025. Itens abaixo são melhorias adicionais, não bloqueiam desenvolvimento do frontend.

**Problemas Remanescentes**:

1. **Arquivos com múltiplas classes** (restantes):
   - ~~`DeadLetterServiceFactory.cs` contém: `NoOpDeadLetterService`, `IDeadLetterServiceFactory`, `EnvironmentBasedDeadLetterServiceFactory`~~ ✅ **RESOLVIDO** (19 Dez 2025)
   - ~~`IDeadLetterService.cs` contém: `DeadLetterStatistics`, `FailureRate`~~ ✅ **RESOLVIDO** (19 Dez 2025)
   - ~~`MessageRetryMiddleware.cs` contém: `IMessageRetryMiddlewareFactory`, `MessageRetryMiddlewareFactory`, `MessageRetryExtensions`~~ ✅ **RESOLVIDO** (19 Dez 2025)
   - ✅ **Factories organizados em pasta dedicada** (`Messaging/Factories/`)
   - ✅ `IMessageBusFactory.cs` + `MessageBusFactory.cs` separados
   - ✅ `IDeadLetterServiceFactory.cs` + `DeadLetterServiceFactory.cs` separados
   - `RabbitMqInfrastructureManager.cs` não possui interface separada `IRabbitMqInfrastructureManager` (avaliar necessidade)

2. **Inconsistência de nomenclatura** (se aplicável):
   - ~~Arquivo `DeadLetterServiceFactory.cs`, mas classe principal é `EnvironmentBasedDeadLetterServiceFactory`~~ ✅ **RESOLVIDO** (19 Dez 2025)
   - Arquivo `MessageBusFactory.cs` - verificar se precisa renomear

3. **Integration Events ausentes**:
   - Documents, SearchProviders, ServiceCatalogs não possuem integration events em Messages/
   - Faltam event handlers para comunicação entre módulos

**Ações de Refatoração** (BACKLOG - não crítico):
- [x] ~~Separar `NoOpDeadLetterService` em arquivo próprio: `NoOpDeadLetterService.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [✓] ~~Extrair `IDeadLetterServiceFactory` para arquivo próprio~~ ✅ CONCLUÍDO (19 Dez 2025) - em `Messaging/Factories/IDeadLetterServiceFactory.cs`
- [✓] ~~Renomear `EnvironmentBasedDeadLetterServiceFactory` → `DeadLetterServiceFactory`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair `DeadLetterStatistics` para: `DeadLetterStatistics.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair `FailureRate` para: `FailureRate.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair `IMessageRetryMiddlewareFactory` para: `IMessageRetryMiddlewareFactory.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair `MessageRetryMiddlewareFactory` para: `MessageRetryMiddlewareFactory.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Extrair `MessageRetryExtensions` para: `MessageRetryExtensions.cs`~~ ✅ CONCLUÍDO (19 Dez 2025)
- [x] ~~Criar `IMessageBusFactory.cs` separado e organizar factories em pasta dedicada~~ ✅ CONCLUÍDO (19 Dez 2025) - pasta `Messaging/Factories/`

- [ ] Avaliar necessidade de extrair `IRabbitMqInfrastructureManager` para arquivo separado
- [ ] Reorganizar estrutura de pastas em Messaging/ (sugestão abaixo) - se necessário
- [ ] Adicionar integration events para módulos faltantes - quando houver necessidade de comunicação inter-módulos
- [ ] Criar testes unitários para classes de messaging (>70% coverage) - se coverage cair abaixo do threshold

**Estrutura Proposta** (após refatoração):
```
src/Shared/Messaging/
├── Abstractions/
│   ├── IMessageBus.cs
│   ├── IMessageBusFactory.cs
│   ├── IDeadLetterService.cs
│   ├── IDeadLetterServiceFactory.cs
│   ├── IMessageRetryMiddlewareFactory.cs
│   └── IRabbitMqInfrastructureManager.cs
├── Factories/
│   ├── IMessageBusFactory.cs
│   ├── MessageBusFactory.cs
│   ├── IDeadLetterServiceFactory.cs
│   ├── DeadLetterServiceFactory.cs
├── DeadLetter/
│   ├── DeadLetterStatistics.cs
│   ├── FailureRate.cs
│   ├── DeadLetterOptions.cs
│   ├── NoOpDeadLetterService.cs
│   ├── DeadLetterServiceFactory.cs
│   ├── RabbitMqDeadLetterService.cs
│   └── ServiceBusDeadLetterService.cs
├── Handlers/
│   ├── MessageRetryMiddleware.cs
│   ├── MessageRetryMiddlewareFactory.cs
│   └── MessageRetryExtensions.cs
├── RabbitMq/
│   ├── RabbitMqMessageBus.cs
│   ├── RabbitMqInfrastructureManager.cs
│   └── RabbitMqOptions.cs
├── Options/
│   ├── ServiceBusOptions.cs
│   ├── MessageBusOptions.cs
│   ├── RabbitMqOptions.cs
│   └── DeadLetterOptions.cs
├── Services/
│   └── ServiceBusInitializationService.cs
├── ServiceBus/
│   ├── ServiceBusMessageBus.cs
│   └── ServiceBusTopicManager.cs
├── Messages/
│   ├── Documents/
│   │   ├── DocumentUploadedIntegrationEvent.cs
│   │   └── DocumentVerifiedIntegrationEvent.cs
│   ├── Providers/
│   ├── Users/
│   └── ...
└── EventTypeRegistry.cs
```

**Prioridade**: MÉDIA  
**Estimativa**: 8-10 horas  
**Sprint**: Sprint 5.5 / BACKLOG (baixa prioridade, não crítico para MVP)  
**Benefício**: Código mais organizado, manutenível e testável

---

#### 🔧 Refatoração Extensions (MeAjudaAi.Shared) (4-6h)

**Situação**: INCONSISTÊNCIA DE PADRÃO  
**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup)

**Problemas Identificados**:

1. **Extensions dentro de classes de implementação**:
   - `BusinessMetricsMiddlewareExtensions` está dentro de `BusinessMetricsMiddleware.cs`
   - Outros middlewares/serviços podem ter o mesmo padrão

2. **Falta de consolidação**:
   - Extensions espalhadas em múltiplos arquivos
   - Dificulta descoberta de métodos de extensão disponíveis
   - Falta padrão consistente com os módulos

**Ações de Refatoração**:
- [ ] Extrair `BusinessMetricsMiddlewareExtensions` para arquivo próprio
- [ ] Criar arquivo `MonitoringExtensions.cs` consolidando todas extensions de Monitoring
- [ ] Criar arquivo `CachingExtensions.cs` consolidando todas extensions de Caching
- [ ] Criar arquivo `MessagingExtensions.cs` consolidando todas extensions de Messaging
- [ ] Criar arquivo `AuthorizationExtensions.cs` consolidando todas extensions de Authorization
- [ ] Revisar pasta `Extensions/` - manter apenas extensions gerais/cross-cutting
- [ ] Documentar padrão: cada funcionalidade tem seu `<Funcionalidade>Extensions.cs`
- [ ] Aplicar padrão em todas as pastas do Shared

**Estrutura Proposta** (após refatoração):
```
src/Shared/
├── Monitoring/
│   ├── BusinessMetricsMiddleware.cs
│   ├── MetricsCollectorService.cs
│   └── MonitoringExtensions.cs ← NOVO (consolidado)
├── Caching/
│   ├── HybridCacheService.cs
│   └── CachingExtensions.cs ← NOVO (consolidado)
├── Messaging/
│   ├── ... (classes de messaging)
│   └── MessagingExtensions.cs ← NOVO (consolidado)
├── Authorization/
│   ├── ... (classes de autorização)
│   └── AuthorizationExtensions.cs ← NOVO (consolidado)
└── Extensions/
    ├── ServiceCollectionExtensions.cs (gerais)
    ├── ModuleServiceRegistrationExtensions.cs
    └── ... (apenas extensions cross-cutting)
```

**Padrão de Nomenclatura**:
- Arquivo: `<Funcionalidade>Extensions.cs` (e.g., `MonitoringExtensions.cs`)
- Classe: `public static class <Funcionalidade>Extensions`
- Namespace: `MeAjudaAi.Shared.<Funcionalidade>`

**Prioridade**: BAIXA  
**Estimativa**: 4-6 horas  
**Benefício**: Código mais organizado e consistente com padrão dos módulos

---

## ⚠️ CRÍTICO: Hangfire + Npgsql 10.x Compatibility Risk

**Arquivo**: `Directory.Packages.props`  
**Linhas**: 45-103  
**Situação**: VALIDAÇÃO EM ANDAMENTO - BLOQUEIO DE DEPLOY  
**Severidade**: ALTA  
**Issue**: [Criar issue para rastreamento]

**Descrição**: 
Hangfire.PostgreSql 1.20.12 foi compilado contra Npgsql 6.x, mas o projeto está migrando para Npgsql 10.x, que introduz breaking changes. A compatibilidade em runtime não foi validada pelo mantenedor do Hangfire.PostgreSql.

**Problema Identificado**:
- Npgsql 10.x introduz mudanças incompatíveis (breaking changes)
- Hangfire.PostgreSql 1.20.12 não foi testado oficialmente com Npgsql 10.x
- Risco de falhas em: persistência de jobs, serialização, conexão, corrupção de dados
- Deploy para produção está BLOQUEADO até validação completa

**Mitigação Implementada**:
1. ✅ Documentação detalhada de estratégia de versões em `Directory.Packages.props`
2. ✅ Testes de integração removidos - monitoramento via health checks
3. ✅ CI/CD gating configurado (`.github/workflows/pr-validation.yml`)
4. ✅ Procedimentos de rollback documentados
5. ✅ Plano de monitoramento de produção definido

**Validação Necessária ANTES de Deploy para Produção**:
- [ ] Todos os testes de integração Hangfire passando no CI/CD
- [ ] Validação manual localmente com carga realística
- [ ] Monitoramento de produção configurado (alertas de taxa de falha >5%)
- [ ] Procedimento de rollback testado localmente
- [ ] Plano de comunicação para stakeholders aprovado

**Opções de Implementação**:

**OPÇÃO 1 (ATUAL)**: Manter Npgsql 10.x + Hangfire.PostgreSql 1.20.12
- Requer validação completa via testes de integração
- Monitorar: <https://github.com/frankhommers/Hangfire.PostgreSql/issues>
- Rollback para Opção 2 se falhas detectadas

**OPÇÃO 2 (FALLBACK SEGURO)**: Downgrade para Npgsql 8.x
- Versões conhecidas e compatíveis
- Trade-off: Adia benefícios da migração para .NET 10
- Implementação imediata se Opção 1 falhar

**OPÇÃO 3 (FUTURO)**: Aguardar Hangfire.PostgreSql 2.x
- Suporte oficial para Npgsql 10.x
- Timeline desconhecida

**OPÇÃO 4 (EMERGÊNCIA)**: Backend alternativo
- Hangfire.Pro.Redis (requer licença)
- Hangfire.SqlServer (requer infraestrutura SQL Server)

**Prioridade**: CRÍTICA  
**Dependências**: Testes de integração, validação local, monitoramento de produção  
**Prazo**: Antes de qualquer deploy para produção

**Critérios de Aceitação**:
- [x] Testes de integração implementados e passando
- [x] CI/CD gating configurado para bloquear deploy se testes falharem
- [x] Documentação de compatibilidade criada
- [x] Procedimento de rollback documentado e testado
- [ ] Validação local com simulação de carga de produção
- [ ] Monitoramento de produção configurado
- [ ] Equipe treinada em procedimento de rollback
- [ ] Stakeholders notificados sobre o risco e plano de mitigação

**Documentação**:
- Guia completo: Monitoramento via health checks em produção
- Testes: Removidos - validação via health checks
- CI/CD: `.github/workflows/pr-validation.yml` (step "CRITICAL - Hangfire Npgsql 10.x Compatibility Tests")
- Configuração: `Directory.Packages.props` (linhas 45-103)

---

## ⚠️ MÉDIO: Falta de Testes para Infrastructure Extensions

**Arquivos**: 
- `src/Aspire/MeAjudaAi.AppHost/Extensions/KeycloakExtensions.cs`
- `src/Aspire/MeAjudaAi.AppHost/Extensions/PostgreSqlExtensions.cs`
- `src/Aspire/MeAjudaAi.AppHost/Extensions/MigrationExtensions.cs`

**Situação**: SEM TESTES - BAIXA PRIORIDADE  
**Severidade**: BAIXA  
**Sprint**: BACKLOG (não crítico - validação implícita)  
**Issue**: [BACKLOG - Considerar apenas se houver incidentes em produção]

**Descrição**: 
As classes de extensão do AppHost que configuram infraestrutura (Keycloak, PostgreSQL, Migrations) não possuem testes unitários/integração. Porém, análise técnica indica **baixo ROI para testes formais**.

**Componentes Sem Testes**:
1. **KeycloakExtensions** (~170 linhas) - "wiring code" de orquestração Aspire
2. **PostgreSqlExtensions** (~260 linhas) - configuração de containers/Azure
3. **MigrationExtensions** (~50 linhas) - registro de HostedService

**Mitigação ATUAL (Suficiente para MVP)**:
1. ✅ **Validação Implícita**: Falhas detectadas imediatamente no startup
   - PostgreSQL não sobe → aplicação não inicia
   - Keycloak configuração errada → erro visível nos logs
   - Migrations falham → aplicação não fica operacional
2. ✅ **Código de Orquestração**: Basicamente chamadas `.WithEnvironment()`, `.WithDataVolume()`
   - Pouca lógica complexa para testar
   - Validações são simples (senha vazia, hostname ausente)
3. ✅ **Logging Detalhado**: Console outputs indicam configurações aplicadas
4. ✅ **Estrutura Limpa**: Options/Results/Services bem separados

**Risco**: BAIXO - Bugs aparecem rapidamente em desenvolvimento

**Alternativas de Validação** (ordem de prioridade):

**OPÇÃO 1 (RECOMENDADA)**: Deixar como está
- Custo-benefício: Criar testes formais tem ROI baixo
- Tempo: 4-6h para coverage básico
- Benefício: Marginal - bugs já detectados em runtime
- **Decisão**: Priorizar testes de componentes com lógica de negócio real

**OPÇÃO 2**: Smoke Tests (30min - se houver incidentes)
- Criar teste E2E que valida AppHost startup completo
- Captura 80% dos problemas dessas extensions
- Implementar APENAS se houver incidentes em produção

**OPÇÃO 3**: Testes Formais (4-6h - BACKLOG)
- Usar `Aspire.Hosting.Testing`
- Mock `IDistributedApplicationBuilder`
- Testar cada método de extensão
- **Implementar SOMENTE** se:
  - Houver bugs recorrentes em produção relacionados a essas extensions
  - Refatoração grande planeada (>100 linhas mudadas)
  - Cliente/compliance exigir coverage específico

**Prioridade**: BAIXA → BACKLOG  
**Ação Atual**: NENHUMA (aguardar necessidade real)  
**Critério de Reavaliação**: Incidentes em produção OU refatoração >100 linhas

**Documentação**:
- Análise técnica registrada (20/Dez/2025)
- Decisão: Priorizar Hangfire/Database tests (maior ROI)

---

## ✅ ~~Swagger ExampleSchemaFilter - Migração para Swashbuckle 10.x~~ [REMOVIDO]

**Status**: REMOVIDO PERMANENTEMENTE (13 Dez 2025)  
**Razão**: Código problemático que sempre quebrava, difícil de testar, e não essencial

**Decisão**:
O `ExampleSchemaFilter` foi **removido completamente** do projeto por:
- Estar desabilitado desde a migração Swashbuckle 10.x (sempre quebrava)
- Causar erros de compilação frequentes no CI/CD
- Ser difícil de testar e manter
- Funcionalidade puramente cosmética (adicionar exemplos automáticos ao Swagger)
- Swagger funciona perfeitamente sem ele
- Exemplos podem ser adicionados manualmente via XML comments quando necessário

**Arquivos Removidos**:
- `src/Bootstrapper/MeAjudaAi.ApiService/Filters/ExampleSchemaFilter.cs` ❌
- `tests/MeAjudaAi.ApiService.Tests/Unit/Swagger/ExampleSchemaFilterTests.cs` ❌
- TODO em `DocumentationExtensions.cs` removido

**Alternativa**:
Use **XML documentation comments** para adicionar exemplos quando necessário:
```csharp
/// <summary>
/// Email do usuário
/// </summary>
/// <example>usuario@exemplo.com</example>
public string Email { get; set; }
```

**Commit**: [Adicionar hash após commit]

---
- Original PR/Issue que introduziu IOpenApiSchema: [A investigar]

---

## Melhorias nos Testes de Integração

### Melhoria do Teste de Status de Verificação de Prestador
**Arquivo**: `tests/MeAjudaAi.Integration.Tests/Providers/ProvidersIntegrationTests.cs`  
**Linha**: ~172-199  
**Situação**: Aguardando Implementação de Funcionalidade Base  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - TODO resolution  

**Descrição**: 
O teste `GetProvidersByVerificationStatus_ShouldReturnOnlyPendingProviders` atualmente apenas valida a estrutura da resposta devido à falta de endpoints de gerenciamento de status de verificação.

**Problema Identificado**:
- TODO comentário nas linhas 180-181 indica limitação atual
- Teste não pode verificar comportamento real de filtragem
- Não há como definir status de verificação durante criação de prestador

**Melhoria Necessária**:
- Implementar endpoints de gerenciamento de status de verificação de prestadores (aprovar/rejeitar/atualizar verificação)
- Criar prestadores de teste com diferentes status de verificação
- Melhorar o teste para verificar o comportamento real de filtragem (apenas prestadores com status Pending retornados)
- Adicionar testes similares para outros status de verificação (Approved, Rejected, etc.)

**Opções de Implementação**:
1. **Abrir nova issue** para rastrear implementação de endpoints de gerenciamento de status
2. **Implementar funcionalidade** de atualização de status de verificação
3. **Criar testes mais abrangentes** quando endpoints estiverem disponíveis

**Prioridade**: Média  
**Dependências**: Endpoints de API para gerenciamento de status de verificação de prestadores  

**Critérios de Aceitação**:
- [ ] Endpoints de gerenciamento de status de verificação de prestadores disponíveis
- [ ] Teste pode criar prestadores com diferentes status de verificação
- [ ] Teste verifica que a filtragem retorna apenas prestadores com o status especificado
- [ ] Teste inclui limpeza dos dados de teste criados
- [ ] Testes similares adicionados para todos os valores de status de verificação

---

## 🧪 Testes E2E Ausentes - Módulo SearchProviders

**Módulo**: `src/Modules/SearchProviders`  
**Tipo**: Débito de Teste  
**Severidade**: MÉDIA  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - BACKLOG (2-3 sprints)  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**:
O módulo SearchProviders não possui testes E2E (end-to-end), apenas testes de integração e unitários. Testes E2E são necessários para validar o fluxo completo de busca de prestadores, incluindo integração com APIs externas (IBGE), filtros, paginação, e respostas HTTP completas.

**Contexto**:
- Identificado durante code review automatizado (CodeRabbit)
- Testes de integração existentes cobrem lógica de negócio e repositórios
- Faltam testes que validam endpoints HTTP completos com autenticação real

**Impacto**:
- Risco de regressões em endpoints de busca não detectadas até produção
- Falta de validação de integração completa API externa → Aplicação → Resposta HTTP
- Dificuldade em validar comportamento de autenticação e autorização em cenários reais

**Escopo de Testes E2E Necessários**:

1. **SearchProviders API Endpoints**:
   - [ ] `GET /api/search-providers/search` - Busca com múltiplos filtros
   - [ ] `GET /api/search-providers/search` - Paginação e ordenação
   - [ ] `GET /api/search-providers/search` - Busca com autenticação/autorização
   - [ ] `GET /api/search-providers/search` - Respostas de erro (400, 401, 404, 500)

2. **Integração com IBGE API**:
   - [ ] Validação de respostas da API do IBGE (mock ou real)
   - [ ] Tratamento de timeouts e erros de rede
   - [ ] Validação de mapeamento de dados geográficos (UF, município)

3. **Filtros e Busca**:
   - [ ] Busca por localização (estado, cidade)
   - [ ] Busca por tipo de serviço
   - [ ] Busca por status de verificação
   - [ ] Combinação de múltiplos filtros

4. **Desempenho e Carga**:
   - [ ] Busca com grande volume de resultados (1000+ prestadores)
   - [ ] Validação de tempos de resposta (<500ms para buscas simples)
   - [ ] Cache de resultados de API externa

**Arquivos Relacionados**:
- `src/Modules/SearchProviders/API/` - Endpoints a serem testados
- `tests/MeAjudaAi.E2E.Tests/` - Localização sugerida para novos testes
- `tests/MeAjudaAi.Integration.Tests/Infrastructure/WireMockFixture.cs` - Mock de IBGE API

**Prioridade**: Média  
**Estimativa**: 2-3 sprints  
**Dependências**: 
- Infraestrutura de testes E2E já estabelecida (`MeAjudaAi.E2E.Tests`)
- WireMock configurado para simulação de IBGE API
- TestContainers disponível para PostgreSQL e Redis

**Critérios de Aceitação**:
- [ ] Pelo menos 15 testes E2E cobrindo cenários principais de busca
- [ ] Cobertura de autenticação/autorização em todos os endpoints
- [ ] Testes validam códigos de status HTTP corretos
- [ ] Testes validam estrutura completa de resposta JSON
- [ ] Testes incluem cenários de erro e edge cases
- [ ] Testes executam em CI/CD com sucesso
- [ ] Documentação de testes E2E atualizada

**Notas Técnicas**:
- Utilizar `TestContainerTestBase` como base para testes E2E
- Configurar WireMock para simular respostas da API do IBGE
- Usar `ConfigurableTestAuthenticationHandler` para cenários de autenticação
- Validar integração com Redis (cache) e PostgreSQL (dados)

---

## 📦 Microsoft.OpenApi 2.3.0 - Bloqueio de Atualização para 3.x

**Arquivo**: `Directory.Packages.props` (linha ~46)  
**Situação**: BLOQUEADO - Incompatibilidade com ASP.NET Core Source Generators  
**Severidade**: BAIXA (não crítico, funciona perfeitamente)  
**Sprint**: N/A - Aguardar correção da Microsoft  
**Issue**: [Monitoramento contínuo]

**Descrição**:
Microsoft.OpenApi está pinado em versão 2.3.0 porque a versão 3.0.2 é incompatível com os source generators do ASP.NET Core 10.0 (`Microsoft.AspNetCore.OpenApi.SourceGenerators`).

**Problema Identificado**:
```
error CS0200: Property or indexer 'IOpenApiMediaType.Example' cannot be assigned to -- it is read only
```

**Testes Realizados**:
```text
- ✅ Testado com SDK 10.0.101 (Dez 2025) - ainda quebra
- ✅ Testado Microsoft.OpenApi 3.0.2 - incompatível
- ✅ Confirmado que 2.3.0 funciona perfeitamente
```

**Causa Raiz**:
- Microsoft.OpenApi 3.x mudou `IOpenApiMediaType.Example` para read-only (breaking change)
- ASP.NET Core source generator ainda gera código que tenta escrever nessa propriedade
- Source generator não foi atualizado para API do OpenApi 3.x

**Dependência**: Swashbuckle.AspNetCore
- Swashbuckle 10.x depende de Microsoft.OpenApi (transitivo)
- Projeto usa Swashbuckle para Swagger UI e customizações avançadas
- Swashbuckle v10 migration guide: [Swashbuckle v10 Migration](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)

**Opções de Resolução**:

**OPÇÃO 1 (ATUAL - RECOMENDADA)**: Manter Microsoft.OpenApi 2.3.0
- ✅ Funciona perfeitamente
- ✅ Zero impacto em funcionalidades
- ✅ Swagger UI completo e funcional
- ⚠️ Versão desatualizada (mas estável)

**OPÇÃO 2 (FUTURO)**: Aguardar correção da Microsoft
- Microsoft atualiza source generator para OpenApi 3.x
- Timeline: Desconhecida (provavelmente .NET 11 ou patch futuro)
- Monitorar: [ASP.NET Core Issues](https://github.com/dotnet/aspnetcore/issues)

**OPÇÃO 3 (COMPLEXA - NÃO RECOMENDADA AGORA)**: Migrar para ASP.NET Core OpenAPI nativo
- Remove Swashbuckle completamente
- Usa `Microsoft.AspNetCore.OpenApi` nativo (.NET 9+)
- **PROBLEMA**: Não inclui Swagger UI por padrão
  - Precisa adicionar Scalar/SwaggerUI/RapiDoc separadamente
  - Perde configurações avançadas de UI (InjectStylesheet, DocExpansion, etc)
- **ESFORÇO**: 5-8 horas de trabalho
  - Migrar CustomSchemaIds → transformers
  - Migrar CustomOperationIds → transformers  
  - Migrar ApiVersionOperationFilter → transformers
  - Configurar UI externa (Scalar recomendado)
  - Atualizar 3 arquivos de teste
- **ROI**: Baixo - funcionalidade atual é completa

**Monitoramento**:
- [ ] Verificar releases do .NET SDK para correções no source generator
- [ ] Testar Microsoft.OpenApi 3.x a cada atualização de SDK
- [ ] Monitorar Swashbuckle releases para melhor suporte OpenApi 3.x
- [ ] Avaliar migração para OpenAPI nativo quando UI nativo estiver disponível

**Prioridade**: BAIXA (não urgente)  
**Estimativa**: Aguardar correção oficial (sem ação necessária)  
**Workaround Atual**: Manter 2.3.0 (100% funcional)

**Critérios para Atualização**:
- [ ] Microsoft corrigir source generator para OpenApi 3.x, OU
- [ ] Swashbuckle suportar completamente OpenApi 3.x, OU
- [ ] Necessidade real de features do OpenApi 3.x (atualmente nenhuma)

**Documentação**:
- Comentário detalhado em `Directory.Packages.props` (linhas 46-49)
- Migration guide Swashbuckle: [Swashbuckle v10 Migration](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- ASP.NET Core OpenAPI docs: [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi)

**Nota**: Esta limitação **NÃO afeta** funcionalidade, performance ou segurança. É puramente uma questão de versão de dependência.

---

## 📋 Padronização de Records (Para Próxima Sprint)

**Arquivo**: Múltiplos arquivos em `src/Shared/Contracts/**` e `src/Modules/**/Domain/**`  
**Situação**: INCONSISTÊNCIA - Dois padrões em uso  
**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - Baixa prioridade  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**: 
Atualmente existem dois padrões de sintaxe para records no projeto:

### Padrão 1: Positional Records (Sintaxe Concisa)

```csharp
public sealed record ModuleCoordinatesDto(
    double Latitude,
    double Longitude);
```

### Padrão 2: Property-based Records (Sintaxe Explícita)

```csharp
public sealed record ModuleLocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
```

**Análise**:

*Positional Records:*
- ✅ Mais conciso
- ✅ Gera automaticamente construtor, desconstrutor, Equals, GetHashCode
- ✅ Ideal para DTOs simples e imutáveis
- ❌ Menos flexível para validação/lógica customizada
- ❌ Ordem dos parâmetros importa

*Property-based Records:*
- ✅ Maior flexibilidade (validação, valores padrão complexos)
- ✅ Permite required e init-only de forma explícita
- ✅ Ordem não importa
- ❌ Mais verboso
- ❌ Não gera desconstrutor automaticamente

**Recomendação**:

*Para DTOs simples* (maioria dos casos em Contracts/Modules): Usar **Positional Records**
- São mais concisos
- Comunicação entre módulos não precisa de lógica complexa
- Imutabilidade garantida por design

*Para Value Objects e Domain Models*: Usar **Property-based Records**
- Permite validação no construtor
- Maior controle sobre comportamento

**Ação Sugerida**:
Na próxima sprint, padronizar todos os records em:
- `src/Shared/Contracts/**/*.cs` → Positional Records
- `src/Modules/**/Domain/**/*.cs` → Property-based Records (onde fizer sentido)

**Arquivos para Revisar**:
- [ ] Todos os DTOs em Contracts/Modules
- [ ] Value Objects em Domain
- [ ] Responses/Requests em Shared

**Prioridade**: BAIXA (não urgente, melhoria de consistência)  
**Estimativa**: 2-3 horas  

---

## Instruções para Mantenedores

1. **Conversão para Issues do GitHub**: 
   - Copiar a descrição da melhoria para um novo issue do GitHub
   - Adicionar labels apropriadas (`technical-debt`, `testing`, `enhancement`)
   - Vincular ao arquivo específico e número da linha
   - Adicionar ao backlog do projeto com prioridade apropriada

2. **Atualizando este Documento**:
   - Marcar itens como "Issue Criado" com número do issue quando convertido
   - Remover itens completos ou mover para seção "Concluído"
   - Adicionar novos itens de débito técnico conforme identificados

3. **Referências de Código**:
   - Usar tag `[ISSUE]` em comentários TODO para indicar itens rastreados aqui
   - Incluir caminho do arquivo e números de linha para navegação fácil
   - Manter descrições específicas e acionáveis
- Roadmap: Adicionado em "Média Prioridade (6-12 meses - Fase 2)"
---

## 🔮 Melhorias Futuras (Backlog)

### 🧪 Testing & Quality Assurance

**Severidade**: MÉDIA  
**Sprint**: Backlog (não bloqueante)

**Unit Tests - Memory Management**:
- [ ] Add unit tests for LocalizationSubscription disposal
- [ ] Add unit tests for PerformanceHelper LRU eviction
- [ ] Create unit tests for .resx resource loading

**Production Monitoring**:
- [ ] Memory profiling in production environment
- [ ] Monitor cache hit rates and eviction frequency

**Origem**: Sprint 7.16 (Memory Leak Fixes) e Sprint 7.17 (Localization Migration)

---

### 🌐 Localization (i18n) Enhancements

**Severidade**: MÉDIA  
**Sprint**: Backlog (expansão gradual)

**Hardcoded Strings Migration**:
- [ ] Migrate ErrorHandlingService hardcoded strings to .resx (48 mensagens de erro)
- [ ] Integrate FluentValidation with localized error messages
- [ ] Add more resource strings (currently only 48 base strings)

**Advanced Localization Features**:
- [ ] Add pluralization examples (ICU MessageFormat)
- [ ] Add date/time localization (DateTimeFormatInfo)
- [ ] Add number formatting localization (NumberFormatInfo)

**Impacto**: Melhora experiência do usuário para expansão internacional

**Origem**: Sprint 7.17 (Localization Migration)

---

### ⚡ Error Handling & Resilience

**Severidade**: MÉDIA  
**Sprint**: Backlog (otimização)

**Cancellation Token Propagation**:
- [ ] Update ExecuteApiCallAsync extension method to accept CancellationToken
- [ ] Apply cancellation pattern to ServiceCatalogsEffects
- [ ] Apply cancellation pattern to DocumentsEffects
- [ ] Apply cancellation pattern to LocationsEffects
- [ ] Add per-component CancellationTokenSource that cancels on Dispose()
- [ ] Implement navigation-triggered cancellation in routing layer

**Benefícios**:
- Previne requisições zombie após navegação
- Melhora responsividade da aplicação
- Reduz carga no backend

**Status Atual**: ExecuteWithErrorHandlingAsync já suporta CancellationToken (Sprint 7.18)

**Origem**: Sprint 7.18 (Correlation ID & Cancellation Support)

---

### 🎨 UI/UX Improvements

**Severidade**: BAIXA  
**Sprint**: Backlog

**Brand Color Scheme**:
- [ ] Apply login page color scheme (blue, cream, orange, white) to entire Admin Portal
- [ ] Update MudBlazor theme with brand colors
- [ ] Standardize component styling across portal

**Impacto**: Consistência visual com identidade da marca

**Origem**: Sprint 7.19 (User Request - Jan 16, 2026)