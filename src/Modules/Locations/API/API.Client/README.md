# MeAjudaAi Locations API Client

Esta coleção do Bruno contém todos os endpoints do módulo de geolocalização (cidades permitidas e busca de localizações).

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token de Admin.
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📁 Estrutura da Coleção

```text
API.Client/
├── collection.bru.example       # Template de configuração
├── collection.bru               # Configuração local (não versionado)
├── README.md                    # Este arquivo
└── Admin/                       # Endpoints admin
    ├── CreateAllowedCity.bru
    ├── DeleteAllowedCity.bru
    ├── GetAllAllowedCities.bru
    ├── GetAllowedCityById.bru
    ├── PatchAllowedCity.bru
    ├── SearchLocations.bru
    └── UpdateAllowedCity.bru
```

## 📋 Endpoints Disponíveis

Todos os endpoints requerem permissão `LocationsManage` (Admin).

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/locations/admin/allowed-cities` | Criar nova cidade permitida | Admin |
| GET | `/api/v1/locations/admin/allowed-cities` | Listar todas as cidades permitidas | Admin |
| GET | `/api/v1/locations/admin/allowed-cities/{id}` | Buscar cidade por ID | Admin |
| PUT | `/api/v1/locations/admin/allowed-cities/{id}` | Atualizar cidade permitida | Admin |
| PATCH | `/api/v1/locations/admin/allowed-cities/{id}` | Atualizar parcialmente (Raio, Ativo) | Admin |
| DELETE | `/api/v1/locations/admin/allowed-cities/{id}` | Deletar cidade permitida | Admin |
| GET | `/api/v1/locations/search` | Buscar cidades/endereços para cadastro | Admin |

## 🔒 Autenticação

Todos os endpoints requerem:
- **Bearer Token** válido (JWT)
- **Permissão**: `LocationsManage`

### Obter Token

Use a collection `Setup/SetupGetKeycloakToken.bru`:

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

## 🔧 Variáveis da Collection

```
baseUrl = http://localhost:5000
keycloakUrl = http://localhost:8080
realm = meajudaai-realm
clientId = meajudaai-client
adminUser =
adminPassword =
accessToken =
cityId = [CONFIGURE_AQUI após criar cidade]
```

## 🧪 Fluxo de Teste Sugerido

### 1. Setup

```bash
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```

### 2. Autenticação

- Execute `Setup/SetupGetKeycloakToken.bru`
- Configure `{{accessToken}}` no Bruno

### 3. CRUD Completo

#### a) Criar Cidade
- Execute `Admin/CreateAllowedCity.bru`
- Copie o `id` retornado → configure `{{cityId}}`

#### b) Buscar por ID
- Execute `Admin/GetAllowedCityById.bru`
- Valide: cidade retornada com dados corretos

#### c) Listar Todas
- Execute `Admin/GetAllAllowedCities.bru`
- Valide: cidade criada aparece na lista

#### d) Atualizar (full)
- Execute `Admin/UpdateAllowedCity.bru`
- Valide: 204 No Content

#### e) Atualizar parcial (patch)
- Execute `Admin/PatchAllowedCity.bru`
- Valide: 200 OK

#### f) Buscar localizações
- Execute `Admin/SearchLocations.bru`
- Valide: resultados relevantes para a query

#### g) Deletar
- Execute `Admin/DeleteAllowedCity.bru`
- Valide: 200 OK
- Execute `GetAllowedCityById.bru` novamente → deve retornar 404

## 📊 Status Codes

| Cenário | Método | Esperado |
|---------|--------|----------|
| Listar cidades | GET | 200 OK |
| Buscar cidade existente | GET | 200 OK |
| Buscar cidade inexistente | GET | 404 Not Found |
| Criar cidade válida | POST | 201 Created |
| Criar cidade duplicada | POST | 400 Bad Request |
| Atualizar cidade existente | PUT | 204 No Content |
| Atualizar cidade inexistente | PUT | 404 Not Found |
| Atualizar parcialmente | PATCH | 200 OK |
| Deletar cidade existente | DELETE | 200 OK |
| Deletar cidade inexistente | DELETE | 404 Not Found |
| Operação sem token | ANY | 401 Unauthorized |
| Operação sem permissão | ANY | 403 Forbidden |

## 🚨 Troubleshooting

### 401 Unauthorized
- Execute `Setup/SetupGetKeycloakToken.bru` primeiro
- Token pode ter expirado → obter novo token

### 403 Forbidden
- Usuário não possui permissão `LocationsManage`
- Use credenciais de admin no Keycloak

### 404 Not Found
- Verifique se `{{cityId}}` está correto
- Cidade pode ter sido deletada

### 400 Bad Request - Cidade Duplicada
- Já existe cidade com mesmo `cityName` + `stateSigla`
- Use nomes diferentes ou DELETE a cidade existente primeiro
