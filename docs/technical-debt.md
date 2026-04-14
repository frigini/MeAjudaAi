# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **débitos técnicos e seu histórico de otimização**. Itens podem aparecer como PENDENTES ou OTIMIZADO/FECHADO para histórico.

---

## 🆕 Sprint 6-7 - Débitos Técnicos

**Sprint**: Sprint 6-7 (30 Dez 2025 - 16 Jan 2026)  
**Status**: Itens de baixa a média prioridade



## 🔄 Refatorações de Código (BACKLOG)

**Status**: Baixa prioridade, não críticos para MVP


## 🔗 GitHub Issues - Débitos Técnicos Sincronizados




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

- [ ] Implement focus-based cancellation strategy

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

## 📝 Instruções para Mantenedores

1. **Conversão para Issues**: Copiar descrição para GitHub issue com labels (`technical-debt`, `testing`, `enhancement`)
2. **Atualizando Documento**: Remover itens completos, adicionar novos conforme identificados
3. **Referências**: Usar tag `[ISSUE]` em comentários TODO, incluir path e linhas

---