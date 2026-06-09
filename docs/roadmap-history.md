# 📜 Histórico do Roadmap - MeAjudaAi

Este documento contém o registro de todas as sprints concluídas para fins de auditoria e contexto histórico.

---

## ✅ Sprint 14 - Infraestrutura, Normalização & Eventos (Concluída em Jun 2026)

**Objetivo**: Normalização arquitetural do backend, centralização de contratos de API e implementação de mensageria baseada em eventos para bounded contexts.

### Entregas:
- ✅ **Padronização de Infraestrutura e Autorização**: Migração global para `BaseDbContext`, padrão `IUnitOfWork` com serviços keyados e normalização dos middlewares de autorização.
- ✅ **Normalização de API Endpoints**: Padronização de todos os endpoints dos 10 módulos utilizando `ApiEndpoints` centralizados e `BaseEndpoint.CreateVersionedGroup`.
- ✅ **Core Integration Events**: Implementação, publicação e desacoplamento via `IMessageBus` para eventos de `Bookings`, `Payments`, `Providers` e `Locations`.
- ✅ **Limpeza de Débitos Técnicos**: Refatoração de serialização (`SystemTextJsonSerializer`), correção de `ClassifyFailure`, remoção de emojis em logs e normalização de `usings` duplicados.

---

## ✅ Sprint 13 - RabbitMQ Excellence & i18n Tests (Concluída em 28 Abr 2026)

**Objetivo**: Consolidação total da infraestrutura de mensageria, i18n frontend e UI/UX Admin.

### Entregas:
- ✅ **RabbitMQ Excellence**: Implementação real do `RabbitMqInfrastructureManager` com métodos assíncronos (`CreateQueueAsync`, `CreateExchangeAsync`, `BindQueueToExchangeAsync`), eliminando stubs.
- ✅ **Deadlock Fix**: Remoção de deadlocks em `CreateQueueAsync`, `CreateExchangeAsync`, `BindQueueToExchangeAsync` - agora `GetChannelAsync` gerencia lock internamente.
- ✅ **Safe Dispose**: `DisposeAsync` agora adquire lock antes de dispose e usa flag `_disposed` para prevenir operações pós-descarte.
- ✅ **Fail-Fast DI**: `MessagingExtensions` agora lança `InvalidOperationException` quando `IRabbitMqInfrastructureManager` não está registrado (em vez de retornar silenciosamente).
- ✅ **i18n Frontend**: Implementação de `useTranslation` no dashboard Admin, tradução de labels via i18n (`t()`), uso de `useMemo` para otimização.
- ✅ **i18n Test Mocks**: Implementação de mocks de i18next para testes em Admin, Provider e Customer apps com suporte a `defaultValue` (incluindo strings vazias).
- ✅ **UI/UX Admin Portal**: Aplicação de cores da marca (laranja #D96704, brand #E0702B, cream #FDFBF7) no CSS, variáveis CSS para cores secundárias, `data-testid` únicos em CardTitle.
- ✅ **Testes Unitários**: Cobertura superior a 90%, testes de `MessagingExtensions` e `RabbitMqInfrastructureManager` atualizados e passando.

---

## ✅ Sprint 12 - Bookings & Messaging Excellence (Concluída em 26 Abr 2026)

**Objetivo**: Implementar o sistema de agendamentos e consolidar a infraestrutura de mensageria com Rebus.

### Entregas:
- ✅ **Bookings Module**: Implementação completa (Backend/Frontend) de agendamentos com gestão de disponibilidade do prestador e fluxo de reserva do cliente.
- ✅ **Messaging Excellence**: Migração parcial para Rebus v3 e implementação de atributos `[DedicatedTopic]`, `[HighVolumeEvent]` e `[CriticalEvent]` para roteamento avançado.
- ✅ **Qualidade**: Cobertura completa atingida; módulo Bookings incluído no workflow de CI.
- ✅ **API & Contratos**: Padronização de enums (`EBookingStatus`) e exposição via Minimal APIs com autorização.
- ✅ **Manutenção**: Atualização da stack tecnológica (.NET 10.0.7, Aspire 13.2.4).

---

## ✅ Sprint 11 - Monetização & Polimento (Concluída em 15 Abr 2026)

**Objetivo**: Habilitar o faturamento da plataforma e finalizar a experiência do usuário.

### Entregas:
- ✅ **Payments Module**: Implementação de assinaturas (Stripe), webhooks, billing portal e renovações automáticas com padrão ACL.
- ✅ **Localização Frontend**: Suporte completo a i18n (PT-BR/EN-US) no Customer App, incluindo formulários e erros.
- ✅ **UX Polish**: Implementação de skeleton loaders animados para melhor percepção de desempenho.
- ✅ **Qualidade**: Cobertura de testes unitários e de integração para todos os fluxos críticos de pagamento e localização.

> Para histórico completo anterior, consultar: `git log --oneline -- docs/roadmap-history.md`
