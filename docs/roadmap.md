# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 12 (Bookings & Messaging Excellence)
**Status**: 🚀 Em Início
**Meta MVP**: 12 - 16 de Maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🚀 Sprint 12 - Bookings & Messaging Excellence (28 Abr - 12 Mai 2026)

**Objetivo**: Implementar o sistema de agendamentos e consolidar a infraestrutura de mensageria com Rebus.

### 🔴 MUST-HAVE:

#### 1. 📅 Bookings Module (Módulo de Agendamentos)
*   **Domínio**: Entidade `Booking`, Value Objects para `TimeSlot` e `Availability`, enum `BookingStatus`.
*   **Funcionalidades**:
    *   **Gestão de Disponibilidade**: Prestador define horários e dias de trabalho.
    *   **Fluxo de Reserva**: Cliente solicita agendamento -> Notificação ao Prestador -> Confirmação/Rejeição.
    *   **Cancelamento**: Regras de negócio para cancelamento com ou sem estorno (integração futura com Payments).
*   **Infraestrutura**: Schema `bookings` no PostgreSQL e Migrations.

#### 2. 📨 Messaging Excellence (Rebus Migration)
*   **Consolidação**: Remover dependências diretas de `RabbitMQ.Client` nos módulos (exceto infra de base).
*   **Estabilização**: Validar handlers de eventos e retries usando as novas funcionalidades do Rebus.
*   **Desejável**: Implementar `[DedicatedTopic]` e `[CriticalEvent]` conforme planejado na Fase 3.

---

## 🔮 Roadmaps Futuros (MVP Launch & Além)

### Fase 3: Escala e Provedores Reais (Próximas Atividades)
*   **Provedores de Comunicação**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
*   **Verificação Automatizada**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.
*   **i18n Apps Provider/Admin**: Localização frontend para os apps de Prestador e Administrador.
*   **Documentação Final**: Manuais de Usuário e Guias de Implantação (revisão global).

### Fase 4: Experiência e Engajamento
*   **Módulo de Agendamentos (Bookings)**: Calendário de disponibilidade.
*   **Sistema de Disputas**: Mediação administrativa para conflitos.

### 🚀 Arquitetura Evolutiva e Mensageria (Desejável)
*   **Evolução do Service Bus**: Implementar lógica de infraestrutura no `Shared.Messaging` para interpretar atributos de mensageria:
    *   `[DedicatedTopic]`: Uso de `ITopicNameConvention` no Rebus para desviar eventos críticos/frequentes para filas dedicadas, evitando o "vizinho barulhento".
    *   `[HighVolumeEvent]`: Otimização de I/O no RabbitMQ (mensagens transientes ou Lazy Queues) e paralelismo massivo via `SetNumberOfWorkers`.
    *   `[CriticalEvent]`: Garantia de persistência via Quorum Queues e priorização de processamento (`x-max-priority`).

---

## ✅ Concluído Recentemente

*   **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente. (Abril 2026)
*   **Sprint 10**: Módulo de Ratings, Moderação de Conteúdo, Login Social Instagram (#141), Alinhamento de Realms Keycloak, Infra CI/CD (OpenAPI gating) e Documentação (coleções Bruno). (Abril 2026)
*   **Sprint 9**: Estabilização global, Módulo de Comunicações (Infra), Resiliência (`CancellationToken`) e Localização Backend (.resx). (Abril 2026)
*   **Sprint 8D/8E**: Migração completa do Admin Portal para React e Testes E2E com Playwright. (Março 2026)

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
