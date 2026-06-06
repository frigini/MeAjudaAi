# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Maio 2026)

**Sprint Atual**: Sprint 14 (Em andamento)

**Status**: 🚀 Em Execução

**Meta MVP**: 12 a 16 de maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🏃‍♂️ Sprint Atual: Planejamento Tático

| Sprint | Foco | Status |
|--------|------|--------|
| **14** | **Revisão Geral e Débito Técnico** | 🚀 Em Execução |
| **14.1**| **Backend Code Review & Sync** | 🚀 Em Execução |

### Detalhes Sprint 14
- ✅ **Concluído**: Revisão geral de pendências de arquitetura e qualidade.
- ✅ **Concluído**: Implementação de `ServiceNameUpdatedIntegrationEvent` para sincronização orientada a eventos entre `ServiceCatalogs` e `Providers`, eliminando débito técnico de isolamento de schemas.

### Detalhes Sprint 14.1
- Code Review de backend focado em conformidade com padrões DDD e mensageria.
- Validação da robustez do barramento de eventos implementado.
- Limpeza final de códigos de fallback pós-refatoração de persistência.


---

## 🔮 Roadmaps Futuros (MVP Launch & Além)

### Fase 3: Escala e Provedores Reais

- **Provedores de Comunicação (Próximo)**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
- **Verificação Automatizada (Próximo)**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.

### Fase 4: Experiência e Engajamento

- **Sistema de Disputas**: Mediação administrativa para conflitos.
- **Melhorias em Bookings**: Sincronização com Google Calendar/Outlook e lembretes automáticos.
- **Analytics & Reports (Novo)**: 
    - **Provedores**: Dashboards de conversão (cliques vs. agendamentos), métricas de faturamento e avaliação média temporal.
    - **Administração**: Relatórios de crescimento da plataforma, hotspots geográficos de demanda e exportação de dados para contabilidade (CSV/PDF).
    - **Inteligência**: Sugestões de precificação baseadas na demanda da região.

### 🚀 Arquitetura Evolutiva e Mensageria (Objetivos)

- **Desempenho do Service Bus (Planejado)**: Implementar ajuste fino de paralelismo baseado no atributo `[HighVolumeEvent]` e otimizações no `RabbitMqInfrastructureManager`.
- **Resiliência Crítica (Planejado)**: Garantir persistência via Quorum Queues para eventos marcados com `[CriticalEvent]`.
- **Roteamento por Atributo (Em Andamento)**: Evolução do `AttributeTopicNameConvention` para suporte total a tópicos dedicados.

---

## ✅ Concluído Recentemente

- **Sprint 13**: RabbitMQ Excellence (infraestrutura real com RabbitMqInfrastructureManager, deadlocks corrigidos, dispose seguro), i18n mocks para testes (Provider/Admin/Customer), fail-fast em DI de Messaging.
- **Sprint 12**: Bookings Module completo, Command Handlers (Reject/Complete), queries de listagem, automação com Domain Events, integração frontend de agenda.
- **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente.

---

## 📜 Histórico Completo

Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
