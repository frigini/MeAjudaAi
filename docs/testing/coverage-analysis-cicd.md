# 📊 Análise de Code Coverage: CI/CD vs Local

**Data:** 2024-11-28  
**Branch:** `improve-tests-coverage`  
**Coverage Pipeline:** 35.11%  
**Coverage Local:** 21%  
**Discrepância:** +14.11 pontos percentuais

---

## 🔍 Investigação da Discrepância

### 1. Testes Executados na Pipeline (ci-cd.yml)

A pipeline executa **5 suítes de testes**:

```yaml
# Linha 87-105: ci-cd.yml
dotnet test tests/MeAjudaAi.Shared.Tests/MeAjudaAi.Shared.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Shared

dotnet test tests/MeAjudaAi.Architecture.Tests/MeAjudaAi.Architecture.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Architecture

dotnet test tests/MeAjudaAi.Integration.Tests/MeAjudaAi.Integration.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Integration

dotnet test src/Modules/Users/Tests/MeAjudaAi.Modules.Users.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Users

dotnet test tests/MeAjudaAi.E2E.Tests/MeAjudaAi.E2E.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/E2E
```

**Total:** 5 projetos de teste

### 2. Infraestrutura da Pipeline

#### PostgreSQL Service Container
```yaml
services:
  postgres:
    image: postgis/postgis:16-3.4
    env:
      POSTGRES_PASSWORD: test123
      POSTGRES_USER: postgres
      POSTGRES_DB: meajudaai_test
    options: >-
      --health-cmd pg_isready
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
    ports:
      - 5432:5432
```

**Diferença crítica:**
- ✅ **Pipeline:** PostgreSQL roda como GitHub Actions service container (sempre disponível)
- ❌ **Local:** TestContainers precisa de Docker Desktop rodando (frequentemente com problemas)

#### E2E Tests na Pipeline

**DESCOBERTA IMPORTANTE:** E2E tests **RODAM NA PIPELINE** e provavelmente **PASSAM**!

**Por que funcionam na pipeline mas falham local?**

1. **Docker-in-Docker:** GitHub Actions runner já tem Docker disponível
2. **TestContainers:** Consegue criar containers sem problemas
3. **Rede:** Sem firewall/proxy bloqueando
4. **Recursos:** Runner tem CPU/RAM suficientes

**Por que falham localmente?**

```
Docker API responded with status code=InternalServerError
Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'
```

1. **Docker Desktop com problemas:** Serviço não está respondendo
2. **Named pipe bloqueado:** `docker_engine` inacessível
3. **Possível:** Docker Desktop precisa reiniciar ou WSL2 backend com problemas

### 3. Geração do Coverage Report

```yaml
# Linha 112-120: ci-cd.yml
- name: Generate Code Coverage Report
  run: |
    reportgenerator \
      -reports:"TestResults/**/coverage.cobertura.xml" \
      -targetdir:"TestResults/Coverage" \
      -reporttypes:"Html;Cobertura;JsonSummary" \
      -assemblyfilters:"-*.Tests*" \
      -classfilters:"-*.Migrations*"
```

**Agregação:** Combina coverage de **todos os 5 projetos**

**Filtros aplicados:**
- ❌ Exclui assemblies `*.Tests*`
- ❌ Exclui classes `*.Migrations*`

### 4. Comparação: Pipeline vs Local

| Aspecto | Pipeline (35.11%) | Local (21%) |
|---------|------------------|-------------|
| **Shared.Tests** | ✅ Roda | ✅ Roda |
| **Architecture.Tests** | ✅ Roda | ✅ Roda |
| **Integration.Tests** | ✅ Roda | ✅ Roda |
| **Users.Tests** | ✅ Roda | ✅ Roda |
| **E2E.Tests** | ✅ Roda (76 testes) | ❌ Falha (Docker) |
| **Documents.Tests** | ❌ Não roda | ✅ Roda |
| **Providers.Tests** | ❌ Não roda | ✅ Roda |
| **Infraestrutura** | GitHub Actions (Docker nativo) | Docker Desktop (com problemas) |
| **Coverage Tool** | XPlat Code Coverage + ReportGenerator | XPlat Code Coverage + ReportGenerator |

**Conclusão da Discrepância:**

A diferença de **+14pp** vem principalmente de:
1. **E2E Tests rodando na pipeline** (+10-12pp estimado)
2. **Possível**: Pipeline roda outros módulos não incluídos no teste local

---

## 📈 Análise do Coverage Atual (35.11%)

### Status por Layer (Estimado)

| Layer | Coverage Estimado | Status |
|-------|------------------|--------|
| **API** | ~0% | ❌ Não testado (apenas via E2E) |
| **Application** | ~40-50% | ⚠️ Handlers parcialmente testados |
| **Domain** | ~30-40% | ⚠️ Entities/VOs com gaps |
| **Infrastructure** | ~20-30% | ❌ Baixo (repositories, external services) |

### Threshold Configuration

```yaml
# pr-validation.yml linha 509
thresholds: '70 85'
```

- ⚠️ **Warning:** 70% (atual: 35.11% ❌)
- ✅ **Good:** 85%
- **Gap:** -34.89 pontos percentuais

---

## 🎯 Recomendações para Aumentar Coverage

### Fase 1: Quick Wins (35% → 50%) - 1 sprint

#### 1.1 Application Layer - Commands/Queries sem testes

**Módulos prioritários:**
- `Documents.Application` (39% coverage)
- `Providers.Application` (47% coverage)
- `ServiceCatalogs.Application` (precisa análise)

**Ação:**
```bash
# Identificar handlers sem testes
grep -r "class.*CommandHandler\|class.*QueryHandler" src/Modules/*/Application/ \
  | while read file; do
      handler=$(basename "$file" .cs)
      test_file="Tests/${handler}Tests.cs"
      [ ! -f "$test_file" ] && echo "❌ Missing: $test_file"
    done
```

**Estimativa:** +10-15pp coverage

#### 1.2 Domain Layer - Value Objects

**Foco:**
- `DocumentType` (enum/value object)
- `Address` (value object com validações)
- `CPF/CNPJ` (value objects)
- `Email`, `PhoneNumber`

**Exemplo de teste:**
```csharp
public class AddressTests
{
    [Theory]
    [InlineData("", "City", "State", "12345")] // Invalid: empty street
    [InlineData("Street", "", "State", "12345")] // Invalid: empty city
    public void Create_WithInvalidData_ShouldThrowDomainException(
        string street, string city, string state, string zipCode)
    {
        // Act & Assert
        var act = () => Address.Create(street, city, state, zipCode);
        act.Should().Throw<DomainException>();
    }
}
```

**Estimativa:** +5-8pp coverage

#### 1.3 Validators - FluentValidation

**Ação:**
```bash
# Identificar validators sem testes
find src/Modules/*/Application -name "*Validator.cs" \
  | while read validator; do
      test="${validator/Application/Tests}"
      test="${test/.cs/Tests.cs}"
      [ ! -f "$test" ] && echo "❌ Missing: $test"
    done
```

**Estimativa:** +5pp coverage

**Total Fase 1:** +20-28pp → **55-63% coverage**

---

### Fase 2: Coverage Profundo (50% → 70%) - 1-2 sprints

#### 2.1 Infrastructure Layer

**Foco:**
- Repository implementations (mock DbContext)
- External service clients (WireMock)
- Event handlers (domain events)

**Exemplo:**
```csharp
public class ProviderRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingProvider_ReturnsProvider()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ProvidersDbContext(options);
        var provider = ProviderFactory.CreateValid();
        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        var repository = new ProviderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
    }
}
```

**Estimativa:** +8-12pp coverage

#### 2.2 Integration Tests - Scenarios Complexos

**Adicionar:**
- Testes de comunicação entre módulos
- Domain events propagation
- Transaction rollback scenarios

**Estimativa:** +5-7pp coverage

**Total Fase 2:** +13-19pp → **68-82% coverage**

---

## 🚨 Issues Identificados

### 1. Docker Desktop Local

**Problema:**
```
Docker API responded with status code=InternalServerError
Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'
```

**Soluções:**

#### Opção A: Reiniciar Docker Desktop
```powershell
# PowerShell como Admin
Restart-Service -Name "com.docker.service" -Force
```

#### Opção B: Reiniciar WSL2 (se usar WSL2 backend)
```powershell
wsl --shutdown
# Reabrir Docker Desktop
```

#### Opção C: Reinstalar Docker Desktop
- Desinstalar completamente
- Baixar última versão: https://www.docker.com/products/docker-desktop/
- Instalar e configurar

### 2. E2E Tests Dependem de Docker

**Problema:** Se Docker falha, 76 testes E2E falham (100% failure rate)

**Solução Implementada:**
- ✅ `TestContainerFixture` com retry logic
- ✅ Timeouts aumentados (1min → 5min)
- ✅ IClassFixture para compartilhar containers

**Próximos passos:**
- [ ] Validar que fixture funciona quando Docker estiver ok
- [ ] Migrar todas 19 classes de teste para IClassFixture
- [ ] Adicionar health checks no InitializeAsync

### 3. Módulos Faltando Coverage na Pipeline

**Observação:** Pipeline não roda testes de:
- `Documents.Tests`
- `Providers.Tests`
- `SearchProviders.Tests`
- `ServiceCatalogs.Tests`

**Recomendação:** Adicionar ao workflow

```yaml
# Adicionar em ci-cd.yml após linha 100
dotnet test src/Modules/Documents/Tests/MeAjudaAi.Modules.Documents.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Documents

dotnet test src/Modules/Providers/Tests/MeAjudaAi.Modules.Providers.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/Providers

dotnet test src/Modules/ServiceCatalogs/Tests/MeAjudaAi.Modules.ServiceCatalogs.Tests.csproj \
  --configuration Release --no-build --verbosity normal \
  --collect:"XPlat Code Coverage" --results-directory TestResults/ServiceCatalogs
```

**Impacto estimado:** +5-8pp coverage na pipeline

---

## 📋 Action Items

### Imediato (Esta Sprint)

- [x] Analisar workflow CI/CD ✅
- [ ] Corrigir Docker Desktop local
- [ ] Validar TestContainerFixture funciona
- [ ] Adicionar módulos faltantes ao workflow
- [ ] Commitar TestContainerFixture e InfrastructureHealthTests migrado

### Próxima Sprint (Coverage 35% → 55%)

- [ ] Adicionar testes para Commands/Queries handlers sem coverage
- [ ] Adicionar testes para Value Objects (Domain)
- [ ] Adicionar testes para Validators (FluentValidation)
- [ ] Configurar quality gate no workflow (fail se <60%)

### Sprint Futuro (Coverage 55% → 70%)

- [ ] Testes de Infrastructure layer (repositories, external services)
- [ ] Integration tests complexos (módulos comunicando)
- [ ] Migrar todas classes E2E para IClassFixture
- [ ] Implementar BDD com SpecFlow (opcional)

---

## 📊 Métricas de Progresso

### Baseline
- **Data:** 2024-11-28
- **Coverage Pipeline:** 35.11%
- **Coverage Local:** 21% (com E2E falhando)
- **Threshold Warning:** 70%
- **Gap:** -34.89pp

### Meta Sprint 1
- **Target:** 55% (+19.89pp)
- **Estratégia:** Quick wins (Handlers, VOs, Validators)

### Meta Sprint 2
- **Target:** 70% (+15pp)
- **Estratégia:** Infrastructure, Integration complexa

### Meta Final (Ideal)
- **Target:** 85%
- **Estratégia:** Coverage comprehensivo + BDD

---

## 🔗 Referências

- [Workflow CI/CD](.github/workflows/ci-cd.yml)
- [Workflow PR Validation](.github/workflows/pr-validation.yml)
- [E2E Architecture Analysis](./e2e-architecture-analysis.md)
- [TestContainerFixture](../../tests/MeAjudaAi.E2E.Tests/Base/TestContainerFixture.cs)
