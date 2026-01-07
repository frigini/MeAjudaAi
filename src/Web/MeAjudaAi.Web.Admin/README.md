# MeAjudaAi.Web.Admin

Portal administrativo Blazor WebAssembly para gerenciamento da plataforma MeAjudaAi.

## 🚀 Quick Start

### Pré-requisitos

- .NET 10 SDK
- Node.js (para Playwright tests)
- IDE: Visual Studio 2022+ ou VS Code com extensão C# Dev Kit

### Executar localmente

```bash
# 1. Navegar para o diretório do projeto
cd src/Web/MeAjudaAi.Web.Admin

# 2. Restaurar dependências
dotnet restore

# 3. Executar (Development Server)
dotnet run

# Ou usar o watch mode para hot reload
dotnet watch run
```

Acesse: `https://localhost:5001` (porta pode variar)

### Build para produção

```bash
# Build Release
dotnet build -c Release

# Build com AOT Compilation (mais lento, melhor performance)
dotnet publish -c Release

# Output: bin/Release/net10.0/publish/wwwroot/
```

## 📦 Dependências

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.0.1 | Blazor WASM runtime |
| `Microsoft.AspNetCore.Components.WebAssembly.Authentication` | 10.0.1 | OIDC authentication |
| `MudBlazor` | 8.0.0+ | Material Design UI |
| `Fluxor.Blazor.Web` | 6.1.0 | State management |
| `Fluxor.Blazor.Web.ReduxDevTools` | 6.1.0 | Redux DevTools (DEBUG only) |
| `Refit.HttpClientFactory` | 9.0.2 | HTTP client generation |

## 🏗️ Estrutura do Projeto

```plaintext
MeAjudaAi.Web.Admin/
├── Pages/                          # Páginas roteáveis (@page)
│   ├── Home.razor                 # Dashboard com KPIs
│   ├── Providers.razor            # Listagem/CRUD de providers
│   ├── Documents.razor            # Gerenciamento de documentos
│   ├── Services.razor             # Catálogo de serviços
│   ├── Settings.razor             # Configurações do sistema
│   ├── Counter.razor              # Template example (remover)
│   ├── Weather.razor              # Template example (remover)
│   └── NotFound.razor             # Página 404
│
├── Layout/                         # Layouts compartilhados
│   ├── MainLayout.razor           # Layout principal (AppBar + Drawer)
│   ├── MainLayout.razor.css       # Estilos do layout
│   ├── NavMenu.razor              # Menu lateral de navegação
│   └── NavMenu.razor.css          # Estilos do menu
│
├── Features/                       # Fluxor stores (PLANEJADO)
│   ├── Providers/
│   │   ├── ProvidersState.cs
│   │   ├── ProvidersActions.cs
│   │   ├── ProvidersReducers.cs
│   │   └── ProvidersEffects.cs
│   └── Dashboard/
│
├── Components/                     # Componentes reutilizáveis (PLANEJADO)
│   ├── ProviderCard.razor
│   ├── DocumentUploader.razor
│   └── KpiCard.razor
│
├── wwwroot/                        # Assets estáticos
│   ├── css/
│   │   └── app.css                # Estilos globais
│   ├── lib/                       # Bibliotecas JavaScript (Bootstrap - remover)
│   ├── favicon.png                # Favicon
│   ├── icon-192.png               # PWA icon
│   └── index.html                 # HTML host page
│
├── App.razor                       # Componente raiz (Router + Providers)
├── _Imports.razor                  # Global using statements
├── Program.cs                      # Entry point + DI configuration
└── MeAjudaAi.Web.Admin.csproj     # Project file
```

## 🎨 Componentes MudBlazor

### Exemplo: MudDataGrid com Paginação

```razor
@page "/providers"
@inject IProvidersApi ProvidersApi

<MudDataGrid T="ModuleProviderDto" 
             ServerData="LoadServerData"
             Filterable="true" 
             SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Nome" />
        <PropertyColumn Property="x => x.Email" Title="Email" />
        <PropertyColumn Property="x => x.VerificationStatus" Title="Status">
            <CellTemplate>
                <MudChip Color="GetStatusColor(context.Item.VerificationStatus)">
                    @context.Item.VerificationStatus
                </MudChip>
            </CellTemplate>
        </PropertyColumn>
        <TemplateColumn Title="Ações" Sortable="false">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                               Size="Size.Small"
                               OnClick="@(() => ViewDetails(context.Item.Id))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>

@code {
    private async Task<GridData<ModuleProviderDto>> LoadServerData(GridState<ModuleProviderDto> state)
    {
        var result = await ProvidersApi.GetProvidersAsync(
            state.Page + 1, 
            state.PageSize);

        if (result.IsSuccess)
        {
            return new GridData<ModuleProviderDto>
            {
                Items = result.Value.Items,
                TotalItems = result.Value.TotalItems
            };
        }

        return new GridData<ModuleProviderDto>();
    }
}
```

## 🔄 State Management com Fluxor

### 1. Definir State

```csharp
// Features/Providers/ProvidersState.cs
public record ProvidersState
{
    public IReadOnlyList<ModuleProviderDto> Providers { get; init; } = [];
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 2. Definir Actions

```csharp
// Features/Providers/ProvidersActions.cs
public record LoadProvidersAction(int PageNumber = 1, int PageSize = 20);
public record LoadProvidersSuccessAction(PagedResult<ModuleProviderDto> Result);
public record LoadProvidersFailureAction(string ErrorMessage);
```

### 3. Definir Reducers

```csharp
// Features/Providers/ProvidersReducers.cs
public static class ProvidersReducers
{
    [ReducerMethod]
    public static ProvidersState Reduce(ProvidersState state, LoadProvidersAction action) =>
        state with { IsLoading = true };

    [ReducerMethod]
    public static ProvidersState Reduce(ProvidersState state, LoadProvidersSuccessAction action) =>
        state with 
        { 
            Providers = action.Result.Items,
            IsLoading = false,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ProvidersState Reduce(ProvidersState state, LoadProvidersFailureAction action) =>
        state with 
        { 
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
}
```

### 4. Definir Effects (side effects)

```csharp
// Features/Providers/ProvidersEffects.cs
public class ProvidersEffects
{
    private readonly IProvidersApi _api;

    public ProvidersEffects(IProvidersApi api)
    {
        _api = api;
    }

    [EffectMethod]
    public async Task HandleLoadProviders(LoadProvidersAction action, IDispatcher dispatcher)
    {
        var result = await _api.GetProvidersAsync(action.PageNumber, action.PageSize);

        if (result.IsSuccess)
        {
            dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value));
        }
        else
        {
            dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error.Message));
        }
    }
}
```

### 5. Usar no componente

```razor
@inject IState<ProvidersState> ProvidersState
@inject IDispatcher Dispatcher

@if (ProvidersState.Value.IsLoading)
{
    <MudProgressCircular Indeterminate="true" />
}
else if (!string.IsNullOrEmpty(ProvidersState.Value.ErrorMessage))
{
    <MudAlert Severity="Severity.Error">@ProvidersState.Value.ErrorMessage</MudAlert>
}
else
{
    @foreach (var provider in ProvidersState.Value.Providers)
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

## 🧪 Testes

### bUnit - Testes de Componentes

```bash
# Criar projeto de testes
dotnet new bunit -n MeAjudaAi.Web.Admin.Tests

# Adicionar referência
dotnet add reference ../MeAjudaAi.Web.Admin/MeAjudaAi.Web.Admin.csproj

# Executar testes
dotnet test
```

### Playwright - Testes E2E

```bash
# Instalar Playwright
dotnet add package Microsoft.Playwright
pwsh bin/Debug/net10.0/playwright.ps1 install

# Executar testes E2E
dotnet test --filter Category=E2E
```

## 📝 Configuração

### appsettings.json

> **Note**: Production Keycloak uses `auth.meajudaai.com` as the canonical domain (not `keycloak.meajudaai.com`).

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

> **Note**: Development API URL must match AppHost configuration (see `src/Aspire/MeAjudaAi.AppHost/appsettings.Development.json`).

```json
{
  "ApiBaseUrl": "https://localhost:7032",
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",
    "ClientId": "meajudaai-admin-portal-dev"
  }
}
```

## 🐛 Debugging

### Redux DevTools

Fluxor integra com [Redux DevTools](https://chrome.google.com/webstore/detail/redux-devtools/):

1. Instalar extensão do Chrome
2. Executar app em modo DEBUG
3. Abrir DevTools → Redux tab
4. Ver actions, state diffs, time-travel debugging

### DevTools do Navegador

```bash
# Executar com debugging habilitado
dotnet run --configuration Debug

# Abrir Chrome DevTools (F12)
# Sources → Definir breakpoints em arquivos .razor/.cs
```

## 📚 Documentação Adicional

- [Admin Portal - Documentação Completa](../../docs/modules/admin-portal.md)
- [MudBlazor Components](https://mudblazor.com/components/list)
- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

## 🗺️ Roadmap

### ✅ Sprint 6 - Week 1 (COMPLETED)
- ✅ Criar projeto Blazor WASM
- ✅ Integrar MudBlazor UI library
- ✅ Configurar Fluxor state management
- ✅ Criar layout base (AppBar + Drawer)
- ✅ Criar páginas placeholder (Providers, Documents, Services, Settings)

### 🔄 Sprint 6 - Week 2 (IN PROGRESS)
- [ ] Implementar Fluxor stores (Providers, Dashboard)
- [ ] Configurar Keycloak OIDC authentication
- [ ] Criar Dashboard com KPIs (total providers, pending verifications, etc.)
- [ ] Implementar Providers list com MudDataGrid

### ⏳ Sprint 6 - Week 3 (PLANNED)
- [ ] Testes bUnit para componentes
- [ ] Testes E2E Playwright
- [ ] Documentação Storybook-like para componentes
