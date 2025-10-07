# Code Coverage - Como Visualizar e Interpretar

## 📊 Onde Ver as Porcentagens de Coverage

### 1. **GitHub Actions - Logs do Workflow**
Nas execuções do workflow `PR Validation`, você encontrará as porcentagens em:

#### Step: "Code Coverage Summary"
```
📊 Code Coverage Summary
========================
Line Coverage: 85.3%
Branch Coverage: 78.9%
```

#### Step: "Display Coverage Percentages"  
```
📊 CODE COVERAGE SUMMARY
========================

📄 Coverage file: ./coverage/users/users.opencover.xml
  📈 Line Coverage: 85.3%
  🌿 Branch Coverage: 78.9%

💡 For detailed coverage report, check the 'Code Coverage Summary' step above
🎯 Minimum thresholds: 70% (warning) / 85% (good)
```

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
```

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
```

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
```

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
```

### **3. Adicionar Testes para Cenários Edge Case**
- Valores nulos
- Listas vazias  
- Exceptions
- Condições de erro

## 📁 Arquivos de Coverage Gerados

### **Estrutura dos Artifacts**
```
coverage/
├── users/
│   ├── users.opencover.xml     # Coverage detalhado do módulo Users
│   └── users-test-results.trx  # Resultados dos testes
└── shared/
    ├── shared.opencover.xml    # Coverage do código compartilhado
    └── shared-test-results.trx
```

### **Formato OpenCover XML**
```xml
<CoverageSession>
  <Summary numSequencePoints="1000" visitedSequencePoints="853" 
           sequenceCoverage="85.3" numBranchPoints="500" 
           visitedBranchPoints="394" branchCoverage="78.9" />
</CoverageSession>
```

## 🛠️ Ferramentas para Visualização Local

### **1. Coverage Gutters (VS Code)**
```bash
# Instalar extensão Coverage Gutters
# Abrir arquivo .opencover.xml
# Ver linhas coloridas no editor:
# - Verde: Linha testada
# - Vermelho: Linha não testada
# - Amarelo: Linha parcialmente testada
```

### **2. ReportGenerator**
```bash
# Gerar relatório HTML
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage/**/*.opencover.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### **3. dotCover/JetBrains Rider**
```bash
# Usar ferramenta integrada do Rider
# Run → Cover Unit Tests
# Ver relatório visual no IDE
```

## 📊 Exemplos de Relatórios

### **Relatório de Sucesso (≥85%)**
```
✅ Coverage: 87.2% (Target: 85%)
📈 Line Coverage: 87.2% (1308/1500 lines)
🌿 Branch Coverage: 82.4% (412/500 branches)
🎯 Quality Gate: PASSED
```

### **Relatório de Warning (70-84%)**
```
⚠️ Coverage: 76.8% (Target: 85%)
📈 Line Coverage: 76.8% (1152/1500 lines)  
🌿 Branch Coverage: 71.2% (356/500 branches)
🎯 Quality Gate: WARNING - Consider adding more tests
```

### **Relatório de Falha (<70%)**
```
❌ Coverage: 65.3% (Target: 70%)
📈 Line Coverage: 65.3% (980/1500 lines)
🌿 Branch Coverage: 58.6% (293/500 branches)
🎯 Quality Gate: FAILED - Insufficient test coverage
```

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
```

### **Modo Leniente (Não Falhar)**
```yaml
# Adicionar variável de ambiente
env:
  STRICT_COVERAGE: false  # true = falha se < threshold
```

## 📚 Links Úteis

- [CodeCoverageSummary Action](https://github.com/irongut/CodeCoverageSummary)
- [OpenCover Documentation](https://github.com/OpenCover/opencover)
- [Coverage Best Practices](../development-guidelines.md#testing-guidelines)