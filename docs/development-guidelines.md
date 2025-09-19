# Development Guidelines

This document provides comprehensive guidelines for developing with the MeAjudaAi platform, including setup, coding standards, and best practices.

## Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Project Structure](#project-structure)
3. [Coding Standards](#coding-standards)
4. [Testing Guidelines](#testing-guidelines)
5. [Debugging and Troubleshooting](#debugging-and-troubleshooting)
6. [Performance Considerations](#performance-considerations)

## Development Environment Setup

### Prerequisites

- **.NET 9 SDK** - Latest version
- **Docker Desktop** - For running infrastructure services
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** - Version control

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd MeAjudaAi
   ```

2. **Setup and run locally**:
   ```bash
   ./run-local.sh setup
   ./run-local.sh run
   ```

3. **Access the application**:
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`
   - Aspire Dashboard: `http://localhost:15000`

### Environment Configuration

The application uses hierarchical configuration:
1. `appsettings.json` - Base configuration
2. `appsettings.Development.json` - Development overrides
3. Environment variables - Runtime overrides

Key development settings in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meajudaai_dev;Username=postgres;Password=postgres"
  },
  "Authentication": {
    "UseTestAuthentication": true
  }
}
```

## Project Structure

### Solution Organization

```
MeAjudaAi/
├── src/
│   ├── Aspire/                    # .NET Aspire orchestration
│   │   ├── MeAjudaAi.AppHost/     # Application host
│   │   └── MeAjudaAi.ServiceDefaults/ # Shared defaults
│   ├── Bootstrapper/              # API entry point
│   │   └── MeAjudaAi.ApiService/  # Main API service
│   ├── Modules/                   # Domain modules
│   │   └── Users/                 # User management module
│   └── Shared/                    # Shared components
│       └── MeAjudai.Shared/       # Common utilities
├── tests/                         # Test projects
├── infrastructure/                # Infrastructure as Code
└── docs/                         # Documentation
```

### Module Structure (DDD)

Each module follows the Clean Architecture pattern:
```
Module/
├── API/                          # Controllers, DTOs
├── Application/                  # Use cases, CQRS handlers
├── Domain/                       # Entities, aggregates, domain services
└── Infrastructure/               # Data access, external services
```

### Naming Conventions

- **Namespaces**: `MeAjudaAi.{Module}.{Layer}`
- **Files**: PascalCase (e.g., `UserService.cs`)
- **Classes**: PascalCase (e.g., `public class UserService`)
- **Methods**: PascalCase (e.g., `public void CreateUser()`)
- **Variables**: camelCase (e.g., `var userName = "test"`)
- **Constants**: PascalCase (e.g., `public const string ApiVersion`)

## Coding Standards

### General Principles

1. **SOLID Principles**: Follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion
2. **DRY (Don't Repeat Yourself)**: Avoid code duplication through abstraction
3. **KISS (Keep It Simple, Stupid)**: Prefer simple, readable solutions
4. **YAGNI (You Aren't Gonna Need It)**: Don't implement features until they're needed

### Code Organization

1. **File Structure**:
   ```csharp
   // 1. Using statements (grouped and sorted)
   using System;
   using Microsoft.Extensions.DependencyInjection;
   
   // 2. Namespace
   namespace MeAjudaAi.Users.Application;
   
   // 3. Class definition
   public class UserService
   {
       // 4. Fields (private, readonly when possible)
       private readonly IUserRepository _userRepository;
       
       // 5. Constructor
       public UserService(IUserRepository userRepository)
       {
           _userRepository = userRepository;
       }
       
       // 6. Public methods
       public async Task<User> GetUserAsync(int id)
       {
           return await _userRepository.GetByIdAsync(id);
       }
       
       // 7. Private methods
       private void ValidateUser(User user)
       {
           // validation logic
       }
   }
   ```

2. **Method Guidelines**:
   - Keep methods small (< 20 lines when possible)
   - Use meaningful parameter names
   - Return specific types, not generic objects
   - Use async/await for I/O operations

3. **Error Handling**:
   ```csharp
   // Use specific exceptions
   public async Task<User> GetUserAsync(int id)
   {
       if (id <= 0)
           throw new ArgumentException("User ID must be positive", nameof(id));
           
       var user = await _userRepository.GetByIdAsync(id);
       if (user == null)
           throw new UserNotFoundException($"User with ID {id} not found");
           
       return user;
   }
   ```

### CQRS Implementation

1. **Commands** (write operations):
   ```csharp
   public record CreateUserCommand(string Email, string Name) : ICommand<User>;
   
   public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
   {
       public async Task<User> Handle(CreateUserCommand command, CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

2. **Queries** (read operations):
   ```csharp
   public record GetUserQuery(int Id) : IQuery<User>;
   
   public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
   {
       public async Task<User> Handle(GetUserQuery query, CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

## Testing Guidelines

### Test Structure

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test component interactions
3. **End-to-End Tests**: Test complete user workflows

### Testing Conventions

```csharp
[Test]
public async Task GetUser_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "Test User" };
    _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(expectedUser);
    
    // Act
    var result = await _userService.GetUserAsync(userId);
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedUser));
}
```

### Test Authentication

For testing endpoints that require authentication, use the TestAuthenticationHandler:

```csharp
[Test]
public async Task GetProtectedResource_WithTestAuth_ReturnsResource()
{
    // Configure test authentication
    Environment.SetEnvironmentVariable("Authentication:UseTestAuthentication", "true");
    
    // Test implementation
}
```

See [Testing Documentation](testing/) for detailed testing guidelines.

## Debugging and Troubleshooting

### Development Tools

1. **Aspire Dashboard**: Monitor application health and metrics at `http://localhost:15000`
2. **Swagger UI**: Test API endpoints at `http://localhost:5000/swagger`
3. **Application Logs**: View structured logs in console or log files

### Common Issues

1. **Database Connection**:
   ```bash
   # Check PostgreSQL is running
   docker ps | grep postgres
   
   # Check connection string
   dotnet user-secrets list
   ```

2. **Authentication Issues**:
   ```bash
   # Enable test authentication
   export Authentication__UseTestAuthentication=true
   
   # Check Keycloak status
   docker ps | grep keycloak
   ```

3. **Performance Issues**:
   - Use Aspire dashboard to monitor metrics
   - Enable detailed logging for specific components
   - Use profiling tools like dotTrace or PerfView

### Logging Configuration

Configure logging levels in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "MeAjudaAi": "Debug",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

## Performance Considerations

### Database Optimization

1. **Use async/await** for all database operations
2. **Implement pagination** for large result sets
3. **Use projections** to select only needed columns
4. **Configure proper indexes** for frequently queried fields

### Caching Strategy

1. **Memory Cache**: For frequently accessed, small data
2. **Distributed Cache (Redis)**: For session data and shared cache
3. **Response Caching**: For static or semi-static API responses

### API Performance

1. **Use compression** for API responses
2. **Implement rate limiting** to prevent abuse
3. **Use proper HTTP status codes** and response formats
4. **Minimize payload size** through DTOs and projections

### Monitoring

Use the built-in health checks and metrics:
- Health endpoint: `/health`
- Readiness endpoint: `/health/ready`
- Liveness endpoint: `/health/live`

## Development Workflow

1. **Create feature branch** from main
2. **Implement feature** following coding standards
3. **Write tests** for new functionality
4. **Run local tests** and ensure they pass
5. **Create pull request** with detailed description
6. **Code review** and address feedback
7. **Merge to main** after approval

### Git Conventions

- **Branch naming**: `feature/user-authentication`, `bugfix/login-issue`
- **Commit messages**: Use conventional commits format
  ```
  feat: add user authentication endpoints
  fix: resolve null reference in user service
  docs: update API documentation
  ```

## Additional Resources

- [Authentication Documentation](authentication.md)
- [Testing Guidelines](testing/)
- [Architecture Overview](architecture.md)
- [Infrastructure Documentation](infrastructure.md)