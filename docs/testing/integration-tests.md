# Guia de Testes de Integração

## Visão Geral
Este documento fornece orientação abrangente para escrever e manter testes de integração na plataforma MeAjudaAi.

> **📚 Documentação Relacionada**:
> - [Infraestrutura de Testes (TestContainers)](./test-infrastructure.md) - Infraestrutura de containers para testes
> - [Guia de Cobertura de Código](./coverage.md) - Guia de cobertura de código
> - [Exemplos de Autenticação em Testes](./test-auth-examples.md) - Exemplos de autenticação em testes

## Estratégia de Testes de Integração

O projeto implementa uma **arquitetura de testes de integração em dois níveis** para equilibrar cobertura de testes, desempenho e isolamento:

### 1. Testes de Integração em Nível de Módulo (Escopo do Módulo)
**Localização**: `src/Modules/{Module}/Tests/Integration/`

Estes testes validam **componentes de infraestrutura individuais** dentro de um módulo usando dependências reais. Cada módulo gerencia sua própria infraestrutura de teste.

- **Escopo**: Componentes de módulo único (Repositories, Services, Queries)
- **Infraestrutura**: TestContainers PostgreSQL isolados por classe de teste (um banco lógico por classe)
- **Classes Base**: `{Module}IntegrationTestBase`
- **Velocidade**: Mais rápido (apenas componentes do módulo necessários carregados)
- **Propósito**: Validar persistência de dados, lógica de repositório e queries específicas do módulo
- **Isolamento**: Cada classe de teste usa um banco de dados PostgreSQL isolado criado via Testcontainers

**Diferença chave vs E2E**: Testes de módulo usam **banco PostgreSQL real via Testcontainers** mas **não carregam a aplicação completa**. Apenas os serviços do módulo são registrados, sem endpoints HTTP, middleware de pipeline, ou comunicação com outros módulos.

**Casos de Uso de Exemplo**:
- Testar `IUserQueries.GetByIdAsync()` com um banco de dados PostgreSQL real
- Validar que consultas complexas retornam dados corretos
- Testar lógica de repository (Add, Delete, TryFind)
- Verificar tratamento de transações via UnitOfWork

**Estrutura de Exemplo**:
```csharp
// Localização: src/Modules/Users/Tests/Integration/UserPersistenceIntegrationTests.cs
public class UserPersistenceIntegrationTests : DatabaseTestBase
{
    private UsersDbContext _context;
    private IUserQueries _userQueries;

    [Fact]
    public async Task Add_WithValidUser_ShouldPersistUser()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var repository = uow.GetRepository<User, UserId>();
        var user = CreateValidUser();

        // Act - seed via repository
        repository.Add(user);
        await uow.SaveChangesAsync();

        // Assert - leitura via queries (AsNoTracking)
        var retrieved = await userQueries.GetByIdAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
    }
}
```

### 2. Testes de Integração End-to-End (Centralizado)
**Localização**: `tests/MeAjudaAi.Integration.Tests/`

Estes testes validam **fluxos completos de aplicação** com todos os módulos integrados:

- **Escopo**: Aplicação completa (endpoints HTTP, container DI completo, todos os módulos)
- **Infraestrutura**: Aplicação completa via `WebApplicationFactory` + TestContainers
- **Classes Base**: `BaseIntegrationTest`, `BaseApiTest`
- **Velocidade**: Mais lento (pilha completa de aplicação carregada)
- **Propósito**: Validar workflows end-to-end, contratos de API, comunicação entre módulos, middleware, autenticação
- **Isolamento**: Cada classe de teste pode usar banco isolado ou compartilhado

**Diferença chave vs módulo**: Testes E2E carregam **toda a aplicação** com pipeline de middleware completo, autenticação, autorização, e todos os módulos registrados. Testam a **API HTTP completa**.

**Casos de Uso de Exemplo**:
- Testar que `POST /api/v1/users` cria usuário e retorna resposta HTTP correta
- Validar fluxos de autenticação e autorização via middleware
- Testar comunicação entre módulos (ex: criar provider valida que usuário existe via módulo Users)
- Verificar workflows de negócio completos atravessando múltiplos módulos
- Testar políticas de segurança, headers, compressão
- Testar integração com infraestrutura (RabbitMQ, Hangfire, Azure Blob Storage)

**Estrutura de Exemplo**:
```csharp
// Localização: tests/MeAjudaAi.Integration.Tests/Modules/Users/UsersApiTests.cs
public class UsersApiTests : ApiTestBase
{
    [Fact]
    public async Task RegisterUser_ValidData_ShouldReturnCreated()
    {
        // Testa requisição/resposta HTTP completa
        // Todos os módulos carregados e integrados
        var response = await Client.PostAsJsonAsync("/api/users/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Matriz de Decisão: Qual Nível Usar?

| Cenário de Teste | Nível de Componente | End-to-End |
|--------------|----------------|------------|
| Operações CRUD de repositório | ✅ | ❌ |
| Consultas complexas de banco de dados | ✅ | ❌ |
| Migrações de banco de dados | ✅ | ❌ |
| Lógica de negócio de serviço | ✅ | ❌ |
| Endpoints HTTP | ❌ | ✅ |
| Fluxos de autenticação | ❌ | ✅ |
| Comunicação entre módulos | ❌ | ✅ |
| Workflows completos | ❌ | ✅ |
| Ciclo de vida de recurso (CRUD+) | ❌ | ✅ |
| Validação de regras de negócio | ❌ | ✅ |

### Comparação de Módulos

**Módulos com Testes em Nível de Módulo** (`src/Modules/{Module}/Tests/Integration/`):
- ✅ Users - testes de repository e queries específicas do módulo
- ✅ Providers - testes de repository, queries e lógica de persistência
- ✅ SearchProviders - testes de queries e busca geoespacial

**Módulos com Apenas Testes E2E** (em `tests/MeAjudaAi.Integration.Tests/`):
- ✅ Bookings - testes de API e workflow
- ✅ Communications - testes de outbox pattern e API
- ✅ Documents - testes de API e storage
- ✅ Locations - testes de APIs externas (CEP, IBGE), geocoding, e API
- ✅ Payments - testes de API e gateway mockado
- ✅ Ratings - testes de API e module API
- ✅ ServiceCatalogs - testes de API e ciclo de vida

**Nota sobre Módulos sem Testes de Módulo**: Alguns módulos não possuem `Tests/Integration/` no nível do módulo pois suas funcionalidades são mais bem testadas via testes E2E que carregam a aplicação completa. Isso é uma decisão de design - testes de módulo são úteis quando há queries ou repositories complexos que se beneficiam de testes isolados.

### Categorias de Teste
1. **Testes de Integração de API** - Testando ciclos completos de requisição/resposta HTTP (E2E)
2. **Testes de Integração de Banco de Dados** - Testando persistência e recuperação de dados (Componente)
3. **Testes de Integração de Serviço** - Testando interação entre múltiplos serviços (Ambos os níveis)
4. **Testes de Ciclo de Vida** - Testando ciclo de vida completo de recurso (Create → Read → Update → Delete + validações)
5. **Testes de Recursos Avançados** - Testando regras de negócio complexas e operações específicas de domínio

### Organização de Testes E2E por Cenário

Testes E2E são organizados por **cenário de teste** em vez de simplesmente por módulo, melhorando a manutenibilidade e descoberta:

**Padrão 1: Testes de Integração de Módulo** (`{Module}ModuleTests.cs`)
- Foco: Funcionalidade básica do módulo e integração
- Escopo: Operações CRUD principais e caminhos felizes
- Exemplo: `UsersModuleTests.cs`, `ProvidersModuleTests.cs`

**Padrão 2: Testes de Ciclo de Vida** (`{Module}LifecycleE2ETests.cs`)
- Foco: Validação completa do ciclo de vida de recursos
- Escopo: Create → Update → Delete + transições de estado
- Exemplo: `ProvidersLifecycleE2ETests.cs`, `UsersLifecycleE2ETests.cs`
- Cobertura: Endpoints PUT/PATCH/DELETE com validação de regras de negócio

**Padrão 3: Testes Específicos de Recurso** (`{Module}{Feature}E2ETests.cs`)
- Foco: Recursos de domínio específicos ou sub-recursos
- Escopo: Workflows complexos e operações relacionadas
- Exemplos:
  - `ProvidersDocumentsE2ETests.cs` - Upload/exclusão de documentos
  - `DocumentsVerificationE2ETests.cs` - Workflow de verificação de documentos
  - `ServiceCatalogsAdvancedE2ETests.cs` - Operações avançadas de catálogo

**Padrão 4: Testes Transversais** (`{Concern}E2ETests.cs`)
- Foco: Preocupações entre módulos
- Escopo: Autorização, autenticação, infraestrutura
- Exemplo: `PermissionAuthorizationEndToEndTests.cs`

**Benefícios desta organização:**
- 🎯 **Intenção Clara**: Propósito do teste é óbvio pelo nome do arquivo
- 📁 **Navegação Fácil**: Encontre testes por cenário (Ctrl+P → "lifecycle")
- 🐛 **Falhas Isoladas**: Falhas agrupadas por domínio de recurso
- 📊 **Rastreamento de Cobertura**: Rastreie cobertura de endpoints por categoria
- 🔄 **Melhor Manutenção**: Arquivos de teste menores e focados

### Configuração de Ambiente de Teste
Testes de integração usam TestContainers para ambientes de teste isolados e reproduzíveis:

- **Containers PostgreSQL** - Instâncias de banco de dados isoladas
- **Containers Redis** - Teste de camada de cache
- **Teste de Message Bus** - Validação de comunicação entre serviços

## Classes Base de Teste

### SharedApiTestBase
A classe `SharedApiTestBase` fornece funcionalidade comum para testes de integração de API:

```csharp
public abstract class SharedApiTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; }
    protected TestContainerDatabase Database { get; private set; }
    
    // Métodos de configuração e limpeza
}
```

### Recursos Principais
- Gerenciamento automático do ciclo de vida de containers de teste
- Autenticação de teste configurada
- Inicialização de schema de banco de dados
- Configuração de cliente HTTP

## Autenticação em Testes

### Manipulador de Autenticação de Teste
Testes de integração usam o `ConfigurableTestAuthenticationHandler` para:

- **Autenticação Previsível** - Configuração consistente de usuário de teste
- **Teste Baseado em Papel** - Testando diferentes permissões de usuário
- **Cenários Não Autenticados** - Testando endpoints públicos

### Configuração
```csharp
services.AddAuthentication("Test")
    .AddScheme<AuthenticationSchemeOptions, ConfigurableTestAuthenticationHandler>(
        "Test", options => { });
```

## Testes de Banco de Dados

### Gerenciamento de Banco de Dados de Teste
- Cada classe de teste recebe um container PostgreSQL isolado
- Schema de banco de dados é aplicado automaticamente
- Dados de teste são limpos entre os testes

### Integração com Entity Framework
```csharp
protected async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> action)
{
    using var context = CreateDbContext();
    return await action(context);
}
```

## Escrevendo Testes de Integração

### Estrutura de Teste
1. **Arrange** - Configurar dados de teste e configuração
2. **Act** - Executar a operação sendo testada
3. **Assert** - Verificar os resultados esperados

### Exemplo de Teste
```csharp
[Fact]
public async Task CreateUser_ValidData_ReturnsCreatedUser()
{
    // Arrange
    var createUserRequest = new CreateUserRequest
    {
        Email = "test@example.com",
        Name = "Test User"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/users", createUserRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var user = await response.Content.ReadFromJsonAsync<UserResponse>();
    user.Email.Should().Be(createUserRequest.Email);
}
```

## Melhores Práticas

### Organização de Testes
- Agrupe testes relacionados na mesma classe de teste
- Use nomes de teste descritivos
- Siga o padrão AAA (Arrange, Act, Assert)

### Considerações de Desempenho
- Minimize operações de banco de dados
- Reutilize containers de teste quando possível
- Use async/await adequadamente

### Gerenciamento de Dados de Teste
- Use builders de dados de teste para objetos complexos
- Limpe dados de teste após cada teste
- Evite dependências entre testes

## Solução de Problemas

### Problemas Comuns
1. **Falhas de Inicialização de Container** - Verifique disponibilidade do Docker
2. **Problemas de Conexão com Banco de Dados** - Verifique strings de conexão
3. **Problemas de Autenticação** - Verifique configuração de autenticação de teste

### Depurando Testes
- Habilite logging detalhado para execuções de teste
- Use helpers de saída de teste para depuração
- Verifique logs de container para problemas de infraestrutura

## Métricas de Cobertura de Endpoints

### Status Atual de Cobertura

O projeto mantém **100% de cobertura de endpoints E2E** com 41 testes de endpoint, num total de 103 testes E2E (incluindo infraestrutura, autorização e lifecycle):

| Módulo | Endpoints | Testes | Cobertura |
|--------|-----------|-------|----------|
| **Providers** | 14 | 14 | 100% |
| **ServiceCatalogs** | 17 | 17 | 100% |
| **Documents** | 4 | 4 | 100% |
| **Users** | 6 | 6 | 100% |
| **TOTAL** | **41** | **41** | **100%** |

### Distribuição de Testes por Categoria

- **Integração de Módulo**: 36 testes (funcionalidade básica de módulo)
- **Testes de Ciclo de Vida**: 18 testes (workflows CRUD completos)
- **Autorização**: 8 testes (validação de permissões)
- **Entre Módulos**: 7 testes (comunicação inter-módulos)
- **Infraestrutura**: 34 testes (verificações de saúde, configuração)

### Cobertura por Tipo de Teste

**Módulo Providers (14 endpoints)**:
- CRUD Básico: `ProvidersModuleTests.cs` (6 testes)
- Ciclo de Vida: `ProvidersLifecycleE2ETests.cs` (6 testes)
- Documentos: `ProvidersDocumentsE2ETests.cs` (2 testes)

**Módulo ServiceCatalogs (17 endpoints)**:
- Integração: `ServiceCatalogsModuleIntegrationTests.cs` (12 testes)
- Avançado: `ServiceCatalogsAdvancedE2ETests.cs` (5 testes)

**Módulo Documents (4 endpoints)**:
- Básico: `DocumentsModuleTests.cs` (1 teste)
- Verificação: `DocumentsVerificationE2ETests.cs` (3 testes)

**Módulo Users (6 endpoints)**:
- Integração: `UsersModuleTests.cs` (2 testes)
- Ciclo de Vida: `UsersLifecycleE2ETests.cs` (6 testes) - cobertura abrangente de DELETE

### Evolução da Cobertura

```text
Antes (78% de cobertura):
├─ Providers: 8/14 (57%)
├─ ServiceCatalogs: 15/17 (88%)
├─ Documents: 3/4 (75%)
└─ Users: 6/6 (100%)

Depois (100% de cobertura):
├─ Providers: 14/14 (100%) ✅ +6 endpoints
├─ ServiceCatalogs: 17/17 (100%) ✅ +2 endpoints
├─ Documents: 4/4 (100%) ✅ +1 endpoint
└─ Users: 6/6 (100%) ✅ Cobertura DELETE aprimorada
```

## Integração CI/CD

### Execução Automatizada de Testes
Testes de integração são executados como parte do pipeline CI/CD:

- **Validação de Pull Request** - Todos os testes devem passar (103/103)
- **Execução Paralela** - Testes executam em paralelo para desempenho
- **Relatório de Cobertura** - Cobertura de testes de integração é rastreada
- **Cobertura de Endpoints** - 100% de cobertura de endpoints mantida

### Configuração de Ambiente
- Testes usam configuração específica de ambiente
- Segredos e dados sensíveis são gerenciados com segurança
- Isolamento de teste é mantido através de execuções paralelas

## Documentação Relacionada

- [Diretrizes de Desenvolvimento](../development.md)
- [Configuração CI/CD](../ci-cd.md)