# 🌱 Database Seeds - Essential Domain Data

Scripts SQL para popular dados **essenciais de domínio** no PostgreSQL.

**Localização:** `infrastructure/database/seeds/` (parte da infraestrutura do banco)

---

## 🔄 Execução Automática

Estes seeds são executados **automaticamente** pelo Docker Compose durante a inicialização do container PostgreSQL:

```yaml
# infrastructure/compose/base/postgres.yml
volumes:
  - ../../database:/docker-entrypoint-initdb.d
```

O script `infrastructure/database/01-init-meajudaai.sh` executa os seeds após criar schemas/roles/permissions.

---

## 📋 Ordem de Execução

**Automática via Docker Compose:**
1. `modules/*/00-roles.sql` - Roles por módulo
2. `modules/*/01-permissions.sql` - Permissões por módulo
3. `views/cross-module-views.sql` - Views cross-module
4. **`seeds/*.sql`** - **Data seeds (aqui!)** ← Executado automaticamente

**Manual (pós-migrations):**
```powershell
# Executar todos os seeds em ordem
Get-ChildItem infrastructure/database/seeds/*.sql | Sort-Object Name | ForEach-Object {
    psql -h localhost -U meajudaai_user -d meajudaai_service_catalogs -f $_.FullName
}

# Ou executar individual
psql -h localhost -U meajudaai_user -d meajudaai_service_catalogs -f infrastructure/database/seeds/01-seed-service-catalogs.sql
```

---

## 📋 Seeds Disponíveis

| # | Script | Módulo | Descrição | Itens |
|---|--------|--------|-----------|-------|
| 01 | `01-seed-service-catalogs.sql` | ServiceCatalogs | Categorias e serviços essenciais | 8 categorias + 12 serviços |

---

## ❓ FAQ

### Por que só ServiceCatalogs tem seed?

**Apenas módulos com dados essenciais de domínio precisam de seeds SQL.**

| Módulo | Precisa Seed? | Motivo |
|--------|---------------|--------|
| **ServiceCatalogs** | ✅ Sim | Categorias e serviços padrão do sistema |
| Users | ❌ Não | Usuários são cadastrados via Keycloak |
| Providers | ❌ Não | Prestadores se cadastram via API |
| Documents | ❌ Não | Documentos são upload de usuários |
| Locations | ❌ Não | AllowedCities são configuração (não domínio) |

### Diferença entre seed SQL e seed PowerShell?

| Tipo | Quando | Propósito |
|------|--------|-----------|
| **SQL** (`scripts/database/`) | Após migrations, SEMPRE | Dados essenciais de domínio |
| **PowerShell** (`scripts/seed-dev-data.ps1`) | Manual, OPCIONAL | Dados de teste para desenvolvimento |

---

## 📝 Convenções

- **Prefixo numérico**: `01-`, `02-`, etc. (define ordem de execução)
- **Idempotente**: Todos os scripts verificam se dados já existem antes de inserir
- **UUIDs fixos**: Usar UUIDs determinísticos para referências entre módulos
- **Comentários**: Explicar propósito de cada bloco de dados

---

## 🔮 Futuros Seeds

Quando novos módulos precisarem de dados essenciais:

```sql
-- Exemplo: 02-seed-other-module.sql
-- APENAS se o módulo tiver dados de domínio padrão
-- (configurações do sistema, tipos pré-definidos, etc.)
```
