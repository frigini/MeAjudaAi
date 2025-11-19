# Arquitetura e Padrões de Desenvolvimento - MeAjudaAi

Este documento detalha a arquitetura, padrões de design e diretrizes de desenvolvimento do projeto MeAjudaAi.

## 🏗️ Visão Geral da Arquitetura

### **Clean Architecture + DDD**
O MeAjudaAi implementa Clean Architecture combinada com Domain-Driven Design (DDD) para máxima testabilidade e manutenibilidade.

```mermaid
graph TB
    subgraph "🌐 Presentation Layer"
        API[API Controllers]
        MW[Middlewares]
        FIL[Filtros]
    end
    
    subgraph "📋 Application Layer"
        CMD[Commands]
        QRY[Queries]
        HDL[Handlers]
        VAL[Validators]
    end
    
    subgraph "🏛️ Domain Layer"
        ENT[Entities]
        VO[Value Objects]
        DOM[Domain Services]
        EVT[Domain Events]
    end
    
    subgraph "🔧 Infrastructure Layer"
        REPO[Repositories]
        EXT[External Services]
        CACHE[Caching]
        MSG[Messaging]
    end
    
    API --> HDL
    HDL --> DOM
    HDL --> REPO
    REPO --> ENT
    DOM --> ENT
    ENT --> VO
```

### **Modular Monolith**
Estrutura modular que facilita futuras extrações para microserviços.

```
src/
├── Modules/                    # Módulos de domínio
│   ├── Users/                  # Gestão de usuários
│   ├── Providers/              # Prestadores de serviços
│   ├── Services/               # Catálogo de serviços (futuro)
│   ├── Bookings/               # Agendamentos (futuro)
│   └── Payments/               # Pagamentos (futuro)
├── Shared/                     # Componentes compartilhados
│   └── MeAjudaAi.Shared/       # Primitivos e abstrações
├── Bootstrapper/               # Configuração e startup
│   └── MeAjudaAi.ApiService/   # API principal
└── Aspire/                     # Orquestração de desenvolvimento
    ├── MeAjudaAi.AppHost/      # Host Aspire
    └── MeAjudaAi.ServiceDefaults/ # Configurações padrão
```

## 🎯 Domain-Driven Design (DDD)

### **Bounded Contexts**

#### 1. **Users Context** 
**Responsabilidade**: Gestão completa de identidade e perfis de usuário

```csharp
namespace MeAjudaAi.Modules.Users.Domain;

/// <summary>
/// Contexto delimitado para gestão de usuários e identidade
/// </summary>
public class UsersContext
{
    // Entidades principais
    public DbSet<User> Users { get; set; }
    
    // Agregados relacionados
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
}
```bash
**Conceitos do Domínio**:
- **User**: Agregado raiz para dados básicos de identidade
- **UserProfile**: Perfil detalhado (experiência, habilidades, localização)
- **UserPreferences**: Preferências e configurações personalizadas

#### 2. **Providers Context** 
**Responsabilidade**: Gestão completa de prestadores de serviços

```csharp
namespace MeAjudaAi.Modules.Providers.Domain;

/// <summary>
/// Contexto delimitado para gestão de prestadores de serviços
/// </summary>
public class ProvidersContext
{
    // Entidades principais
    public DbSet<Provider> Providers { get; set; }
}
```

**Conceitos do Domínio**:
- **Provider**: Agregado raiz para prestadores de serviços com perfil empresarial
- **BusinessProfile**: Perfil empresarial detalhado (razão social, contato, endereço)
- **Document**: Documentos de verificação (CPF, CNPJ, certificações)
- **Qualification**: Qualificações e habilitações profissionais
- **VerificationStatus**: Status de verificação (Pending, Verified, Rejected, etc.)

#### 3. **service_catalogs Context** (Implementado)
**Responsabilidade**: Catálogo administrativo de categorias e serviços

**Conceitos Implementados**:
- **ServiceCategory**: Categorias hierárquicas de serviços (aggregate root)
- **Service**: Serviços oferecidos vinculados a categorias (aggregate root)
- **DisplayOrder**: Ordenação customizada para apresentação
- **Activation/Deactivation**: Controle de visibilidade no catálogo

**Schema**: `service_catalogs` (isolado no PostgreSQL)

#### 4. **Location Context** (Implementado)
**Responsabilidade**: Geolocalização e lookup de CEP brasileiro

**Conceitos Implementados**:
- **Cep**: Value object para CEP validado
- **Coordinates**: Latitude/Longitude para geolocalização
- **Address**: Endereço completo com dados estruturados
- **CepLookupService**: Integração com ViaCEP, BrasilAPI, OpenCEP (fallback)

**Observação**: Módulo stateless (sem schema próprio), fornece serviços via Module API

#### 5. **Bookings Context** (Futuro)
**Responsabilidade**: Agendamento e execução de serviços

**Conceitos Planejados**:
- **Booking**: Agregado raiz para agendamentos
- **Schedule**: Disponibilidade de prestadores
- **ServiceExecution**: Execução e acompanhamento do serviço

### **Agregados e Entidades**

#### Agregado User

```csharp
/// <summary>
/// Agregado raiz para gestão de usuários do sistema
/// Responsável por manter a consistência dos dados do usuário
/// </summary>
public class User : AggregateRoot<UserId>
{
    /// <summary>Identificador único externo (Keycloak)</summary>
    public ExternalUserId ExternalId { get; private set; }
    
    /// <summary>Email do usuário (único)</summary>
    public Email Email { get; private set; }
    
    /// <summary>Nome completo do usuário</summary>
    public FullName FullName { get; private set; }
    
    /// <summary>Tipo do usuário no sistema</summary>
    public UserType UserType { get; private set; }
    
    /// <summary>Status atual do usuário</summary>
    public UserStatus Status { get; private set; }
    
    /// <summary>Perfil detalhado do usuário</summary>
    public UserProfile Profile { get; private set; }
    
    /// <summary>Preferências do usuário</summary>  
    public UserPreferences Preferences { get; private set; }
}
```

#### Agregado Provider

```csharp
/// <summary>
/// Agregado raiz para gestão de prestadores de serviços
/// Responsável por manter a consistência dos dados do prestador
/// </summary>
public class Provider : AggregateRoot<ProviderId>
{
    /// <summary>Identificador do usuário associado</summary>
    public Guid UserId { get; private set; }
    
    /// <summary>Nome do prestador</summary>
    public string Name { get; private set; }
    
    /// <summary>Tipo do prestador (Individual ou Company)</summary>
    public EProviderType Type { get; private set; }
    
    /// <summary>Perfil empresarial completo</summary>
    public BusinessProfile BusinessProfile { get; private set; }
    
    /// <summary>Status de verificação atual</summary>
    public EVerificationStatus VerificationStatus { get; private set; }
    
    /// <summary>Documentos de verificação</summary>
    public IReadOnlyCollection<Document> Documents { get; }
    
    /// <summary>Qualificações profissionais</summary>  
    public IReadOnlyCollection<Qualification> Qualifications { get; }
}
```

### **Value Objects**

```csharp
/// <summary>
/// Value Object para identificador de usuário
/// Garante type safety e validação de identificadores
/// </summary>
public sealed record UserId(Guid Value) : EntityId(Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public static UserId From(string value) => new(Guid.Parse(value));
}

/// <summary>
/// Value Object para email com validação
/// </summary>
public sealed record Email
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email não pode ser vazio");
            
        if (!EmailValidator.IsValid(value))
            throw new ArgumentException($"Email inválido: {value}");
            
        Value = value.ToLowerInvariant();
    }
    
    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}
```

#### Value Objects do Módulo Providers

```csharp
/// <summary>
/// Value Object para identificador de prestador
/// </summary>
public sealed record ProviderId(Guid Value) : EntityId(Value)
{
    public static ProviderId New() => new(Guid.NewGuid());
    public static ProviderId From(Guid value) => new(value);
}

/// <summary>
/// Value Object para perfil empresarial
/// </summary>
public class BusinessProfile : ValueObject
{
    public string LegalName { get; private set; }
    public string? FantasyName { get; private set; }
    public string? Description { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public Address PrimaryAddress { get; private set; }

    public BusinessProfile(
        string legalName,
        ContactInfo contactInfo,
        Address primaryAddress,
        string? fantasyName = null,
        string? description = null)
    {
        // Validações e inicialização
    }
}

/// <summary>
/// Value Object para documentos
/// </summary>
public class Document : ValueObject
{
    public string Number { get; private set; }
    public EDocumentType DocumentType { get; private set; }
    
    public Document(string number, EDocumentType documentType)
    {
        // Validações e inicialização
    }
}
```

### **Domain Events**

```csharp
/// <summary>
/// Evento disparado quando um novo usuário é registrado
/// </summary>
public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    Email Email,
    UserType UserType,
    DateTime OccurredAt
) : DomainEvent(OccurredAt);

/// <summary>
/// Evento disparado quando perfil do usuário é atualizado
/// </summary>
public sealed record UserProfileUpdatedDomainEvent(
    UserId UserId,
    UserProfile UpdatedProfile,
    DateTime OccurredAt
) : DomainEvent(OccurredAt);
```

#### Domain Events do Módulo Providers

```csharp
/// <summary>
/// Evento disparado quando um novo prestador é registrado
/// </summary>
public sealed record ProviderRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    EProviderType Type,
    string Email
) : DomainEvent(AggregateId, Version);

/// <summary>
/// Evento disparado quando um documento é adicionado
/// </summary>
public sealed record ProviderDocumentAddedDomainEvent(
    Guid AggregateId,
    int Version,
    string DocumentNumber,
    EDocumentType DocumentType
) : DomainEvent(AggregateId, Version);

/// <summary>
/// Evento disparado quando o status de verificação é atualizado
/// </summary>
public sealed record ProviderVerificationStatusUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    EVerificationStatus OldStatus,
    EVerificationStatus NewStatus,
    string? UpdatedBy
) : DomainEvent(AggregateId, Version);

/// <summary>
/// Evento disparado quando um prestador é excluído
/// </summary>
public sealed record ProviderDeletedDomainEvent(
    Guid AggregateId,
    int Version,
    string Reason
) : DomainEvent(AggregateId, Version);
```

## ⚡ CQRS (Command Query Responsibility Segregation)

### **Estrutura de Commands**

```csharp
/// <summary>
/// Command para registro de novo usuário
/// </summary>
public sealed record RegisterUserCommand(
    string ExternalId,
    string Email,
    string FirstName,
    string LastName,
    UserType UserType
) : ICommand<RegisterUserResult>;

/// <summary>
/// Handler para processamento do command RegisterUser
/// </summary>
public sealed class RegisterUserCommandHandler 
    : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUserProfileService _profileService;
    private readonly IEventBus _eventBus;

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken)
    {
        // 1. Validar se usuário já existe
        var existingUser = await _usersRepository
            .GetByExternalIdAsync(command.ExternalId, cancellationToken);
            
        if (existingUser is not null)
            return RegisterUserResult.UserAlreadyExists(command.ExternalId);

        // 2. Criar agregado User
        var user = User.Create(
            ExternalUserId.From(command.ExternalId),
            new Email(command.Email),
            new FullName(command.FirstName, command.LastName),
            command.UserType
        );

        // 3. Criar perfil inicial
        await _profileService.CreateInitialProfileAsync(user.Id, cancellationToken);

        // 4. Persistir
        await _usersRepository.AddAsync(user, cancellationToken);

        // 5. Publicar eventos de domínio
        await _eventBus.PublishAsync(user.DomainEvents, cancellationToken);

        return RegisterUserResult.Success(user.Id);
    }
}
`yaml

### **Estrutura de Queries**

```csharp
/// <summary>
/// Query para buscar usuário por ID
/// </summary>
public sealed record GetUserByIdQuery(UserId UserId) : IQuery<UserDto?>;

/// <summary>
/// Handler para query GetUserById
/// </summary>
public sealed class GetUserByIdQueryHandler 
    : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUsersReadRepository _repository;

    public async Task<UserDto?> Handle(
        GetUserByIdQuery query, 
        CancellationToken cancellationToken)
    {
        return await _repository.GetUserByIdAsync(query.UserId, cancellationToken);
    }
}
`csharp

### **DTOs e Mapeamento**

```csharp
/// <summary>
/// DTO para transferência de dados de usuário
/// </summary>
public sealed record UserDto(
    string Id,
    string ExternalId,
    string Email,
    string FirstName,
    string LastName,
    string UserType,
    string Status,
    UserProfileDto? Profile,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Mapper para conversão entre entidades e DTOs
/// </summary>
public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto(
            Id: user.Id.Value.ToString(),
            ExternalId: user.ExternalId.Value,
            Email: user.Email.Value,
            FirstName: user.FullName.FirstName,
            LastName: user.FullName.LastName,
            UserType: user.UserType.ToString(),
            Status: user.Status.ToString(),
            Profile: user.Profile?.ToDto(),
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
`sql

## 🔌 Dependency Injection e Modularização

### **Registro de Serviços por Módulo**

```csharp
/// <summary>
/// Extensão para registro dos serviços do módulo Users
/// </summary>
public static class UsersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Contexto de banco
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Users")));

        // Repositórios
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUsersReadRepository, UsersReadRepository>();

        // Serviços de domínio
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserValidationService, UserValidationService>();

        // Handlers CQRS
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly));

        // Validators
        services.AddValidatorsFromAssembly(typeof(RegisterUserCommandValidator).Assembly);

        // Event Handlers
        services.AddScoped<INotificationHandler<UserRegisteredDomainEvent>, 
                          SendWelcomeEmailHandler>();

        return services;
    }
}
`csharp

### **Configuração no Program.cs**

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Service Defaults (Aspire)
        builder.AddServiceDefaults();

        // Módulos de domínio
        builder.Services.AddUsersModule(builder.Configuration);
        // builder.Services.AddServicesModule(builder.Configuration); // Futuro
        // builder.Services.AddBookingsModule(builder.Configuration); // Futuro

        // Shared services
        builder.Services.AddSharedServices(builder.Configuration);

        // Infrastructure
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        // Middleware pipeline
        app.UseSharedMiddleware();
        app.MapUsersEndpoints();
        app.MapDefaultEndpoints();

        app.Run();
    }
}
`yaml

## 📡 Event-Driven Architecture

### **Domain Events**

```csharp
/// <summary>
/// Classe base para eventos de domínio
/// </summary>
public abstract record DomainEvent(DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Interface para eventos de domínio
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}

/// <summary>
/// Agregado base com suporte a eventos de domínio
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : EntityId
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
`csharp

### **Implementação do Event Bus**

```csharp
/// <summary>
/// Event Bus para publicação de eventos
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IDomainEvent;
    
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do Event Bus usando MediatR
/// </summary>
public sealed class MediatREventBus : IEventBus
{
    private readonly IMediator _mediator;

    public MediatREventBus(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IDomainEvent
    {
        await _mediator.Publish(@event, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            await _mediator.Publish(@event, cancellationToken);
        }
    }
}
`sql

### **Event Handlers**

```csharp
/// <summary>
/// Handler para evento de usuário registrado
/// </summary>
public sealed class SendWelcomeEmailHandler 
    : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public async Task Handle(
        UserRegisteredDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        try
        {
            var welcomeEmail = new WelcomeEmail(
                To: notification.Email,
                UserType: notification.UserType
            );

            await _emailService.SendAsync(welcomeEmail, cancellationToken);

            _logger.LogInformation(
                "Email de boas-vindas enviado para {Email} (UserId: {UserId})",
                notification.Email, notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao enviar email de boas-vindas para {Email} (UserId: {UserId})",
                notification.Email, notification.UserId);
        }
    }
}
`csharp

## 🛡️ Padrões de Segurança

### **Autenticação e Autorização**

```csharp
/// <summary>
/// Serviço de autenticação integrado com Keycloak
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string token, CancellationToken cancellationToken = default);
    Task<UserContext> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contexto do usuário atual autenticado
/// </summary>
public sealed record UserContext(
    string ExternalId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions
);

/// <summary>
/// Filtro de autorização customizado
/// </summary>
public sealed class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationRequirement
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"RequirePermission:{permission}";
    }
}
`	ext

### **Validation Pattern**

```csharp
/// <summary>
/// Validator para command de registro de usuário
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("ExternalId é obrigatório");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email deve ser válido");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Nome deve ter entre 1 e 100 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Sobrenome deve ter entre 1 e 100 caracteres");

        RuleFor(x => x.UserType)
            .IsInEnum()
            .WithMessage("Tipo de usuário inválido");
    }
}
`csharp

## 🔄 Padrões de Resilência

### **Retry Pattern**

```csharp
/// <summary>
/// Política de retry para operações críticas
/// </summary>
public static class RetryPolicies
{
    public static readonly RetryPolicy DatabaseRetryPolicy = Policy
        .Handle<PostgresException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning(
                    "Tentativa {RetryCount} falhou. Tentando novamente em {Delay}ms",
                    retryCount, timespan.TotalMilliseconds);
            });

    public static readonly RetryPolicy ExternalServiceRetryPolicy = Policy
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(
            retryCount: 2,
            sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
}
`yaml

### **Circuit Breaker Pattern**

```csharp
/// <summary>
/// Circuit Breaker para serviços externos
/// </summary>
public static class CircuitBreakerPolicies
{
    public static readonly CircuitBreakerPolicy ExternalServiceCircuitBreaker = Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (exception, duration) =>
            {
                // Log circuit breaker opened
            },
            onReset: () =>
            {
                // Log circuit breaker closed
            });
}
`csharp

## 📊 Observabilidade e Monitoramento

### **Logging Structure**

```csharp
/// <summary>
/// Logger estruturado para operações de usuário
/// </summary>
public static partial class UserLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Usuário {UserId} registrado com sucesso (Email: {Email}, Type: {UserType})")]
    public static partial void UserRegistered(
        this ILogger logger, string userId, string email, string userType);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Tentativa de registro de usuário duplicado (ExternalId: {ExternalId})")]
    public static partial void DuplicateUserRegistration(
        this ILogger logger, string externalId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Erro ao registrar usuário (ExternalId: {ExternalId})")]
    public static partial void UserRegistrationFailed(
        this ILogger logger, string externalId, Exception exception);
}
`	ext

### **Métricas Personalizadas**

```csharp
/// <summary>
/// Métricas customizadas para o módulo Users
/// </summary>
public sealed class UserMetrics
{
    private readonly Counter<int> _userRegistrationsCounter;
    private readonly Histogram<double> _registrationDuration;
    private readonly ObservableGauge<int> _activeUsersGauge;

    public UserMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MeAjudaAi.Users");

        _userRegistrationsCounter = meter.CreateCounter<int>(
            "user_registrations_total",
            description: "Total de registros de usuários");

        _registrationDuration = meter.CreateHistogram<double>(
            "user_registration_duration_ms",
            description: "Duração do processo de registro de usuário");

        _activeUsersGauge = meter.CreateObservableGauge<int>(
            "active_users_total",
            description: "Número atual de usuários ativos");
    }

    public void RecordUserRegistration(UserType userType, double durationMs)
    {
        _userRegistrationsCounter.Add(1, 
            new KeyValuePair<string, object?>("user_type", userType.ToString()));
        
        _registrationDuration.Record(durationMs,
            new KeyValuePair<string, object?>("user_type", userType.ToString()));
    }
}
`csharp

## 🧪 Padrões de Teste

### **Test Structure**

```csharp
/// <summary>
/// Classe base para testes de unidade do domínio
/// </summary>
public abstract class DomainTestBase
{
    protected static User CreateValidUser(
        string externalId = "test-external-id",
        string email = "test@example.com",
        UserType userType = UserType.Customer)
    {
        return User.Create(
            ExternalUserId.From(externalId),
            new Email(email),
            new FullName("Test", "User"),
            userType
        );
    }
}

/// <summary>
/// Testes para o agregado User
/// </summary>
public sealed class UserTests : DomainTestBase
{
    [Fact]
    public void Create_ValidData_ShouldCreateUser()
    {
        // Arrange
        var externalId = ExternalUserId.From("test-id");
        var email = new Email("test@example.com");
        var fullName = new FullName("Test", "User");
        var userType = UserType.Customer;

        // Act
        var user = User.Create(externalId, email, fullName, userType);

        // Assert
        user.Should().NotBeNull();
        user.ExternalId.Should().Be(externalId);
        user.Email.Should().Be(email);
        user.FullName.Should().Be(fullName);
        user.UserType.Should().Be(userType);
        user.Status.Should().Be(UserStatus.Active);
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>();
    }
}
`yaml

### **Integration Tests**

```csharp
/// <summary>
/// Classe base para testes de integração
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
    }

    protected T GetService<T>() where T : notnull
        => Scope.ServiceProvider.GetRequiredService<T>();
}

/// <summary>
/// Testes de integração para endpoints de usuário
/// </summary>
public sealed class UserEndpointsTests : IntegrationTestBase
{
    public UserEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RegisterUser_ValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterUserRequest(
            ExternalId: "test-external-id",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            UserType: "Customer"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
    }
}
`csharp

## 🔌 Module APIs - Comunicação Entre Módulos

### **Padrão Module APIs**

O padrão Module APIs é usado para comunicação síncrona e type-safe entre módulos. Cada módulo pode expor uma API pública através de uma interface bem definida, permitindo que outros módulos a consumam diretamente, sem acoplamento forte com a implementação interna.

### **Estrutura Recomendada**

```csharp
/// <summary>
/// Interface da API pública do módulo Users
/// Define contratos para comunicação síncrona entre módulos.
/// </summary>
public interface IUsersModuleApi : IModuleApi
{
    Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação da API do módulo Users
/// Localizada em: src/Modules/Users/Application/ModuleApi/
/// </summary>
[ModuleApi("Users", "1.0")]
public sealed class UsersModuleApi : IUsersModuleApi
{
    // A implementação utiliza os handlers e serviços internos do módulo Users
    // para responder às solicitações, sem expor detalhes da camada de domínio.
}
```

---

## 📡 Integration Events - Comunicação Assíncrona

### **Padrão Integration Events**

Para comunicação assíncrona e desacoplada, o projeto utiliza o padrão de **Integration Events**. Um módulo publica um evento em um message bus (como RabbitMQ ou Azure Service Bus) quando um estado importante é alterado. Outros módulos podem se inscrever para receber notificações desses eventos e reagir a eles, sem que o publicador precise conhecê-los.

Este padrão é ideal para:
- Notificar outros módulos sobre a criação, atualização ou exclusão de entidades.
- Disparar fluxos de trabalho em background.
- Manter a consistência eventual entre diferentes Bounded Contexts.

### **Estrutura Recomendada**

```csharp
/// <summary>
/// Define um evento de integração que ocorreu no sistema.
/// Herda de IEvent e adiciona um campo 'Source' para identificar o módulo de origem.
/// </summary>
public interface IIntegrationEvent : IEvent
{
    string Source { get; }
}

/// <summary>
/// Exemplo de um evento de integração publicado quando um usuário é registrado.
/// Este evento carrega os dados essenciais para que outros módulos possam reagir.
/// </summary>
public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Username,
    string Email,
    DateTime RegisteredAt
) : IIntegrationEvent;
```

---

## 🚦 Status Atual da Implementação

Atualmente, a arquitetura do projeto **define os padrões** para comunicação síncrona (`IModuleApi`) e assíncrona (`IIntegrationEvent`), mas **eles ainda não foram implementados para comunicação entre os módulos existentes**.

- **`IModuleApi`**: As interfaces e implementações estão definidas dentro de seus respectivos módulos, mas nenhum módulo está injetando ou consumindo a API de outro módulo.
- **`IIntegrationEvent`**: Os eventos estão definidos no projeto `Shared`, mas não há `Handlers` nos módulos para consumir esses eventos. Os módulos atualmente lidam apenas com `Domain Events` internos.

A sua percepção de que os módulos não se comunicam está correta. O próximo passo no desenvolvimento é implementar esses padrões para criar um sistema coeso.

---

## 💡 Exemplos Conceituais de Implementação

A seguir, exemplos de como implementar os dois padrões de comunicação.

### 1. Exemplo de `IModuleApi` (Comunicação Síncrona)

**Cenário**: Ao criar um novo `Provider`, o módulo `Providers` precisa verificar se o `UserId` associado já existe no módulo `Users`.

**Passos de Implementação**:

1.  **Injetar `IUsersModuleApi`**: No `CreateProviderCommandHandler` do módulo `Providers`, injete a interface `IUsersModuleApi`.
2.  **Chamar o Método da API**: Utilize o método `CheckUserExistsAsync` para validar a existência do usuário.

**Exemplo de Código (Conceitual):**

```csharp
// Local: C:\Code\MeAjudaAi\src\Modules\Providers\Application\Providers\Commands\CreateProvider\CreateProviderCommandHandler.cs

// 1. Injetar a IUsersModuleApi
public class CreateProviderCommandHandler(IUsersModuleApi usersModuleApi, /* outras dependências */) 
    : IRequestHandler<CreateProviderCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // 2. Chamar a API para verificar se o usuário existe
        var userExistsResult = await _usersModuleApi.CheckUserExistsAsync(request.UserId, cancellationToken);

        if (userExistsResult.IsFailure || !userExistsResult.Value)
        {
            return Result.Failure<ProviderDto>(new Error("User.NotFound", "O usuário especificado não existe."));
        }

        // --- Lógica para criação do provider ---
        // ...
    }
}
```

### 2. Exemplo de `IIntegrationEvent` (Comunicação Assíncrona)

**Cenário**: Quando um novo usuário se registra, o módulo `Users` publica um `UserRegisteredIntegrationEvent`. O módulo `Search` escuta este evento para indexar o novo usuário em seu sistema de busca.

**Passos de Implementação**:

**A. Publicando o Evento (Módulo `Users`)**

1.  **Injetar `IMessageBus`**: No `CreateUserCommandHandler`, injete o serviço de message bus.
2.  **Publicar o Evento**: Após criar o usuário com sucesso, publique o evento no barramento.

**Exemplo de Código (Publicador):**

```csharp
// Local: C:\Code\MeAjudaAi\src\Modules\Users\Application\Users\Commands\CreateUser\CreateUserCommandHandler.cs

// 1. Injetar o message bus
public class CreateUserCommandHandler(IMessageBus messageBus, /* outras dependências */)
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // --- Lógica para criar o usuário ---
        var user = new User(/* ... */);
        await _userRepository.AddAsync(user, cancellationToken);
        
        // 2. Criar e publicar o evento de integração
        var integrationEvent = new UserRegisteredIntegrationEvent(
            user.Id.Value,
            user.Username.Value,
            user.Email.Value,
            user.CreatedAt
        );
        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return Result.Success(user.ToDto());
    }
}
```

**B. Consumindo o Evento (Módulo `Search`)**

1.  **Criar um Event Handler**: No módulo `Search`, crie uma classe que implementa `IEventHandler<UserRegisteredIntegrationEvent>`.
2.  **Implementar a Lógica**: No método `HandleAsync`, implemente a lógica para indexar o usuário.
3.  **Registrar o Handler**: Adicione o handler no contêiner de injeção de dependência do módulo `Search`.

**Exemplo de Código (Consumidor):**

```csharp
// Local: C:\Code\MeAjudaAi\src\Modules\Search\Application\EventHandlers\UserRegisteredIntegrationEventHandler.cs

// 1. Criar o handler
public class UserRegisteredIntegrationEventHandler : IEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer;
    public UserRegisteredIntegrationEventHandler(ISearchIndexer searchIndexer)
    {
        _searchIndexer = searchIndexer;
    }

    // 2. Implementar a lógica de tratamento
    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var userDocument = new SearchableUser
        {
            Id = @event.UserId,
            Username = @event.Username,
            Email = @event.Email
        };
        await _searchIndexer.IndexUserAsync(userDocument, cancellationToken);
    }
}

// Local: C:\Code\MeAjudaAi\src\Modules\Search\Infrastructure\Extensions.cs
public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // ... outras configurações

    // 3. Registrar o handler
    services.AddScoped<IEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredIntegrationEventHandler>();

    return services;
}
```

## 📡 API Collections e Documentação

### **Estratégia Multi-Formato**

O projeto utiliza múltiplos formatos de collections para diferentes necessidades:

#### **1. OpenAPI/Swagger (PRINCIPAL)**
- 🎯 **Documentação oficial** gerada automaticamente do código
- 🔄 **Sempre atualizada** com o código fonte
- 🌐 **Padrão da indústria** para APIs REST
- 📊 **UI interativa** disponível em `/api-docs`

```csharp
// Endpoints automaticamente documentados
[HttpPost("register")]
[ProducesResponseType<RegisterUserResponse>(201)]
[ProducesResponseType<ApiErrorResponse>(400)]
public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
{
    // Implementação...
}
```

#### **2. Bruno Collections (.bru) - DESENVOLVIMENTO**
- ✅ **Controle de versão** no Git
- ✅ **Leve e eficiente** para desenvolvedores
- ✅ **Variáveis de ambiente** configuráveis
- ✅ **Scripts pré/pós-request** em JavaScript

```
# Estrutura Bruno
src/Shared/API.Collections/
├── Common/
│   ├── GlobalVariables.bru
│   ├── StandardHeaders.bru
│   └── EnvironmentVariables.bru
├── Setup/
│   ├── SetupGetKeycloakToken.bru
│   └── HealthCheckAll.bru
└── Modules/
    └── Users/
        ├── CreateUser.bru
        ├── GetUsers.bru
        └── UpdateUser.bru
`$([System.Environment]::NewLine)

- 🤝 **Compartilhamento fácil** com QA, PO, clientes
- 🔄 **Geração automática** via OpenAPI
- 🧪 **Testes automáticos** integrados
- 📊 **Monitoring e reports** nativos

### **Geração Automática de Collections**

#### **Comandos Disponíveis**

`ash

# Gerar todas as collections
cd tools/api-collections
./generate-all-collections.sh        # Linux/Mac
./generate-all-collections.bat       # Windows

# Apenas Postman
npm run generate:postman

# Validar collections
npm run validate
```

#### **Estrutura de Output**

```text
src/Shared/API.Collections/Generated/
├── MeAjudaAi-API-Collection.json           # Collection principal
├── MeAjudaAi-development-Environment.json  # Ambiente desenvolvimento
├── MeAjudaAi-staging-Environment.json      # Ambiente staging
├── MeAjudaAi-production-Environment.json   # Ambiente produção
└── README.md                               # Instruções de uso
```

### **Configurações Avançadas do Swagger**

#### **Filtros Personalizados**

```
// Exemplos automáticos baseados em convenções
options.SchemaFilter<ExampleSchemaFilter>();

// Tags organizadas por módulos
options.DocumentFilter<ModuleTagsDocumentFilter>();

// Versionamento de API
options.OperationFilter<ApiVersionOperationFilter>();
`sql

#### **Melhorias Implementadas**

- **📝 Exemplos Inteligentes**: Baseados em nomes de propriedades e tipos
- **🏷️ Tags Organizadas**: Agrupamento lógico por módulos
- **🔒 Segurança JWT**: Configuração automática de Bearer tokens
- **📊 Schemas Reutilizáveis**: Componentes comuns (paginação, erros)
- **🌍 Multi-ambiente**: URLs para dev/staging/production

### **Boas Práticas para Collections**

#### **✅ RECOMENDADO**

1. **Manter OpenAPI como fonte única da verdade**
2. **Bruno para desenvolvimento diário**
3. **Postman para colaboração e testes**
4. **Regenerar collections após mudanças na API**
5. **Versionar Bruno collections no Git**

#### **❌ EVITAR**

1. **Edição manual de Postman collections geradas**
2. **Duplicação de documentação entre formatos**
3. **Collections desatualizadas sem regeneração**
4. **Hardcoding de URLs nos collections**

### **Workflow Recomendado**

1. **Desenvolver** API com documentação OpenAPI
2. **Testar** localmente com Bruno collections
3. **Gerar** Postman collections para colaboração
4. **Compartilhar** com equipe via Postman workspace
5. **Regenerar** collections em cada release

### **Exportação OpenAPI para Clientes REST**

#### **Comando Único**
`ash

# Gera especificação OpenAPI completa
.\scripts\export-openapi.ps1 -OutputPath "api/api-spec.json"
```

**Características:**
- ✅ **Funciona offline** (não precisa rodar aplicação)
- ✅ **Health checks incluídos** (/health, /health/ready, /health/live)  
- ✅ **Schemas com exemplos** realistas
- ✅ **Múltiplos ambientes** (dev, staging, production)
- ⚠️ **Arquivo não versionado** (incluído no .gitignore)

#### **Importar em Clientes de API**

**APIDog**: Importar → Do Arquivo → Selecionar arquivo  
**Postman**: Importar → Arquivo → Fazer Envio de Arquivos → Selecionar arquivo  
**Insomnia**: Import/Export → Import Data → Selecionar arquivo  
**Bruno**: Import → OpenAPI → Selecionar arquivo  
**Thunder Client**: Import → OpenAPI → Selecionar arquivo  

### **Monitoramento e Testes**

Especificação OpenAPI inclui:

- ✅ **Health endpoints** para monitoramento
- ✅ **Schemas de erro** padronizados  
- ✅ **Paginação** consistente
- ✅ **Exemplos realistas** para desenvolvimento
- ✅ **Documentação rica** com descrições detalhadas

```json
// Health check response example
{
  "status": "Saudável",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0",
  "environment": "Development",
  "checks": {
    "database": { "status": "Healthy", "duration": "00:00:00.0123456" },
    "cache": { "status": "Healthy", "duration": "00:00:00.0087432" }
  }
}
```text
---

📖 **Próximos Passos**: Este documento serve como base para o desenvolvimento. Consulte também a [documentação de infraestrutura](./infrastructure.md) e [guia de CI/CD](./ci_cd.md) para informações complementares.