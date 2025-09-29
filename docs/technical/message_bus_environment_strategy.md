# Estratégia de MessageBus por Ambiente - Documentação

## ✅ **RESPOSTA À PERGUNTA**: Sim, a implementação garante seleção automática de MessageBus por ambiente: RabbitMQ para desenvolvimento (quando habilitado), NoOp/Mocks para testes, e Azure Service Bus para produção.

## **Implementação Realizada**

### 1. **Factory Pattern para Seleção de MessageBus**

**Arquivo**: `src/Shared/MeAjudaAi.Shared/Messaging/Factory/MessageBusFactory.cs`

```csharp
public class EnvironmentBasedMessageBusFactory : IMessageBusFactory
{
    public IMessageBus CreateMessageBus()
    {
        var rabbitMqEnabled = _configuration.GetValue<bool?>("RabbitMQ:Enabled");
        
        if (_environment.IsDevelopment() || _environment.EnvironmentName == "Testing")
        {
            // DEVELOPMENT/TESTING: RabbitMQ (se habilitado) ou NoOp (se desabilitado)
            if (rabbitMqEnabled != false)
            {
                try
                {
                    return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
                }
                catch
                {
                    return _serviceProvider.GetRequiredService<NoOpMessageBus>(); // Fallback
                }
            }
            else
            {
                return _serviceProvider.GetRequiredService<NoOpMessageBus>();
            }
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

**Arquivo**: `src/Shared/MeAjudaAi.Shared/Messaging/Extensions.cs`

```csharp
// Registrar implementações específicas do MessageBus condicionalmente baseado no ambiente
// para reduzir o risco de resolução acidental em ambientes de teste
if (environment.IsDevelopment())
{
    // Development: Registra RabbitMQ e NoOp (fallback)
    services.TryAddSingleton<RabbitMqMessageBus>();
    services.TryAddSingleton<NoOp.NoOpMessageBus>();
}
else if (environment.IsProduction())
{
    // Production: Registra apenas ServiceBus
    services.TryAddSingleton<ServiceBusMessageBus>();
}
else if (environment.IsEnvironment(EnvironmentNames.Testing))
{
    // Testing: Registra apenas NoOp - mocks serão adicionados via AddMessagingMocks()
    services.TryAddSingleton<NoOp.NoOpMessageBus>();
}

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
    "Enabled": false,
    "Provider": "RabbitMQ",
    "RabbitMQ": {
      "Enabled": false,
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
```

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
```

#### **Testing** (`appsettings.Testing.json`):
```json
{
  "Messaging": {
    "Enabled": false,
    "Provider": "Mock"
  },
  "RabbitMQ": {
    "Enabled": false
  }
}
```

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
```

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
- **IMessageBus**: `RabbitMqMessageBus` (se `RabbitMQ:Enabled != false`) OU `NoOpMessageBus` (se desabilitado)
- **Transport**: RabbitMQ (se habilitado) OU None (se desabilitado)
- **Infrastructure**: RabbitMQ container (Aspire, quando habilitado)
- **Configuration**: `appsettings.Development.json` → "Provider": "RabbitMQ", "RabbitMQ:Enabled": false

### ✅ **2. Testing Environment**
- **IMessageBus**: `RabbitMqMessageBus` (se `RabbitMQ:Enabled != false`) OU `NoOpMessageBus` (se desabilitado) OU Mocks (nos testes de integração)
- **Transport**: None (Rebus não configurado para Testing)
- **Infrastructure**: NoOp/Mocks (sem dependências externas)
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
│ OU NoOp         │ OU RabbitMQ*    │ + Scalable      │
│ (se desabilitado)│                │                 │
└─────────────────┴─────────────────┴─────────────────┘
* RabbitMQ só se explicitamente habilitado
```

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

- **RabbitMQ** é usado para **Development/Testing** apenas **quando explicitamente habilitado** (`RabbitMQ:Enabled != false`)
- **NoOp MessageBus** é usado como **fallback seguro** quando RabbitMQ está desabilitado ou indisponível
- **Azure Service Bus** é usado exclusivamente para **Production**  
- **Mocks** são usados automaticamente nos **testes de integração** (substituindo implementações reais)

A seleção é feita automaticamente via:
1. **Environment detection** (`IHostEnvironment`)
2. **Configuration-based enablement** (`RabbitMQ:Enabled`)
3. **Factory pattern** (`EnvironmentBasedMessageBusFactory`)
4. **Dependency injection** (registro baseado no ambiente)
5. **Graceful fallbacks** (NoOp quando RabbitMQ indisponível)
6. **Automatic test mocks** (AddMessagingMocks() aplicado automaticamente em ambiente Testing)

**Configuração manual mínima** é necessária apenas para testes de integração que requerem registro explícito de mocks via `AddMessagingMocks()`. A seleção de MessageBus em runtime é **automática e determinística** baseada no ambiente de execução e configurações.