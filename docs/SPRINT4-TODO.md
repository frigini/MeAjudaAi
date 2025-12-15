# 📋 Sprint 4 - Pendências e Melhorias Futuras

Documento criado em: 15 de Dezembro de 2025  
Sprint: 4 - Health Checks + Data Seeding

---

## ✅ Completado na Sprint 4

### Health Checks
- ✅ `DatabasePerformanceHealthCheck` - Latência PostgreSQL (<100ms healthy, <500ms degraded)
- ✅ `ExternalServicesHealthCheck` - Keycloak (parcial - ver pendências abaixo)
- ✅ `HelpProcessingHealthCheck` - Sistema de processamento de ajuda
- ✅ Health UI Dashboard - `/health-ui` endpoint
- ✅ Configuração completa com AspNetCore.HealthChecks.UI 9.0.0

### Data Seeding
- ✅ `infrastructure/database/seeds/01-seed-service-catalogs.sql` (8 categorias + 12 serviços)
- ✅ Seed automático via Docker Compose
- ✅ `scripts/seed-dev-data.ps1` - Framework para dados de teste (AllowedCities)

### Estrutura do Projeto
- ✅ Reorganização: `automation/` → `infrastructure/automation/`
- ✅ Seeds SQL em `infrastructure/database/seeds/`
- ✅ Documentação atualizada

---

## ⏳ Pendências e TODOs

### 1. Health Checks - External Services (ALTA PRIORIDADE)

**Status:** Parcialmente implementado (apenas Keycloak)

**Faltam adicionar em `ExternalServicesHealthCheck.cs`:**

```csharp
// IBGE API (geolocalização)
try {
    var response = await httpClient.GetAsync(
        "https://servicodados.ibge.gov.br/api/v1/localidades/estados", 
        cancellationToken);
    results["ibge_api"] = new {
        status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
        response_time_ms = stopwatch.ElapsedMilliseconds
    };
}

// Azure Blob Storage (se configurado)
var blobConnectionString = configuration.GetConnectionString("AzureBlob");
if (!string.IsNullOrEmpty(blobConnectionString)) {
    // TODO: Implementar health check para Azure Blob Storage
    // Verificar se container existe e está acessível
}

// Redis (cache - se configurado)
var redisConnection = configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection)) {
    // TODO: Implementar health check para Redis
    // AspNetCore.HealthChecks.Redis já está instalado
}

// RabbitMQ (messaging - se configurado)
var rabbitMqConnection = configuration.GetConnectionString("RabbitMQ");
if (!string.IsNullOrEmpty(rabbitMqConnection)) {
    // TODO: Implementar health check para RabbitMQ
}
```

**Arquivo:** `src/Shared/Monitoring/ExternalServicesHealthCheck.cs`

---

### 2. Health Checks por Módulo (MÉDIA PRIORIDADE)

**Status:** NÃO implementado

**Objetivo:** Cada módulo deve expor health checks específicos de suas operações críticas.

**Implementação sugerida:**

```csharp
// src/Modules/Users/Infrastructure/HealthChecks/UsersHealthCheck.cs
public class UsersHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        // Verificar se pode:
        // 1. Conectar ao banco meajudaai_users
        // 2. Executar query básica (SELECT COUNT(*) FROM Users)
        // 3. Verificar Keycloak integration
        return HealthCheckResult.Healthy();
    }
}

// src/Modules/Providers/Infrastructure/HealthChecks/ProvidersHealthCheck.cs
public class ProvidersHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        // Verificar se pode:
        // 1. Conectar ao banco meajudaai_providers
        // 2. Verificar indexação de busca (se implementada)
        // 3. Verificar integração com Documents module
        return HealthCheckResult.Healthy();
    }
}

// src/Modules/Documents/Infrastructure/HealthChecks/DocumentsHealthCheck.cs
public class DocumentsHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        // Verificar se pode:
        // 1. Conectar ao banco meajudaai_documents
        // 2. Acessar Azure Blob Storage
        // 3. Verificar Hangfire jobs (se configurado)
        return HealthCheckResult.Healthy();
    }
}

// Similar para: Locations, ServiceCatalogs, Search
```

**Registro em cada módulo:**

```csharp
// src/Modules/Users/Infrastructure/ModuleExtensions.cs
services.AddHealthChecks()
    .AddCheck<UsersHealthCheck>("users_module", tags: ["ready", "module", "users"]);
```

---

### 3. Dados de Teste para Desenvolvimento (BAIXA PRIORIDADE)

**Status:** Framework criado, dados NÃO populados

**Arquivo:** `scripts/seed-dev-data.ps1`

**Faltam implementar:**

#### 3.1. Usuários de Teste

```powershell
# Adicionar ao seed-dev-data.ps1 após linha 87

Write-Host "👤 Seeding: Test Users (Keycloak)" -ForegroundColor Yellow

$testUsers = @(
    @{
        username = "admin@meajudaai.com"
        email = "admin@meajudaai.com"
        firstName = "Admin"
        lastName = "Sistema"
        role = "admin"
        password = "Admin@123"
    }
    @{
        username = "customer@test.com"
        email = "customer@test.com"
        firstName = "Cliente"
        lastName = "Teste"
        role = "customer"
        password = "Customer@123"
    }
    @{
        username = "provider@test.com"
        email = "provider@test.com"
        firstName = "Prestador"
        lastName = "Teste"
        role = "provider"
        password = "Provider@123"
    }
)

foreach ($user in $testUsers) {
    Write-Info "Criando usuário: $($user.username)"
    # TODO: Implementar criação via Keycloak Admin API
    # POST $keycloakUrl/admin/realms/meajudaai/users
}
```

#### 3.2. Providers de Exemplo

```powershell
# Adicionar ao seed-dev-data.ps1

Write-Host "🏢 Seeding: Test Providers" -ForegroundColor Yellow

$testProviders = @(
    @{
        name = "Clínica Saúde Bem-Estar"
        type = "Company"
        document = "12.345.678/0001-90"
        serviceIds = @("20000000-0000-0000-0000-000000000001") # Consulta Médica
        cityId = "3550308" # São Paulo
    }
    @{
        name = "João Silva - Psicólogo"
        type = "Individual"
        document = "123.456.789-00"
        serviceIds = @("20000000-0000-0000-0000-000000000002") # Atendimento Psicológico
        cityId = "3304557" # Rio de Janeiro
    }
)

foreach ($provider in $testProviders) {
    Write-Info "Criando provider: $($provider.name)"
    # TODO: Implementar via API POST /api/v1/providers
}
```

#### 3.3. Documentos de Teste

```powershell
# TODO: Upload de documentos fake para providers de teste
# Usar arquivos PDF/JPEG de exemplo em tests/fixtures/
```

---

### 4. Testes Automatizados (ALTA PRIORIDADE)

**Status:** NÃO implementados

#### 4.1. Unit Tests para Health Checks

```csharp
// tests/MeAjudaAi.Shared.Tests/Monitoring/DatabasePerformanceHealthCheckTests.cs
public class DatabasePerformanceHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenLatencyBelow100ms_ReturnsHealthy() { }
    
    [Fact]
    public async Task CheckHealthAsync_WhenLatencyBetween100And500ms_ReturnsDegraded() { }
    
    [Fact]
    public async Task CheckHealthAsync_WhenLatencyAbove500ms_ReturnsUnhealthy() { }
    
    [Fact]
    public async Task CheckHealthAsync_WhenConnectionFails_ReturnsUnhealthy() { }
}

// Similar para ExternalServicesHealthCheck, HelpProcessingHealthCheck
```

#### 4.2. Integration Tests para Data Seeding

```csharp
// tests/MeAjudaAi.Integration.Tests/Database/SeedTests.cs
public class ServiceCatalogsSeedTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Seed_ShouldInsert8Categories() 
    {
        // Arrange: Execute seed script
        await ExecuteSqlFile("infrastructure/database/seeds/01-seed-service-catalogs.sql");
        
        // Act: Query database
        var count = await _db.ServiceCategories.CountAsync();
        
        // Assert
        Assert.Equal(8, count);
    }
    
    [Fact]
    public async Task Seed_ShouldBeIdempotent() 
    {
        // Execute twice, should not duplicate
        await ExecuteSqlFile("infrastructure/database/seeds/01-seed-service-catalogs.sql");
        await ExecuteSqlFile("infrastructure/database/seeds/01-seed-service-catalogs.sql");
        
        var count = await _db.ServiceCategories.CountAsync();
        Assert.Equal(8, count); // Not 16!
    }
}
```

---

### 5. Documentação Faltante (MÉDIA PRIORIDADE)

#### 5.1. Guia de Health Checks

**Arquivo a criar:** `docs/health-checks.md`

**Conteúdo:**
- Como acessar `/health`, `/health/live`, `/health/ready`, `/health-ui`
- Interpretação de status (Healthy, Degraded, Unhealthy)
- Como adicionar novo health check customizado
- Thresholds configuráveis
- Alertas e monitoramento (integração futura com Azure Monitor/AppInsights)

#### 5.2. Guia de Data Seeding

**Arquivo a criar:** `docs/data-seeding.md`

**Conteúdo:**
- Estratégia: SQL vs PowerShell/API
- Como adicionar novo seed SQL
- Como adicionar dados de teste via API
- Ambientes: Development vs Production
- Troubleshooting

---

## 🔮 Melhorias Futuras (Post-MVP)

### 1. Health Checks Avançados

- [ ] Health checks com métricas customizadas (Prometheus format)
- [ ] Alertas automáticos via webhook quando Unhealthy
- [ ] Dashboard web customizado (além do padrão do AspNetCore.HealthChecks.UI)
- [ ] Histórico de health status (armazenar em banco)

### 2. Data Seeding Avançado

- [ ] Seed de dados realistas via Faker/Bogus
- [ ] Seed de imagens/documentos fake para blob storage
- [ ] Seed de histórico de atividades (audit trail)
- [ ] Geração de dados para testes de performance (volume)

### 3. Observabilidade

- [ ] Integração com OpenTelemetry
- [ ] Distributed tracing entre módulos
- [ ] Correlação de logs com health checks
- [ ] Dashboards no Grafana

---

## 📝 Notas de Implementação

### External Services - Dependências

| Serviço | Pacote | Status |
|---------|--------|--------|
| PostgreSQL | ✅ AspNetCore.HealthChecks.Npgsql 9.0.0 | Instalado |
| Redis | ✅ AspNetCore.HealthChecks.Redis 8.0.1 | Instalado |
| RabbitMQ | ⏳ Não instalado | TODO |
| Azure Blob | ⏳ Não instalado | TODO |

### Health Checks Tags

Use tags para filtrar health checks:
- `ready` - Verifica se app está pronta para receber tráfego
- `live` - Verifica se app está viva (não travada)
- `module` - Health check de módulo específico
- `external` - Serviços externos
- `database` - Verificações de banco de dados
- `business` - Lógica de negócio

---

## 🎯 Priorização para Próxima Sprint

**Sprint 5 - Recomendações:**

1. **ALTA:** Completar ExternalServicesHealthCheck (IBGE API, Azure Blob, Redis)
2. **ALTA:** Implementar testes unitários para health checks existentes
3. **MÉDIA:** Adicionar health checks por módulo (Users, Providers, Documents)
4. **BAIXA:** Expandir seed-dev-data.ps1 com usuários/providers de teste

**Estimativa:** 1-2 dias de trabalho

---

**Última atualização:** 15/12/2025 - Sprint 4  
**Autor:** GitHub Copilot  
**Status do Documento:** Em manutenção (atualizar conforme implementação)
