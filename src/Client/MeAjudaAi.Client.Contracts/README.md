# MeAjudaAi.Client.Contracts

**SDK oficial .NET para consumir a API REST do MeAjudaAi.**

## 📦 O que é este projeto?

Este é o **SDK (Software Development Kit) oficial** do MeAjudaAi, semelhante ao AWS SDK, Stripe SDK, ou Azure SDK. Ele facilita o consumo da API REST através de **clientes HTTP tipados** gerados automaticamente pelo **Refit**.

### Por que usar um SDK?

| Sem SDK (HttpClient manual) | Com SDK (MeAjudaAi.Client.Contracts) |
|------------------------------|--------------------------------------|
| 20+ linhas de código boilerplate | 2 linhas (interface + atributo) |
| Serialização JSON manual | ✅ Automática |
| Query parameters manual | ✅ Atributo `[Query]` |
| Tratamento de erros HTTP manual | ✅ `Result<T>` tipado |
| Sem IntelliSense/autocomplete | ✅ Type-safe com documentação XML |
| Código duplicado entre projetos | ✅ Reutilizável (Blazor WASM, MAUI, Console) |

## 🎯 Propósito

Este projeto contém **interfaces Refit** que definem endpoints da API REST do MeAjudaAi. Os DTOs são compartilhados de `MeAjudaAi.Shared.Contracts`.

## 🏗️ Arquitetura do SDK

### Como funciona internamente?

```
┌─────────────────────────────────────┐
│  Blazor Component / MAUI Page       │
│  @inject IProvidersApi _api         │
└──────────────┬──────────────────────┘
               │ (interface tipada)
┌──────────────▼──────────────────────┐
│  Refit (proxy/code generator)       │
│  - Lê atributos [Get], [Post]       │
│  - Serializa parâmetros             │
│  - Deserializa respostas            │
└──────────────┬──────────────────────┘
               │ (chama)
┌──────────────▼──────────────────────┐
│  HttpClient (.NET Core)             │
│  - Connection pooling               │
│  - Headers, cookies, timeout        │
│  - IHttpClientFactory integration   │
└──────────────┬──────────────────────┘
               │ (HTTP/HTTPS)
┌──────────────▼──────────────────────┐
│  MeAjudaAi.ApiService (backend)     │
│  GET /api/v1/providers/{id}         │
└─────────────────────────────────────┘
```

### Refit gera código automaticamente

**Você escreve apenas a interface:**
```csharp
public interface IProvidersApi
{
    [Get("/api/v1/providers/{id}")]
    Task<Result<ProviderDto>> GetProviderAsync(Guid id);
}
```

**Refit gera a implementação em runtime:**
```csharp
// Código gerado automaticamente (simplificado)
public class ProvidersApiGenerated : IProvidersApi
{
    private readonly HttpClient _httpClient;
    
    public async Task<Result<ProviderDto>> GetProviderAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/providers/{id}");
        return await response.Content.ReadFromJsonAsync<Result<ProviderDto>>();
    }
}
```

## 🎯 Responsabilidades

- ✅ Interfaces Refit com atributos HTTP (`[Get]`, `[Post]`, etc.),
- ✅ Documentação XML dos endpoints (HTTP codes, parâmetros, retornos),
- ✅ Modelos específicos de paginação (`PagedResult<T>`),
- ✅ Query parameters e route parameters

## 🚫 O que NÃO incluir

- ❌ DTOs (usar `MeAjudaAi.Shared.Contracts`)
- ❌ Lógica de negócio
- ❌ Validadores FluentValidation (usar Shared.Contracts)
- ❌ Implementações concretas (Refit gera automaticamente)

## 📂 Estrutura

```text
MeAjudaAi.Client.Contracts/
├── Api/
│   ├── IProvidersApi.cs          # Gestão de providers (CRUD, verificação)
│   ├── IDocumentsApi.cs          # Upload e validação de documentos
│   ├── IServiceCatalogsApi.cs    # Catálogo de serviços (categorias + serviços)
│   ├── ILocationsApi.cs          # Restrições geográficas (cidades permitidas)
│   └── IUsersApi.cs              # (FUTURO) Gestão de usuários
└── Models/
    └── PagedResult.cs            # Modelo de paginação genérico
```

### Status dos SDKs por Módulo

| Módulo | SDK | Usado por | Status |
|--------|-----|-----------|--------|
| **Providers** | ✅ IProvidersApi | Admin Portal (Sprint 6-7) | Completo |
| **Documents** | ✅ IDocumentsApi | Admin Portal (Sprint 7) | Completo |
| **ServiceCatalogs** | ✅ IServiceCatalogsApi | Admin Portal (Sprint 6-7) | Completo |
| **Locations** | ✅ ILocationsApi | Admin Portal (Sprint 7) | Completo |
| **Users** | ⏳ Planejado | Admin Portal (Sprint 8+) | Pendente |
| **SearchProviders** | ❌ Não necessário | Customer App (API interna) | N/A |

## 🔧 Uso no Admin Portal

### 1. Instalar dependência (já configurado)
```bash
dotnet add reference ../../Client/MeAjudaAi.Client.Contracts
```

### 2. Registrar SDKs no DI (Program.cs)
```csharp
using Refit;
using MeAjudaAi.Client.Contracts.Api;

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";

// Registrar todos os SDKs necessários para o Admin Portal
builder.Services.AddRefitClient<IProvidersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddRefitClient<IDocumentsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddRefitClient<IServiceCatalogsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddRefitClient<ILocationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
```

### 3. Injetar em páginas Blazor
```csharp
@page "/providers"
@inject IProvidersApi ProvidersApi
@inject ISnackbar Snackbar

<MudDataGrid Items="@_providers" Loading="@_isLoading">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Nome" />
        <PropertyColumn Property="x => x.Email" Title="Email" />
    </Columns>
</MudDataGrid>

@code {
    private IReadOnlyList<ModuleProviderDto> _providers = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        var result = await ProvidersApi.GetProvidersAsync(pageNumber: 1, pageSize: 20);
        
        if (result.IsSuccess)
        {
            _providers = result.Value.Items;
        }
        else
        {
            Snackbar.Add($"Erro: {result.Error.Message}", Severity.Error);
        }
        
        _isLoading = false;
    }
}
```

### 4. Usar com Fluxor (State Management - Recomendado)
```csharp
public class LoadProvidersEffect : Effect<LoadProvidersAction>
{
    private readonly IProvidersApi _api;

    public LoadProvidersEffect(IProvidersApi api)
    {
        _api = api;
    }

    public override async Task HandleAsync(LoadProvidersAction action, IDispatcher dispatcher)
    {
        var result = await _api.GetProvidersAsync(action.PageNumber, action.PageSize);
        
        if (result.IsSuccess)
        {
            dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value.Items));
        }
        else
        {
            dispatcher.Dispatch(new LoadProvidersFailureAction(result.Error));
        }
    }
}
```

## 💡 Exemplos Práticos por Módulo

### IDocumentsApi - Upload de Documento
```csharp
@inject IDocumentsApi DocumentsApi

private async Task UploadDocumentAsync(IBrowserFile file, Guid providerId)
{
    using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
    var streamPart = new StreamPart(stream, file.Name, file.ContentType);
    
    var result = await DocumentsApi.UploadDocumentAsync(providerId, streamPart, "RG");
    
    if (result.IsSuccess)
        Snackbar.Add($"✅ Documento {result.Value.DocumentId} enviado", Severity.Success);
}
```

### ILocationsApi - CRUD de Cidades
```csharp
@inject ILocationsApi LocationsApi

private async Task CreateCityAsync()
{
    var request = new CreateAllowedCityRequestDto
    {
        City = "São Paulo", State = "SP", Country = "Brasil",
        Latitude = -23.5505, Longitude = -46.6333, ServiceRadiusKm = 50
    };
    
    var result = await LocationsApi.CreateAllowedCityAsync(request);
    if (result.IsSuccess) await RefreshCitiesAsync();
}
```

## �📝 Convenções

### 1. Documentação XML Obrigatória
```csharp
/// <summary>
/// Lista todos os providers com paginação.
/// </summary>
/// <param name="pageNumber">Número da página (1-based)</param>
/// <param name="pageSize">Tamanho da página (máximo 100)</param>
/// <param name="cancellationToken">Token de cancelamento da operação</param>
/// <returns>Lista paginada de providers com metadados de paginação</returns>
/// <response code="200">Lista de providers retornada com sucesso</response>
/// <response code="400">Parâmetros de paginação inválidos</response>
/// <response code="401">Não autenticado</response>
/// <response code="403">Sem permissão para listar providers</response>
```

### 2. Atributos Refit
```csharp
[Get("/api/v{version}/providers")]          // Route parameters
[Get("/api/v1/providers/{id}")]             // Path parameter
[Post("/api/v1/providers")]                 // Body
[Put("/api/v1/providers/{id}")]             // Path + Body
[Delete("/api/v1/providers/{id}")]          // Delete
```

### 3. Query Parameters
```csharp
Task<Result<PagedResult<T>>> GetAsync(
    [Query] int pageNumber = 1,
    [Query] int pageSize = 20,
    [Query] string? filter = null);
```

### 4. Headers
```csharp
[Headers("Accept: application/json")]
Task<Result<T>> GetAsync([Header("X-Custom")] string customHeader);
```

## 🧪 Testes

- **Refit mocks**: Usar `RestService.For<IProvidersApi>(mockHttpMessageHandler)`
- **WireMock.NET**: Simular API real para testes de integração
- **bUnit**: Testar componentes Blazor que injetam APIs

## 🔗 Dependências

- **MeAjudaAi.Shared.Contracts** - DTOs compartilhados
- **Refit** - Geração automática de clientes HTTP

## 📚 Referências

- [Refit Documentation](https://github.com/reactiveui/refit)
- [API Versioning](../../../docs/api-automation.md)
- [Authentication & Authorization](../../../docs/authentication-and-authorization.md)
