# MeAjudaAi ServiceCatalogs API Client

Coleção Bruno para gerenciamento do catálogo de serviços.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token de Admin (necessário para criação/edição).
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## Endpoints

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/service-catalogs/categories` | Criar categoria | AdminOnly |
| GET | `/api/v1/service-catalogs/categories` | Listar categorias | AllowAnonymous |
| POST | `/api/v1/service-catalogs/services` | Criar serviço | AdminOnly |
| GET | `/api/v1/service-catalogs/services` | Listar serviços | AllowAnonymous |
| GET | `/api/v1/service-catalogs/services/{id}` | Obter serviço por ID | AllowAnonymous |
| GET | `/api/v1/service-catalogs/services/category/{id}` | Serviços por categoria | AllowAnonymous |
