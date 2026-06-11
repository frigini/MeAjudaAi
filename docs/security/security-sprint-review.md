# 🔐 Sprint de Segurança — Revisão de Código e Arquitetura

> **Aplicação:** Monolito Modular (.NET)
> **Framework de Referência:** OWASP Top 10
> **Objetivo:** Identificar, corrigir e documentar vulnerabilidades de segurança antes da entrada em produção.

---

## Sumário

1. [A01 — Broken Access Control](#a01--broken-access-control)
2. [A02 — Cryptographic Failures](#a02--cryptographic-failures)
3. [A03 — Injection](#a03--injection)
4. [A04 — Insecure Design](#a04--insecure-design)
5. [A05 — Security Misconfiguration](#a05--security-misconfiguration)
6. [A06 — Vulnerable and Outdated Components](#a06--vulnerable-and-outdated-components-supply-chain)
7. [A07 — Identification and Authentication Failures](#a07--identification-and-authentication-failures)
8. [A08 — Software and Data Integrity Failures](#a08--software-and-data-integrity-failures)
9. [A09 — Security Logging and Monitoring Failures](#a09--security-logging-and-monitoring-failures)
10. [A10 — Server-Side Request Forgery (SSRF)](#a10--server-side-request-forgery-ssrf)
11. [Checklist Consolidado](#checklist-consolidado)

---

## A01 — Broken Access Control

### Visão Geral

Controle de acesso quebrado ocorre quando usuários conseguem agir fora de suas permissões intencionais: acessar dados de outros usuários, realizar ações administrativas sem privilégio, ou manipular IDs de objetos para acessar recursos alheios (IDOR — Insecure Direct Object Reference).

### Diretrizes para .NET

#### Object-Level Authorization (IDOR)

Toda vez que um recurso é acessado por ID (ex.: `/api/documents/{id}`), a aplicação **deve** verificar se o usuário autenticado é o proprietário ou tem permissão explícita sobre aquele recurso. Nunca confie apenas na autenticação bem-sucedida.

```csharp
// ❌ ERRADO — apenas busca o documento pelo ID sem verificar ownership
public async Task<IActionResult> GetDocument(int id)
{
    var doc = await _repo.GetByIdAsync(id);
    return Ok(doc);
}

// ✅ CORRETO — verifica se o usuário tem acesso ao recurso
public async Task<IActionResult> GetDocument(int id)
{
    var userId = User.GetUserId();
    var doc = await _repo.GetByIdAsync(id);

    if (doc == null || doc.OwnerId != userId)
        return Forbid();

    return Ok(doc);
}
```

#### Role-Based Access Control (RBAC)

Implemente autorização baseada em papéis no lado do servidor. Nunca dependa da UI para esconder funcionalidades — qualquer endpoint exposto deve ter sua própria verificação.

```csharp
// Usando Policy-based authorization no ASP.NET Core
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("FinanceAccess", policy =>
        policy.RequireRole("Admin", "Finance"));
});

[Authorize(Policy = "AdminOnly")]
[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(int id) { ... }
```

#### Access Deny by Default

Configure o pipeline de autorização para negar acesso por padrão em toda a aplicação. Use `[AllowAnonymous]` apenas em endpoints que genuinamente não requerem autenticação.

```csharp
// Program.cs — Require authorization globally
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

#### Enforce Per-Request Checks

Não confie no contexto de sessão para inferir permissões. Cada requisição deve validar permissões de forma independente.

```csharp
// Middleware customizado de verificação de tenant
public class TenantAuthorizationMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();
        var userId = context.User.GetUserId();

        if (!await tenantService.UserBelongsToTenantAsync(userId, tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
        await _next(context);
    }
}
```

#### Logging de Negações de Acesso

```csharp
// Logar todas as tentativas de acesso negadas
if (!hasAccess)
{
    _logger.LogWarning("Access denied: User {UserId} attempted to access resource {ResourceId} at {Timestamp}",
        userId, resourceId, DateTimeOffset.UtcNow);
    return Forbid();
}
```

### Developer Checklist — A01

- [ ] Controles de acesso são aplicados no servidor para todas as operações sensíveis?
- [ ] Verificações de ownership do objeto são realizadas antes de retornar dados?
- [ ] Rotas admin/API estão protegidas contra acesso não privilegiado?
- [ ] Existem testes automatizados que validam que acessos não autorizados são bloqueados?
- [ ] Papéis e permissões são gerenciados de forma consistente entre módulos?
- [ ] Dados de tenant estão devidamente isolados em sistemas compartilhados?
- [ ] Rate limiting e proteção contra replay estão configurados?

---

## A02 — Cryptographic Failures

### Visão Geral

Falhas criptográficas expõem dados sensíveis em repouso ou em trânsito devido ao uso de algoritmos fracos, ausência de criptografia, ou gerenciamento inadequado de chaves.

### Diretrizes para .NET

#### Algoritmos de Criptografia

Use apenas algoritmos modernos e autenticados. Evite MD5, SHA-1, DES, RC2, e ECB mode para qualquer dado sensível.

```csharp
// ✅ AES-GCM — criptografia autenticada (AEAD)
using var aes = new AesGcm(key);
var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
RandomNumberGenerator.Fill(nonce);
var tag = new byte[AesGcm.TagByteSizes.MaxSize];
aes.Encrypt(nonce, plaintext, ciphertext, tag);

// ✅ ChaCha20-Poly1305 via bibliotecas como libsodium-net
// Adequado para ambientes onde AES-NI não está disponível em hardware
```

#### Armazenamento de Senhas

Nunca armazene senhas em texto claro ou com hashes simples (MD5, SHA-256 sem salt). Use algoritmos de derivação de chave com custo computacional ajustável.

```csharp
// ✅ BCrypt — recomendado para .NET
using BCrypt.Net;
var hash = BCrypt.HashPassword(password, workFactor: 12);
var isValid = BCrypt.Verify(password, hash);

// ✅ Argon2 via Konscious.Security.Cryptography
var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
{
    Salt = salt,
    DegreeOfParallelism = 8,
    MemorySize = 65536,
    Iterations = 4
};
var hash = await argon2.GetBytesAsync(32);
```

#### Gerenciamento de Chaves

Nunca hardcode chaves criptográficas. Use serviços gerenciados como Azure Key Vault, AWS Secrets Manager, ou HashiCorp Vault.

```csharp
// ✅ Azure Key Vault com DefaultAzureCredential
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// Acesso na aplicação
var connectionString = configuration["MyApp--DbConnectionString"];
```

#### Configuração TLS

```csharp
// Program.cs — Enforce TLS 1.2+
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        httpsOptions.OnAuthenticate = (context, sslOptions) =>
        {
            sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(
                new[] { TlsCipherSuite.TLS_AES_256_GCM_SHA384 });
        };
    });
});

// Redirect HTTP para HTTPS
app.UseHttpsRedirection();
app.UseHsts();
```

#### Data Integrity — HMAC

```csharp
// Verificação de integridade com HMAC-SHA256
using var hmac = new HMACSHA256(key);
var computedHash = hmac.ComputeHash(data);
var isValid = CryptographicOperations.FixedTimeEquals(computedHash, receivedHash);
// Use FixedTimeEquals para evitar timing attacks
```

### Developer Checklist — A02

- [ ] Todos os dados sensíveis estão criptografados em repouso e em trânsito?
- [ ] Chaves são armazenadas com segurança e rotacionadas regularmente?
- [ ] Apenas algoritmos criptográficos aprovados são utilizados (AES-GCM, ChaCha20-Poly1305, TLS 1.2+)?
- [ ] TLS está configurado e forçado em todos os endpoints?
- [ ] A criptografia usa bibliotecas seguras de alto nível (não implementações manuais)?
- [ ] Senhas utilizam bcrypt, scrypt ou Argon2 com salt único por usuário?

---

## A03 — Injection

### Visão Geral

Vulnerabilidades de injeção (SQL, NoSQL, OS Command, LDAP) ocorrem quando dados não confiáveis são enviados a um interpretador como parte de um comando ou query. No .NET, as principais superfícies de ataque são consultas SQL dinâmicas e chamadas a processos do sistema operacional.

### Diretrizes para .NET

#### SQL Injection — Parameterized Queries

```csharp
// ❌ ERRADO — concatenação direta de input
var query = $"SELECT * FROM Users WHERE Email = '{email}'";

// ✅ CORRETO — Dapper com parâmetros
var user = await connection.QuerySingleOrDefaultAsync<User>(
    "SELECT * FROM Users WHERE Email = @Email",
    new { Email = email });

// ✅ CORRETO — EF Core (parameterizado automaticamente)
var user = await context.Users
    .Where(u => u.Email == email)
    .FirstOrDefaultAsync();

// ✅ CORRETO — ADO.NET com SqlParameter
using var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email });
```

#### Entity Framework Core — Evitar Raw SQL Inseguro

```csharp
// ❌ ERRADO — interpolação direta em FromSqlRaw
var users = context.Users.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'");

// ✅ CORRETO — FormattableString (EF Core trata como parâmetro)
var users = context.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}");

// ✅ CORRETO — Parâmetros explícitos com FromSqlRaw
var param = new SqlParameter("@Name", name);
var users = context.Users.FromSqlRaw("SELECT * FROM Users WHERE Name = @Name", param);
```

#### NoSQL Injection

```csharp
// MongoDB — NUNCA injete input diretamente em filtros
// ❌ ERRADO
var filter = $"{{ name: '{userInput}' }}";

// ✅ CORRETO — usar construtores tipados
var filter = Builders<User>.Filter.Eq(u => u.Name, userInput);
var result = await collection.Find(filter).ToListAsync();
```

#### OS Command Injection

```csharp
// ❌ ERRADO — invocar shell com input do usuário
Process.Start("cmd.exe", $"/c dir {userInput}");

// ✅ CORRETO — usar APIs que não invocam shell
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "git",
        ArgumentList = { "log", "--oneline", safeRevision }, // ArgumentList escapa automaticamente
        UseShellExecute = false,
        RedirectStandardOutput = true
    }
};
```

#### Input Validation

```csharp
// Validação estrita com Data Annotations
public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Name contains invalid characters.")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; }
}

// Validação de schema com FluentValidation
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).Matches(@"^[a-zA-Z\s]+$");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

#### Stored Procedures

Stored Procedures ajudam, mas **não substituem** parameterização. Um stored procedure chamado com concatenação de strings ainda é vulnerável.

```csharp
// ✅ Stored Procedure com parâmetros via Dapper
var user = await conn.QuerySingleOrDefaultAsync<User>(
    "sp_GetUserByEmail",
    new { Email = email },
    commandType: CommandType.StoredProcedure);
```

### Developer Checklist — A03

- [ ] Todas as queries de banco de dados são parametrizadas ou escapadas com segurança?
- [ ] Nenhum input de usuário é concatenado diretamente em queries ou comandos?
- [ ] Campos de input são validados por tipo, tamanho e formato?
- [ ] Testes automatizados verificam padrões de injeção (ex.: `' OR 1=1 --`)?
- [ ] Falhas de validação de input estão sendo logadas e alertadas?

---

## A04 — Insecure Design

### Visão Geral

Design inseguro refere-se a falhas arquiteturais onde controles de segurança não foram considerados desde o início. Não é sobre implementação defeituosa, mas sobre a ausência de design seguro por princípio.

### Diretrizes para .NET

#### Threat Modeling

Antes de implementar qualquer funcionalidade nova, realize modelagem de ameaças (ex.: STRIDE) para identificar vetores de ataque potenciais.

```text
Para cada funcionalidade, responda:
- Quem são os atores (internos e externos)?
- Quais dados são processados ou transmitidos?
- O que acontece se um ator malicioso manipular os dados?
- Quais são os limites de confiança (trust boundaries)?
```

#### Secure Defaults

```csharp
// Configuração de CORS restritiva por padrão
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("https://app.meudominio.com") // Sem wildcards
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type");
    });
});
```

#### Defense in Depth

Não dependa de uma única camada de segurança. Combine: validação de input, autorização, auditoria de logs, e monitoramento.

```text
Camadas de segurança recomendadas:
1. WAF (Web Application Firewall) — filtragem de tráfego externo
2. Rate Limiting — proteção contra abuso e brute-force
3. Input Validation — validação de schema/tipo
4. Authorization Middleware — verificação de permissões
5. Data Layer Checks — ownership/tenant validation
6. Audit Logging — rastreabilidade de ações
```

#### Design for Least Privilege

```csharp
// Exemplo: conta de serviço de banco de dados com permissões mínimas
// Em SQL Server, o usuário de aplicação deve ter apenas SELECT/INSERT/UPDATE/DELETE
// nas tabelas necessárias — nunca db_owner ou permissões DDL.

// Perfis de usuário com escopo mínimo
public enum UserRole
{
    ReadOnly,   // Apenas leitura
    Standard,   // CRUD sobre próprios recursos
    Manager,    // CRUD sobre recursos do time
    Admin       // Acesso total — auditado e com MFA obrigatório
}
```

#### Rate Limiting e Replay Prevention

```csharp
// ASP.NET Core Rate Limiting (built-in .NET 7+)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

app.UseRateLimiter();

[RateLimiter("LoginPolicy")]
[HttpPost("auth/login")]
public async Task<IActionResult> Login(LoginRequest request) { ... }
```

#### Validate Trust Boundaries

```csharp
// Nunca confie em dados passados do lado cliente — re-valide no servidor
// Exemplo: preço de produto nunca deve vir do frontend
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // ❌ NÃO FAÇA: var total = request.ClientTotal;

    // ✅ FAÇA: Recalcule valores sensíveis no servidor
    var product = await _productRepo.GetByIdAsync(request.ProductId);
    var total = product.Price * request.Quantity;
    // ...
}
```

### Developer Checklist — A04

- [ ] Ameaças foram modeladas para esta funcionalidade?
- [ ] Autorização e ownership são verificados em todo acesso?
- [ ] Dados manipulados pelo lado cliente são rejeitados/recalculados no servidor?
- [ ] Rate limiting e proteção contra replay estão ativos?
- [ ] Dados de tenant estão isolados corretamente em sistemas compartilhados?

---

## A05 — Security Misconfiguration

### Visão Geral

Configurações de segurança inadequadas incluem permissões excessivas, features desnecessárias habilitadas, mensagens de erro detalhadas em produção, credenciais padrão, e headers HTTP ausentes.

### Diretrizes para .NET

#### Error Handling — Não Expor Stack Traces

```csharp
// Program.cs — configuração de erro por ambiente
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Stack trace visível apenas em dev
}
else
{
    app.UseExceptionHandler("/error"); // Página de erro genérica em produção
    app.UseHsts();
}

// Controller de erro genérico
[ApiExplorerSettings(IgnoreApi = true)]
[Route("/error")]
public IActionResult Error()
{
    // ✅ Retorna mensagem genérica — detalhes apenas nos logs internos
    return Problem(title: "An unexpected error occurred.", statusCode: 500);
}

// Global Exception Handler com logging interno
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken token)
    {
        _logger.LogError(exception,
            "Unhandled exception on {Path} at {Timestamp}",
            context.Request.Path, DateTimeOffset.UtcNow);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            new { error = "An internal error occurred." }, token);
        return true;
    }
}
```

#### Security Headers

```csharp
// Middleware para adicionar headers de segurança
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'";
    await next();
});
```

#### CORS Restritivo

```csharp
// ❌ NUNCA em produção
policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

// ✅ Allowlist explícita
policy.WithOrigins("https://app.meudominio.com")
      .WithMethods("GET", "POST")
      .WithHeaders("Authorization", "Content-Type")
      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
```

#### Default Settings — Desabilitar o Desnecessário

```csharp
// Remover headers que revelam informações do servidor
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false; // Remove "Server: Kestrel"
});

// Desabilitar debug em produção — appsettings.Production.json
{
  "DetailedErrors": false,
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Autenticação em Interfaces Admin

```csharp
// Proteger Swagger/OpenAPI em produção
if (!app.Environment.IsDevelopment())
{
    // Não expor Swagger publicamente em produção
    // Se necessário, proteja com autenticação
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "internal/api-docs/{documentName}/swagger.json";
    });
}
```

#### Cloud Storage com Menor Privilégio

```csharp
// Azure Blob Storage — Managed Identity (sem credenciais em código)
var blobServiceClient = new BlobServiceClient(
    new Uri($"https://{accountName}.blob.core.windows.net"),
    new DefaultAzureCredential()); // Usa Managed Identity automaticamente

// Configurar ACL privado por padrão — blobs não são públicos sem configuração explícita
var containerClient = blobServiceClient.GetBlobContainerClient("documents");
await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
```

### Developer Checklist — A05

- [ ] Mensagens de erro são sanitizadas antes de exibir ao usuário?
- [ ] Todos os modos debug estão desabilitados em produção?
- [ ] Credenciais padrão foram alteradas e acesso restringido?
- [ ] Cloud storage está protegido com ACLs privadas ou IAM policies?
- [ ] Headers HTTP e políticas CORS estão corretamente configurados?
- [ ] Configurações de deployment foram testadas para segurança antes do go-live?

---

## A06 — Vulnerable and Outdated Components (Supply Chain)

### Visão Geral

O uso de dependências desatualizadas, com vulnerabilidades conhecidas, ou provenientes de fontes não confiáveis representa um vetor de ataque crítico na cadeia de suprimento de software.

### Diretrizes para .NET

#### Trusted Internal Repositories

Use **Artifactory** como fonte primária de pacotes NuGet em builds de produção. Evite pulls diretos de nuget.org sem validação.

```xml
<!-- NuGet.Config — configurar fonte interna como primária -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="Artifactory" value="https://artifactory.meudominio.com/artifactory/nuget-local/" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="Artifactory">
      <package pattern="MeuDominio.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

#### Version Pinning

```xml
<!-- ✅ Sempre fixe versões exatas no .csproj -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.8" />

<!-- ❌ Evite ranges que podem puxar versões maliciosas -->
<PackageReference Include="SomePackage" Version="*" />
```

#### Dependency Scanning — FOSSA no CI/CD

```yaml
# GitHub Actions — FOSSA para scan de vulnerabilidades e licenças
- name: Run FOSSA Scan
  uses: fossas/fossa-action@main
  with:
    api-key: ${{ secrets.FOSSA_API_KEY }}
    run-tests: true

# Também considere: dotnet CLI audit
- name: NuGet Audit
  run: dotnet list package --vulnerable --include-transitive
```

#### Integrity Verification

```powershell
# Verificar hash de pacote antes de usar em CI/CD
$hash = Get-FileHash "MyPackage.1.0.0.nupkg" -Algorithm SHA256
if ($hash.Hash -ne $expectedHash) {
    throw "Package integrity check failed!"
}
```

#### Minimize Surface Area

Remova dependências não utilizadas regularmente. Dependências transitivas desnecessárias também aumentam a superfície de ataque.

```bash
# Verificar dependências não utilizadas
dotnet tool install -g dotnet-outdated-tool
dotnet outdated

# Remover pacotes não usados
dotnet remove package UnusedPackage
```

#### Secrets Management

```csharp
// ❌ NUNCA em código ou repositório
var apiKey = "sk-prod-abc123secret";

// ✅ User Secrets (desenvolvimento local)
// dotnet user-secrets set "ExternalApi:Key" "dev-key"

// ✅ Azure Key Vault (produção)
builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());

// ✅ Variáveis de ambiente injetadas pelo orquestrador (Kubernetes Secrets)
var apiKey = Environment.GetEnvironmentVariable("EXTERNAL_API_KEY");
```

#### Dependency Configuration

```json
// appsettings.json — hardening de configurações
{
  "ConnectionStrings": {
    // ❌ NUNCA aqui — use Key Vault ou variáveis de ambiente
  },
  "AllowedHosts": "app.meudominio.com" // Não usar "*" em produção
}
```

### Developer Checklist — A06

- [ ] Todas as dependências estão com versão fixada (pinned)?
- [ ] Artifactory é a fonte primária de pacotes?
- [ ] FOSSA (ou equivalente) está ativo no CI/CD para scan de vulnerabilidades?
- [ ] Um Software Bill of Materials (SBOM) está sendo mantido?
- [ ] Secrets de build pipeline estão seguros (não em repositório)?
- [ ] Integridade de código e artefatos é verificada (checksums/assinaturas)?
- [ ] Pacotes não utilizados são removidos regularmente?

---

## A07 — Identification and Authentication Failures

### Visão Geral

Falhas de autenticação permitem que atacantes comprometam senhas, tokens de sessão, ou assumam identidades de outros usuários.

### Diretrizes para .NET

#### Password Storage

```csharp
// ✅ BCrypt com work factor adequado (mínimo 10, recomendado 12)
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

#### Multi-Factor Authentication

```csharp
// Integração com TOTP (Time-based One-Time Password) via Authenticator apps
// Biblioteca: Otp.NET

using OtpNet;
var secretKey = KeyGeneration.GenerateRandomKey(20);
var totp = new Totp(secretKey);
var totpCode = totp.ComputeTotp(); // Código atual

// Verificação do código informado pelo usuário
var isValid = totp.VerifyTotp(userCode, out long timeStepMatched,
    new VerificationWindow(previous: 1, future: 1)); // Tolerância de 1 step (30s)
```

#### Rate Limiting em Login

```csharp
// Implementação de lockout nativo do ASP.NET Core Identity
builder.Services.Configure<LockoutOptions>(options =>
{
    options.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.MaxFailedAccessAttempts = 5;
    options.AllowedForNewUsers = true;
});

// Resposta consistente — não revelar se email existe ou não
return BadRequest("Invalid credentials."); // Sempre a mesma mensagem
```

#### JWT Handling

```csharp
// Configuração segura de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
            ClockSkew = TimeSpan.FromSeconds(30), // Tolerância mínima
            // ⚠️ NUNCA aceitar algoritmo "none"
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };
    });

// Geração de token com expiração curta
var token = new JwtSecurityToken(
    issuer: config["Jwt:Issuer"],
    audience: config["Jwt:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(15), // Tokens de acesso: curta duração
    signingCredentials: credentials);
```

#### Session Management

```csharp
// Configuração de cookies de sessão seguros
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;       // Não acessível via JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Apenas HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Proteção CSRF
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Expirar por inatividade
    });

// Invalidar sessão no logout
[HttpPost("auth/logout")]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync();
    // Também invalidar refresh tokens no banco
    await _tokenService.RevokeUserTokensAsync(User.GetUserId());
    return Ok();
}
```

#### Password Policies

```csharp
builder.Services.Configure<PasswordOptions>(options =>
{
    options.RequiredLength = 12;
    options.RequireNonAlphanumeric = true;
    options.RequireUppercase = true;
    options.RequireLowercase = true;
    options.RequireDigit = true;
});

// Bloquear senhas comuns (lista de "Have I Been Pwned" ou similar)
public async Task<bool> IsPasswordCompromisedAsync(string password)
{
    var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(password));
    var prefix = Convert.ToHexString(sha1)[..5];
    // Consultar API k-anonymity do HIBP
    var response = await _httpClient.GetStringAsync(
        $"https://api.pwnedpasswords.com/range/{prefix}");
    return response.Contains(Convert.ToHexString(sha1)[5..]);
}
```

#### API Authentication — AuthX

```csharp
// Todo endpoint de API deve autenticar — exceções devem ser explícitas e documentadas
// Exemplo de serviço de autenticação centralizado (padrão DocuSign/AuthX)
public class AuthenticationService
{
    public async Task<AuthResult> AuthenticateAsync(HttpRequest request)
    {
        var token = ExtractBearerToken(request);
        if (string.IsNullOrEmpty(token))
            return AuthResult.Unauthorized("Missing token.");

        var principal = await ValidateTokenAsync(token);
        if (principal == null)
            return AuthResult.Unauthorized("Invalid or expired token.");

        return AuthResult.Success(principal);
    }
}

// Todo controller base deve chamar autenticação
public class SecureControllerBase : ControllerBase
{
    protected async Task<IActionResult?> EnsureAuthenticatedAsync()
    {
        var result = await _authService.AuthenticateAsync(Request);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Message });
        return null;
    }
}
```

### Developer Checklist — A07

- [ ] Senhas são armazenadas com hash seguro (bcrypt/Argon2)?
- [ ] MFA está implementado para todas as operações sensíveis?
- [ ] Tentativas de login são limitadas por rate limiting e logadas?
- [ ] Tokens de sessão e JWTs são imprevisíveis e de curta duração?
- [ ] Sessões são invalidadas corretamente no logout ou timeout?
- [ ] Falhas de autenticação são logadas para monitoramento?

---

## A08 — Software and Data Integrity Failures

### Visão Geral

Falhas de integridade ocorrem quando código ou dados são usados sem verificação de integridade — como atualizações de software sem assinatura, deserialização insegura, e pipelines CI/CD comprometidos.

### Diretrizes para .NET

#### Deserialização Segura

O `BinaryFormatter` é **extremamente perigoso** e foi desativado no .NET 9 por padrão. Nunca deserialize dados não confiáveis com ele ou com `SoapFormatter`, `NetDataContractSerializer`, ou `LosFormatter`.

```csharp
// ❌ PROIBIDO — BinaryFormatter desativado no .NET 9+
var obj = (MyType)new BinaryFormatter().Deserialize(stream);

// ✅ CORRETO — System.Text.Json (seguro por padrão)
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    MaxDepth = 32, // Limitar profundidade para evitar DoS
    // Não usar TypeNameHandling.All ou TypeNameHandling.Auto
};
var result = JsonSerializer.Deserialize<MyType>(json, options);

// ✅ Newtonsoft.Json com configuração segura
var settings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.None, // NUNCA usar All ou Auto com input externo
    MaxDepth = 32
};
var result = JsonConvert.DeserializeObject<MyType>(json, settings);
```

#### Software Updates — Verificação de Integridade

```csharp
// Verificar checksum antes de processar arquivo de atualização
public async Task<bool> VerifyFileIntegrityAsync(string filePath, string expectedSha256)
{
    using var sha256 = SHA256.Create();
    await using var stream = File.OpenRead(filePath);
    var hash = await sha256.ComputeHashAsync(stream);
    var actual = Convert.ToHexString(hash).ToLowerInvariant();
    return actual == expectedSha256.ToLowerInvariant();
}

// Verificar assinatura digital
public bool VerifySignature(byte[] data, byte[] signature, RSA publicKey)
{
    return publicKey.VerifyData(data, signature,
        HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
```

#### CI/CD Pipeline Security

```yaml
# GitHub Actions — boas práticas de segurança
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read        # Apenas leitura ao repositório
      packages: write       # Apenas para publicar pacotes

    steps:
      - uses: actions/checkout@v4
        with:
          persist-credentials: false # Não persistir token do GitHub

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      # Secrets via GitHub Secrets — nunca em código
      - name: Build
        env:
          CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
        run: dotnet build --configuration Release

      # Verificar integridade de artefatos
      - name: Compute artifact hash
        run: sha256sum ./publish/MyApp.dll > artifact.sha256
```

#### File Integrity Monitoring

```csharp
// Validar integridade de arquivos críticos na inicialização
public class FileIntegrityService
{
    private readonly Dictionary<string, string> _expectedHashes;

    public async Task ValidateCriticalFilesAsync()
    {
        foreach (var (filePath, expectedHash) in _expectedHashes)
        {
            var actual = await ComputeSha256Async(filePath);
            if (actual != expectedHash)
            {
                _logger.LogCritical(
                    "INTEGRITY VIOLATION: File {File} hash mismatch. Expected: {Expected}, Actual: {Actual}",
                    filePath, expectedHash, actual);
                // Alertar equipe de segurança imediatamente
                await _alertService.RaiseCriticalAlertAsync("File integrity violation", filePath);
            }
        }
    }
}
```

#### Package Sources

```xml
<!-- Restringir fontes de pacotes — apenas registries confiáveis -->
<configuration>
  <disabledPackageSources>
    <add key="nuget.org" value="true" /> <!-- Desabilitar em builds de produção -->
  </disabledPackageSources>
  <packageSources>
    <add key="Artifactory-Internal" value="https://artifactory.meudominio.com/..." />
  </packageSources>
</configuration>
```

### Developer Checklist — A08

- [ ] Atualizações de software são assinadas e validadas antes da instalação?
- [ ] Todos os pacotes de terceiros são verificados e escaneados?
- [ ] Operações de deserialização são evitadas em inputs não confiáveis?
- [ ] Acesso ao CI/CD é limitado e monitorado para adulteração?
- [ ] Checksum ou assinatura digital está em vigor para arquivos e artefatos críticos?

---

## A09 — Security Logging and Monitoring Failures

### Visão Geral

A ausência de logs adequados e monitoramento efetivo permite que ataques passem despercebidos. A detecção tardia amplia drasticamente o impacto de uma violação.

### Diretrizes para .NET

#### Configuração de Logging Estruturado

```csharp
// Program.cs — Serilog para logging estruturado
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.Seq("https://seq.meudominio.com") // Centralizado
        .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces);
});
```

#### Login and Access Events

```csharp
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Successful login: User={Email} IP={IpAddress} Timestamp={Timestamp}",
                request.Email, ipAddress, DateTimeOffset.UtcNow);
        }
        else
        {
            _logger.LogWarning(
                "Failed login attempt: User={Email} IP={IpAddress} Reason={Reason} Timestamp={Timestamp}",
                request.Email, ipAddress, result.FailureReason, DateTimeOffset.UtcNow);
        }

        return result.IsSuccess ? Ok(result.Token) : Unauthorized();
    }
}
```

#### Sensitive Operations — Audit Trail

```csharp
// Interceptor EF Core para auditoria automática
public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken token)
    {
        var context = eventData.Context!;
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted))
        {
            _logger.LogInformation(
                "Data change: Entity={Entity} Action={Action} User={UserId} IP={IpAddress} Timestamp={Timestamp}",
                entry.Entity.GetType().Name, entry.State, userId, ipAddress, DateTimeOffset.UtcNow);
        }

        return await base.SavingChangesAsync(eventData, result, token);
    }
}
```

#### Privacy-Aware Logging — Mascaramento de Dados Sensíveis

```csharp
// ❌ NUNCA logar dados sensíveis
_logger.LogDebug("Login attempt: Email={Email} Password={Password}", email, password);

// ✅ Mascarar ou excluir dados sensíveis dos logs
_logger.LogDebug("Login attempt: Email={Email}", email);

// Destructuring policy para mascaramento automático com Serilog
Log.Logger = new LoggerConfiguration()
    .Destructure.ByTransforming<LoginRequest>(r => new
    {
        r.Email,
        Password = "***REDACTED***" // Nunca logar senha
    })
    .CreateLogger();
```

#### Log Protection

```csharp
// Configurar logs para destino imutável e com acesso restrito
// Em Azure: Log Analytics Workspace com retenção e RBAC
// Em AWS: CloudWatch Logs com KMS encryption e resource policies

// Exemplo: appending logs para arquivo imutável local (desenvolvimento)
.WriteTo.File("logs/app-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30,
    shared: false,
    restrictedToMinimumLevel: LogEventLevel.Warning)
```

#### Alerting

```csharp
// Middleware de detecção de anomalias
public class AnomalyDetectionMiddleware
{
    private readonly IMemoryCache _cache;

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"requests:{ip}:{DateTime.UtcNow:yyyyMMddHHmm}";

        var count = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        _cache.Set(key, count + 1);

        if (count > 100) // Threshold configurável
        {
            _logger.LogWarning(
                "High request rate detected: IP={IpAddress} Count={Count} Window=1min",
                ip, count);
            await _alertService.TriggerAlertAsync("HighRequestRate", ip);
        }

        await _next(context);
    }
}
```

#### Correlation IDs para Rastreabilidade

```csharp
// Middleware de Correlation ID
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["UserId"] = context.User.GetUserId() ?? "anonymous",
            ["RequestPath"] = context.Request.Path,
            ["IpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        }))
        {
            await _next(context);
        }
    }
}
```

### Developer Checklist — A09

- [ ] Tentativas de autenticação (sucesso e falha) são logadas?
- [ ] Logs são revisados por um sistema de monitoramento centralizado?
- [ ] Dados sensíveis como senhas e tokens estão excluídos dos logs?
- [ ] Alertas são disparados em comportamentos suspeitos ou ações de alto risco?
- [ ] Mensagens de log são estruturadas para análise (JSON/campos tipados)?
- [ ] Logs são centralizados e protegidos contra adulteração?

---

## A10 — Server-Side Request Forgery (SSRF)

### Visão Geral

SSRF ocorre quando um servidor faz requisições HTTP a partir de URLs ou destinos fornecidos pelo usuário, permitindo que atacantes acessem recursos internos, metadata de cloud (ex.: `169.254.169.254`), ou serviços não expostos publicamente.

### Diretrizes para .NET

#### URL Validation — Allowlist Estrita

```csharp
// ✅ Validar URLs contra allowlist antes de fazer requisições
public class UrlValidator
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "api.parceiro.com",
        "cdn.meudominio.com"
    };

    public bool IsUrlAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Verificar schema (apenas HTTPS)
        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // Verificar host contra allowlist
        if (!AllowedHosts.Contains(uri.Host))
            return false;

        // Verificar se não é IP privado/reservado
        if (IsPrivateIpAddress(uri.Host))
            return false;

        return true;
    }

    private bool IsPrivateIpAddress(string host)
    {
        if (!IPAddress.TryParse(host, out var ip))
        {
            // Resolver DNS e verificar o IP resultante
            try
            {
                var addresses = Dns.GetHostAddresses(host);
                return addresses.Any(IsPrivateIp);
            }
            catch { return true; } // Falhar de forma segura
        }
        return IsPrivateIp(ip);
    }

    private static bool IsPrivateIp(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
            (bytes[0] == 10 ||
             (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
             (bytes[0] == 192 && bytes[1] == 168) ||
             bytes[0] == 127 || // Loopback
             (bytes[0] == 169 && bytes[1] == 254)); // Link-local (metadata cloud)
    }
}
```

#### DNS Rebinding Protection

```csharp
// Resolver hostname UMA VEZ e reusar o IP — evitar DNS rebinding
public class SafeHttpClientService
{
    public async Task<HttpResponseMessage> FetchUrlAsync(string url)
    {
        if (!_urlValidator.IsUrlAllowed(url))
            throw new SecurityException($"URL not in allowlist: {url}");

        var uri = new Uri(url);

        // Resolver DNS e validar IP antes de conectar
        var ipAddresses = await Dns.GetHostAddressesAsync(uri.Host);
        var targetIp = ipAddresses.First();

        if (_urlValidator.IsPrivateIpAddress(targetIp.ToString()))
            throw new SecurityException($"URL resolves to private IP: {targetIp}");

        // Conectar diretamente ao IP resolvido (evita novo lookup DNS)
        using var handler = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(5),
            ResponseDrainTimeout = TimeSpan.FromSeconds(10)
        };
        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10),
            MaxResponseContentBufferSize = 10 * 1024 * 1024 // 10MB max
        };

        return await client.GetAsync(url);
    }
}
```

#### No Raw URL Input

```csharp
// ❌ ERRADO — passar URL do usuário diretamente para HttpClient
[HttpPost("fetch")]
public async Task<IActionResult> FetchUrl([FromBody] FetchRequest request)
{
    var response = await _httpClient.GetAsync(request.Url); // SSRF
    return Ok(await response.Content.ReadAsStringAsync());
}

// ✅ CORRETO — validar e usar apenas IDs de fontes pré-aprovadas
[HttpPost("fetch")]
public async Task<IActionResult> FetchContent([FromBody] FetchRequest request)
{
    // Usar ID de fonte pre-aprovada, não URL direta
    if (!_approvedSources.TryGetValue(request.SourceId, out var baseUrl))
        return BadRequest("Unknown source.");

    var path = Uri.EscapeDataString(request.ResourcePath); // Apenas o path
    var fullUrl = $"{baseUrl}/{path}";
    var response = await _safeHttpClient.FetchUrlAsync(fullUrl);
    return Ok(await response.Content.ReadAsStringAsync());
}
```

#### Firewalling de Saída

Configure regras de firewall para que o servidor de aplicação **não possa** iniciar conexões para redes internas de dentro de uma requisição web. Isso é uma camada de defesa em profundidade.

```bash
# iptables — bloquear saída para redes privadas a partir do processo da aplicação
iptables -A OUTPUT -d 10.0.0.0/8 -j DROP
iptables -A OUTPUT -d 172.16.0.0/12 -j DROP
iptables -A OUTPUT -d 192.168.0.0/16 -j DROP
iptables -A OUTPUT -d 169.254.169.254/32 -j DROP  # AWS/Azure metadata
```

#### Proxying via API Gateway

Todas as requisições externas iniciadas pela aplicação devem passar por um **secure proxy ou API gateway** que:
- Aplique autenticação e autorização de saída
- Registre todas as chamadas externas
- Aplique rate limiting de saída
- Bloqueie destinos não autorizados

#### Timeouts e Size Limits

```csharp
// HttpClient configurado com limites adequados
builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.MaxResponseContentBufferSize = 5 * 1024 * 1024; // 5MB
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5))
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
```

### Developer Checklist — A10

- [ ] Controles de acesso são aplicados no servidor para todas as operações sensíveis?
- [ ] Object ownership é verificado antes de retornar dados?
- [ ] Rotas admin/API estão protegidas contra acesso não privilegiado?
- [ ] Todas as URLs fornecidas por usuário são validadas e restritas por hostname/IP?
- [ ] Acesso de rede de saída do servidor de aplicação é segmentado e firewallado?
- [ ] Requisições são proxiadas via serviço seguro ou gateway de allowlist?

---

## Checklist Consolidado

Use esta tabela para rastrear o progresso da sprint de segurança por módulo da aplicação.

| # | Categoria OWASP | Item de Verificação | Módulo Auth | Módulo Docs | Módulo Users | Status |
|---|---|---|---|---|---|---|
| 1 | A01 | Object-level authorization em todo endpoint sensível | ⬜ | ⬜ | ⬜ | Pendente |
| 2 | A01 | RBAC aplicado server-side (não client-side) | ⬜ | ⬜ | ⬜ | Pendente |
| 3 | A01 | Deny by default configurado globalmente | ⬜ | ⬜ | ⬜ | Pendente |
| 4 | A01 | Logs de access denied ativados | ⬜ | ⬜ | ⬜ | Pendente |
| 5 | A02 | Senhas hasheadas com bcrypt/Argon2 | ⬜ | — | ⬜ | Pendente |
| 6 | A02 | TLS 1.2+ enforced, ciphers fracos desabilitados | ⬜ | ⬜ | ⬜ | Pendente |
| 7 | A02 | Chaves em Key Vault (sem hardcode) | ⬜ | ⬜ | ⬜ | Pendente |
| 8 | A03 | Todas as queries SQL parametrizadas | ⬜ | ⬜ | ⬜ | Pendente |
| 9 | A03 | Input validation com allowlists/schema | ⬜ | ⬜ | ⬜ | Pendente |
| 10 | A03 | Sem raw OS command execution com input | ⬜ | ⬜ | ⬜ | Pendente |
| 11 | A04 | Threat modeling realizado para cada feature | ⬜ | ⬜ | ⬜ | Pendente |
| 12 | A04 | Rate limiting em endpoints críticos | ⬜ | ⬜ | ⬜ | Pendente |
| 13 | A04 | Valores sensíveis recalculados no servidor | ⬜ | ⬜ | ⬜ | Pendente |
| 14 | A05 | Stack traces não expostos em produção | ⬜ | ⬜ | ⬜ | Pendente |
| 15 | A05 | Security headers configurados | ⬜ | ⬜ | ⬜ | Pendente |
| 16 | A05 | CORS sem wildcards em produção | ⬜ | ⬜ | ⬜ | Pendente |
| 17 | A05 | Debug modes desabilitados em produção | ⬜ | ⬜ | ⬜ | Pendente |
| 18 | A06 | Versões de dependências fixadas (pinned) | ⬜ | ⬜ | ⬜ | Pendente |
| 19 | A06 | FOSSA ativo no CI/CD | ⬜ | ⬜ | ⬜ | Pendente |
| 20 | A06 | Secrets fora do repositório | ⬜ | ⬜ | ⬜ | Pendente |
| 21 | A07 | MFA para operações sensíveis | ⬜ | — | ⬜ | Pendente |
| 22 | A07 | JWT com validação de algoritmo, sem "none" | ⬜ | — | — | Pendente |
| 23 | A07 | Sessões invalidadas no logout | ⬜ | — | — | Pendente |
| 24 | A07 | Login rate-limited e bloqueio após tentativas | ⬜ | — | — | Pendente |
| 25 | A08 | BinaryFormatter removido/substituído | ⬜ | ⬜ | ⬜ | Pendente |
| 26 | A08 | Integridade de artefatos CI/CD verificada | ⬜ | ⬜ | ⬜ | Pendente |
| 27 | A08 | Lockfiles e SBOM mantidos | ⬜ | ⬜ | ⬜ | Pendente |
| 28 | A09 | Logging estruturado com Serilog/Seq | ⬜ | ⬜ | ⬜ | Pendente |
| 29 | A09 | Dados sensíveis mascarados nos logs | ⬜ | ⬜ | ⬜ | Pendente |
| 30 | A09 | Correlation IDs em todas as requisições | ⬜ | ⬜ | ⬜ | Pendente |
| 31 | A09 | Alertas configurados para eventos críticos | ⬜ | ⬜ | ⬜ | Pendente |
| 32 | A10 | Allowlist de URLs externas configurada | ⬜ | ⬜ | ⬜ | Pendente |
| 33 | A10 | DNS rebinding protection implementada | ⬜ | ⬜ | ⬜ | Pendente |
| 34 | A10 | Firewall de saída restringindo IPs privados | ⬜ | ⬜ | ⬜ | Pendente |
| 35 | A10 | Timeouts e size limits em HttpClient | ⬜ | ⬜ | ⬜ | Pendente |

---

## Referências

- [OWASP Top 10 — 2021](https://owasp.org/www-project-top-ten/)
- [OWASP Software Component Verification Standard](https://owasp.org/www-project-software-component-verification-standard/)
- [ASP.NET Core Error Handling — Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [ASP.NET Core Logging — Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging)
- [BinaryFormatter Security Guide — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide)
- [CodeQL — Insecure Direct Object Reference (C#)](https://codeql.github.com/codeql-query-help/csharp/cs-web-insecure-direct-object-reference/)
- [SQL Injection Prevention — Microsoft Docs](https://learn.microsoft.com/en-us/archive/msdn-magazine/2004/september/data-security-stop-sql-injection-attacks-before-they-stop-you)

---

*Documento gerado como parte do planejamento da sprint de segurança. Revise e adapte os exemplos de código às convenções e módulos específicos do seu projeto.*
