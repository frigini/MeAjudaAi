# ApiService Tests

## Overview

Testes unitários e de integração para o bootstrapper/web host da aplicação (`MeAjudaAi.ApiService`).

## Project Structure

```
tests/MeAjudaAi.ApiService.Tests/
├── Unit/                           # Testes unitários
│   ├── Middleware/                  # Testes de middlewares
│   └── Swagger/                    # Testes de configuração Swagger/OpenAPI
├── MeAjudaAi.ApiService.Tests.csproj
└── README.md
```

## Responsibilities

**O que testar:**
- Configuração do web host e DI container
- Middlewares de aplicação (global exception handler, security headers)
- Configuração de Swagger/OpenAPI
- Health checks

**O que NÃO testar:**
- Endpoints específicos de módulos (já coberto por Integration.Tests)
- Lógica de negócio (já coberto por unit tests dos módulos)

## Writing Tests

### Base Classes

| Base Class | Purpose | Use When |
|------------|---------|----------|
| `WebApplicationFactory<T>` | Testes de host com DI | Testes de configuração do web host |

### Example

```csharp
public class SwaggerConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Swagger_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Running Tests

```bash
dotnet test tests/MeAjudaAi.ApiService.Tests
```

## Dependencies

- `MeAjudaAi.ApiService` (projeto de produção)
- `MeAjudaAi.Modules.Users.Infrastructure`
- `MeAjudaAi.Modules.Providers.Infrastructure`
- `MeAjudaAi.Shared.Tests` (infraestrutura compartilhada)
