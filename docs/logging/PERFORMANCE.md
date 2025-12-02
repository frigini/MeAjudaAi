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