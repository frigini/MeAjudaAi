# MeAjudaAi Documents API Client

Esta coleção do Bruno contém todos os endpoints do módulo de documentos da aplicação MeAjudaAi.

## 📁 Estrutura da Coleção

```yaml
API.Client/
├── collection.bru.example       # Template de configuração (copie para collection.bru)
├── collection.bru               # Configuração local (não versionado - criar local)
├── README.md                    # Documentação completa  
└── Admin/
    ├── UploadDocument.bru       # POST /api/v1/documents
    ├── GetDocument.bru          # GET /api/v1/documents/{id}
    ├── GetProviderDocuments.bru # GET /api/v1/documents/provider/{providerId}
    ├── VerifyDocument.bru       # POST /api/v1/documents/{id}/verify
    └── RejectDocument.bru       # POST /api/v1/documents/{id}/reject
```

**🔗 Recursos Compartilhados (em `src/Shared/API.Collections/`):**
- `Setup/SetupGetKeycloakToken.bru` - Autenticação Keycloak
- `Common/GlobalVariables.bru` - Variáveis globais  
- `Common/StandardHeaders.bru` - Headers padrão

## 🚀 Como usar esta coleção

### 1. Pré-requisitos
- [Bruno](https://www.usebruno.com/) instalado
- Aplicação MeAjudaAi rodando localmente
- Keycloak configurado e rodando
- Azure Blob Storage ou Azurite rodando

### 2. Configuração Inicial

#### ⚡ **PRIMEIRO: Execute a configuração compartilhada**
1. **Abra a Collection**: `src/Shared/API.Collections/` no Bruno.
2. **Selecione o Ambiente**: `Local`.
3. **Execute**: `Setup/SetupGetKeycloakToken.bru` para autenticar.
4. **Resultado**: O token de acesso será salvo no ambiente compartilhado.

#### ⚡ **Configure esta Collection**
1. **Selecione o MESMO Ambiente**: Certifique-se de que esta coleção também está usando o ambiente `Local`.
2. **Variáveis herdadas**: `accessToken` e `baseUrl` serão lidos automaticamente do ambiente.

#### Iniciar a aplicação:
```bash
# Na raiz do projeto
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/v1/documents` | Upload de documento | SelfOrAdmin |
| GET | `/api/v1/documents/{id}` | Buscar documento por ID | SelfOrAdmin |
| GET | `/api/v1/documents/provider/{providerId}` | Listar documentos do prestador | SelfOrAdmin |
| POST | `/api/v1/documents/{id}/verify` | Verificar documento | AdminOnly |
| POST | `/api/v1/documents/{id}/reject` | Rejeitar documento | AdminOnly |

## 🔒 Políticas de Autorização

- **SelfOrAdmin**: Prestador pode acessar próprios documentos OU admin acessa qualquer
- **AdminOnly**: Apenas administradores

## 📄 Tipos de Documento Suportados

- **IdentityDocument**: RG, CNH, Passaporte
- **ProofOfResidence**: Conta de luz, água, telefone
- **ProfessionalLicense**: Registro profissional (CREA, CRM, etc.)
- **BusinessLicense**: Alvará de funcionamento, contrato social

## 📊 Status de Documento

- **Uploaded**: Documento foi enviado
- **PendingVerification**: Aguardando verificação manual
- **Verified**: Documento verificado e aprovado
- **Rejected**: Documento rejeitado (motivo obrigatório)
- **Failed**: Falha no processo de verificação

## 🔧 Variáveis da Collection

```yaml
baseUrl: http://localhost:5000
accessToken: [AUTO-SET by shared setup]
providerId: [CONFIGURE_AQUI]
documentId: [CONFIGURE_AQUI após upload]
```

## 🚨 Troubleshooting

### Erro 401 (Unauthorized)
- Execute `src/Shared/API.Collections/Setup/SetupGetKeycloakToken.bru` primeiro
- Confirme se o token não expirou

### Erro 403 (Forbidden)
- Verifique se é o próprio prestador acessando seus documentos
- Para endpoints AdminOnly, use token de administrador

### Erro 400 (Validation Error)
- Verifique se arquivo está em formato válido (PDF, JPG, PNG)
- Confirme se tamanho do arquivo não excede limite (5MB)
- Valide se DocumentType é um dos tipos suportados

### Erro 500 (Azurite Connection)
- Confirme se Azurite está rodando (Docker ou localmente)
- Verifique connection string no appsettings.json
- Execute health check para validar blob storage

---

**📝 Última atualização**: Junho 4, 2026  
**🏗️ Versão da API**: v1  
**🔧 Bruno Version**: Compatível com versões recentes
