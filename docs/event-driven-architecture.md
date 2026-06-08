# Arquitetura de Eventos e Comunicação entre Módulos

Este documento descreve as diretrizes e padrões para comunicação entre módulos no MeAjudaAi.

## 1. Princípios de Comunicação

O sistema adota uma abordagem híbrida:
- **Comunicação Síncrona (Public APIs)**: Utilizada para consultas imediatas, validações obrigatórias e operações transacionais cross-module. É baseada no padrão `IModuleApi`.
- **Comunicação Assíncrona (Integration Events)**: Utilizada para efeitos colaterais, notificação de mudanças de estado e sincronização eventual (denormalização). É baseada em um Message Bus (RabbitMQ).

## 2. Public APIs (Síncrona)

Cada módulo deve expor uma interface `IModuleApi` se precisar disponibilizar funcionalidades para outros módulos.

- **Contrato**: Deve residir em `src/Contracts/Modules/{ModuleName}/I{ModuleName}ModuleApi.cs`.
- **DTOs**: Devem ser compartilhados via `src/Contracts/Modules/{ModuleName}/DTOs/`.
- **Implementação**: Deve ser registrada no DI como um serviço `Keyed` ou via interface comum.

### Auditoria de APIs (Estado Atual)

| Módulo | API Pública | Finalidade |
| :--- | :--- | :--- |
| Bookings | `IBookingsModuleApi` | Consultas de agendamentos e status |
| Communications | `ICommunicationsModuleApi` | Envio de notificações (E-mail/SMS/Push) e consultas de logs |
| Documents | `IDocumentsModuleApi` | Validação de documentos |
| Locations | `ILocationsModuleApi` | Geocoding e Validação de Cidades |
| Payments | `IPaymentsModuleApi` | Status de assinaturas |
| Providers | `IProvidersModuleApi` | Consultas de perfil e disponibilidade |
| Ratings | `IRatingsModuleApi` | Métricas de avaliação |
| SearchProviders | `ISearchProvidersModuleApi` | Gestão do índice de busca |
| ServiceCatalogs | `IServiceCatalogsModuleApi` | Catálogo de serviços |
| Users | `IUsersModuleApi` | Identidade e perfil |

## 3. Integration Events (Assíncrona)

### Padrão de Produção (Domínio -> Integração)
Módulos não devem publicar eventos de integração diretamente no Handler de Comando. A regra é:
1. O agregado dispara um `DomainEvent`.
2. Um `DomainEventHandler` interno ao módulo consome o `DomainEvent`.
3. Este handler traduz o evento de domínio para um `IntegrationEvent` e publica no Message Bus.

### Padrão de Consumo (Integração)
Handler que consome um `IntegrationEvent` e realiza ações secundárias (ex: salvar dados denormalizados, disparar notificação).

### Matriz de Auditoria de Eventos

| Evento | Consumidor | Ação atual | Status |
| :--- | :--- | :--- | :--- |
| BookingCancelledIntegrationEvent | Communications | Notificação | OK |
| BookingCompletedIntegrationEvent | Communications | Convite de Avaliação | OK |
| BookingConfirmedIntegrationEvent | Communications | Notificação | OK |
| BookingCreatedIntegrationEvent | Communications | Notificação | OK |
| BookingRejectedIntegrationEvent | Communications | Notificação | OK |
| DocumentRejectedIntegrationEvent | Communications | Notificação | OK |
| DocumentVerifiedIntegrationEvent | Communications, Providers | Notificação / Verificação | OK |
| AllowedCityCreatedIntegrationEvent | SearchProviders | Apenas log | Pendente funcional |
| AllowedCityDeletedIntegrationEvent | SearchProviders | Apenas log | Pendente funcional |
| AllowedCityUpdatedIntegrationEvent | SearchProviders | Apenas log | Pendente funcional |
| SubscriptionActivatedIntegrationEvent | Payments, Providers | Ativação/Promoção | OK |
| SubscriptionCanceledIntegrationEvent | Payments, Providers | Cancelamento/Demote | OK |
| SubscriptionExpiredIntegrationEvent | Payments, Providers | Expiração/Demote | OK |
| SubscriptionRenewedIntegrationEvent | Payments | Renovação | OK |
| ProviderActivatedIntegrationEvent | Communications, SearchProviders | Ativação/Indexação | OK |
| ProviderAwaitingVerificationIntegrationEvent| Communications | Notificação | OK |
| ProviderDeletedIntegrationEvent | SearchProviders | Remoção do índice | OK |
| ProviderIndexRequiredIntegrationEvent | SearchProviders | Indexação | OK |
| ProviderProfileUpdatedIntegrationEvent | SearchProviders | Indexação | OK |
| ProviderRegisteredIntegrationEvent | Communications | Boas-vindas | OK |
| ProviderServicesUpdatedIntegrationEvent | SearchProviders | Indexação | OK |
| ProviderVerificationStatusUpdatedIntegrationEvent| Communications | Notificação | OK |
| ReviewApprovedIntegrationEvent | SearchProviders | Indexação | OK |
| ServiceActivatedIntegrationEvent | SearchProviders | Indexação | OK |
| ServiceDeactivatedIntegrationEvent | SearchProviders | Indexação | OK |
| ServiceNameUpdatedIntegrationEvent | Providers | Atualização | OK |
| UserDeletedIntegrationEvent | Ratings | Remoção de avaliações | OK |
| UserProfileUpdatedIntegrationEvent | Communications | Notificação | OK |
| UserRegisteredIntegrationEvent | Communications | Boas-vindas | OK |

## 5. Eventos no Backlog

| Evento | Motivo |
| :--- | :--- |
| SearchableProviderIndexedIntegrationEvent | Sem consumidor ativo; aguardando requisito de analytics ou notificação. |
| AllowedCityCreatedIntegrationEvent | Pendente funcional |
| AllowedCityDeletedIntegrationEvent | Pendente funcional |
| AllowedCityUpdatedIntegrationEvent | Pendente funcional |


## 4. Resiliência de Mensagens

Para garantir a confiabilidade da comunicação assíncrona:

### Estratégia de Idempotência

| Tipo de Handler | Estratégia |
| :--- | :--- |
| Notificação | `CommunicationLog` / `correlationId` |
| Mutação de Estado | `ProcessedIntegrationEvents` (Tabela por módulo) |
| Reindexação | Idempotência natural (Upsert/Delete if exists) + Log EventId |
| Remoção | Idempotência natural (Não falhar se já removido) |

- **Quorum Queues**: Eventos marcados com o atributo `[CriticalEvent]` (em `MeAjudaAi.Shared.Messaging.Attributes`) são declarados como Quorum Queues para garantir persistência e alta disponibilidade no RabbitMQ.
- **High Volume**: Eventos marcados com `[HighVolumeEvent]` configuram o prefetchCount do consumidor baseando-se em `MaxParallelism` do atributo.
- **Outbox Pattern**: Todos os eventos devem ser persistidos na tabela `Outbox` do módulo origem dentro da mesma transação do domínio antes de serem publicados.
