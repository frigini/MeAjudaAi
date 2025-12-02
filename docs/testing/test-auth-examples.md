# TestAuthenticationHandler - Exemplos Práticos

## 🧪 Testes de Integração

### Teste Básico de Endpoint Protegido

```csharp
[Test]
public async Task GetUsers_WithTestAuth_ShouldReturnUsers()
{
    // Arrange: TestAuthenticationHandler automaticamente autentica como admin
    
    // Act
    var response = await _client.GetAsync("/api/users");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
    users.Should().NotBeNull();
}
```

### Teste de Autorização por Role

```csharp
[Test]
public async Task AdminEndpoint_WithTestAuth_ShouldAllowAccess()
{
    // TestHandler sempre fornece role "admin"
    var response = await _client.GetAsync("/api/admin/settings");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Should().NotBeNull();
}

[Test]  
public async Task UserEndpoint_WithTestAuth_ShouldAllowAccess()
{
    // TestHandler também satisfaz políticas de usuário autenticado
    var response = await _client.GetAsync("/api/users/profile");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Teste de Claims Específicos

```csharp
[Test]
public async Task GetCurrentUser_WithTestAuth_ShouldReturnTestUser()
{
    // Act
    var response = await _client.GetAsync("/api/users/me");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var user = await response.Content.ReadFromJsonAsync<UserDto>();
    user.Id.Should().Be("test-user-id");
    user.Email.Should().Be("test@example.com");
    user.Name.Should().Be("test-user");
}
```

## 🔧 Desenvolvimento Local

### Setup para Desenvolvimento

```csharp
// Program.cs para desenvolvimento
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    Console.WriteLine("🚨 Running with TestAuthenticationHandler - Development/Testing Mode");
    
    builder.Services.AddAuthentication("AspireTest")
        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
            "AspireTest", options => { });
}

var app = builder.Build();

// Middleware que mostra quando TestAuth está ativo
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true && 
            context.User.Identity.AuthenticationType == "AspireTest")
        {
            context.Response.Headers.Add("X-Test-Auth", "Active");
        }
        await next();
    });
}
```

### Verificação em Runtime

```csharp
[HttpGet("debug/auth")]
public IActionResult GetAuthInfo()
{
    if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
        return NotFound();
    
    return Ok(new
    {
        IsAuthenticated = User.Identity?.IsAuthenticated,
        AuthenticationType = User.Identity?.AuthenticationType,
        Name = User.Identity?.Name,
        Claims = User.Claims.Select(c => new { c.Type, c.Value }),
        IsTestAuth = User.Identity?.AuthenticationType == "AspireTest"
    });
}
```

## 🚀 CI/CD Pipeline

### GitHub Actions

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    env:
      ASPNETCORE_ENVIRONMENT: Testing
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Run Integration Tests
      run: |
        echo "🚨 Running with TestAuthenticationHandler for CI"
        dotnet test tests/MeAjudaAi.Integration.Tests/ \
          --configuration Release \
          --logger "console;verbosity=detailed"
```

### Azure DevOps

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  ASPNETCORE_ENVIRONMENT: 'Testing'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests with TestAuth'
  inputs:
    command: 'test'
    projects: 'tests/**/*.csproj'
    arguments: '--configuration Release --logger trx --collect:"XPlat Code Coverage"'
    testRunTitle: 'Integration Tests (TestAuth)'
```

## 🎯 Cenários Específicos

### Teste de Upload com Autenticação

```csharp
[Test]
public async Task UploadFile_WithTestAuth_ShouldSucceed()
{
    // Arrange
    var fileContent = "test content";
    var content = new MultipartFormDataContent();
    content.Add(new StringContent(fileContent), "file", "test.txt");
    
    // Act: TestAuth automaticamente fornece autorização
    var response = await _client.PostAsync("/api/files/upload", content);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Teste de WebSocket com Autenticação

```csharp
[Test]
public async Task ConnectWebSocket_WithTestAuth_ShouldConnect()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // TestAuth automaticamente autentica requisições WebSocket
    var webSocketClient = _factory.Server.CreateWebSocketClient();
    
    // Act
    var webSocket = await webSocketClient.ConnectAsync(
        new Uri("ws://localhost/hub/notifications"), 
        CancellationToken.None);
    
    // Assert
    webSocket.State.Should().Be(WebSocketState.Open);
}
```

### Teste de Rate Limiting

```csharp
[Test]
public async Task RateLimit_WithTestAuth_ShouldApplyAuthenticatedLimits()
{
    // TestAuth faz requisições serem tratadas como autenticadas
    // Aplicando limites de rate para usuários autenticados (mais permissivos)
    
    var tasks = Enumerable.Range(0, 150) // Limite auth = 200/min
        .Select(_ => _client.GetAsync("/api/users"))
        .ToArray();
    
    var responses = await Task.WhenAll(tasks);
    
    // Deve aceitar mais requisições por estar "autenticado"
    var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
    successCount.Should().BeGreaterThan(100); // Mais que limite anônimo
}
```

## 🔍 Debugging e Troubleshooting

### Verificar se TestAuth Está Ativo

```csharp
[HttpGet("health/auth")]
public IActionResult CheckAuthHealth()
{
    var isTestAuth = User.Identity?.AuthenticationType == "AspireTest";
    var environment = _environment.EnvironmentName;
    
    return Ok(new
    {
        Environment = environment,
        IsTestAuthActive = isTestAuth,
        IsProduction = _environment.IsProduction(),
        AuthenticationType = User.Identity?.AuthenticationType,
        UserName = User.Identity?.Name,
        Roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
    });
}
```

### Log Personalizado para Testes

```csharp
public class TestAuthAwareLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _innerLogger;
    
    public TestAuthAwareLogger(ILogger<T> innerLogger)
    {
        _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _innerLogger.BeginScope(state);
    }
    
    public bool IsEnabled(LogLevel logLevel)
    {
        return _innerLogger.IsEnabled(logLevel);
    }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
            
        var originalMessage = formatter(state, exception);
        var prefixedMessage = $"[TEST-AUTH] {originalMessage}";
        
        _innerLogger.Log(logLevel, eventId, prefixedMessage, exception, (msg, ex) => msg);
    }
    
    public void LogInformation(string message, params object[] args)
    {
        _innerLogger.LogInformation($"[TEST-AUTH] {message}", args);
    }
}
```

### Assertion Helper para Testes

```csharp
public static class TestAuthAssertions
{
    public static void ShouldBeTestAuthenticated(this HttpResponseMessage response)
    {
        response.Headers.Should().ContainKey("X-Test-Auth");
        response.Headers.GetValues("X-Test-Auth").First().Should().Be("Active");
    }
    
    public static void ShouldHaveAdminClaims(this ClaimsPrincipal user)
    {
        user.Should().NotBeNull();
        user.Identity?.IsAuthenticated.Should().BeTrue();
        user.IsInRole("admin").Should().BeTrue();
        user.FindFirst("sub")?.Value.Should().Be("test-user-id");
    }
}
```