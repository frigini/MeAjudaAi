# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **apenas débitos técnicos PENDENTES**. Itens resolvidos são removidos deste documento.

---

## 🆕 Sprint 6-7 - Débitos Técnicos

**Sprint**: Sprint 6-7 (30 Dez 2025 - 16 Jan 2026)  
**Status**: Itens de baixa a média prioridade

### 🎨 Frontend - Warnings de Analyzers (BAIXA)

**Severidade**: BAIXA (code quality)  
**Sprint**: Sprint 7.16 (planejado)

**Descrição**: Build do Admin Portal gera warnings de analyzers (SonarLint + MudBlazor):

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

**Impacto**: Nenhum - build continua 100% funcional

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
**Sprint**: Sprint 7.16 (automação desejável)

**Descrição**: Client `admin-portal` precisa ser criado MANUALMENTE no Keycloak realm `meajudaai`.

**Situação Atual**:
- ✅ Documentação completa: `docs/keycloak-admin-portal-setup.md`
- ❌ Processo manual (8-10 passos via Admin Console)

**Problemas**:
1. **Onboarding lento**: Novo desenvolvedor precisa seguir ~10 passos
2. **Erro humano**: Fácil esquecer redirect URIs ou roles
3. **Reprodutibilidade**: Ambiente local pode divergir de dev/staging

**Ações Recomendadas** (Sprint 7.16):
- [ ] Criar script de automação: `scripts/setup-keycloak-clients.ps1`
- [ ] Usar Keycloak Admin REST API para criar client programaticamente
- [ ] Integrar script em `dotnet run --project src/Aspire/MeAjudaAi.AppHost`

**Impacto**: Developer experience - não bloqueia produção

---

## 🔄 Refatorações de Código (BACKLOG)

**Status**: Baixa prioridade, não críticos para MVP

### 🏗️ Refatoração MeAjudaAi.Shared.Messaging (BACKLOG)

**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: BACKLOG

**Problemas Remanescentes**:
- `RabbitMqInfrastructureManager.cs` não possui interface separada `IRabbitMqInfrastructureManager` (avaliar necessidade)
- Integration Events ausentes: Documents, SearchProviders, ServiceCatalogs não possuem integration events
- Faltam event handlers para comunicação entre módulos

**Ações Pendentes**:
- [ ] Avaliar necessidade de extrair `IRabbitMqInfrastructureManager` para arquivo separado
- [ ] Adicionar integration events para módulos faltantes (quando houver necessidade)
- [ ] Criar testes unitários para classes de messaging (se coverage cair abaixo do threshold)

**Prioridade**: BAIXA  
**Estimativa**: 4-6 horas

---

### 🔧 Refatoração Extensions (MeAjudaAi.Shared)

**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: BACKLOG

**Problemas**:
1. **Extensions dentro de classes de implementação**: `BusinessMetricsMiddlewareExtensions` está dentro de `BusinessMetricsMiddleware.cs`
2. **Falta de consolidação**: Extensions espalhadas em múltiplos arquivos

**Ações Pendentes**:
- [ ] Extrair `BusinessMetricsMiddlewareExtensions` para arquivo próprio
- [ ] Criar arquivos consolidados: `MonitoringExtensions.cs`, `CachingExtensions.cs`, `MessagingExtensions.cs`, `AuthorizationExtensions.cs`
- [ ] Documentar padrão: cada funcionalidade tem seu `<Funcionalidade>Extensions.cs`

**Prioridade**: BAIXA  
**Estimativa**: 4-6 horas

---

## ⚠️ CRÍTICO: Hangfire + Npgsql 10.x Compatibility Risk

**Arquivo**: `Directory.Packages.props`  
**Situação**: MONITORAMENTO CONTÍNUO  
**Severidade**: MÉDIA (funciona em desenvolvimento, não validado em produção)  
**Status**: Sistema rodando com Npgsql 10.0 + Hangfire.PostgreSql 1.20.13

**Descrição**: 
Hangfire.PostgreSql 1.20.13 foi compilado contra Npgsql 6.x, mas o projeto está usando Npgsql 10.x (EF Core 10.0.2). A compatibilidade funciona em desenvolvimento mas não foi formalmente validada pelo mantenedor do Hangfire.

**Status Atual**:
- ✅ **Build**: Compila sem erros
- ✅ **Desenvolvimento**: Aplicação funciona normalmente
- ⚠️ **Produção**: Não validado com carga real

**Mitigação Implementada**:
1. ✅ Documentação detalhada em `Directory.Packages.props`
2. ✅ Health checks configurados
3. ✅ Procedimentos de rollback documentados
4. ⚠️ Monitoramento de produção pendente

**Ações Pendentes**:
- [ ] Validação em ambiente staging com carga similar a produção
- [ ] Monitoramento de taxa de falha de jobs (<5% threshold)
- [ ] Configuração de alertas para problemas Hangfire/Npgsql

**Fallback Strategies**:
1. **Downgrade para Npgsql 8.x** (se problemas detectados)
2. **Aguardar Hangfire.PostgreSql 2.x** (com suporte Npgsql 10)
3. **Backend alternativo** (Hangfire.Pro.Redis, Hangfire.SqlServer)

**Prioridade**: MÉDIA  
**Monitorar**: <https://github.com/frankhommers/Hangfire.PostgreSql/issues>

---

## 📦 Microsoft.OpenApi 2.3.0 - Bloqueio de Atualização

**Arquivo**: `Directory.Packages.props`  
**Situação**: BLOQUEADO - Incompatibilidade com ASP.NET Core Source Generators  
**Severidade**: BAIXA (funciona perfeitamente na versão atual)  
**Status**: Pinado em 2.3.0

**Descrição**:
Microsoft.OpenApi 3.x é incompatível com os source generators do ASP.NET Core 10.0. Erro confirmado em teste realizado em 16/01/2026 com SDK 10.0.102.

**Erro Encontrado**:
```text
error CS0200: Property or indexer 'IOpenApiMediaType.Example' cannot be assigned to -- it is read only
```

**Testes Realizados**:
- ✅ Testado com SDK 10.0.101 (Dez 2025) - incompatível
- ✅ Testado com SDK 10.0.102 (Jan 2026) - incompatível  
- ✅ Testado Microsoft.OpenApi 3.1.3 (16 Jan 2026) - build falha
- ✅ Confirmado que 2.3.0 funciona perfeitamente

**Causa Raiz**:
- Microsoft.OpenApi 3.x mudou `IOpenApiMediaType.Example` para read-only
- ASP.NET Core source generator ainda gera código que tenta escrever nessa propriedade
- Source generator não foi atualizado para API do OpenApi 3.x

**Decisão**: Manter Microsoft.OpenApi 2.3.0
- ✅ Funciona 100%
- ✅ Zero impacto em funcionalidades
- ✅ Swagger UI completo e funcional
- ⚠️ Versão desatualizada (mas estável)

**Monitoramento**:
- [ ] Verificar releases do .NET SDK para correções no source generator
- [ ] Testar Microsoft.OpenApi 3.x a cada atualização de SDK

**Prioridade**: BAIXA (não urgente, não afeta funcionalidade)  
**Monitorar**: <https://github.com/dotnet/aspnetcore/issues>

---

## 📋 Padronização de Records

**Arquivo**: Múltiplos arquivos em `src/Shared/Contracts/**` e `src/Modules/**/Domain/**`  
**Severidade**: MÉDIA (padronização importante)  
**Sprint**: Sprint 7.16 (Dia 5, ~0.5 dia)

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