# UsersPermissionResolver - Testing Examples

## 🧪 Unit Testing Examples

### Mock Implementation Testing

```csharp
[Fact]
public async Task ResolvePermissionsAsync_WithMockImplementation_ShouldReturnExpectedPermissions()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("Authorization:UseKeycloak", "false")
        })
        .Build();
        
    var logger = Mock.Of<ILogger<UsersPermissionResolver>>();
    var resolver = new UsersPermissionResolver(logger, configuration);

    // Act
    var permissions = await resolver.ResolvePermissionsAsync("admin-user");

    // Assert
    permissions.Should().Contain(EPermissions.AdminUsers);
    permissions.Should().Contain(EPermissions.UsersRead);
}
```

### Keycloak Integration Testing

```csharp
[Fact]
public async Task ResolvePermissionsAsync_WithKeycloakEnabled_ShouldUseKeycloakResolver()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("Authorization:UseKeycloak", "true")
        })
        .Build();
        
    var mockKeycloakResolver = new Mock<IKeycloakPermissionResolver>();
    mockKeycloakResolver.Setup(x => x.ResolvePermissionsAsync("test-user", default))
        .ReturnsAsync(new[] { EPermissions.UsersRead, EPermissions.UsersProfile });
        
    var logger = Mock.Of<ILogger<UsersPermissionResolver>>();
    var resolver = new UsersPermissionResolver(logger, configuration, mockKeycloakResolver.Object);

    // Act
    var permissions = await resolver.ResolvePermissionsAsync("test-user");

    // Assert
    mockKeycloakResolver.Verify(x => x.ResolvePermissionsAsync("test-user", default), Times.Once);
    permissions.Should().Contain(EPermissions.UsersRead);
}
```

## 🔧 Integration Testing

### Test Configuration

```json
{
  "Authorization": {
    "UseKeycloak": false
  },
  "Logging": {
    "LogLevel": {
      "MeAjudaAi.Modules.Users.Application.Authorization.UsersPermissionResolver": "Debug"
    }
  }
}
```

### WebApplicationFactory Example

```csharp
public class UsersPermissionResolverIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsersPermissionResolverIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("Authorization:UseKeycloak", "false")
                });
            });
        });
    }

    [Theory]
    [InlineData("admin-user", EPermissions.AdminUsers)]
    [InlineData("manager-user", EPermissions.UsersList)]  
    [InlineData("regular-user", EPermissions.UsersRead)]
    public async Task ResolvePermissions_ShouldReturnCorrectPermissions(string userId, EPermissions expectedPermission)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<UsersPermissionResolver>();

        // Act
        var permissions = await resolver.ResolvePermissionsAsync(userId);

        // Assert
        permissions.Should().Contain(expectedPermission);
    }
}
```

## 🏭 Production Testing

### Environment-Specific Testing

```csharp
public class ProductionUsersPermissionResolverTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ResolvePermissions_WithRealKeycloak_ShouldWork()
    {
        // Skip if not in CI/CD with real Keycloak instance
        Skip.If(Environment.GetEnvironmentVariable("CI_KEYCLOAK_ENABLED") != "true");

        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Production.json")
            .AddEnvironmentVariables()
            .Build();
            
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPermissionBasedAuthorization(configuration);
        
        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<UsersPermissionResolver>();

        // Act & Assert - usando usuário real do Keycloak
        var realUserId = Environment.GetEnvironmentVariable("TEST_USER_ID")!;
        var permissions = await resolver.ResolvePermissionsAsync(realUserId);
        
        permissions.Should().NotBeEmpty();
    }
}
```

## 📊 Performance Testing

### Benchmarking

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class UsersPermissionResolverBenchmark
{
    private UsersPermissionResolver _mockResolver = null!;
    private UsersPermissionResolver _keycloakResolver = null!;

    [GlobalSetup]
    public void Setup()
    {
        var mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Authorization:UseKeycloak", "false") })
            .Build();
            
        var keycloakConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Authorization:UseKeycloak", "true") })
            .Build();

        var logger = Mock.Of<ILogger<UsersPermissionResolver>>();
        var keycloakResolver = Mock.Of<IKeycloakPermissionResolver>();
        
        _mockResolver = new UsersPermissionResolver(logger, mockConfig);
        _keycloakResolver = new UsersPermissionResolver(logger, keycloakConfig, keycloakResolver);
    }

    [Benchmark]
    public async Task<IReadOnlyList<EPermissions>> MockImplementation()
    {
        return await _mockResolver.ResolvePermissionsAsync("test-user");
    }

    [Benchmark]  
    public async Task<IReadOnlyList<EPermissions>> KeycloakImplementation()
    {
        return await _keycloakResolver.ResolvePermissionsAsync("test-user");
    }
}
```

## 🔍 Debugging

### Enable Debug Logging

```json
{
  "Logging": {
    "LogLevel": {
      "MeAjudaAi.Modules.Users.Application.Authorization.UsersPermissionResolver": "Debug",
      "MeAjudaAi.Shared.Authorization.Keycloak.KeycloakPermissionResolver": "Debug"
    }
  }
}
```

### Expected Log Output

```
[Debug] UsersPermissionResolver initialized with Mock implementation
[Debug] Retrieved 1 mock roles for user regular-user: meajudaai-user  
[Debug] Resolved 2 Users module permissions for user regular-user from roles: meajudaai-user using Mock
```

## 🚨 Error Scenarios Testing

### Keycloak Unavailable

```csharp
[Fact]
public async Task ResolvePermissions_WhenKeycloakUnavailable_ShouldFallbackToMock()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Authorization:UseKeycloak", "true") })
        .Build();
        
    var mockKeycloakResolver = new Mock<IKeycloakPermissionResolver>();
    mockKeycloakResolver.Setup(x => x.ResolvePermissionsAsync(It.IsAny<string>(), default))
        .ThrowsAsync(new HttpRequestException("Keycloak unavailable"));
        
    var logger = Mock.Of<ILogger<UsersPermissionResolver>>();
    var resolver = new UsersPermissionResolver(logger, configuration, mockKeycloakResolver.Object);

    // Act
    var permissions = await resolver.ResolvePermissionsAsync("test-user");

    // Assert - Should fallback to mock and still return permissions
    permissions.Should().NotBeEmpty();
    permissions.Should().Contain(EPermissions.UsersRead);
}
```