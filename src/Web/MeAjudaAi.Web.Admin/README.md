# MeAjudaAi.Web.Admin

Portal administrativo Blazor WebAssembly para gerenciamento da plataforma MeAjudaAi.

## 📑 Índice

- [Quick Start](#-quick-start)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [State Management (Fluxor)](#-state-management-fluxor)
- [Sistema de Resiliência (Polly)](#-sistema-de-resiliência-polly)
- [Validação (FluentValidation)](#-validação-fluentvalidation)
- [Componentes MudBlazor](#-componentes-mudblazor)
- [Configuração](#-configuração)
- [Testes](#-testes)
- [Debugging](#-debugging)

---

## 🚀 Quick Start

### Pré-requisitos

- .NET 10 SDK
- Node.js (para Playwright tests)
- IDE: Visual Studio 2022+ ou VS Code com extensão C# Dev Kit

### Executar localmente

```bash
cd src/Web/MeAjudaAi.Web.Admin
dotnet restore
dotnet watch run  # Hot reload habilitado
```

Acesse: `https://localhost:5001`

### Build para produção

```bash
dotnet publish -c Release  # Com AOT compilation
```

**Documentação Completa:** [docs/modules/admin-portal.md](../../../docs/modules/admin-portal.md)

---

## 📦 Dependências Principais

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.0.1 | Blazor WASM runtime |
| `MudBlazor` | 8.15.0 | Material Design UI components |
| `Fluxor.Blazor.Web` | 6.9.0 | State management (Redux pattern) |
| `Refit` | 9.0.2 | Type-safe HTTP clients |
| `FluentValidation` | 11.0.0+ | Form validation com regras brasileiras |
| `Polly` | 8.0.0+ | Resilience (retry, circuit breaker, timeout) |

---

## 🏗️ Estrutura do Projeto

```
MeAjudaAi.Web.Admin/
├── Features/              # Fluxor stores (State + Actions + Reducers + Effects)
│   ├── Providers/
│   ├── Documents/
│   ├── ServiceCatalogs/
│   └── Errors/
├── Components/            # Componentes reutilizáveis
│   └── Dialogs/          # Modais (Create, Edit, Verify, etc)
├── Pages/                 # Páginas roteáveis (@page)
├── Services/              # Services (logging, resilience, permissions)
│   └── Resilience/       # Polly policies e handlers
├── Validators/            # FluentValidation validators
├── DTOs/                  # Data Transfer Objects
├── Constants/             # Constantes (status, tipos, etc)
├── Helpers/               # Helpers (acessibilidade, performance)
├── Layout/                # MainLayout, NavMenu
└── wwwroot/               # Assets estáticos (CSS, icons)
```

---

## 🔄 State Management (Fluxor)

O projeto usa **Fluxor** (implementação Redux para Blazor) com padrão unidirecional de dados.

### Anatomia de um Feature

```csharp
// 1. State (imutável)
public record ProvidersState
{
    public IReadOnlyList<ProviderDto> Items { get; init; } = [];
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}

// 2. Actions (eventos)
public record LoadProvidersAction(int Page = 1, int PageSize = 20);
public record LoadProvidersSuccessAction(PagedResult<ProviderDto> Result);
public record LoadProvidersFailureAction(string Error);

// 3. Reducers (transformações puras)
public static class ProvidersReducers
{
    [ReducerMethod]
    public static ProvidersState OnLoad(ProvidersState state, LoadProvidersAction _) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static ProvidersState OnSuccess(ProvidersState state, LoadProvidersSuccessAction action) =>
        state with { Items = action.Result.Items, IsLoading = false };
}

// 4. Effects (side effects assíncronos)
public class ProvidersEffects
{
    [EffectMethod]
    public async Task HandleLoad(LoadProvidersAction action, IDispatcher dispatcher)
    {
        var result = await _api.GetProvidersAsync(action.Page, action.PageSize);
        
        if (result.IsSuccess)
            dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value));
        else
            dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error.Message));
    }
}
```

### Uso em Componentes

```razor
@inject IState<ProvidersState> State
@inject IDispatcher Dispatcher

@if (State.Value.IsLoading)
{
    <MudProgressCircular Indeterminate="true" />
}
else
{
    @foreach (var provider in State.Value.Items)
    {
        <ProviderCard Provider="@provider" />
    }
}

@code {
    protected override void OnInitialized()
    {
        Dispatcher.Dispatch(new LoadProvidersAction());
    }
}
```

**Redux DevTools:** Extensão Chrome disponível em modo DEBUG para time-travel debugging.

---

## 🛡️ Sistema de Resiliência (Polly)

Todas as chamadas HTTP usam políticas Polly para garantir robustez contra falhas transitórias.

### Políticas Implementadas

1. **Retry Policy** (3 tentativas com backoff exponencial: 2s, 4s, 8s)
   - Erros HTTP 5xx, 408 (Timeout)
   
2. **Circuit Breaker** (abre após 5 falhas consecutivas, aguarda 30s)
   - Estados: `Closed` → `Open` → `Half-Open` → `Closed`
   - Previne sobrecarga do servidor
   
3. **Timeout Policy**
   - Operações normais: 30s
   - Uploads: 2min (sem retry para evitar duplicação)

### Indicador Visual de Status

O `ConnectionStatusIndicator.razor` no AppBar mostra:
- ✅ **Verde (Cloud Done)**: Conectado
- 🟡 **Amarelo (Cloud Sync)**: Reconectando
- 🔴 **Vermelho (Cloud Off)**: Sem conexão

### Uso em Effects

```csharp
[EffectMethod]
public async Task HandleLoad(LoadAction action, IDispatcher dispatcher)
{
    await dispatcher.ExecuteApiCallAsync(
        apiCall: () => _api.GetDataAsync(),
        snackbar: _snackbar,
        operationName: "Carregar dados",
        onSuccess: data => dispatcher.Dispatch(new LoadSuccessAction(data)),
        onError: ex => dispatcher.Dispatch(new LoadFailureAction(ex.Message))
    );
    // Retry, circuit breaker, timeout e notificações são automáticos
}
```

**Benefícios:**
- ✅ Auto-recuperação transparente
- ✅ Mensagens de erro amigáveis
- ✅ Logs detalhados para diagnóstico
- ✅ Proteção contra sobrecarga do servidor

---

## ✅ Validação (FluentValidation)

Validações client-side com regras específicas para dados brasileiros.

### Validadores Disponíveis

**Criar Provider:**
```csharp
public class CreateProviderRequestDtoValidator : AbstractValidator<CreateProviderRequestDto>
{
    public CreateProviderRequestDtoValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty()
            .ValidCpfOrCnpj();  // Valida checksum de CPF/CNPJ

        RuleFor(x => x.Email)
            .NotEmpty()
            .ValidEmail();

        RuleFor(x => x.Phone)
            .ValidBrazilianPhone();  // (00) 00000-0000 ou (00) 0000-0000

        RuleFor(x => x.Name)
            .NotEmpty()
            .NoXss();  // Previne XSS
    }
}
```

**Upload de Documentos:**
```csharp
public class UploadDocumentValidator : AbstractValidator<IBrowserFile>
{
    public UploadDocumentValidator()
    {
        RuleFor(x => x.Name)
            .ValidFileType(new[] { ".pdf", ".jpg", ".jpeg", ".png" })
            .NoXss();

        RuleFor(x => x.Size)
            .MaxFileSize(10 * 1024 * 1024);  // 10 MB

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedTypes.Contains(ct));
    }
}
```

### Extensions Reutilizáveis

```csharp
// Extensions/ValidationExtensions.cs
.ValidCpf()           // Valida CPF com dígitos verificadores
.ValidCnpj()          // Valida CNPJ com dígitos verificadores
.ValidCpfOrCnpj()     // Aceita CPF ou CNPJ
.ValidBrazilianPhone() // Valida telefone brasileiro
.ValidCep()           // Valida CEP (00000-000)
.NoXss()              // Remove HTML, scripts, event handlers
.SanitizeInput()      // Sanitiza string
.ValidFileType()      // Valida extensão de arquivo
.MaxFileSize()        // Valida tamanho de arquivo
```

### Uso em Formulários MudBlazor

```razor
@inject IValidator<CreateProviderRequestDto> Validator

<MudForm Model="@model" Validation="@(ValidateField)">
    <MudTextField @bind-Value="model.Name" 
                  For="@(() => model.Name)"
                  Label="Nome" />
    
    <MudTextField @bind-Value="model.Document" 
                  For="@(() => model.Document)"
                  Label="CPF/CNPJ" />
</MudForm>

@code {
    private CreateProviderRequestDto model = new();
    
    private IEnumerable<string> ValidateField(object value)
    {
        var result = Validator.Validate(model);
        return result.Errors.Select(e => e.ErrorMessage);
    }
}
```

---

## 🎨 Componentes MudBlazor

### MudDataGrid com Paginação Server-Side

```razor
<MudDataGrid T="ProviderDto" 
             ServerData="LoadServerData"
             Filterable="true" 
             SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Nome" />
        <PropertyColumn Property="x => x.VerificationStatus">
            <CellTemplate>
                <MudChip Color="@GetStatusColor(context.Item.VerificationStatus)">
                    @VerificationStatus.ToDisplayName(context.Item.VerificationStatus)
                </MudChip>
            </CellTemplate>
        </PropertyColumn>
    </Columns>
</MudDataGrid>
```

### MudDialog Reutilizável

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Criar Provider</MudText>
    </TitleContent>
    <DialogContent>
        <MudForm @ref="form" Model="@model">
            <!-- Campos -->
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancelar</MudButton>
        <MudButton Color="Color.Primary" OnClick="Submit">Salvar</MudButton>
    </DialogActions>
</MudDialog>
```

**Referência Completa:** [MudBlazor Components](https://mudblazor.com/components/list)

---

## ⚙️ Configuração

### appsettings.json (Produção)

```json
{
  "ApiBaseUrl": "https://api.meajudaai.com",
  "Keycloak": {
    "Authority": "https://auth.meajudaai.com/realms/meajudaai",
    "ClientId": "meajudaai-admin-portal",
    "ResponseType": "code"
  }
}
```

### appsettings.Development.json

```json
{
  "ApiBaseUrl": "https://localhost:7032",
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",
    "ClientId": "meajudaai-admin-portal-dev"
  }
}
```

**Nota:** API URL deve corresponder ao `AppHost` configurado em `src/Aspire/MeAjudaAi.AppHost/`.

---

## 🧪 Testes

### bUnit (Testes de Componentes)

```bash
dotnet new bunit -n MeAjudaAi.Web.Admin.Tests
dotnet test
```

### Playwright (Testes E2E)

```bash
dotnet add package Microsoft.Playwright
pwsh bin/Debug/net10.0/playwright.ps1 install
dotnet test --filter Category=E2E
```

**Cobertura Atual:** 43 testes bUnit (componentes, reducers, effects, services)

---

## 🐛 Debugging

### Redux DevTools

1. Instalar [extensão Chrome](https://chrome.google.com/webstore/detail/redux-devtools/)
2. Executar em modo DEBUG: `dotnet run --configuration Debug`
3. Abrir DevTools → Redux tab
4. Ver actions, state diffs, time-travel debugging

### Browser DevTools

- **Sources:** Definir breakpoints em arquivos `.razor` e `.cs`
- **Console:** Logs do aplicativo e erros JavaScript
- **Network:** Inspecionar requisições HTTP e respostas

---

## 📚 Documentação Adicional

- [Admin Portal - Arquitetura Completa](../../../docs/modules/admin-portal.md)
- [MudBlazor Components](https://mudblazor.com/components/list)
- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [Polly Documentation](https://www.pollydocs.org/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

---

## 🗺️ Roadmap

### ✅ Sprint 6 - Setup (CONCLUÍDO)
- ✅ Projeto Blazor WASM criado
- ✅ MudBlazor integrado
- ✅ Fluxor configurado
- ✅ Layout base (AppBar + Drawer + NavMenu)

### ✅ Sprint 7 - Features (CONCLUÍDO)
- ✅ CRUD completo de Providers
- ✅ Gestão de Documentos
- ✅ Catálogo de Serviços
- ✅ Dashboard com gráficos
- ✅ Sistema de Resiliência (Polly)
- ✅ FluentValidation integrado

### ✅ Sprint 7.16 - Technical Debt (CONCLUÍDO)
- ✅ Keycloak automation
- ✅ 0 warnings no build
- ✅ 43 testes bUnit
- ✅ Records padronizados

### ⏳ Sprint 8 - Customer App (22 Jan - 4 Fev 2026)
- [ ] Blazor WASM Customer App
- [ ] MAUI Hybrid Mobile App

---

**Última Atualização:** 17 de Janeiro de 2026  
**Status:** ✅ Production-ready (Admin Portal)
