# AGENT.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

**MeAjudaAi** is a service marketplace platform built with .NET 9 and .NET Aspire, implementing a Modular Monolith architecture with Domain-Driven Design (DDD), CQRS, and Event-Driven patterns. The platform connects service providers with customers.

### Key Technologies
- **.NET 9** with C# 12
- **.NET Aspire** for orchestration and observability
- **PostgreSQL 15+** as primary database
- **Entity Framework Core 9** for data access
- **Keycloak** for authentication/authorization
- **Redis** for distributed caching
- **RabbitMQ/Azure Service Bus** for messaging
- **xUnit v3** for testing

## Development Commands

### Running the Application

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

### Building

```powershell
# Build entire solution
dotnet build

# Build specific configuration
dotnet build --configuration Release

# Restore dependencies
dotnet restore
```

### Testing

```powershell
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific module tests
dotnet test src\Modules\Users\Tests\
dotnet test src\Modules\Providers\Tests\

# Run specific test categories
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Generate HTML coverage report (requires reportgenerator tool)
reportgenerator -reports:"coverage\**\coverage.opencover.xml" -targetdir:"coverage\html" -reporttypes:Html
```

### Database Migrations

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

### Code Quality

```powershell
# Format code according to .editorconfig
dotnet format

# Run code analysis
dotnet build --verbosity normal

# Clean build artifacts
dotnet clean
```

### API Documentation

```powershell
# Generate OpenAPI spec for API clients (APIDog, Postman, Insomnia, Bruno)
.\scripts\export-openapi.ps1

# Specify custom output path
.\scripts\export-openapi.ps1 -OutputPath "docs\api-spec.json"

# Access Swagger UI when running:
# https://localhost:7524/swagger
```

## Architecture Overview

### Modular Monolith Structure

The codebase follows a Modular Monolith pattern where each module is independently deployable but runs in the same process for development simplicity. Future extraction to microservices is possible.

```
src/
├── Aspire/                          # .NET Aspire orchestration
│   ├── MeAjudaAi.AppHost/          # Host application
│   └── MeAjudaAi.ServiceDefaults/  # Shared configurations
├── Bootstrapper/
│   └── MeAjudaAi.ApiService/       # Main API entry point
├── Modules/                         # Domain modules (bounded contexts)
│   ├── Users/                      # User management module
│   │   ├── API/                    # HTTP endpoints (Minimal APIs)
│   │   ├── Application/            # CQRS handlers (Commands/Queries)
│   │   ├── Domain/                 # Domain entities, value objects, events
│   │   ├── Infrastructure/         # EF Core, repositories, external services
│   │   └── Tests/                  # Unit and integration tests
│   └── Providers/                  # Service provider module
│       ├── API/
│       ├── Application/
│       ├── Domain/
│       ├── Infrastructure/
│       └── Tests/
└── Shared/
    └── MeAjudaAi.Shared/           # Cross-cutting concerns, abstractions
```

### Key Architectural Patterns

#### 1. Clean Architecture Layers
- **Domain Layer**: Entities, Value Objects, Domain Events, Domain Services
- **Application Layer**: CQRS Commands/Queries, Handlers, Application Services
- **Infrastructure Layer**: EF Core DbContexts, Repositories, External Integrations
- **Presentation Layer**: Minimal API endpoints, DTOs

#### 2. CQRS (Command Query Responsibility Segregation)
- **Commands**: Modify state (RegisterUserCommand, UpdateProviderCommand)
- **Queries**: Read data (GetUserByIdQuery, GetProvidersQuery)
- **Handlers**: Process commands/queries with MediatR
- **Validators**: FluentValidation for input validation

#### 3. Domain-Driven Design
- **Aggregates**: User, Provider (with aggregate roots)
- **Value Objects**: Email, UserId, ProviderId, Address, ContactInfo
- **Domain Events**: UserRegisteredDomainEvent, ProviderVerificationStatusUpdatedDomainEvent
- **Bounded Contexts**: Users, Providers (future: Services, Bookings, Payments)

#### 4. Event-Driven Architecture
- **Domain Events**: Internal module communication via MediatR
- **Integration Events**: Cross-module communication via message bus
- **Event Handlers**: React to events (e.g., send welcome email on user registration)

#### 5. Module APIs Pattern
- Modules expose public APIs via interfaces (e.g., `IUsersModuleApi`, `IProvidersModuleApi`)
- In-process, type-safe communication between modules
- DTOs in `Shared/Contracts/Modules/{ModuleName}/DTOs/`
- Located in `{Module}/Application/Services/`

### Database Per Module (Schema Isolation)
Each module has its own PostgreSQL schema:
- **Users**: `meajudaai_users` schema
- **Providers**: `meajudaai_providers` schema
- **Future modules**: Dedicated schemas for isolation

### Important Design Decisions

#### UUID v7 for IDs
The project uses .NET 9's UUID v7 (time-ordered) instead of UUID v4 for:
- Better database indexing performance
- Natural chronological ordering
- Compatibility with PostgreSQL 18+
- Centralized generation via `UuidGenerator` in `MeAjudaAi.Shared.Time`

#### Central Package Management
- All package versions defined in `Directory.Packages.props`
- xUnit v3 used for all tests
- Consistent versioning across solution

#### Code Quality Standards
- EditorConfig enforced via `.editorconfig`
- SonarAnalyzer for static analysis
- Test authentication handler for integration tests
- No warnings as errors (by design for flexibility)

## Development Guidelines

### Adding a New Module

Follow the established pattern from existing modules:

1. **Create module structure**:
   ```text
   src/Modules/{ModuleName}/
   ├── API/
   ├── Application/
   │   ├── Commands/
   │   ├── Queries/
   │   └── Services/
   ├── Domain/
   │   ├── Entities/
   │   ├── ValueObjects/
   │   └── Events/
   ├── Infrastructure/
   │   ├── Persistence/
   │   └── Repositories/
   └── Tests/
   ```

2. **Register module in `Program.cs`**:
   ```csharp
   builder.Services.Add{ModuleName}Module(builder.Configuration);
   ```

3. **Update CI/CD workflow** (`.github/workflows/pr-validation.yml`):
   ```bash
   MODULES=(
     "Users:src/Modules/Users/MeAjudaAi.Modules.Users.Tests/"
     "Providers:src/Modules/Providers/MeAjudaAi.Modules.Providers.Tests/"
     "{ModuleName}:src/Modules/{ModuleName}/MeAjudaAi.Modules.{ModuleName}.Tests/"
   )
   ```

4. **Create database schema** in `infrastructure/database/schemas/`

5. **Add Module API** in `Shared/Contracts/Modules/{ModuleName}/`

### Naming Conventions

```csharp
// Commands: [Verb][Entity]Command
public sealed record RegisterUserCommand(...);
public sealed record UpdateProviderCommand(...);

// Queries: Get[Entity]By[Criteria]Query
public sealed record GetUserByIdQuery(UserId UserId);
public sealed record GetProvidersByStatusQuery(EVerificationStatus Status);

// Handlers: [Command/Query]Handler
public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result>;

// Value Objects: PascalCase, sealed records
public sealed record UserId(Guid Value);
public sealed record Email(string Value);

// Domain Events: [Entity][Action]DomainEvent
public sealed record UserRegisteredDomainEvent(...);
public sealed record ProviderDeletedDomainEvent(...);
```

### Testing Strategy

#### Test Structure
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and dependencies
    var command = new RegisterUserCommand(...);
    
    // Act - Execute the operation
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert - Verify the outcome
    result.IsSuccess.Should().BeTrue();
}
```

#### Test Categories
- **Unit Tests**: Domain logic, command/query handlers (in module Tests/ folder)
- **Integration Tests**: API endpoints, database operations (tests/MeAjudaAi.Integration.Tests/)
- **E2E Tests**: Full user scenarios (tests/MeAjudaAi.E2E.Tests/)
- **Architecture Tests**: Enforce architectural rules (tests/MeAjudaAi.Architecture.Tests/)

#### Coverage Requirements
- Minimum: 70% (CI warning threshold)
- Recommended: 85%
- Excellent: 90%+

### Commit Message Format

Use Conventional Commits:
```texttext
feat(users): add user registration endpoint
fix(providers): resolve null reference in verification service
refactor(shared): extract validation to separate class
test(providers): add tests for provider registration
docs(readme): update installation instructions
chore(deps): update EF Core to 9.0.9
```

### Code Review Checklist

Before submitting a PR:
- [ ] All tests pass locally
- [ ] Code follows existing naming conventions
- [ ] No warnings in build output
- [ ] Added/updated tests for new functionality
- [ ] Updated documentation if needed
- [ ] Added XML documentation for public APIs
- [ ] Used `Result<T>` pattern for operations that can fail
- [ ] Domain events published for state changes
- [ ] Migrations added for database changes

## Common Patterns

### Result Pattern (Error Handling)

```csharp
// Return Result<T> instead of throwing exceptions
public async Task<Result<User>> RegisterUserAsync(RegisterUserCommand command)
{
    var validation = await _validator.ValidateAsync(command);
    if (!validation.IsValid)
        return Result.Failure<User>(validation.Errors);
    
    var user = User.Create(...);
    await _repository.AddAsync(user);
    
    return Result.Success(user);
}
```

### Domain Events

```csharp
// In aggregate root
public class Provider : AggregateRoot<ProviderId>
{
    public void UpdateVerificationStatus(EVerificationStatus newStatus)
    {
        var oldStatus = VerificationStatus;
        VerificationStatus = newStatus;
        
        // Raise domain event
        RaiseDomainEvent(new ProviderVerificationStatusUpdatedDomainEvent(
            Id.Value,
            Version,
            oldStatus,
            newStatus,
            null
        ));
    }
}

// Event handler
public class ProviderVerificationStatusUpdatedHandler 
    : INotificationHandler<ProviderVerificationStatusUpdatedDomainEvent>
{
    public async Task Handle(ProviderVerificationStatusUpdatedDomainEvent notification, 
                           CancellationToken cancellationToken)
    {
        // React to event (e.g., send notification, update related entities)
    }
}
```

### Module API Communication

```csharp
// Define interface in Shared/Contracts/Modules/Users/
public interface IUsersModuleApi : IModuleApi
{
    Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> UserExistsAsync(Guid userId, CancellationToken ct = default);
}

// Implement in Users/Application/Services/
[ModuleApi("Users", "1.0")]
public sealed class UsersModuleApi : IUsersModuleApi
{
    private readonly IMediator _mediator;
    
    public async Task<Result<bool>> UserExistsAsync(Guid userId, CancellationToken ct = default)
    {
        var query = new CheckUserExistsQuery(userId);
        var result = await _mediator.Send(query, ct);
        return Result.Success(result);
    }
}

// Use in another module
public class CreateOrderHandler
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<Result> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        // Check if user exists via Module API
        var userExists = await _usersApi.UserExistsAsync(command.UserId, ct);
        if (!userExists.IsSuccess || !userExists.Value)
            return Result.Failure("User not found");
            
        // Continue with order creation...
    }
}
```

### Entity Framework Configuration

```csharp
// DbContext per module with dedicated schema
public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        
        // Set default schema
        modelBuilder.HasDefaultSchema("meajudaai_users");
    }
}

// Entity configuration
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        // Configure ID with conversion
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
               .HasConversion(
                   id => id.Value,
                   value => UserId.From(value));
        
        // Configure value objects
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("email")
                 .IsRequired();
        });
    }
}
```

## Troubleshooting

### Build Issues

**Error**: Package version conflicts
```powershell
# Solution: Clean and restore
dotnet clean
dotnet restore --force
```

**Error**: Aspire not found
```powershell
# Solution: Install Aspire workload
dotnet workload update
dotnet workload install aspire
```

### Database Issues

**Error**: Cannot connect to PostgreSQL
```powershell
# Check if Docker is running
docker ps

# Check PostgreSQL container
docker logs [container-id]

# Verify connection string environment variables
echo $env:DB_HOST
echo $env:DB_PORT
echo $env:DB_PASSWORD
```

**Error**: Migration already applied
```powershell
# Check migration status
.\scripts\ef-migrate.ps1 -Command status -Module Users

# Rollback if needed (use carefully)
dotnet ef database update [PreviousMigrationName] --context UsersDbContext
```

### Test Issues

**Error**: Tests fail with database errors
- Ensure PostgreSQL service is running
- Check that connection string environment variables are set
- Verify TestContainers has access to Docker socket

**Error**: Coverage reports missing
```powershell
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

## Important Files

- **`Directory.Build.props`**: Global MSBuild properties, warning suppressions
- **`Directory.Packages.props`**: Central package version management
- **`.editorconfig`**: Code style and formatting rules
- **`nuget.config`**: NuGet package sources
- **`README.md`**: User-facing documentation
- **`docs/architecture.md`**: Detailed architectural decisions and patterns
- **`docs/development.md`**: Comprehensive development guide
- **`scripts/README.md`**: All available development scripts

## CI/CD Notes

- **Pull Request Validation**: Runs on all PRs to `master` and `develop`
- **Required Secrets**: `POSTGRES_PASSWORD`, `POSTGRES_USER`, `POSTGRES_DB`
- **Optional Secrets**: `KEYCLOAK_ADMIN_PASSWORD` (for auth features)
- **Test Coverage**: Automatically collected and reported on PRs
- **Module Tests**: Each module has isolated test suite that runs in parallel

## Additional Resources

- [Architecture Documentation](docs/architecture.md) - Deep dive into patterns and decisions
- [Development Guide](docs/development.md) - Comprehensive development workflows
- [Infrastructure Guide](docs/infrastructure.md) - Deployment and infrastructure setup
- [CI/CD Setup](docs/ci_cd.md) - Pipeline configuration and deployment
- [Adding New Modules](docs/adding-new-modules.md) - Step-by-step module creation