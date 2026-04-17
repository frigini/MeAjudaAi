# Estratégia de Messaging - Plataforma MeAjudaAi

## 1. Visão Geral

Este documento descreve a estratégia completa de messaging da plataforma MeAjudaAi, focada exclusivamente no **RabbitMQ** para todos os ambientes (desenvolvimento e produção) e **NoOp/Mocks** para ambientes de teste, garantindo isolamento e confiabilidade.

## 2. MessageBus por Ambiente

### 2.1 Resumo da Implementação

✅ A implementação garante seleção automática de MessageBus por ambiente:
- **RabbitMQ (via Rebus)** para desenvolvimento e produção
- **NoOp/Mocks** para testes (sem dependências externas)

### 2.2 Factory Pattern para Seleção de MessageBus

**Arquivo**: `src/Shared/Messaging/Factories/MessageBusFactory.cs`

O sistema utiliza um Factory para instanciar o provedor correto baseado no ambiente, garantindo que testes nunca tentem conectar em brokers reais.

### 2.3 Configurações por Ambiente

#### Desenvolvimento (`appsettings.Development.json`)

```json
{
  "Messaging": {
    "Enabled": true,
    "RabbitMQ": {
      "DefaultQueueName": "meajudaai-events-dev"
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
      "DefaultQueueName": "meajudaai-events-prod"
    }
  }
}
```

> **Nota de Migração**: Os nomes das filas foram alterados para lowercase (ex: `meajudaai-events`) para seguir as melhores práticas do RabbitMQ e consistência com `DefaultQueueName` no `RabbitMqOptions.cs`. Antes de realizar o deploy com estas novas configurações, certifique-se de que as filas antigas (com PascalCase) foram drenadas ou migradas, pois o sistema criará novas filas automaticamente.

### 2.4 Dead Letter Queue (DLQ)

A estratégia de Dead Letter Queue para RabbitMQ inclui:
- ✅ **Retentativa automática** com backoff exponencial
- ✅ **Classificação inteligente** de falhas
- ✅ **Dead Letter Exchange (DLX)** automático
- ✅ **TTL configurável** para mensagens na DLQ

## 3. Apêndice: Guia de Migração de Filas (PascalCase para lowercase)

Com a mudança para nomes de fila em lowercase (ex: `MeAjudaAi-Events` para `meajudaai-events`), siga este checklist antes do deploy:

1. **Descoberta**: Identifique filas PascalCase existentes via Management UI ou `rabbitmqctl list_queues`.
2. **Drenagem**: Certifique-se de que todos os consumidores antigos processaram as mensagens pendentes.
3. **Migração (Opcional)**: Se houver volume crítico, use a ferramenta `rabbitmqadmin` para mover mensagens entre filas.
4. **Verificação**: Confirme se os novos consumidores estão apontando para as filas em lowercase.
5. **Limpeza**: Remova as filas PascalCase vazias para evitar confusão operacional.

Ferramentas recomendadas: `rabbitmqctl`, `rabbitmqadmin` e RabbitMQ Management UI.

## 4. Conclusão

A plataforma unificou sua infraestrutura de messaging no **RabbitMQ** através do **Rebus**, simplificando a arquitetura e garantindo paridade entre os ambientes de desenvolvimento e produção.
