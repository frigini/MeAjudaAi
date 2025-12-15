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
- **SQL Scripts**: Dados essenciais de domínio (executar após migrations)
- **PowerShell/API**: Dados de teste/desenvolvimento (executar manualmente quando necessário)

---

#### `seed-service-catalogs.sql` - Seed Dados Essenciais (SQL)
**Quando executar:** Após migrations, **antes** de iniciar a aplicação pela primeira vez

**Uso:**
```powershell
# Via psql direto
psql -h localhost -U meajudaai_user -d meajudaai_service_catalogs -f scripts/seed-service-catalogs.sql

# Via Docker Compose
docker exec -i meajudaai-postgres psql -U meajudaai_user -d meajudaai_service_catalogs < scripts/seed-service-catalogs.sql

# Ou usando ConnectionString do appsettings
$connectionString = "Host=localhost;Database=meajudaai_service_catalogs;Username=meajudaai_user;Password=your_password"
psql "$connectionString" -f scripts/seed-service-catalogs.sql
```

**Funcionalidades:**
- **Dados ESSENCIAIS** de domínio que devem existir em TODOS os ambientes
- Insere 8 categorias padrão (Saúde, Educação, Assistência Social, Jurídico, Habitação, Transporte, Alimentação, Trabalho e Renda)
- Insere 12 serviços essenciais vinculados às categorias
- **Idempotente**: não insere se dados já existem (verifica antes)
- **Usa UUIDs fixos** para referências consistentes entre ambientes

**Categorias inseridas:**
1. **Saúde**: Consulta Médica Geral, Atendimento Psicológico, Fisioterapia
2. **Educação**: Reforço Escolar, Alfabetização de Adultos
3. **Assistência Social**: Orientação Social, Apoio a Famílias
4. **Jurídico**: Orientação Jurídica Gratuita, Mediação de Conflitos
5. **Habitação**: Reparos Residenciais
6. **Transporte** (categoria criada, serviços para expansão futura)
7. **Alimentação** (categoria criada, serviços para expansão futura)
8. **Trabalho e Renda**: Capacitação Profissional, Intermediação de Emprego

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
- Keycloak rodando em http://localhost:8080
- Credenciais: admin/admin123

**Configuração:**
- Variável `API_BASE_URL`:
  - **Default `http://localhost:5000`** - use quando executar API diretamente via `dotnet run`
  - **Override com `-ApiBaseUrl`** - necessário quando usar Aspire orchestration (portas dinâmicas como `https://localhost:7524` ou `http://localhost:5545`)
- Apenas para ambiente: Development
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
2. `psql -f seed-service-catalogs.sql` (dados essenciais)
3. `.\seed-dev-data.ps1` (dados de teste - opcional)
