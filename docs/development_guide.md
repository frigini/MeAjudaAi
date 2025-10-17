# Guia de Desenvolvimento - MeAjudaAi

Este guia fornece instruções práticas para desenvolvedores trabalhando no projeto MeAjudaAi.

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
```csharp
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
```csharp
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
```text
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
public class user { }                           // Não use minúsculas
public class UserService { }                   // Evite sufixo "Service" genérico
public class UserManager { }                   // Evite sufixo "Manager"
```csharp
#### **Namespaces**
```csharp
// ✅ Estrutura padrão
namespace MeAjudaAi.Modules.Users.Domain.Entities;
namespace MeAjudaAi.Modules.Users.Application.Commands;
namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;
namespace MeAjudaAi.Shared.Common.Exceptions;
```csharp
#### **Métodos e Variáveis**
```csharp
// ✅ Métodos: PascalCase, descritivos
public async Task<User?> GetUserByExternalIdAsync(string externalId);
public void RegisterUser(RegisterUserCommand command);

// ✅ Variáveis: camelCase
var userRepository = GetService<IUsersRepository>();
var existingUser = await userRepository.GetByIdAsync(userId);

// ✅ Constantes: PascalCase
public const string DefaultConnectionStringName = "DefaultConnection";
```csharp
## 📋 Workflows de Desenvolvimento

### **Feature Development Flow**

```mermaid
graph LR
    A[Feature Branch] --> B[Implement]
    B --> C[Unit Tests]
    C --> D[Integration Tests]
    D --> E[PR Review]
    E --> F[Merge to Develop]
    F --> G[Deploy to Dev]
```text
#### **1. Criar Feature Branch**
```bash
# Partir sempre do develop
git checkout develop
git pull origin develop

# Criar branch feature com padrão: feature/JIRA-123-description
git checkout -b feature/USER-001-user-registration
```csharp
#### **2. Implementação TDD**
```csharp
// 1️⃣ Escrever teste primeiro
[Fact]
public void RegisterUser_ValidData_ShouldCreateUser()
{
    // Arrange
    var command = new RegisterUserCommand("ext-123", "test@test.com", "John", "Doe", UserType.Customer);
    
    // Act & Assert - Deve falhar inicialmente
    var result = await _handler.Handle(command, CancellationToken.None);
    result.IsSuccess.Should().BeTrue();
}

// 2️⃣ Implementar código mínimo para passar
public class RegisterUserCommandHandler 
{
    public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        // Implementação mínima
        return RegisterUserResult.Success(UserId.New());
    }
}

// 3️⃣ Refatorar com implementação completa
```yaml
#### **3. Commits Semânticos**
```bash
# Formato: type(scope): description
git commit -m "feat(users): add user registration endpoint"
git commit -m "test(users): add user registration unit tests"
git commit -m "docs(users): update user API documentation"
git commit -m "fix(users): handle duplicate email validation"
git commit -m "refactor(users): extract user validation service"
```csharp
**Tipos de commit**:
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Documentação
- `test`: Testes
- `refactor`: Refatoração sem mudança de comportamento
- `perf`: Melhoria de performance
- `chore`: Tarefas de manutenção

### **Code Review Guidelines**

#### **Checklist do Reviewer**
- [ ] **Arquitetura**: Segue padrões DDD/Clean Architecture?
- [ ] **SOLID**: Princípios respeitados?
- [ ] **Testes**: Cobertura adequada (>80%)?
- [ ] **Segurança**: Dados sensíveis protegidos?
- [ ] **Performance**: Queries otimizadas?
- [ ] **Documentação**: XML comments em métodos públicos?
- [ ] **Convenções**: Nomenclatura e estrutura consistentes?

#### **Estrutura de Feedback**
```markdown
## ✅ Positivos
- Boa implementação do padrão Command/Handler
- Testes bem estruturados com AAA pattern

## 🔧 Sugestões
- Considere extrair validação para um validator específico
- Adicione logging para melhor observabilidade

## ❌ Obrigatórias
- Falta tratamento de exceção em UserRepository.SaveAsync()
- Connection string hardcoded (usar IConfiguration)
```csharp
## 🧪 Estratégias de Teste

### **Pirâmide de Testes**

```text
    🔺 E2E Tests (5%)
     Integration Tests (25%)
        Unit Tests (70%)
```text
### **Padrões de Teste**

#### **Unit Tests - Domain Layer**
```csharp
public sealed class UserTests
{
    [Theory]
    [InlineData("", "Descrição", "Nome não pode ser vazio")]
    [InlineData("A", "Descrição", "Nome deve ter pelo menos 2 caracteres")]
    [InlineData("Very very very long name that exceeds maximum", "Descrição", "Nome não pode exceder 100 caracteres")]
    public void Create_InvalidName_ShouldThrowException(string name, string description, string expectedError)
    {
        // Arrange & Act
        var act = () => new FullName(name, "Valid");
        
        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage($"*{expectedError}*");
    }
    
    [Fact]
    public void Create_ValidData_ShouldCreateUser()
    {
        // Arrange
        var externalId = ExternalUserId.From("test-123");
        var email = new Email("test@example.com");
        var fullName = new FullName("John", "Doe");
        
        // Act
        var user = User.Create(externalId, email, fullName, UserType.Customer);
        
        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(UserId.Empty);
        user.Status.Should().Be(UserStatus.Active);
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>();
    }
}
```csharp
#### **Integration Tests - Application Layer**
```csharp
public sealed class RegisterUserCommandHandlerTests : IntegrationTestBase
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            ExternalId: "test-external-id",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            UserType: UserType.Customer
        );
        
        var handler = GetService<IRequestHandler<RegisterUserCommand, RegisterUserResult>>();
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verificar persistência
        var repository = GetService<IUsersRepository>();
        var savedUser = await repository.GetByExternalIdAsync(command.ExternalId);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Value.Should().Be(command.Email);
    }
    
    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange - Criar usuário existente
        await SeedUserAsync("existing@example.com");
        
        var command = new RegisterUserCommand(
            ExternalId: "new-external-id",
            Email: "existing@example.com", // Email duplicado
            FirstName: "Jane",
            LastName: "Doe",
            UserType: UserType.Customer
        );
        
        var handler = GetService<IRequestHandler<RegisterUserCommand, RegisterUserResult>>();
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("já está em uso");
    }
}
```csharp
#### **E2E Tests - API Layer**
```csharp
public sealed class UserEndpointsTests : ApiTestBase
{
    [Fact]
    public async Task RegisterUser_ValidRequest_ShouldReturn201()
    {
        // Arrange
        var request = new RegisterUserRequest(
            ExternalId: "test-external-id",
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            UserType: "Customer"
        );
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/users/register", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        content.Should().NotBeNull();
        content!.UserId.Should().NotBeEmpty();
        
        // Verificar header Location
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/users/{content.UserId}");
    }
    
    [Fact]
    public async Task GetUser_ExistingUser_ShouldReturn200()
    {
        // Arrange
        var userId = await SeedTestUserAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/users/{userId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId.ToString());
    }
}
```text
### **Test Utilities e Builders**

#### **Test Data Builders**
```csharp
public sealed class UserBuilder
{
    private string _externalId = "test-external-id";
    private string _email = "test@example.com";
    private string _firstName = "John";
    private string _lastName = "Doe";
    private UserType _userType = UserType.Customer;
    
    public UserBuilder WithExternalId(string externalId)
    {
        _externalId = externalId;
        return this;
    }
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public UserBuilder AsServiceProvider()
    {
        _userType = UserType.ServiceProvider;
        return this;
    }
    
    public User Build()
    {
        return User.Create(
            ExternalUserId.From(_externalId),
            new Email(_email),
            new FullName(_firstName, _lastName),
            _userType
        );
    }
    
    public RegisterUserCommand BuildCommand()
    {
        return new RegisterUserCommand(_externalId, _email, _firstName, _lastName, _userType);
    }
}

// Uso
var user = new UserBuilder()
    .WithEmail("provider@example.com")
    .AsServiceProvider()
    .Build();
```csharp
## 🔍 Debugging e Troubleshooting

### **Configuração de Debug**

#### **launchSettings.json**
```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7032;http://localhost:5032",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT": "Information",
        "ASPNETCORE_LOGGING__LOGLEVEL__MEAJUDAAI": "Debug"
      }
    },
    "Docker": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "publishAllPorts": true,
      "useSSL": true
    }
  }
}
```text
### **Logs Estruturados**

```csharp
// ✅ Bom - Logs estruturados
_logger.LogInformation(
    "Usuário {UserId} registrado com sucesso. Email: {Email}, Tipo: {UserType}",
    user.Id, user.Email, user.UserType);

// ✅ Bom - Logs com contexto de erro
_logger.LogError(exception,
    "Erro ao registrar usuário. ExternalId: {ExternalId}, Email: {Email}",
    command.ExternalId, command.Email);

// ❌ Ruim - Logs sem estrutura
_logger.LogInformation($"User {user.Id} created");
_logger.LogError("Error occurred: " + exception.Message);
```csharp
### **Ferramentas de Debug**

#### **Serilog Configuration**
```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .WriteTo.Console()
        .WriteTo.File("logs/meajudaai-.log", 
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)
        .WriteTo.Seq("http://localhost:5341") // Se usando Seq
);
```yaml
#### **Application Insights (Produção)**
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
});

// Custom telemetry
public class UserTelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackUserRegistration(User user)
    {
        _telemetryClient.TrackEvent("UserRegistered", new Dictionary<string, string>
        {
            ["UserId"] = user.Id.ToString(),
            ["UserType"] = user.UserType.ToString(),
            ["Email"] = user.Email.Value
        });
    }
}
```bash
## 📦 Package Management

### **Estrutura de Dependências**

```xml
<!-- MeAjudaAi.Shared.csproj - Shared kernel -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.8.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- Module-specific -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
```sql
### **Versionamento**

#### **Central Package Management**
```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
    <PackageVersion Include="MediatR" Version="12.2.0" />
    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.3.0" />
  </ItemGroup>
</Project>
```yaml
## 🛠️ Ferramentas e Scripts

### **Scripts Úteis**

#### **Banco de Dados**
```bash
# Reset completo do banco
./scripts/reset-database.sh

# Aplicar migrations de um módulo específico  
dotnet ef database update --context UsersDbContext

# Gerar migration
dotnet ef migrations add AddUserProfile --context UsersDbContext --output-dir Infrastructure/Persistence/Migrations

# Script SQL da migration
dotnet ef migrations script --context UsersDbContext --output migration.sql
```bash
#### **Testes**
```bash
# Executar todos os testes
dotnet test

# Testes com cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Testes de um módulo específico
dotnet test tests/MeAjudaAi.Modules.Users.Tests/

# Executar teste específico
dotnet test --filter "FullyQualifiedName~RegisterUserCommandHandlerTests"
```yaml
#### **Code Quality**
```bash
# Formatação de código
dotnet format

# Análise estática
dotnet run --project tools/StaticAnalysis

# Security scan
dotnet security-scan
```bash
### **Aliases Úteis**

```bash
# .bashrc ou .zshrc
alias drun="dotnet run"
alias dtest="dotnet test"
alias dbuild="dotnet build"
alias drestore="dotnet restore"
alias dformat="dotnet format"

# Específicos do projeto
alias aspire="cd src/Aspire/MeAjudaAi.AppHost && dotnet run"
alias api="cd src/Bootstrapper/MeAjudaAi.ApiService && dotnet run"
alias migrate="dotnet ef database update --context UsersDbContext"
```text
## 📚 Recursos e Referências

### **Documentação Interna**
- [🏗️ Arquitetura e Padrões](./architecture.md)
- [🚀 Infraestrutura](./infrastructure.md)  
- [🔄 CI/CD](./ci_cd.md)
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