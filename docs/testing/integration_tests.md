# Integration Tests Guide

## Overview
This document provides comprehensive guidance for writing and maintaining integration tests in the MeAjudaAi platform.

## Integration Testing Strategy

The project implements a **two-level integration testing architecture** to balance test coverage, performance, and isolation:

### 1. Component-Level Integration Tests (Module-Scoped)
**Location**: `src/Modules/{Module}/Tests/Integration/`

These tests validate **individual infrastructure components** within a module using real dependencies:

- **Scope**: Single module components (Repositories, Services, Queries)
- **Infrastructure**: Isolated TestContainers per test class
- **Base Classes**: `DatabaseTestBase`, `{Module}IntegrationTestBase`
- **Speed**: Faster (only necessary components loaded)
- **Purpose**: Validate data persistence, repository logic, and infrastructure services
- **Isolation**: Each module manages its own test infrastructure

**Example Use Cases**:
- Testing `UserRepository.GetByIdAsync()` with a real PostgreSQL database
- Validating complex queries return correct data
- Testing database migrations and schema compatibility
- Verifying repository transaction handling

**Example Structure**:
```csharp
// Location: src/Modules/Users/Tests/Integration/UserRepositoryIntegrationTests.cs
public class UserRepositoryTests : DatabaseTestBase
{
    private UserRepository _repository;
    private UsersDbContext _context;

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldPersistUser()
    {
        // Uses real PostgreSQL via TestContainers
        // Tests only repository + database interaction
    }
}
```

### 2. End-to-End Integration Tests (Centralized)
**Location**: `tests/MeAjudaAi.Integration.Tests/Modules/{Module}/`

These tests validate **complete application flows** with all modules integrated:

- **Scope**: Full application (HTTP endpoints, DI container, all modules)
- **Infrastructure**: Complete application via `WebApplicationFactory`
- **Base Classes**: `ApiTestBase`, `SharedIntegrationTestFixture`
- **Speed**: Slower (entire application stack)
- **Purpose**: Validate end-to-end workflows, API contracts, cross-module communication
- **Isolation**: Shared test infrastructure for all E2E tests

**Example Use Cases**:
- Testing `POST /api/v1/users` creates user and returns correct HTTP response
- Validating authentication and authorization flows
- Testing cross-module communication (e.g., creating a provider validates user exists)
- Verifying complete business workflows

**Example Structure**:
```csharp
// Location: tests/MeAjudaAi.Integration.Tests/Modules/Users/UsersApiTests.cs
public class UsersApiTests : ApiTestBase
{
    [Fact]
    public async Task RegisterUser_ValidData_ShouldReturnCreated()
    {
        // Tests complete HTTP request/response
        // All modules loaded and integrated
        var response = await Client.PostAsJsonAsync("/api/users/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Decision Matrix: Which Level to Use?

| Test Scenario | Component-Level | End-to-End |
|--------------|----------------|------------|
| Repository CRUD operations | ✅ | ❌ |
| Complex database queries | ✅ | ❌ |
| Database migrations | ✅ | ❌ |
| Service business logic | ✅ | ❌ |
| HTTP endpoints | ❌ | ✅ |
| Authentication flows | ❌ | ✅ |
| Cross-module communication | ❌ | ✅ |
| Complete workflows | ❌ | ✅ |

### Module Comparison

**Modules with Component-Level Tests**:
- ✅ Users (4 test files)
- ✅ Providers (3 test files)
- ✅ Search (2 test files)

**Modules with Only E2E Tests**:
- ✅ Documents (simpler infrastructure, no complex repositories)
- ✅ Location (external APIs only, no database)

### Test Categories
1. **API Integration Tests** - Testing complete HTTP request/response cycles (E2E)
2. **Database Integration Tests** - Testing data persistence and retrieval (Component)
3. **Service Integration Tests** - Testing interaction between multiple services (Both levels)

### Test Environment Setup
Integration tests use TestContainers for isolated, reproducible test environments:

- **PostgreSQL Containers** - Isolated database instances
- **Redis Containers** - Caching layer testing
- **Message Bus Testing** - Service communication validation

## Test Base Classes

### SharedApiTestBase
The `SharedApiTestBase` class provides common functionality for API integration tests:

```csharp
public abstract class SharedApiTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; }
    protected TestContainerDatabase Database { get; private set; }
    
    // Setup and teardown methods
}
```

### Key Features
- Automatic test container lifecycle management
- Configured test authentication
- Database schema initialization
- HTTP client configuration

## Authentication in Tests

### Test Authentication Handler
Integration tests use the `ConfigurableTestAuthenticationHandler` for:

- **Predictable Authentication** - Consistent test user setup
- **Role-Based Testing** - Testing different user permissions
- **Unauthenticated Scenarios** - Testing public endpoints

### Configuration
```csharp
services.AddAuthentication("Test")
    .AddScheme<AuthenticationSchemeOptions, ConfigurableTestAuthenticationHandler>(
        "Test", options => { });
```

## Database Testing

### Test Database Management
- Each test class gets an isolated PostgreSQL container
- Database schema is automatically applied
- Test data is cleaned up between tests

### Entity Framework Integration
```csharp
protected async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> action)
{
    using var context = CreateDbContext();
    return await action(context);
}
```

## Writing Integration Tests

### Test Structure
1. **Arrange** - Set up test data and configuration
2. **Act** - Execute the operation being tested
3. **Assert** - Verify the expected outcomes

### Example Test
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

## Best Practices

### Test Organization
- Group related tests in the same test class
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

### Performance Considerations
- Minimize database operations
- Reuse test containers when possible
- Use async/await properly

### Test Data Management
- Use test data builders for complex objects
- Clean up test data after each test
- Avoid dependencies between tests

## Troubleshooting

### Common Issues
1. **Container Startup Failures** - Check Docker availability
2. **Database Connection Issues** - Verify connection strings
3. **Authentication Problems** - Check test authentication configuration

### Debugging Tests
- Enable detailed logging for test runs
- Use test output helpers for debugging
- Check container logs for infrastructure issues

## CI/CD Integration

### Automated Test Execution
Integration tests run as part of the CI/CD pipeline:

- **Pull Request Validation** - All tests must pass
- **Parallel Execution** - Tests run in parallel for performance
- **Coverage Reporting** - Integration test coverage is tracked

### Environment Configuration
- Tests use environment-specific configuration
- Secrets and sensitive data are managed securely
- Test isolation is maintained across parallel runs

## Related Documentation

- [Development Guidelines](../development.md)
- [CI/CD Setup](../ci_cd.md)