# 📝 Sistema de Logging Estruturado - MeAjudaAi

## 🎯 Visão Geral

Sistema de logging híbrido que combina:
- ⚙️ **Configuração** via `appsettings.json`
- 🏗️ **Lógica avançada** via código C#
- 📊 **Coleta estruturada** via Seq

## 🏗️ Arquitetura

```text
HTTP Request → LoggingContextMiddleware → Serilog → Console + Seq
                      ↓
              [CorrelationId, UserContext, Performance]
```

## 🔧 Componentes

### 1. **LoggingContextMiddleware**
- ✅ Adiciona Correlation ID automático
- ✅ Captura contexto de requisição
- ✅ Mede tempo de resposta
- ✅ Enriquece logs com dados do usuário

### 2. **SerilogConfigurator**
- ✅ Configuração híbrida (JSON + C#)
- ✅ Enrichers automáticos por ambiente
- ✅ Integração com Application Insights

### 3. **CorrelationIdEnricher**
- ✅ Correlation ID por requisição
- ✅ Rastreamento distribuído
- ✅ Headers HTTP automáticos

## 📊 Estrutura de Logs

### 🔒 Configuração de PII (Informações Pessoais)

> ⚠️ **SEGURANÇA**: Por padrão, dados pessoais (PII) são SEMPRE redacted em logs para proteção de privacidade e conformidade LGPD/GDPR.

**Configuração em `appsettings.json`:**
```jsonc
{
  "Logging": {
    "SuppressPII": true,  // Padrão: true (produção)
    "PII": {
      "EnableInDevelopment": true,   // Apenas em Development
      "RedactionText": "[REDACTED]", // Texto de substituição
      "HashTechnicalIds": true,      // Hash IDs técnicos em produção (opcional)
      "HashAlgorithm": "SHA-256",    // Algoritmo para hash dos IDs
      "AllowedFields": ["CorrelationId", "UserId", "SessionId"] // IDs técnicos sempre permitidos*
    }
  }
}
```

**Configuração por ambiente:**
```jsonc
// appsettings.Development.json - APENAS desenvolvimento local
{
  "Logging": {
    "SuppressPII": false  // Permitir PII apenas em dev local
  }
}

// appsettings.Production.json - OBRIGATÓRIO em produção
{
  "Logging": {
    "SuppressPII": true   // SEMPRE redact PII em produção
  }
}
```

### Propriedades Automáticas

**Com SuppressPII=true (Padrão/Produção):**
```jsonc
{
  "Timestamp": "2025-09-17T10:30:00.123Z",
  "Level": "Information",
  "CorrelationId": "abc-123-def-456",
  "Message": "Request completed GET /users/***",
  "Properties": {
    "Application": "MeAjudaAi",
    "Environment": "Production",
    "RequestPath": "/users/***",
    "RequestMethod": "GET", 
    "StatusCode": 200,
    "ElapsedMilliseconds": 45,
    "UserId": "user-123",
    "Username": "[REDACTED]"
  }
}
```

**Com SuppressPII=false (Development apenas):**
```jsonc
{
  "Timestamp": "2025-09-17T10:30:00.123Z",
  "Level": "Information",
  "CorrelationId": "abc-123-def-456",
  "Message": "Request completed GET /users/123",
  "Properties": {
    "Application": "MeAjudaAi",
    "Environment": "Development",
    "RequestPath": "/users/123",
    "RequestMethod": "GET",
    "StatusCode": 200,
    "ElapsedMilliseconds": 45,
    "UserId": "user-123",
    "Username": "joao.silva"
  }
}
```

## 🎯 Uso nos Controllers

### 🔒 Logging com Proteção PII

**Regras de PII nos Logs:**
- ✅ **IDs técnicos**: Sempre permitidos (UserId, CorrelationId, SessionId)*
  - *Estes IDs são necessários para correlação e debugging em produção*
  - *Não contêm informações pessoais identificáveis diretamente*
- ❌ **Dados pessoais**: Sempre redacted (Username, Email, Nome, CPF, etc.)
- ⚠️ **Dados sensíveis**: Sempre redacted (Passwords, Tokens, Keys)

> **\*Nota de Conformidade**: IDs técnicos são permitidos quando pseudonimizados e governados por controles de acesso. Em jurisdições rigorosas ou políticas organizacionais específicas, habilite `HashTechnicalIds: true` em produção para aplicar hash SHA-256 aos identificadores.

### Exemplo Básico
```csharp
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IPIILogger _piiLogger; // Logger com proteção PII

    public async Task<IActionResult> GetUser(int id)
    {
        // ✅ Seguro - IDs técnicos são permitidos
        _logger.LogInformation("Fetching user {UserId}", id);
        
        using (_logger.PushOperationContext("GetUser", new { UserId = id }))
        {
            var user = await _userService.GetByIdAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", id);
                return NotFound();
            }
            
            // ✅ Seguro - redaction automática de PII baseada na configuração
            _piiLogger.LogInformation("User {UserId} with email {Email} fetched", 
                user.Id, user.Email); // Email será redacted se SuppressPII=true
                
            _logger.LogInformation("User {UserId} fetched successfully", id);
            return Ok(user);
        }
    }
}
```

### Contexto Avançado com Proteção PII
```csharp
public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
{
    // ✅ Seguro - Subject ID é técnico, mas Username pode ser PII
    var subjectId = User.FindFirst("sub")?.Value;
    var username = User.Identity?.Name; // Será redacted automaticamente se for PII
    
    using (_piiLogger.PushUserContext(subjectId, username)) // PII-aware context
    using (_logger.PushOperationContext("UpdateUser", new { UserId = id })) // Não incluir request completo
    {
        _logger.LogInformation("Starting user update for {UserId}", id);
        
        // ✅ Log apenas campos não-PII do request
        _logger.LogDebug("Update request for {UserId} with {FieldCount} fields", 
            id, request.GetModifiedFieldsCount());
        
        try
        {
            var result = await _userService.UpdateAsync(id, request);
            _logger.LogInformation("User {UserId} updated successfully", id);
            
            // ✅ Opcionalmente log dados PII apenas se configurado
            _piiLogger.LogInformation("User {UserId} ({Email}) profile updated", 
                result.Id, result.Email); // Email redacted em produção
                
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            // ❌ Não logar ex.Errors diretamente - pode conter PII
            _logger.LogWarning(ex, "Validation failed for user {UserId} with {ErrorCount} errors", 
                id, ex.Errors?.Count ?? 0);
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", id);
            throw;
        }
    }
}
```

### Implementação do IPIILogger
```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class PIIAwareLogger : IPIILogger
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly bool _suppressPII;

    public PIIAwareLogger(ILogger logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _suppressPII = _config.GetValue<bool>("Logging:SuppressPII", true);
    }

    public void LogInformation(string messageTemplate, params object[] args)
    {
        if (_suppressPII)
        {
            // Redact PII fields using template-aware redaction
            args = RedactPIIInArguments(messageTemplate, args);
        }
        _logger.LogInformation(messageTemplate, args);
    }

    private object[] RedactPIIInArguments(string messageTemplate, object[] args)
    {
        // Parse template placeholders to map parameter names to argument indices
        var placeholders = ExtractPlaceholders(messageTemplate);
        
        for (int i = 0; i < args.Length && i < placeholders.Count; i++)
        {
            var parameterName = placeholders[i];
            
            // Check if parameter name matches PII field patterns
            if (IsPIIField(parameterName) || IsPotentialPII(args[i]))
            {
                var redactionText = _config.GetValue<string>("Logging:PII:RedactionText", "[REDACTED]");
                args[i] = redactionText;
            }
            else
            {
                // Check if technical ID hashing is enabled for allowed fields
                args[i] = HashIfRequired(args[i]?.ToString() ?? "", parameterName);
            }
        }
        
        return args;
    }

    private List<string> ExtractPlaceholders(string messageTemplate)
    {
        // Extract {ParameterName} placeholders from message template
        // Handle both positional {0} and named {UserId} placeholders
        var regex = new Regex(@"\{([^}]+)\}");
        return regex.Matches(messageTemplate)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
    }

    private bool IsPIIField(string fieldName)
    {
        // Read AllowedFields from configuration
        var allowedFields = _config.GetSection("Logging:PII:AllowedFields")
            .Get<string[]>() ?? ["CorrelationId", "UserId", "SessionId"];
        
        // Check if field is in allowed list (case-insensitive)
        if (allowedFields.Any(field => 
            string.Equals(field, fieldName, StringComparison.OrdinalIgnoreCase)))
        {
            return false; // Not PII - field is explicitly allowed
        }
        
        // Check against configured PII field patterns
        var piiFields = new[] { "Email", "Username", "Name", "Phone", "CPF" };
        return piiFields.Any(field => 
            fieldName.Contains(field, StringComparison.OrdinalIgnoreCase));
    }
    
    private string HashIfRequired(string value, string fieldName)
    {
        // Check if technical ID hashing is enabled
        var hashTechnicalIds = _config.GetValue<bool>("Logging:PII:HashTechnicalIds", false);
        if (!hashTechnicalIds) return value;
        
        // Only hash technical IDs (allowed fields)
        var allowedFields = _config.GetSection("Logging:PII:AllowedFields")
            .Get<string[]>() ?? ["CorrelationId", "UserId", "SessionId"];
            
        if (!allowedFields.Any(field => 
            string.Equals(field, fieldName, StringComparison.OrdinalIgnoreCase)))
        {
            return value; // Not a technical ID - don't hash
        }
        
        // Hash the technical ID using configured algorithm
        var algorithm = _config.GetValue<string>("Logging:PII:HashAlgorithm", "SHA-256");
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hashBytes)[..8]; // First 8 chars for readability
    }
}
```

## 🛡️ Melhores Práticas de PII

### Configuração de Ambientes

**Development (Local):**
```jsonc
{
  "Logging": {
    "SuppressPII": false,  // Permitir PII para debug local
    "PII": {
      "WarnOnPII": true,   // Avisar quando PII é detectado
      "LogPIIAccess": true // Log quando PII é acessado
    }
  }
}
```

**Staging/Testing:**
```jsonc
{
  "Logging": {
    "SuppressPII": true,   // OBRIGATÓRIO redact PII
    "PII": {
      "StrictMode": true,  // Modo rigoroso de detecção
      "AuditPIIAttempts": true // Auditar tentativas de log PII
    }
  }
}
```

**Production:**
```jsonc
{
  "Logging": {
    "SuppressPII": true,   // SEMPRE redact PII
    "PII": {
      "StrictMode": true,
      "AuditPIIAttempts": true,
      "HashTechnicalIds": true,      // Hash IDs técnicos para compliance
      "HashAlgorithm": "SHA-256",    // Algoritmo de hash seguro
      "AlertOnPIIBreach": true // Alertas automáticos
    }
  }
}
```

### Classificação de Dados PII

| Categoria | Exemplos | Ação |
|-----------|----------|------|
| **IDs Técnicos*** | UserId, SessionId, CorrelationId | ✅ Sempre permitido |
| **PII Direto** | Email, CPF, Nome, Telefone | ❌ Sempre redact |
| **PII Indireto** | Username, IP, Endereço | ⚠️ Redact por padrão |
| **Dados Sensíveis** | Passwords, Tokens, Keys | 🚫 NUNCA logar |

> **\*IDs Técnicos**: Permitidos quando pseudonimizados e governados por controles de acesso. Configure `HashTechnicalIds: true` se exigido por política organizacional ou jurisdição.

### Validação de Configuração

```csharp
// Startup validation
public void ValidateLoggingConfiguration()
{
    var suppressPII = _config.GetValue<bool>("Logging:SuppressPII");
    var environment = _config.GetValue<string>("ASPNETCORE_ENVIRONMENT");
    
    // OBRIGATÓRIO: PII deve estar suprimido em produção
    if (environment == "Production" && !suppressPII)
    {
        throw new InvalidOperationException(
            "SECURITY: SuppressPII MUST be true in Production environment");
    }
    
    // AVISO: PII habilitado em outros ambientes
    if (!suppressPII && environment != "Development")
    {
        _logger.LogWarning("PII logging is ENABLED in {Environment}. " +
            "Ensure this is intentional for debugging purposes only", environment);
    }
}
```

## 🔍 Queries Úteis no Seq

### Performance
```sql
-- Requests lentos (> 1 segundo)
@Message like "%completed%" and ElapsedMilliseconds > 1000

-- Top 10 endpoints mais lentos
@Message like "%completed%" 
| summarize avg(ElapsedMilliseconds) by RequestPath 
| order by avg_ElapsedMilliseconds desc 
| limit 10
```

### Erros
```sql
-- Erros por usuário
@Level = "Error" and UserId is not null
| summarize count() by UserId
| order by count desc

-- Correlation ID para debug
CorrelationId = "abc-123-def"
```

### Business Intelligence
```sql
-- Atividade por módulo
@Message like "%completed%" 
| summarize count() by substring(RequestPath, 0, indexof(RequestPath, '/', 1))
| order by count desc
```

## 🚀 Próximos Passos

1. ✅ **Implementado** - Sistema base de logging
2. 🔄 **Próximo** - Métricas e monitoramento
3. 📋 **Pendente** - Alertas automáticos
4. 📊 **Futuro** - Dashboards customizados

## 🔗 Documentação Relacionada

- [Seq Setup](./SEQ_SETUP.md)
- [Correlation ID Best Practices](./CORRELATION_ID.md)
- [Performance Monitoring](./PERFORMANCE.md)