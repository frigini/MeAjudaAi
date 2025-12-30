# MeAjudaAi.Client.Contracts

Biblioteca de contratos HTTP para clientes frontend (Blazor WebAssembly, Mobile, SPA).

## 📦 Propósito

Este projeto contém **interfaces Refit** que definem endpoints da API REST do MeAjudaAi. Os DTOs são reutilizados de `MeAjudaAi.Shared.Contracts`.

## 🎯 Responsabilidades

- ✅ Interfaces Refit com atributos HTTP (`[Get]`, `[Post]`, etc.)
- ✅ Documentação XML dos endpoints (HTTP codes, parâmetros, retornos)
- ✅ Modelos específicos de paginação (`PagedResult<T>`)
- ✅ Query parameters e route parameters

## 🚫 O que NÃO incluir

- ❌ DTOs (usar `MeAjudaAi.Shared.Contracts`)
- ❌ Lógica de negócio
- ❌ Validadores FluentValidation (usar Shared.Contracts)
- ❌ Implementações concretas (Refit gera automaticamente)

## 📂 Estrutura

```
MeAjudaAi.Client.Contracts/
├── Api/
│   ├── IProvidersApi.cs        # GET /api/v1/providers
│   ├── IDocumentsApi.cs        # GET /api/v1/documents
│   ├── IServicesApi.cs         # GET /api/v1/services
│   └── ...
└── Models/
    └── PagedResult.cs          # Modelo de paginação
```

## 🔧 Uso no Frontend

### 1. Registrar no DI (Program.cs)
```csharp
using Refit;
using MeAjudaAi.Client.Contracts.Api;

builder.Services.AddRefitClient<IProvidersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.meajudaai.com"));
```

### 2. Injetar em componentes Blazor
```csharp
@inject IProvidersApi ProvidersApi

@code {
    private async Task LoadProvidersAsync()
    {
        var result = await ProvidersApi.GetProvidersAsync(pageNumber: 1, pageSize: 20);
        
        if (result.IsSuccess)
        {
            providers = result.Value.Items;
        }
    }
}
```

### 3. Usar com Fluxor (State Management)
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

## 📝 Convenções

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
