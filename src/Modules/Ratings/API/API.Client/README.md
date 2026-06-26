# MeAjudaAi Ratings API Client

Esta coleção do Bruno contém os endpoints para o módulo de avaliações.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token.
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📋 Endpoints Disponíveis

### Públicos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/ratings` | Criar uma avaliação (autenticado) |
| GET | `/api/v1/ratings/{id}` | Buscar avaliação por ID (somente aprovadas) |
| GET | `/api/v1/ratings/provider/{providerId}` | Listar avaliações aprovadas de um prestador (com paginação) |

### Admin

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/ratings/{id}/status` | Consultar status da moderação |

## 📖 Parâmetros de Paginação

O endpoint de listagem por prestador suporta paginação via query string:

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|--------|-----------|
| `page` | int | 1 | Número da página |
| `pageSize` | int | 20 | Itens por página (máx: 100) |

**Exemplo**: `GET /api/v1/ratings/provider/{providerId}?page=2&pageSize=10`
