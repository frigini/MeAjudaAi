# Admin Portal - Blazor WebAssembly

Portal administrativo do MeAjudaAi construído com Blazor WebAssembly (.NET 10).

## 📋 Visão Geral

O Admin Portal é uma aplicação Single Page Application (SPA) que permite aos administradores gerenciar:
- **Providers**: Cadastro, verificação e aprovação de prestadores de serviço
- **Documents**: Validação de documentos (RG, CNH, CNPJ) via Azure Document Intelligence
- **Services**: Catálogo de serviços oferecidos
- **Settings**: Configurações do sistema e preferências

## 🏗️ Arquitetura

### Stack Tecnológico

| Componente | Tecnologia | Versão | Propósito |
|------------|-----------|--------|-----------|
| **Framework** | Blazor WebAssembly | .NET 10 | SPA no browser (sem servidor ASP.NET Core) |
| **UI Library** | MudBlazor | 7.21.0+ | Material Design components |
| **State Management** | Fluxor | 6.1.0+ | Flux/Redux pattern (previsível, testável) |
| **API Client** | Refit | 9.0.2+ | HTTP client tipado (geração automática) |
| **Autenticação** | OIDC/Keycloak | - | OpenID Connect via Keycloak |
| **Validação** | FluentValidation | Shared | Validadores compartilhados backend/frontend |
| **Testes Componentes** | bUnit | - | Unit tests de componentes Blazor |
| **Testes E2E** | Playwright | - | Testes end-to-end |

### Estrutura de Projetos

```plaintext
src/
├── Web/
│   └── MeAjudaAi.Web.Admin/              # Blazor WASM App
│       ├── Pages/                        # Rotas e páginas
│       │   ├── Home.razor               # Dashboard (KPIs)
│       │   ├── Providers.razor          # Lista/CRUD providers
│       │   ├── Documents.razor          # Gerenciamento documentos
│       │   ├── Services.razor           # Catálogo de serviços
│       │   └── Settings.razor           # Configurações
│       ├── Layout/                      # Layouts compartilhados
│       │   ├── MainLayout.razor         # Layout principal (AppBar + Drawer)
│       │   └── NavMenu.razor            # Menu de navegação
│       ├── Features/                    # Fluxor stores (PLANEJADO Sprint 6.2)
│       │   ├── Providers/
│       │   │   ├── ProvidersState.cs
│       │   │   ├── ProvidersActions.cs
│       │   │   ├── ProvidersReducers.cs
│       │   │   └── ProvidersEffects.cs
│       │   └── Dashboard/
│       ├── Components/                  # Componentes reutilizáveis (PLANEJADO)
│       ├── Services/                    # Services e helpers
│       ├── wwwroot/                     # Assets estáticos
│       └── Program.cs                   # Configuração DI
│
├── Client/
│   └── MeAjudaAi.Client.Contracts/      # Refit interfaces
│       ├── Api/
│       │   └── IProvidersApi.cs         # Endpoints REST documentados
│       └── README.md
│
└── Shared/
    └── MeAjudaAi.Shared.Contracts/      # DTOs portáveis
        ├── Contracts/Modules/           # Contratos dos módulos
        ├── Functional/                  # Result pattern
        └── README.md
```

## 🎨 Padrões de Design

### 1. Flux/Redux com Fluxor

**State Management centralizado** para previsibilidade e testabilidade.

```csharp
// State: Estado imutável
public record ProvidersState
{
    public IReadOnlyList<ModuleProviderDto> Providers { get; init; } = [];
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
}

// Actions: Eventos que descrevem mudanças
public record LoadProvidersAction(int PageNumber, int PageSize);
public record LoadProvidersSuccessAction(PagedResult<ModuleProviderDto> Result);
public record LoadProvidersFailureAction(string ErrorMessage);

// Reducers: Funções puras que atualizam o state
public static ProvidersState Reduce(ProvidersState state, LoadProvidersSuccessAction action) =>
    state with 
    { 
        Providers = action.Result.Items,
        IsLoading = false,
        CurrentPage = action.Result.PageNumber,
        TotalPages = action.Result.TotalPages
    };

// Effects: Side effects (API calls, etc.)
public class LoadProvidersEffect : Effect<LoadProvidersAction>
{
    private readonly IProvidersApi _api;

    public override async Task HandleAsync(LoadProvidersAction action, IDispatcher dispatcher)
    {
        var result = await _api.GetProvidersAsync(action.PageNumber, action.PageSize);
        
        if (result.IsSuccess)
            dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value));
        else
            dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error.Message));
    }
}
```

### 2. Result Pattern

**Tratamento funcional de erros** sem exceptions.

```csharp
// API retorna Result<T>
var result = await providersApi.GetProviderByIdAsync(id);

// Pattern matching para tratamento
var message = result switch
{
    { IsSuccess: true, Value: var provider } => $"Provider: {provider.Name}",
    { IsFailure: true, Error: var error } => $"Error: {error.Message}",
    _ => "Unknown state"
};

// Ou usando métodos
if (result.IsSuccess)
{
    var provider = result.Value;
    // usar provider
}
else
{
    var error = result.Error;
    Snackbar.Add(error.Message, Severity.Error);
}
```

### 3. Component Communication

**Cascading Parameters** para compartilhar estado sem prop drilling.

```razor
@* App.razor *@
<CascadingValue Value="@_userContext">
    <Router AppAssembly="@typeof(App).Assembly">
        ...
    </Router>
</CascadingValue>

@* Componente filho *@
[CascadingParameter]
public UserContext UserContext { get; set; }
```

## 🔐 Autenticação e Autorização

### OpenID Connect via Keycloak

**Configuração** (Program.cs):

```csharp
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.ProviderOptions.Authority = "https://auth.meajudaai.com/realms/meajudaai";
    options.ProviderOptions.ClientId = "meajudaai-admin-portal";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
});
```

**Uso em páginas**:

```razor
@page "/providers"
@attribute [Authorize(Roles = "Admin,SuperAdmin")]

@* Apenas usuários autenticados com roles Admin ou SuperAdmin *@
```

**Injetar contexto de autenticação**:

```razor
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private async Task<string> GetUserNameAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.Identity?.Name ?? "Anonymous";
    }
}
```

## 🧪 Testes

### bUnit - Testes de Componentes

```csharp
public class ProvidersPageTests : TestContext
{
    [Fact]
    public void ProvidersPage_ShouldRenderProvidersList()
    {
        // Arrange
        var mockApi = Substitute.For<IProvidersApi>();
        mockApi.GetProvidersAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(Result.Success(new PagedResult<ModuleProviderDto> { ... }));
        
        Services.AddSingleton(mockApi);
        Services.AddFluxor(o => o.ScanAssemblies(typeof(ProvidersState).Assembly));

        // Act
        var cut = RenderComponent<Providers>();

        // Assert
        cut.Find("h3").TextContent.Should().Be("Providers");
        cut.FindAll(".provider-card").Should().HaveCount(5);
    }
}
```

### Playwright - Testes E2E

```csharp
[Test]
public async Task AdminCanViewProvidersList()
{
    // Arrange
    await LoginAsAdmin();
    
    // Act
    await Page.GotoAsync("https://localhost:5001/providers");
    await Page.WaitForSelectorAsync(".provider-card");
    
    // Assert
    var providers = await Page.QuerySelectorAllAsync(".provider-card");
    Assert.That(providers.Count, Is.GreaterThan(0));
    
    await Page.ScreenshotAsync(new() { Path = "providers-list.png" });
}
```

## 🎨 MudBlazor - Guia de Estilo

### Componentes Comuns

#### 1. MudDataGrid - Tabelas com Paginação

```razor
<MudDataGrid T="ModuleProviderDto" 
             Items="@_providers" 
             Filterable="true" 
             SortMode="SortMode.Multiple"
             Pagination="true"
             RowsPerPage="20">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Email" Title="Email" />
        <PropertyColumn Property="x => x.VerificationStatus" Title="Status" />
        <TemplateColumn Title="Actions">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                               OnClick="@(() => ViewProvider(context.Item.Id))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```

#### 2. MudDialog - Modais

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Confirm Action</MudText>
    </TitleContent>
    <DialogContent>
        <MudText>Are you sure you want to approve this provider?</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled" OnClick="Confirm">
            Confirm
        </MudButton>
    </DialogActions>
</MudDialog>
```

#### 3. MudForm + FluentValidation

```razor
<MudForm Model="@_provider" @ref="_form" Validation="@(_validator.ValidateValue)">
    <MudTextField @bind-Value="_provider.Name" 
                  Label="Name" 
                  For="@(() => _provider.Name)" />
    
    <MudTextField @bind-Value="_provider.Email" 
                  Label="Email" 
                  For="@(() => _provider.Email)" />
    
    <MudButton OnClick="Submit" Color="Color.Primary">Submit</MudButton>
</MudForm>

@code {
    private ProviderValidator _validator = new();
    
    private async Task Submit()
    {
        await _form.Validate();
        if (_form.IsValid)
        {
            // Submit
        }
    }
}
```

## 📱 Responsividade

MudBlazor usa **breakpoints** do Material Design:

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        @* xs: mobile, sm: tablet, md: desktop, lg: large desktop *@
        <MudCard>Content</MudCard>
    </MudItem>
</MudGrid>
```

**Breakpoints**:
- `xs`: 0-600px (mobile)
- `sm`: 600-960px (tablet)
- `md`: 960-1280px (desktop)
- `lg`: 1280-1920px (large desktop)
- `xl`: 1920px+ (extra large)

## 🚀 Performance

### 1. Virtualization

Para listas grandes, use `Virtualize`:

```razor
<Virtualize Items="@_providers" Context="provider">
    <ProviderCard Provider="@provider" />
</Virtualize>
```

### 2. Lazy Loading

```razor
@page "/providers"

<PageTitle>Providers</PageTitle>

@if (_providers == null)
{
    <MudProgressCircular Indeterminate="true" />
}
else
{
    <MudDataGrid Items="@_providers" ... />
}

@code {
    private IReadOnlyList<ModuleProviderDto>? _providers;

    protected override async Task OnInitializedAsync()
    {
        _providers = await LoadProvidersAsync();
    }
}
```

### 3. AOT Compilation

Blazor WASM com **Ahead-of-Time compilation** para melhor performance:

```xml
<PropertyGroup>
    <RunAOTCompilation>true</RunAOTCompilation>
</PropertyGroup>
```

**Trade-offs**:
- ✅ Performance em runtime (+30-50% mais rápido)
- ✅ Menor uso de memória
- ❌ Build time maior (5-10x mais lento)
- ❌ Tamanho do bundle maior (~2MB+)

## 🔧 Configuração

### appsettings.json

```json
{
  "ApiBaseUrl": "https://api.meajudaai.com",
  "Keycloak": {
    "Authority": "https://auth.meajudaai.com/realms/meajudaai",
    "ClientId": "meajudaai-admin-portal",
    "ResponseType": "code"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Program.cs - Dependency Injection

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress) 
});

// MudBlazor
builder.Services.AddMudServices();

// Fluxor State Management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
#if DEBUG
    options.UseReduxDevTools();
#endif
});

// Refit API Clients
builder.Services.AddRefitClient<IProvidersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

await builder.Build().RunAsync();
```

## 📚 Referências

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [MudBlazor Components](https://mudblazor.com/components/list)
- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [Refit Documentation](https://github.com/reactiveui/refit)
- [bUnit Documentation](https://bunit.dev/)
- [Playwright .NET](https://playwright.dev/dotnet/)

## 🗺️ Roadmap

### Sprint 6 - Week 2 (6-10 Jan 2026)
- [ ] Implementar Fluxor stores (Providers, Dashboard)
- [ ] Configurar Keycloak OIDC
- [ ] Criar Dashboard com KPIs
- [ ] Implementar lista de Providers com MudDataGrid

### Sprint 6 - Week 3 (13-17 Jan 2026)
- [ ] Testes bUnit para componentes principais
- [ ] Testes E2E Playwright (login → providers list)
- [ ] Documentação de componentes (Storybook-like)

### Sprint 7 - Funcionalidades Avançadas
- [ ] Real-time updates (SignalR)
- [ ] Offline support (PWA)
- [ ] Dark mode persistente
- [ ] Internacionalização (pt-BR, en-US, es-ES)
