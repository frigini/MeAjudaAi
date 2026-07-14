# 📊 Inventário de Scripts - MeAjudaAi

**Última atualização:** Jul 2026  
**Status:** Diretório `scripts/` removido — scripts de automação estão em `infrastructure/`

---

## 📝 Resumo Executivo

- **Scripts de automação:** Gerenciados via .NET Aspire e `infrastructure/`
- **Filosofia:** Aspire gerencia infra local; Docker Compose é fallback; CI/CD via GitHub Actions

---

## Scripts Ativos

### `infrastructure/` (3 scripts)

| Script | Finalidade |
|--------|-----------|
| `database/01-init-meajudaai.sh` | Entrypoint Docker (informativo) |
| `keycloak/scripts/keycloak-init-prod.sh` | Init Keycloak produção |
| `compose/standalone/postgres/init/02-custom-setup.sh` | Setup PostgreSQL standalone |

### `infrastructure/automation/` (2 scripts)

| Script | Finalidade |
|--------|-----------|
| `setup-cicd.ps1` | Setup CI/CD Azure |
| `setup-ci-only.ps1` | Setup CI sem deploy |

### `build/` (Makefile)

| Comando | Finalidade |
|---------|-----------|
| `make help` | Ver comandos disponíveis |
| `make dev` | Iniciar desenvolvimento |
| `make test` | Executar testes |

---

## Como Iniciar o Projeto

```powershell
# Recomendado: Aspire
cd src/Aspire/MeAjudaAi.AppHost
dotnet run

# Fallback: Docker Compose
docker compose -f infrastructure/compose/environments/development.yml up -d
```
