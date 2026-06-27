# MeAjudaAi ServiceCatalogs API Client

Coleção Bruno para gerenciamento do catálogo de serviços.

## Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token de Admin (necessário para criação/edição).
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## Estrutura

```
API.Client/
  collection.bru
  ServiceCategory/
    CreateCategory.bru
    ListCategories.bru
    GetCategoryById.bru
    UpdateCategory.bru
    ActivateCategory.bru
    DeactivateCategory.bru
    DeleteCategory.bru
  Service/
    CreateService.bru
    ListServices.bru
    GetServiceById.bru
    GetServicesByCategory.bru
    UpdateService.bru
    ChangeServiceCategory.bru
    ActivateService.bru
    DeactivateService.bru
    DeleteService.bru
    ValidateServices.bru
```

## Endpoints Disponíveis

### ServiceCategory

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/service-catalogs/categories` | Criar categoria | Admin |
| GET | `/api/v1/service-catalogs/categories` | Listar categorias | Public |
| GET | `/api/v1/service-catalogs/categories/{id}` | Obter categoria por ID | Admin |
| PUT | `/api/v1/service-catalogs/categories/{id}` | Atualizar categoria | Admin |
| POST | `/api/v1/service-catalogs/categories/{id}/activate` | Ativar categoria | Admin |
| POST | `/api/v1/service-catalogs/categories/{id}/deactivate` | Desativar categoria | Admin |
| DELETE | `/api/v1/service-catalogs/categories/{id}` | Deletar categoria | Admin |

### Service

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/service-catalogs/services` | Criar serviço | Admin |
| GET | `/api/v1/service-catalogs/services` | Listar serviços | Public |
| GET | `/api/v1/service-catalogs/services/{id}` | Obter serviço por ID | Public |
| GET | `/api/v1/service-catalogs/services/category/{categoryId}` | Serviços por categoria | Public |
| PUT | `/api/v1/service-catalogs/services/{id}` | Atualizar serviço | Admin |
| POST | `/api/v1/service-catalogs/services/{id}/change-category` | Mudar categoria do serviço | Admin |
| POST | `/api/v1/service-catalogs/services/{id}/activate` | Ativar serviço | Admin |
| POST | `/api/v1/service-catalogs/services/{id}/deactivate` | Desativar serviço | Admin |
| DELETE | `/api/v1/service-catalogs/services/{id}` | Deletar serviço | Admin |
| POST | `/api/v1/service-catalogs/services/validate` | Validar serviços | Admin |

## Fluxo de Uso

1. **ServiceCategory**: CreateCategory → ListCategories → GetCategoryById → UpdateCategory → Activate/Deactivate → DeleteCategory
2. **Service**: CreateService → ListServices → GetServiceById → UpdateService → ChangeCategory → Activate/Deactivate → DeleteService
3. **Validation**: ValidateServices
