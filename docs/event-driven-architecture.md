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
| Communications | N/A | (Interno - disparado via eventos) |
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

| Evento | Status | Consumidor(es) Identificado(s) |
| :--- | :--- | :--- |
| BookingCancelledIntegrationEvent | OK | Communications |
| BookingCompletedIntegrationEvent | OK | Communications (Rating Invite) |
| BookingConfirmedIntegrationEvent | OK | Communications |
| BookingCreatedIntegrationEvent | OK | Communications |
| BookingRejectedIntegrationEvent | OK | Communications |
| DocumentRejectedIntegrationEvent | OK | DocumentRejectedIntegrationEventHandler (Communications) |
| DocumentVerifiedIntegrationEvent | OK | DocumentVerifiedIntegrationEventHandler (Communications, Providers) |
| AllowedCityCreatedIntegrationEvent | Pendente | - |
| AllowedCityDeletedIntegrationEvent | Pendente | - |
| AllowedCityUpdatedIntegrationEvent | Pendente | - |
| SubscriptionActivatedIntegrationEvent | OK | SubscriptionActivatedIntegrationEventHandler (Payments, Providers) |
| SubscriptionCanceledIntegrationEvent | OK | SubscriptionCanceledIntegrationEventHandler (Payments, Providers) |
| SubscriptionExpiredIntegrationEvent | OK | SubscriptionExpiredIntegrationEventHandler (Payments, Providers) |
| SubscriptionRenewedIntegrationEvent | OK | SubscriptionRenewedIntegrationEventHandler (Payments) |
| ProviderActivatedIntegrationEvent | OK | ProviderActivatedIntegrationEventHandler (Communications, SearchProviders) |
| ProviderAwaitingVerificationIntegrationEvent| OK | ProviderAwaitingVerificationIntegrationEventHandler (Communications) |
| ProviderDeletedIntegrationEvent | OK | ProviderDeletedIntegrationEventHandler (SearchProviders) |
| ProviderIndexRequiredIntegrationEvent | OK | ProviderIndexRequiredIntegrationEventHandler (SearchProviders) |
| ProviderProfileUpdatedIntegrationEvent | OK | ProviderProfileUpdatedIntegrationEventHandler (SearchProviders) |
| ProviderRegisteredIntegrationEvent | OK | ProviderRegisteredIntegrationEventHandler (Communications) |
| ProviderServicesUpdatedIntegrationEvent | OK | ProviderServicesUpdatedIntegrationEventHandler (SearchProviders) |
| ProviderVerificationStatusUpdatedIntegrationEvent| OK | ProviderVerificationStatusUpdatedIntegrationEventHandler (Communications) |
| ReviewApprovedIntegrationEvent | OK | ReviewApprovedIntegrationEventHandler (SearchProviders) |
| SearchableProviderIndexedIntegrationEvent | Pendente | - |
| ServiceActivatedIntegrationEvent | OK | ServiceActivatedIntegrationEventHandler (SearchProviders) |
| ServiceDeactivatedIntegrationEvent | OK | ServiceDeactivatedIntegrationEventHandler (SearchProviders) |
| ServiceNameUpdatedIntegrationEvent | OK | ServiceNameUpdatedIntegrationEventHandler (Providers) |
| UserDeletedIntegrationEvent | OK | UserDeletedIntegrationEventHandler (Ratings) |
| UserProfileUpdatedIntegrationEvent | OK | UserProfileUpdatedIntegrationEventHandler (Communications) |
| UserRegisteredIntegrationEvent | OK | UserRegisteredIntegrationEventHandler (Communications) |


## 4. Resiliência de Mensagens

Para garantir a confiabilidade da comunicação assíncrona:

- **Quorum Queues**: Eventos marcados com o atributo `[CriticalEvent]` (em `MeAjudaAi.Shared.Messaging.Attributes`) são declarados como Quorum Queues para garantir persistência e alta disponibilidade no RabbitMQ.
- **Outbox Pattern**: Todos os eventos devem ser persistidos na tabela `Outbox` do módulo origem dentro da mesma transação do domínio antes de serem publicados.
