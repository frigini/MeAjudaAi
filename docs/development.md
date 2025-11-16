# Guia de Desenvolvimento - MeAjudaAi

Este guia fornece instruções práticas e diretrizes abrangentes para desenvolvedores trabalhando no projeto MeAjudaAi.

## 🚀 Setup Inicial do Ambiente

### **Pré-requisitos**

| Ferramenta | Versão | Descrição |
|------------|--------|-----------|
| **.NET SDK** | 9.0+ | Framework principal |
| **Docker Desktop** | Latest | Containers para desenvolvimento |
| **Visual Studio** | 2022 17.8+ | IDE recomendada |
| **PostgreSQL** | 15+ | Banco de dados (via Docker) |
| **Git** | Latest | Controle de versão |

### **Setup Rápido**

```bash
# 1. Clonar o repositório
git clone https://github.com/frigini/MeAjudaAi.git
cd MeAjudaAi

# 2. Verificar ferramentas
dotnet --version    # Deve ser 9.0+
docker --version    # Verificar se Docker está rodando

# 3. Restaurar dependências
dotnet restore

# 4. Executar com Aspire (recomendado)
cd src/Aspire/MeAjudaAi.AppHost
dotnet run

# OU executar apenas a API
cd src/Bootstrapper/MeAjudaAi.ApiService
dotnet run

# Executar via Aspire (com dashboard)
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```

### **Configuração do Visual Studio**

#### Extensões Recomendadas
- **C# Dev Kit**: Produtividade C#
- **Docker**: Suporte a containers
- **GitLens**: Melhor integração Git
- **SonarLint**: Análise de código
- **Thunder Client**: Teste de APIs

#### Configurações do Editor
```json
// .vscode/settings.json
{
    "dotnet.defaultSolution": "./MeAjudaAi.sln",
    "omnisharp.enableEditorConfigSupport": true,
    "editor.formatOnSave": true,
    "csharp.semanticHighlighting.enabled": true,
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    }
}
```

## 🏗️ Estrutura do Projeto

### **Organização de Código**

```text
src/
├── Modules/                           # Módulos de domínio
│   └── Users/                         # Módulo de usuários
│       ├── API/                       # Endpoints HTTP
│       │   ├── UsersEndpoints.cs      # Minimal APIs
│       │   └── Requests/              # DTOs de request
│       ├── Application/               # Lógica de aplicação (CQRS)
│       │   ├── Commands/              # Commands e handlers
│       │   ├── Queries/               # Queries e handlers
│       │   └── Services/              # Serviços de aplicação
│       ├── Domain/                    # Lógica de domínio
│       │   ├── Entities/              # Entidades e agregados
│       │   ├── ValueObjects/          # Value objects
│       │   ├── Events/                # Domain events
│       │   └── Services/              # Domain services
│       └── Infrastructure/            # Acesso a dados e externos
│           ├── Persistence/           # Entity Framework
│           ├── Repositories/          # Implementação de repositórios
│           └── ExternalServices/      # Integrações externas
├── Shared/                           # Componentes compartilhados
│   └── MeAjudaAi.Shared/             # Primitivos e abstrações
└── Bootstrapper/                     # Configuração da aplicação
    └── MeAjudaAi.ApiService/         # API principal
```

## 📋 Padrões de Desenvolvimento

### **Convenções de Nomenclatura**

#### **Arquivos e Classes**
```csharp
// ✅ Correto
public sealed class User { }                    // Entidades: PascalCase
public sealed record UserId(Guid Value);        // Value Objects: PascalCase
public sealed record RegisterUserCommand();     // Commands: [Verb][Entity]Command
public sealed record GetUserByIdQuery();        // Queries: Get[Entity]By[Criteria]Query
public sealed class RegisterUserCommandHandler; // Handlers: [Command/Query]Handler

// ❌ Incorreto
public class userService { }                    // Nome deve ser PascalCase
public record user_id();                        // Use PascalCase, não snake_case
public class GetUsersQueryHandler { }           // Deve especificar critério
```

#### **Métodos e Variáveis**
```csharp
// ✅ Correto - camelCase para variáveis e parâmetros
public async Task<User> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
{
    var userEntity = await _repository.FindByIdAsync(userId);
    return userEntity;
}

// ❌ Incorreto
public async Task<User> get_user(userid id) { } // PascalCase para métodos, camelCase para parâmetros
```

### **Coding Standards .NET/C#**

#### **1. Seguir Convenções Microsoft**
- Use convenções oficiais de C# da Microsoft
- Implemente proper error handling
- Adicione documentação XML para APIs públicas

#### **2. Clean Code**
```csharp
// ✅ Bom
public async Task<Result<User>> RegisterUserAsync(
    RegisterUserCommand command, 
    CancellationToken cancellationToken = default)
{
    // Validação
    var validationResult = await _validator.ValidateAsync(command, cancellationToken);
    if (!validationResult.IsValid)
        return Result.Failure(validationResult.Errors);

    // Lógica de negócio
    var user = User.Create(command.Email, command.Name);
    await _repository.AddAsync(user, cancellationToken);
    
    return Result.Success(user);
}

// ❌ Ruim
public async Task<User> RegisterUser(RegisterUserCommand cmd)
{
    var u = new User(); // Nome vago
    u.Email = cmd.Email; // Setters públicos violam encapsulamento
    await _repo.Add(u); // Sem tratamento de erro
    return u;
}
```

#### **3. Tratamento de Erros**
```csharp
// ✅ Use Result pattern para operações que podem falhar
public async Task<Result<User>> GetUserAsync(UserId id)
{
    try
    {
        var user = await _repository.FindByIdAsync(id);
        return user is null 
            ? Result.Failure($"User with ID {id} not found")
            : Result.Success(user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user {UserId}", id);
        return Result.Failure("An error occurred while retrieving the user");
    }
}
```

## 🛠️ Comandos de Desenvolvimento

### Executando a Aplicação

```powershell
# Run with Aspire (RECOMMENDED - includes all services)
cd src\Aspire\MeAjudaAi.AppHost
dotnet run

# Run API only (without Aspire orchestration)
cd src\Bootstrapper\MeAjudaAi.ApiService
dotnet run

# Access points after running:
# - Aspire Dashboard: https://localhost:17063 or http://localhost:15297
# - API Service: https://localhost:7524 or http://localhost:5545
```

### Build

```powershell
# Build entire solution
dotnet build

# Build specific configuration
dotnet build --configuration Release

# Restore dependencies
dotnet restore
```

### Testes

```powershell
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Testes por módulo
dotnet test src\Modules\Users\Tests\
dotnet test src\Modules\Providers\Tests\

# Filtrar por categoria
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Generate HTML coverage report (requires reportgenerator tool)
reportgenerator -reports:"coverage\**\coverage.opencover.xml" -targetdir:"coverage\html" -reporttypes:Html
```

### Migrations de Banco de Dados

```powershell
# Apply all migrations (RECOMMENDED - cross-platform PowerShell script)
.\scripts\ef-migrate.ps1

# Apply migrations for specific module
.\scripts\ef-migrate.ps1 -Module Users
.\scripts\ef-migrate.ps1 -Module Providers

# Check migration status
.\scripts\ef-migrate.ps1 -Command status

# Add new migration
.\scripts\ef-migrate.ps1 -Command add -Module Users -MigrationName "AddNewField"

# Environment variables needed:
# - DB_HOST (default: localhost)
# - DB_PORT (default: 5432)
# - DB_NAME (default: MeAjudaAi)
# - DB_USER (default: postgres)
# - DB_PASSWORD (required)
```

### Qualidade de Código

```powershell
# Aplicar formatação automática
dotnet format

# Verificar warnings
dotnet build --verbosity normal

# Limpar artefatos
dotnet clean
```

### Documentação da API

```powershell
# Generate OpenAPI spec for API clients (APIDog, Postman, Insomnia, Bruno)
.\scripts\export-openapi.ps1

# Specify custom output path
.\scripts\export-openapi.ps1 -OutputPath "docs\api-spec.json"

# Access Swagger UI when running:
# https://localhost:7524/swagger
```

## 🧪 Diretrizes de Testes

### **Testing Strategy Overview**

O MeAjudaAi segue uma estratégia abrangente de testes baseada na pirâmide de testes:

```text
                    /\
                   /  \
                  / E2E \
                 /________\
                /          \
               / Integration \
              /_______________\
             /                 \
            /   Unit Tests      \
           /____________________\
          /                      \
         /   Architecture Tests   \
        /______________________\
```

### **1. Padrões de Nomenclatura para Testes**
```csharp
// ✅ Padrão: [MethodName]_[Scenario]_[ExpectedResult]
[Test]
public async Task RegisterUser_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var command = new RegisterUserCommand("user@example.com", "João Silva");
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Email.Should().Be("user@example.com");
}

[Test]
public async Task RegisterUser_WithInvalidEmail_ShouldReturnValidationError()
{
    // Arrange
    var command = new RegisterUserCommand("invalid-email", "João Silva");
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Contain("email");
}
```

### **2. Testes de Integração**
```csharp
public class UsersEndpointsTests : IntegrationTestBase
{
    [Test]
    public async Task POST_Users_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = new { Email = "test@example.com", Name = "Test User" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### **3. Test Authentication Handler**

Para facilitar os testes, o sistema possui um handler de autenticação configurável:

```csharp
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options,
                                   ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Options.Enabled)
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Options.UserId ?? "test-user-id"),
            new(ClaimTypes.Name, Options.UserName ?? "Test User"),
            new(ClaimTypes.Email, Options.UserEmail ?? "test@example.com")
        };

        // Add permissions if specified
        if (Options.Permissions?.Any() == true)
        {
            foreach (var permission in Options.Permissions)
            {
                claims.Add(new Claim(AuthConstants.Claims.Permission, permission.ToString()));
            }
        }

        var identity = new ClaimsIdentity(claims, TestAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthenticationDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

### **4. Multi-Environment Testing Strategy**

#### Test Configuration per Environment
```json
{
  "TestConfiguration": {
    "Development": {
      "UseInMemoryDatabase": true,
      "UseTestAuthentication": true,
      "SkipPermissionValidation": false
    },
    "CI": {
      "UseInMemoryDatabase": true,
      "UseTestAuthentication": true,
      "SkipPermissionValidation": false,
      "EnableCodeCoverage": true
    },
    "Staging": {
      "UseInMemoryDatabase": false,
      "UseTestAuthentication": false,
      "SkipPermissionValidation": false
    }
  }
}
```

### **5. Permission System Testing**

#### Testing Type-Safe Permissions
```csharp
[Test]
public async Task GetUserPermissions_WithValidUser_ShouldReturnCorrectPermissions()
{
    // Arrange
    var userId = "test-user-id";
    var expectedPermissions = new[] 
    { 
        EPermission.Users_Read, 
        EPermission.Users_Write 
    };
    
    // Act
    var permissions = await _permissionService.GetUserPermissionsAsync(userId);
    
    // Assert
    permissions.Should().Contain(expectedPermissions);
}

[Test]
public async Task CheckPermission_WithAuthorizedUser_ShouldReturnTrue()
{
    // Arrange
    var userId = "authorized-user";
    await _permissionService.GrantPermissionAsync(userId, EPermission.Users_Read);
    
    // Act
    var hasPermission = await _permissionService.HasPermissionAsync(userId, EPermission.Users_Read);
    
    // Assert
    hasPermission.Should().BeTrue();
}
```

### **6. Code Coverage Guidelines**

#### Coverage Thresholds
- **Minimum**: 70% (warning threshold)
- **Good**: 85% (recommended threshold)
- **Excellent**: 90%+

#### Viewing Coverage Reports
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate HTML report (optional)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.opencover.xml" -targetdir:"./coverage/html" -reporttypes:Html
```

#### Coverage in CI/CD
O pipeline automaticamente:
- Gera relatórios de coverage para cada PR
- Comenta automaticamente nos PRs com estatísticas
- Falha se o coverage cair abaixo de 70%

### **7. Integration Test Setup**

#### Base Integration Test Class
```csharp
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;

    protected IntegrationTestBase()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real services with test doubles
                    services.AddTestAuthentication();
                    services.AddInMemoryDatabase();
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    protected void SetTestUser(string userId, params EPermission[] permissions)
    {
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test", CreateTestToken(userId, permissions));
    }
}
```

### **8. Architecture Tests**

#### Testing Architectural Rules
```csharp
[Test]
public void Controllers_Should_OnlyDependOnMediatR()
{
    var result = Types.InCurrentDomain()
        .That().ResideInNamespace("MeAjudaAi.*.Controllers")
        .Should().OnlyDependOn("MediatR", "Microsoft.AspNetCore", "MeAjudaAi.Shared")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}

[Test]
public void DomainEntities_Should_NotDependOnInfrastructure()
{
    var result = Types.InCurrentDomain()
        .That().ResideInNamespace("MeAjudaAi.*.Domain")
        .Should().NotDependOn("MeAjudaAi.*.Infrastructure")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

### **9. Running Tests**

#### Local Development
```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Unit"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in watch mode
dotnet watch test
```

#### CI/CD Pipeline
Tests are automatically executed in the following scenarios:
- Every pull request
- Every push to main/develop branches
- Scheduled nightly runs

### **10. Testing Best Practices**

#### ✅ **Do's**
- Write tests for all business logic
- Use descriptive test names that explain the scenario
- Follow the AAA pattern (Arrange, Act, Assert)
- Test both success and failure scenarios
- Mock external dependencies
- Use test data builders for complex objects

#### ❌ **Don'ts**
- Don't test framework code
- Don't write tests that depend on other tests
- Don't use real databases in unit tests
- Don't test private methods directly
- Don't ignore failing tests

#### Test Data Builders Example
```csharp
public class UserBuilder
{
    private string _email = "default@example.com";
    private string _name = "Default User";
    private List<EPermission> _permissions = new();

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPermissions(params EPermission[] permissions)
    {
        _permissions.AddRange(permissions);
        return this;
    }

    public User Build() => new(_email, _name, _permissions);
}

// Usage in tests
var user = new UserBuilder()
    .WithEmail("test@example.com")
    .WithPermissions(EPermission.Users_Read, EPermission.Users_Write)
    .Build();
```

## 🔄 Git Workflow

### **Fluxo de Branches**

1. **Criar feature branch a partir de `master`**
   ```bash
   git checkout master
   git pull origin master
   git checkout -b feature/user-registration
   ```

2. **Fazer commits pequenos e focados**
   ```bash
   git add .
   git commit -m "feat: add user registration command and handler"
   ```

3. **Escrever mensagens de commit claras**
   ```bash
   # ✅ Bom
   git commit -m "feat: add user email validation"
   git commit -m "fix: resolve null reference in user service"
   git commit -m "refactor: extract user validation to separate method"
   
   # ❌ Ruim
   git commit -m "changes"
   git commit -m "fix stuff"
   git commit -m "WIP"
   ```

4. **Criar Pull Request para review**
5. **Garantir que todos os testes passem antes do merge**

### **Convenções de Commit**
- `feat:` Nova funcionalidade
- `fix:` Correção de bug
- `refactor:` Refatoração de código
- `test:` Adição ou modificação de testes
- `docs:` Alterações na documentação
- `chore:` Tarefas de manutenção

## 👥 Processo de Code Review

### **Checklist de Review**

#### **Funcionalidade**
- [ ] O código resolve o problema proposto?
- [ ] Todos os edge cases estão cobertos?
- [ ] Performance está adequada?

#### **Qualidade**
- [ ] Código está legível e bem estruturado?
- [ ] Nomes de variáveis/métodos são descritivos?
- [ ] Não há código duplicado?
- [ ] Tratamento de erros está adequado?

#### **Testes**
- [ ] Testes unitários cobrem a funcionalidade?
- [ ] Testes de integração estão incluídos (se necessário)?
- [ ] Todos os testes estão passando?

#### **Documentação**
- [ ] Documentação foi atualizada?
- [ ] Comentários explicam o "porquê", não o "como"?
- [ ] README reflete mudanças (se aplicável)?

## 📚 Documentação

### **Diretrizes de Documentação**

1. **Atualizar documentação ao adicionar funcionalidades**
2. **Manter arquivos README atualizados**
3. **Documentar breaking changes no changelog**
4. **Adicionar comentários XML para APIs públicas**

```csharp
/// <summary>
/// Registers a new user in the system
/// </summary>
/// <param name="command">The registration command containing user details</param>
/// <param name="cancellationToken">Cancellation token for the operation</param>
/// <returns>A result containing the registered user or error information</returns>
/// <exception cref="ValidationException">Thrown when validation fails</exception>
public async Task<Result<User>> RegisterUserAsync(
    RegisterUserCommand command, 
    CancellationToken cancellationToken = default)
```

## 📦 Adicionando Novos Módulos ao CI/CD

### Como adicionar um novo módulo ao pipeline de testes

Quando criar um novo módulo (ex: Orders, Payments, etc.), siga estes passos para incluí-lo no pipeline de CI/CD:

#### 1. Estrutura do Módulo

Certifique-se de que o novo módulo siga a estrutura padrão:

```yaml
src/Modules/{ModuleName}/
├── MeAjudaAi.Modules.{ModuleName}.API/
├── MeAjudaAi.Modules.{ModuleName}.Application/
├── MeAjudaAi.Modules.{ModuleName}.Domain/
├── MeAjudaAi.Modules.{ModuleName}.Infrastructure/
└── MeAjudaAi.Modules.{ModuleName}.Tests/      # ← Testes unitários
```
#### 2. Atualizar o Workflow de PR

No arquivo `.github/workflows/pr-validation.yml`, adicione o novo módulo na seção `MODULES`:

```bash
MODULES=(
  "Users:src/Modules/Users/MeAjudaAi.Modules.Users.Tests/"
  "Providers:src/Modules/Providers/MeAjudaAi.Modules.Providers.Tests/"
  "Services:src/Modules/Services/MeAjudaAi.Modules.Services.Tests/"  # ← Nova linha
)
```
#### 3. Atualizar o Workflow Aspire (se necessário)

No arquivo `.github/workflows/aspire-ci-cd.yml`, se o módulo tiver testes específicos que precisam ser executados no pipeline de deploy, adicione-os na seção de testes:

```bash
dotnet test src/Modules/{ModuleName}/MeAjudaAi.Modules.{ModuleName}.Tests/ --no-build --configuration Release
```
#### 4. Cobertura de Código

O sistema automaticamente:
- ✅ Coleta cobertura APENAS dos testes unitários do módulo
- ✅ Inclui apenas as classes do módulo no relatório (`[MeAjudaAi.Modules.{ModuleName}.*]*`)
- ✅ Exclui classes de teste e assemblies de teste
- ✅ Gera relatórios separados por módulo

#### 5. Testes que NÃO geram cobertura

Estes tipos de teste são executados, mas NÃO contribuem para o relatório de cobertura:
- `tests/MeAjudaAi.Architecture.Tests/` - Testes de arquitetura
- `tests/MeAjudaAi.Integration.Tests/` - Testes de integração
- `tests/MeAjudaAi.Shared.Tests/` - Testes do shared
- `tests/MeAjudaAi.E2E.Tests/` - Testes end-to-end

#### 6. Validação

Após adicionar um novo módulo:
1. Verifique se o pipeline executa sem erros
2. Confirme que o relatório de cobertura inclui o novo módulo
3. Verifique se não há DLLs duplicadas no relatório

## 📚 Recursos e Referências

### **Documentação Interna**
- [🏗️ Arquitetura e Padrões](./architecture.md)
- [🚀 Infraestrutura](./infrastructure.md)  
- [🔄 CI/CD](./ci_cd.md)
- [🔐 Autenticação](./authentication.md)
- [🧪 Guia de Testes](#-diretrizes-de-testes)
- [📖 README Principal](../README.md)

### **Documentação Externa**
- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Aspire](https://learn.microsoft.com/dotnet/aspire/)

### **Padrões e Boas Práticas**
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://docs.microsoft.com/azure/architecture/patterns/cqrs)
- [C# Coding Standards](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

❓ **Dúvidas?** Entre em contato com a equipe de desenvolvimento ou abra uma [issue](https://github.com/frigini/MeAjudaAi/issues) no repositório.