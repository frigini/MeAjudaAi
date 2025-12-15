# 🛠️ Scripts - MeAjudaAi

Scripts PowerShell essenciais para desenvolvimento e operações da aplicação.

---

## 📋 Scripts Disponíveis

### 🗄️ Banco de Dados e Migrations

#### `ef-migrate.ps1` - Entity Framework Migrations
**Uso:**
```powershell
# Aplicar migrações em todos os módulos
.\scripts\ef-migrate.ps1

# Aplicar em módulo específico
.\scripts\ef-migrate.ps1 -Module Providers

# Adicionar nova migração
.\scripts\ef-migrate.ps1 -Command add -Module Users -MigrationName "AddNewField"

# Ver status das migrações
.\scripts\ef-migrate.ps1 -Command status
```

**Funcionalidades:**
- Aplica migrações usando `dotnet ef`
- Suporta múltiplos módulos (Users, Providers)
- Comandos: migrate, add, remove, status
- Configuração via variáveis de ambiente

---

#### `migrate-all.ps1` - Migrations para Todos os Módulos
**Uso:**
```powershell
# Aplicar todas as migrações
.\scripts\migrate-all.ps1

# Ver status
.\scripts\migrate-all.ps1 -Command status

# Resetar bancos (CUIDADO!)
.\scripts\migrate-all.ps1 -Command reset
```

**Funcionalidades:**
- Descobre automaticamente todos os DbContexts
- Executa migrações em sequência
- Comandos: migrate, create, reset, status

---

### 📄 API e Documentação

#### `export-openapi.ps1` - Export OpenAPI Specification
**Uso:**
```powershell
# Export para arquivo padrão
.\scripts\export-openapi.ps1

# Export para arquivo específico
.\scripts\export-openapi.ps1 -OutputPath "api/frontend-api.json"
```

**Funcionalidades:**
- Exporta especificação OpenAPI da API
- Formato JSON compatível com ferramentas
- Usado para gerar cliente HTTP/Bruno Collections

---

### 🌱 Seed de Dados

**Estratégia de Seeding:**
- **SQL Seeds** (`infrastructure/database/seeds/`): Dados essenciais de domínio (executados automaticamente no Docker Compose)
- **PowerShell/API** (`scripts/seed-dev-data.ps1`): Dados de teste/desenvolvimento (executar manualmente quando necessário)

**IMPORTANTE:** Seeds SQL estão em `infrastructure/database/seeds/`, pois fazem parte da infraestrutura do banco de dados (executados com schema/roles/permissions).

---

#### Data Seeds Essenciais (SQL)
**Localização:** `infrastructure/database/seeds/` 

**Execução automática via Docker Compose:**
- Ao iniciar container PostgreSQL pela primeira vez
- Script `01-init-meajudaai.sh` executa seeds após criar schemas

**Execução manual (se necessário):**
```powershell
# Executar todos os seeds em ordem
Get-ChildItem infrastructure/database/seeds/*.sql | Sort-Object Name | ForEach-Object {
    psql -h localhost -U meajudaai_user -d meajudaai_service_catalogs -f $_.FullName
}
```

**Documentação completa:** Ver [infrastructure/database/seeds/README.md](../infrastructure/database/seeds/README.md)

---

#### `seed-dev-data.ps1` - Seed Dados de TESTE (PowerShell/API)
**Quando executar:** Manualmente, apenas quando precisar de dados de teste

**Uso:**
```powershell
# Quando executar API diretamente (dotnet run) - usa default http://localhost:5000
.\scripts\seed-dev-data.ps1

# Quando usar Aspire orchestration - override para portas Aspire
.\scripts\seed-dev-data.ps1 -ApiBaseUrl "https://localhost:7524"
# ou
.\scripts\seed-dev-data.ps1 -ApiBaseUrl "http://localhost:5545"
```

**Funcionalidades:**
- **Dados de TESTE** via API REST (requer API rodando e autenticação)
- Adiciona 10 cidades permitidas (capitais brasileiras) para testes
- Futuramente: usuários demo, providers fake para testes
- **NÃO** insere ServiceCategories/Services (isso é feito via SQL)

**Pré-requisitos:**
- API rodando em $ApiBaseUrl
- Keycloak rodando em <http://localhost:8080>
- Credenciais: admin/admin123

**Configuração:**
- Variável `API_BASE_URL`:
  - **Default `http://localhost:5000`** - use quando executar API diretamente via `dotnet run`
  - **Override com `-ApiBaseUrl`** - necessário quando usar Aspire orchestration (portas dinâmicas como `https://localhost:7524` ou `http://localhost:5545`)
- Apenas para ambiente: Development

---

## 📍 Outros Scripts no Projeto

### Infrastructure Scripts
Localizados em `infrastructure/` - documentados em [infrastructure/SCRIPTS.md](../infrastructure/SCRIPTS.md)

### Automation Scripts
Localizados em `infrastructure/automation/` - documentados em [infrastructure/automation/README.md](../infrastructure/automation/README.md)

### Build Scripts
Localizados em `build/` - documentados em [build/README.md](../build/README.md)

---

## 📊 Resumo

- **Total de scripts:** 5 PowerShell + 1 SQL
- **Foco:** Migrations, seed de dados, export de API
- **Filosofia:** Apenas scripts com utilidade clara e automação

### Estratégia de Seeding
| Tipo | Quando | Propósito | Exemplo |
|------|--------|-----------|---------|
| **SQL Scripts** | Após migrations | Dados essenciais de domínio | ServiceCategories, Services |
| **PowerShell/API** | Manualmente (testes) | Dados opcionais de teste | AllowedCities demo, Providers fake |

**Ordem de Execução:**
1. `dotnet ef database update` (migrations)
2. Docker Compose executa automaticamente `infrastructure/database/seeds/*.sql`
3. `.\seed-dev-data.ps1` (dados de teste - opcional, manual)
