# Test Strategy - MeAjudaAi

## Visão Geral

Este documento define a estratégia de testes para o projeto MeAjudaAi, estabelecendo as responsabilidades de cada camada de teste, regras para evitar redundância, e convenções para criação de novos testes.

## Estrutura de Testes

O projeto possui 7 projetos de teste com responsabilidades distintas:

```
tests/
├── MeAjudaAi.Shared.Tests/           # Infraestrutura compartilhada de testes
├── MeAjudaAi.Integration.Tests/      # Testes de integração cross-module (HTTP API)
├── MeAjudaAi.E2E.Tests/              # Testes E2E com Testcontainers
├── MeAjudaAi.ApiService.Tests/       # Testes do bootstrapper/web host
├── MeAjudaAi.Architecture.Tests/     # Guardrails de arquitetura (fitness functions)
├── MeAjudaAi.Gateway.Tests/          # Testes do edge/gateway
└── src/Modules/*/Tests/              # Testes internos de cada módulo (unit + integration)
```

### Hierarquia de Testes

```
┌─────────────────────────────────────────────────────────────┐
│  E2E Tests (E2E.Tests)                                      │
│  • Fluxos completos multi-módulo com Testcontainers          │
│  • Valida orquestração entre módulos                         │
│  • lentos (~5-30s)                                           │
├─────────────────────────────────────────────────────────────┤
│  Cross-Module Integration (Integration.Tests.Modules)       │
│  • Testes HTTP via WebApplicationFactory (sem Testcontainers)│
│  • Valida contratos de API, status codes, serialização       │
│  • Testa camada de apresentação (controllers, middleware)     │
│  • médios (~100ms-2s)                                        │
├─────────────────────────────────────────────────────────────┤
│  Module Integration (src/Modules/*/Tests/Integration/)       │
│  • Testa handlers com banco real (Testcontainers)            │
│  • Valida repository, queries, domain services               │
│  • Testa idempotência e retry                                │
├─────────────────────────────────────────────────────────────┤
│  Module Unit Tests (src/Modules/*/Tests/Unit/)               │
│  • Lógica isolada com mocks                                  │
│  • Entities, ValueObjects, Handlers, Validators, Mappers     │
│  • muito rápidos (<100ms)                                    │
├─────────────────────────────────────────────────────────────┤
│  ApiService Tests (ApiService.Tests)                         │
│  • Configuração do web host e DI container                   │
│  • Middlewares globais                                        │
├─────────────────────────────────────────────────────────────┤
│  Gateway Tests (Gateway.Tests)                               │
│  • Roteamento e transformação de requisições                 │
├─────────────────────────────────────────────────────────────┤
│  Architecture Tests (Architecture.Tests)                     │
│  • Guardrails e fitness functions                            │
│  • Regras de nomenclatura e dependências                     │
└─────────────────────────────────────────────────────────────┘
```

## Responsabilidades por Camada

### 1. Unit Tests (`src/Modules/*/Tests/Unit/`)

**O que testar:**
- Entities e Value Objects (regras de negócio, invariantes)
- Command/Query Handlers (lógica com mocks de dependências)
- Validators (regras de validação)
- Mappers/Extensions (conversões)
- Event Handlers (publicação de eventos)
- Middlewares (lógica condicional isolada)

**O que NÃO testar:**
- Integração com banco de dados
- Endpoints HTTP
- Fluxos multi-step

**Convenções:**
- Um arquivo de teste por classe de produção
- Nome: `{ClasseProdução}Tests.cs`
- Usar `Mock<T>` ou `Substitute.For<T>()` para dependências
- Seguir padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`

**Exemplo:**
```csharp
public class CreateProviderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_ShouldCreateProvider()
    {
        // Arrange
        var repositoryMock = new Mock<IProviderRepository>();
        var handler = new CreateProviderCommandHandler(repositoryMock.Object);
        var command = new CreateProviderCommand { Name = "Test" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>()), Times.Once);
    }
}
```

### 2. Module Integration Tests (`src/Modules/*/Tests/Integration/`)

**O que testar:**
- Repository/Query implementations com banco real
- EF Core configuration (conversores, constraints, indexes)
- Domain Services com infraestrutura real
- Idempotência de operações
- Retry em falhas transitórias

**O que NÃO testar:**
- Endpoints HTTP (já coberto por Integration.Tests.Modules)
- Lógica que pode ser testada com mocks

**Convenções:**
- Usar `IntegrationTestBase` do módulo
- Banco via Testcontainers (PostgreSQL)
- Um arquivo por feature ou grupo de cenários relacionados

### 3. Cross-Module Integration Tests (`tests/MeAjudaAi.Integration.Tests/Modules/`)

**O que testar:**
- Contratos de API (formato de resposta, campos obrigatórios)
- Status codes HTTP (200, 201, 400, 401, 403, 404, 451)
- Autenticação e autorização em endpoints
- Serialização/deserialização JSON
- Paginação e filtros via query params
- Endpoints públicos (AllowAnonymous)
- Headers de segurança
- Formato de erros (ProblemDetails)

**O que NÃO testar:**
- Lógica de negócio interna (já coberto por unit tests do módulo)
- Persistência no banco (já coberto por module integration tests)
- Fluxos multi-módulo (já coberto por E2E tests)

**Convenções:**
- Usar `BaseApiTest` como base
- Um arquivo por módulo ou grupo de endpoints relacionados
- Nome: `{Modulo}ApiTests.cs` ou `{Modulo}EndpointsTests.cs`
- Seguir padrão AAA estritamente

**Exemplo:**
```csharp
public class ProvidersApiTests : BaseApiTest
{
    [Fact]
    public async Task CreateProvider_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var request = new { Name = "Test Provider" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### 4. E2E Tests (`tests/MeAjudaAi.E2E.Tests/`)

**O que testar:**
- Fluxos completos de negócio (multi-endpoint, multi-módulo)
- Criação de provider → upload documento → verificação → busca
- Saga de booking → notificação → avaliação
- Seed de dados → API → persistência verificada via DB

**O que NÃO testar:**
- Endpoints individuais (já coberto por Integration.Tests)
- Lógica de negócio isolada (já coberto por unit tests)

**Convenções:**
- Usar `TestContainerFixture` com PostgreSQL real
- Nome: `{Feature}EndToEndTests.cs` ou `{Feature}ApiTests.cs`

### 5. ApiService Tests (`tests/MeAjudaAi.ApiService.Tests/`)

**O que testar:**
- Configuração do web host e DI container
- Middlewares de aplicação (global exception handler, security headers)
- Configuração de Swagger/OpenAPI
- Health checks

**O que NÃO testar:**
- Endpoints específicos de módulos (já coberto por Integration.Tests)
- Lógica de negócio (já coberto por unit tests dos módulos)

**Convenções:**
- Usar `WebApplicationFactory` para testes de host
- Nome: `{Componente}Tests.cs`

### 6. Architecture Tests (`tests/MeAjudaAi.Architecture.Tests/`)

**O que testar:**
- Guardrails de arquitetura (dependências entre camadas)
- Regras de nomenclatura
- Convenções de projeto
- Limites de dependências

**O que NÃO testar:**
- Lógica de negócio
- Integração com infraestrutura

**Convenções:**
- Usar NetArchTest.Rules para verificação de regras
- Nome: `{Regra}Tests.cs`

### 7. Gateway Tests (`tests/MeAjudaAi.Gateway.Tests/`)

**O que testar:**
- Roteamento de requisições
- Transformação de headers
- Rate limiting
- Circuit breakers

**O que NÃO testar:**
- Lógica de negócio dos módulos
- Persistência

**Convenções:**
- Usar `WebApplicationFactory` com mocks de backend
- Nome: `{Feature}Tests.cs`

## Regras para Evitar Redundância

### Regra 1: Cada cenário em apenas UMA camada

| Cenário | Camada correta |
|---------|---------------|
| Entity cria corretamente invariantes | Unit (no módulo) |
| Handler cria provider com mock | Unit (no módulo) |
| Repository persiste e recupera | Module Integration |
| POST /providers retorna 201 | Cross-Module Integration |
| POST /providers sem auth retorna 401 | Cross-Module Integration |
| Provider → Documento → Verificação → Busca | E2E |

### Regra 2: Não testar o mesmo endpoint em múltiplos arquivos

**ERRADO** (redundante):
```csharp
// ProvidersApiTests.cs
[Fact] public async Task GetProviders_ShouldReturnOk() { ... }

// ProvidersIntegrationTests.cs
[Fact] public async Task GetProviders_ShouldReturnOk() { ... }

// ProvidersAdminIntegrationTests.cs
[Fact] public async Task GetProviders_WithFilters_ShouldReturnOk() { ... }
```

**CORRETO** (consolidado):
```csharp
// ProvidersApiTests.cs - testa contrato e filtros
[Fact] public async Task GetProviders_WithValidRequest_ShouldReturnOk() { ... }
[Fact] public async Task GetProviders_WithFilters_ShouldReturnFilteredResults() { ... }
```

### Regra 3: Smoke tests de DB connectivity são redundantes

Não criar múltiplos testes `CanConnectToDatabase_ShouldWork` no mesmo projeto. Um único teste por fixture é suficiente.

### Regra 4: DI registration tests são redundantes

Não duplicar testes de `XxxDbContext` not null e `IXxxQueries` not null. Um único arquivo `{Modulo}DbContextTests.cs` por módulo é suficiente.

### Regra 5: Debug tests devem ser temporários

Arquivos de teste com prefixo `Debug_` ou `*_DebugTests.cs` devem ser removidos após o debug. Não são testes permanentes.

## Padrão AAA (Arrange-Act-Assert)

**TODOS** os testes em `MeAjudaAi.Integration.Tests.Modules` devem seguir o padrão AAA com comentários explícitos:

```csharp
[Fact]
public async Task CreateCategory_WithValidData_ShouldReturnCreated()
{
    // Arrange
    AuthConfig.ConfigureAdmin();
    var request = new { Name = "Test Category", Description = "Test" };

    // Act
    var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var location = response.Headers.Location?.ToString();
    location.Should().Contain("/api/v1/service-catalogs/categories");
}
```

**Exceções permitidas:**
- `// Cleanup` após Assert para remoção de dados de teste
- `// Arrange` com descrição adicional: `// Arrange - no auth configured`

## Matriz de Decisão

Ao criar um novo teste, use esta matriz:

| Pergunta | Unit | Module Integration | Cross-Module Integration | E2E |
|----------|------|--------------------|--------------------------|-----|
| Testa lógica isolada? | ✅ | ❌ | ❌ | ❌ |
| Precisa de banco real? | ❌ | ✅ | ❌ | ✅ |
| Testa EF Core config? | ❌ | ✅ | ❌ | ❌ |
| Testa endpoint HTTP? | ❌ | ❌ | ✅ | ✅ |
| Testa status code? | ❌ | ❌ | ✅ | ✅ |
| Testa serialização JSON? | ❌ | ❌ | ✅ | ❌ |
| Testa autenticação/autorização? | ❌ | ❌ | ✅ | ✅ |
| Testa múltiplos módulos? | ❌ | ❌ | ❌ | ✅ |
| Testa fluxo de negócio completo? | ❌ | ❌ | ❌ | ✅ |
| Executa em < 100ms? | ✅ | ❌ | ❌ | ❌ |
| Usa mocks? | ✅ | Poucos | Mínimo | Mínimo |

## Convenções de Nomenclatura

| Camada | Padrão de nome | Exemplo |
|--------|----------------|---------|
| Unit | `{Classe}Tests.cs` | `CreateProviderCommandHandlerTests.cs` |
| Module Integration | `{Feature}IntegrationTests.cs` | `ProviderIdempotencyIntegrationTests.cs` |
| Cross-Module Integration | `{Modulo}ApiTests.cs` | `ProvidersApiTests.cs` |
| Cross-Module Integration | `{Modulo}EndpointsTests.cs` | `UsersEndpointsTests.cs` |
| E2E | `{Feature}EndToEndTests.cs` | `ProviderDashboardApiTests.cs` |

## Checklist para Code Review de Testes

- [ ] O teste está na camada correta?
- [ ] Não existe cenário equivalente em outro arquivo/projeto?
- [ ] Segue o padrão AAA com comentários?
- [ ] Nome do teste descreve o cenário e resultado esperado?
- [ ] Não usa InMemory Database para testar EF Core config?
- [ ] Não é um smoke test duplicado (CanConnect, DI registration)?
- [ ] Não é um arquivo de debug temporário?
