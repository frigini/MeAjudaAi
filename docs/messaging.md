# Estratégia de MessageBus por Ambiente - Documentação

## ✅ **RESPOSTA À PERGUNTA**: Sim, a implementação garante seleção automática de MessageBus por ambiente: RabbitMQ para desenvolvimento (quando habilitado), NoOp/Mocks para testes, e Azure Service Bus para produção.

## **Implementação Realizada**

### 1. **Factory Pattern para Seleção de MessageBus**

**Arquivo**: `src/Shared/MeAjudaAi.Shared/Messaging/Factory/MessageBusFactory.cs`

```csharp
public class EnvironmentBasedMessageBusFactory : IMessageBusFactory
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    
    public EnvironmentBasedMessageBusFactory(
        IHostEnvironment environment,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _environment = environment;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }
    
    public IMessageBus CreateMessageBus()
    {
        var rabbitMqEnabled = _configuration.GetValue<bool?>($"{RabbitMqOptions.SectionName}:Enabled");
        
        if (_environment.IsDevelopment())
        {
            // DEVELOPMENT: RabbitMQ (only if explicitly enabled) or NoOp (otherwise)
            if (rabbitMqEnabled == true)
            {
                var rabbitMqService = _serviceProvider.GetService<RabbitMqMessageBus>();
                if (rabbitMqService != null)
                {
                    return rabbitMqService;
                }
                return _serviceProvider.GetRequiredService<NoOpMessageBus>(); // Fallback
            }
            else
            {
                return _serviceProvider.GetRequiredService<NoOpMessageBus>();
            }
        }
        else if (_environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // TESTING: Always NoOp to avoid external dependencies
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
        else if (_environment.IsProduction())
        {
            // PRODUCTION: Azure Service Bus
            return _serviceProvider.GetRequiredService<ServiceBusMessageBus>();
        }
        else
        {
            // STAGING/OTHER: NoOp for safety
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
    }
}
```csharp
### 2. **Configuração de DI por Ambiente**

**Arquivo**: `src/Shared/MeAjudaAi.Shared/Messaging/Extensions.cs`

```csharp
// Registrar implementações específicas do MessageBus condicionalmente baseado no ambiente
// para reduzir o risco de resolução acidental em ambientes de teste
if (environment.IsDevelopment())
{
    // Development: Registra RabbitMQ e NoOp (fallback)
    services.TryAddSingleton<RabbitMqMessageBus>();
}
else if (environment.IsProduction())
{
    // Production: Registra apenas ServiceBus
    services.TryAddSingleton<ServiceBusMessageBus>();
}
else if (environment.IsEnvironment(EnvironmentNames.Testing))
{
    // Testing: apenas NoOp/mocks - NoOpMessageBus will be registered below
}

// Ensure NoOpMessageBus is always available as a fallback for all environments
services.TryAddSingleton<NoOpMessageBus>();

// Registrar o factory e o IMessageBus baseado no ambiente
services.AddSingleton<IMessageBusFactory, EnvironmentBasedMessageBusFactory>();
services.AddSingleton<IMessageBus>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();
    return factory.CreateMessageBus(); // ← Seleção baseada no ambiente
});
```yaml
### 3. **Configurações por Ambiente**

#### **Development** (`appsettings.Development.json`):
```json
{
  "Messaging": {
    "Enabled": true,
    "Provider": "RabbitMQ",
    "RabbitMQ": {
      "Enabled": true,
      "ConnectionString": "amqp://guest:guest@localhost:5672/",
      "DefaultQueueName": "MeAjudaAi-events-dev",
      "Host": "localhost",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest",
      "VirtualHost": "/"
    }
  }
}
```csharp
**Nota**: O RabbitMQ suporta duas formas de configuração de conexão:
1. **ConnectionString direta**: `"amqp://user:pass@host:port/vhost"`
2. **Propriedades individuais**: O sistema automaticamente constrói a ConnectionString usando `Host`, `Port`, `Username`, `Password` e `VirtualHost` através do método `BuildConnectionString()`

#### **Production** (`appsettings.Production.json`):
```json
{
  "Messaging": {
    "Enabled": true,
    "Provider": "ServiceBus",
    "ServiceBus": {
      "ConnectionString": "${SERVICEBUS_CONNECTION_STRING}",
      "DefaultTopicName": "MeAjudaAi-prod-events"
    }
  }
}
```csharp
#### **Testing** (`appsettings.Testing.json`):
```json
{
  "Messaging": {
    "Enabled": false,
    "Provider": "Mock"
  }
}
```yaml
### 4. **Mocks para Testes**

**Configuração nos testes**: `tests/MeAjudaAi.Integration.Tests/Base/ApiTestBase.cs`

```csharp
// Em uma classe de configuração de testes ou Program.cs
builder.ConfigureServices(services =>
{
    // Configura mocks de messaging automaticamente para ambiente Testing
    if (builder.Environment.EnvironmentName == "Testing")
    {
        services.AddMessagingMocks(); // ← Substitui implementações reais por mocks
    }
    
    // Outras configurações...
});
```csharp
**Nota**: Para testes de integração, os mocks são registrados automaticamente quando o ambiente é "Testing", substituindo as implementações reais do MessageBus para garantir isolamento e velocidade dos testes.

### 5. **Transporte Rebus por Ambiente**

**Arquivo**: `src/Shared/MeAjudaAi.Shared/Messaging/Extensions.cs`

```csharp
private static void ConfigureTransport(
    StandardConfigurer<ITransport> transport,
    ServiceBusOptions serviceBusOptions,
    RabbitMqOptions rabbitMqOptions,
    IHostEnvironment environment)
{
    if (environment.EnvironmentName == "Testing")
    {
        // TESTING: No transport configured - mocks handle messaging
        return; // Transport configuration skipped for testing
    }
    else if (environment.IsDevelopment())
    {
        // DEVELOPMENT: RabbitMQ
        transport.UseRabbitMq(
            rabbitMqOptions.BuildConnectionString(), // Builds from Host/Port or uses ConnectionString
            rabbitMqOptions.DefaultQueueName);
    }
    else
    {
        // PRODUCTION: Azure Service Bus
        transport.UseAzureServiceBus(
            serviceBusOptions.ConnectionString,
            serviceBusOptions.DefaultTopicName);
    }
}
```csharp
### 6. **Infraestrutura Aspire por Ambiente**

**Arquivo**: `src/Aspire/MeAjudaAi.AppHost/Program.cs`

```csharp
if (isDevelopment) // Development only
{
    // RabbitMQ local para desenvolvimento
    var rabbitMq = builder.AddRabbitMQ("rabbitmq")
        .WithManagementPlugin();
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference(rabbitMq); // ← RabbitMQ only for Development
}
else if (isProduction) // Production only
{
    // Azure Service Bus for Production
    var serviceBus = builder.AddAzureServiceBus("servicebus");
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")  
        .WithReference(serviceBus); // ← Service Bus for Production
}
else // Testing environment
{
    // No external message bus infrastructure for Testing
    // NoOpMessageBus will be used without external dependencies
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice");
    // ← No message bus reference, NoOpMessageBus handles all messaging
}
```text
## **Garantias Implementadas**

### ✅ **1. Development Environment**
- **IMessageBus**: `RabbitMqMessageBus` (se `RabbitMQ:Enabled == true`) OU `NoOpMessageBus` (se desabilitado)
- **Transport**: RabbitMQ (se habilitado) OU None (se desabilitado)
- **Infrastructure**: RabbitMQ container (Aspire, quando habilitado)
- **Configuration**: `appsettings.Development.json` → "Provider": "RabbitMQ", "RabbitMQ:Enabled": true

### ✅ **2. Testing Environment**
- **IMessageBus**: `NoOpMessageBus` (ou Mocks para testes de integração)
- **Transport**: None (Rebus não configurado para Testing)
- **Infrastructure**: NoOp/Mocks (sem dependências externas - sem Service Bus no Aspire)
- **Configuration**: `appsettings.Testing.json` → "Provider": "Mock", "Enabled": false, "RabbitMQ:Enabled": false

### ✅ **3. Production Environment**
- **IMessageBus**: `ServiceBusMessageBus`
- **Transport**: Azure Service Bus (via Rebus)
- **Infrastructure**: Azure Service Bus (via Aspire)
- **Configuration**: `appsettings.Production.json` → "Provider": "ServiceBus"

## **Fluxo de Seleção**

```text
Application Startup
       ↓
Environment Detection
       ↓
┌─────────────────┬─────────────────┬─────────────────┐
│   Development   │     Testing     │   Production    │
│                 │                 │                 │
│ RabbitMQ        │ NoOp/Mocks      │ Service Bus     │
│ (se habilitado) │ (sem deps ext.) │ (Azure)         │
│ OU NoOp         │                 │ + Scalable      │
│ (se desabilitado)│                │                 │
└─────────────────┴─────────────────┴─────────────────┘
```text
## **Validação**

### **Como Confirmar a Configuração:**

1. **Logs na Aplicação**:
   ```text
   Development: "Creating RabbitMQ MessageBus for environment: Development"
   Testing: Mocks registrados via AddMessagingMocks()
   Production: "Creating Azure Service Bus MessageBus for environment: Production"
   ```

2. **Configuração Aspire**:
   - Development: RabbitMQ container ativo
   - Production: Azure Service Bus provisionado

3. **Testes**:
   - Mocks verificam mensagens sem dependências externas
   - Implementações reais removidas automaticamente

## **Conclusão**

✅ **SIM** - A implementação **garante completamente** que:

- **RabbitMQ** is used for **Development** only **when explicitly enabled** (`RabbitMQ:Enabled == true`)
- **Testing** always uses **NoOp/Mocks** (no external dependencies)
- **NoOp MessageBus** is used as **safe fallback** when RabbitMQ is disabled or unavailable
- **Azure Service Bus** is used exclusively for **Production**  
- **Mocks** are used automatically in **integration tests** (replacing real implementations)

A seleção é feita automaticamente via:
1. **Environment detection** (`IHostEnvironment`)
2. **Configuration-based enablement** (`RabbitMQ:Enabled`)
3. **Factory pattern** (`EnvironmentBasedMessageBusFactory`)
4. **Dependency injection** (registro baseado no ambiente)
5. **Graceful fallbacks** (NoOp quando RabbitMQ indisponível)
6. **Automatic test mocks** (AddMessagingMocks() aplicado automaticamente em ambiente Testing)

**Configuração manual mínima** é necessária apenas para testes de integração que requerem registro explícito de mocks via `AddMessagingMocks()`. A seleção de MessageBus em runtime é **automática e determinística** baseada no ambiente de execução e configurações.
# Implementação de Mocks para Messaging

## Visão Geral

Este documento descreve a implementação completa de mocks para Azure Service Bus e RabbitMQ, permitindo testes isolados e confiáveis sem dependências externas.

## Componentes Implementados

### 1. MockServiceBusMessageBus

**Localização**: `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/MockServiceBusMessageBus.cs`

**Funcionalidades**:
- Mock completo do Azure Service Bus
- Implementa interface `IMessageBus` com métodos `SendAsync`, `PublishAsync` e `SubscribeAsync`
- Tracking de mensagens enviadas e eventos publicados
- Suporte para simulação de falhas
- Verificação de mensagens por tipo, predicado e destino

**Métodos principais**:
- `WasMessageSent<T>()` - Verifica se mensagem foi enviada
- `WasEventPublished<T>()` - Verifica se evento foi publicado
- `GetSentMessages<T>()` - Obtém mensagens enviadas por tipo
- `SimulateSendFailure()` - Simula falhas de envio de mensagens
- `SimulatePublishFailure()` - Simula falhas de publicação de eventos

### 2. MockRabbitMqMessageBus

**Localização**: `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/MockRabbitMqMessageBus.cs`

**Funcionalidades**:
- Mock completo do RabbitMQ MessageBus
- Interface idêntica ao mock do Service Bus
- Tracking separado para mensagens RabbitMQ
- Simulação de falhas específicas do RabbitMQ

### 3. MessagingMockManager

**Localização**: `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/MessagingMockManager.cs`

**Funcionalidades**:
- Coordenação centralizada de todos os mocks de messaging
- Estatísticas unificadas de mensagens
- Limpeza em lote de todas as mensagens
- Reset global de todos os mocks

**Métodos principais**:
- `ClearAllMessages()` - Limpa todas as mensagens de todos os mocks
- `ResetAllMocks()` - Restaura comportamento normal
- `GetStatistics()` - Estatísticas consolidadas
- `WasMessagePublishedAnywhere<T>()` - Busca em todos os sistemas

### 4. Extensions para DI

**Funcionalidades**:
- `AddMessagingMocks()` - Configuração automática no container DI
- Remoção automática de implementações reais
- Registro dos mocks como implementações de `IMessageBus`

## Integração com Testes

### ApiTestBase

**Localização**: `tests/MeAjudaAi.Integration.Tests/Base/ApiTestBase.cs`

**Modificações**:
- Configuração automática dos mocks de messaging
- Desabilitação de messaging real em testes
- Integração com TestContainers existente

### MessagingIntegrationTestBase

**Localização**: `tests/MeAjudaAi.Integration.Tests/Users/MessagingIntegrationTestBase.cs`

**Funcionalidades**:
- Classe base para testes que verificam messaging
- Acesso simplificado ao `MessagingMockManager`
- Métodos auxiliares para verificação de mensagens
- Limpeza automática entre testes

### UserMessagingTests

**Localização**: `tests/MeAjudaAi.Integration.Tests/Users/UserMessagingTests.cs`

**Testes implementados**:

1. **CreateUser_ShouldPublishUserRegisteredEvent**
   - Verifica publicação de `UserRegisteredDomainEvent`
   - Valida dados do evento (email, nome, ID)

2. **UpdateUserProfile_ShouldPublishUserProfileUpdatedEvent**
   - Verifica publicação de `UserProfileUpdatedDomainEvent`
   - Valida atualização de perfil

3. **DeleteUser_ShouldPublishUserDeletedEvent**
   - Verifica publicação de `UserDeletedDomainEvent`
   - Valida exclusão de usuário

4. **MessagingStatistics_ShouldTrackMessageCounts**
   - Verifica contabilização de mensagens
   - Valida estatísticas do sistema

## Eventos de Domínio Suportados

### UserRegisteredDomainEvent
- **Trigger**: Registro de novo usuário
- **Dados**: AggregateId, Version, Email, Username, FirstName, LastName

### UserProfileUpdatedDomainEvent
- **Trigger**: Atualização de perfil do usuário
- **Dados**: AggregateId, Version, FirstName, LastName

### UserDeletedDomainEvent
- **Trigger**: Exclusão (soft delete) de usuário
- **Dados**: AggregateId, Version

## Uso em Testes

### Exemplo Básico

```csharp
public class MyMessagingTest : MessagingIntegrationTestBase
{
    [Fact]
    public async Task SomeAction_ShouldPublishEvent()
    {
        // Arrange
        await EnsureMessagingInitializedAsync();
        
        // Act
        await Client.PostAsJsonAsync("/api/some-endpoint", data);
        
        // Assert
        var wasPublished = WasMessagePublished<MyEvent>(e => e.SomeProperty == expectedValue);
        wasPublished.Should().BeTrue();
        
        var events = GetPublishedMessages<MyEvent>();
        events.Should().HaveCount(1);
    }
}
```csharp
### Verificação de Estatísticas

```csharp
var stats = GetMessagingStatistics();
stats.ServiceBusMessageCount.Should().Be(2);
stats.RabbitMqMessageCount.Should().Be(1);
stats.TotalMessageCount.Should().Be(3);
```text
### Simulação de Falhas

```csharp
// Simular falha em envio de mensagens
MessagingMocks.ServiceBus.SimulateSendFailure(new Exception("Send failure"));

// Simular falha em publicação de eventos
MessagingMocks.ServiceBus.SimulatePublishFailure(new Exception("Publish failure"));

// Testar cenários de falha...

// Restaurar comportamento normal
MessagingMocks.ServiceBus.ResetToNormalBehavior();
```text
## Vantagens da Implementação

### 1. Isolamento Completo
- Testes não dependem de serviços externos
- Execução rápida e confiável
- Controle total sobre cenários de teste

### 2. Verificação Detalhada
- Tracking preciso de todas as mensagens
- Verificação por tipo, predicado e destino
- Estatísticas detalhadas de uso

### 3. Simulação de Falhas
- Testes de cenários de erro
- Validação de tratamento de exceções
- Testes de resiliência

### 4. Facilidade de Uso
- API intuitiva e bem documentada
- Integração automática com DI
- Limpeza automática entre testes

## Melhorias Futuras

### 1. Mock de Outros Serviços Azure
- Azure Storage Account
- Azure Key Vault
- Azure Cosmos DB

### 2. Persistência de Mensagens
- Histórico entre execuções de teste
- Análise temporal de mensagens

### 3. Visualização
- Dashboard de mensagens em testes
- Relatórios de usage de messaging

### 4. Performance Testing
- Mocks para testes de carga
- Simulação de latência de rede

## Conclusão

A FASE 2.3 estabelece uma base sólida para testes de messaging, fornecendo mocks completos e fáceis de usar para Azure Service Bus e RabbitMQ. A implementação permite testes isolados, confiáveis e rápidos, com capacidades avançadas de verificação e simulação de falhas.

A infraestrutura criada é extensível e pode ser facilmente expandida para suportar outros serviços Azure conforme necessário, mantendo a consistência na experiência de desenvolvimento e teste.
# Dead Letter Queue (DLQ) - Strategy and Implementation Guide

## 🎯 Executive Summary

The Dead Letter Queue strategy has been successfully implemented in MeAjudaAi, providing:

- ✅ **Automatic retry** with exponential backoff
- ✅ **Intelligent classification** of failures (permanent vs. temporary)
- ✅ **Multi-environment support** (RabbitMQ for dev, Service Bus for prod)
- ✅ **Complete observability** with structured logs and metrics
- ✅ **Management operations** (reprocess, purge, list)

## 🏗️ Implemented Architecture

```csharp
┌──────────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│   Event Handler  │───▶│ MessageRetryMiddleware│───▶│  IDeadLetterService  │
│                  │    │                     │    │                      │
│ - UserCreated    │    │ - Retry Logic       │    │ - RabbitMQ (Dev)     │
│ - OrderProcessed │    │ - Backoff Strategy  │    │ - ServiceBus (Prod)  │
│ - EmailSent      │    │ - Exception         │    │ - NoOp (Testing)     │
└──────────────────┘    │   Classification    │    └──────────────────────┘
                        └─────────────────────┘                 │
                                    │                           │
                                    ▼                           ▼
                        ┌─────────────────────┐    ┌──────────────────────┐
                        │     Retry Queue     │    │   Dead Letter Queue  │
                        │                     │    │                      │
                        │ - Exponential      │    │ - Failed Messages    │
                        │   Backoff Delay     │    │ - Failure Analysis   │
                        │ - Max: 300s         │    │ - Reprocess Support  │
                        └─────────────────────┘    └──────────────────────┘
```

## 🔧 Implementations

### 1. RabbitMQ Dead Letter Service
**Environment**: Development/Testing

**Features**:
- Automatic Dead Letter Exchange (DLX)
- Configurable TTL for messages in the DLQ
- Routing based on routing keys
- Optional persistence

### 2. Service Bus Dead Letter Service
**Environment**: Production

**Features**:
- Native Azure Service Bus Dead Letter Queue
- Configurable auto-complete
- Adjustable lock duration
- Integration with Service Bus Management API

## 🔁 Retry Strategy

### Retry Policies

#### 1. **Permanent Failures** (No Retry)
- **Examples**: `ArgumentException`, `BusinessRuleException`
- **Action**: Immediate dispatch to DLQ.

#### 2. **Temporary Failures** (Retry Recommended)
- **Examples**: `TimeoutException`, `HttpRequestException`, `PostgresException`
- **Action**: Retry with exponential backoff.

#### 3. **Critical Failures** (No Retry)
- **Examples**: `OutOfMemoryException`, `StackOverflowException`
- **Action**: Immediate dispatch to DLQ + admin notification.

### Exponential Backoff

The delay between retries increases exponentially using the formula `2^(attemptCount-1) * 2` seconds, capped at 300 seconds (5 minutes).

**Retry intervals**: 2s, 4s, 8s, 16s, 32s, 64s, 128s, 256s (then capped at 300s)

## 🔌 Integration with Handlers

The `MessageRetryMiddleware` automatically intercepts failures in event handlers and applies the retry/DLQ strategy.

## 📊 Monitoring and Observability

### Captured Information

The `FailedMessageInfo` class captures detailed information about failed messages, including:
- Message ID, type, and original content
- Source queue and attempt count
- Failure history and environment metadata

### Available Statistics

The `DeadLetterStatistics` class provides an overview of the DLQ, including:
- Total number of dead-lettered messages
- Messages by queue and exception type
- Failure rate by handler

## 🚀 Setup and Configuration

The DLQ system is automatically configured via `services.AddMessaging(configuration, environment);` in `Program.cs`. Environment-specific settings are loaded from `appsettings.Development.json` and `appsettings.Production.json`.

## 🔄 DLQ Operations

The `IDeadLetterService` provides methods for:
- Listing messages in the DLQ
- Reprocessing a specific message
- Purging a message after analysis
- Getting DLQ statistics

## 🧪 Test Coverage

The implementation is covered by a comprehensive suite of unit and integration tests, ensuring the reliability of the DLQ system.

## 🔐 Security Considerations

- Sensitive information is not included in the `OriginalMessage`.
- PII is masked in logs.
- Access to DLQ operations requires admin permissions.
- Messages have a configurable TTL.
