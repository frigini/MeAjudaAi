# 📊 Inventário de Scripts - MeAjudaAi

**Última atualização:** 13 de dezembro de 2025  
**Status:** Simplificado - apenas scripts essenciais

---

## 📝 Resumo Executivo

- **Total de scripts ativos:** 4 PowerShell  
- **Scripts removidos:** 20 (Bash redundantes + PowerShell coverage)
- **Documentação:** 100%
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

**Documentação:** [scripts/README.md](../scripts/README.md)

---

## 🗑️ Scripts Removidos (20 total)

### Bash Scripts - Redundantes para Ambiente Windows (7)

| Script | Motivo da Remoção | Data |
|--------|------------------|------|
| `dev.sh` | Redundante - uso PowerShell/dotnet diretamente | 13/12/2025 |
| `test.sh` | Redundante - uso `dotnet test` diretamente | 13/12/2025 |
| `deploy.sh` | Não utilizado - deploy via Azure/GitHub Actions | 13/12/2025 |
| `optimize.sh` | Over-engineering - configurações via runsettings | 13/12/2025 |
| `setup.sh` | Não utilizado - setup via Aspire/Docker Compose | 13/12/2025 |
| `utils.sh` | 586 linhas não utilizadas | 13/12/2025 |
| `seed-dev-data.sh` | Duplicado - mantido apenas .ps1 | 13/12/2025 |

### PowerShell Coverage - Redundantes (7)

| Script | Motivo da Remoção | Data |
|--------|------------------|------|
| `aggregate-coverage-local.ps1` | Redundante com `dotnet test --collect` | 13/12/2025 |
| `test-coverage-like-pipeline.ps1` | Redundante - uso config/coverage.runsettings | 13/12/2025 |
| `generate-clean-coverage.ps1` | Over-engineering - filtros via coverlet.json | 13/12/2025 |
| `analyze-coverage-detailed.ps1` | Não utilizado - análise via ReportGenerator | 13/12/2025 |
| `find-coverage-gaps.ps1` | Não utilizado - gaps visíveis no report HTML | 13/12/2025 |
| `monitor-coverage.ps1` | Não utilizado - histórico via GitHub Actions | 13/12/2025 |
| `track-coverage-progress.ps1` | Não utilizado - tracking via badges/CI | 13/12/2025 |

---

## 📂 Outros Diretórios com Scripts

### `/infrastructure/` (9 scripts ativos)

**Documentação:** [infrastructure/SCRIPTS.md](../infrastructure/SCRIPTS.md)

- Database: `01-init-meajudaai.sh`, `create-module.ps1`, `test-database-init.*`
- Keycloak: `keycloak-init-dev.sh`, `keycloak-init-prod.sh`
- Docker: `setup-secrets.sh`, `verify-resources.sh`

### `/automation/` (2 scripts ativos)

**Documentação:** [automation/README.md](../automation/README.md)

- `setup-cicd.ps1` - Setup completo CI/CD com Azure
- `setup-ci-only.ps1` - Setup apenas CI sem custos

### `/build/` (2 scripts ativos)

**Documentação:** [build/README.md](../build/README.md)

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
