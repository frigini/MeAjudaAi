# Pull Request Validation Workflow

**Arquivo**: `.github/workflows/pr-validation.yml`  
**Última Atualização**: 4 de Dezembro de 2025

---

## 📋 Visão Geral

O workflow de PR Validation é o **gatekeeper** do projeto - garante que todo código enviado ao repositório atende aos padrões de qualidade antes de ser mergeado. É executado automaticamente em Pull Requests para `master` e `develop`, e pode ser disparado manualmente via `workflow_dispatch`.

### Objetivos Principais

1. ✅ **Qualidade de Código**: Verificar formatação, análise estática e cobertura de testes
2. 🧪 **Testes Automatizados**: Executar Unit, Integration, Architecture e E2E tests
3. 📊 **Cobertura de Código**: Garantir cobertura mínima (objetivo: 70%)
4. 🏗️ **Validação Arquitetural**: Verificar organização de namespaces e dependências
5. 🔐 **Segurança**: Validar configurações e secrets

---

## 🔧 Configuração e Variáveis de Ambiente

### Variáveis Globais

```yaml
env:
  DOTNET_VERSION: '10.0.x'              # .NET 10 (migração de .NET 9)
  STRICT_COVERAGE: false                # Meta: true quando coverage >= 70%
  POSTGRES_PASSWORD: <secret/fallback>  # Senha do banco de dados
  POSTGRES_USER: <secret/fallback>      # Usuário PostgreSQL
  POSTGRES_DB: <secret/fallback>        # Nome do banco de testes
```

### Permissões Necessárias

```yaml
permissions:
  contents: read          # Ler código do repositório
  pull-requests: write    # Comentar no PR
  checks: write          # Publicar status checks
  statuses: write        # Atualizar status do PR
```

---

## 🎯 Estrutura do Workflow

O workflow é composto por **1 job principal** (`code-quality`) com **múltiplas etapas sequenciais**.

### Serviços Docker (Services)

Antes de executar os testes, o workflow provisiona serviços necessários:

#### 1. PostgreSQL (PostGIS)
```yaml
image: postgis/postgis:16-3.4
ports: 5432:5432
health-checks: pg_isready
```
- **Uso**: Integration/E2E tests, migrations
- **Configuração**: Variáveis de ambiente + health checks
- **Extensões**: PostGIS para funcionalidades geoespaciais

#### 2. Azurite (Azure Storage Emulator)
```yaml
image: mcr.microsoft.com/azure-storage/azurite
ports: 10000-10002
```
- **Uso**: Testes de armazenamento blob (opcional)
- **Substituição**: Pode ser removido se não houver testes de storage

---

## 📦 Etapas do Workflow

### 1️⃣ Setup e Preparação

#### **Checkout code**
```yaml
- uses: actions/checkout@v6
  with:
    fetch-depth: 0  # Clone completo para análise de diff
```
- Baixa o código do PR
- `fetch-depth: 0` permite diff com branch base

#### **Setup .NET**
```yaml
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: '10.0.x'
```
- Instala .NET SDK 10.0 (latest stable)
- Usa versão especificada em `global.json` se disponível

#### **Validate Secrets Configuration**
- Verifica se secrets obrigatórios estão configurados
- Exibe fallbacks para desenvolvimento local
- **Crítico**: POSTGRES_PASSWORD, POSTGRES_USER, POSTGRES_DB

#### **Check Keycloak Configuration**
- Valida secret `KEYCLOAK_ADMIN_PASSWORD` (opcional)
- Exibe mensagens informativas se não configurado
- Testes de autenticação podem ser skippados sem Keycloak

#### **Install PostgreSQL Client**
```bash
sudo apt-get install postgresql-client
```
- Necessário para comandos `pg_isready`, `psql`
- Usado para health checks e migrations

---

### 2️⃣ Build e Restauração

#### **Restore dependencies**
```bash
dotnet restore MeAjudaAi.sln --force-evaluate
```
- Restaura pacotes NuGet
- `--force-evaluate`: Força reavaliação de dependências

#### **Build solution**
```bash
dotnet build MeAjudaAi.sln --configuration Release --no-restore
```
- Compila todo o projeto em modo Release
- `--no-restore`: Usa pacotes já restaurados (economia de tempo)
- **Falha aqui**: Build quebrado, PR bloqueado

---

### 3️⃣ Infraestrutura e Database

#### **Wait for PostgreSQL to be ready**
```bash
while ! pg_isready -h localhost -p 5432; do
  sleep 1
  counter=$((counter+1))
  # Max 60 tentativas (1 minuto)
done
```
- Aguarda PostgreSQL aceitar conexões
- Timeout: 60 segundos
- **Falha aqui**: Problema de infraestrutura

#### **Setup PostgreSQL connection**
```bash
connection_string="Host=localhost;Port=5432;Database=$POSTGRES_DB;..."
echo "connection-string=$connection_string" >> $GITHUB_OUTPUT
```
- Monta connection string para testes
- Exporta como output `db.connection-string` para steps seguintes

---

### 4️⃣ Testes Automatizados

#### **Run Unit Tests**

**O que faz**:
- Executa testes unitários de **todos os módulos** (Providers, ServiceCatalogs, Users, etc.)
- Coleta cobertura de código usando Coverlet
- Exclui assemblies de teste, migrations, database e contracts

**Configuração de Coverage**:
```bash
INCLUDE_FILTER="[MeAjudaAi.*]*"
EXCLUDE_FILTER="[*]*Tests*;[*]*.Migrations.*;[*]*.Database;[*]*.Contracts"
EXCLUDE_BY_FILE="**/*OpenApi*.generated.cs,**/RegexGenerator.g.cs"
EXCLUDE_BY_ATTRIBUTE="Obsolete,GeneratedCode,CompilerGenerated"
```

**Por módulo**:
- Detecta automaticamente módulos em `src/Modules/*/Tests/Unit/`
- Gera runsettings XML com filtros de coverage
- Executa: `dotnet test` com `--collect:"XPlat Code Coverage"`
- Salva resultados em `./coverage/unit/<module>/`

**Exemplo de Output**:
```
🧪 UNIT TESTS - MODULE: Providers
================================
  Total tests: 156
  Passed: 156
  Failed: 0
  Skipped: 0
  Coverage: coverage.opencover.xml → ./coverage/unit/providers/
```

---

#### **Run Architecture Tests**

**O que faz**:
- Valida regras arquiteturais usando **NetArchTest**
- Verifica camadas (Domain, Application, Infrastructure, API)
- Garante que dependências seguem princípios DDD

**Regras Validadas**:
- ✅ Domain não depende de Infrastructure
- ✅ Application depende apenas de Domain
- ✅ Entities estão em `Domain.Entities`
- ✅ Repositories em `Infrastructure.Persistence`

**Comando**:
```bash
dotnet test tests/MeAjudaAi.ArchitectureTests/ \
  --configuration Release \
  --verbosity normal \
  --logger "trx;LogFileName=architecture-test-results.trx"
```

---

#### **Run Integration Tests**

**O que faz**:
- Testa integrações entre camadas (API ↔ Database ↔ MessageBus)
- Usa **TestContainers** para PostgreSQL isolado
- Executa migrations reais contra banco de teste

**Diferenças vs Unit Tests**:
- Sem `--no-build` (pode recompilar se necessário)
- Database real (não mocks)
- Tempo de execução maior (~5-10 minutos)

**Configuração**:
```bash
INTEGRATION_RUNSETTINGS="/tmp/integration.runsettings"
EXCLUDE_FILTER="[*.Tests]*,[testhost]*"
```

**Connection String**:
```bash
ConnectionStrings__DefaultConnection=${{ steps.db.outputs.connection-string }}
```

---

#### **Run E2E Tests**

**O que faz**:
- Testa fluxos completos end-to-end (API → Database → Response)
- Simula requests HTTP reais usando `WebApplicationFactory`
- Valida contratos de API (OpenAPI schemas)

**Cenários Testados**:
- Criar Provider → Buscar → Atualizar → Deletar
- Autenticação e autorização (se Keycloak configurado)
- Paginação e filtros de busca
- Validações de input e error handling

**Tempo**: ~10-15 minutos (mais lento que Integration)

---

### 5️⃣ Análise de Cobertura

#### **Generate Aggregated Coverage Report**

**Ferramentas**:
- **ReportGenerator**: Consolida múltiplos arquivos `coverage.opencover.xml`
- **Cobertura**: Tool de cobertura de linha de comando

**Processo**:
1. **Busca Coverage Files**:
   ```bash
   find ./coverage -name 'coverage.opencover.xml' -not -path '*/merged/*'
   ```

2. **Consolida com ReportGenerator**:
   ```bash
   dotnet tool run reportgenerator \
     -reports:"./coverage/**/coverage.opencover.xml" \
     -targetdir:"./coverage/merged" \
     -reporttypes:"Cobertura;HtmlInline_AzurePipelines;MarkdownSummaryGithub"
   ```
   
   **Outputs**:
   - `Cobertura.xml`: Formato para ferramentas de CI/CD
   - `HtmlInline_AzurePipelines`: Relatório visual
   - `MarkdownSummaryGithub`: Summary para comentar no PR

3. **Calcula Métricas**:
   ```bash
   Line Coverage:    57.29% (11,892 / 20,758)
   Branch Coverage:  45.12% (1,234 / 2,734)
   Method Coverage:  62.45% (3,456 / 5,534)
   ```

---

#### **Validate namespace reorganization**

**O que faz**:
- Verifica se arquivos seguem convenção de namespaces
- Exemplo: `src/Modules/Users/Domain/Entities/User.cs` → namespace `MeAjudaAi.Modules.Users.Domain.Entities`

**Falha se**:
- Namespace não corresponde ao caminho do arquivo
- Arquivos fora da estrutura esperada

---

### 6️⃣ Publicação de Resultados

#### **Upload coverage reports**
```yaml
- uses: actions/upload-artifact@v4
  with:
    name: coverage-report
    path: |
      ./coverage/merged/
      ./coverage/**/coverage.opencover.xml
```
- Disponibiliza relatórios para download
- Preserva por 30 dias (padrão GitHub)

#### **Upload Test Results**
```yaml
- uses: actions/upload-artifact@v4
  with:
    name: test-results
    path: ./coverage/**/*.trx
```
- Arquivos `.trx` contêm detalhes de cada teste
- Útil para debugar falhas

---

#### **Code Coverage Summary**

**Ferramenta**: `irongut/CodeCoverageSummary@v1.3.0`

**O que faz**:
1. Lê `./coverage/merged/Cobertura.xml`
2. Gera tabela Markdown com métricas
3. **Comenta automaticamente no PR** com:
   - Coverage por assembly
   - Coverage total (Line, Branch, Method)
   - Status: ✅ Pass ou ❌ Fail

**Exemplo de Comentário**:
```markdown
## Code Coverage Summary

| Assembly | Line | Branch | Method |
|----------|------|--------|--------|
| Providers.Domain | 78.4% | 65.2% | 82.1% |
| ServiceCatalogs.API | 45.3% | 38.7% | 51.2% |
| **TOTAL** | **57.29%** | **45.12%** | **62.45%** |

⚠️ Coverage below 70% threshold (STRICT_COVERAGE=false)
```

**Thresholds**:
```yaml
thresholds: '60 80'  # Warning < 60%, Error < 80%
```

---

## ⚙️ Scripts Auxiliares

### `.github/scripts/generate-runsettings.sh`

**Criado**: 4 de Dezembro de 2025 (para eliminar duplicação)

**Funções**:

#### `escape_xml()`
```bash
escape_xml() {
  echo "$1" | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g; ...'
}
```
- Escapa caracteres especiais XML (&, <, >, ", ')
- Previne XML malformado em runsettings

#### `generate_runsettings()`
```bash
generate_runsettings file exclude_filter exclude_by_file exclude_by_attr [include_filter]
```
- Gera arquivo XML de configuração Coverlet
- Parâmetros:
  1. `file`: Caminho do arquivo de saída
  2. `exclude_filter`: Assemblies a excluir (e.g., `[*]*Tests*`)
  3. `exclude_by_file`: Arquivos a excluir (glob patterns)
  4. `exclude_by_attr`: Atributos a excluir (e.g., `Obsolete,GeneratedCode`)
  5. `include_filter`: (Opcional) Assemblies a incluir explicitamente

**Exemplo de Uso**:
```bash
source ./.github/scripts/generate-runsettings.sh

generate_runsettings \
  "/tmp/unit.runsettings" \
  "[*]*Tests*;[*]*.Migrations.*" \
  "**/*OpenApi*.generated.cs" \
  "Obsolete,GeneratedCode" \
  "[MeAjudaAi.*]*"
```

---

## 🚨 Condições de Falha

O workflow **falha** (bloqueia merge) se:

1. ❌ **Build falhar** (erros de compilação)
2. ❌ **Testes falharem** (qualquer teste com status Failed)
3. ❌ **Architecture Tests falharem** (violação de regras)
4. ❌ **Coverage < threshold** (quando `STRICT_COVERAGE=true`)
5. ❌ **Namespace validation falhar** (arquivos fora do padrão)

---

## 📊 Métricas e Performance

### Tempos Típicos de Execução

| Etapa | Tempo Médio | Notas |
|-------|-------------|-------|
| Setup (Checkout, .NET, PostgreSQL) | ~2 min | Inclui download de imagens Docker |
| Build | ~3 min | Depende de cache NuGet |
| Unit Tests | ~5 min | Paralelizado por módulo |
| Architecture Tests | ~30 seg | Rápido, validação estática |
| Integration Tests | ~8 min | TestContainers + migrations |
| E2E Tests | ~12 min | Requests HTTP reais |
| Coverage Report | ~2 min | ReportGenerator consolidação |
| **TOTAL** | **~25-30 min** | Pode variar com carga do GitHub |

### Otimizações Aplicadas

1. ✅ **Caching de NuGet**: `actions/setup-dotnet` cacheia pacotes
2. ✅ **Paralelização**: Unit tests executam por módulo
3. ✅ **`--no-build`**: Testes usam binários já compilados
4. ✅ **`--no-restore`**: Build usa pacotes já restaurados
5. ✅ **Health checks**: Aguarda serviços antes de executar testes

---

## 🔐 Secrets Necessários

### Obrigatórios
- `POSTGRES_PASSWORD`: Senha do banco de teste (fallback: `test123`)
- `POSTGRES_USER`: Usuário PostgreSQL (fallback: `postgres`)
- `POSTGRES_DB`: Nome do banco (fallback: `meajudaai_test`)

### Opcionais
- `KEYCLOAK_ADMIN_PASSWORD`: Senha admin Keycloak (para testes de autenticação)

**Configuração**: `Settings → Secrets and variables → Actions → New repository secret`

---

## 📝 Coverage - Exclusões Importantes

### Assemblies Excluídos

```bash
[*]*Tests*              # Todos os assemblies de teste
[*]*.Migrations.*       # Entity Framework Migrations
[*]*.Database           # Configuração de database
[*]*.Contracts          # DTOs e contratos de API
[testhost]*            # Host de execução de testes
```

**Motivo**: Migrations tem 96-97% coverage artificial (código gerado), inflando métricas.

### Arquivos Excluídos

```bash
**/*OpenApi*.generated.cs       # Código gerado por OpenAPI
**/System.Runtime.CompilerServices*.cs  # Runtime do compilador
**/*RegexGenerator.g.cs         # Regex source generators
```

### Atributos Excluídos

```bash
[Obsolete]              # Código deprecado
[GeneratedCode]         # Código gerado
[CompilerGenerated]     # Gerado pelo compilador
```

---

## 🎯 Roadmap e Melhorias Futuras

### Sprint 2 (Meta: Coverage 70%)

- [ ] **Habilitar `STRICT_COVERAGE: true`**
  - Bloquear PRs com coverage < 70%
  - Tracking: [Issue #33](https://github.com/frigini/MeAjudaAi/issues/33)

- [ ] **Adicionar testes para módulos faltantes**:
  - SearchProviders (0% coverage atualmente)
  - Locations (coverage parcial)
  - Shared libraries

### Melhorias de Infraestrutura

- [ ] **Matrix strategy**: Testar em múltiplas versões .NET (9.x, 10.x)
- [ ] **Cache de Docker layers**: Acelerar startup de PostgreSQL
- [ ] **Mutation Testing**: Adicionar Stryker.NET para validar qualidade dos testes
- [ ] **SonarCloud**: Integração para análise estática avançada

### Developer Experience

- [ ] **Pre-commit hooks**: Executar formatação e testes locais
- [ ] **Coverage badges**: Adicionar badges no README
- [ ] **Comentários detalhados**: Diff de coverage (antes vs depois)

---

## 🔗 Referências

### Documentação Relacionada

- [Code Coverage Guide](../testing/code-coverage-guide.md)
- [Integration Tests](../testing/integration-tests.md)
- Architecture tests (pending implementation)
- [CI/CD Overview](../ci-cd.md)

### Ferramentas e Actions

- [actions/checkout@v6](https://github.com/actions/checkout)
- [actions/setup-dotnet@v5](https://github.com/actions/setup-dotnet)
- [irongut/CodeCoverageSummary](https://github.com/irongut/CodeCoverageSummary)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [Coverlet](https://github.com/coverlet-coverage/coverlet)

---

## 💡 FAQ

### Por que o workflow demora tanto?

**Resposta**: O workflow executa ~1,400 testes (Unit + Integration + E2E) contra um banco PostgreSQL real. E2E tests são particularmente lentos pois simulam requests HTTP completos. Tempo médio: 25-30 minutos.

### Por que STRICT_COVERAGE está false?

**Resposta**: Meta é 70% coverage. Atualmente estamos em **57.29%** (após correções de Migrations). Quando atingirmos 70%, habilitaremos `STRICT_COVERAGE: true` para bloquear PRs abaixo desse threshold.

### Posso rodar o workflow localmente?

**Resposta**: Parcialmente. Use:
```bash
# Unit Tests
dotnet test --collect:"XPlat Code Coverage"

# Com Docker Compose (PostgreSQL)
docker-compose up -d postgres
dotnet test --filter "Category=Integration"
```

Porém, o workflow completo (com artifacts, comentários no PR) só funciona no GitHub Actions.

### O que fazer se PostgreSQL não iniciar?

**Resposta**: 
1. Verificar health checks no step "Wait for PostgreSQL to be ready"
2. Verificar logs: `Actions → PR Validation → code-quality → Setup PostgreSQL connection`
3. Possível timeout (> 60s): Problema de infraestrutura GitHub

---

**Última Atualização**: 4 de Dezembro de 2025  
**Mantenedor**: @frigini  
**Questões**: Abra uma issue ou consulte [CI/CD Troubleshooting](../ci-cd.md#troubleshooting)
