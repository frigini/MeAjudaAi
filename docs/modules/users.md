# 👥 Módulo Users - Gestão de Usuários

Este documento detalha a implementação completa do módulo Users, responsável pela gestão de usuários e integração com autenticação na plataforma MeAjudaAi.

## 🎯 Visão Geral

O módulo Users implementa um **Bounded Context** dedicado para gestão de identidade e perfis de usuários, seguindo os princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**.

### **Responsabilidades Principais**
- ✅ **Registro e gestão de usuários**
- ✅ **Integração com Keycloak** para autenticação externa
- ✅ **Perfis de usuário** detalhados
- ✅ **Preferências personalizadas**
- ✅ **Soft delete** e gestão de lifecycle
- ✅ **Module API** para comunicação entre módulos

## 🏗️ Arquitetura do Módulo

### **Estrutura de Pastas**
```
src/Modules/Users/
├── API/                           # Camada de apresentação
│   ├── Endpoints/                 # Minimal APIs
│   ├── Extensions.cs              # Registro de serviços
│   └── Mappers/                   # Mapeamento entre camadas
├── Application/                   # Camada de aplicação
│   ├── Commands/                  # Commands (CQRS)
│   ├── Queries/                   # Queries (CQRS)
│   ├── Handlers/                  # Command/Query handlers
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Services/                  # Module API e serviços
│   └── Mappers/                   # Mapeadores
├── Domain/                        # Camada de domínio
│   ├── Entities/                  # Agregados e entidades
│   ├── ValueObjects/              # Value Objects
│   ├── Events/                    # Domain Events
│   ├── Exceptions/                # Exceções de domínio
│   ├── Services/                  # Domain Services
│   └── Repositories/              # Interfaces de repositório
├── Infrastructure/                # Camada de infraestrutura
│   ├── Persistence/               # Entity Framework
│   │   ├── Configurations/        # Configurações EF
│   │   └── Migrations/            # Migrações
│   ├── Identity/                  # Integração Keycloak
│   ├── Services/                  # Implementações de serviços
│   └── Repositories/              # Implementações de repositório
└── Tests/                         # Testes unitários
    └── Unit/                      # Testes por camada
```

## 🎭 Domain Model

### **Agregado Principal: User**

```csharp
/// <summary>
/// Agregado raiz para gestão de usuários do sistema
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public string KeycloakId { get; private set; }      // ID externo do Keycloak
    public string Username { get; private set; }        // Nome de usuário único
    public string Email { get; private set; }           // Email único
    public string FirstName { get; private set; }       // Nome
    public string LastName { get; private set; }        // Sobrenome
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Métodos de negócio
    public string GetFullName() => $"{FirstName} {LastName}".Trim();
    public void UpdateProfile(string firstName, string lastName);
    public void MarkAsDeleted();
}
```

### **Value Objects**

#### **UserId**
```csharp
public sealed record UserId(Guid Value) : EntityId(Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public static UserId From(string value) => new(Guid.Parse(value));
}
```

#### **UserProfile**
```csharp
public class UserProfile : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public PhoneNumber? PhoneNumber { get; }
    public string FullName => $"{FirstName} {LastName}";
    
    public UserProfile(string firstName, string lastName, PhoneNumber? phoneNumber = null)
    {
        // Validações e inicialização
    }
}
```

#### **PhoneNumber**
```csharp
public class PhoneNumber : ValueObject
{
    public string Value { get; }
    
    public PhoneNumber(string value)
    {
        // Validação de formato de telefone
    }
}
```

## 🔄 Domain Events

### **Eventos Implementados**
```csharp
/// <summary>
/// Evento disparado quando um novo usuário é registrado
/// </summary>
public record UserRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    string KeycloakId,
    string Username,
    string Email
) : DomainEvent(AggregateId, Version);

/// <summary>
/// Evento disparado quando perfil é atualizado
/// </summary>
public record UserProfileUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);
```

## ⚡ CQRS Implementation

### **Commands**
- ✅ **RegisterUserCommand**: Registro de novo usuário
- ✅ **UpdateUserProfileCommand**: Atualização de perfil
- ✅ **DeleteUserCommand**: Exclusão lógica

### **Queries**
- ✅ **GetUserByIdQuery**: Busca por ID
- ✅ **GetUserByKeycloakIdQuery**: Busca por ID do Keycloak
- ✅ **GetUserByEmailQuery**: Busca por email
- ✅ **GetUserByUsernameQuery**: Busca por username
- ✅ **GetUsersByIdsQuery**: Busca em lote

## 🌐 API Endpoints

### **Endpoints Principais**
- ✅ `POST /api/v1/users/register` - Registrar usuário
- ✅ `GET /api/v1/users` - Listar usuários (paginado)
- ✅ `GET /api/v1/users/{id}` - Obter usuário por ID
- ✅ `PUT /api/v1/users/{id}` - Atualizar perfil
- ✅ `DELETE /api/v1/users/{id}` - Excluir usuário
- ✅ `GET /api/v1/users/check-email` - Verificar disponibilidade de email

## 🔌 Module API

### **Interface IUsersModuleApi**
```csharp
public interface IUsersModuleApi : IModuleApi
{
    Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleUserBasicDto>>> GetUsersBatchAsync(IReadOnlyList<Guid> userIds, CancellationToken cancellationToken = default);
    Task<Result<bool>> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

### **DTOs para Module API**
```csharp
public sealed record ModuleUserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string FullName
);

public sealed record ModuleUserBasicDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive
);
```

## 🔐 Integração com Keycloak

### **Fluxo de Autenticação**
1. **Usuário autentica** no Keycloak
2. **JWT token** é validado pela aplicação
3. **Usuário é sincronizado** automaticamente se não existir
4. **Contexto de usuário** é estabelecido para a sessão

### **Domain Service: KeycloakUserDomainService**
```csharp
public interface IKeycloakUserDomainService
{
    Task<Result<TokenValidationResult>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Result<bool>> UserExistsInKeycloakAsync(string keycloakId, CancellationToken cancellationToken = default);
}
```

## 🗄️ Persistência

### **Schema de Banco**
```sql
-- Tabela principal de usuários
CREATE TABLE users.Users (
    Id uuid PRIMARY KEY,
    KeycloakId varchar(255) NOT NULL UNIQUE,
    Username varchar(100) NOT NULL UNIQUE,
    Email varchar(255) NOT NULL UNIQUE,
    FirstName varchar(100) NOT NULL,
    LastName varchar(100) NOT NULL,
    IsDeleted boolean NOT NULL DEFAULT false,
    DeletedAt timestamp NULL,
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp NULL
);

-- Índices para performance
CREATE INDEX idx_users_keycloak_id ON users.Users(KeycloakId);
CREATE INDEX idx_users_email ON users.Users(Email);
CREATE INDEX idx_users_username ON users.Users(Username);
CREATE INDEX idx_users_deleted ON users.Users(IsDeleted, DeletedAt);
```

### **Configuração Entity Framework**
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "users");
        
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value));
            
        builder.Property(u => u.KeycloakId)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(u => u.Username)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();
            
        // Índices únicos
        builder.HasIndex(u => u.KeycloakId).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        
        // Soft delete
        builder.HasIndex(u => new { u.IsDeleted, u.DeletedAt });
    }
}
```

## 🧪 Estratégia de Testes

### **Cobertura Completa**
- ✅ **Testes Unitários**: Domain, Application, Infrastructure
- ✅ **Testes de Integração**: API endpoints completos
- ✅ **Testes de Module API**: Comunicação entre módulos
- ✅ **Testes Arquiteturais**: Validação de dependências

### **Padrões de Teste**
```csharp
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Domain")]
public class UserTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var keycloakId = "keycloak-123";
        var username = "testuser";
        var email = "test@example.com";
        var firstName = "Test";
        var lastName = "User";

        // Act
        var user = new User(keycloakId, username, email, firstName, lastName);

        // Assert
        user.KeycloakId.Should().Be(keycloakId);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsDeleted.Should().BeFalse();
        
        // Verifica evento de domínio
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>();
    }
}
```

## 📊 Métricas e Observabilidade

### **Logs Estruturados**
```csharp
[LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Information,
    Message = "User {UserId} registered successfully (Email: {Email}, Username: {Username})")]
public static partial void UserRegistered(
    this ILogger logger, Guid userId, string email, string username);

[LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Warning,
    Message = "Duplicate user registration attempt (KeycloakId: {KeycloakId})")]
public static partial void DuplicateUserRegistration(
    this ILogger logger, string keycloakId);
```

### **Métricas Customizadas**
- **user_registrations_total**: Total de registros
- **user_authentication_duration_ms**: Tempo de autenticação
- **active_users_total**: Usuários ativos
- **keycloak_sync_operations_total**: Operações de sincronização

## 🔧 Configuração

### **Registro no DI Container**
```csharp
public static class UsersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Users")));

        // Repositórios
        services.AddScoped<IUsersRepository, UsersRepository>();

        // Domain Services
        services.AddScoped<IKeycloakUserDomainService, KeycloakUserDomainService>();

        // Handlers CQRS
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly));

        // Validators
        services.AddValidatorsFromAssembly(typeof(RegisterUserCommandValidator).Assembly);

        // Module API
        services.AddScoped<IUsersModuleApi, UsersModuleApi>();

        // Keycloak integration
        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));
        services.AddHttpClient<IKeycloakService, KeycloakService>();

        return services;
    }
}
```

### **Configuração do Keycloak**
```json
{
  "Keycloak": {
    "Authority": "https://keycloak.exemplo.com/realms/meajudaai",
    "ClientId": "meajudaai-api",
    "ClientSecret": "client-secret",
    "RequireHttpsMetadata": true,
    "ValidateAudience": true,
    "ValidateIssuer": true,
    "ClockSkew": "00:05:00"
  }
}
```

## 🔗 Integração com Outros Módulos

### **Módulo Providers**
O módulo Users fornece a base de identidade para prestadores de serviços:

```csharp
// Providers referencia Users via UserId
public class Provider : AggregateRoot<ProviderId>
{
    public Guid UserId { get; private set; }  // Referência ao User
    // ... outros dados específicos do provider
}
```

### **Comunicação via Module API**
```csharp
// Exemplo de uso em outro módulo
public class SomeOtherModuleService
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<Result> ProcessUserData(Guid userId)
    {
        var userResult = await _usersApi.GetUserByIdAsync(userId);
        if (userResult.IsFailure)
            return Result.Failure("User not found");
            
        var user = userResult.Value;
        // Processar dados do usuário...
    }
}
```

## 🚀 Próximos Passos

### **Funcionalidades Planejadas**
- 🔄 **Avatar e fotos de perfil**
- 🔄 **Preferências avançadas** (notificações, privacidade)
- 🔄 **Histórico de atividades**
- 🔄 **Integração com redes sociais**
- 🔄 **Two-factor authentication**

### **Melhorias Técnicas**
- 🔄 **Cache distribuído** para consultas frequentes
- 🔄 **Event Sourcing** para auditoria
- 🔄 **Background sync** com Keycloak
- 🔄 **Bulk operations** para gestão em massa

---

## 📚 Referências

- **[Arquitetura Geral](../architecture.md)** - Padrões e estrutura
- **[Autenticação](../authentication.md)** - Integração com Keycloak
- **[Módulo Providers](./providers.md)** - Integração com prestadores
- **[Guia de Desenvolvimento](../development.md)** - Setup e diretrizes

---

*📅 Última atualização: Novembro 2025*  
*✨ Documentação mantida pela equipe de desenvolvimento MeAjudaAi*