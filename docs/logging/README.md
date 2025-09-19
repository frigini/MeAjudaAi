# 📝 Sistema de Logging Estruturado - MeAjudaAi

## 🎯 Visão Geral

Sistema de logging híbrido que combina:
- ⚙️ **Configuração** via `appsettings.json`
- 🏗️ **Lógica avançada** via código C#
- 📊 **Coleta estruturada** via Seq

## 🏗️ Arquitetura

```
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

### Propriedades Automáticas
```json
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

### Exemplo Básico
```csharp
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public async Task<IActionResult> GetUser(int id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);
        
        using (_logger.PushOperationContext("GetUser", new { UserId = id }))
        {
            var user = await _userService.GetByIdAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", id);
                return NotFound();
            }
            
            _logger.LogInformation("User {UserId} fetched successfully", id);
            return Ok(user);
        }
    }
}
```

### Contexto Avançado
```csharp
public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
{
    using (_logger.PushUserContext(User.FindFirst("sub")?.Value, User.Identity?.Name))
    using (_logger.PushOperationContext("UpdateUser", new { UserId = id, request }))
    {
        _logger.LogInformation("Starting user update for {UserId}", id);
        
        try
        {
            var result = await _userService.UpdateAsync(id, request);
            _logger.LogInformation("User {UserId} updated successfully", id);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for user {UserId}: {Errors}", 
                id, ex.Errors);
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