# AllowedCities Admin API - Bruno Collections

Coleção de requests Bruno para testar os endpoints Admin de gerenciamento de cidades permitidas (Geographic Restrictions).

## 📋 Endpoints Disponíveis

| Request | Método | Endpoint | Descrição |
|---------|--------|----------|-----------|
| Get All Allowed Cities | GET | `/api/v1/locations/admin/allowed-cities` | Lista todas as cidades permitidas |
| Get Allowed City By Id | GET | `/api/v1/locations/admin/allowed-cities/{id}` | Busca cidade específica por ID |
| Create Allowed City | POST | `/api/v1/locations/admin/allowed-cities` | Cria nova cidade permitida |
| Update Allowed City | PUT | `/api/v1/locations/admin/allowed-cities/{id}` | Atualiza cidade existente |
| Delete Allowed City | DELETE | `/api/v1/locations/admin/allowed-cities/{id}` | Remove cidade (soft delete) |

## 🔐 Autenticação

Todos os endpoints requerem:
- **Bearer Token** válido (JWT)
- **Role**: `Admin`

### Obter Token

Use a collection `Setup/SetupGetKeycloakToken.bru` para obter um token de admin:

```json
POST {{keycloakUrl}}/realms/{{realmName}}/protocol/openid-connect/token
Body:
{
  "grant_type": "password",
  "client_id": "meajudaai-api",
  "username": "admin@meajudaai.com",
  "password": "admin123"
}
```

Copie o `access_token` e configure na variável `{{accessToken}}`.

## 🌐 Variáveis de Ambiente

Configure as seguintes variáveis no Bruno:

### Development (Local)
```
baseUrl = http://localhost:5000
keycloakUrl = http://localhost:8080
realmName = meajudaai
accessToken = <seu-token-aqui>
```

### Staging/Production
```
baseUrl = https://api-staging.meajudaai.com
keycloakUrl = https://auth-staging.meajudaai.com
realmName = meajudaai
accessToken = <seu-token-aqui>
```

## 🧪 Fluxo de Teste Sugerido

### 1. Setup Inicial
```bash
# Iniciar aplicação localmente
dotnet run --project src/Aspire/MeAjudaAi.AppHost

# Aguardar API estar disponível
curl http://localhost:5000/health
```

### 2. Autenticação
- Execute `Setup/SetupGetKeycloakToken.bru`
- Copie o `access_token` retornado
- Configure variável `{{accessToken}}` no Bruno

### 3. Testes CRUD Completo

#### a) Criar Cidade
- Execute `CreateAllowedCity.bru`
- Body exemplo:
  ```json
  {
    "cityName": "São Paulo",
    "stateSigla": "SP",
    "ibgeCode": "3550308"
  }
  ```
- Copie o `id` retornado → configure `{{allowedCityId}}`

#### b) Buscar por ID
- Execute `GetAllowedCityById.bru`
- Valide: cidade retornada com dados corretos

#### c) Listar Todas
- Execute `GetAllAllowedCities.bru`
- Valide: cidade criada aparece na lista

#### d) Atualizar
- Execute `UpdateAllowedCity.bru`
- Body exemplo:
  ```json
  {
    "cityName": "São Paulo",
    "stateSigla": "SP",
    "ibgeCode": "3550308",
    "isActive": false
  }
  ```
- Valide: 204 No Content

#### e) Deletar
- Execute `DeleteAllowedCity.bru`
- Valide: 204 No Content
- Execute `GetAllowedCityById.bru` novamente → deve retornar 404

## ✅ Validações de Status Codes

| Cenário | Método | Esperado |
|---------|--------|----------|
| Listar cidades (sucesso) | GET | 200 OK |
| Buscar cidade existente | GET | 200 OK |
| Buscar cidade inexistente | GET | 404 Not Found |
| Criar cidade válida | POST | 201 Created |
| Criar cidade duplicada | POST | 400 Bad Request |
| Atualizar cidade existente | PUT | 204 No Content |
| Atualizar cidade inexistente | PUT | 404 Not Found |
| Atualizar com duplicação | PUT | 400 Bad Request |
| Deletar cidade existente | DELETE | 204 No Content |
| Deletar cidade inexistente | DELETE | 404 Not Found |
| Qualquer operação sem token | ANY | 401 Unauthorized |
| Qualquer operação sem role Admin | ANY | 403 Forbidden |

## 🐛 Troubleshooting

### 401 Unauthorized
- Verifique se `{{accessToken}}` está configurado
- Token pode ter expirado (validade: 5 minutos) → obter novo token

### 403 Forbidden
- Usuário não possui role `Admin`
- Use credenciais de admin no Keycloak

### 404 Not Found
- Verifique se `{{allowedCityId}}` está correto
- Cidade pode ter sido deletada

### 400 Bad Request - Cidade Duplicada
- Já existe cidade com mesmo `cityName` + `stateSigla`
- Use nomes diferentes ou DELETE a cidade existente primeiro

## 📚 Recursos Adicionais

- **Swagger UI**: `http://localhost:5000/swagger`
- **Architecture Docs**: `docs/architecture.md`
- **API Spec**: `api/api-spec.json`
- **E2E Tests**: `tests/MeAjudaAi.E2E.Tests/Modules/Locations/AllowedCitiesEndToEndTests.cs`

## 🔗 Links Relacionados

- [Locations Module Documentation](../../../../docs/modules/locations.md)
- [Geographic Restriction Architecture](../../../../docs/architecture.md#geographic-restriction)
- [Sprint 3 Parte 2 Roadmap](../../../../docs/roadmap.md#sprint-3-parte-2)
