# Estratégia de Dead Letter Queue (DLQ) - MeAjudaAi

## 📋 Visão Geral

O sistema de Dead Letter Queue (DLQ) do MeAjudaAi implementa uma estratégia robusta de recuperação de falhas em mensageria, fornecendo:

- **Retry automático** com backoff exponencial
- **Classificação inteligente** de tipos de falha
- **Dead Letter Queue** para mensagens que falharam permanentemente
- **Monitoramento e observabilidade** completos
- **Suporte multi-transporte** (RabbitMQ e Azure Service Bus)

## 🏗️ Arquitetura

### Componentes Principais

```csharp
┌─────────────────────────────────────────────────────────┐
│                    Message Handler                       │
│                                                         │
│  ┌─────────────────┐    ┌─────────────────────────────┐ │
│  │ Retry Middleware │───▶│ Dead Letter Service         │ │
│  │                 │    │ - Classification            │ │
│  │ - Backoff       │    │ - Retry Logic              │ │
│  │ - Attempt Count │    │ - DLQ Sending              │ │
│  └─────────────────┘    └─────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
         ┌─────────────────────────────────────┐
         │         Transport Layer             │
         │                                     │
         │  ┌─────────────┐ ┌─────────────────┐│
         │  │ RabbitMQ    │ │ Azure Service   ││
         │  │ DLQ         │ │ Bus DLQ         ││
         │  │             │ │                 ││
         │  └─────────────┘ └─────────────────┘│
         └─────────────────────────────────────┘
```text
### Factory Pattern por Ambiente

- **Development**: `RabbitMqDeadLetterService`
- **Testing**: `NoOpDeadLetterService`
- **Production**: `ServiceBusDeadLetterService`

## ⚙️ Configuração

### appsettings.json

```json
{
  "Messaging": {
    "DeadLetter": {
      "Enabled": true,
      "MaxRetryAttempts": 3,
      "InitialRetryDelaySeconds": 5,
      "BackoffMultiplier": 2.0,
      "MaxRetryDelaySeconds": 300,
      "DeadLetterTtlHours": 72,
      "EnableDetailedLogging": true,
      "EnableAdminNotifications": true,
      "NonRetryableExceptions": [
        "System.ArgumentException",
        "MeAjudaAi.Shared.Exceptions.BusinessRuleException"
      ],
      "RetryableExceptions": [
        "System.TimeoutException",
        "System.Net.Http.HttpRequestException"
      ]
    }
  }
}
```csharp
### Registro no Container DI

```csharp
// Program.cs ou Startup.cs
services.AddMessaging(configuration, environment); // Inclui DLQ automaticamente

// Ou configuração específica
services.AddDeadLetterQueue(configuration, environment, options =>
{
    options.ConfigureForProduction(); // ou ConfigureForDevelopment()
});
```yaml
## 🔄 Classificação de Falhas

### Tipos de Falha

| Tipo | Comportamento | Exemplos |
|------|---------------|----------|
| **Permanent** | Não faz retry, vai direto para DLQ | `ArgumentException`, `BusinessRuleException` |
| **Transient** | Faz retry com backoff | `TimeoutException`, `HttpRequestException` |
| **Critical** | Não faz retry, alerta crítico | `OutOfMemoryException`, `StackOverflowException` |
| **Unknown** | Retry conservador (máx. 50% das tentativas) | Exceções não classificadas |

### Configuração de Exceções

```csharp
public class DeadLetterOptions
{
    // Exceções que NÃO devem causar retry
    public string[] NonRetryableExceptions { get; set; } = {
        "System.ArgumentException",
        "System.ArgumentNullException",
        "MeAjudaAi.Shared.Exceptions.BusinessRuleException"
    };

    // Exceções que SEMPRE devem causar retry
    public string[] RetryableExceptions { get; set; } = {
        "System.TimeoutException",
        "System.Net.Http.HttpRequestException",
        "Npgsql.PostgresException"
    };
}
```csharp
## 🔧 Implementação

### 1. Handler com Retry Automático

```csharp
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IDeadLetterService _deadLetterService;

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Opção 1: Extensão simples
        var success = await @event.ExecuteWithRetryAsync(
            ProcessUserCreatedEvent,
            serviceProvider,
            sourceQueue: "users-events",
            cancellationToken);

        if (!success)
        {
            _logger.LogError("Event sent to DLQ: {UserId}", @event.UserId);
        }
    }

    private async Task ProcessUserCreatedEvent(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Lógica que pode falhar
        await SendWelcomeEmailAsync(@event);
        await UpdateStatisticsAsync(@event);
    }
}
```yaml
### 2. Middleware Manual

```csharp
public class CustomEventHandler
{
    private readonly IMessageRetryMiddlewareFactory _retryFactory;

    public async Task HandleCustomEvent(CustomEvent @event)
    {
        var middleware = _retryFactory.CreateMiddleware<CustomEvent>(
            handlerType: nameof(CustomEventHandler),
            sourceQueue: "custom-events");

        var success = await middleware.ExecuteWithRetryAsync(
            @event, 
            ProcessCustomEvent, 
            cancellationToken);
    }
}
```csharp
## 📊 Monitoramento e Observabilidade

### 1. Métricas

O sistema registra automaticamente:

- **Tentativas de retry** por tipo de mensagem
- **Mensagens enviadas para DLQ** por motivo
- **Taxa de falha** por handler
- **Tempo de processamento** por tentativa

### 2. Logs Estruturados

```csharp
// Exemplo de logs gerados automaticamente
_logger.LogWarning(
    "Message sent to dead letter queue. MessageId: {MessageId}, Type: {MessageType}, Attempts: {Attempts}, Reason: {Reason}",
    messageId, messageType, attemptCount, exception.Message);
```text
### 3. API de Administração

```csharp
[ApiController]
[Route("api/admin/deadletter")]
public class DeadLetterController : ControllerBase
{
    // GET /api/admin/deadletter/statistics
    [HttpGet("statistics")]
    public async Task<DeadLetterStatistics> GetStatistics()
    {
        return await _deadLetterService.GetDeadLetterStatisticsAsync();
    }

    // GET /api/admin/deadletter/queues/{queueName}/messages
    [HttpGet("queues/{queueName}/messages")]
    public async Task<IEnumerable<FailedMessageInfo>> ListMessages(string queueName)
    {
        return await _deadLetterService.ListDeadLetterMessagesAsync(queueName);
    }

    // POST /api/admin/deadletter/queues/{queueName}/messages/{messageId}/reprocess
    [HttpPost("queues/{queueName}/messages/{messageId}/reprocess")]
    public async Task ReprocessMessage(string queueName, string messageId)
    {
        await _deadLetterService.ReprocessDeadLetterMessageAsync(queueName, messageId);
    }
}
```yaml
## 🔀 Transporte por Ambiente

### RabbitMQ (Development)

```csharp
// Configuração automática de Dead Letter Exchange (DLX)
public class RabbitMqDeadLetterService : IDeadLetterService
{
    private async Task EnsureDeadLetterInfrastructureAsync(string deadLetterQueueName)
    {
        // Declara exchange de dead letter
        _channel.ExchangeDeclare(
            exchange: "dlx.meajudaai",
            type: ExchangeType.Topic,
            durable: true);

        // Declara fila com TTL
        var arguments = new Dictionary<string, object>
        {
            ["x-message-ttl"] = TimeSpan.FromHours(24).TotalMilliseconds
        };

        _channel.QueueDeclare(
            queue: deadLetterQueueName,
            durable: true,
            arguments: arguments);
    }
}
```csharp
### Azure Service Bus (Production)

```csharp
// Usa Dead Letter Queues nativas do Service Bus
public class ServiceBusDeadLetterService : IDeadLetterService
{
    public async Task SendToDeadLetterAsync<TMessage>(TMessage message, Exception exception, ...)
    {
        var deadLetterQueueName = $"{sourceQueue}$DeadLetterQueue";
        var sender = _client.CreateSender(deadLetterQueueName);

        var serviceBusMessage = new ServiceBusMessage(failedMessageInfo.ToJson())
        {
            TimeToLive = TimeSpan.FromHours(_options.DeadLetterTtlHours)
        };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
```text
## 🚀 Algoritmo de Retry

### Backoff Exponencial

```csharp
public TimeSpan CalculateRetryDelay(int attemptCount)
{
    var baseDelay = TimeSpan.FromSeconds(_options.InitialRetryDelaySeconds);
    var exponentialDelay = TimeSpan.FromSeconds(
        baseDelay.TotalSeconds * Math.Pow(_options.BackoffMultiplier, attemptCount - 1));
    var maxDelay = TimeSpan.FromSeconds(_options.MaxRetryDelaySeconds);

    return exponentialDelay > maxDelay ? maxDelay : exponentialDelay;
}
```csharp
### Exemplo de Sequência

| Tentativa | Delay | Cálculo |
|-----------|-------|---------|
| 1 | 0s | Primeira tentativa |
| 2 | 5s | 5 × 2^0 = 5s |
| 3 | 10s | 5 × 2^1 = 10s |
| 4 | 20s | 5 × 2^2 = 20s |
| 5 | 40s | 5 × 2^3 = 40s |

## 📈 Estrutura de Dados

### FailedMessageInfo

```csharp
public sealed class FailedMessageInfo
{
    public string MessageId { get; set; }
    public string MessageType { get; set; }
    public string OriginalMessage { get; set; }
    public string SourceQueue { get; set; }
    public DateTime FirstAttemptAt { get; set; }
    public DateTime LastAttemptAt { get; set; }
    public int AttemptCount { get; set; }
    public string LastFailureReason { get; set; }
    public List<FailureAttempt> FailureHistory { get; set; }
    public EnvironmentMetadata Environment { get; set; }
}
```yaml
### Estatísticas DLQ

```csharp
public sealed class DeadLetterStatistics
{
    public int TotalDeadLetterMessages { get; set; }
    public Dictionary<string, int> MessagesByQueue { get; set; }
    public Dictionary<string, int> MessagesByExceptionType { get; set; }
    public Dictionary<string, FailureRate> FailureRateByHandler { get; set; }
}
```csharp
## 🔧 Validação e Saúde

### Health Check

```csharp
// Validação automática na inicialização
await host.EnsureMessagingInfrastructureAsync(); // Inclui DLQ validation

// Health check endpoint
GET /api/admin/deadletter/health
{
  "serviceType": "RabbitMqDeadLetterService",
  "shouldRetry": true,
  "retryDelayMs": 5000,
  "status": "Healthy"
}
```text
## 🚦 Cenários de Uso

### 1. Falha de Rede Temporária
```text
Tentativa 1: HttpRequestException → Retry em 5s
Tentativa 2: HttpRequestException → Retry em 10s  
Tentativa 3: Sucesso ✅
```text
### 2. Erro de Validação
```yaml
Tentativa 1: ArgumentException → DLQ imediatamente ❌
```text
### 3. Falha Persistente
```yaml
Tentativa 1: TimeoutException → Retry em 5s
Tentativa 2: TimeoutException → Retry em 10s
Tentativa 3: TimeoutException → DLQ ❌
```text
## 📚 Documentos Relacionados

- [Estratégia de MessageBus por Ambiente](./message_bus_environment_strategy.md)
- [Guia de Infraestrutura](../infrastructure.md)
- [Arquitetura e Padrões](../architecture.md)

## ✅ Benefícios Implementados

- ✅ **Recuperação automática** de falhas temporárias
- ✅ **Prevenção de loops infinitos** com limites de retry
- ✅ **Classificação inteligente** de tipos de falha
- ✅ **Observabilidade completa** com logs e métricas
- ✅ **Flexibilidade de configuração** por ambiente
- ✅ **Ferramentas de administração** para reprocessamento
- ✅ **Suporte multi-transporte** (RabbitMQ/Service Bus)
- ✅ **Integração transparente** com handlers existentes