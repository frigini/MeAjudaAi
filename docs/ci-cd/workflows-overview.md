# GitHub Actions Workflows - Visão Geral

**Última Atualização**: 4 de Dezembro de 2025  
**Total de Workflows**: 7 workflows ativos

---

## 📋 Índice de Workflows

| Workflow | Propósito | Trigger | Tempo Médio |
|----------|-----------|---------|-------------|
| [PR Validation](#1-pr-validation) | Validação de qualidade em PRs | PRs para master/develop | ~25-30 min |
| [CI/CD Pipeline](#2-cicd-pipeline) | Build, test e deploy contínuo | Push para master/develop | ~30-40 min |
| [Aspire CI/CD](#3-aspire-cicd) | Pipeline específico do Aspire | Push/PR em `src/Aspire/**` | ~15-20 min |
| [Check Dependencies](#4-check-dependencies) | Monitora pacotes desatualizados | Diário (09:00 UTC) | ~2-3 min |
| [Monitor Compatibility](#5-monitor-package-compatibility) | Monitora compatibilidade Aspire/Hangfire | Diário (13:00 UTC) | ~1-2 min |
| [Package Watch](#6-package-watch-notifications) | Observa repositórios upstream | Diário (11:00 UTC) | ~1-2 min |
| [Dependabot Auto-Merge](#7-dependabot-auto-merge) | Auto-merge de atualizações seguras | PRs do Dependabot | ~30 seg |

---

## 1. PR Validation

**Arquivo**: `.github/workflows/pr-validation.yml`  
**Documentação Completa**: [pr-validation-workflow.md](./pr-validation-workflow.md)

### Propósito
Workflow **crítico** que garante qualidade de código antes do merge. É o **gatekeeper** do projeto.

### Trigger
```yaml
on:
  pull_request:
    branches: [master, develop]
  workflow_dispatch:  # Manual trigger
```

### Principais Etapas
1. ✅ **Code Quality Checks** - Formatação, análise estática
2. 🧪 **Unit Tests** - Por módulo com cobertura
3. 🏗️ **Architecture Tests** - Validação de camadas DDD
4. 🔗 **Integration Tests** - Testes contra PostgreSQL real
5. 🌐 **E2E Tests** - Fluxos completos de API
6. 📊 **Coverage Report** - Agregação e publicação (meta: 70%)

### Serviços Docker
- PostgreSQL (PostGIS 16-3.4)
- Azurite (Azure Storage Emulator)

### Condições de Falha
- ❌ Build quebrado
- ❌ Testes falhando
- ❌ Coverage < 70% (quando `STRICT_COVERAGE=true`)
- ❌ Violação de regras arquiteturais

### Métricas Atuais
- **Cobertura**: 57.29% (meta: 70%)
- **Testes**: ~1,400 (Unit + Integration + E2E)
- **Tempo**: 25-30 minutos

---

## 2. CI/CD Pipeline

**Arquivo**: `.github/workflows/ci-cd.yml`

### Propósito
Pipeline completo de **Continuous Integration** e **Continuous Deployment** para master e develop.

### Trigger
```yaml
on:
  push:
    branches: [master, develop]
  workflow_dispatch:
    inputs:
      deploy_infrastructure: true/false
      cleanup_after_test: true/false
```

### Jobs

#### Job 1: Build and Test
- Compilação Release
- Unit tests com cobertura
- Exclusões: Migrations, Database, Contracts, código gerado

#### Job 2: Deploy to Development (opcional)
- Deploy de infraestrutura Azure
- Provisionamento de recursos (dev environment)
- Cleanup opcional após deploy

### Diferenças vs PR Validation
| Aspecto | PR Validation | CI/CD |
|---------|---------------|-------|
| **Foco** | Validação de qualidade | Build + Deploy |
| **Cobertura** | Detalhada (Unit+Integration+E2E) | Simplificada (Unit) |
| **Deploy** | Nunca | Opcional (dev environment) |
| **Tempo** | 25-30 min | 30-40 min (com deploy) |

### Azure Resources (Dev)
- Resource Group: `meajudaai-dev`
- Location: `brazilsouth`
- Services: App Service, PostgreSQL, Service Bus, etc.

---

## 3. Aspire CI/CD

**Arquivo**: `.github/workflows/aspire-ci-cd.yml`

### Propósito
Pipeline **especializado** para mudanças no projeto Aspire (AppHost, ServiceDefaults).

### Trigger
```yaml
on:
  push:
    paths:
      - 'src/Aspire/**'
      - '.github/workflows/aspire-ci-cd.yml'
  pull_request:
    paths:
      - 'src/Aspire/**'
```

**Otimização**: Só executa se arquivos Aspire mudarem (economia de recursos).

### Etapas Específicas

#### 1. Install Aspire Workload
```bash
dotnet workload install aspire \
  --skip-sign-check \
  --source https://api.nuget.org/v3/index.json
```
- Instala workload Aspire (templates, ferramentas)
- Suporte a .NET 10 preview packages

#### 2. Build Solution
- Foco em projetos Aspire:
  - `MeAjudaAi.AppHost`
  - `MeAjudaAi.ServiceDefaults`

#### 3. Run Tests
- Testes específicos de AppHost
- Validação de service discovery
- Health checks de recursos Aspire

### Quando Usar
- Modificações em `AppHost.csproj`
- Mudanças em `ServiceDefaults`
- Atualização de Aspire packages

---

## 4. Check Dependencies

**Arquivo**: `.github/workflows/check-dependencies.yml`

### Propósito
Monitora pacotes NuGet desatualizados e cria issues automaticamente.

### Trigger
```yaml
on:
  schedule:
    - cron: '0 9 * * *'  # Diário às 9h UTC (6h BRT)
  workflow_dispatch:
```

**Nota**: Durante Sprint 0 (.NET 10 migration) roda **diariamente**. Após merge para master, mudar para **semanal** (segundas-feiras).

### Ferramentas
- **dotnet-outdated-tool**: Detecta pacotes desatualizados
- Verifica atualizações **Major** (breaking changes)
- Ignora dependências transitivas (`--transitive:false`)

### Comportamento

#### 1. Detecção de Pacotes
```bash
dotnet outdated --upgrade:Major --transitive:false --fail-on-updates
```
- Exit code 0 = nenhum pacote desatualizado
- Exit code > 0 = updates disponíveis

#### 2. Criação de Issue
Se pacotes desatualizados encontrados:
- ✅ **Verifica issues existentes** (evita duplicação)
- 📝 **Cria/atualiza issue** com label `dependencies,automated`
- 📊 **Anexa relatório completo** do dotnet-outdated

#### 3. Issue Template
```markdown
## 📦 Pacotes Desatualizados Detectados

**Data**: [timestamp]

### Relatório dotnet-outdated
[output completo]

### Ações Recomendadas
1. Revisar breaking changes nas release notes
2. Testar em branch separada
3. Atualizar packages gradualmente
```

### Configuração Pós-Sprint 0
```yaml
# Alterar de diário para semanal
- cron: '0 9 * * 1'  # Segundas-feiras às 9h UTC
```

---

## 5. Monitor Package Compatibility

**Arquivo**: `.github/workflows/monitor-package-compatibility.yml`

### Propósito
Monitora **pacotes específicos** bloqueando a migração .NET 10.

### Trigger
```yaml
on:
  schedule:
    - cron: '0 13 * * *'  # Diário às 10h BRT (após Dependabot)
  workflow_dispatch:
```

### Pacotes Monitorados

#### 1. Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
**Problema**: Versão atual usa EF Core 9.x, precisamos 10.x

**Ações**:
- 🔍 Query NuGet API para versões 13.x+
- ✅ Detecta lançamento de versão compatível
- 📝 Comenta em **Issue #38** com instruções de teste
- 🏷️ Adiciona label `ready-to-test`

**API Call**:
```bash
curl https://api.nuget.org/v3-flatcontainer/aspire.npgsql.entityframeworkcore.postgresql/index.json
```

#### 2. Hangfire.PostgreSql (futuro)
**Problema**: Npgsql 9.x dependency, precisamos 10.x

**Tracking**: Issue #39

### Template de Notificação
```markdown
## 🔔 Nova Versão Detectada!

**Versão**: `13.0.1`

### ✅ Próximos Passos
1. Verificar release notes
2. Testar em branch separada:
   git checkout -b test/aspire-efcore-13.0.1
   dotnet add package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL --version 13.0.1
3. Validar integração

### 📦 Versões Disponíveis
[lista completa]
```

---

## 6. Package Watch Notifications

**Arquivo**: `.github/workflows/package-watch-notifications.yml`

### Propósito
Observa **repositórios upstream** para atividades relacionadas a EF Core 10 / Npgsql 10.

### Trigger
```yaml
on:
  schedule:
    - cron: '0 11 * * *'  # Diário às 8h BRT (antes do Dependabot)
  workflow_dispatch:
```

### Repositórios Monitorados

#### 1. dotnet/aspire
**Busca**: Commits mencionando "EF Core 10" ou "EntityFramework 10"

**GitHub API**:
```bash
gh api /repos/dotnet/aspire/commits \
  --field per_page=20 \
  -q '.[] | select(.commit.message | test("ef.*core.*10|efcore.*10"))'
```

**Notifica**: Issue #38

#### 2. frankhommers/Hangfire.PostgreSql
**Busca**: Issues/PRs sobre "v2" ou "Npgsql 10"

**GitHub Search API**:
```bash
gh api '/search/issues?q=repo:frankhommers/Hangfire.PostgreSql+npgsql+10+OR+version+2'
```

**Notifica**: Issue #39

### Fluxo de Notificação
1. 🔍 **Busca atividade** nos repositórios
2. 📊 **Extrai commits/issues** relevantes
3. 💬 **Comenta na issue** com detalhes
4. 🔗 **Links diretos** para commits/PRs

### Por que é Útil?
- ⏰ Detecta mudanças **antes** de releases oficiais
- 📣 Alerta sobre trabalho em progresso (WIP PRs)
- 🚀 Permite preparação antecipada para updates

---

## 7. Dependabot Auto-Merge

**Arquivo**: `.github/workflows/dependabot-auto-merge.yml`

### Propósito
Automatiza merge de atualizações **seguras** do Dependabot (patch updates).

### Trigger
```yaml
on:
  pull_request:  # Qualquer PR
  # Executa APENAS se github.actor == 'dependabot[bot]'
```

### Política de Auto-Merge

#### Pacotes Aprovados (Patch Updates)
```yaml
- Aspire.*                    # Aspire packages
- FluentAssertions           # Test utilities
- Bogus                      # Test data generation
- SonarAnalyzer.CSharp       # Code analysis
```

#### Critérios de Auto-Merge
1. ✅ **Update Type**: `semver-patch` (x.y.**Z**)
2. ✅ **Pacote na whitelist**: Aspire, FluentAssertions, Bogus
3. ✅ **CI passa**: PR Validation sucesso
4. ✅ **Auto-approve**: Workflow aprova automaticamente

### Fluxo
```
Dependabot cria PR (patch update)
    ↓
Workflow verifica metadata
    ↓
Se pacote seguro → Auto-approve
    ↓
PR Validation executa
    ↓
Se CI verde → Auto-merge (squash)
```

### Tipos de Update NÃO Auto-Merged
- ❌ **Minor updates** (x.**Y**.z) - Requer revisão manual
- ❌ **Major updates** (**X**.y.z) - Breaking changes, sempre manual
- ❌ Pacotes críticos (e.g., Npgsql, EF Core) - Sempre manual

### Configuração de Merge
```yaml
gh pr merge --auto --squash "$PR_URL"
```
- **Auto**: Merge quando CI passar
- **Squash**: Commits consolidados

---

## 🔄 Cronograma Diário dos Workflows

```
06:00 BRT (09:00 UTC) - Check Dependencies
    ↓ [1 hora]
08:00 BRT (11:00 UTC) - Package Watch Notifications
    ↓ [2 horas]
10:00 BRT (13:00 UTC) - Monitor Package Compatibility
```

**Ordem estratégica**:
1. **Check Dependencies**: Identifica updates disponíveis
2. **Package Watch**: Detecta atividade upstream
3. **Monitor Compatibility**: Verifica se pacotes bloqueadores foram lançados

---

## 🎯 Estratégia de Workflows por Ambiente

### Development (develop branch)
- ✅ PR Validation (em PRs)
- ✅ CI/CD Pipeline (em push)
- ✅ Aspire CI/CD (mudanças em Aspire)
- ❌ Deploy para produção (nunca)

### Production (master branch)
- ✅ PR Validation (em PRs)
- ✅ CI/CD Pipeline (em push)
- ✅ Deploy para produção (manual via workflow_dispatch)

### Scheduled Jobs (qualquer branch)
- ✅ Check Dependencies
- ✅ Monitor Compatibility
- ✅ Package Watch

---

## 🔐 Secrets Necessários

### Obrigatórios
| Secret | Uso | Workflows |
|--------|-----|-----------|
| `POSTGRES_PASSWORD` | Banco de teste | PR Validation, CI/CD, Aspire CI/CD |
| `POSTGRES_USER` | Usuário PostgreSQL | PR Validation, CI/CD, Aspire CI/CD |
| `POSTGRES_DB` | Nome do banco | PR Validation, CI/CD, Aspire CI/CD |

### Opcionais
| Secret | Uso | Workflows |
|--------|-----|-----------|
| `KEYCLOAK_ADMIN_PASSWORD` | Testes de autenticação | PR Validation |
| `AZURE_CREDENTIALS` | Deploy Azure | CI/CD (deploy jobs) |

### Fallbacks para Desenvolvimento
```yaml
POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD || 'test123' }}
POSTGRES_USER: ${{ secrets.POSTGRES_USER || 'postgres' }}
POSTGRES_DB: ${{ secrets.POSTGRES_DB || 'meajudaai_test' }}
```

---

## 📊 Métricas de Uso

### Execuções Mensais Estimadas

| Workflow | Frequência | Execuções/mês | Tempo Total |
|----------|------------|---------------|-------------|
| PR Validation | ~10 PRs/semana | ~40 | ~16-20 horas |
| CI/CD Pipeline | ~20 pushes/semana | ~80 | ~40-50 horas |
| Aspire CI/CD | ~2 pushes/semana | ~8 | ~2-3 horas |
| Check Dependencies | Diário | ~30 | ~1-1.5 horas |
| Monitor Compatibility | Diário | ~30 | ~30-60 min |
| Package Watch | Diário | ~30 | ~30-60 min |
| Dependabot Auto-Merge | ~5 PRs/semana | ~20 | ~10-15 min |

**Total Estimado**: ~60-75 horas de CI/CD por mês

### Otimizações de Custo
1. ✅ **Path filters** em Aspire CI/CD (evita execuções desnecessárias)
2. ✅ **Caching** de NuGet packages
3. ✅ **`--no-build`** em testes (reusa compilação)
4. ✅ **Scheduled jobs leves** (~1-3 min cada)

---

## 🚀 Próximos Passos e Melhorias

### Sprint 0 (Migração .NET 10)
- [ ] Habilitar `STRICT_COVERAGE: true` quando coverage >= 70%
- [ ] Migrar Check Dependencies para **semanal** (segundas-feiras)
- [ ] Remover Monitor Compatibility após upgrade de Aspire/Hangfire

### Melhorias de Infraestrutura
- [ ] **Matrix strategy**: Testar em Ubuntu + Windows
- [ ] **Reusable workflows**: Extrair jobs comuns
- [ ] **Composite actions**: Consolidar setup steps
- [ ] **GitHub Environments**: Separar dev/staging/prod

### Observabilidade
- [ ] **Badges no README**: Coverage, build status, dependencies
- [ ] **Dashboards**: Visualização de métricas de CI/CD
- [ ] **Alertas**: Notificações em Slack/Discord para falhas

---

## 📚 Documentação Relacionada

- **PR Validation**: [pr-validation-workflow.md](./pr-validation-workflow.md) (documentação detalhada)
- **CI/CD Overview**: [../ci-cd.md](../ci-cd.md)
- **Code Coverage**: [../testing/code-coverage-guide.md](../testing/code-coverage-guide.md)
- **Architecture Tests**: (pending implementation)

---

## 💡 FAQ

### Qual a diferença entre PR Validation e CI/CD Pipeline?
**PR Validation** foca em **qualidade** (testes extensivos, coverage). **CI/CD** foca em **build + deploy** (testes simplificados).

### Por que 3 workflows de monitoramento de pacotes?
- **Check Dependencies**: Monitora **todos** os pacotes (dotnet-outdated)
- **Monitor Compatibility**: Monitora **pacotes específicos** bloqueadores (.NET 10)
- **Package Watch**: Monitora **repositórios upstream** (atividade de desenvolvimento)

### Posso desabilitar workflows temporariamente?
Sim, use `if: false` no job ou comente o arquivo. Evite deletar (perde histórico).

### Como testar mudanças em workflows?
Use `workflow_dispatch` para trigger manual ou crie branch `test/workflow-changes` e abra PR de teste.

---

**Última Atualização**: 4 de Dezembro de 2025  
**Mantenedor**: @frigini  
**Questões**: Abra uma issue com label `ci-cd`
