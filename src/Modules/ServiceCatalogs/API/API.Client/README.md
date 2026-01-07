# MeAjudaAi ServiceCatalogs API Client

Coleção Bruno para gerenciamento do catálogo de serviços.

## Endpoints

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/service-catalogs/categories` | Criar categoria | AdminOnly |
| GET | `/api/v1/service-catalogs/categories` | Listar categorias | AllowAnonymous |
| POST | `/api/v1/service-catalogs/services` | Criar serviço | AdminOnly |
| GET | `/api/v1/service-catalogs/services` | Listar serviços | AllowAnonymous |
| GET | `/api/v1/service-catalogs/services/{id}` | Obter serviço por ID | AllowAnonymous |
| GET | `/api/v1/service-catalogs/services/category/{id}` | Serviços por categoria | AllowAnonymous |
