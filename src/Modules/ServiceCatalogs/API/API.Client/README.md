# MeAjudaAi ServiceCatalogs API Client

Coleção Bruno para gerenciamento do catálogo de serviços.

## Endpoints

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/catalogs/categories` | Criar categoria | AdminOnly |
| GET | `/api/v1/catalogs/categories` | Listar categorias | AllowAnonymous |
| POST | `/api/v1/catalogs/services` | Criar serviço | AdminOnly |
| GET | `/api/v1/catalogs/services` | Listar serviços | AllowAnonymous |
| GET | `/api/v1/catalogs/services/category/{id}` | Serviços por categoria | AllowAnonymous |
