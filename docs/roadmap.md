# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Junho 2026)

**Sprint Atual**: Sprint 14 (Finalização)

**Status**: 🚀 Em Execução

**Meta MVP**: 12 a 16 de maio de 2026 (Concluída)

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🏃‍♂️ Sprint Atual: Planejamento Tático

| Sprint | Foco | Status |
|--------|------|--------|
| **14** | **Auditoria de Débitos Técnicos** | 🚀 Em Execução |

### Detalhes Sprint 14
- ✅ **Concluído**: Padronização de Infraestrutura e Autorização.
- ✅ **Concluído**: Normalização de Endpoints da API em todos os módulos.
- ✅ **Concluído**: Implementação Core de Eventos de Integração (Bookings, Payments, Providers, Locations).
- 🚀 **Em Execução (Plano de Auditoria e Implementação)**:
    - **Fase 1**: Auditoria (Matriz de Eventos) – ✅ Concluído. (Ver `docs/event-audit-matrix.md`)
    - **Fase 2**: Implementação de Handlers pendentes – ✅ Concluído. (Todos os 10 módulos auditados e handlers/APIs implementados)
    - **Fase 3**: Testes (Unitários e Integração) – ✅ Concluído.
    - **Fase 4**: Automação e Qualidade de Wiring – ✅ Concluído.
    - **Pendências Técnicas (Concluído)**:
        - ✅ Bug: [HasCompletedBookingAsync falha com >100 bookings](https://github.com/MeAjudaAi/MeAjudaAi/issues/1)
        - ✅ Task: [Aplicar atributos [CriticalEvent]/[HighVolumeEvent] nos eventos](https://github.com/MeAjudaAi/MeAjudaAi/issues/2)
    - **Extra**: Auditoria de Comunicação Síncrona vs Assíncrona realizada (Ver `docs/communication-audit.md`) e criação de APIs Públicas.
    - **Infraestrutura e Mensageria**:
        - **Desempenho do Service Bus (Concluído)**: ✅ Ajuste fino de paralelismo baseado no atributo `[HighVolumeEvent]` via QoS.
        - **Resiliência Crítica (Concluído)**: ✅ Persistência via Quorum Queues para eventos `[CriticalEvent]`.
        - **Roteamento por Atributo (Concluído)**: ✅ Suporte total a tópicos dedicados via `AttributeTopicNameConvention`.

---

## 🔮 Roadmaps Futuros (Além do MVP)

### Fase 4: Experiência e Engajamento

- **Sistema de Disputas**: Mediação administrativa para conflitos.
- **Melhorias em Bookings**: Sincronização com Google Calendar/Outlook e lembretes automáticos.
- **Pendências de Infraestrutura (SearchProviders)**:
    - [ ] `AllowedCityCreated/Updated/DeletedIntegrationEventHandler`: Stub/log implementado. Ativar lógica de reindexação regional quando o filtro de cidade estiver disponível em `SearchProviders`. (Marcado com `[ExcludeFromCodeCoverage]` por serem stubs temporários).
- **Novos Eventos de Integração (Roadmap)**:
    - `AllowedCity*` com ação real (quando `SearchProviders` filtrar por cidade/região).
    - `SubscriptionExpiringSoonIntegrationEvent` (comunicação preventiva de expiração).
    - `ReviewRejectedIntegrationEvent` (notificação de cliente/prestador sobre moderação).
    - `ProviderTierUpdatedIntegrationEvent` (se tier Gold/Standard afetar Search/Communications/Admin).
- **Evolução de APIs Públicas**:
    - `IPaymentsModuleApi`: Avaliar `GetSubscriptionStatusAsync` caso surja consumidor específico para apenas o status.
    - `ICommunicationsModuleApi`: Melhorar documentação de finalidade (uso interno vs admin).
- **Analytics & Reports (Novo)**: 
    - **Provedores**: Dashboards de conversão (cliques vs. agendamentos), métricas de faturamento e avaliação média temporal.
    - **Administração**: Relatórios de crescimento da plataforma, hotspots geográficos de demanda e exportação de dados para contabilidade (CSV/PDF).
    - **Inteligência**: Sugestões de precificação baseadas na demanda da região.


---

## ✅ Concluído Recentemente

- **Sprint 14**: Infraestrutura e Acesso, Padronização de API, Core Integration Events.
- **Sprint 13**: RabbitMQ Excellence (infraestrutura real com RabbitMqInfrastructureManager, deadlocks corrigidos, dispose seguro), i18n mocks para testes (Provider/Admin/Customer), fail-fast em DI de Messaging.
- **Sprint 12**: Bookings Module completo, Command Handlers (Reject/Complete), queries de listagem, automação com Domain Events, integração frontend de agenda.
- **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente.

---

## 📜 Histórico Completo

Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
