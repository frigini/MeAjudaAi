# Estratégia de Messaging - Plataforma MeAjudaAi

## 1. Visão Geral

Este documento descreve a estratégia completa de messaging da plataforma MeAjudaAi, incluindo a seleção automática de MessageBus por ambiente, implementação de Dead Letter Queue (DLQ) e sistema de mocks para testes. A arquitetura suporta RabbitMQ para desenvolvimento, Azure Service Bus para produção, e mocks/NoOp para ambientes de teste, garantindo isolamento e confiabilidade em todos os cenários.

## 2. MessageBus por Ambiente

### 2.1 Resumo da Implementação

✅ A implementação garante seleção automática de MessageBus por ambiente:
- **RabbitMQ** para desenvolvimento (quando habilitado)
- **NoOp/Mocks** para testes (sem dependências externas)
- **Azure Service Bus** para produção

### 2.2 Factory Pattern para Seleção de MessageBus

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
            // DESENVOLVIMENTO: RabbitMQ (apenas se explicitamente habilitado) ou NoOp (caso contrário)
            if (rabbitMqEnabled == true)
            {
                var rabbitMqService = _serviceProvider.GetService<RabbitMqMessageBus>();
                if (rabbitMqService != null)
                {
                    return rabbitMqService;
                }
                return _serviceProvider.GetRequiredService<NoOpMessageBus>(); // Fallback (reserva)
            }
            else
            {
                return _serviceProvider.GetRequiredService<NoOpMessageBus>();
            }
        }
        else if (_environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // TESTE: Sempre NoOp para evitar dependências externas
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
        else if (_environment.IsProduction())
        {
            // PRODUÇÃO: Azure Service Bus
            return _serviceProvider.GetRequiredService<ServiceBusMessageBus>();
        }
        else
        {
            // OUTROS: NoOp por segurança
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }
    }
}
```

### 2.3 Configuração de Dependency Injection por Ambiente

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
    // Testing: apenas NoOp/mocks - NoOpMessageBus será registrado abaixo
}

// Garantir que NoOpMessageBus esteja sempre disponível como fallback para todos os ambientes
services.TryAddSingleton<NoOpMessageBus>();

// Registrar o factory e o IMessageBus baseado no ambiente
services.AddSingleton<IMessageBusFactory, EnvironmentBasedMessageBusFactory>();
services.AddSingleton<IMessageBus>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();
    return factory.CreateMessageBus(); // ← Seleção baseada no ambiente
});
```

### 2.4 Configurações por Ambiente

#### Development (`appsettings.Development.json`)

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
```

**Nota**: O RabbitMQ suporta duas formas de configuração de conexão:
1. **ConnectionString direta**: `"amqp://user:pass@host:port/vhost"`
2. **Propriedades individuais**: O sistema automaticamente constrói a ConnectionString usando `Host`, `Port`, `Username`, `Password` e `VirtualHost` através do método `BuildConnectionString()`

#### Production (`appsettings.Production.json`)

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

#### Testing (`appsettings.Testing.json`)

```json
{
  "Messaging": {
    "Enabled": false,
    "Provider": "Mock"
  }
}
```

### 2.5 Configuração de Mocks para Testes

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

### 2.6 Transporte Rebus por Ambiente

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
        // TESTE: Nenhum transporte configurado - mocks lidam com messaging
        return; // Configuração de transporte ignorada para testing
    }
    else if (environment.IsDevelopment())
    {
        // DESENVOLVIMENTO: RabbitMQ
        transport.UseRabbitMq(
            rabbitMqOptions.BuildConnectionString(), // Constrói a partir de Host/Port ou usa ConnectionString
            rabbitMqOptions.DefaultQueueName);
    }
    else
    {
        // PRODUÇÃO: Azure Service Bus
        transport.UseAzureServiceBus(
            serviceBusOptions.ConnectionString,
            serviceBusOptions.DefaultTopicName);
    }
}
```

### 2.7 Infraestrutura Aspire por Ambiente

**Arquivo**: `src/Aspire/MeAjudaAi.AppHost/Program.cs`

```csharp
if (isDevelopment) // Apenas Development
{
    // RabbitMQ local para desenvolvimento
    var rabbitMq = builder.AddRabbitMQ("rabbitmq")
        .WithManagementPlugin();
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference(rabbitMq); // ← RabbitMQ apenas para Development
}
else if (isProduction) // Apenas Production
{
    // Azure Service Bus para Production
    var serviceBus = builder.AddAzureServiceBus("servicebus");
    
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")  
        .WithReference(serviceBus); // ← Service Bus para Production
}
else // Ambiente Testing
{
    // Sem infraestrutura externa de message bus para Testing
    // NoOpMessageBus será usado sem dependências externas
    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice");
    // ← Sem referência a message bus, NoOpMessageBus gerencia todo o messaging
}
```

### 2.8 Garantias Implementadas

#### ✅ 1. Ambiente Development
- **IMessageBus**: `RabbitMqMessageBus` (se `RabbitMQ:Enabled == true`) OU `NoOpMessageBus` (se desabilitado)
- **Transport**: RabbitMQ (se habilitado) OU None (se desabilitado)
- **Infrastructure**: RabbitMQ container (Aspire, quando habilitado)
- **Configuration**: `appsettings.Development.json` → "Provider": "RabbitMQ", "RabbitMQ:Enabled": true

#### ✅ 2. Ambiente Testing
- **IMessageBus**: `NoOpMessageBus` (ou Mocks para testes de integração)
- **Transport**: None (Rebus não configurado para Testing)
- **Infrastructure**: NoOp/Mocks (sem dependências externas)
- **Configuration**: `appsettings.Testing.json` → "Provider": "Mock", "Enabled": false, "RabbitMQ:Enabled": false

#### ✅ 3. Ambiente Production
- **IMessageBus**: `ServiceBusMessageBus`
- **Transport**: Azure Service Bus (via Rebus)
- **Infrastructure**: Azure Service Bus (via Aspire)
- **Configuration**: `appsettings.Production.json` → "Provider": "ServiceBus"

### 2.9 Fluxo de Seleção

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
│ OU NoOp         │                 │ + Escalável     │
│ (se desabilitado)│                │                 │
└─────────────────┴─────────────────┴─────────────────┘
```

### 2.10 Validação

#### Como Confirmar a Configuração

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

### 2.11 Resumo

✅ A implementação **garante completamente** que:

- **RabbitMQ** é usado para **Development** apenas **quando explicitamente habilitado** (`RabbitMQ:Enabled == true`)
- **Testing** sempre usa **NoOp/Mocks** (sem dependências externas)
- **NoOp MessageBus** é usado como **fallback seguro** quando RabbitMQ está desabilitado ou indisponível
- **Azure Service Bus** é usado exclusivamente para **Production**  
- **Mocks** são usados automaticamente em **testes de integração** (substituindo implementações reais)

A seleção é feita automaticamente via:
1. **Detecção de ambiente** (`IHostEnvironment`)
2. **Habilitação baseada em configuração** (`RabbitMQ:Enabled`)
3. **Factory pattern** (`EnvironmentBasedMessageBusFactory`)
4. **Dependency injection** (registro baseado no ambiente)
5. **Fallbacks graciosos** (NoOp quando RabbitMQ indisponível)
6. **Mocks automáticos para testes** (AddMessagingMocks() aplicado automaticamente em ambiente Testing)

**Configuração manual mínima** é necessária apenas para testes de integração que requerem registro explícito de mocks via `AddMessagingMocks()`. A seleção de MessageBus em runtime é **automática e determinística** baseada no ambiente de execução e configurações.

## 3. Dead Letter Queue (DLQ)

### 3.1 Visão Geral

A estratégia de Dead Letter Queue foi implementada com sucesso na plataforma MeAjudaAi, fornecendo:

- ✅ **Retentativa automática** com backoff exponencial
- ✅ **Classificação inteligente** de falhas (permanentes vs. temporárias)
- ✅ **Suporte multi-ambiente** (RabbitMQ para dev, Service Bus para prod)
- ✅ **Observabilidade completa** com logs estruturados e métricas
- ✅ **Operações de gerenciamento** (reprocessar, purgar, listar)

### 3.2 Arquitetura Implementada

```text
┌──────────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│   Event Handler  │───▶│ MessageRetryMiddleware│───▶│  IDeadLetterService  │
│                  │    │                     │    │                      │
│ - UserCreated    │    │ - Lógica de Retry   │    │ - RabbitMQ (Dev)     │
│ - OrderProcessed │    │ - Estratégia de     │    │ - ServiceBus (Prod)  │
│ - EmailSent      │    │   Backoff           │    │ - NoOp (Testing)     │
└──────────────────┘    │ - Classificação de  │    └──────────────────────┘
                        │   Exceções          │                 │
                        └─────────────────────┘                 │
                                    │                           │
                                    ▼                           ▼
                        ┌─────────────────────┐    ┌──────────────────────┐
                        │   Fila de Retry     │    │   Dead Letter Queue  │
                        │                     │    │                      │
                        │ - Backoff           │    │ - Mensagens Falhas   │
                        │   Exponencial       │    │ - Análise de Falhas  │
                        │ - Máx: 300s         │    │ - Suporte a          │
                        └─────────────────────┘    │   Reprocessamento    │
                                                   └──────────────────────┘
```

### 3.3 Implementações

#### 3.3.1 RabbitMQ Dead Letter Service
**Ambiente**: Development/Testing

**Funcionalidades**:
- Dead Letter Exchange (DLX) automático
- TTL configurável para mensagens na DLQ
- Roteamento baseado em routing keys
- Persistência opcional

#### 3.3.2 Service Bus Dead Letter Service
**Ambiente**: Production

**Funcionalidades**:
- Dead Letter Queue nativa do Azure Service Bus
- Auto-complete configurável
- Duração de lock ajustável
- Integração com API de gerenciamento do Service Bus

### 3.4 Estratégia de Retry

#### 3.4.1 Políticas de Retry

##### 1. Falhas Permanentes (Sem Retry)
- **Exemplos**: `ArgumentException`, `BusinessRuleException`
- **Ação**: Envio imediato para DLQ

##### 2. Falhas Temporárias (Retry Recomendado)
- **Exemplos**: `TimeoutException`, `HttpRequestException`, `PostgresException`
- **Ação**: Retry com backoff exponencial

##### 3. Falhas Críticas (Sem Retry)
- **Exemplos**: `OutOfMemoryException`, `StackOverflowException`
- **Ação**: Envio imediato para DLQ + notificação de admin

#### 3.4.2 Backoff Exponencial

O atraso entre retentativas aumenta exponencialmente usando a fórmula `2^(attemptCount-1) * 2` segundos, limitado a 300 segundos (5 minutos).

**Intervalos de retry**: 2s, 4s, 8s, 16s, 32s, 64s, 128s, 256s (depois limitado a 300s)

### 3.5 Integração com Handlers

O `MessageRetryMiddleware` automaticamente intercepta falhas em event handlers e aplica a estratégia de retry/DLQ.

### 3.6 Monitoramento e Observabilidade

#### 3.6.1 Informações Capturadas

A classe `FailedMessageInfo` captura informações detalhadas sobre mensagens que falharam, incluindo:
- ID da mensagem, tipo e conteúdo original
- Fila de origem e contagem de tentativas
- Histórico de falhas e metadados do ambiente

#### 3.6.2 Estatísticas Disponíveis

A classe `DeadLetterStatistics` fornece uma visão geral da DLQ, incluindo:
- Número total de mensagens na DLQ
- Mensagens por fila e tipo de exceção
- Taxa de falhas por handler

### 3.7 Configuração e Setup

O sistema DLQ é configurado automaticamente via `services.AddMessaging(configuration, environment);` em `Program.cs`. Configurações específicas do ambiente são carregadas de `appsettings.Development.json` e `appsettings.Production.json`.

### 3.8 Operações DLQ

O `IDeadLetterService` fornece métodos para:
- Listar mensagens na DLQ
- Reprocessar uma mensagem específica
- Purgar uma mensagem após análise
- Obter estatísticas da DLQ

### 3.9 Cobertura de Testes

A implementação é coberta por uma suite abrangente de testes unitários e de integração, garantindo a confiabilidade do sistema DLQ.

### 3.10 Considerações de Segurança

- Informações sensíveis não são incluídas no `OriginalMessage`
- PII é mascarado nos logs
- Acesso a operações DLQ requer permissões de admin
- Mensagens têm TTL configurável

## 4. Implementação de Mocks

### 4.1 Visão Geral

Este capítulo descreve a implementação completa de mocks para Azure Service Bus e RabbitMQ, permitindo testes isolados e confiáveis sem dependências externas.

### 4.2 Componentes Implementados

#### 4.2.1 MockServiceBusMessageBus

**Localização**: `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/MockServiceBusMessageBus.cs`

**Funcionalidades**:
- Mock completo do Azure Service Bus
- Implementa interface `IMessageBus` com métodos `SendAsync`, `PublishAsync` e `SubscribeAsync`
- Rastreamento de mensagens enviadas e eventos publicados
- Suporte para simulação de falhas
- Verificação de mensagens por tipo, predicado e destino

**Métodos principais**:
- `WasMessageSent<T>()` - Verifica se mensagem foi enviada
- `WasEventPublished<T>()` - Verifica se evento foi publicado
- `GetSentMessages<T>()` - Obtém mensagens enviadas por tipo
- `SimulateSendFailure()` - Simula falhas de envio de mensagens
- `SimulatePublishFailure()` - Simula falhas de publicação de eventos

#### 4.2.2 MockRabbitMqMessageBus

**Localização**: `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/MockRabbitMqMessageBus.cs`

**Funcionalidades**:
- Mock completo do RabbitMQ MessageBus
- Interface idêntica ao mock do Service Bus
- Rastreamento separado para mensagens RabbitMQ
- Simulação de falhas específicas do RabbitMQ

#### 4.2.3 MessagingMockManager

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

#### 4.2.4 Extensions para Dependency Injection

**Funcionalidades**:
- `AddMessagingMocks()` - Configuração automática no container DI
- Remoção automática de implementações reais
- Registro dos mocks como implementações de `IMessageBus`

### 4.3 Integração com Testes

#### 4.3.1 ApiTestBase

**Localização**: `tests/MeAjudaAi.Integration.Tests/Base/ApiTestBase.cs`

**Modificações**:
- Configuração automática dos mocks de messaging
- Desabilitação de messaging real em testes
- Integração com TestContainers existente

#### 4.3.2 MessagingIntegrationTestBase

**Localização**: `tests/MeAjudaAi.Integration.Tests/Users/MessagingIntegrationTestBase.cs`

**Funcionalidades**:
- Classe base para testes que verificam messaging
- Acesso simplificado ao `MessagingMockManager`
- Métodos auxiliares para verificação de mensagens
- Limpeza automática entre testes

#### 4.3.3 UserMessagingTests

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

### 4.4 Eventos de Domínio Suportados

#### UserRegisteredDomainEvent
- **Gatilho**: Registro de novo usuário
- **Dados**: AggregateId, Version, Email, Username, FirstName, LastName

#### UserProfileUpdatedDomainEvent
- **Gatilho**: Atualização de perfil do usuário
- **Dados**: AggregateId, Version, FirstName, LastName

#### UserDeletedDomainEvent
- **Gatilho**: Exclusão (soft delete) de usuário
- **Dados**: AggregateId, Version

### 4.5 Uso em Testes

#### 4.5.1 Exemplo Básico

```csharp
public class MyMessagingTest : MessagingIntegrationTestBase
{
    [Fact]
    public async Task SomeAction_ShouldPublishEvent()
    {
        // Preparação
        await EnsureMessagingInitializedAsync();
        
        // Ação
        await Client.PostAsJsonAsync("/api/some-endpoint", data);
        
        // Verificação
        var wasPublished = WasMessagePublished<MyEvent>(e => e.SomeProperty == expectedValue);
        wasPublished.Should().BeTrue();
        
        var events = GetPublishedMessages<MyEvent>();
        events.Should().HaveCount(1);
    }
}
```

#### 4.5.2 Verificação de Estatísticas

```csharp
var stats = GetMessagingStatistics();
stats.ServiceBusMessageCount.Should().Be(2);
stats.RabbitMqMessageCount.Should().Be(1);
stats.TotalMessageCount.Should().Be(3);
```

#### 4.5.3 Simulação de Falhas

```csharp
// Simular falha em envio de mensagens
MessagingMocks.ServiceBus.SimulateSendFailure(new Exception("Send failure"));

// Simular falha em publicação de eventos
MessagingMocks.ServiceBus.SimulatePublishFailure(new Exception("Publish failure"));

// Testar cenários de falha...

// Restaurar comportamento normal
MessagingMocks.ServiceBus.ResetToNormalBehavior();
```

### 4.6 Vantagens da Implementação

#### 4.6.1 Isolamento Completo
- Testes não dependem de serviços externos
- Execução rápida e confiável
- Controle total sobre cenários de teste

#### 4.6.2 Verificação Detalhada
- Rastreamento preciso de todas as mensagens
- Verificação por tipo, predicado e destino
- Estatísticas detalhadas de uso

#### 4.6.3 Simulação de Falhas
- Testes de cenários de erro
- Validação de tratamento de exceções
- Testes de resiliência

#### 4.6.4 Facilidade de Uso
- API intuitiva e bem documentada
- Integração automática com DI
- Limpeza automática entre testes

### 4.7 Melhorias Futuras

#### 4.7.1 Mock de Outros Serviços Azure
- Azure Storage Account
- Azure Key Vault
- Azure Cosmos DB

#### 4.7.2 Persistência de Mensagens
- Histórico entre execuções de teste
- Análise temporal de mensagens

#### 4.7.3 Visualização
- Dashboard de mensagens em testes
- Relatórios de uso de messaging

#### 4.7.4 Testes de Performance
- Mocks para testes de carga
- Simulação de latência de rede

## 5. Referências

### 5.1 Arquivos Principais

- `src/Shared/MeAjudaAi.Shared/Messaging/Factory/MessageBusFactory.cs` - Factory Pattern para seleção de MessageBus
- `src/Shared/MeAjudaAi.Shared/Messaging/Extensions.cs` - Configuração de DI e transporte
- `src/Aspire/MeAjudaAi.AppHost/Program.cs` - Configuração de infraestrutura Aspire
- `tests/MeAjudaAi.Shared.Tests/Mocks/Messaging/` - Implementações de mocks

### 5.2 Documentos Relacionados

- [Arquitetura](architecture.md) - Visão geral da arquitetura da plataforma
- [Configuração](configuration.md) - Detalhes sobre configurações da aplicação
- [Desenvolvimento](development.md) - Guia de desenvolvimento local

### 5.3 Conclusão

A infraestrutura de messaging da plataforma MeAjudaAi estabelece uma base sólida para comunicação assíncrona entre componentes, fornecendo:

- **Flexibilidade multi-ambiente** com seleção automática de MessageBus
- **Confiabilidade** através de Dead Letter Queue e estratégias de retry
- **Testabilidade** com mocks completos e fáceis de usar
- **Observabilidade** com logs estruturados e métricas detalhadas

A implementação permite desenvolvimento local eficiente com RabbitMQ, testes isolados com mocks/NoOp, e escalabilidade em produção com Azure Service Bus, mantendo consistência na experiência de desenvolvimento e teste em todos os ambientes.
