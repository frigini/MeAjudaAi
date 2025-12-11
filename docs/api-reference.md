# Referência da API

## Visão Geral

A API MeAjudaAi segue os padrões REST e está documentada usando OpenAPI 3.0. Todos os endpoints requerem autenticação via JWT (exceto endpoints públicos de health check).

## Especificação OpenAPI

- **Arquivo versionado**: `api/api-spec.json` (na raiz do repositório)
- **Swagger UI (Desenvolvimento)**: `http://localhost:5001/swagger`
- **Swagger UI (Staging)**: `https://meajudaai-staging.azurewebsites.net/swagger`
- **Download runtime**: `/swagger/v1/swagger.json`

## Endpoints Principais

### 🔐 Autenticação

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `POST` | `/api/v1/auth/login` | Autenticar usuário | Público |
| `POST` | `/api/v1/auth/refresh` | Renovar token | Bearer |
| `POST` | `/api/v1/auth/logout` | Encerrar sessão | Bearer |

### 👤 Usuários

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `GET` | `/api/v1/users` | Listar usuários | Bearer | Admin |
| `GET` | `/api/v1/users/{id}` | Obter usuário | Bearer | Owner, Admin |
| `POST` | `/api/v1/users` | Criar usuário | Público | - |
| `PUT` | `/api/v1/users/{id}` | Atualizar usuário | Bearer | Owner, Admin |
| `DELETE` | `/api/v1/users/{id}` | Deletar usuário | Bearer | Owner, Admin |
| `GET` | `/api/v1/users/{id}/profile` | Perfil do usuário | Bearer | Owner, Admin |

### 🛠️ Prestadores

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `GET` | `/api/v1/providers` | Listar prestadores | Bearer | Customer, Provider, Admin |
| `GET` | `/api/v1/providers/{id}` | Obter prestador | Bearer | Customer, Provider, Admin |
| `POST` | `/api/v1/providers` | Criar prestador | Bearer | Provider, Admin |
| `PUT` | `/api/v1/providers/{id}` | Atualizar prestador | Bearer | Provider (owner), Admin |
| `DELETE` | `/api/v1/providers/{id}` | Deletar prestador | Bearer | Provider (owner), Admin |
| `GET` | `/api/v1/providers/search` | Buscar prestadores | Bearer | Customer, Admin |
| `POST` | `/api/v1/providers/{id}/services` | Adicionar serviço | Bearer | Provider (owner), Admin |

### 📄 Documentos

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `POST` | `/api/v1/documents/upload` | Upload de documento | Bearer | Customer, Provider |
| `GET` | `/api/v1/documents/{id}` | Obter documento | Bearer | Owner, Admin |
| `GET` | `/api/v1/documents/{id}/download` | Download documento | Bearer | Owner, Admin |
| `POST` | `/api/v1/documents/{id}/analyze` | Analisar documento (AI) | Bearer | Owner, Admin |
| `GET` | `/api/v1/documents/{id}/status` | Status da análise | Bearer | Owner, Admin |
| `DELETE` | `/api/v1/documents/{id}` | Deletar documento | Bearer | Owner, Admin |

### 🔍 Busca de Prestadores

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `GET` | `/api/v1/search/providers` | Buscar por critérios | Bearer | Customer, Admin |
| `GET` | `/api/v1/search/providers/nearby` | Buscar por localização | Bearer | Customer, Admin |
| `GET` | `/api/v1/search/providers/by-service` | Buscar por serviço | Bearer | Customer, Admin |
| `GET` | `/api/v1/search/suggestions` | Sugestões de busca | Bearer | Customer, Admin |

### 📍 Localizações

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `GET` | `/api/v1/locations/{id}` | Obter localização | Bearer | All |
| `POST` | `/api/v1/locations/geocode` | Geocodificar endereço | Bearer | All |
| `POST` | `/api/v1/locations/reverse-geocode` | Geocodificação reversa | Bearer | All |

### 📋 Catálogo de Serviços

| Método | Endpoint | Descrição | Auth | Roles |
|--------|----------|-----------|------|-------|
| `GET` | `/api/v1/service-catalogs` | Listar catálogos | Bearer | All |
| `GET` | `/api/v1/service-catalogs/{id}` | Obter catálogo | Bearer | All |
| `POST` | `/api/v1/service-catalogs` | Criar catálogo | Bearer | Admin |
| `PUT` | `/api/v1/service-catalogs/{id}` | Atualizar catálogo | Bearer | Admin |
| `DELETE` | `/api/v1/service-catalogs/{id}` | Deletar catálogo | Bearer | Admin |

### 🏥 Health Checks

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/health` | Status geral | Público |
| `GET` | `/health/ready` | Readiness probe | Público |
| `GET` | `/health/live` | Liveness probe | Público |

## Versionamento

A API segue versionamento semântico via URL path (`/api/v1/...`).

### Estratégia de Breaking Changes

1. **Minor**: Adicionar novos endpoints ou campos opcionais → sem quebra
2. **Major**: Remover endpoints ou campos obrigatórios → nova versão (`/api/v2/...`)
3. **Deprecation**: Mínimo 6 meses de aviso antes de remover versões antigas

## Códigos de Status

| Código | Significado | Uso |
|--------|-------------|-----|
| `200` | OK | Sucesso geral |
| `201` | Created | Recurso criado |
| `204` | No Content | Sucesso sem corpo de resposta |
| `400` | Bad Request | Erro de validação |
| `401` | Unauthorized | Autenticação necessária |
| `403` | Forbidden | Sem permissão |
| `404` | Not Found | Recurso não encontrado |
| `409` | Conflict | Conflito (ex: email duplicado) |
| `422` | Unprocessable Entity | Validação de negócio |
| `500` | Internal Server Error | Erro do servidor |

## Headers Obrigatórios

```http
Authorization: Bearer <jwt_token>
Content-Type: application/json
Accept: application/json
X-Correlation-ID: <uuid>  # Opcional mas recomendado para rastreamento
```

## Rate Limiting

- **Desenvolvimento**: Sem limite
- **Staging**: 100 req/min por IP
- **Production**: 60 req/min por usuário autenticado

Headers de resposta:
```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1701234567
```

## Paginação

Endpoints de listagem suportam paginação via query parameters:

```
GET /api/v1/users?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

Resposta:
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalItems": 98
  }
}
```

## Filtros

Suporte a filtros via query string:

```
GET /api/v1/providers?city=São Paulo&serviceType=Encanador&rating>=4.0
```

## Erros

Formato padrão de erro:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Erro de validação",
    "details": [
      {
        "field": "email",
        "message": "Email inválido"
      }
    ],
    "correlationId": "123e4567-e89b-12d3-a456-426614174000"
  }
}
```

## CORS

- **Desenvolvimento**: `*` (qualquer origem)
- **Production**: Lista explícita de domínios autorizados

## Segurança

### Autenticação
- JWT com refresh token
- Tokens expiram em 15 minutos
- Refresh tokens expiram em 7 dias

### Autorização
- Role-based access control (RBAC)
- Roles: `Customer`, `Provider`, `Admin`
- Políticas definidas via `[Authorize(Policy = "...")]`

### Proteções
- ✅ HTTPS obrigatório em produção
- ✅ CORS configurado
- ✅ Rate limiting
- ✅ SQL injection (EF Core parametrizado)
- ✅ XSS (sanitização de inputs)
- ✅ CSRF tokens para forms

## Swagger UI - Funcionalidades

### Desenvolvimento Local

Acesse `http://localhost:5001/swagger` para:

- ✅ Explorar todos os endpoints interativamente
- ✅ Testar requisições diretamente no browser
- ✅ Ver schemas de request/response
- ✅ Autenticar e testar com JWT
- ✅ Download da especificação OpenAPI

### Autenticação no Swagger UI

1. Clique em **Authorize** (cadeado verde)
2. Cole seu JWT: `Bearer <seu_token_aqui>`
3. Clique em **Authorize**
4. Todos os requests usarão o token automaticamente

## Links Relacionados

- [Autenticação e Autorização](./authentication-and-authorization.md)
- [Módulos da Aplicação](./modules/users.md)
- [Guia de Desenvolvimento](./development.md)
- [CI/CD](./ci-cd.md)

## Gerando Especificação Atualizada

```bash
# Rodar aplicação localmente
dotnet run --project src/MeAjudaAi.AppHost

# Baixar spec atualizada
curl http://localhost:5001/swagger/v1/swagger.json -o api/api-spec.json

# Commit
git add api/api-spec.json
git commit -m "docs: update OpenAPI spec"
```

---

💡 **Nota**: Para detalhes de implementação de cada módulo, consulte a [documentação de módulos](./modules/users.md).
