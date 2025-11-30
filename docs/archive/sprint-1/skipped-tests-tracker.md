# 🔍 Rastreamento de Testes Skipped

**Última Atualização**: 21 Nov 2025  
**Status**: 12 testes skipped em 4 categorias  
**Meta**: Resolver todos até Sprint 2

> **Nota**: Este documento de arquivo contém referências a arquivos da Sprint 1 que foram reorganizados ou removidos. Para informações atualizadas sobre testes, consulte [Guia de Testes](../../testing/).

---

## 📊 Resumo Executivo

| Categoria | Quantidade | Prioridade | Sprint Alvo | Status |
|-----------|-----------|------------|-------------|---------|
| **E2E - AUTH** | 5 | 🚨 CRÍTICA | Sprint 1 (Dia 3) | ⏳ Pendente |
| **E2E - INFRA** | 2 | 🔴 ALTA | Sprint 1-2 | ⏳ Pendente |
| **Integration - Aspire** | 3 | 🟡 MÉDIA | Sprint 2 | ⏳ Pendente |
| **Architecture - Técnico** | 1 | 🟢 BAIXA | Sprint 3+ | ⏳ Pendente |
| **Diagnostic** | 1 | ⚪ N/A | N/A (mantido disabled) | ✅ OK |

**Total**: 12 testes skipped (11 para resolver)

---

## 🚨 Categoria 1: E2E - AUTH (5 testes) - SPRINT 1 DIA 3

**Root Cause**: `SetAllowUnauthenticated(true)` em `TestContainerTestBase.cs` força todos os requests como Admin, quebrando testes de permissão.

**Solução**: Refatorar `ConfigurableTestAuthenticationHandler` para usar `UserRole.Anonymous` ao invés de forçar Admin.

### Testes Afetados:

#### 1.1 `UserWithCreatePermission_CanCreateUser`
- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Authorization/PermissionAuthorizationE2ETests.cs:57`
- **Sintoma**: Retorna 403 Forbidden ao invés de 201 Created
- **Esperado**: Usuário com permissão UsersCreate deve conseguir criar usuário
- **Fix**: Remover Skip após refactor do auth handler
- **Estimativa**: 30min (incluído no refactor geral)

#### 1.2 `UserWithoutCreatePermission_CannotCreateUser`
- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Authorization/PermissionAuthorizationE2ETests.cs:88`
- **Sintoma**: Retorna BadRequest ao invés de Forbidden
- **Esperado**: Usuário SEM permissão deve receber 403 Forbidden
- **Fix**: Remover Skip após refactor do auth handler
- **Estimativa**: 30min (incluído no refactor geral)

#### 1.3 `UserWithMultiplePermissions_HasAppropriateAccess`
- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Authorization/PermissionAuthorizationE2ETests.cs:117`
- **Sintoma**: SetAllowUnauthenticated força Admin, ignorando permissões configuradas
- **Esperado**: Usuário com permissões específicas deve ter acesso granular
- **Fix**: Remover Skip após refactor do auth handler
- **Estimativa**: 30min (incluído no refactor geral)

#### 1.4 `ApiVersioning_ShouldWork_ForDifferentModules`
- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Integration/ApiVersioningTests.cs:44`
- **Sintoma**: Retorna 403 Forbidden ao invés de OK/401/400
- **Esperado**: Diferentes versões da API devem responder corretamente
- **Fix**: Remover Skip após refactor do auth handler
- **Estimativa**: 30min (incluído no refactor geral)

#### 1.5 `CreateUser_ShouldTriggerDomainEvents`
- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Integration/ModuleIntegrationTests.cs:12`
- **Sintoma**: Retorna 403 Forbidden ao invés de 201/409
- **Esperado**: Criação de usuário deve retornar Created ou Conflict
- **Fix**: Remover Skip após refactor do auth handler
- **Estimativa**: 30min (incluído no refactor geral)

### 📝 Plano de Ação (Sprint 1 - Dia 3)

```csharp
// ANTES (TestContainerTestBase.cs)
static TestContainerTestBase()
{
    ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(true);
    // ❌ Força TODOS requests como Admin
}

// DEPOIS (Sprint 1 - Dia 3)
static TestContainerTestBase()
{
    // ✅ Permite unauthenticated mas usa Anonymous (não Admin)
    ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(
        allow: true, 
        defaultRole: UserRole.Anonymous
    );
}
```

**Checklist**:
- [ ] Adicionar parâmetro `defaultRole` em `SetAllowUnauthenticated`
- [ ] Modificar `HandleAuthenticateAsync` para respeitar role configurável
- [ ] Remover `Skip` dos 5 testes
- [ ] Rodar testes localmente (deve passar)
- [ ] Rodar testes no CI/CD (deve passar)
- [ ] Validar que outros testes não quebraram

---

## 🔴 Categoria 2: E2E - INFRA (2 testes)

### 2.1 `RequestDocumentVerification_Should_UpdateStatus` - SPRINT 2

- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/Modules/DocumentsVerificationE2ETests.cs:16`
- **Root Cause**: Azurite container não acessível do app container no CI/CD (localhost mismatch)
- **Sintoma**: Teste passa localmente mas falha no GitHub Actions
- **Prioridade**: 🔴 ALTA (bloqueia funcionalidade de upload de documentos)
- **Sprint Alvo**: Sprint 2 (após Module Integration)

**Opções de Solução**:
1. **Opção A** (Recomendada): Usar `TestContainers.Azurite` package
   - Vantagem: Gerenciamento automático de networking
   - Desvantagem: Adiciona dependência
   - Estimativa: 2h

2. **Opção B**: Configurar Docker networking manualmente
   - Vantagem: Sem dependências extras
   - Desvantagem: Configuração complexa no workflow
   - Estimativa: 4h

3. **Opção C**: Usar Azure Storage real em CI/CD
   - Vantagem: Ambiente idêntico a produção
   - Desvantagem: Custo + gestão de secrets
   - Estimativa: 3h

**Decisão**: Opção A (TestContainers.Azurite)

**Checklist Sprint 2**:
- [ ] Adicionar package `Testcontainers.Azurite`
- [ ] Refatorar `TestContainerTestBase` para incluir Azurite container
- [ ] Atualizar connection string no workflow
- [ ] Remover Azurite service do `pr-validation.yml`
- [ ] Remover Skip do teste
- [ ] Validar no CI/CD

---

### 2.2 `ModuleToModuleCommunication_ShouldWorkForDifferentConsumers` - SPRINT 1 DIA 3

- **Arquivo**: `tests/MeAjudaAi.E2E.Tests/CrossModuleCommunicationE2ETests.cs:55`
- **Tipo**: Theory (3 casos de teste)
- **Root Cause**: Race condition - usuários criados no Arrange não encontrados no Act
- **Sintoma**: Passa localmente mas falha no CI/CD (timing issue)
- **Prioridade**: 🚨 CRÍTICA (valida comunicação entre módulos)
- **Sprint Alvo**: Sprint 1 (Dia 3)

**Solução**:
```csharp
// Adicionar await delay após criação de usuários
await CreateUserAsync(userId, username, email);
await Task.Delay(100); // Workaround para garantir persistência no CI/CD
```

**Checklist Sprint 1 - Dia 3**:
- [ ] Adicionar `await Task.Delay(100)` após `CreateUserAsync`
- [ ] Investigar se TestContainers precisa de flush explícito
- [ ] Considerar usar `WaitUntilAsync` helper
- [ ] Remover Skip
- [ ] Rodar teste 10x consecutivas localmente
- [ ] Validar no CI/CD

---

## 🟡 Categoria 3: Integration - Aspire (3 testes) - SPRINT 2

**Root Cause**: HttpContext.User ou Aspire logging causando 500 Internal Server Error ao invés dos status codes esperados.

**Contexto**: E2E tests cobrem estes cenários, então não bloqueiam funcionalidade, mas indicam problema na camada de integração.

### 3.1 `GetDocumentStatus_NonExistentId_Should_ReturnNotFound`
- **Arquivo**: `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentsApiTests.cs:35`
- **Sintoma**: Retorna 500 ao invés de 404
- **Root Cause**: HttpContext.User claims precisam de investigação
- **Workaround**: E2E test cobre este cenário
- **Estimativa**: 2h

### 3.2 `GetDocumentStatus_Should_ReturnNotFound_WhenDocumentDoesNotExist`
- **Arquivo**: `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentsApiTests.cs:134`
- **Sintoma**: Retorna 500 ao invés de 404
- **Root Cause**: Aspire logging interceptando exceção
- **Workaround**: E2E test cobre este cenário
- **Estimativa**: 2h

### 3.3 `UploadDocument_Should_Return_BadRequest_WhenFileIsInvalid`
- **Arquivo**: `tests/MeAjudaAi.Integration.Tests/Modules/Documents/DocumentsApiTests.cs:205`
- **Sintoma**: Retorna 500 ao invés de 400
- **Root Cause**: Aspire logging interceptando validação
- **Workaround**: E2E test cobre este cenário
- **Estimativa**: 2h

**Plano de Ação Sprint 2**:
- [ ] Habilitar Aspire logging detalhado no ambiente de testes
- [ ] Investigar middleware pipeline (ordem de execução)
- [ ] Verificar se ExceptionHandlerMiddleware está configurado
- [ ] Adicionar logs estruturados para debugging
- [ ] Corrigir HttpContext.User claims em integration tests
- [ ] Remover Skip dos 3 testes
- [ ] Validar que retornam status codes corretos

---

## 🟢 Categoria 4: Architecture - Técnico (1 teste) - SPRINT 3+

### 4.1 `ModuleBoundaries_DbContextsShouldNotBePublic`
- **Arquivo**: `tests/MeAjudaAi.Architecture.Tests/ModuleBoundaryTests.cs:127`
- **Root Cause**: Limitação técnica do EF Core
- **Justificativa**: DbContext DEVE ser público para ferramentas de design-time (migrations, scaffolding)
- **Prioridade**: 🟢 BAIXA (não afeta funcionalidade)
- **Decisão**: Manter Skip permanentemente ou reavaliar em Sprint 3+

**Contexto**:
```csharp
// IDEAL (arquitetura limpa):
internal class UsersDbContext : DbContext { }

// REALIDADE (requerido pelo EF Core):
public class UsersDbContext : DbContext { }
// ↑ Necessário para: dotnet ef migrations add, design-time services
```

**Alternativas**:
1. Manter Skip permanentemente (recomendado)
2. Criar DbContext interno + wrapper público (overhead desnecessário)
3. Usar reflection em ferramentas de design-time (muito complexo)

**Decisão**: Aceitar como limitação técnica do framework. Manter Skip.

---

## ⚪ Categoria 5: Diagnostic (1 teste) - MANTER DISABLED

### 5.1 `ResponseFormat_Debug`
- **Arquivo**: `tests/MeAjudaAi.Integration.Tests/Modules/ServiceCatalogs/ServiceCatalogsResponseDebugTest.cs:12`
- **Tipo**: Teste diagnóstico (não é teste real)
- **Uso**: Habilitar manualmente apenas para debug
- **Ação**: Manter Skip permanentemente ✅

---

## 📈 Roadmap de Resolução

### Sprint 1 - Dia 3 (24 Nov)
**Objetivo**: Resolver 8 testes (5 AUTH + 3 RACE CONDITION)

- [ ] Refatorar `ConfigurableTestAuthenticationHandler` (4h)
- [ ] Remover Skip de 5 testes AUTH
- [ ] Adicionar retry logic em 3 testes race condition
- [ ] Validar no CI/CD
- [ ] **Meta**: 93/100 → 98/100 E2E tests passing (98.0%)

### Sprint 2 (Dec 2-6)
**Objetivo**: Resolver 4 testes (1 AZURITE + 3 ASPIRE)

- [ ] Implementar TestContainers.Azurite (2h)
- [ ] Investigar Aspire logging issues (6h)
- [ ] Remover Skip de 4 testes
- [ ] **Meta**: 98/100 → 99/100 tests passing (99.0%)

### Sprint 3+ (TBD)
**Objetivo**: Decisão final sobre DbContext visibility

- [ ] Reavaliar necessidade do teste de arquitetura
- [ ] Aceitar como limitação técnica OU implementar workaround complexo
- [ ] **Meta**: 99/100 → 100/100 tests passing (100%) ou aceitar 99%

---

## 🔄 Processo de Tracking

### Como Atualizar Este Documento:

1. **Ao descobrir novo teste skipped**:
   ```bash
   # Adicionar à categoria apropriada
   # Estimar esforço e sprint alvo
   # Atualizar resumo executivo
   ```

2. **Ao resolver teste skipped**:
   ```bash
   # Mudar status de ⏳ Pendente para ✅ Resolvido
   # Adicionar link para PR/commit
   # Atualizar métricas do resumo
   ```

3. **Ao adicionar novo Skip temporário**:
   ```bash
   # Documentar IMEDIATAMENTE neste arquivo
   # Criar issue no GitHub
   # Assignar para próximo sprint
   ```

---

## 📊 Métricas Atuais (21 Nov 2025)

### E2E Tests (100 total)
- ✅ Passing: 93 (93.0%)
- ⏭️ Skipped: 7 (7.0%)
- ❌ Failing: 0

### Integration Tests (~150 total)
- ✅ Passing: 147 (98.0%)
- ⏭️ Skipped: 3 (2.0%)
- ❌ Failing: 0

### Architecture Tests (15 total)
- ✅ Passing: 14 (93.3%)
- ⏭️ Skipped: 1 (6.7%)
- ❌ Failing: 0

### Unit Tests (296 total)
- ✅ Passing: 296 (100%)
- ⏭️ Skipped: 0
- ❌ Failing: 0

**Total Geral**: 550/562 passing (97.9%), 12 skipped, 0 failing ✅

---

## 🎯 Definição de Concluído

Um teste skipped pode ser considerado **resolvido** quando:

- [x] Skip attribute removido do código
- [x] Teste passa localmente (10 execuções consecutivas)
- [x] Teste passa no CI/CD (3 PRs consecutivos)
- [x] Root cause documentado em commit message
- [x] Code review aprovado
- [x] Este documento atualizado com status ✅

---

## 📚 Referências

> **Note**: Este é um documento arquivado do Sprint 1. As referências originais foram reorganizadas ou removidas. Para documentação atualizada, consulte:
> - [Architecture Decision Records](../../architecture.md)
> - [Testing Strategy](../../testing/test-infrastructure.md)

---

**Próxima Revisão**: 24 Nov 2025 (após Sprint 1 Dia 3)
