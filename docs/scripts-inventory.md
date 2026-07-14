# 📊 Inventário de Scripts - MeAjudaAi

**Última atualização:** 2025-12-13  
**Status:** Simplificado - apenas scripts essenciais

---

## 📝 Resumo Executivo

- **Total de scripts ativos:** 4 PowerShell  
- **Scripts removidos:** 20 (Bash redundantes + PowerShell coverage)
- **Documentação:** 100% (todos os 4 scripts ativos documentados em scripts/README.md)
- **Filosofia:** Manter apenas scripts com utilidade clara e automação

---

## 📂 Localização: `/scripts/`

### Scripts Ativos (4)

| Script | Tipo | Finalidade | Status | Automação |
|--------|------|------------|--------|-----------|
| `ef-migrate.ps1` | PowerShell | Entity Framework migrations | ✅ Ativo | ✅ Sim |
| `migrate-all.ps1` | PowerShell | Migrations de todos os módulos | ✅ Ativo | ✅ Sim |
| `export-openapi.ps1` | PowerShell | Export especificação OpenAPI | ✅ Ativo | ✅ Sim |
| `seed-dev-data.ps1` | PowerShell | Seed dados de desenvolvimento | ✅ Ativo | ✅ Sim |

**Documentação:** [scripts/README.md](../scripts/README.md) - Todos os scripts estão documentados

---

## 🗑️ Scripts Removidos (20 total)

### Bash Scripts - Redundantes para Ambiente Windows (7)

| Script | Motivo da Remoção | Data |
|--------|------------------|------|
| `dev.sh` | Redundante - uso PowerShell/dotnet diretamente | 2025-12-13 |
| `test.sh` | Redundante - uso `dotnet test` diretamente | 2025-12-13 |
| `deploy.sh` | Não utilizado - deploy via Azure/GitHub Actions | 2025-12-13 |
| `optimize.sh` | Over-engineering - configurações via runsettings | 2025-12-13 |
| `setup.sh` | Não utilizado - setup via Aspire/Docker Compose | 2025-12-13 |
| `utils.sh` | 586 linhas não utilizadas | 2025-12-13 |
| `seed-dev-data.sh` | Duplicado - mantido apenas .ps1 | 2025-12-13 |

### PowerShell Coverage - Redundantes (7)

| Script | Motivo da Remoção | Data |
|--------|------------------|------|
| `aggregate-coverage-local.ps1` | Redundante com `dotnet test --collect` | 2025-12-13 |
| `test-coverage-like-pipeline.ps1` | Redundante - uso config/coverage.runsettings | 2025-12-13 |
| `generate-clean-coverage.ps1` | Over-engineering - filtros via coverlet.json | 2025-12-13 |
| `analyze-coverage-detailed.ps1` | Não utilizado - análise via ReportGenerator | 2025-12-13 |
| `find-coverage-gaps.ps1` | Não utilizado - gaps visíveis no report HTML | 2025-12-13 |
| `monitor-coverage.ps1` | Não utilizado - histórico via GitHub Actions | 2025-12-13 |
| `track-coverage-progress.ps1` | Não utilizado - tracking via badges/CI | 2025-12-13 |

---

## 📂 Outros Diretórios com Scripts

### `/infrastructure/` (3 scripts ativos)

**Documentação:** [infrastructure/README.md](../infrastructure/README.md)

- Database: `01-init-meajudaai.sh`
- Keycloak: `keycloak-init-prod.sh` (dev realm import é automático via Aspire)
- Docker: `02-custom-setup.sh`

### `/infrastructure/automation/` (2 scripts ativos)

**Documentação:** [infrastructure/automation/README.md](../infrastructure/automation/README.md)

- `setup-cicd.ps1` - Setup completo CI/CD com Azure
- `setup-ci-only.ps1` - Setup apenas CI sem custos

### `/build/` (2 scripts ativos)

**Localização:** `/build/README.md` - Contém Makefile e scripts de instalação do .NET SDK

- `dotnet-install.sh` - Instalação customizada do .NET SDK
- `Makefile` - Comandos make para build/test/deploy

### `/.github/workflows/` (scripts inline)

Scripts embutidos nos workflows YAML do GitHub Actions

---

## 📊 Métricas

| Métrica | Antes | Depois | Mudança |
|---------|-------|--------|---------|
| Scripts em /scripts/ | 19 | 4 | -79% |
| Linhas de código | ~5000 | ~800 | -84% |
| Documentação | 44% | 100% | +56pp |
| Scripts obsoletos | 14 | 0 | -100% |
| Manutenção necessária | Alta | Baixa | ⬇️ |

---

## ✅ Limpeza Realizada

1. ✅ Removidos 7 scripts Bash redundantes para ambiente Windows
2. ✅ Removidos 7 scripts PowerShell de coverage (over-engineering)
3. ✅ Mantidos apenas 4 scripts essenciais com automação clara
4. ✅ Documentação atualizada refletindo filosofia "delete don't deprecate"
5. ✅ README simplificado focando nos scripts ativos

---

## 🎯 Filosofia de Manutenção

**Critérios para manter um script:**
1. ✅ Tem automação clara (usado em CI/CD ou desenvolvimento diário)
2. ✅ Resolve problema que não pode ser feito com ferramentas nativas (.NET CLI, Docker, etc)
3. ✅ É mantido e atualizado regularmente

**Critérios para remover:**
1. ❌ Script "one-time" que já foi executado (migrations)
2. ❌ Duplicação de funcionalidade (PS1 vs SH)
3. ❌ Over-engineering (scripts complexos quando solução simples existe)
4. ❌ Não utilizado há mais de 3 meses sem justificativa

---

**Mantido por:** Equipe MeAjudaAi  
**Última revisão:** Sprint 3 Parte 2 (Dezembro 2025)
