# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **débitos técnicos e seu histórico de otimização**. Itens podem aparecer como PENDENTES ou OTIMIZADO/FECHADO para histórico.

---

## 🆕 Sprint 6-7 - Débitos Técnicos

**Sprint**: Sprint 6-7 (30 Dez 2025 - 16 Jan 2026)  
**Status**: Itens de baixa a média prioridade

### 📊 Frontend - Cobertura de Testes (MÉDIA)

**Severidade**: MÉDIA (quality assurance)  
**Status**: 🔄 EM SPRINT 8E (E2E Tests com Playwright)

**Descrição**: Admin Portal foi migrado para React. Testes de frontend agora focam em E2E com Playwright.

**Framework de Testes**: Playwright (para todos os apps React)
- Customer Web App
- Provider Web App
- Admin Portal (React)

**BDD**: Playwright para testes end-to-end de fluxos completos (Frontend → Backend → APIs terceiras).

---


## 🔄 Refatorações de Código (BACKLOG)

**Status**: Baixa prioridade, não críticos para MVP

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging (OTIMIZADO)

**Status**: ✅ `IRabbitMqInfrastructureManager` implementado.
**Pendente**: Event handlers para comunicação entre novos módulos (SearchProviders, ServiceCatalogs).

---

## 🔗 GitHub Issues - Débitos Técnicos Sincronizados



### 🚀 [ISSUE #112] tech: aguardar versão stable do Aspire.Hosting.Keycloak
**Status**: 📋 OTIMIZADO (Sprint 8B.2)  
**Descrição**: Aspire.Hosting.Keycloak (preview) não suporta health checks reais. Serviços iniciam sem esperar Keycloak estar pronto.
**Impacto**: Console logs do backend e Admin Portal mostram falhas de conexão transientes até Keycloak inicializar.

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

- [ ] Apply brand colors (blue, cream, orange) to entire Admin Portal (React)
- [ ] Update React component library theme
- [ ] Standardize component styling

**Origem**: Sprint 7.19

---

## ✅ Resumo de Débitos Técnicos Resolvidos (Sprint 8B.2 - Tech Excellence)

### 🛡️ Final Technical Excellence - Automação e Padrões
- ✅ **Automação Keycloak**: Implementado `KeycloakBootstrapService` no AppHost para criar automaticamente os clients via API REST do Keycloak durante a inicialização, substituindo a necessidade de scripts PowerShell externos.
- ✅ **Refatoração Shared**: Extensões de monitoramento centralizadas em `MonitoringExtensions.cs`.
- ✅ **Issue #113**: Configuração de logging de resiliência HTTP com Polly modernizada para injetar `ILogger` a partir do DI, corrigindo problemas de log tracking.
- ✅ **Padronização de Records**: Sintaxe de DTOs atualizada para o formato "Positional Records" (ex: `ModuleDocumentDto`), mantendo a abordagem property-based apenas onde há validação complexa de domínio.
## 📝 Instruções para Mantenedores

1. **Conversão para Issues**: Copiar descrição para GitHub issue com labels (`technical-debt`, `testing`, `enhancement`)
2. **Atualizando Documento**: Remover itens completos, adicionar novos conforme identificados
3. **Referências**: Usar tag `[ISSUE]` em comentários TODO, incluir path e linhas

---