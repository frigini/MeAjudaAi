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
```csharp
### **Configuração no Program.cs**
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```text
## 📝 Estrutura de Logs

### **Template Serilog**
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
        "{CorrelationId} {SourceContext}{NewLine}{Exception}")
    .CreateLogger();
```sql
### **Exemplo de Log**
```json
[14:30:25 INF] User created successfully f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d MeAjudaAi.Users.Application
[14:30:25 INF] Email notification sent f7b3c4d2-8e91-4a6b-9c5d-1e2f3a4b5c6d MeAjudaAi.Notifications
```text
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
```sql
### **Message Bus Integration**
```csharp
public class DomainEventWithCorrelation
{
    public string CorrelationId { get; set; }
    public IDomainEvent Event { get; set; }
    public DateTime Timestamp { get; set; }
}
```csharp
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
```text
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
```text
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
```csharp
### **Logs Sem Correlation**
```csharp
// Verificar se LogContext está sendo usado
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    logger.LogInformation("This log will have correlation ID");
}
```text
## 🔗 Links Relacionados

- [Performance Monitoring](./PERFORMANCE.md)
- [SEQ Setup](./seq-setup.md)
- [SEQ Configuration](./seq-setup.md)
# Performance Monitoring - MeAjudaAi

Este documento descreve as estratégias e ferramentas de monitoramento de performance no MeAjudaAi.

## 📊 Métricas de Performance

### **Application Performance Monitoring (APM)**
- **OpenTelemetry**: Instrumentação automática para .NET
- **Traces distribuídos**: Rastreamento de requests entre serviços
- **Métricas de aplicação**: Contadores, histogramas e gauges

### **Métricas de Banco de Dados**
```csharp
public class DatabasePerformanceMetrics
{
    private readonly Counter<int> _queryCounter;
    private readonly Histogram<double> _queryDuration;
    
    public DatabasePerformanceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MeAjudaAi.Database");
        _queryCounter = meter.CreateCounter<int>("db_queries_total");
        _queryDuration = meter.CreateHistogram<double>("db_query_duration_ms");
    }
    
    public void RecordQuery(string operation, double durationMs)
    {
        _queryCounter.Add(1, new("operation", operation));
        _queryDuration.Record(durationMs, new("operation", operation));
    }
}
```csharp
## 🔍 Instrumentação

### **Custom Metrics**
- **Response times**: Tempo de resposta por endpoint
- **Throughput**: Requests por segundo
- **Error rates**: Taxa de erro por módulo
- **Resource utilization**: CPU, memória, I/O

### **Health Checks**
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UsersDbContext>("users-db")
    .AddRedis(connectionString)
    .AddRabbitMQ(rabbitMqConnection)
    .AddKeycloak();
```csharp
## 📈 Dashboards e Alertas

### **Grafana Dashboards**
- **Application Overview**: Métricas gerais da aplicação
- **Database Performance**: Performance do PostgreSQL
- **Infrastructure**: Recursos de sistema e containers

### **Alerting Rules**
- **High Error Rate**: > 5% em 5 minutos
- **Slow Response Time**: P95 > 2 segundos
- **Database Latency**: Queries > 1 segundo
- **Memory Usage**: > 85% de utilização

## 🎯 Performance Targets

### **Response Time SLAs**
- **API Endpoints**: P95 < 500ms
- **Database Queries**: P95 < 100ms
- **Authentication**: P95 < 200ms

### **Availability SLAs**
- **Application**: 99.9% uptime
- **Database**: 99.95% uptime
- **Cache**: 99.5% uptime

## 🔧 Otimização

### **Database Optimization**
- **Indexing**: Índices estratégicos por bounded context
- **Query optimization**: Análise de execution plans
- **Connection pooling**: Configuração adequada do pool

### **Caching Strategy**
- **Response caching**: Cache de responses HTTP
- **Distributed caching**: Redis para dados compartilhados
- **In-memory caching**: Cache local para dados estáticos

## 📝 Logging Performance

Integração com sistema de logging para correlação:

```csharp
logger.LogInformation("Query executed: {Operation} in {Duration}ms", 
    operation, duration);
```

## 🔗 Links Relacionados

- [Correlation ID Best Practices](./correlation-id.md)
- [SEQ Configuration](./seq-setup.md)
# 📊 Seq - Logging Estruturado com Serilog

## 🚀 Setup Rápido para Desenvolvimento

### Docker Compose (Recomendado)

Adicione ao seu `docker-compose.development.yml`:

```yaml
services:
  seq:
    image: datalust/seq:latest
    container_name: meajudaai-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    restart: unless-stopped

volumes:
  seq_data:
```

### Docker Run (Simples)

```bash
docker run -d \
  --name seq \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -v seq_data:/data \
  datalust/seq:latest
```

## 🎯 Configuração por Ambiente

### Development
- **URL**: `http://localhost:5341`
- **Interface**: `http://localhost:5341`
- **Custo**: 🆓 Gratuito
- **Limite**: Ilimitado

### Production
- **URL**: Configure `${SEQ_SERVER_URL}`
- **API Key**: Configure `${SEQ_API_KEY}`
- **Custo**: 🆓 Gratuito até 32MB/dia
- **Escalabilidade**: $390/ano para 1GB/dia

## 📱 Interface Web

Acesse `http://localhost:5341` para:
- ✅ **Busca estruturada** com sintaxe SQL-like
- ✅ **Filtros por propriedades** (UserId, CorrelationId, etc.)
- ✅ **Dashboards** personalizados
- ✅ **Alertas** por email/webhook
- ✅ **Análise de trends** e performance

## 🔍 Exemplos de Queries

```sql
-- Buscar por usuário específico
UserId = "123" and @Level = "Error"

-- Buscar por correlation ID
CorrelationId = "abc-123-def"

-- Performance lenta
@Message like "%responded%" and Elapsed > 1000

-- Erros de autenticação
@Message like "%authentication%" and @Level = "Error"
```

## 💰 Custos por Volume

| Volume/Dia | Eventos/Dia | Custo/Ano | Cenário |
|------------|-------------|-----------|---------|
| < 32MB | ~100k | 🆓 $0 | MVP/Startup |
| < 1GB | ~3M | $390 | Crescimento |
| < 10GB | ~30M | $990 | Empresa |

## 🛠️ Comandos Úteis

```bash
# Iniciar Seq
docker start seq

# Ver logs do Seq
docker logs seq

# Backup dos dados
docker exec seq cat /data/Documents/seq.db > backup.db

# Verificar saúde
curl http://localhost:5341/api/diagnostics/status
```

## 🎯 Próximos Passos

1. **Desenvolvimento**: Execute `docker run` e acesse `localhost:5341`
2. **CI/CD**: Adicione Seq ao pipeline de desenvolvimento
3. **Produção**: Configure servidor Seq dedicado
4. **Monitoramento**: Configure alertas para erros críticos

## 🔗 Links Úteis

- [Documentação Seq](https://docs.datalust.co/docs)
- [Serilog + Seq](https://docs.datalust.co/docs/using-serilog)
- [Pricing](https://datalust.co/pricing)
