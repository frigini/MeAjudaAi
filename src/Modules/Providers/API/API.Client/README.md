# API.Client - Módulo Providers

Esta pasta contém coleções Bruno (`.bru`) para testar os endpoints do módulo Providers.

## 📁 Estrutura

```text
API.Client/
├── collection.bru.example  # Template de variáveis (copie para collection.bru)
├── collection.bru          # Variáveis globais (não versionado - crie local)
├── README.md               # Este arquivo
│
├── ProviderAdmin/          # Endpoints administrativos
│   ├── CreateProvider.bru
│   ├── GetProviders.bru
│   ├── GetProviderById.bru
│   ├── GetProviderByUserId.bru
│   ├── GetProvidersByCity.bru
│   ├── GetProvidersByState.bru
│   ├── GetProvidersByType.bru
│   ├── GetProvidersByVerificationStatus.bru
│   ├── UpdateProviderProfile.bru
│   ├── UpdateVerificationStatus.bru
│   ├── AddDocument.bru
│   ├── RemoveDocument.bru
│   ├── DeleteProvider.bru
│   ├── RequireBasicInfoCorrection.bru
│   └── StreamProviderVerificationEvents.bru
│
├── Public/                 # Endpoints públicos/autenticados
│   ├── BecomeProvider.bru
│   ├── GetPublicProviderByIdOrSlug.bru
│   └── UpdateProviderDeviceToken.bru
│
├── Public/Me/              # Endpoints do próprio prestador
│   ├── GetMyProviderProfile.bru
│   ├── GetMyProviderStatus.bru
│   ├── UpdateMyProviderProfile.bru
│   ├── ActivateMyProviderProfile.bru
│   ├── DeactivateMyProviderProfile.bru
│   ├── DeleteMyProviderProfile.bru
│   └── UploadMyDocument.bru
│
└── ProviderServices/       # Associação de serviços
    ├── AddServiceToProvider.bru
    └── RemoveServiceFromProvider.bru
```

## 🚀 Como Usar

1. **Instale o Bruno**: [https://usebruno.com/](https://usebruno.com/)
2. **Abra a pasta** `API.Client` no Bruno.
3. **Configure o Ambiente**:
   - Utilize o arquivo de ambiente compartilhado: `src/Shared/API.Collections/environments/Local.bru`.
   - No Bruno, você pode importar este arquivo como um novo ambiente.
4. **Obtenha o Token**:
   - Abra a coleção `src/Shared/API.Collections`.
   - Execute `Setup/SetupGetKeycloakToken.bru` usando o ambiente `Local`.
5. **Execute os Endpoints**: Certifique-se de que a coleção `Providers API` está usando o mesmo ambiente `Local`.

## 🔐 Autenticação

A maioria dos endpoints requer autenticação via Bearer Token. O token é gerenciado automaticamente via variável de ambiente `accessToken` após a execução do setup compartilhado.

**Endpoints públicos (não requerem token):**
- `GetPublicProviderByIdOrSlug.bru`

## 📝 Endpoints por Categoria

### ProviderAdmin - Gestão Administrativa

| Arquivo | Método | Endpoint | Descrição |
|---------|--------|----------|-----------|
| `CreateProvider.bru` | POST | `/api/v1/providers` | Criar novo prestador |
| `GetProviders.bru` | GET | `/api/v1/providers` | Listar prestadores (paginado) |
| `GetProviderById.bru` | GET | `/api/v1/providers/{id}` | Buscar por ID |
| `GetProviderByUserId.bru` | GET | `/api/v1/providers/user/{userId}` | Buscar por ID do usuário |
| `GetProvidersByCity.bru` | GET | `/api/v1/providers/by-city/{city}` | Filtrar por cidade |
| `GetProvidersByState.bru` | GET | `/api/v1/providers/by-state/{state}` | Filtrar por estado |
| `GetProvidersByType.bru` | GET | `/api/v1/providers/by-type/{type}` | Filtrar por tipo |
| `GetProvidersByVerificationStatus.bru` | GET | `/api/v1/providers/verification-status/{status}` | Filtrar por status |
| `UpdateProviderProfile.bru` | PUT | `/api/v1/providers/{id}/profile` | Atualizar perfil |
| `UpdateVerificationStatus.bru` | PUT | `/api/v1/providers/{id}/verification-status` | Atualizar status de verificação |
| `AddDocument.bru` | POST | `/api/v1/providers/{id}/documents` | Adicionar documento |
| `RemoveDocument.bru` | DELETE | `/api/v1/providers/{id}/documents/{docId}` | Remover documento |
| `DeleteProvider.bru` | DELETE | `/api/v1/providers/{id}` | Excluir prestador |
| `RequireBasicInfoCorrection.bru` | POST | `/api/v1/providers/{id}/require-correction` | Solicitar correção |
| `StreamProviderVerificationEvents.bru` | GET | `/api/v1/providers/{id}/verification-events` | SSE events |

### Public - Endpoints Públicos/Autenticados

| Arquivo | Método | Endpoint | Descrição | Auth |
|---------|--------|----------|-----------|------|
| `BecomeProvider.bru` | POST | `/api/v1/providers/become` | Criar prestador (usuário logado) | ✅ |
| `GetPublicProviderByIdOrSlug.bru` | GET | `/api/v1/providers/public/{idOrSlug}` | Buscar perfil público | ❌ |
| `UpdateProviderDeviceToken.bru` | PUT | `/api/v1/providers/{id}/device-token` | Atualizar token push | ✅ |

### Public/Me - Perfil Próprio

| Arquivo | Método | Endpoint | Descrição |
|---------|--------|----------|-----------|
| `GetMyProviderProfile.bru` | GET | `/api/v1/providers/me` | Meu perfil |
| `GetMyProviderStatus.bru` | GET | `/api/v1/providers/me/status` | Meu status |
| `UpdateMyProviderProfile.bru` | PUT | `/api/v1/providers/me` | Atualizar meu perfil |
| `ActivateMyProviderProfile.bru` | POST | `/api/v1/providers/me/activate` | Ativar meu perfil |
| `DeactivateMyProviderProfile.bru` | POST | `/api/v1/providers/me/deactivate` | Desativar meu perfil |
| `DeleteMyProviderProfile.bru` | DELETE | `/api/v1/providers/me` | Excluir meu perfil |
| `UploadMyDocument.bru` | POST | `/api/v1/providers/me/documents` | Enviar documento |

### ProviderServices - Associação de Serviços

| Arquivo | Método | Endpoint | Descrição |
|---------|--------|----------|-----------|
| `AddServiceToProvider.bru` | POST | `/api/v1/providers/{id}/services/{serviceId}` | Adicionar serviço |
| `RemoveServiceFromProvider.bru` | DELETE | `/api/v1/providers/{id}/services/{serviceId}` | Remover serviço |

## 🏷️ Tags e Organização

- **ProviderAdmin**: Operações administrativas de prestadores
- **Public**: Endpoints públicos ou para usuários autenticados
- **Public/Me**: Endpoints específicos para o próprio prestador
- **ProviderServices**: Gerenciamento de serviços do catálogo

## 🧪 Testes

Cada endpoint inclui:
- Exemplos de request/response
- Códigos de status esperados
- Documentação dos parâmetros
- Cenários de teste comuns