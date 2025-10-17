# TestAuthenticationHandler - Configuração e Uso

## 🔧 Configuração Básica

### Configuração no Program.cs

```csharp
// Em Program.cs ou Startup.cs
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    // ✅ Configuração para desenvolvimento e testes
    builder.Services.AddAuthentication("AspireTest")
        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
            "AspireTest", options => { });
    
    // Log de warning para visibilidade
    builder.Services.AddLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    });
}
else
{
    // ✅ Configuração real para outros ambientes
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://your-keycloak-server/realms/meajudaai";
            options.Audience = "meajudaai-api";
            options.RequireHttpsMetadata = true;
        });
}

var app = builder.Build();

// Habilite autenticação/autorização no pipeline
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```csharp
### Configuração de Autorização

```csharp
// Políticas de autorização funcionam normalmente
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin")); // TestHandler sempre fornece role "admin"
    
    options.AddPolicy("UserPolicy", policy =>
        policy.RequireAuthenticatedUser()); // TestHandler sempre autentica
});
```text
**⚠️ Importante**: Para que a política `AdminOnly` funcione corretamente, o `TestAuthenticationHandler` deve criar a identidade com o tipo de claim correto:

```csharp
// Dentro do handler ao criar a identity:
var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
```yaml
## 🔍 Verificação de Ambiente

### Validação Automática

O sistema inclui validação automática para prevenir uso incorreto:

```csharp
// Esta validação é executada no startup (em Program.cs) — antes de builder.Build()
if (builder.Environment.IsProduction() && /* TestHandler detectado */)
{
    throw new InvalidOperationException(
        "TestAuthenticationHandler cannot be used in Production environment!");
}
```csharp
### Variáveis de Ambiente

Certifique-se de que as seguintes variáveis estão configuradas:

```bash
# Para desenvolvimento
ASPNETCORE_ENVIRONMENT=Development

# Para testes
ASPNETCORE_ENVIRONMENT=Testing

# Em produção, defina:
# ASPNETCORE_ENVIRONMENT=Production
```csharp
## 📊 Monitoramento e Logs

### Logs de Segurança

O handler gera logs específicos para auditoria:

```text
[WARN] 🚨 TEST AUTHENTICATION ACTIVE: Bypassing real authentication. 
Request from 127.0.0.1 authenticated as admin user automatically. 
Ensure this is NOT a production environment!
```yaml
### Logs de Debug

Em modo debug, logs adicionais são gerados:

```text
[DEBUG] Test authentication completed. Generated claims: 9, 
Identity: test-user, IsAuthenticated: True
```text
## 🎯 Casos de Uso Recomendados

### 1. Testes de Integração

```csharp
[Test]
public async Task GetUsers_WithAuthentication_ShouldReturnUsers()
{
    // TestHandler automaticamente autentica como admin
    var response = await _client.GetAsync("/api/users");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```csharp
### 2. Desenvolvimento Local

- Permite testar endpoints protegidos sem configurar Keycloak
- Acelera o desenvolvimento de APIs
- Facilita debugging de autorização

### 3. Pipelines CI/CD

- Testes automatizados sem dependências externas
- Validação rápida de endpoints
- Verificação de políticas de autorização

## ⚙️ Configurações Avançadas

### Customização de Claims

Para casos específicos, você pode estender o handler:

```csharp
public class CustomTestAuthenticationHandler
  : AuthenticationHandler<AuthenticationSchemeOptions>
{
  public CustomTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
    : base(options, logger, encoder, clock) { }

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, "test-user"),
      new Claim(ClaimTypes.Name, "test-user"),
      new Claim(ClaimTypes.Role, "admin"),
      new Claim("department", "IT"),
      new Claim("level", "senior")
    };
    var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(
      principal,
      new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15) },
      Scheme.Name);
    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}
```csharp
### Múltiplos Esquemas

```csharp
// Para cenários complexos com múltiplos esquemas
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = "Test-Admin";
  options.DefaultChallengeScheme = "Test-Admin";
})
    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
        "Test-Admin", options => { })
    .AddScheme<AuthenticationSchemeOptions, TestUserAuthenticationHandler>(
        "Test-User", options => { });

// Alternativa por endpoint:
// [Authorize(AuthenticationSchemes = "Test-User")]
```text
## 🔒 Boas Práticas de Segurança

### 1. Sempre Verificar Ambiente

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;

// Exemplo usando IHostEnvironment injetado
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHostEnvironment _environment;
    
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IHostEnvironment environment) 
        : base(options, logger, encoder, clock)
    {
        _environment = environment;
    }
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException("TestAuthenticationHandler not allowed in this environment");
        }
        
        // ... resto da implementação
    }
}
```csharp
### 2. Logs de Auditoria

```csharp
_logger.LogWarning("TEST AUTH: Request {Path} authenticated with test handler from IP {IP}",
    Context.Request.Path, Context.Connection.RemoteIpAddress);
```text
### 3. Timeouts Curtos

```csharp
// Configurar expiração via AuthenticationProperties em vez de claim string
var claims = new[]
{
    new Claim(ClaimTypes.Name, "test-user"),
    new Claim(ClaimTypes.Role, "Admin"),
    // Removido claim "exp" - usando AuthenticationProperties.ExpiresUtc
};

var identity = new ClaimsIdentity(claims, "Test");
var principal = new ClaimsPrincipal(identity);

// Definir expiração adequada via AuthenticationProperties
var properties = new AuthenticationProperties
{
    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15), // Expira em 15 minutos
    IsPersistent = false // Não persiste entre sessões do browser
};

var ticket = new AuthenticationTicket(principal, properties, "Test");
return AuthenticateResult.Success(ticket);
```text