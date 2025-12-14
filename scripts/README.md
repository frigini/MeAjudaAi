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

#### `seed-dev-data.ps1` - Seed Dados de Desenvolvimento
**Uso:**
```powershell
# Quando executar API diretamente (dotnet run) - usa default http://localhost:5000
.\scripts\seed-dev-data.ps1

# Quando usar Aspire orchestration - override para portas Aspire
.\scripts\seed-dev-data.ps1 -ApiBaseUrl "https://localhost:7524"
# ou
.\scripts\seed-dev-data.ps1 -ApiBaseUrl "http://localhost:5545"

# Seed padrão (Development)
.\scripts\seed-dev-data.ps1
```

**Funcionalidades:**
- Popula categorias de serviços
- Cria serviços básicos
- Adiciona cidades permitidas
- Cria usuários de teste
- Gera providers de exemplo

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
Localizados em `automation/` - documentados em [automation/README.md](../automation/README.md)

### Build Scripts
Localizados em `build/` - documentados em [build/README.md](../build/README.md)

---

## 📊 Resumo

- **Total de scripts:** 4 PowerShell essenciais
- **Foco:** Migrations, seed de dados, export de API
- **Filosofia:** Apenas scripts com utilidade clara e automação
