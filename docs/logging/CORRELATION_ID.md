# Correlation ID Best Practices - MeAjudaAi

Este documento descreve as melhores práticas para implementação e uso de Correlation IDs no MeAjudaAi.

## 🎯 O que é Correlation ID

O **Correlation ID** é um identificador único que acompanha uma requisição através de todos os serviços e componentes, permitindo rastrear e correlacionar logs de uma operação completa.

## 🛠️ Implementação

### **Geração Automática**
```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                          ?? Guid.NewGuid().ToString();
                          
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

### **Configuração no Program.cs**
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

## 📝 Estrutura de Logs

### **Template Serilog**
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
        "{CorrelationId} {SourceContext}{NewLine}{Exception}")
    .CreateLogger();
```

### **Exemplo de Log**
```
[14:30:25 INF] User created successfully f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d MeAjudaAi.Users.Application
[14:30:25 INF] Email notification sent f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d MeAjudaAi.Notifications
```

## 🔄 Propagação Entre Serviços

### **HTTP Client Configuration**
```csharp
public class CorrelationIdHttpClientHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdHttpClientHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### **Message Bus Integration**
```csharp
public class DomainEventWithCorrelation
{
    public string CorrelationId { get; set; }
    public IDomainEvent Event { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## 🔍 Rastreamento

### **Queries no SEQ**
```sql
-- Buscar todos os logs de uma operação
CorrelationId = "f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d"

-- Operações com erro
CorrelationId = "f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d" and @Level = "Error"

-- Performance de uma operação
CorrelationId = "f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d" 
| where @Message like "%completed%"
| project @Timestamp, Duration
```

## 📊 Métricas e Monitoring

### **Correlation ID Metrics**
```csharp
public class CorrelationMetrics
{
    private readonly Histogram<double> _requestDuration;
    
    public CorrelationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MeAjudaAi.Correlation");
        _requestDuration = meter.CreateHistogram<double>("request_duration_ms");
    }
    
    public void RecordRequestDuration(string correlationId, double durationMs)
    {
        _requestDuration.Record(durationMs, 
            new("correlation_id", correlationId));
    }
}
```

### **Dashboard Queries**
- **Average Request Duration**: Tempo médio por correlation ID
- **Error Rate**: Percentual de correlation IDs com erro
- **Service Hops**: Número de serviços por requisição

## ✅ Melhores Práticas

### **Formato do Correlation ID**
- **UUID v4**: Garantia de unicidade global
- **Formato**: `f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d`
- **Case**: Lowercase para consistência

### **Propagação**
- ✅ **Sempre propague** entre serviços HTTP
- ✅ **Inclua em eventos** de domain
- ✅ **Adicione em logs** estruturados
- ✅ **Retorne no response** para debugging

### **Logging**
- ✅ **Use structured logging** (Serilog)
- ✅ **Contexto automático** via middleware
- ✅ **Enrichment** em todos os logs
- ✅ **Correlation na exception** handling

## 🚨 Troubleshooting

### **Correlation ID Missing**
```csharp
// Verificar se middleware está registrado
app.UseMiddleware<CorrelationIdMiddleware>();

// Verificar ordem dos middlewares
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
```

### **Logs Sem Correlation**
```csharp
// Verificar se LogContext está sendo usado
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    logger.LogInformation("This log will have correlation ID");
}
```

## 🔗 Links Relacionados

- [Logging Setup](./README.md)
- [Performance Monitoring](./performance.md)
- [SEQ Configuration](./seq_setup.md)