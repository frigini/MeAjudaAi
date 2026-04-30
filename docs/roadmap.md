# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 13.3 (Refatoração de Persistência — Fase 0)

**Status**: 🚧 Em Andamento — Fase 0 Shared

**Meta MVP**: 12 a 16 de maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🏃 Sprint 13.1: Otimizações e Refinamentos (Concluído)

## 🚀 Sprint 13.2: Edge Infrastructure & API Gateway (Planejado)

*   **Implementação do YARP Gateway**: Criação do projeto `MeAjudaAi.Gateway` como ponto único de entrada para todos os frontends (Admin, Mobile, Web).
*   **BFF (Backend for Frontend)**: Configuração de rotas segregadas e políticas de CORS/Rate Limiting específicas para cada perfil de acesso.
*   **Security Hardening**: Centralização de validação de tokens JWT/Keycloak e sanitização de headers globais no Gateway.
*   **Service Discovery**: Integração com .NET Aspire para roteamento dinâmico para o ApiService.
*   **Resiliência**: Configuração de retentativas (Retries) e Circuit Breaker para endpoints críticos de integração.

---

## 🔧 Sprint 13.3: Refatoração de Persistência — Fase 0 (Em Andamento)

*   **Preparação da Camada Compartilhada**: Criação das interfaces base `IRepository<TAggregate, TKey>` e `IUnitOfWork` sob `src/Shared/Database/`. O `DbContext` de cada módulo será a implementação concreta, eliminando o anti-pattern de wrapping classes.
*   **Criação de `IRepository<TAggregate, TKey>`**: Interface genérica com métodos `TryFindAsync`, `Add`, `Delete` e implementação default de `DeleteAsync`. Elimina o anti-pattern de `SaveChangesAsync` por método.
*   **Criação de `IUnitOfWork`**: Interface com `GetRepository<TAggregate, TKey>()` e `SaveChangesAsync`. O `DbContext` será a implementação concreta.
*   **Verificação do `BaseDbContext`**: Confirmação de que o dispatch de domain events funciona corretamente com a nova herança.
*   **Remoção do Scrutor Scan**: Eliminação do scan automático de `*Repository` via `AddModuleRepositories`, substituído por registro explícito do `DbContext` por módulo.

> **Exceções acordadas**: `IOutboxRepository<TMessage>` permanece em seu local atual (infraestrutura de mensageria, não aggregate DDD). A operação `AddIfNoOverlapAsync` de Bookings será movida para um `IBookingCommandService` dedicado (transação serializável com retry).

> **Nota**: Esta é a Fase 0 (Shared) do plano de refatoração em 5 fases. Serve como base para todas as fases seguintes. O módulo Locations será o piloto na Fase 1.

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
