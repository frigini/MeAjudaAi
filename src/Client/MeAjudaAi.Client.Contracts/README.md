# MeAjudaAi.Client.Contracts

**SDK oficial .NET para consumir a API REST do MeAjudaAi.**

## 📦 O que é este projeto?

Este é o **SDK (Software Development Kit) oficial** do MeAjudaAi para consumo em projetos .NET. Ele facilita o consumo da API REST através de **interfaces Refit** geradas automaticamente.

> **Nota**: Para o frontend React/Next.js, utilize os clientes gerados via OpenAPI (gerado automaticamente pelo `openapi-generator`).

### Por que usar um SDK?

| Sem SDK (HttpClient manual) | Com SDK (MeAjudaAi.Client.Contracts) |
|------------------------------|--------------------------------------|
| 20+ linhas de código boilerplate | 2 linhas (interface + atributo) |
| Serialização JSON manual | ✅ Automática |
| Query parameters manual | ✅ Atributo `[Query]` |
| Tratamento de erros HTTP manual | ✅ `Result<T>` tipado |
| Sem IntelliSense/autocomplete | ✅ Type-safe com documentação XML |
| Código duplicado entre projetos | ✅ Reutilizável entre projetos .NET |

## 🎯 Propósito

Este projeto contém **interfaces Refit** que definem endpoints da API REST do MeAjudaAi. Os DTOs são compartilhados de `MeAjudaAi.Contracts.Modules.*`.

## 🏗️ Arquitetura do SDK

### Como funciona internamente?

```text
┌─────────────────────────────────────┐
│  Serviço .NET (API, Worker, etc.)   │
│  _api.GetProviderByIdAsync(id)      │
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
    Task<ModuleProviderDto> GetProviderByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Refit gera a implementação em runtime:**
```csharp
// Código gerado automaticamente (simplificado)
public class ProvidersApiGenerated : IProvidersApi
{
    private readonly HttpClient _httpClient;
    
    public async Task<ModuleProviderDto> GetProviderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/api/v1/providers/{id}", cancellationToken);
        return await response.Content.ReadFromJsonAsync<ModuleProviderDto>(cancellationToken);
    }
}
```

## 🎯 Responsabilidades

- ✅ Interfaces Refit com atributos HTTP (`[Get]`, `[Post]`, etc.),
- ✅ Documentação XML dos endpoints (HTTP codes, parâmetros, retornos),
- ✅ Modelos específicos de paginação (`PagedResult<T>`),
- ✅ Query parameters e route parameters

## 🚫 O que NÃO incluir

- ❌ Lógica de negócio
- ❌ Implementações concretas (Refit gera automaticamente)

**Nota**: DTOs são compartilhados de `MeAjudaAi.Contracts.Modules.*` (não `MeAjudaAi.Shared.Contracts`).

## 📂 Estrutura

```text
MeAjudaAi.Client.Contracts/
├── Api/
│   ├── IProvidersApi.cs          # Gestão de providers (CRUD, verificação)
│   ├── IDocumentsApi.cs          # Upload e validação de documentos
│   ├── IServiceCatalogsApi.cs    # Catálogo de serviços (categorias + serviços)
│   ├── ILocationsApi.cs          # Restrições geográficas (cidades permitidas)
│   ├── IBookingsApi.cs           # Agendamentos (CRUD, lifecycle, availability, schedule)
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
| **Bookings** | ✅ IBookingsApi | Customer App | Completo |
| **Users** | ⏳ Planejado | Admin Portal (Sprint 8+) | Pendente |
| **SearchProviders** | ❌ Não necessário | Customer App (API interna) | N/A |

## 🔧 Uso em Projetos .NET

### 1. Instalar dependência
```bash
dotnet add reference ../../Client/MeAjudaAi.Client.Contracts
```

### 2. Registrar SDKs no DI (Program.cs)
```csharp
using Refit;
using MeAjudaAi.Client.Contracts.Api;

var apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7001";

// Registrar SDKs necessários
services.AddRefitClient<IProvidersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

services.AddRefitClient<IBookingsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));
```

### 3. Injetar e usar
```csharp
public class ProviderService
{
    private readonly IProvidersApi _providersApi;

    public ProviderService(IProvidersApi providersApi)
    {
        _providersApi = providersApi;
    }

    public async Task<ModuleProviderDto?> GetProviderAsync(Guid id)
    {
        return await _providersApi.GetProviderByIdAsync(id);
    }
}
```

## 💡 Exemplos Práticos por Módulo

### IBookingsApi - Criar Agendamento
```csharp
public async Task<ModuleBookingDto> CreateBookingAsync(Guid providerId, Guid serviceId, DateTimeOffset start, DateTimeOffset end)
{
    var request = new CreateBookingRequestDto(providerId, serviceId, start, end);
    return await _bookingsApi.CreateBookingAsync(request);
}
```

### ILocationsApi - CRUD de Cidades
```csharp
public async Task CreateCityAsync()
{
    var request = new CreateAllowedCityRequestDto
    {
        City = "São Paulo", State = "SP", Country = "Brasil",
        Latitude = -23.5505, Longitude = -46.6333, ServiceRadiusKm = 50
    };
    
    await _locationsApi.CreateAllowedCityAsync(request);
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

## 🔗 Dependências

- **MeAjudaAi.Contracts.Modules.*** - DTOs de módulos (Bookings, Communications, etc.)
- **Refit** - Geração automática de clientes HTTP

## 📚 Referências

- [Refit Documentation](https://github.com/reactiveui/refit)
- [API Versioning](../../../docs/api-automation.md)
- [Authentication & Authorization](../../../docs/authentication-and-authorization.md)
