# MeAjudaAi.Shared.Tests

Biblioteca de infraestrutura de testes compartilhada para todos os projetos do MeAjudaAi.

## Visão Geral

Este projeto centraliza:
- **Infraestrutura de testes** — classes base, fixtures, mocks, helpers e builders reutilizáveis
- **Testes unitários** — de todas as classes de `MeAjudaAi.Shared`
- **Testes de ServiceDefaults e Contracts** — para garantir consistência (fora da cobertura)
- **Base para outros projetos de teste** — Integration.Tests, E2E.Tests e todos os módulos

## Estrutura

```text
MeAjudaAi.Shared.Tests/
├── TestInfrastructure/                  # Infraestrutura compartilhada
│   ├── Base/                            # Classes base para testes
│   │   ├── BaseIntegrationTest.cs       # Base para testes com containers compartilhados
│   │   ├── BaseDatabaseTest.cs          # Base para testes com DB isolado (Respawn)
│   │   ├── BaseInMemoryDatabaseTest.cs  # Base para testes com EF Core InMemory
│   │   ├── BaseSqliteInMemoryDatabaseTest.cs  # Base para testes com SQLite in-memory
│   │   ├── BaseEventHandlerTest.cs      # Base para testes de event handlers
│   │   └── BaseModuleApiTest.cs         # Base para testes de API de módulos
│   ├── Builders/                        # Test data builders (padrão Builder)
│   │   ├── BaseBuilder.cs               # Builder base genérico
│   │   └── Modules/                     # Builders por módulo
│   │       ├── Bookings/                # BookingBuilder, AvailabilityBuilder, etc.
│   │       ├── Communications/          # CommunicationLogBuilder, EmailTemplateBuilder, etc.
│   │       ├── Documents/               # DocumentBuilder
│   │       ├── Locations/               # AllowedCityBuilder
│   │       ├── Payments/                # PaymentTransactionBuilder, SubscriptionBuilder, etc.
│   │       ├── Providers/               # ProviderBuilder, BusinessProfileDtoBuilder
│   │       ├── Ratings/                 # ReviewBuilder
│   │       ├── SearchProviders/         # SearchableProviderBuilder
│   │       ├── ServiceCatalogs/         # ServiceBuilder, ServiceCategoryBuilder
│   │       └── Users/                   # UserBuilder, EmailBuilder, UsernameBuilder
│   ├── Collections/                     # Definições de collections xUnit
│   │   └── ModuleCollections.cs         # Collections consolidadas (10 módulos)
│   ├── Commands/                        # Testes de CQRS commands
│   │   ├── TestCommand.cs
│   │   ├── TestCommandHandlers.cs
│   │   └── TestPipelineBehavior.cs
│   ├── Configuration/                   # Configuração de testes
│   │   └── TestLoggingConfiguration.cs
│   ├── Constants/                       # Constantes para testes
│   │   ├── TestData.cs
│   │   └── TestUrls.cs
│   ├── Containers/                      # Containers Docker compartilhados
│   │   ├── SharedTestContainers.cs      # Gerencia PostgreSQL, RabbitMq, Redis
│   │   └── SimpleDatabaseFixture.cs     # Fixture simples com PostGIS
│   ├── Extensions/                      # Extensões para testes
│   │   ├── MigrationDiscoveryExtensions.cs
│   │   ├── TestAuthenticationExtensions.cs
│   │   ├── TestBaseAuthExtensions.cs
│   │   └── TestInfrastructureExtensions.cs
│   ├── Handlers/                        # Handlers de autenticação mock
│   │   ├── BaseTestAuthenticationHandler.cs
│   │   ├── ConfigurableTestAuthenticationHandler.cs
│   │   ├── InstanceTestAuthenticationHandler.cs
│   │   ├── TestAuthenticationConfiguration.cs
│   │   ├── TestContextAwareHandler.cs
│   │   └── Interfaces/
│   │       └── ITestAuthenticationConfiguration.cs
│   ├── Helpers/                         # Helpers utilitários
│   │   ├── CompositeTestUnitOfWork.cs
│   │   ├── DbContextSchemaHelper.cs
│   │   ├── EnvironmentVariableRestorer.cs
│   │   └── TestConnectionHelper.cs
│   ├── Metrics/                         # Métricas para testes
│   │   └── TestMeterFactory.cs
│   ├── Mocks/                           # Objetos mock organizados por categoria
│   │   ├── MockGeographicValidationService.cs
│   │   ├── MockHostEnvironment.cs
│   │   ├── MockLocalizerBuilder.cs
│   │   ├── Caching/                     # FakeHybridCache
│   │   ├── E2E/                         # MockNoOpMessaging
│   │   ├── Http/                        # MockHttpClientBuilder, MockHttpMessageHandler
│   │   ├── Jobs/                        # MockBackgroundJobService
│   │   ├── Messaging/                   # FakeSynchronousMessageBus
│   │   └── Modules/                     # Mocks específicos por módulo
│   │       ├── Communications/
│   │       ├── Documents/
│   │       ├── Payments/
│   │       ├── Providers/
│   │       ├── ServiceCatalogs/
│   │       └── Users/
│   ├── Options/                         # Opções de configuração para testes
│   │   ├── TestCacheOptions.cs
│   │   ├── TestDatabaseOptions.cs
│   │   ├── TestExternalServicesOptions.cs
│   │   └── TestInfrastructureOptions.cs
│   └── Services/                        # Serviços para testes
│       └── TestCacheService.cs
├── Contracts/                           # Testes unitários de Contracts
│   └── Unit/
│       ├── DTOs/
│       ├── Functional/
│       └── Models/
├── ServiceDefaults/                     # Testes unitários de ServiceDefaults
│   └── Unit/
├── Unit/                                # Testes unitários de MeAjudaAi.Shared
│   ├── Authorization/
│   ├── Behaviors/
│   ├── Caching/
│   ├── Commands/
│   ├── Database/
│   ├── Domain/
│   ├── Endpoints/
│   ├── Events/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Geolocation/
│   ├── Jobs/
│   ├── Messaging/
│   ├── Middleware/
│   ├── Modules/
│   ├── Monitoring/
│   ├── Queries/
│   ├── Serialization/
│   └── Utilities/
├── GlobalTestConfiguration.cs           # Configuração global de paralelização
└── README.md
```

## Componentes Principais

### Classes Base (`TestInfrastructure/Base/`)

| Classe | Descrição |
|--------|-----------|
| `BaseIntegrationTest` | Base para testes de integração com containers compartilhados (PostgreSQL, RabbitMq). Auto-migração, DI, cleanup automático. |
| `BaseDatabaseTest` | Base para testes com banco isolado por teste usando Respawn. |
| `BaseInMemoryDatabaseTest<T>` | Base para testes com EF Core InMemory. |
| `BaseSqliteInMemoryDatabaseTest<T>` | Base para testes com SQLite in-memory. |
| `BaseEventHandlerTest<TEvent, THandler>` | Base para testes de event handlers com mensageria. |
| `BaseModuleApiTest` | Base para testes de API de módulos. |

### Containers (`TestInfrastructure/Containers/`)

| Componente | Descrição |
|------------|-----------|
| `SharedTestContainers` | Gerencia containers estáticos compartilhados (PostgreSQL + PostGIS, RabbitMq). Start/Stop/Cleanup. |
| `SimpleDatabaseFixture` | Fixture simples com PostGIS para testes que precisam de DB sem container compartilhado. |

### Collections (`TestInfrastructure/Collections/`)

Todas as definições de collection dos módulos estão consolidadas em `ModuleCollections.cs`:
- `UsersIntegrationTests`, `ProvidersIntegrationTests`, `BookingsIntegrationTests`, etc.
- `DisableParallelization = true` para evitar race conditions com containers compartilhados.

### Mocks (`TestInfrastructure/Mocks/`)

Organizados por categoria:
- **Caching/**: `FakeHybridCache` — cache in-memory para testes
- **Http/**: `MockHttpClientBuilder`, `MockHttpMessageHandler` — simulação de HTTP
- **Jobs/**: `MockBackgroundJobService` — jobs em background mockados
- **Messaging/**: `FakeSynchronousMessageBus` — message bus síncrono para testes com eventos
- **Modules/**: Mocks específicos por módulo (Keycloak, PaymentGateway, BlobStorage, etc.)

### Builders (`TestInfrastructure/Builders/`)

Padrão Builder para criação de dados de teste:
- `UserBuilder`, `ProviderBuilder`, `BookingBuilder`, etc.
- Organizados por módulo em `Builders/Modules/`
- `BaseBuilder<T>` fornece interface genérica

### Handlers de Autenticação (`TestInfrastructure/Handlers/`)

| Handler | Descrição |
|---------|-----------|
| `ConfigurableTestAuthenticationHandler` | Autenticação configurável por teste via `AsyncLocal` |
| `InstanceTestAuthenticationHandler` | Autenticação por instância (para testes que precisam de múltiplos usuários) |
| `TestContextAwareHandler` | Injeta header `X-Test-Context-Id` nas requisições |

### Helpers (`TestInfrastructure/Helpers/`)

| Helper | Descrição |
|--------|-----------|
| `CompositeTestUnitOfWork` | Redireciona `IUnitOfWork` para o DbContext correto por aggregate |
| `DbContextSchemaHelper` | Mapeia nome do DbContext → schema PostgreSQL |
| `EnvironmentVariableRestorer` | Salva e restaura variáveis de ambiente durante testes |
| `TestConnectionHelper` | Obtém connection strings com fallback Aspire/env vars |

## Como Usar

### Teste de Integração com Container Compartilhado

```csharp
public class MeuTesteIntegracao : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions() => new()
    {
        Database = new TestDatabaseOptions { Schema = "meu_schema" }
    };

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddMeuModuloTestInfrastructure(options);
    }

    [Fact]
    public async Task DeveSalvarEntidade()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeuDbContext>();
        // ...
    }
}
```

### Teste com Builder

```csharp
var user = new UserBuilder()
    .WithName("João Silva")
    .WithEmail("joao@example.com")
    .Build();
```

### Teste com Autenticação Mock

```csharp
public class MeuTesteAuth : BaseIntegrationTest
{
    [Fact]
    public async Task DeveAcessarEndpointProtegido()
    {
        TestAuthenticationExtensions.AuthenticateAsAdmin();
        var response = await Client.GetAsync("/api/v1/admin/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Teste de Event Handler

```csharp
public class MeuEventHandlerTest : BaseEventHandlerTest<MeuEvento, MeuEventHandler>
{
    [Fact]
    public async Task DeveProcessarEvento()
    {
        var evento = new MeuEvento { Id = Guid.NewGuid() };
        await Handler.HandleAsync(evento, CancellationToken.None);
        // Verificar efeitos colaterais
    }
}
```

## Convenções

### Nomenclatura

- **Classes de teste**: `{ClasseTestada}Tests.cs`
- **Mocks**: `Mock{Service}` (para Moq) ou `Fake{Service}` (para behavioral)
- **Builders**: `{Entity}Builder.cs`
- **Fixtures**: `{Scope}Fixture.cs`

### Padrão AAA

Todos os testes seguem o padrão AAA (Arrange-Act-Assert) com comentários em inglês:

```csharp
[Fact]
public async Task DeveRealizarOperacao()
{
    // Arrange
    var input = PrepareTestData();

    // Act
    var result = await SystemUnderTest.Execute(input);

    // Assert
    result.Should().BeSuccessful();
}
```

### FluentAssertions

Uso obrigatório de FluentAssertions:

```csharp
// ✅ Correto
result.Should().NotBeNull();
response.StatusCode.Should().Be(HttpStatusCode.Created);

// ❌ Evitar
Assert.NotNull(result);
Assert.Equal(HttpStatusCode.Created, response.StatusCode);
```

## Pacotes NuGet

- `xunit.v3` — framework de testes
- `FluentAssertions` — assertivas expressivas
- `Moq` — mocking framework
- `Bogus` — geração de dados fake
- `AutoFixture` + `AutoFixture.AutoMoq` — dados automatizados
- `Testcontainers.PostgreSql` — containers PostgreSQL
- `Testcontainers.RabbitMq` — containers RabbitMQ
- `Respawn` — reset de banco entre testes
- `Microsoft.EntityFrameworkCore.InMemory` — DB in-memory
- `Microsoft.EntityFrameworkCore.Sqlite` — DB SQLite in-memory
- `Hangfire.InMemory` — Hangfire para testes

## Referências

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [TestContainers](https://dotnet.testcontainers.org/)
- [AutoFixture](https://github.com/AutoFixture/AutoFixture)
