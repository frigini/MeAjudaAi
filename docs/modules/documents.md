# Módulo de Documentos

Este documento descreve o módulo de gerenciamento de documentos do MeAjudaAi, incluindo arquitetura, configuração e melhorias futuras.

## Visão Geral

O módulo Documents permite que prestadores de serviço façam upload de documentos necessários para validação (RG, CPF, CNH, comprovante de residência, certificados), com processamento automático via OCR e verificação assíncrona.

### Tecnologias Utilizadas

- **Azure Blob Storage**: Armazenamento de arquivos
- **Azure Document Intelligence**: OCR e extração de dados
- **Hangfire**: Processamento assíncrono de verificação
- **PostgreSQL**: Persistência com schema isolado

---

## Arquitetura

### Camadas

```
src/Modules/Documents/
├── Api/                    # Endpoints HTTP
│   ├── Endpoints/         # Minimal APIs
│   └── Extensions.cs      # Configuração do módulo
├── Application/           # Use cases (CQRS)
│   ├── Commands/         # Upload, MarkAsVerified, MarkAsRejected
│   ├── Queries/          # GetStatus, GetProviderDocuments
│   └── Jobs/             # DocumentVerificationJob (Hangfire)
├── Domain/               # Regras de negócio
│   ├── Entities/        # Document aggregate
│   ├── Enums/           # EDocumentStatus, EDocumentType
│   ├── Events/          # Domain events
│   └── Repositories/    # Interfaces
└── Infrastructure/       # Implementações técnicas
    ├── Data/            # EF Core, migrations
    └── Repositories/    # Implementação IDocumentRepository
```

### Fluxo de Processamento

```
1. Upload
   ├─> POST /api/v1/documents/upload
   ├─> UploadDocumentCommandHandler
   ├─> Azure Blob Storage (salvamento)
   ├─> PostgreSQL (metadados)
   └─> Hangfire (enfileira verificação)

2. Verificação Assíncrona
   ├─> DocumentVerificationJob (background)
   ├─> Azure Document Intelligence (OCR)
   ├─> Validação de dados extraídos
   └─> MarkAsVerified/MarkAsRejected

3. Consulta
   ├─> GET /api/v1/documents/status/{id}
   └─> GetDocumentStatusQueryHandler
```

### Tipos de Documento

| Tipo | Enum | Modelo Azure |
|------|------|--------------|
| RG | `IdentityDocument` | `prebuilt-idDocument` |
| CPF | `IdentityDocument` | `prebuilt-idDocument` |
| CNH | `DriverLicense` | `prebuilt-idDocument` |
| Comprovante de Residência | `ProofOfAddress` | `prebuilt-document` |
| Certificado | `Certificate` | `prebuilt-document` |

### Estados do Documento

```
Uploaded (1)
    ↓
PendingVerification (2)
    ↓
    ├─> Verified (3)
    ├─> Rejected (4)
    └─> Failed (5) [erro técnico]
```

---

## Configuração

### 1. Azure Blob Storage

#### Desenvolvimento Local (Azurite)

```bash
# Instalação
npm install -g azurite

# Executar
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

Configuração em `appsettings.Development.json`:
```json
{
  "Azure": {
    "Storage": {
      "ConnectionString": "UseDevelopmentStorage=true",
      "BlobContainerName": "documents"
    }
  }
}
```

#### Produção (Azure Storage Account)

```bash
# Via user-secrets (recomendado)
dotnet user-secrets set "Azure:Storage:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"

# Via variável de ambiente
$env:Azure__Storage__ConnectionString = "DefaultEndpointsProtocol=https;..."
```

**Passos no Portal Azure**:
1. Criar Storage Account
2. Obter connection string em **Access keys** → **Connection string**
3. Configurar via user-secrets ou variáveis de ambiente

### 2. Azure Document Intelligence

```bash
# User-secrets
dotnet user-secrets set "Azure:DocumentIntelligence:Endpoint" "https://your-resource.cognitiveservices.azure.com/"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "your-api-key-here"

# Variáveis de ambiente
$env:Azure__DocumentIntelligence__Endpoint = "https://your-resource.cognitiveservices.azure.com/"
$env:Azure__DocumentIntelligence__ApiKey = "your-api-key-here"
```

**Passos no Portal Azure**:
1. Criar recurso **Azure AI Document Intelligence**
2. Obter **Endpoint** e **API Key** em **Keys and Endpoint**

### 3. Hangfire (Background Jobs)

```json
{
  "Hangfire": {
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire",
    "WorkerCount": 5,
    "PollingIntervalSeconds": 15,
    "RetryAttempts": 3,
    "AutomaticRetryDelaySeconds": 60
  }
}
```

**Dashboard**: Disponível em `http://localhost:5000/hangfire` (dev) ou `https://api.meajudaai.com/hangfire` (prod)

> ⚠️ **Segurança**: Adicione autenticação ao dashboard do Hangfire em produção.

### 4. Database (PostgreSQL)

```bash
# Connection string via user-secrets
dotnet user-secrets set "ConnectionStrings:DocumentsDb" "Host=localhost;Database=meajudaai;Username=documents_owner;Password=your-password"
```

**Schema criado automaticamente**:
- Schema: `documents`
- Roles: `documents_owner`, `documents_role`, `hangfire_role`
- Tabelas: `documents`, `hangfire_*`

### Exemplo Completo - User Secrets

```bash
cd src/Bootstrapper/MeAjudaAi.ApiService

# Azure Storage (Desenvolvimento)
dotnet user-secrets set "Azure:Storage:ConnectionString" "UseDevelopmentStorage=true"

# Azure Document Intelligence
dotnet user-secrets set "Azure:DocumentIntelligence:Endpoint" "https://meajudaai-docs.cognitiveservices.azure.com/"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "your-32-char-api-key"

# Database
dotnet user-secrets set "ConnectionStrings:DocumentsDb" "Host=localhost;Database=meajudaai;Username=documents_owner;Password=SecurePass123!"
```

### Verificação de Configuração

```bash
# Listar secrets
cd src/Bootstrapper/MeAjudaAi.ApiService
dotnet user-secrets list

# Testar Azure Storage
# Use Azure Storage Explorer: https://azure.microsoft.com/features/storage-explorer/

# Testar Document Intelligence
# Use: https://formrecognizer.appliedai.azure.com/studio
```

---

## Custos Azure (Estimativa)

| Serviço | Free Tier | Custo Standard | Estimativa Mensal |
|---------|-----------|----------------|-------------------|
| **Storage Account** | 5 GB | ~R$ 0,10/GB | 1000 docs (~500MB) = ~R$ 0,05 |
| **Document Intelligence** | 500 páginas/mês | ~R$ 5,00/1000 páginas | 500 docs/mês = ~R$ 2,50 |
| **Total** | - | - | **~R$ 2,55/mês** |

---

## Troubleshooting

### Erro: "Unable to connect to Azurite"
```bash
# Verificar se está rodando
netstat -an | findstr "10000"

# Reiniciar
azurite --silent --location c:\azurite
```

### Erro: "Azure Document Intelligence unauthorized"
```bash
# Verificar API Key
dotnet user-secrets list | findstr "DocumentIntelligence"

# Regenerar no Portal Azure se necessário
```

### Erro: "Hangfire schema not found"
```bash
# Executar migrations
dotnet ef database update --project src/Modules/Documents/Infrastructure --context DocumentsDbContext

# Verificar schema
psql -U postgres -d meajudaai -c "\dn"
```

---

## Melhorias Futuras

### 1. Type-Safety no Repositório

**Sugestão**: Usar `DocumentId` ao invés de `Guid` nas assinaturas:

```csharp
// Atual
Task<Document?> GetByIdAsync(Guid id, ...);

// Proposto
Task<Document?> GetByIdAsync(DocumentId id, ...);
```

**Status**: ⏸️ Adiado para v2

**Razão**: Manter consistência com outros módulos (Users, Providers usam Guid). Revisar em refatoração cross-module.

---

### 2. IDateTimeProvider no Domain

**Sugestão**: Injetar `IDateTimeProvider` ao invés de `DateTime.UtcNow`:

```csharp
// Atual
public static Document Create(...) 
{
    UploadedAt = DateTime.UtcNow
}

// Proposto
public static Document Create(..., IDateTimeProvider dateTimeProvider)
```

**Status**: ⏸️ Adiado para v2

**Razão**: 
- Viola princípios DDD (agregados devem ser self-contained)
- Testes atuais já validam timestamps com tolerância (`BeCloseTo`)
- Alternativa: usar factories se necessário

---

### 3. Domain Event para Falhas

**Sugestão**: Adicionar `DocumentFailedDomainEvent` e separar `FailureReason` de `RejectionReason`.

**Status**: ⏸️ Adiado para v2

**Razão**: MVP não requer rastreamento detalhado de falhas técnicas. Implementar quando houver dashboard de monitoramento ou SLA tracking.

---

### 4. Azurite em Testes de Integração

**Status**: ✅ IMPLEMENTADO (19 Dez 2025)

**Implementação**:
- `SimpleDatabaseFixture` agora cria container Azurite (3.33.0) em paralelo com PostgreSQL
- `ApiTestBase` configura `Azure:Storage:ConnectionString` com Azurite
- `AddDocumentsTestServices(useAzurite: true)` permite uso de AzureBlobStorageService real
- Testes de integração agora usam blob storage determinístico (não mock)

**Benefícios**:
- ✅ Testes determinísticos de upload/download/SAS URL generation
- ✅ Validação completa do fluxo de blob storage em CI/CD
- ✅ Elimina diferenças de comportamento entre mock e Azure real
- ✅ 9 testes de integração passando com Azurite

---

### 5. Input Guards no Agregado

**Sugestão**: Adicionar validações no factory method `Document.Create()`.

**Status**: ❌ Não implementar

**Razão**: Validação já ocorre em `UploadDocumentCommandHandler` (Application layer). Command validation é responsabilidade da camada de aplicação (padrão CQRS).

---

## Resumo de Status

| Melhoria | Status | Prioridade | Revisar quando |
|----------|--------|-----------|----------------|
| DocumentId no Repository | ⏸️ Adiado | Baixa | Refatoração cross-module |
| IDateTimeProvider no Domain | ⏸️ Adiado | Baixa | Time travel testing necessário |
| DocumentFailedDomainEvent | ⏸️ Adiado | Média | Dashboard de monitoramento |
| Azurite em Integration tests | ✅ Implementado | N/A | Sprint 5.5 (19 Dez 2025) |
| Input guards no agregado | ❌ Não implementar | N/A | Se factory ficar público |

---

## Referências

- [Azure Blob Storage Documentation](https://learn.microsoft.com/azure/storage/blobs/)
- [Azure Document Intelligence Documentation](https://learn.microsoft.com/azure/ai-services/document-intelligence/)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [Azurite Emulator](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)

---

**Última atualização**: 2025-11-14  
**Próxima revisão**: 2025-12-01
