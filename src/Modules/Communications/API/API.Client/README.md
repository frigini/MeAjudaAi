# MeAjudaAi Communications API Client

Esta coleção do Bruno contém os endpoints para o módulo de comunicações.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token de Admin.
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/communications/logs` | Listar logs de comunicações |
| GET | `/api/v1/communications/templates` | Listar templates de email |
