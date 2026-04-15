# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 11 (Monetização & Polimento)
**Status**: 🚧 Em andamento
**Meta MVP**: 12 - 16 de Maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 💰 Sprint 11 - Monetização & Polimento (13 Abr - 27 Abr 2026) 🚧 [EM ANDAMENTO]

**Objetivo**: Habilitar o faturamento da plataforma e finalizar a experiência do usuário.

### 🔴 MUST-HAVE:

#### 1. 💳 Payments Module (Módulo de Pagamentos)
*   **Arquitetura**: Padrão de **Anti-Corruption Layer (ACL)**. A lógica de negócio não conhece tipos do Stripe. Abstração via `IPaymentGateway`.
*   **Funcionalidades**:
    *   **Assinaturas de Prestadores**: Planos Free, Standard e Gold.
    *   **Stripe Checkout & Webhooks**: Redirecionamento seguro e processamento de eventos (`invoice.paid`, etc.).
    *   **Portal de Billing**: Gestão de cartões e cancelamentos.
*   **Schema DB**: `payments` | **ModuleName**: `Payments`.

#### 2. 🌍 Localização Frontend (i18n)
*   **Arquitetura**: `i18next` + `react-i18next`.
*   **Funcionalidades**: Suporte PT-BR/EN-US, tradução automática de erros do **Zod** e seletor de idioma na UI.

#### 3. 🏁 Preparação para Lançamento
*   **Endurecimento**: Skeletons de carregamento e mensagens de erro amigáveis em todos os fluxos.
*   **Documentação Final**: Manuais de Usuário e Guias de Implantação.

---

## 🔮 Roadmaps Futuros (Pós-MVP)

### Fase 3: Escala e Provedores Reais
*   **Provedores de Comunicação**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
*   **Verificação Automatizada**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.

### Fase 4: Experiência e Engajamento
*   **Módulo de Agendamentos (Bookings)**: Calendário de disponibilidade.
*   **Sistema de Disputas**: Mediação administrativa para conflitos.

---

## ✅ Concluído Recentemente

*   **Sprint 10**: Módulo de Ratings, Moderação de Conteúdo, Login Social Instagram (#141), Alinhamento de Realms Keycloak, Infra CI/CD (OpenAPI gating) e Documentação (coleções Bruno).
*   **Sprint 9**: Estabilização global, Módulo de Comunicações (Infra), Resiliência (`CancellationToken`) e Localização Backend (.resx).
*   **Sprint 8D/8E**: Migração completa do Admin Portal para React e Testes E2E com Playwright.

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
