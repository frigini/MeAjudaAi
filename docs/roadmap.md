# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 11 (Monetização & Polimento)
**Status**: ✅ Concluído
**Meta MVP**: 12 - 16 de Maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## ✅ Sprint 11 - Monetização & Polimento (13-27 Abr 2026) 🏆 [CONCLUÍDO] — Concluída em 15 Abr 2026

**Objetivo**: Habilitar o faturamento da plataforma e finalizar a experiência do usuário.

### 🔴 MUST-HAVE:

#### 1. 💳 Payments Module (Módulo de Pagamentos)
*   **Arquitetura**: Padrão de **Anti-Corruption Layer (ACL)**. A lógica de negócio não conhece tipos do Stripe. Abstração via `IPaymentGateway`. ✅
*   **Funcionalidades**:
    *   ✅ **Assinaturas de Prestadores**: Planos Free, Standard e Gold — implementado com `CreateSubscriptionCommandHandler` com padrão gateway-first e compensação em caso de falha.
    *   ✅ **Stripe Checkout & Webhooks**: Redirecionamento seguro via `CreateSubscriptionEndpoint` e processamento assíncrono via padrão Inbox (`ProcessInboxJob`).
    *   ✅ **Qualidade & Testes**: Suíte completa de testes unitários e de integração validando fluxos críticos e tratamento de erros.
    *   ✅ **Handler `invoice.paid`**: Processamento de renovações mensais e registro de `PaymentTransaction` para auditoria.
    *   ✅ **Billing Portal**: Endpoint para gestão de assinaturas via Stripe Customer Portal.
    *   **Localização & i18n**:
        *   ✅ **Frontend (Customer App)**: Localização completa (PT-BR/EN-US) com suporte a pluralização e datas/moedas.
        *   ✅ **Backend (FluentValidation)**: Integração de mensagens de validação com arquivos de recurso `.resx` (concluído na Sprint 9 e validado nesta).
*   **Schema DB**: `payments` | **ModuleName**: `Payments`.

#### 2. 🌍 Localização Frontend (i18n)
*   **Arquitetura**: `i18next` + `react-i18next`. PT-BR como padrão, EN-US como alternativa.
*   **Escopo Sprint 11**: App **Customer** (`MeAjudaAi.Web.Customer`) — navegação, formulários, erros Zod localizados, seletor de idioma no header. ✅
*   **Qualidade**: Testes unitários com mock de i18n passando. ✅

#### 3. 🎨 UX Polish
*   ✅ **Skeletons de Carregamento**: Placeholders visuais animados (pulse) integrados nas listas de busca e perfil do prestador. Testes de unidade criados. ✅

---

## 🔮 Roadmaps Futuros (Pós-MVP)

### Fase 3: Escala e Provedores Reais
*   **Provedores de Comunicação**: Substituir Stubs por SendGrid (E-mail), Twilio (SMS) e Firebase (Push).
*   **Verificação Automatizada**: OCR via Azure AI Vision e integração com APIs de antecedentes criminais.
*   **i18n Apps Provider/Admin**: Localização frontend para os apps de Prestador e Administrador.
*   **Documentação Final**: Manuais de Usuário e Guias de Implantação (revisão global).

### Fase 4: Experiência e Engajamento
*   **Módulo de Agendamentos (Bookings)**: Calendário de disponibilidade.
*   **Sistema de Disputas**: Mediação administrativa para conflitos.

---

## ✅ Concluído Recentemente

*   **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente.
*   **Sprint 10**: Módulo de Ratings, Moderação de Conteúdo, Login Social Instagram (#141), Alinhamento de Realms Keycloak, Infra CI/CD (OpenAPI gating) e Documentação (coleções Bruno).
*   **Sprint 9**: Estabilização global, Módulo de Comunicações (Infra), Resiliência (`CancellationToken`) e Localização Backend (.resx).
*   **Sprint 8D/8E**: Migração completa do Admin Portal para React e Testes E2E com Playwright.

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
