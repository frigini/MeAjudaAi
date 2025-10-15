# Estratégia de Dead Letter Queue (DLQ) - MeAjudaAi

## 📋 Visão Geral

Este documento descreve a estratégia completa de Dead Letter Queue implementada no sistema MeAjudaAi para garantir robustez e resiliência no processamento de mensagens.

## 🎯 Objetivos

- **Resiliência**: Garantir que falhas temporárias não resultem em perda de mensagens
- **Observabilidade**: Fornecer visibilidade completa sobre falhas de processamento
- **Recuperação**: Permitir reprocessamento de mensagens falhadas
- **Escalabilidade**: Suportar diferentes estratégias por ambiente (RabbitMQ/Service Bus)

## 🏗️ Arquitetura

### Componentes Principais

```
┌─────────────────────┐    ┌──────────────────────┐    ┌─────────────────────┐
│   Message Handler   │───▶│ MessageRetryMiddleware│───▶│  IDeadLetterService │
└─────────────────────┘    └──────────────────────┘    └─────────────────────┘
                                       │                            │
                                       ▼                            ▼
                           ┌──────────────────────┐    ┌─────────────────────┐
                           │   Retry Logic +      │    │    Dead Letter      │
                           │   Exponential        │    │      Queue          │
                           │     Backoff          │    │   (RabbitMQ/SB)     │
                           └──────────────────────┘    └─────────────────────┘
```

### Interfaces Core

#### `IDeadLetterService`
Interface principal para gerenciamento de Dead Letter Queue

```csharp
public interface IDeadLetterService
{
    Task SendToDeadLetterAsync<TMessage>(TMessage message, Exception exception, 
        string handlerType, string sourceQueue, int attemptCount, CancellationToken cancellationToken = default);
    
    bool ShouldRetry(Exception exception, int attemptCount);
    TimeSpan CalculateRetryDelay(int attemptCount);
    Task ReprocessDeadLetterMessageAsync(string deadLetterQueueName, string messageId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FailedMessageInfo>> ListDeadLetterMessagesAsync(string deadLetterQueueName, int maxCount = 50, CancellationToken cancellationToken = default);
}
```

## 🔧 Implementações

### 1. RabbitMQ Dead Letter Service
**Ambiente**: Development/Testing

**Características**:
- Dead Letter Exchange (DLX) automático
- TTL configurável para mensagens na DLQ
- Roteamento baseado em routing keys
- Persistência opcional

**Configuração**:
```json
{
  "Messaging": {
    "DeadLetter": {
      "RabbitMq": {
        "DeadLetterExchange": "dlx.meajudaai",
        "DeadLetterRoutingKey": "deadletter",
        "EnableAutomaticDlx": true,
        "EnablePersistence": true
      }
    }
  }
}
```

### 2. Service Bus Dead Letter Service
**Ambiente**: Production

**Características**:
- Dead Letter Queue nativo do Azure Service Bus
- Auto-complete configurável
- Lock duration ajustável
- Integração com Service Bus Management API

**Configuração**:
```json
{
  "Messaging": {
    "DeadLetter": {
      "ServiceBus": {
        "DeadLetterQueueSuffix": "$DeadLetterQueue",
        "EnableAutoComplete": true,
        "MaxLockDurationMinutes": 5
      }
    }
  }
}
```

## 🔁 Estratégia de Retry

### Políticas de Retry

#### 1. **Falhas Permanentes** (Não Retry)
```csharp
string[] permanentExceptions = {
    "System.ArgumentException",
    "System.ArgumentNullException", 
    "System.FormatException",
    "MeAjudaAi.Shared.Exceptions.BusinessRuleException",
    "MeAjudaAi.Shared.Exceptions.DomainException"
};
```
- **Ação**: Envio imediato para DLQ
- **Justificativa**: Erros de lógica/validação que não serão resolvidos com retry

#### 2. **Falhas Temporárias** (Retry Recomendado)
```csharp
string[] transientExceptions = {
    "System.TimeoutException",
    "System.Net.Http.HttpRequestException",
    "Npgsql.PostgresException",
    "System.Net.Sockets.SocketException"
};
```
- **Ação**: Retry com backoff exponencial
- **Justificativa**: Problemas de rede/infraestrutura que podem ser resolvidos

#### 3. **Falhas Críticas** (Não Retry)
```csharp
if (exception is OutOfMemoryException or StackOverflowException)
    return FailureType.Critical;
```
- **Ação**: Envio imediato para DLQ + notificação de admin
- **Justificativa**: Problemas sistêmicos que requerem intervenção

### Backoff Exponencial

```csharp
public TimeSpan CalculateRetryDelay(int attemptCount)
{
    var baseDelay = TimeSpan.FromSeconds(InitialRetryDelaySeconds);
    var exponentialDelay = TimeSpan.FromSeconds(
        baseDelay.TotalSeconds * Math.Pow(BackoffMultiplier, attemptCount - 1));
    var maxDelay = TimeSpan.FromSeconds(MaxRetryDelaySeconds);
    
    return exponentialDelay > maxDelay ? maxDelay : exponentialDelay;
}
```

**Exemplo de Delays**:
- Tentativa 1: 5 segundos
- Tentativa 2: 10 segundos  
- Tentativa 3: 20 segundos
- Máximo: 300 segundos (5 minutos)

## 🔌 Integração com Handlers

### Uso Automático via Middleware

```csharp
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // O MessageRetryMiddleware intercepta automaticamente falhas
        // e aplica a estratégia de retry/DLQ
        
        await ProcessUserCreation(@event, cancellationToken);
    }
}
```

### Uso Manual com Extensões

```csharp
public async Task ProcessMessage<TMessage>(TMessage message, string sourceQueue)
{
    var success = await message.ExecuteWithRetryAsync(
        handler: async (msg, ct) => await ProcessMessageLogic(msg, ct),
        serviceProvider: _serviceProvider,
        sourceQueue: sourceQueue);
    
    if (!success)
    {
        _logger.LogWarning("Message sent to dead letter queue");
    }
}
```

## 📊 Monitoramento e Observabilidade

### Informações Capturadas

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
```

### Estatísticas Disponíveis

```csharp
public sealed class DeadLetterStatistics
{
    public int TotalDeadLetterMessages { get; set; }
    public Dictionary<string, int> MessagesByQueue { get; set; }
    public Dictionary<string, int> MessagesByExceptionType { get; set; }
    public Dictionary<string, FailureRate> FailureRateByHandler { get; set; }
}
```

### Logs Estruturados

```csharp
_logger.LogWarning(
    "Message sent to dead letter queue. MessageId: {MessageId}, Type: {MessageType}, Queue: {Queue}, Attempts: {Attempts}, Reason: {Reason}",
    failedMessageInfo.MessageId, typeof(TMessage).Name, deadLetterQueueName, attemptCount, exception.Message);
```

## 🚀 Setup e Configuração

### 1. Configuração no DI Container

```csharp
// Program.cs ou Extensions
services.AddMessaging(configuration, environment);
// DLQ é automaticamente configurado via AddMessaging

// Ou configuração específica
services.AddDeadLetterQueue(configuration, environment, options =>
{
    options.ConfigureForDevelopment(); // ou ConfigureForProduction()
});
```

### 2. Configuração de Ambiente

#### Development (appsettings.Development.json)
```json
{
  "Messaging": {
    "DeadLetter": {
      "Enabled": true,
      "MaxRetryAttempts": 3,
      "InitialRetryDelaySeconds": 2,
      "BackoffMultiplier": 2.0,
      "MaxRetryDelaySeconds": 60,
      "DeadLetterTtlHours": 24,
      "EnableDetailedLogging": true,
      "EnableAdminNotifications": false
    }
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "Messaging": {
    "DeadLetter": {
      "Enabled": true,
      "MaxRetryAttempts": 5,
      "InitialRetryDelaySeconds": 5,
      "BackoffMultiplier": 2.0,
      "MaxRetryDelaySeconds": 300,
      "DeadLetterTtlHours": 72,
      "EnableDetailedLogging": false,
      "EnableAdminNotifications": true
    }
  }
}
```

### 3. Inicialização da Infraestrutura

```csharp
// Program.cs
var app = builder.Build();

// Garantir infraestrutura de messaging (inclui DLQ)
await app.EnsureMessagingInfrastructureAsync();

await app.RunAsync();
```

## 🔄 Operações de DLQ

### 1. Listar Mensagens na DLQ

```csharp
var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();
var messages = await deadLetterService.ListDeadLetterMessagesAsync("dlq.users-events", maxCount: 10);

foreach (var message in messages)
{
    Console.WriteLine($"Message {message.MessageId}: {message.LastFailureReason}");
}
```

### 2. Reprocessar Mensagem

```csharp
await deadLetterService.ReprocessDeadLetterMessageAsync("dlq.users-events", messageId);
```

### 3. Purgar Mensagem (Após Análise)

```csharp
await deadLetterService.PurgeDeadLetterMessageAsync("dlq.users-events", messageId);
```

### 4. Obter Estatísticas

```csharp
var statistics = await deadLetterService.GetDeadLetterStatisticsAsync();
Console.WriteLine($"Total messages in DLQ: {statistics.TotalDeadLetterMessages}");
```

## 🧪 Testes

### Testes Unitários

```csharp
[Fact]
public async Task SendToDeadLetterAsync_WithPermanentException_ShouldSendImmediately()
{
    // Arrange
    var exception = new ArgumentException("Invalid argument");
    var message = new TestMessage();
    
    // Act
    await _deadLetterService.SendToDeadLetterAsync(message, exception, "TestHandler", "test-queue", 1);
    
    // Assert
    _mockLogger.Verify(/* verificar log de DLQ */);
}

[Theory]
[InlineData(typeof(TimeoutException), 1, true)]
[InlineData(typeof(ArgumentException), 1, false)]
[InlineData(typeof(TimeoutException), 5, false)]
public void ShouldRetry_WithDifferentExceptions_ReturnsExpectedResult(Type exceptionType, int attemptCount, bool expected)
{
    // Arrange
    var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;
    
    // Act
    var result = _deadLetterService.ShouldRetry(exception, attemptCount);
    
    // Assert
    result.Should().Be(expected);
}
```

### Testes de Integração

```csharp
[Fact]
public async Task MessageRetryMiddleware_WithTransientFailure_ShouldRetryAndSucceed()
{
    // Arrange
    var message = new TestMessage();
    var callCount = 0;
    
    Task TestHandler(TestMessage msg, CancellationToken ct)
    {
        callCount++;
        if (callCount < 3)
            throw new TimeoutException("Temporary failure");
        return Task.CompletedTask;
    }
    
    // Act
    var success = await message.ExecuteWithRetryAsync(TestHandler, _serviceProvider, "test-queue");
    
    // Assert
    success.Should().BeTrue();
    callCount.Should().Be(3);
}
```

## 📈 Métricas e Alertas

### Métricas Recomendadas

1. **Taxa de Falha por Handler**
   - `dlq_failure_rate_by_handler{handler_type="UserCreatedEventHandler"}`

2. **Mensagens na DLQ por Fila**
   - `dlq_message_count{queue="users-events"}`

3. **Tempo Médio até DLQ**
   - `dlq_time_to_failure_seconds{message_type="UserCreatedEvent"}`

4. **Volume de Reprocessamento**
   - `dlq_reprocess_count{queue="users-events"}`

### Alertas Sugeridos

1. **DLQ Growth Alert**
   ```
   dlq_message_count > 100
   ```

2. **High Failure Rate Alert**
   ```
   dlq_failure_rate_by_handler > 0.1 (10%)
   ```

3. **Old Messages Alert**
   ```
   dlq_oldest_message_age_hours > 24
   ```

## 🔐 Segurança

### Informações Sensíveis

- **Não** incluir dados sensíveis no `OriginalMessage`
- **Mascarar** informações PII nos logs
- **Criptografar** mensagens na DLQ se necessário

### Acesso à DLQ

- Restringir acesso de leitura/reprocessamento a administradores
- Auditar operações de reprocessamento
- Implementar políticas de retenção

## 📚 Referências

- [RabbitMQ Dead Letter Exchange](https://www.rabbitmq.com/dlx.html)
- [Azure Service Bus Dead Letter Queue](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues)
- [Retry Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Circuit Breaker Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)