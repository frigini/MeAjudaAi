# Integration Tests Guide

## Overview
This document provides comprehensive guidance for writing and maintaining integration tests in the MeAjudaAi platform.

## Integration Testing Strategy

### Test Categories
1. **API Integration Tests** - Testing complete HTTP request/response cycles
2. **Database Integration Tests** - Testing data persistence and retrieval
3. **Service Integration Tests** - Testing interaction between multiple services

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
```csharp
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
```csharp
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
```csharp
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
```text
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

- [Test Authentication Handler](test_authentication_handler.md)
- [Development Guidelines](../development.md)
- [CI/CD Setup](../ci_cd.md)