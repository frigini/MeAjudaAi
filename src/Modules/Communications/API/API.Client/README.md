# MeAjudaAi Communications API Client

Esta coleção do Bruno contém todos os endpoints do módulo de comunicações (templates de e-mail e logs).

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
├── Public/                      # Endpoints públicos
│   ├── GetCommunicationLogs.bru
│   └── GetEmailTemplates.bru
└── Admin/                       # Endpoints admin
    ├── CreateEmailTemplate.bru
    ├── UpdateEmailTemplate.bru
    ├── ActivateEmailTemplate.bru
    └── DeactivateEmailTemplate.bru
```

## 📋 Endpoints Disponíveis

### Public

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/api/v1/communications/logs` | Listar logs de comunicações | Admin |
| GET | `/api/v1/communications/templates` | Listar templates de email | Admin |

### Admin - Email Templates

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/v1/communications/templates` | Criar novo template de e-mail | Admin |
| PUT | `/api/v1/communications/templates/{id}` | Atualizar template existente | Admin |
| PATCH | `/api/v1/communications/templates/{id}/activate` | Ativar template | Admin |
| PATCH | `/api/v1/communications/templates/{id}/deactivate` | Desativar template | Admin |

## 🔒 Autenticação

Todos os endpoints requerem:
- **Bearer Token** válido (JWT)
- **Role**: `Admin`

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

```text
baseUrl = http://localhost:5000
accessToken = [AUTO-SET by shared setup]
templateId = [CONFIGURE_AQUI após criar template]
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

#### a) Criar Template
- Execute `Admin/CreateEmailTemplate.bru`
- Copie o `id` retornado → configure `{{templateId}}`

#### b) Atualizar Template
- Execute `Admin/UpdateEmailTemplate.bru`
- Valide: 204 No Content

#### c) Desativar Template
- Execute `Admin/DeactivateEmailTemplate.bru`
- Valide: 204 No Content

#### d) Ativar Template
- Execute `Admin/ActivateEmailTemplate.bru`
- Valide: 204 No Content

#### e) Listar Templates
- Execute `Public/GetEmailTemplates.bru`
- Valide: template criado aparece na lista

## 📊 Status Codes

| Cenário | Método | Esperado |
|---------|--------|----------|
| Listar templates | GET | 200 OK |
| Criar template válido | POST | 201 Created |
| Atualizar template existente | PUT | 204 No Content |
| Ativar template | PATCH | 204 No Content |
| Desativar template | PATCH | 204 No Content |
| Operação sem token | ANY | 401 Unauthorized |
| Operação sem role Admin | ANY | 403 Forbidden |

## 🚨 Troubleshooting

### 401 Unauthorized
- Execute `Setup/SetupGetKeycloakToken.bru` primeiro
- Token pode ter expirado → obter novo token

### 403 Forbidden
- Usuário não possui role `Admin`
- Use credenciais de admin no Keycloak

### 404 Not Found
- Verifique se `{{templateId}}` está correto
