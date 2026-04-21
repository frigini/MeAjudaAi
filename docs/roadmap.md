# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 13 (Escala & Provedores Reais)

**Status**: 🚀 Em Início

**Meta MVP**: 12 a 16 de maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🔮 Roadmaps Futuros (MVP Launch & Além)

### Fase 3: Escala e Provedores Reais (Próximas Atividades)
*   **Provedores de Comunicação**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
*   **Verificação Automatizada**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.
*   **i18n Apps Provider/Admin**: Localização frontend para os apps de Prestador e Administrador.
*   **Documentação Final**: Manuais de Usuário e Guias de Implantação (revisão global).

### Fase 4: Experiência e Engajamento
*   **Sistema de Disputas**: Mediação administrativa para conflitos.
*   **Melhorias em Bookings**: Sincronização com Google Calendar/Outlook e lembretes automáticos.

### 🚀 Arquitetura Evolutiva e Mensageria (Objetivos)
*   **Performance do Service Bus (Planejado)**: Implementar ajuste fino de paralelismo baseado no atributo `[HighVolumeEvent]` e otimizações no `RabbitMqInfrastructureManager`.
*   **Resiliência Crítica (Planejado)**: Garantir persistência via Quorum Queues para eventos marcados com `[CriticalEvent]`.
*   **Roteamento por Atributo (Em Andamento)**: Evolução do `AttributeTopicNameConvention` para suporte total a tópicos dedicados.

---

## ✅ Concluído Recentemente

*   **Sprint 12**: Módulo de Bookings completo (Backend/Frontend), Migração final Rebus v3, Atributos de roteamento avançado e testes de arquitetura. (Abril 2026)
*   **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente. (Abril 2026)
*   **Sprint 10**: Módulo de Ratings, Moderação de Conteúdo, Login Social Instagram (#141), Alinhamento de Realms Keycloak, Infra CI/CD (OpenAPI gating) e Documentação (coleções Bruno). (Abril 2026)

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
