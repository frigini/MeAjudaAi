# MeAjudaAi Payments API Client

Esta coleção do Bruno contém os endpoints para o módulo de pagamentos e assinaturas.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token.
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/payments/subscriptions` | Criar uma assinatura |
| GET | `/api/v1/payments/billing-portal` | Obter link do portal de cobrança |
| POST | `/api/payments/webhooks/stripe` | Endpoint de webhook do Stripe para receber eventos de pagamento e assinatura |
