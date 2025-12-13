# 📋 Inventário Completo de Scripts - MeAjudaAi

> **Data da Auditoria**: 12 de Dezembro de 2025  
> **Total de Scripts**: 32 arquivos (.sh + .ps1)  
> **Status**: 🔄 Auditoria Completa - Ação Necessária

---

## 📊 Resumo Executivo

| Categoria | Quantidade | Status | Ação Recomendada |
|-----------|------------|--------|------------------|
| **Scripts Ativos** (em uso) | 22 | ✅ Manter | Documentar melhor |
| **Scripts Migração** (one-time) | 4 | ⚠️ Deprecar | Mover para `deprecated/` |
| **Scripts Redundantes** | 6 | 🔴 Remover | Consolidar ou deletar |

---

## 1️⃣ Scripts Principais (`/scripts/`) - ✅ **MANTER**

### **Desenvolvimento & Build**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `dev.sh` | Bash | Desenvolvimento local (menu interativo) | ✅ Ativo | ✅ Sim |
| `setup.sh` | Bash | Onboarding de novos devs | ✅ Ativo | ✅ Sim |
| `utils.sh` | Bash | Biblioteca de funções compartilhadas | ✅ Ativo | ✅ Sim |
| `optimize.sh` | Bash | Otimizações de performance | ✅ Ativo | ✅ Sim |

### **Testes**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `test.sh` | Bash | Execução de testes (unit/int/e2e) | ✅ Ativo | ✅ Sim |

### **Deploy**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `deploy.sh` | Bash | Deploy Azure (Bicep) | ✅ Ativo | ✅ Sim |

### **Banco de Dados**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `ef-migrate.ps1` | PowerShell | Migrations EF Core (recomendado) | ✅ Ativo | ✅ Sim |
| `migrate-all.ps1` | PowerShell | Migrations customizadas (avançado) | ⚠️ Duplicado | ⚠️ Parcial |
| `seed-dev-data.ps1` | PowerShell | Seeding dados de desenvolvimento | ✅ Ativo | ✅ Sim |
| `seed-dev-data.sh` | Bash | Seeding dados (Linux/macOS) | ✅ Ativo | ✅ Sim |

**⚠️ AÇÃO NECESSÁRIA:**
- `migrate-all.ps1` é redundante com `ef-migrate.ps1`?
- **Decisão**: Manter ambos OU consolidar funcionalidades

### **API & Documentação**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `export-openapi.ps1` | PowerShell | Gerar OpenAPI spec (offline) | ✅ Ativo | ✅ Sim |

### **Code Coverage** (PowerShell)
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `generate-clean-coverage.ps1` | PowerShell | Relatório limpo (sem código gerado) | ✅ Ativo | ✅ Sim |
| `test-coverage-like-pipeline.ps1` | PowerShell | Simular pipeline CI/CD | ✅ Ativo | ✅ Sim |
| `track-coverage-progress.ps1` | PowerShell | Progresso rumo à meta 70% | ✅ Ativo | ✅ Sim |
| `find-coverage-gaps.ps1` | PowerShell | Identificar gaps de testes | ✅ Ativo | ✅ Sim |
| `monitor-coverage.ps1` | PowerShell | Histórico e tendências | ✅ Ativo | ✅ Sim |
| `analyze-coverage-detailed.ps1` | PowerShell | Análise granular por módulo | ✅ Ativo | ✅ Sim |
| `aggregate-coverage-local.ps1` | PowerShell | Merge de múltiplos arquivos | ✅ Ativo | ✅ Sim |

---

## 2️⃣ Scripts de Build (`/build/`) - ⚠️ **DEPRECAR**

| Arquivo | Tipo | Propósito | Status | Ação |
|---------|------|-----------|--------|------|
| `migrate-xunit.ps1` | PowerShell | Migração xUnit v2→v3 | 🔴 **Obsoleto** | Mover para `deprecated/` |
| `migrate-xunit.sh` | Bash | Migração xUnit v2→v3 | 🔴 **Obsoleto** | Mover para `deprecated/` |
| `migrate-to-dotnet10.ps1` | PowerShell | Migração .NET 9→10 | 🔴 **Obsoleto** | Mover para `deprecated/` |
| `fix-package-references.ps1` | PowerShell | Fix de packages (one-time) | 🔴 **Obsoleto** | Mover para `deprecated/` |
| `dotnet-install.sh` | Bash | Instalação .NET (CI/CD) | ✅ Ativo | ⚠️ Verificar se ainda usado |

**⚠️ MOTIVO PARA DEPRECAR:**
- Scripts de migração são **one-time tasks** já executadas
- Projeto já está em .NET 10 e xUnit v3
- Manter apenas para referência histórica

---

## 3️⃣ Scripts de Infrastructure (`/infrastructure/`) - ✅ **MANTER**

### **Database**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `test-database-init.sh` | Bash | Testar init scripts PostgreSQL | ✅ Ativo | ❌ Não |
| `test-database-init.ps1` | PowerShell | Testar init scripts PostgreSQL | ✅ Ativo | ❌ Não |
| `database/01-init-meajudaai.sh` | Bash | Init PostgreSQL schemas | ✅ Ativo | ❌ Não |
| `database/create-module.ps1` | PowerShell | Criar novo módulo DB | ✅ Ativo | ❌ Não |

### **Docker Compose**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `compose/environments/setup-secrets.sh` | Bash | Configurar secrets Docker | ✅ Ativo | ❌ Não |
| `compose/environments/verify-resources.sh` | Bash | Verificar recursos Docker | ✅ Ativo | ❌ Não |
| `compose/standalone/postgres/init/02-custom-setup.sh` | Bash | Setup customizado PostgreSQL | ✅ Ativo | ❌ Não |

### **Keycloak**
| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `keycloak/scripts/keycloak-init-dev.sh` | Bash | Init Keycloak dev | ✅ Ativo | ❌ Não |
| `keycloak/scripts/keycloak-init-prod.sh` | Bash | Init Keycloak prod | ✅ Ativo | ❌ Não |

**⚠️ AÇÃO NECESSÁRIA:**
- **TODOS** os scripts de infrastructure precisam de documentação
- Criar `infrastructure/README.md` consolidado

---

## 4️⃣ Scripts de Automation (`/automation/`) - ✅ **MANTER**

| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `setup-cicd.ps1` | PowerShell | Setup CI/CD completo | ✅ Ativo | ✅ Sim |
| `setup-ci-only.ps1` | PowerShell | Setup apenas CI | ✅ Ativo | ✅ Sim |

---

## 5️⃣ Scripts de Docs (`/docs/`) - ⚠️ **AVALIAR**

| Arquivo | Tipo | Propósito | Status | Ação |
|---------|------|-----------|--------|------|
| `configuration-templates/configure-environment.sh` | Bash | Configurar appsettings.json | ⚠️ Duplicado? | Verificar vs manual config |

**⚠️ QUESTÃO:**
- Esse script é usado OU é apenas template de exemplo?
- Se não for usado, mover para `examples/` ou remover

---

## 6️⃣ Scripts de Tools (`/tools/`) - ⚠️ **AVALIAR**

| Arquivo | Tipo | Propósito | Status | Ação |
|---------|------|-----------|--------|------|
| `api-collections/generate-all-collections.sh` | Bash | Gerar collections API | ⚠️ Obsoleto? | Verificar se ainda usado |

**⚠️ QUESTÃO:**
- Com Bruno Collections manuais criadas, esse script ainda é necessário?
- Se não for usado, mover para `deprecated/` ou remover

---

## 7️⃣ Scripts de GitHub (`/.github/`) - ✅ **MANTER**

| Arquivo | Tipo | Propósito | Status | Documentado |
|---------|------|-----------|--------|-------------|
| `scripts/generate-runsettings.sh` | Bash | Gerar .runsettings (CI/CD) | ✅ Ativo | ❌ Não |

**⚠️ AÇÃO NECESSÁRIA:**
- Documentar propósito e uso no pipeline

---

## 🎯 Plano de Ação Recomendado

### **Fase 1: Limpeza Imediata (1 hora)**
```bash
# 1. Criar pasta deprecated
mkdir -p build/deprecated

# 2. Mover scripts de migração obsoletos
mv build/migrate-xunit.ps1 build/deprecated/
mv build/migrate-xunit.sh build/deprecated/
mv build/migrate-to-dotnet10.ps1 build/deprecated/
mv build/fix-package-references.ps1 build/deprecated/

# 3. Adicionar README explicando
cat > build/deprecated/README.md <<'EOF'
# Deprecated Build Scripts

Scripts neste diretório são **obsoletos** e mantidos apenas para referência histórica.

## Scripts de Migração (Já Executados)
- `migrate-xunit.ps1/sh`: Migração xUnit v2→v3 (concluída Nov 2025)
- `migrate-to-dotnet10.ps1`: Migração .NET 9→10 (concluída Nov 2025)
- `fix-package-references.ps1`: Fix de package versions (concluído Nov 2025)

**⚠️ NÃO EXECUTE ESTES SCRIPTS** - Eles foram criados para migrations one-time já concluídas.
EOF
```

### **Fase 2: Consolidação (`/scripts/` vs `/build/`) (2 horas)**

**Proposta**: Consolidar `ef-migrate.ps1` e `migrate-all.ps1`

```bash
# Analisar diferenças
diff scripts/ef-migrate.ps1 scripts/migrate-all.ps1

# Se redundante: 
# - Manter ef-migrate.ps1 (padrão EF Core)
# - Deprecar migrate-all.ps1 OU consolidar funcionalidades
```

### **Fase 3: Documentação Infrastructure (3 horas)**

Criar `infrastructure/README.md`:

```markdown
# 🏗️ Infrastructure Scripts

## Database Scripts
- `test-database-init.sh/ps1`: Valida scripts de init PostgreSQL
- `database/01-init-meajudaai.sh`: Cria schemas de todos módulos
- `database/create-module.ps1`: Template para novo módulo

## Keycloak Scripts  
- `keycloak/scripts/keycloak-init-dev.sh`: Configura Keycloak dev
- `keycloak/scripts/keycloak-init-prod.sh`: Configura Keycloak prod

## Docker Compose
- `compose/environments/setup-secrets.sh`: Setup de secrets
- `compose/environments/verify-resources.sh`: Health check recursos
```

### **Fase 4: Revisão & Remoção (1 hora)**

**Scripts a investigar e potencialmente remover:**
1. `tools/api-collections/generate-all-collections.sh` - Substituído por Bruno Collections manuais?
2. `docs/configuration-templates/configure-environment.sh` - Usado OU apenas exemplo?

**Critério de Remoção:**
- ❌ Não foi usado nos últimos 3 meses (verificar git log)
- ❌ Funcionalidade duplicada por outra ferramenta
- ❌ Apenas template/exemplo (mover para `examples/`)

### **Fase 5: Atualização README Master (30 min)**

Adicionar seção em `scripts/README.md`:

```markdown
## 📍 Outros Scripts no Projeto

Além dos scripts principais em `/scripts/`, o projeto contém:

- **`/infrastructure/`**: Scripts de setup de banco, Keycloak, Docker
  - Ver [infrastructure/README.md](../infrastructure/README.md)
- **`/automation/`**: Scripts de CI/CD setup
  - Ver [automation/README.md](../automation/README.md)
- **`/build/`**: Scripts de build e migrations (alguns deprecados)
  - Ver [build/README.md](../build/README.md)
- **`/.github/scripts/`**: Scripts usados nos workflows CI/CD
- **`/tools/`**: Ferramentas auxiliares (avaliar necessidade)

**⚠️ Scripts deprecados**: Foram movidos para `*/deprecated/` e NÃO devem ser executados.
```

---

## 📈 Métricas de Sucesso

| Métrica | Antes | Depois |
|---------|-------|--------|
| **Scripts ativos** | 32 | ~22 |
| **Scripts documentados** | 18/32 (56%) | 22/22 (100%) |
| **Duplicações** | 6 | 0 |
| **READMEs completos** | 3 | 6 |

---

## 🔗 Referências

- [scripts/README.md](../scripts/README.md) - Scripts principais
- [automation/README.md](../automation/README.md) - CI/CD setup
- [build/README.md](../build/README.md) - Build tools
- [infrastructure/README.md](../infrastructure/README.md) - Infrastructure (a criar)

---

**Última Atualização**: 12 Dez 2025  
**Responsável**: Auditoria Sprint 3-P2
