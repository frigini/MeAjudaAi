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

### Decisões de Design e Limitações Conhecidas

#### Communications — `GetTemplatesAsync` retorna apenas templates ativos

O endpoint `GET /api/v1/communications/templates` (via `ICommunicationsModuleApi.GetTemplatesAsync`) retorna apenas templates com `IsActive = true`. A query subjacente (`DbContextEmailTemplateQueries.GetAllAsync`) filtra por `x.IsActive`.

**Justificativa:** O endpoint serve templates disponíveis para envio de notificações. Templates inativos (versões históricas desativadas por `UpdateEmailTemplate`) não devem ser usados para envio.

**Limitação:** Não é possível listar templates inativos para auditoria ou reativação via este endpoint. Se gestão administrativa de templates for necessária, criar um endpoint admin separado:

```
GET /admin/communications/templates?includeInactive=true
```

#### Communications — `IsAvailableAsync` aceita módulo sem templates

O health check do módulo (`CommunicationsModuleApi.IsAvailableAsync`) retorna `true` mesmo quando não existem templates seedados, emitindo apenas um `LogWarning`.

**Justificativa:** Templates são opcionais — é possível enviar e-mails com corpo direto (`HtmlBody`/`TextBody`) sem depender de templates. O warning sinaliza que dados de seed podem estar ausentes, mas o módulo permanece operacional.

#### Communications — `SendEmailAsync` com TemplateKey e Body mutuamente exclusivos

O método `SendEmailAsync` aceita `EmailMessageDto` com `Body` opcional. A semântica é:

- **Com `TemplateKey`**: O corpo é renderizado a partir do template. `Body` é ignorado.
- **Sem `TemplateKey`**: `Body` é obrigatório e usado como `HtmlBody` ou `TextBody` conforme `IsHtml`.

Essa separação evita `ArgumentException` do `EmailOutboxPayload.Create()`, que rejeita a combinação de `TemplateKey` + `HtmlBody`/`TextBody`.

#### Bookings — `IBookingsApi` (Client.Contracts) completo

A interface Refit `IBookingsApi` em `src/Client/MeAjudaAi.Client.Contracts/Api/IBookingsApi.cs` expõe todos os endpoints REST do módulo:

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/bookings` | Criar agendamento |
| GET | `/api/v1/bookings/{id}` | Buscar agendamento por ID |
| GET | `/api/v1/bookings/my` | Listar meus agendamentos (paginado) |
| GET | `/api/v1/bookings/provider/{providerId}` | Listar agendamentos do prestador (paginado) |
| GET | `/api/v1/bookings/availability/{providerId}` | Consultar disponibilidade |
| POST | `/api/v1/bookings/schedule` | Definir agenda do prestador |
| PUT | `/api/v1/bookings/{id}/confirm` | Confirmar agendamento |
| PUT | `/api/v1/bookings/{id}/reject` | Rejeitar agendamento |
| PUT | `/api/v1/bookings/{id}/complete` | Concluir agendamento |
| PUT | `/api/v1/bookings/{id}/cancel` | Cancelar agendamento |

O endpoint SSE (`GET /api/v1/bookings/{id}/events`) não é suportado via Refit e deve ser consumido com `HttpClient` diretamente para streams `text/event-stream`.

A API inter-module (`IBookingsModuleApi`) é independente e expõe: `GetBookingByIdAsync`, `HasCompletedBookingAsync`, `GetProviderBookingsAsync`.

#### Bookings — Semântica temporal de `CreateBookingRequestDto`

O request usa `DateTimeOffset` (Start/End), enquanto a resposta (`ModuleBookingDto`) usa `DateOnly`/`TimeOnly`:

```
Request:  DateTimeOffset Start, DateTimeOffset End
Response: DateOnly Date, TimeOnly StartTime, TimeOnly EndTime
```

**Comportamento:** O `DateTimeOffset` enviado pelo cliente representa o instante solicitado. O módulo converte para a data e horário local do prestador (via `TimeZoneResolver`) antes de persistir como `DateOnly` + `TimeSlot`. O offset do cliente não é preservado — apenas a data/hora resultante no fuso do prestador é armazenada.

## 3. Integration Events (Assíncrona)

### Padrão de Produção (Domínio -> Integração)
Módulos não devem publicar eventos de integração diretamente no Handler de Comando. A regra é:
1. O agregado dispara um `DomainEvent`.
2. Um `DomainEventHandler` interno ao módulo consome o `DomainEvent`.
3. Este handler traduz o evento de domínio para um `IntegrationEvent` e publica no Message Bus.

### Padrão de Consumo (Integração)
Handler que consome um `IntegrationEvent` e realiza ações secundárias (ex: salvar dados denormalizados, disparar notificação).

### Matriz de Auditoria de Eventos

| Evento | Produtor | Consumidor | Status |
| :--- | :--- | :--- | :--- |
| BookingCancelledIntegrationEvent | Bookings | Communications | OK |
| BookingCompletedIntegrationEvent | Bookings | Communications | OK |
| BookingConfirmedIntegrationEvent | Bookings | Communications | OK |
| BookingCreatedIntegrationEvent | Bookings | Communications | OK |
| BookingRejectedIntegrationEvent | Bookings | Communications | OK |
| DocumentRejectedIntegrationEvent | Documents | Communications | OK |
| DocumentVerifiedIntegrationEvent | Documents | Communications, Providers | OK |
| AllowedCityCreatedIntegrationEvent | Locations | SearchProviders | Pendente funcional |
| AllowedCityDeletedIntegrationEvent | Locations | SearchProviders | Pendente funcional |
| AllowedCityUpdatedIntegrationEvent | Locations | SearchProviders | Pendente funcional |
| SubscriptionActivatedIntegrationEvent | Payments | Providers | OK |
| SubscriptionCanceledIntegrationEvent | Payments | Providers | OK |
| SubscriptionExpiredIntegrationEvent | Payments | Providers | OK |
| SubscriptionRenewedIntegrationEvent | Payments | — | Pendente (sem consumidor) |
| ProviderActivatedIntegrationEvent | Providers | Communications, SearchProviders | OK |
| ProviderAwaitingVerificationIntegrationEvent| Providers | Communications | OK |
| ProviderDeletedIntegrationEvent | Providers | SearchProviders | OK |
| ProviderIndexRequiredIntegrationEvent | Providers | SearchProviders | OK |
| ProviderProfileUpdatedIntegrationEvent | Providers | SearchProviders | OK |
| ProviderRegisteredIntegrationEvent | Providers | Communications | OK |
| ProviderServicesUpdatedIntegrationEvent | Providers | SearchProviders | OK |
| ProviderVerificationStatusUpdatedIntegrationEvent| Providers | Communications | OK |
| ReviewApprovedIntegrationEvent | Ratings | SearchProviders | OK |
| ServiceActivatedIntegrationEvent | ServiceCatalogs | SearchProviders | OK |
| ServiceDeactivatedIntegrationEvent | ServiceCatalogs | SearchProviders | OK |
| ServiceNameUpdatedIntegrationEvent | ServiceCatalogs | Providers | OK |
| UserDeletedIntegrationEvent | Users | Ratings | OK |
| UserProfileUpdatedIntegrationEvent | Users | Communications | OK |
| UserRegisteredIntegrationEvent | Users | Communications | OK |

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
