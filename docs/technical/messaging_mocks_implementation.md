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
```

### Verificação de Estatísticas

```csharp
var stats = GetMessagingStatistics();
stats.ServiceBusMessageCount.Should().Be(2);
stats.RabbitMqMessageCount.Should().Be(1);
stats.TotalMessageCount.Should().Be(3);
```

### Simulação de Falhas

```csharp
// Simular falha em envio de mensagens
MessagingMocks.ServiceBus.SimulateSendFailure(new Exception("Send failure"));

// Simular falha em publicação de eventos
MessagingMocks.ServiceBus.SimulatePublishFailure(new Exception("Publish failure"));

// Testar cenários de falha...

// Restaurar comportamento normal
MessagingMocks.ServiceBus.ResetToNormalBehavior();
```

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