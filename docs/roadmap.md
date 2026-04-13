# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Abril 2026)

**Sprint Atual**: 10 (Qualidade & Onboarding)
**Status**: 🔄 Em Andamento
**Meta MVP**: 12 - 16 de Maio de 2026

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🚀 Sprint 10 - Qualidade & Onboarding (12 Abr - 26 Abr 2026) [EM ANDAMENTO]

**Objetivo**: Estabelecer confiança na plataforma através de avaliações e simplificar o acesso de novos prestadores.

### 🔴 MUST-HAVE:

#### 1. 🌟 Ratings Module (Módulo de Avaliações) [EM ANDAMENTO]
*   **Arquitetura**: **Consistência Eventual**. O módulo de busca (`SearchProviders`) não fará Join com o módulo de Ratings. Sempre que um review for aprovado, um `ReviewApprovedIntegrationEvent` será disparado e o módulo de busca atualizará o campo `AverageRating` no seu próprio registro desnormalizado.
*   **Funcionalidades**:
    *   ✅ **Avaliação de Prestadores**: Clientes podem adicionar nota (1 a 5 estrelas) e comentário textual após a conclusão de um serviço.
    *   ✅ **Moderação de Conteúdo**: Filtro automático via Regex e manual para comentários que violem as regras (xingamentos, ofensas, SPAM).
    *   ✅ **Ranking de Busca**: Algoritmo de busca priorizando prestadores com melhor média e maior volume de avaliações verificadas.
*   **Schema DB**: `ratings` | **ModuleName**: `Ratings`.

#### 2. 🔑 Login Social (Instagram) - ISSUE #141 [EM ANDAMENTO]
*   **Ação**: Configuração de Identity Provider nativo do Instagram no Keycloak para permitir que prestadores usem seu perfil do Instagram para autenticação.

#### 3. 🛡️ OpenAPI Breaking Change Gating (CI) [EM ANDAMENTO]
*   **Ação**: Novo step no workflow de PR para comparar o `api-base.json` com `api-current/api-spec.json` usando `breaking-only` e `fail-on-diff` para falhar o build caso existam mudanças destrutivas na API. Corrigido para rodar em ambiente `Testing` para evitar dependências de segredos reais.

#### 4. 📋 Coleções Bruno (.bru) [EM ANDAMENTO]
*   **Ação**: Documentação técnica de 100% dos endpoints existentes em `src/Modules/*/API/API.Client/`.

---

## 💰 Sprint 11 - Monetização & Polimento (27 Abr - 11 Mai 2026) [EM PLANEJAMENTO]

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

*   **Sprint 9**: Estabilização global, Módulo de Comunicações (Infra), Resiliência (`CancellationToken`) e Localização Backend (.resx).
*   **Sprint 8D/8E**: Migração completa do Admin Portal para React e Testes E2E com Playwright.

---

## 📜 Histórico Completo
Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
