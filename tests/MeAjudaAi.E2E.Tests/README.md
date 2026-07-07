# Infraestrutura de Testes E2E

## Visão Geral

Testes E2E usam **TestContainers** para ambientes isolados com PostgreSQL (PostGIS) e Redis, via `WebApplicationFactory` e `IClassFixture`.

## Arquitetura

### Fixture Principal: `TestContainerFixture`

Classe que gerencia containers, factory, migrações e helpers para testes E2E.

- 🐳 Containers Docker compartilhados (PostgreSQL + Redis)
- 🗃️ Migrações EF Core automáticas com retry
- 🔐 Autenticação via `ConfigurableTestAuthenticationHandler`
- 🧹 Cleanup por schema com `TRUNCATE CASCADE`

### Fixture com Eventos: `EventsEnabledTestContainerFixture`

Subclass que habilita `SynchronousInMemoryMessageBus` e `DomainEventProcessor` para testes que dependem de eventos de integração entre módulos.

### Estrutura

```
tests/MeAjudaAi.E2E.Tests/
├── Base/
│   ├── BaseE2ETest.cs                            # Base class (elimina boilerplate)
│   ├── TestContainerFixture.cs                   # Fixture principal
│   ├── EventsEnabledTestContainerFixture.cs      # Com message bus habilitado
│   └── ResponseTypes.cs                          # Tipos de resposta E2E
├── Modules/
│   ├── Bookings/
│   ├── Communications/
│   ├── Documents/
│   ├── Locations/
│   ├── Payments/
│   ├── Providers/
│   ├── Ratings/
│   ├── SearchProviders/
│   ├── ServiceCatalogs/
│   └── Users/
├── Authorization/
├── CrossModule/
└── Infrastructure/
```

## Como Usar

### Base Classes

Use `BaseE2ETest<TFixture>` or `BaseEventsE2ETest` para eliminar boilerplate (`IAsyncLifetime`, `InitializeAsync`, `DisposeAsync`):

```csharp
// Teste simples
public class MyTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture) { }

// Teste com eventos de integração
public class MyEventsTests(EventsEnabledTestContainerFixture fixture) : BaseEventsE2ETest(fixture) { }

// Teste com dependências extras (ITestOutputHelper, etc.)
public class MyTests(EventsEnabledTestContainerFixture fixture, ITestOutputHelper output) 
    : BaseEventsE2ETest(fixture)
{
    private readonly ITestOutputHelper _output = output;
}
```

Controle o cleanup via `CleanupBeforeEachTest` (padrão: `true`).

### Teste Simples

```csharp
[Trait("Category", "E2E")]
[Trait("Module", "Users")]
public class UsersEndToEndTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{
    [Fact]
    public async Task GetUser_ShouldReturnOk()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Act
        var response = await Fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Teste com Eventos de Integração

```csharp
[Trait("Category", "E2E")]
[Trait("Module", "Ratings")]
public class RatingsEndToEndTests(EventsEnabledTestContainerFixture fixture) : BaseEventsE2ETest(fixture)
{
    [Fact]
    public async Task CreateReview_ShouldUpdateSearch()
    {
        EventsEnabledTestContainerFixture.AuthenticateAsAdmin();
        // ... teste com eventos de integração
    }
}
```

### Acesso Direto ao Banco

```csharp
await Fixture.WithServiceScopeAsync(async services =>
{
    var context = services.GetRequiredService<UsersDbContext>();
    var user = await context.Users.FirstAsync();
    user.Should().NotBeNull();
});
```

### Criar Usuário de Teste

```csharp
var userId = await Fixture.CreateTestUserAsync();
```

## Métodos Disponíveis

### Autenticação (estáticos)
| Método | Descrição |
|--------|-----------|
| `AuthenticateAsAdmin()` | Admin com todas as permissões |
| `AuthenticateAsUser(userId, username)` | Usuário regular |
| `AuthenticateAsAnonymous()` | Sem autenticação |
| `AuthenticateAsAdminWithProvider(providerId)` | Admin vinculado a um provider |
| `BeforeEachTest()` | Limpa contexto de autenticação |

### HTTP (na instância)
| Método | Descrição |
|--------|-----------|
| `Fixture.ApiClient` | HttpClient autenticado |
| `Fixture.PostJsonAsync<T>(uri, content)` | POST com JSON |
| `Fixture.PutJsonAsync<T>(uri, content)` | PUT com JSON |
| `Fixture.PatchJsonAsync<T>(uri, content)` | PATCH com JSON |
| `TestContainerFixture.ReadJsonAsync<T>(response)` | Desserializa resposta |
| `TestContainerFixture.ExtractIdFromLocation(header)` | Extrai GUID do Location |

### Database
| Método | Descrição |
|--------|-----------|
| `Fixture.WithServiceScopeAsync(action)` | Executa com scope de serviços |
| `Fixture.CleanupDatabaseAsync()` | TRUNCATE todas as tabelas + Redis FLUSHALL |
| `Fixture.CreateTestUserAsync()` | Cria usuário via API e retorna ID |

### Propriedades
| Propriedade | Descrição |
|-------------|-----------|
| `Fixture.ApiClient` | HttpClient configurado |
| `Fixture.Faker` | Gerador de dados fake (Bogus) |
| `TestContainerFixture.JsonOptions` | Opções de serialização JSON |
| `Fixture.Services` | IServiceProvider da factory |

## Configuração dos Containers

### PostgreSQL
- **Image**: `postgis/postgis:16-3.4`
- **Database**: `meajudaai_test`
- **Credentials**: `postgres/test123`
- **Schema**: Criado automaticamente via `CREATE SCHEMA IF NOT EXISTS`
- **Migrações**: Aplicadas via `ApplyAllDiscoveredMigrationsAsync()` com retry

### Redis
- **Image**: `redis:7-alpine`
- **Port**: Alocado dinamicamente

### Serviços Mockados
| Serviço | Mock | Motivo |
|---------|------|--------|
| `IKeycloakService` | `MockKeycloakService` | Sem dependência externa |
| `IUserDomainService` | `MockUserDomainService` | Sem dependência externa |
| `IBlobStorageService` | `MockBlobStorageService` | Sem Azure Storage |
| `IDocumentIntelligenceService` | `MockDocumentIntelligenceService` | Sem Azure OCR |
| `IPaymentGateway` | `MockPaymentGateway` | Sem Stripe real |
| `IMessageBus` | `MockNoOpMessageBus` ou `SynchronousInMemoryMessageBus` | Isolamento / eventos |
| `IDomainEventProcessor` | `MockNoOpDomainEventProcessor` ou `DomainEventProcessor` | Isolamento / eventos |

### Serviços Desabilitados
- Keycloak, Hangfire, Rate Limiting, Geographic Restriction, Cache, RabbitMQ

## Infraestrutura Compartilhada (Shared.Tests)

Componentes reutilizáveis em `Shared.Tests/TestInfrastructure/`:

| Componente | Local | Uso |
|------------|-------|-----|
| `BaseE2ETest<TFixture>` | `E2E.Tests/Base/` | Elimina boilerplate IAsyncLifetime, constructor, InitializeAsync, DisposeAsync |
| `BaseEventsE2ETest` | `E2E.Tests/Base/` | Base para testes com `SynchronousInMemoryMessageBus` habilitado |
| `ConfigurableTestAuthenticationHandler` | `Handlers/` | Autenticação por teste com `AsyncLocal` |
| `TestContextAwareHandler` | `Handlers/` | Injeta header `X-Test-Context-Id` nas requisições |
| `CompositeTestUnitOfWork` | `Helpers/` | Redireciona `IUnitOfWork` para o DbContext correto por aggregate |
| `DbContextSchemaHelper` | `Helpers/` | Mapeia nome do DbContext → schema PostgreSQL |
| `MigrationDiscoveryExtensions` | `Extensions/` | Descobre e aplica migrações com retry |
| `SharedTestContainers` | `Containers/` | Gerencia containers compartilhados entre testes |
| `SynchronousInMemoryMessageBus` | `Mocks/Messaging/` | Message bus síncrono para testes com eventos |

## Paralelização

A paralelização está **desabilitada** via `[assembly: CollectionBehavior(DisableTestParallelization = true)]` para evitar race conditions com containers compartilhados.

## Troubleshooting

### Docker não encontrado
Verifique se Docker Desktop está rodando.

### Testes lentos na primeira execução
Containers e migrações são criados na primeira execução. Execuções seguintes reutilizam containers.

### Erro de conexão com banco
Os containers usam portas dinâmicas. Verifique se o Docker está acessível.

### `AUTHENTICATION CONTEXT NOT FOUND`
Chame `AuthenticateAsAdmin()` (ou outro método de auth) antes de fazer requisições autenticadas.
