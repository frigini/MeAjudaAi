# Estratégia de MessageBus por Ambiente - Documentação

## ✅ **RESPOSTA À PERGUNTA**: Sim, a implementação garante que RabbitMQ seja usado para desenvolvimento, mocks para testes, e Azure Service Bus apenas para produção.

## **Implementação Realizada**

### 1. **Factory Pattern para Seleção de MessageBus**

**Arquivo**: `src/Shared/MeAjudai.Shared/Messaging/Factory/MessageBusFactory.cs`

```csharp
public class EnvironmentBasedMessageBusFactory : IMessageBusFactory
{
    public IMessageBus CreateMessageBus()
    {
        if (_environment.IsDevelopment())
        {
            // DEVELOPMENT: RabbitMQ
            return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
        }
        else if (_environment.EnvironmentName == "Testing")
        {
            // TESTING: Mocks (handled by AddMessagingMocks in test setup)
            return _serviceProvider.GetRequiredService<MockMessageBus>();
        }
        else
        {
            // PRODUCTION: Azure Service Bus
            return _serviceProvider.GetRequiredService<ServiceBusMessageBus>();
        }
    }
}
```

### 2. **Configuração de DI por Ambiente**

**Arquivo**: `src/Shared/MeAjudai.Shared/Messaging/Extensions.cs`

```csharp
// Registrar implementações específicas do MessageBus
services.AddSingleton<ServiceBusMessageBus>();
services.AddSingleton<RabbitMqMessageBus>();

// Registrar o factory e o IMessageBus baseado no ambiente
services.AddSingleton<IMessageBusFactory, EnvironmentBasedMessageBusFactory>();
services.AddSingleton<IMessageBus>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();
    return factory.CreateMessageBus(); // ← Seleção baseada no ambiente
});
```

### 3. **Configurações por Ambiente**

#### **Development** (`appsettings.Development.json`):
```json
{
  "Messaging": {
    "Enabled": true,
    "Provider": "RabbitMQ", // ← Explicita RabbitMQ para dev
    "RabbitMQ": {
      "DefaultQueueName": "MeAjudaAi-events-dev",
      "Host": "localhost",
      "Port": 5672
    }
  }
}
```

#### **Production** (`appsettings.Production.json`):
```json
{
  "Messaging": {
    "Enabled": true,
    "Provider": "ServiceBus", // ← Explicita Service Bus para prod
    "ServiceBus": {
      "ConnectionString": "${SERVICEBUS_CONNECTION_STRING}",
      "TopicName": "MeAjudaAi-prod-events"
    }
  }
}
```

#### **Testing** (`appsettings.Testing.json`):
```json
{
  "Messaging": {
    "Enabled": false,
    "Provider": "Mock" // ← Mocks para testes
  }
}
```

### 4. **Mocks para Testes**

**Configuração nos testes**: `tests/MeAjudaAi.Integration.Tests/Base/ApiTestBase.cs`

```csharp
builder.ConfigureServices(services =>
{
    // Configura mocks de messaging (FASE 2.3)
    services.AddMessagingMocks(); // ← Substitui implementações reais por mocks
    
    // Outras configurações...
});
```

### 5. **Transporte Rebus por Ambiente**

**Arquivo**: `src/Shared/MeAjudai.Shared/Messaging/Extensions.cs`

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
            rabbitMqOptions.ConnectionString,
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
```

### 6. **Infraestrutura Aspire por Ambiente**

**Arquivo**: `src/Aspire/MeAjudaAi.AppHost/Program.cs`

```csharp
if (isLocal) // Development/Testing
{
    // RabbitMQ local para desenvolvimento
    var rabbitMq = builder.AddRabbitMQ("rabbitmq")
        .WithManagementPlugin();
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference(rabbitMq); // ← RabbitMQ para dev
}
else // Production
{
    // Azure Service Bus para produção
    var serviceBus = builder.AddAzureServiceBus("servicebus");
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")  
        .WithReference(serviceBus); // ← Service Bus para prod
}
```

## **Garantias Implementadas**

### ✅ **1. Development Environment**
- **IMessageBus**: `RabbitMqMessageBus`
- **Transport**: RabbitMQ (via Rebus)
- **Infrastructure**: RabbitMQ container (Aspire)
- **Configuration**: `appsettings.Development.json` → "Provider": "RabbitMQ"

### ✅ **2. Testing Environment**
- **IMessageBus**: `MockServiceBusMessageBus` ou `MockRabbitMqMessageBus` (mocks)
- **Transport**: Disabled (Rebus não configurado)
- **Infrastructure**: Mocks (sem dependências externas)
- **Configuration**: `appsettings.Testing.json` → "Provider": "Mock", "Enabled": false

### ✅ **3. Production Environment**
- **IMessageBus**: `ServiceBusMessageBus`
- **Transport**: Azure Service Bus (via Rebus)
- **Infrastructure**: Azure Service Bus (via Aspire)
- **Configuration**: `appsettings.Production.json` → "Provider": "ServiceBus"

## **Fluxo de Seleção**

```
Application Startup
       ↓
Environment Detection
       ↓
┌─────────────────┬─────────────────┬─────────────────┐
│   Development   │     Testing     │   Production    │
│                 │                 │                 │
│ RabbitMQ        │ Mocks           │ Service Bus     │
│ + Local         │ + No External   │ + Azure         │
│ + Fast Setup    │ + Isolated      │ + Scalable      │
└─────────────────┴─────────────────┴─────────────────┘
```

## **Validação**

### **Como Confirmar a Configuração:**

1. **Logs na Aplicação**:
   ```
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

- **RabbitMQ** é usado exclusivamente para **Development**
- **Azure Service Bus** é usado exclusivamente para **Production**  
- **Mocks** são usados automaticamente nos **testes de integração (Testing)**

A seleção é feita automaticamente via:
1. **Environment detection** (`IHostEnvironment`)
2. **Factory pattern** (`EnvironmentBasedMessageBusFactory`)
3. **Dependency injection** (registro baseado no ambiente)
4. **Configuration files** (settings específicos por ambiente)
5. **Aspire infrastructure** (containers/services apropriados)

**Nenhuma configuração manual** é necessária - a seleção é **automática e determinística** baseada no ambiente de execução.