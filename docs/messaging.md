# Estratégia de Messaging - Plataforma MeAjudaAi

## 1. Visão Geral

Este documento descreve a estratégia completa de messaging da plataforma MeAjudaAi, focada exclusivamente no **RabbitMQ** para todos os ambientes (desenvolvimento e produção) e **NoOp/Mocks** para ambientes de teste, garantindo isolamento e confiabilidade.

## 2. MessageBus por Ambiente

### 2.1 Resumo da Implementação

✅ A implementação garante seleção automática de MessageBus por ambiente:
- **RabbitMQ** para desenvolvimento e produção
- **NoOp/Mocks** para testes (sem dependências externas)

### 2.2 Factory Pattern para Seleção de MessageBus

**Arquivo**: `src/Shared/Messaging/Factories/MessageBusFactory.cs`

```csharp
public class MessageBusFactory : IMessageBusFactory
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    
    public MessageBusFactory(
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
        var rabbitMqEnabled = _configuration.GetValue<bool?>("RabbitMQ:Enabled");
        
        if (_environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // TESTE: Sempre NoOp para evitar dependências externas
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
        else
        {
            // PADRÃO (Dev/Prod): RabbitMQ (apenas se explicitamente habilitado) ou NoOp (fallback)
            if (rabbitMqEnabled != false)
            {
                return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
            }
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
    }
}
```

### 2.3 Configuração de Dependency Injection

**Arquivo**: `src/Shared/Messaging/MessagingExtensions.cs`

```csharp
// Registrar RabbitMQ e NoOp (fallback)
services.TryAddSingleton<RabbitMqMessageBus>();
services.TryAddSingleton<NoOpMessageBus>();

// Registrar o factory e o IMessageBus
services.AddSingleton<IMessageBusFactory, MessageBusFactory>();
services.AddSingleton<IMessageBus>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();
    return factory.CreateMessageBus();
});
```

### 2.4 Configurações por Ambiente

#### Desenvolvimento (`appsettings.Development.json`)

```json
{
  "Messaging": {
    "Enabled": true,
    "RabbitMQ": {
      "Enabled": true,
      "ConnectionString": "amqp://guest:guest@localhost:5672/",
      "DefaultQueueName": "MeAjudaAi-events-dev"
    }
  }
}
```

#### Produção (`appsettings.Production.json`)

```json
{
  "Messaging": {
    "Enabled": true,
    "RabbitMQ": {
      "ConnectionString": "${RABBITMQ_CONNECTION_STRING}",
      "DefaultQueueName": "MeAjudaAi-events-prod"
    }
  }
}
```

### 2.5 Dead Letter Queue (DLQ)

A estratégia de Dead Letter Queue para RabbitMQ inclui:
- ✅ **Retentativa automática** com backoff exponencial
- ✅ **Classificação inteligente** de falhas
- ✅ **Dead Letter Exchange (DLX)** automático
- ✅ **TTL configurável** para mensagens na DLQ

### 2.6 Mocks para Testes

Testes de integração usam `AddMessagingMocks()` para substituir o sistema real por rastreadores em memória, permitindo verificar publicações sem infraestrutura externa.

## 3. Conclusão

A plataforma unificou sua infraestrutura de messaging no **RabbitMQ**, simplificando a arquitetura e garantindo paridade entre os ambientes de desenvolvimento e produção.
