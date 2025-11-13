# Configuração do Módulo de Documentos

Este documento descreve as configurações necessárias para o módulo de gerenciamento de documentos.

## Azure Blob Storage

O módulo utiliza Azure Blob Storage para armazenar os documentos enviados pelos prestadores.

### Desenvolvimento Local

Para desenvolvimento local, você pode usar o **Azurite** (emulador do Azure Storage):

```bash
# Instalação global
npm install -g azurite

# Executar
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

A connection string padrão para Azurite já está configurada no `appsettings.Development.json`:
```json
"Azure": {
  "Storage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "BlobContainerName": "documents"
  }
}
```

### Azure (Produção)

Para produção, você precisa criar um **Azure Storage Account**:

1. Acesse o [Portal Azure](https://portal.azure.com)
2. Crie um novo Storage Account
3. Obtenha a connection string em: **Access keys** → **Connection string**
4. Configure via **user-secrets** ou variáveis de ambiente:

```bash
# Via user-secrets (recomendado para desenvolvimento)
dotnet user-secrets set "Azure:Storage:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"

# Via variável de ambiente
$env:Azure__Storage__ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
```

## Azure Document Intelligence

O módulo utiliza Azure AI Document Intelligence (antigo Form Recognizer) para OCR e extração de dados dos documentos brasileiros.

### Criação do Recurso

1. Acesse o [Portal Azure](https://portal.azure.com)
2. Crie um recurso **Azure AI Document Intelligence**
3. Após criação, obtenha:
   - **Endpoint**: URL do serviço (ex: `https://meajudaai-docs.cognitiveservices.azure.com/`)
   - **API Key**: Chave de acesso em **Keys and Endpoint**

### Configuração

```bash
# Via user-secrets (recomendado)
dotnet user-secrets set "Azure:DocumentIntelligence:Endpoint" "https://your-resource.cognitiveservices.azure.com/"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "your-api-key-here"

# Via variáveis de ambiente
$env:Azure__DocumentIntelligence__Endpoint = "https://your-resource.cognitiveservices.azure.com/"
$env:Azure__DocumentIntelligence__ApiKey = "your-api-key-here"
```

### Modelos Suportados

O serviço utiliza os seguintes modelos pré-treinados para documentos brasileiros:

- **RG (Carteira de Identidade)**: `prebuilt-idDocument`
- **CPF**: `prebuilt-idDocument`
- **CNH (Carteira Nacional de Habilitação)**: `prebuilt-idDocument`
- **Comprovante de Residência**: `prebuilt-document` (OCR genérico)
- **Certificados**: `prebuilt-document`

## Hangfire (Background Jobs)

O módulo utiliza Hangfire para processamento assíncrono de verificação de documentos.

### Configurações

```json
"Hangfire": {
  "DashboardEnabled": true,          // Habilita dashboard web
  "DashboardPath": "/hangfire",      // URL do dashboard
  "WorkerCount": 5,                  // Número de workers (produção)
  "PollingIntervalSeconds": 15,      // Intervalo de polling
  "RetryAttempts": 3,                // Tentativas em caso de erro
  "AutomaticRetryDelaySeconds": 60   // Delay entre tentativas
}
```

### Dashboard

Quando habilitado, o dashboard do Hangfire fica disponível em:
- **Desenvolvimento**: `http://localhost:5000/hangfire`
- **Produção**: `https://api.meajudaai.com/hangfire`

> ⚠️ **Segurança**: Em produção, adicione autenticação/autorização ao dashboard do Hangfire.

## Database

O módulo utiliza PostgreSQL com schema isolado `documents`.

### Connection String

A connection string do PostgreSQL é configurada via **Aspire** ou diretamente:

```bash
# Via user-secrets
dotnet user-secrets set "ConnectionStrings:DocumentsDb" "Host=localhost;Database=meajudaai;Username=documents_owner;Password=your-password"

# A migração cria automaticamente:
# - Schema: documents
# - Roles: documents_owner, documents_role, hangfire_role
# - Tabelas: documents, hangfire_*
```

## Verificação de Configuração

Para verificar se todas as configurações estão corretas:

```bash
# Listar user-secrets
cd src/Bootstrapper/MeAjudaAi.ApiService
dotnet user-secrets list

# Testar conexão com Azure Storage (Azurite)
# Instalar Azure Storage Explorer: https://azure.microsoft.com/features/storage-explorer/

# Testar conexão com Document Intelligence
# Use a ferramenta: https://formrecognizer.appliedai.azure.com/studio
```

## Exemplo Completo - User Secrets

```bash
cd src/Bootstrapper/MeAjudaAi.ApiService

# Azure Storage (Desenvolvimento - Azurite)
dotnet user-secrets set "Azure:Storage:ConnectionString" "UseDevelopmentStorage=true"

# Azure Storage (Produção)
dotnet user-secrets set "Azure:Storage:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=meajudaai;AccountKey=your-key;EndpointSuffix=core.windows.net"

# Azure Document Intelligence
dotnet user-secrets set "Azure:DocumentIntelligence:Endpoint" "https://meajudaai-docs.cognitiveservices.azure.com/"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "your-32-char-api-key"

# Database
dotnet user-secrets set "ConnectionStrings:DocumentsDb" "Host=localhost;Database=meajudaai;Username=documents_owner;Password=SecurePass123!"
```

## Custos Azure

### Storage Account
- **Custo**: ~R$ 0,10 por GB/mês (Hot tier)
- **Estimativa**: 1000 documentos (~500MB) = ~R$ 0,05/mês

### Document Intelligence
- **Free Tier**: 500 páginas/mês grátis
- **Custo Standard**: ~R$ 5,00 por 1000 páginas
- **Estimativa**: 500 docs/mês = ~R$ 2,50/mês (se exceder free tier)

## Troubleshooting

### Erro: "Unable to connect to Azurite"
```bash
# Verificar se Azurite está rodando
netstat -an | findstr "10000"

# Reiniciar Azurite
azurite --silent --location c:\azurite
```

### Erro: "Azure Document Intelligence unauthorized"
```bash
# Verificar API Key
dotnet user-secrets list | findstr "DocumentIntelligence"

# Regenerar chave no Portal Azure se necessário
```

### Erro: "Hangfire schema not found"
```bash
# Executar migrations
dotnet ef database update --project src/Modules/Documents/Infrastructure --context DocumentsDbContext

# Verificar se schema hangfire existe
psql -U postgres -d meajudaai -c "\dn"
```

## Referências

- [Azure Blob Storage Documentation](https://learn.microsoft.com/azure/storage/blobs/)
- [Azure Document Intelligence Documentation](https://learn.microsoft.com/azure/ai-services/document-intelligence/)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [Azurite Emulator](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
