# Flux Pattern Implementation - Web Admin

## Overview

Este documento descreve a implementação do padrão Flux (Redux) na aplicação Blazor WebAssembly Admin, utilizando a biblioteca Fluxor.

## Objetivo

Eliminar **mixed concerns** (preocupações misturadas) nos componentes Blazor, separando claramente:
- **Components**: Apenas renderização e dispatching de actions
- **Actions**: Comandos/eventos imutáveis
- **Effects**: Side effects (chamadas de API, I/O)
- **Reducers**: Transformações puras de estado
- **State**: Estado global imutável da aplicação

## Implementação Completa

### Páginas Refatoradas (5/5 - 100%)

Todas as páginas principais foram refatoradas seguindo o padrão Flux estrito:

1. **Providers** (Commit: b98bac98)
   - Delete operation com resiliência
   - Estado: `IsDeleting`, `DeletingProviderId`
   - Simplificação: 30+ linhas → 3 linhas

2. **Documents** (Commit: 152a22ca)
   - Delete e RequestVerification operations
   - Estado: `IsDeleting`, `DeletingDocumentId`, `IsRequestingVerification`, `VerifyingDocumentId`
   - Simplificação: Delete 20+ linhas → 3 linhas, Verify 15+ linhas → 3 linhas

3. **Categories** (Commit: 1afa2daa)
   - Delete e Toggle activation operations
   - Estado: `IsDeletingCategory`, `DeletingCategoryId`, `IsTogglingCategory`, `TogglingCategoryId`
   - Simplificação: Delete 15+ linhas → 3 linhas, Toggle 12+ linhas → 1 linha

4. **Services** (Commit: 399ee25b)
   - Delete e Toggle activation operations
   - Estado: `IsDeletingService`, `DeletingServiceId`, `IsTogglingService`, `TogglingServiceId`
   - Simplificação: Delete 15+ linhas → 3 linhas, Toggle 12+ linhas → 1 linha

5. **AllowedCities** (Commit: 9ee405e0)
   - Delete e Toggle activation operations
   - Estado: `IsDeletingCity`, `DeletingCityId`, `IsTogglingCity`, `TogglingCityId`
   - Simplificação: Delete 15+ linhas → 3 linhas, Toggle 20+ linhas → 3 linhas

### Dialogs (Decisão Arquitetural)

Os dialogs de Create/Edit foram **intencionalmente mantidos** com chamadas diretas de API por razões pragmáticas:

**Justificativa:**
- Dialogs são componentes efêmeros (abrem e fecham)
- Não precisam de estado global persistente
- Complexidade de formulários (validações, múltiplos campos)
- Princípio YAGNI (You Aren't Gonna Need It)

**Dialogs afetados:**
- CreateProviderDialog, EditProviderDialog, VerifyProviderDialog
- CreateCategoryDialog, EditCategoryDialog
- CreateServiceDialog, EditServiceDialog
- CreateAllowedCityDialog, EditAllowedCityDialog
- UploadDocumentDialog

**Padrão atual (funcional):**
1. Dialog faz validação e chamada de API localmente
2. Dialog fecha com `DialogResult.Ok(true)`
3. Página principal dispara `Dispatcher.Dispatch(new Load...Action())` para recarregar

Este padrão é aceitável pois:
- ✅ Separação clara entre dialog (formulário) e página (listagem)
- ✅ Página principal mantém controle do fluxo
- ✅ Estado global não é poluído com estados de formulários temporários

## Padrão Flux - Fluxo de Dados

```
┌─────────────┐
│  Component  │ ← Renderiza estado
└──────┬──────┘
       │ Dispatch Action
       ▼
┌─────────────┐
│   Action    │ (Comando imutável)
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Effect    │ → API Call (com resiliência)
└──────┬──────┘
       │ Dispatch Success/Failure
       ▼
┌─────────────┐
│   Reducer   │ (Função pura)
└──────┬──────┘
       │ Retorna novo estado
       ▼
┌─────────────┐
│    State    │ (Imutável)
└──────┬──────┘
       │ Notifica componentes
       └──────────────────────┐
                              │
                              ▼
                       ┌─────────────┐
                       │  Component  │ (Re-renderiza)
                       └─────────────┘
```

## Anatomia de uma Feature

Exemplo: Providers Delete

### 1. Actions (ProvidersActions.cs)

```csharp
public record DeleteProviderAction(Guid ProviderId);
public record DeleteProviderSuccessAction(Guid ProviderId);
public record DeleteProviderFailureAction(Guid ProviderId, string ErrorMessage);
```

### 2. State (ProvidersState.cs)

```csharp
[FeatureState]
public sealed record ProvidersState
{
    public bool IsDeleting { get; init; }
    public Guid? DeletingProviderId { get; init; }
    // ... outros campos
}
```

### 3. Effects (ProvidersEffects.cs)

```csharp
[EffectMethod]
public async Task HandleDeleteProviderAction(DeleteProviderAction action, IDispatcher dispatcher)
{
    await dispatcher.ExecuteApiCallAsync(
        apiCall: () => _providersApi.DeleteProviderAsync(action.ProviderId),
        snackbar: _snackbar,
        operationName: "Deletar provedor",
        onSuccess: _ => {
            dispatcher.Dispatch(new DeleteProviderSuccessAction(action.ProviderId));
            _snackbar.Add("Provedor excluído com sucesso!", Severity.Success);
            dispatcher.Dispatch(new LoadProvidersAction());
        },
        onError: ex => {
            dispatcher.Dispatch(new DeleteProviderFailureAction(action.ProviderId, ex.Message));
        });
}
```

**Nota:** `ExecuteApiCallAsync` é uma extension que adiciona automaticamente:
- Retry (3 tentativas com backoff exponencial)
- Circuit Breaker (5 falhas em 30s abre circuito por 30s)
- Logging centralizado
- Tratamento de erros consistente

### 4. Reducers (ProvidersReducers.cs)

```csharp
[ReducerMethod]
public static ProvidersState ReduceDeleteProviderAction(ProvidersState state, DeleteProviderAction action)
    => state with 
    { 
        IsDeleting = true, 
        DeletingProviderId = action.ProviderId,
        ErrorMessage = null 
    };

[ReducerMethod]
public static ProvidersState ReduceDeleteProviderSuccessAction(ProvidersState state, DeleteProviderSuccessAction _)
    => state with 
    { 
        IsDeleting = false, 
        DeletingProviderId = null,
        ErrorMessage = null 
    };

[ReducerMethod]
public static ProvidersState ReduceDeleteProviderFailureAction(ProvidersState state, DeleteProviderFailureAction action)
    => state with 
    { 
        IsDeleting = false, 
        DeletingProviderId = null,
        ErrorMessage = action.ErrorMessage 
    };
```

### 5. Component (Providers.razor)

**ANTES (Anti-pattern):**
```csharp
@inject IProvidersApi ProvidersApi
@inject ISnackbar Snackbar

private async Task DeleteProvider(Guid providerId)
{
    try 
    {
        var result = await ProvidersApi.DeleteProviderAsync(providerId);
        if (result.IsSuccess) 
        {
            Snackbar.Add("Sucesso!", Severity.Success);
            Dispatcher.Dispatch(new LoadProvidersAction());
        } 
        else 
        {
            Logger.LogError("Failed: {Error}", result.Error);
            Snackbar.Add("Erro ao deletar", Severity.Error);
        }
    } 
    catch (Exception ex) 
    {
        Logger.LogError(ex, "Exception deleting provider");
        Snackbar.Add("Erro inesperado", Severity.Error);
    }
}
```

**DEPOIS (Flux pattern):**
```csharp
@inject IDispatcher Dispatcher

private async Task OpenDeleteDialog(Guid providerId)
{
    var result = await DialogService.ShowMessageBox(
        "Confirmar Exclusão",
        "Tem certeza que deseja excluir este provedor?",
        yesText: "Excluir", cancelText: "Cancelar");

    if (result == true)
    {
        Dispatcher.Dispatch(new DeleteProviderAction(providerId));
    }
}
```

**Template com estado disabled:**
```html
<MudIconButton Icon="@Icons.Material.Filled.Delete" 
               Color="Color.Error" 
               OnClick="@(() => OpenDeleteDialog(context.Item.Id))" 
               Disabled="@(ProvidersState.Value.IsDeleting && 
                          ProvidersState.Value.DeletingProviderId == context.Item.Id)" />
```

## Benefícios Alcançados

### 1. **Separation of Concerns**
- ✅ Components apenas renderizam e dispatcham
- ✅ Effects isolam side effects
- ✅ Reducers são funções puras e testáveis
- ✅ State é imutável e previsível

### 2. **Resiliência Centralizada**
- ✅ Retry automático em todos os Effects
- ✅ Circuit Breaker para proteção contra falhas em cascata
- ✅ Logging consistente
- ✅ Tratamento de erros padronizado

### 3. **UI/UX Melhorado**
- ✅ Botões desabilitados durante operações (previne duplicação)
- ✅ Loading states visuais
- ✅ Feedback consistente via Snackbar
- ✅ Estado da UI sempre sincronizado

### 4. **Testabilidade**
- ✅ Reducers puros são 100% testáveis
- ✅ Effects podem ser mockados facilmente
- ✅ Actions são imutáveis e serializáveis
- ✅ State é previsível

### 5. **Manutenibilidade**
- ✅ Redução de código: média de 85% menos linhas
- ✅ Lógica centralizada
- ✅ Fácil adicionar novas operações
- ✅ Padrão consistente em toda aplicação

## Métricas de Impacto

| Feature | Antes | Depois | Redução |
|---------|-------|--------|---------|
| Delete Provider | 30+ linhas | 3 linhas | 90% |
| Delete Document | 20+ linhas | 3 linhas | 85% |
| Verify Document | 15+ linhas | 3 linhas | 80% |
| Toggle Category | 12+ linhas | 1 linha | 92% |
| Toggle Service | 12+ linhas | 1 linha | 92% |
| Toggle City | 20+ linhas | 3 linhas | 85% |

**Total:** Aproximadamente **87% de redução** no código dos componentes.

## Guia Rápido: Adicionar Nova Operação

### Passo 1: Criar Actions
```csharp
// Features/MeuModulo/MeuModuloActions.cs
public record MinhaOperacaoAction(Guid Id);
public record MinhaOperacaoSuccessAction(Guid Id);
public record MinhaOperacaoFailureAction(Guid Id, string ErrorMessage);
```

### Passo 2: Atualizar State
```csharp
// Features/MeuModulo/MeuModuloState.cs
public bool IsExecutingOperacao { get; init; }
public Guid? OperacaoItemId { get; init; }
```

### Passo 3: Criar Effect
```csharp
// Features/MeuModulo/MeuModuloEffects.cs
[EffectMethod]
public async Task HandleMinhaOperacaoAction(MinhaOperacaoAction action, IDispatcher dispatcher)
{
    await dispatcher.ExecuteApiCallAsync(
        apiCall: () => _api.MinhaOperacaoAsync(action.Id),
        snackbar: _snackbar,
        operationName: "Minha Operação",
        onSuccess: _ => {
            dispatcher.Dispatch(new MinhaOperacaoSuccessAction(action.Id));
            _snackbar.Add("Sucesso!", Severity.Success);
        },
        onError: ex => {
            dispatcher.Dispatch(new MinhaOperacaoFailureAction(action.Id, ex.Message));
        });
}
```

### Passo 4: Criar Reducers
```csharp
// Features/MeuModulo/MeuModuloReducers.cs
[ReducerMethod]
public static MeuModuloState ReduceMinhaOperacaoAction(MeuModuloState state, MinhaOperacaoAction action)
    => state with { IsExecutingOperacao = true, OperacaoItemId = action.Id };

[ReducerMethod]
public static MeuModuloState ReduceMinhaOperacaoSuccessAction(MeuModuloState state, MinhaOperacaoSuccessAction _)
    => state with { IsExecutingOperacao = false, OperacaoItemId = null };

[ReducerMethod]
public static MeuModuloState ReduceMinhaOperacaoFailureAction(MeuModuloState state, MinhaOperacaoFailureAction action)
    => state with { IsExecutingOperacao = false, OperacaoItemId = null, ErrorMessage = action.ErrorMessage };
```

### Passo 5: Usar no Component
```csharp
// Pages/MeuModulo.razor
<MudIconButton 
    OnClick="@(() => Dispatcher.Dispatch(new MinhaOperacaoAction(item.Id)))"
    Disabled="@(MeuModuloState.Value.IsExecutingOperacao && 
               MeuModuloState.Value.OperacaoItemId == item.Id)" />
```

## Padrões e Convenções

### Nomenclatura

- **Actions:** Verbos no presente: `DeleteProviderAction`, `ToggleCategoryActivationAction`
- **Success:** Adicionar `Success` ao final: `DeleteProviderSuccessAction`
- **Failure:** Adicionar `Failure` + `ErrorMessage`: `DeleteProviderFailureAction`
- **State fields:** Usar `Is` + `Verbo`+`Gerundio`: `IsDeleting`, `IsToggling`
- **ID tracking:** Usar `Verbo`+`Gerundio` + `ItemId`: `DeletingProviderId`, `TogglingCategoryId`

### Estrutura de Arquivos

```
Features/
├── MeuModulo/
│   ├── MeuModuloActions.cs      # Todas as actions
│   ├── MeuModuloState.cs        # Estado imutável
│   ├── MeuModuloEffects.cs      # Side effects (API calls)
│   └── MeuModuloReducers.cs     # Transformações puras
```

### Imutabilidade

Sempre usar `record` com `init`:
```csharp
public sealed record MeuState
{
    public bool IsLoading { get; init; }  // ✅ Correto
    public int Counter { get; set; }      // ❌ Errado!
}
```

### Effects com Resiliência

Sempre usar `ExecuteApiCallAsync` para chamadas de API:
```csharp
await dispatcher.ExecuteApiCallAsync(
    apiCall: () => _api.Operation(),
    snackbar: _snackbar,
    operationName: "Nome da Operação",
    onSuccess: _ => { /* ... */ },
    onError: ex => { /* ... */ }
);
```

## Referências

- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [Redux Pattern](https://redux.js.org/tutorials/fundamentals/part-1-overview)
- [Flux Architecture](https://facebook.github.io/flux/docs/in-depth-overview)

## Histórico de Implementação

| Data | Commit | Descrição |
|------|--------|-----------|
| 2026-01-16 | b98bac98 | Providers Delete operation |
| 2026-01-16 | 152a22ca | Documents Delete & Verify operations |
| 2026-01-16 | 1afa2daa | Categories Delete & Toggle operations |
| 2026-01-16 | 399ee25b | Services Delete & Toggle operations |
| 2026-01-16 | 9ee405e0 | AllowedCities Delete & Toggle operations |

---

**Status:** ✅ Implementação completa para todas as páginas principais  
**Cobertura:** 5/5 páginas (100%)  
**Decisão:** Dialogs mantidos com padrão pragmático  
**Próximos passos:** Adicionar unit tests para Effects e Reducers
