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

**Warnings MudBlazor (MeAjudaAi.Web.Admin)**:
1. **S2094** (records vazios em Actions)
2. **S2953** (App.razor Dispose)
3. **MUD0002** (Casing de atributos HTML em MainLayout.razor)

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

### 🔐 Keycloak Client - Configuração Manual (MÉDIA)

**Severidade**: MÉDIA (developer experience)  
**Status**: 🔄 EM SPRINT 8B.2 (Automação)

**Descrição**: Client `admin-portal` precisa ser criado MANUALMENTE no Keycloak realm `meajudaai`.

**Ações Pendentes**:
- [ ] Criar script de automação: `scripts/setup-keycloak-clients.ps1`
- [ ] Usar Keycloak Admin REST API para criar client programaticamente
- [ ] Integrar script em `dotnet run --project src/Aspire/MeAjudaAi.AppHost`

**Impacto**: Developer experience - não bloqueia produção.

---

## 🔄 Refatorações de Código (BACKLOG)

**Status**: Baixa prioridade, não críticos para MVP

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging (OTIMIZADO)

**Status**: ✅ `IRabbitMqInfrastructureManager` implementado.
**Pendente**: Event handlers para comunicação entre novos módulos (SearchProviders, ServiceCatalogs).

---

### 🔧 Refatoração Extensions (MeAjudaAi.Shared)

**Severidade**: BAIXA (manutenibilidade)  
**Status**: 🔄 EM SPRINT 8B.2
**Ações Pendentes**:
- [ ] Extrair `BusinessMetricsMiddlewareExtensions` para arquivo próprio.
- [ ] Consolidar extensões em `MonitoringExtensions.cs`, `CachingExtensions.cs`, etc.

---

## 🔗 GitHub Issues - Débitos Técnicos Sincronizados

### 🔐 [ISSUE #141] Reintegrar login social com Instagram via Keycloak OIDC
**Severidade**: BAIXA (feature parity)
**Status**: OPEN
**Descrição**: Keycloak 26.x removeu built-in Instagram provider. Necessário configurar como generic OIDC.

### 📊 [ISSUE #113] tech: migrar Polly resilience logging para ILogger do DI
**Severidade**: MÉDIA (observabilidade)
**Status**: 🔄 EM SPRINT 8B.2
**Descrição**: `Microsoft.Extensions.Http.Resilience` não permite injetar ILogger facilmente. Necessário workaround ou custom DelegatingHandler para logar retries e circuit breakers.

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

## 📋 Padronização de Records

**Arquivo**: Múltiplos arquivos em `src/Shared/Contracts/**` e `src/Modules/**/Domain/**`  
**Severidade**: MÉDIA (padronização importante)  
**Status**: 🔄 EM SPRINT 8B.2

**Descrição**: Existem dois padrões de sintaxe para records no projeto:

**Padrão 1 - Positional Records**:
```csharp
public sealed record ModuleCoordinatesDto(double Latitude, double Longitude);
```

**Padrão 2 - Property-based Records**:
```csharp
public sealed record ModuleLocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
```

**Recomendação**:
- DTOs simples → Positional Records
- Value Objects com validação → Property-based Records

**Ação Sugerida** (Sprint 7.16):
- [ ] Padronizar records em `src/Shared/Contracts/**/*.cs`
- [ ] Padronizar records em `src/Modules/**/Domain/**/*.cs`

**Prioridade**: BAIXA  
**Estimativa**: 2-3 horas

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

## 📝 Instruções para Mantenedores

1. **Conversão para Issues**: Copiar descrição para GitHub issue com labels (`technical-debt`, `testing`, `enhancement`)
2. **Atualizando Documento**: Remover itens completos, adicionar novos conforme identificados
3. **Referências**: Usar tag `[ISSUE]` em comentários TODO, incluir path e linhas

---