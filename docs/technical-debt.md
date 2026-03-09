# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **apenas débitos técnicos PENDENTES**. Itens resolvidos são removidos deste documento.

---

## 🆕 Sprint 6-7 - Débitos Técnicos

**Sprint**: Sprint 6-7 (30 Dez 2025 - 16 Jan 2026)  
**Status**: Itens de baixa a média prioridade

### 🎨 Frontend - Warnings de Analyzers (BAIXA)

**Severidade**: BAIXA (code quality)  
**Status**: 🔄 EM SPRINT 8B.2 (Refactoring)

**Descrição**: Build do Admin Portal e Contracts gera warnings de analyzers (SonarLint + MudBlazor).

**Warnings Analisador de Segurança (MeAjudaAi.Contracts)**:
4. **Hard-coded Credential False Positive**: `src/Contracts/Utilities/Constants/ValidationMessages.cs`
   - **Problema**: Mensagens de erro contendo a palavra "Password" disparam o scanner.
   - **Ação**: Adicionar `[SuppressMessage]` ou `.editorconfig` exclusion.

**Impacto**: Nenhum - build continua 100% funcional.

---

### 📊 Frontend - Cobertura de Testes (MÉDIA)

**Severidade**: MÉDIA (quality assurance)  
**Sprint**: Sprint 7.16 (aumentar cobertura)

**Descrição**: Admin Portal tem 43 testes bUnit criados. Meta é maximizar quantidade de testes (não coverage percentual).

**Decisão Técnica**: Coverage percentual NÃO é coletado para Blazor WASM devido a:
- Muito código gerado automaticamente (`.g.cs`, `.razor.g.cs`)
- Métricas não confiáveis para componentes compilados para WebAssembly
- **Foco**: Quantidade e qualidade de testes, não percentual de linhas

**Testes Existentes** (43 testes):
1. **ProvidersPageTests** (4 testes)
2. **DashboardPageTests** (4 testes)
3. **DarkModeToggleTests** (2 testes)
4. **+ 33 outros testes** de Pages, Dialogs, Components

**Gaps de Cobertura**:
- ❌ **Authentication flows**: Login/Logout/Callbacks não testados
- ❌ **Pagination**: GoToPageAction não validado em testes
- ❌ **API error scenarios**: Apenas erro genérico testado
- ❌ **MudBlazor interactions**: Clicks, inputs não validados
- ❌ **Fluxor Effects**: Chamadas API não mockadas completamente

**Ações Recomendadas** (Sprint 7.16):
- [ ] Criar 20+ testes adicionais (meta: 60+ testes totais)
- [ ] Testar fluxos de autenticação
- [ ] Testar paginação
- [ ] Testar interações MudBlazor
- [ ] Aumentar coverage de error scenarios

**Meta**: 60-80+ testes bUnit (quantidade), não coverage percentual

**BDD Futuro**: Após Customer App, implementar SpecFlow + Playwright para testes end-to-end de fluxos completos (Frontend → Backend → APIs terceiras).

---


## 🔄 Refatorações de Código (BACKLOG)

**Status**: Baixa prioridade, não críticos para MVP

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging (OTIMIZADO)

**Status**: ✅ `IRabbitMqInfrastructureManager` implementado.
**Pendente**: Event handlers para comunicação entre novos módulos (SearchProviders, ServiceCatalogs).

---

## 🔗 GitHub Issues - Débitos Técnicos Sincronizados

### 🔐 [ISSUE #141] Reintegrar login social com Instagram via Keycloak OIDC
**Severidade**: BAIXA (feature parity)
**Status**: OPEN
**Descrição**: Keycloak 26.x removeu built-in Instagram provider. Necessário configurar como generic OIDC.


### 🚀 [ISSUE #112] tech: aguardar versão stable do Aspire.Hosting.Keycloak
**Severidade**: MÉDIA (startup lifecycle)
**Status**: OPEN
**Descrição**: Aspire.Hosting.Keycloak (v13.1.0-preview) não suporta health checks reais. Serviços iniciam sem esperar Keycloak estar pronto.

---

## ⚠️ CRÍTICO: Hangfire + Npgsql 10.x Compatibility Risk

**Situação**: MONITORAMENTO CONTÍNUO (Issue #39 CLOSED, mitigado)  
**Severidade**: MÉDIA  
**Status**: Atualizado para **Hangfire.PostgreSql 1.21.1**.
**Nota**: Compatibilidade com Npgsql 10.x validada em desenvolvimento. Aguardar versão 2.x para suporte oficial total.

---


## 🔮 Melhorias Futuras (Backlog)

### 🧪 Testing & Quality Assurance

**Severidade**: MÉDIA  
**Sprint**: Backlog

- [ ] Unit tests for LocalizationSubscription disposal
- [ ] Unit tests for PerformanceHelper LRU eviction
- [ ] Memory profiling in production

**Origem**: Sprint 7.16-7.17 (Memory & Localization)

---

### 🌐 Localization Enhancements

**Severidade**: MÉDIA  
**Sprint**: Backlog

- [ ] Migrate ErrorHandlingService hardcoded strings to .resx
- [ ] Integrate FluentValidation with localized messages
- [ ] Add pluralization examples
- [ ] Add date/time and number formatting localization

**Origem**: Sprint 7.17

---

### ⚡ Error Handling & Resilience

**Severidade**: MÉDIA  
**Sprint**: Backlog

- [ ] Apply CancellationToken to ServiceCatalogs/Documents/Locations Effects
- [ ] Add per-component CancellationTokenSource
- [ ] Implement navigation-triggered cancellation

**Origem**: Sprint 7.18

---

### 🎨 UI/UX Improvements

**Severidade**: BAIXA  
**Sprint**: Backlog

- [ ] Apply brand colors (blue, cream, orange) to entire Admin Portal
- [ ] Update MudBlazor theme
- [ ] Standardize component styling

**Origem**: Sprint 7.19

---

## ✅ Resumo de Débitos Técnicos Resolvidos (Sprint 8B.2)

### 🏗️ NX Monorepo & Root Cleanup
- ✅ **Organização**: Projetos movidos para `src/Web/` com nomes padronizados e estrutura NX.
- ✅ **Limpeza**: Pastas redundantes (`api/`, `packages/`, `site/`, `build/`, `automation/`) removidas da raiz.

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging
- ✅ **Unificação**: Azure Service Bus removido completamente, unificado no RabbitMQ para desenvolvimento e produção.
- ✅ **Infrastructure-as-Code**: Arquivos Bicep de ASB removidos.

### 🛡️ Final Technical Excellence - Automação e Padrões
- ✅ **Automação Keycloak**: Implementado `KeycloakBootstrapService` no AppHost para criar automaticamente os clients via API REST do Keycloak durante a inicialização, substituindo a necessidade de scripts PowerShell externos.
- ✅ **Refatoração Shared**: Extensões de monitoramento centralizadas em `MonitoringExtensions.cs`.
- ✅ **Issue #113**: Configuração de logging de resiliência HTTP com Polly modernizada para injetar `ILogger` a partir do DI, corrigindo problemas de log tracking.
- ✅ **Padronização de Records**: Sintaxe de DTOs atualizada para o formato "Positional Records" (ex: `ModuleDocumentDto`), mantendo a abordagem property-based apenas onde há validação complexa de domínio.

---

## 📝 Instruções para Mantenedores

1. **Conversão para Issues**: Copiar descrição para GitHub issue com labels (`technical-debt`, `testing`, `enhancement`)
2. **Atualizando Documento**: Remover itens completos, adicionar novos conforme identificados
3. **Referências**: Usar tag `[ISSUE]` em comentários TODO, incluir path e linhas

---