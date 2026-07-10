# Gateway Tests

## Overview

Testes unitários e de integração para o gateway/API gateway da aplicação (`MeAjudaAi.Gateway`).

## Project Structure

```
tests/MeAjudaAi.Gateway.Tests/
├── Unit/                           # Testes unitários
├── MeAjudaAi.Gateway.Tests.csproj
└── README.md
```

## Responsibilities

**O que testar:**
- Roteamento de requisições
- Transformação de headers
- Rate limiting
- Circuit breakers

**O que NÃO testar:**
- Lógica de negócio dos módulos
- Persistência

## Writing Tests

### Base Classes

| Base Class | Purpose | Use When |
|------------|---------|----------|
| `WebApplicationFactory<T>` | Testes de host com DI | Testes de configuração do gateway |

### Example

```csharp
public class RoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Request_ToUsers_ShouldRouteToUsersService()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized);
    }
}
```

## Running Tests

```bash
dotnet test tests/MeAjudaAi.Gateway.Tests
```

## Dependencies

- `MeAjudaAi.Gateway` (projeto de produção)
- `MeAjudaAi.Shared.Tests` (infraestrutura compartilhada)
