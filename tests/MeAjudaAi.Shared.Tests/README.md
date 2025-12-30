# MeAjudaAi.Shared.Tests

Biblioteca de infraestrutura de testes compartilhada para todos os módulos do MeAjudaAi.

## 📋 Visão Geral

Este projeto fornece classes base, fixtures, mocks e utilitários para facilitar a criação de testes unitários e de integração consistentes em todo o projeto.

## 🗂️ Estrutura Organizacional

```text
MeAjudaAi.Shared.Tests/
├── TestInfrastructure/           # Infraestrutura principal de testes
│   ├── Base/                     # Classes base para testes
│   │   ├── BaseIntegrationTest.cs        # Base para testes de integração
│   │   ├── BaseSharedIntegrationTest.cs  # Base shared integration
│   │   ├── DatabaseTestBase.cs           # Base para testes de database
│   │   └── EventHandlerTestBase.cs       # Base para testes de handlers
│   ├── Builders/                 # Test data builders
│   │   └── BuilderBase.cs               # Base para builders de teste
│   ├── Configuration/            # Configurações de teste
│   │   └── TestLoggingConfiguration.cs  # Logging para testes
│   ├── Containers/               # TestContainers
│   │   └── SharedTestContainers.cs      # Containers compartilhados
│   ├── Fixtures/                 # xUnit Fixtures
│   │   └── SharedTestFixture.cs         # Fixture compartilhada
│   ├── Handlers/                 # Mock handlers
│   │   ├── Authentication/              # Handlers de autenticação
│   │   │   ├── MockAdminAuthenticationHandler.cs
│   │   │   ├── MockAuthenticationHandler.cs
│   │   │   ├── MockClientAuthenticationHandler.cs
│   │   │   ├── MockProviderAuthenticationHandler.cs
│   │   │   ├── TestAuthenticationHandler.cs
│   │   │   ├── TestAuthenticationSchemeOptions.cs
│   │   │   └── TestAuthenticationService.cs
│   ├── Mocks/                    # Objetos mock
│   │   ├── Http/                        # Mocks HTTP
│   │   ├── Jobs/                        # Mocks de jobs
│   │   └── Messaging/                   # Mocks de messaging
│   ├── Options/                  # Opções de configuração
│   │   ├── TestCacheOptions.cs
│   │   ├── TestDatabaseOptions.cs
│   │   ├── TestExternalServicesOptions.cs
│   │   └── TestInfrastructureOptions.cs
│   └── Services/                 # Serviços mock
│       └── TestCacheService.cs          # Cache para testes
├── Logging/                      # Testes de Logging
│   ├── LoggingConfigurationExtensionsTests.cs
│   └── SerilogConfiguratorTests.cs
├── Messaging/                    # Testes de Messaging
│   ├── EventDispatcherTests.cs
│   └── TestEvent.cs                    # Evento de teste
├── Performance/                  # Testes de Performance
│   ├── BenchmarkExtensions.cs          # Extensões de benchmark
│   ├── BenchmarkResult.cs              # Resultado de benchmark
│   └── TestPerformanceBenchmark.cs
└── Unit/                         # Testes unitários
    ├── Extensions/                     # Testes de extensions
    ├── Helpers/                        # Testes de helpers
    ├── Middleware/                     # Testes de middleware
    │   └── GeographicRestrictionMiddlewareTests.cs
    └── Utils/                          # Testes de utilitários
```

## 🛠️ Componentes Principais

### TestInfrastructure/Base

Classes base que fornecem funcionalidades comuns para diferentes tipos de testes:

- **BaseIntegrationTest**: Base para testes de integração com dependências reais
- **DatabaseTestBase**: Base para testes que precisam de banco de dados
- **EventHandlerTestBase**: Base para testes de event handlers com mensageria

### TestInfrastructure/Containers

TestContainers compartilhados para testes de integração:

- **PostgreSQL + PostGIS**: `postgis/postgis:15-3.4`
- **Redis**: Cache e locks distribuídos
- **Azurite**: Storage emulator

### TestInfrastructure/Handlers/Authentication

Handlers de autenticação mock para diferentes perfis:

- **MockAdminAuthenticationHandler**: Simula usuário admin
- **MockClientAuthenticationHandler**: Simula usuário cliente
- **MockProviderAuthenticationHandler**: Simula usuário prestador
- **TestAuthenticationService**: Serviço de autenticação para testes

### TestInfrastructure/Mocks

Objetos mock organizados por categoria:

- **Http/**: HttpClient, HttpMessageHandler
- **Jobs/**: Background jobs, schedulers
- **Messaging/**: Event bus, message publishers

### TestInfrastructure/Options

Opções de configuração específicas para testes:

- **TestDatabaseOptions**: Configuração de database para testes
- **TestCacheOptions**: Configuração de cache para testes
- **TestExternalServicesOptions**: Mocks de serviços externos
- **TestInfrastructureOptions**: Configurações gerais de infraestrutura

## 🚀 Como Usar

### Testes de Integração com Database

```csharp
public class MeuTesteDeIntegracao : DatabaseTestBase
{
    [Fact]
    public async Task DeveSalvarEntidade()
    {
        // Arrange
        var entity = new MinhaEntidade { Nome = "Teste" };

        // Act
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Set<MinhaEntidade>()
            .FirstOrDefaultAsync(e => e.Nome == "Teste");
        saved.Should().NotBeNull();
    }
}
```

### Testes com Autenticação Mock

```csharp
public class MeuTesteComAuth : BaseIntegrationTest
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Configurar autenticação mock como admin
            services.AddAuthentication("Test")
                .AddScheme<TestAuthenticationSchemeOptions, MockAdminAuthenticationHandler>(
                    "Test", options => { });
        });
    }

    [Fact]
    public async Task DeveAcessarEndpointProtegido()
    {
        // O handler mock já fornece claims de admin
        var response = await Client.GetAsync("/api/v1/admin/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Testes com Event Handlers

```csharp
public class MeuEventHandlerTest : EventHandlerTestBase<MeuEvento, MeuEventHandler>
{
    [Fact]
    public async Task DeveProcessarEvento()
    {
        // Arrange
        var evento = new MeuEvento { Id = Guid.NewGuid() };

        // Act
        await Handler.Handle(evento, CancellationToken.None);

        // Assert
        // Verificar efeitos colaterais
        var result = await VerifyEventProcessed(evento.Id);
        result.Should().BeTrue();
    }
}
```

## 📝 Convenções

### Nomenclatura

- **Classes de teste**: `{ClasseTestada}Tests.cs`
- **Mocks**: `Mock{Service}` ou `Test{Service}`
- **Fixtures**: `{Scope}Fixture.cs`
- **Builders**: `{Entity}Builder.cs`

### Padrão AAA (Arrange-Act-Assert)

Todos os testes seguem o padrão AAA com comentários em inglês:

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

Uso obrigatório de FluentAssertions para assertivas mais expressivas:

```csharp
// ✅ Correto
result.Should().NotBeNull();
result.Errors.Should().BeEmpty();
response.StatusCode.Should().Be(HttpStatusCode.Created);

// ❌ Evitar
Assert.NotNull(result);
Assert.Empty(result.Errors);
Assert.Equal(HttpStatusCode.Created, response.StatusCode);
```

## 🔧 Configuração

### Pacotes Necessários

```xml
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" />
<PackageReference Include="FluentAssertions" />
<PackageReference Include="Moq" />
<PackageReference Include="AutoFixture" />
<PackageReference Include="Testcontainers" />
<PackageReference Include="Testcontainers.PostgreSql" />
<PackageReference Include="Testcontainers.Redis" />
```

### Configuração de TestContainers

Os containers são configurados automaticamente com:

- **PostgreSQL + PostGIS 15-3.4**: Para dados geográficos (NetTopologySuite)
- **Redis 7-alpine**: Para cache e locks
- **Connection strings**: Com `Include Error Detail=true` para diagnóstico

## 📊 Status do Projeto

**Última atualização**: 20 Dezembro 2025 (Sprint 5.5)

### ✅ Completado

- Reorganização completa da estrutura (25/25 itens)
- TestInfrastructure com 8 subpastas organizadas
- Separação de classes auxiliares (TestEvent, BenchmarkResult, etc.)
- Remoção de duplicados (DocumentExtensions, EnumExtensions, SearchableProvider)
- ModuleExtensionsTests movidos para módulos individuais
- ~35 comentários traduzidos (AAA mantido em inglês)
- PostGIS habilitado nos Integration.Tests
- Build verde com 0 erros

### 📝 Documentação

- [Test Infrastructure](../../docs/testing/test-infrastructure.md) - Guia completo de TestContainers
- [Technical Debt](../../docs/technical-debt.md) - Sprint 5.5 refactoring tracking
- [Roadmap](../../docs/roadmap.md) - Sprint 5.5 activities

## 🤝 Contribuindo

Ao adicionar novos testes compartilhados:

1. Identifique a categoria correta em `TestInfrastructure/`
2. Siga os padrões de nomenclatura existentes
3. Adicione documentação XML nos métodos públicos
4. Mantenha o padrão AAA com comentários em inglês
5. Use FluentAssertions para assertivas
6. Adicione exemplos de uso neste README se necessário

## 📚 Referências

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [TestContainers](https://dotnet.testcontainers.org/)
- [AutoFixture](https://github.com/AutoFixture/AutoFixture)
