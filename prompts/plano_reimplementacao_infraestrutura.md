# Plano de Reimplementação - Infraestrutura E2E e CI/CD

## Contexto

A branch `feature/sprint-13.3-refatoracao-persistencia-phase0` contém MUITO mais do que refatoração de persistência. Antes de prosseguir com as Phases 0-5, precisamos estabilizar a infraestrutura que não é relacionada à refatoração.

## Mudanças NÃO relacionadas à refatoração (reimplementar)

### 1. CI/CD

| Arquivo | Status | Prioridade |
|---------|--------|------------|
| `.github/workflows/ci-e2e.yml` | Modificado | Alta |
| `tests/sequential.runsettings` | Modificado | Alta |
| `tests/xunit.runner.json` | Modificado | Alta |

### 2. Global e Configuração

| Arquivo | Status | Prioridade |
|---------|--------|------------|
| `global.json` | Modificado | Alta |
| `Directory.Packages.props` | Modificado | Média |

### 3. Documentação

| Arquivo | Status | Prioridade |
|---------|--------|------------|
| `docs/roadmap.md` | Modificado | Baixa |
| `docs/technical-debt.md` | Modificado | Baixa |
| `prompts/plano-refatoracao-persistencia.md` | Adicionado | Alta (manter!) |

### 4. Infraestrutura E2E (CORE - crashava no CI)

| Arquivo | Descrição | Status | Prioridade |
|---------|-----------|--------|------------|
| `tests/MeAjudaAi.E2E.Tests/Base/E2EStabilityCoordinator.cs` | Coordenador de inicialização e cleanup | Novo | Alta |
| `tests/MeAjudaAi.E2E.Tests/Base/SharedTestContainers.cs` | Containers Docker compartilhados | Novo | Alta |
| `tests/MeAjudaAi.E2E.Tests/Base/TestContainerFixture.cs` | Fixture principal (modificado) | Modificado | Alta |
| `tests/MeAjudaAi.E2E.Tests/Base/TestContextAwareHandler.cs` | Injeção de headers de autenticação | Modificado | Alta |
| `tests/MeAjudaAi.E2E.Tests/Infrastructure/Mocks/MockGeocodingService.cs` | Mock de geocodificação | Novo | Alta |
| `tests/MeAjudaAi.E2E.Tests/Infrastructure/SynchronousInMemoryMessageBus.cs` | Message bus síncrono | Modificado | Média |
| `tests/MeAjudaAi.E2E.Tests/Infrastructure/ValidationStatusCodeEndToEndTests.cs` | Validação de status codes | Modificado | Baixa |
| `tests/MeAjudaAi.E2E.Tests/Base/Helpers/MigrationTestHelper.cs` | Helper de migrations | Modificado | Alta |
| `tests/MeAjudaAi.E2E.Tests/MeAjudaAi.E2E.Tests.csproj` | Projeto E2E | Modificado | Alta |
| `tests/MeAjudaAi.E2E.Tests/xunit.runner.json` | Config xunit | Modificado | Alta |
| `tests/MeAjudaAi.E2E.Tests/packages.lock.json` | Lock de packages | Modificado | Média |

### 5. Testes E2E por Módulo

| Arquivo | Prioridade |
|---------|------------|
| `tests/MeAjudaAi.E2E.Tests/Modules/Bookings/BookingsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Documents/DocumentsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Locations/LocationsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Payments/PaymentsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Providers/ProvidersEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Ratings/RatingsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/SearchProviders/SearchProvidersEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/ServiceCatalogs/ServiceCatalogsEndToEndTests.cs` | Alta |
| `tests/MeAjudaAi.E2E.Tests/Modules/Users/UsersEndToEndTests.cs` | Alta |

### 6. Serviços e Mock

| Arquivo | Prioridade |
|---------|------------|
| `src/Modules/Locations/Infrastructure/Services/NoOpGeocodingService.cs` | Média |

## O que FAZER PARTE da refatoração (NÃO mexer)

### Phase 0 - Shared

- `src/Shared/Database/IRepository.cs`
- `src/Shared/Database/IUnitOfWork.cs`
- `src/Shared/Domain/IHasDomainEvents.cs`
- `src/Shared/Database/Outbox/OutboxRepository.cs`
- `src/Shared/Database/Outbox/OutboxMessageTypes.cs`
- `src/Shared/Database/RoutingUnitOfWork.cs`
- `src/Shared/Extensions/ModuleServiceRegistrationExtensions.cs` (marcado obsoleto)
- `src/Shared/Extensions/ServiceCollectionExtensions.cs`
- `src/Shared/Database/BaseDbContext.cs` (modificado)

### Phase 1 - Locations

- `src/Modules/Locations/Application/Queries/IAllowedCityQueries.cs`
- `src/Modules/Locations/Infrastructure/Persistence/LocationsDbContext.Repositories.cs`
- `src/Modules/Locations/Infrastructure/Queries/DbContextAllowedCityQueries.cs`
- Todos os Handlers de Locations
- Todos os testes de Locations

### Phase 2 - Ratings, Documents, ServiceCatalogs

- Todos os `IXxxQueries` e `DbContextXxxQueries`
- Todos os DbContext partials
- Todos os Handlers refatorados
- Todos os testes de Handlers

---

## Ordem de Reimplementação Sugerida

### Sprint 1: Infraestrutura E2E ( Critical )

1. **E2E Test Project Base**
   - `TestContainerFixture.cs` - Refazer a fixture base
   - `E2EStabilityCoordinator.cs` - Coordenador de inicialização
   - `SharedTestContainers.cs` - Containers compartilhados
   - `MigrationTestHelper.cs` - Helper de migrations

2. **Authentication & Context**
   - `TestContextAwareHandler.cs` - Headers de autenticação
   - `MockGeocodingService.cs` - Mock de geocodificação

3. **Message Bus**
   - `SynchronousInMemoryMessageBus.cs` - Para testes E2E

4. **Test Modules**
   - Cada módulo de teste E2E Separadamente

### Sprint 2: CI/CD e Configuração

1. **GitHub Actions**
   - `.github/workflows/ci-e2e.yml` - Workflow de E2E

2. **Test Configuration**
   - `xunit.runner.json`
   - `sequential.runsettings`

3. **Global**
   - `global.json` - SDK version

---

## Problema Principal Identificado

Os testes E2E crashavam com:
```
The active test run was aborted. Reason: Test host process crashed
Data collector 'Blame' message: The specified inactivity time of 10 minutes has elapsed
```

**Causa provável:** Os containers Docker não estavam ficando prontos a tempo, ou havia race conditions no `E2EStabilityCoordinator`.

**Solução:** Reimplementar a infraestrutura E2E com:
1. Timeouts mais generosos
2. Logs de diagnóstico melhores
3. Sequencialização de inicialização
4. Melhor tratamento de erros

---

## Recomendação

Criar uma **branch separada** para estabilizar a infraestrutura E2E antes de prosseguir com a refatoração:

1. `feature/e2e-infra-stabilization` - Branch temporária para fixar E2E
2. Fazer todos os testes E2E passarem
3. Mergear na branch de refatoração
4. Prosseguir com Phases 0-5

**Alternativa:** Implementar a infraestrutura E2E diretamente na branch atual, mas com cuidado para não misturar com a refatoração.