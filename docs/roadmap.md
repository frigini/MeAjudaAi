# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 13.1 (Otimizações e Refinamentos)

**Status**: 🚧 Em Andamento

**Meta MVP**: 12 a 16 de maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🏃 Sprint 13.1: Otimizações e Refinamentos (Em Andamento)

*   **Performance Zero-Allocation**: Redução drástica de pressão no Garbage Collector em *Hot Paths* (Middlewares de requisições, logging e métricas) e *Application Layer* via `Span<T>` e `ReadOnlySpan<char>`. Eliminação de alocações desnecessárias envolvendo strings e expressões regulares.

---

## 🔮 Roadmaps Futuros (MVP Launch & Além)

### Fase 3: Escala e Provedores Reais
*   **Provedores de Comunicação (Próximo)**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
*   **Verificação Automatizada (Próximo)**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.

### Fase 4: Experiência e Engajamento
*   **Sistema de Disputas**: Mediação administrativa para conflitos.
*   **Melhorias em Bookings**: Sincronização com Google Calendar/Outlook e lembretes automáticos.

### 🚀 Arquitetura Evolutiva e Mensageria (Objetivos)
*   **Desempenho do Service Bus (Planejado)**: Implementar ajuste fino de paralelismo baseado no atributo `[HighVolumeEvent]` e otimizações no `RabbitMqInfrastructureManager`.
*   **Resiliência Crítica (Planejado)**: Garantir persistência via Quorum Queues para eventos marcados com `[CriticalEvent]`.
*   **Roteamento por Atributo (Em Andamento)**: Evolução do `AttributeTopicNameConvention` para suporte total a tópicos dedicados.

---

## ✅ Concluído Recentemente

*   **Sprint 13**: RabbitMQ Excellence (infraestrutura real com RabbitMqInfrastructureManager, deadlocks corrigidos, dispose seguro), i18n mocks para testes (Provider/Admin/Customer), fail-fast em DI de Messaging.
*   **Sprint 12**: Bookings Module completo, Command Handlers (Reject/Complete), queries de listagem, automação com Domain Events, integração frontend de agenda.
*   **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente.

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
