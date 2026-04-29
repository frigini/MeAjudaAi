# 📜 Histórico do Roadmap - MeAjudaAi

Este documento contém o registro de todas as sprints concluídas para fins de auditoria e contexto histórico.

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
    - **Nota Técnica**: Consolidação completa do RabbitMQ adiada para Sprint 13 (implementação real de topologia com QueueDeclareAsync/ExchangeDeclareAsync/QueueBindAsync concluída na Sprint 13).
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

---

## ✅ Sprint 10 - Qualidade & Onboarding (Concluída em 14 Abr 2026)

**Objetivo**: Estabelecer confiança na plataforma através de avaliações e simplificar o acesso de novos prestadores.

### Entregas:
- ✅ **Ratings Module**: Sistema de avaliações completo com desnormalização para performance de busca.
- ✅ **Moderação**: Filtros automáticos e manuais para comentários.
- ✅ **Login Social (Instagram)**: Reintegração via OIDC sincronizada entre ambientes (Issue #141).
- ✅ **Infra CI/CD**: OpenAPI Breaking Change Gating funcional no pipeline.
- ✅ **Documentação**: 100% de cobertura de coleções Bruno (.bru) para módulos ativos.
- ✅ **Keycloak Alignment**: Realms de `dev` e `prod` estruturalmente sincronizados (Roles e Clients).

---

## ✅ Sprint 9 - BUFFER & Mitigação de Risco (Concluída em 11 Abr 2026)

**Foco**: Estabilização, Refatoração e Módulo de Comunicações (Infra).

### Entregas:
- ✅ **Módulo de Comunicações**: Infraestrutura base com Outbox Pattern, Handlers de evento e Stubs.
- ✅ **Resiliência**: Aplicação de `CancellationToken` em repositórios e handlers (ServiceCatalogs, Documents, Locations).
- ✅ **Localização (Backend)**: Migração de strings de erro para arquivos `.resx` em `Shared`.
- ✅ **Segurança**: Endurecimento de middleware (Rate limiting, CORS, Antiforgery, Security Headers).
- ✅ **Outbox Processor**: Recuperação automática de mensagens travadas e tratamento de cancelamento.
- ✅ **Testes**: 100% de aprovação em Unitários (207), Integração (342) e E2E (161).

---

## ✅ Sprint 8E - Testes E2E e Infraestrutura React (Concluída em 25 Mar 2026)
*(Conteúdo anterior preservado...)*

# 🗺️ Roadmap - MeAjudaAi (Histórico Original)

Este documento consolida o planejamento estratégico e tático da plataforma MeAjudaAi, definindo fases de implementação, módulos prioritários e funcionalidades futuras.

---

## 📊 Sumário Executivo

**Projeto**: MeAjudaAi - Plataforma de Conexão entre Clientes e Prestadores de Serviços  
**Status Geral**: Consulte a [Tabela de Sprints](#cronograma-de-sprints) para o status detalhado atualizado.
**Testes/Cobertura**: Backend — Testes: N/A | Cobertura: 91.2%; Frontend — Testes: 42 (Vitest); Cobertura: N/A (Verificação via [coverage-report.xml](artifacts/coverage-report.xml) pós-merge)
**Stack**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 (Customer, Provider, Admin) + Tailwind v4

### Marcos Principais

Consulte a seção [Cronograma de Sprints](#cronograma-de-sprints) abaixo para o status detalhado e atualizado de cada sprint, e datas alvo (incluindo o MVP Launch).

**Procedimento de Revisão de Sprints**
As futuras atualizações da tabela de sprints devem observar a política: análise commit-by-commit newest-first, apresentando um veredicto conciso e resolvendo os follow-ups.

## ⚠️ Notas de Risco

- Estimativas assumem velocidade consistente e ausência de bloqueios maiores
- Primeiro projeto Blazor WASM pode revelar complexidade não prevista
- Sprint 9 reservado como buffer de contingência (não para novas features)

## 🏗️ Decisões Arquiteturais Futuras

### NX Monorepo (Frontend)

**Status**: ✅ Incluído no Sprint 8B.2  
**Branch**: `feature/sprint-8b2-technical-excellence`

**Motivação**: Com Customer Web App (Next.js), Provider App (próximo sprint), Admin Portal (migração planejada) e Mobile (React Native + Expo), o compartilhamento de código (componentes, hooks, tipos TypeScript, schemas Zod) entre os projetos se torna crítico. NX oferece:
- Workspace unificado com `libs/` compartilhadas
- Build cache inteligente (só reconstrói o que mudou)
- Dependency graph entre projetos
- Geração de código consistente

**Escopo (Sprint 8B.2)**:
- Migrar `MeAjudaAi.Web.Customer` para workspace NX
- Criar `apps/customer-web`, `apps/provider-web` (Sprint 8C), `apps/admin-web` (Sprint 8D), `apps/mobile` (Sprint 8E)
- Criar `libs/ui` (componentes compartilhados), `libs/auth`, `libs/api-client`
- Atualizar `.NET Aspire AppHost` para apontar para nova estrutura
- Atualizar CI/CD para usar `nx affected`

**Decisão de antecipação**: NX foi antecipado do pós-MVP para o Sprint 8B.2 porque o Provider App (Sprint 8C) e a migração Admin (Sprint 8D) se beneficiam diretamente do workspace unificado. Criar o NX antes desses projetos evita migração posterior mais custosa.

---
