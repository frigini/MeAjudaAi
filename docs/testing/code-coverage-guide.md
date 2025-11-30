# Code Coverage - Como Visualizar e Interpretar

## 📊 Onde Ver as Porcentagens de Coverage

### 1. **GitHub Actions - Logs do Workflow**
Nas execuções do workflow `PR Validation`, você encontrará as porcentagens em:

#### Step: "Code Coverage Summary"
```csharp
📊 Code Coverage Summary
========================
Line Coverage: 85.3%
Branch Coverage: 78.9%
```text
#### Step: "Display Coverage Percentages"  
```yaml
📊 CODE COVERAGE SUMMARY
========================

📄 Coverage file: ./coverage/users/users.opencover.xml
  📈 Line Coverage: 85.3%
  🌿 Branch Coverage: 78.9%

💡 For detailed coverage report, check the 'Code Coverage Summary' step above
🎯 Minimum thresholds: 70% (warning) / 85% (good)
```bash
### 2. **Pull Request - Comentários Automáticos**
Em cada PR, você verá um comentário automático com:

```markdown
## 📊 Code Coverage Report

| Module | Line Rate | Branch Rate | Health |
|--------|-----------|-------------|---------|
| Users  | 85.3%     | 78.9%      | ✅      |

### 🎯 Quality Gates
- ✅ **Pass**: Coverage ≥ 85%
- ⚠️ **Warning**: Coverage 70-84%  
- ❌ **Fail**: Coverage < 70%
```text
### 3. **Artifacts de Download**
Em cada execução do workflow, você pode baixar:

- **`coverage-reports`**: Arquivos XML detalhados
- **`test-results`**: Resultados TRX dos testes

## 📈 Como Interpretar as Métricas

### **Line Coverage (Cobertura de Linhas)**
- **O que é**: Porcentagem de linhas de código executadas pelos testes
- **Ideal**: ≥ 85%
- **Mínimo aceitável**: ≥ 70%
- **Exemplo**: 85.3% = 853 de 1000 linhas foram testadas

### **Branch Coverage (Cobertura de Branches)**
- **O que é**: Porcentagem de condições/branches testadas (if/else, switch)
- **Ideal**: ≥ 80%
- **Mínimo aceitável**: ≥ 65%
- **Exemplo**: 78.9% = 789 de 1000 branches foram testadas

### **Complexity (Complexidade)**
- **O que é**: Métrica de complexidade ciclomática do código
- **Ideal**: Baixa complexidade com alta cobertura
- **Uso**: Identifica métodos que precisam de refatoração

## 🎯 Thresholds Configurados

### **Limites Atuais**
```yaml
thresholds: '70 85'
```csharp
- **70%**: Limite mínimo (warning se abaixo)
- **85%**: Limite ideal (pass se acima)

### **Comportamento do Pipeline**
- **Coverage ≥ 85%**: ✅ Pipeline passa com sucesso
- **Coverage 70-84%**: ⚠️ Pipeline passa com warning
- **Coverage < 70%**: ❌ Pipeline falha (modo strict)

## 🔧 Como Melhorar o Coverage

### **1. Identificar Código Não Testado**
```bash
# Baixar artifacts de coverage
# Abrir arquivos .opencover.xml em ferramentas como:
# - Visual Studio Code com extensão Coverage Gutters
# - ReportGenerator para HTML reports
```text
### **2. Focar em Branches Não Testadas**
```csharp
// Exemplo de código com baixa branch coverage
public string GetStatus(int value)
{
    if (value > 0) return "Positive";      // ✅ Testado
    else if (value < 0) return "Negative"; // ❌ Não testado  
    return "Zero";                         // ❌ Não testado
}

// Teste necessário para 100% branch coverage
[Test] public void GetStatus_PositiveValue_ReturnsPositive() { }
[Test] public void GetStatus_NegativeValue_ReturnsNegative() { } // Adicionar
[Test] public void GetStatus_ZeroValue_ReturnsZero() { }         // Adicionar
```yaml
### **3. Adicionar Testes para Cenários Edge Case**
- Valores nulos
- Listas vazias  
- Exceptions
- Condições de erro

## 📁 Arquivos de Coverage Gerados

### **Estrutura dos Artifacts**
```csharp
coverage/
├── users/
│   ├── users.opencover.xml     # Coverage detalhado do módulo Users
│   └── users-test-results.trx  # Resultados dos testes
└── shared/
    ├── shared.opencover.xml    # Coverage do código compartilhado
    └── shared-test-results.trx
```text
### **Formato OpenCover XML**
```xml
<CoverageSession>
  <Summary numSequencePoints="1000" visitedSequencePoints="853" 
           sequenceCoverage="85.3" numBranchPoints="500" 
           visitedBranchPoints="394" branchCoverage="78.9" />
</CoverageSession>
```text
## 🛠️ Ferramentas para Visualização Local

### **1. Coverage Gutters (VS Code)**
```bash
# Instalar extensão Coverage Gutters
# Abrir arquivo .opencover.xml
# Ver linhas coloridas no editor:
# - Verde: Linha testada
# - Vermelho: Linha não testada
# - Amarelo: Linha parcialmente testada
```csharp
### **2. ReportGenerator**
```bash
# Gerar relatório HTML
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage/**/*.opencover.xml" -targetdir:"coveragereport" -reporttypes:Html
```yaml
### **3. dotCover/JetBrains Rider**
```bash
# Usar ferramenta integrada do Rider
# Run → Cover Unit Tests
# Ver relatório visual no IDE
```text
## 📊 Exemplos de Relatórios

### **Relatório de Sucesso (≥85%)**
```csharp
✅ Coverage: 87.2% (Target: 85%)
📈 Line Coverage: 87.2% (1308/1500 lines)
🌿 Branch Coverage: 82.4% (412/500 branches)
🎯 Quality Gate: PASSED
```text
### **Relatório de Warning (70-84%)**
```yaml
⚠️ Coverage: 76.8% (Target: 85%)
📈 Line Coverage: 76.8% (1152/1500 lines)  
🌿 Branch Coverage: 71.2% (356/500 branches)
🎯 Quality Gate: WARNING - Consider adding more tests
```text
### **Relatório de Falha (<70%)**
```yaml
❌ Coverage: 65.3% (Target: 70%)
📈 Line Coverage: 65.3% (980/1500 lines)
🌿 Branch Coverage: 58.6% (293/500 branches)
🎯 Quality Gate: FAILED - Insufficient test coverage
```text
## 🔄 Configuração Personalizada

### **Ajustar Thresholds**
No arquivo `.github/workflows/pr-validation.yml`:

```yaml
# Para projetos novos (menos rigoroso)
thresholds: '60 75'

# Para projetos maduros (mais rigoroso)  
thresholds: '80 90'

# Para projetos críticos (muito rigoroso)
thresholds: '90 95'
```yaml
### **Modo Leniente (Não Falhar)**
```yaml
# Adicionar variável de ambiente
env:
  STRICT_COVERAGE: false  # true = falha se < threshold
```text
## 📚 Links Úteis

- [CodeCoverageSummary Action](https://github.com/irongut/CodeCoverageSummary)
- [OpenCover Documentation](https://github.com/OpenCover/opencover)
- [Coverage Best Practices](../development.md#-diretrizes-de-testes)

---

## 🔍 Análise: CI/CD vs Local Coverage

### Discrepância Identificada

**Pipeline (CI/CD)**: 35.11%  
**Local**: 21%  
**Diferença**: +14.11pp

### Por Que a Diferença?

#### Pipeline Executa MAIS Testes
```yaml
# ci-cd.yml - 8 suítes de testes
1. MeAjudaAi.Shared.Tests ✅
2. MeAjudaAi.Architecture.Tests ✅
3. MeAjudaAi.Integration.Tests ✅
4. MeAjudaAi.Modules.Users.Tests ✅
5. MeAjudaAi.Modules.Documents.Tests ✅
6. MeAjudaAi.Modules.Providers.Tests ✅
7. MeAjudaAi.Modules.ServiceCatalogs.Tests ✅
8. MeAjudaAi.E2E.Tests ✅ (76 testes)
```

#### Local Falha em E2E
- **Problema**: Docker Desktop com `InternalServerError`
- **Impacto**: -10-12pp coverage (E2E tests não rodam)
- **Solução**: Ver [test_infrastructure.md - Bloqueios Conhecidos](./test_infrastructure.md#-implementado-otimização-iclassfixture)

### Como Replicar Coverage da Pipeline Localmente

```powershell
# 1. Garantir Docker Desktop funcionando
docker version
docker ps

# 2. Rodar TODAS as suítes (igual pipeline)
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# 3. Gerar relatório agregado
reportgenerator `
  -reports:"TestResults/**/coverage.cobertura.xml" `
  -targetdir:"TestResults/Coverage" `
  -reporttypes:"Html;Cobertura" `
  -assemblyfilters:"-*.Tests*" `
  -classfilters:"-*.Migrations*"

# 4. Abrir relatório
start TestResults/Coverage/index.html
```

### Identificar Gaps de Coverage

Use o script automatizado:

```powershell
.\scripts\find-coverage-gaps.ps1
```

**Saída exemplo**:
```text
📋 COMMAND/QUERY HANDLERS SEM TESTES
Module    Handler                   Type
------    -------                   ----
Providers GetProvidersQueryHandler  Query

💎 VALUE OBJECTS SEM TESTES
Module    ValueObject
------    -----------
Providers Address

🗄️ REPOSITORIES SEM TESTES
Module          Repository
------          ----------
Documents       DocumentRepository

📊 RESUMO: 8 gaps total (+4.4pp estimado)
```

### Roadmap para 70% Coverage

**Atual**: 35.11%  
**Meta Sprint 1**: 55% (+20pp)  
**Meta Sprint 2**: 70% (+15pp)

**Estratégia Sprint 1** (Quick Wins):
1. ✅ Adicionar módulos faltantes ao CI/CD (+5-8pp) - FEITO
2. Adicionar testes para 8 gaps identificados (+4.4pp)
3. Adicionar testes para Application layer sem coverage (+10pp)
4. Adicionar testes para Domain Value Objects (+3pp)

**Estratégia Sprint 2** (Deep Coverage):
1. Testes de Infrastructure (repositories, external services) (+8pp)
2. Integration tests complexos (módulos comunicando) (+5pp)
3. Edge cases e cenários de erro (+2pp)

---