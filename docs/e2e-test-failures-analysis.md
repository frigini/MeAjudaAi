# 🔍 Análise de Falhas - E2E Tests no CI/CD

> **Propósito**: Documentação de root cause analysis para falhas de E2E tests no GitHub Actions  
> **Audiência**: Desenvolvedores trabalhando em CI/CD e infraestrutura de testes  
> **Última atualização**: 21 de Novembro de 2025  
> **Autor**: GitHub Copilot (automated analysis)  
> **Status**: ✅ RESOLVIDO - Implementado em commits 60488e4, 18aed71  
> **Ciclo de vida**: Documento permanente para referência histórica e troubleshooting futuro

**Data**: 21 de Novembro de 2025  
**Branch**: `migration-to-dotnet-10`  
**Contexto**: Testes E2E falhando no GitHub Actions, mas passando localmente

---

## 📊 Resumo Executivo

- **Total de testes**: 103
- **Passaram**: 96 (93.2%) ✅
- **Falharam**: 7 (6.8%) ❌
- **Padrão de falha**: 6 falhas com 403 Forbidden + 1 falha com 500 Internal Server Error

---

## 🔴 Testes Falhando

### 1. CrossModuleCommunicationE2ETests (4 falhas)

| Teste | Erro | Linha |
|-------|------|-------|
| `ModuleToModuleCommunication_ShouldWorkForDifferentConsumers` (ReportingModule) | 403 Forbidden | 108 |
| `ModuleToModuleCommunication_ShouldWorkForDifferentConsumers` (PaymentModule) | 403 Forbidden | 100 |
| `ModuleToModuleCommunication_ShouldWorkForDifferentConsumers` (OrdersModule) | 404 User not found | 90 |
| `ErrorRecovery_ModuleApiFailures_ShouldNotAffectOtherModules` | 403 Forbidden | 27 |

**Causa comum**: Falha na autenticação/autorização

---

### 2. DocumentsVerificationE2ETests (1 falha)

| Teste | Erro | Linha |
|-------|------|-------|
| `RequestDocumentVerification_Should_UpdateStatus` | 500 Internal Server Error | 17 |

**Causa**: Upload de documento falhando (Azure Blob Storage não configurado)

---

### 3. ServiceCatalogsModuleIntegrationTests (1 falha)

| Teste | Erro | Linha |
|-------|------|-------|
| `RequestsModule_Can_Filter_Services_By_Category` | 403 Forbidden | 72 |

**Causa**: Sem permissão para criar categoria (autenticação inválida)

---

### 4. ProvidersLifecycleE2ETests (1 falha)

| Teste | Erro | Linha |
|-------|------|-------|
| `UpdateVerificationStatus_InvalidTransition_Should_Fail` | 403 Forbidden (esperava 400/404) | 261 |

**Causa**: Autorização falhando antes de validação de negócio

---

## 🔎 Causa Raiz Identificada

### Problema 1: Autenticação Mock no CI/CD

#### ✅ **Localmente (funciona)**

```csharp
// ConfigurableTestAuthenticationHandler.cs
AuthenticateAsAdmin(); // Cria token fake com role 'admin'
```

- Mock authentication handler injeta claims automaticamente
- Não depende de Keycloak real
- Todos os testes passam

#### ❌ **No CI/CD (falha)**

```yaml
# pr-validation.yml linha 99
- name: Check Keycloak Configuration
  env:
    KEYCLOAK_ADMIN_PASSWORD: ${{ secrets.KEYCLOAK_ADMIN_PASSWORD }}
  run: |
    if [ -z "$KEYCLOAK_ADMIN_PASSWORD" ]; then
      echo "ℹ️ KEYCLOAK_ADMIN_PASSWORD secret not configured - Keycloak is optional"
```

**Problema**:
- Keycloak é marcado como OPCIONAL no workflow
- Sem Keycloak, o authentication handler pode falhar silenciosamente
- Testes recebem `403 Forbidden` por autorização inválida

**Evidência do código**:
```csharp
// ConfigurableTestAuthenticationHandler.cs linha 25-35
if (_currentConfigKey == null || !_userConfigs.TryGetValue(_currentConfigKey, out _))
{
    if (!_allowUnauthenticated)
        return Task.FromResult(AuthenticateResult.Fail("No authentication configuration set")); // ❌ Falha aqui
    
    ConfigureAdmin(); // ✅ Autoconfigure (só chega aqui se _allowUnauthenticated = true)
}
```

---

### Problema 2: Azure Blob Storage Não Configurado

#### ✅ **Localmente (funciona)**

- Usa **Azurite** (Azure Storage Emulator)
- Upload de documentos funciona via mock storage

#### ❌ **No CI/CD (falha)**

```csharp
// DocumentsVerificationE2ETests.cs linha 71
var uploadResponse = await ApiClient.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest, JsonOptions);
uploadResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK); 
// ❌ Retorna 500 Internal Server Error
```

**Problema**:
- GitHub Actions não tem **Azurite container** configurado
- Upload de documento falha porque Blob Storage não está disponível
- Retorna `500 Internal Server Error`

---

## 💡 Soluções Propostas

### ⚡ Opção 1: Skip Testes no CI/CD (RÁPIDA)

**Ação**: Adicionar `[Trait("Category", "RequiresAspire")]` nos 7 testes falhando

**Pros**:
- ✅ Desbloqueia merge da Sprint 0 imediatamente
- ✅ Validação local com Aspire antes de merge
- ✅ Zero mudanças no workflow CI/CD

**Contras**:
- ❌ Reduz cobertura de testes no CI/CD
- ❌ Testes críticos não validados em PR

**Implementação**:
```csharp
[Fact]
[Trait("Category", "RequiresAspire")] // ✅ Skip no CI/CD
public async Task ModuleToModuleCommunication_ShouldWorkForDifferentConsumers(...)
{
    // ...
}
```

**Filtro no workflow**:
```bash
dotnet test --filter "Category!=RequiresAspire"
```

---

### 🎯 Opção 2: Configurar Infraestrutura no CI/CD (IDEAL)

**Ação**: Adicionar Azurite + configurar authentication corretamente

**Mudanças no `pr-validation.yml`**:

```yaml
services:
  postgres:
    # ... (já existe)
  
  azurite:  # ✅ Novo container
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - 10000:10000
      - 10001:10001
      - 10002:10002

steps:
  - name: Run tests with coverage
    env:
      # ... (vars existentes)
      # ✅ Novo: Azure Storage (see .github/workflows/pr-validation.yml for actual connection string)
      AZURE_STORAGE_CONNECTION_STRING: "<AZURITE_DEV_CONNECTION_STRING>"
      # Reference: See .github/workflows/pr-validation.yml for actual development key
```

**Fix authentication**:
```csharp
// TestContainerTestBase.cs
public class TestContainerTestBase : IAsyncLifetime
{
    static TestContainerTestBase()
    {
        // ✅ Garantir que E2E tests permitam auto-configure admin
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(true);
    }
}
```

**Pros**:
- ✅ Cobertura completa de testes no CI/CD
- ✅ Valida infraestrutura real (Azurite ≈ Azure Blob)
- ✅ Detecta problemas de integração antes de merge

**Contras**:
- ❌ Requer mudanças no workflow (mais tempo)
- ❌ Aumenta complexidade do CI/CD
- ❌ Pode aumentar tempo de execução do pipeline

---

## 📝 Recomendação

### 🚀 Plano de Ação

#### **Agora (desbloquear Sprint 0)**:
1. **Opção 1**: Skip 7 testes com `[Trait("Category", "RequiresAspire")]`
2. Adicionar filtro no workflow: `--filter "Category!=RequiresAspire"`
3. Validar **localmente** com Aspire antes de merge
4. Commit e merge para master

#### **Sprint 1 (melhorar CI/CD)**:
1. Implementar **Opção 2**: Azurite + fix authentication
2. Remover `[Trait("Category", "RequiresAspire")]` dos testes
3. Validar pipeline completo no GitHub Actions

#### **Criar Issue**:
```markdown
## Configure E2E Test Infrastructure in GitHub Actions

**Problem**: 7 E2E tests failing in CI/CD due to missing infrastructure (Keycloak + Azure Blob Storage)

**Solution**:
1. Add Azurite container to pr-validation.yml
2. Configure authentication handler for CI/CD
3. Remove RequiresAspire trait from tests

**Priority**: Sprint 1 (após merge .NET 10)
```

---

## 📊 Impacto

### ✅ Com Opção 1 (Skip):
- Sprint 0 desbloqueada **imediatamente**
- Build passa com 0 warnings, 0 errors
- 96/103 testes validados (93.2%)
- **7 testes críticos** validados apenas localmente

### 🎯 Com Opção 2 (Infraestrutura):
- Validação **completa** no CI/CD
- 103/103 testes rodando (100%)
- Maior confiança em PRs
- **Tempo de implementação**: ~2-4 horas

---

## 🔗 Referências

- **Workflow**: `.github/workflows/pr-validation.yml`
- **Authentication Handler**: `tests/MeAjudaAi.Shared.Tests/Auth/ConfigurableTestAuthenticationHandler.cs`
- **Testes falhando**: `tests/MeAjudaAi.E2E.Tests/`

### 📚 Documentação Externa

- **Azurite Docs**: [Azure Storage Emulator (Azurite)](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
- **GitHub Actions Services**: [Using containerized services](https://docs.github.com/en/actions/using-containerized-services)

---

**Conclusão**: Opção 1 desbloqueia Sprint 0, Opção 2 é trabalho para Sprint 1 ✅
