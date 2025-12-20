# Infraestrutura de Testes - TestContainers

## Visão Geral

A infraestrutura de testes do MeAjudaAi utiliza **TestContainers** para criar ambientes isolados e reproduzíveis, eliminando dependências externas e garantindo testes confiáveis.

## Arquitetura

### Componentes Principais

```text
TestContainerTestBase (Base class para E2E)
├── PostgreSQL Container (Banco de dados isolado)
├── Redis Container (Cache isolado)
├── MockKeycloakService (Autenticação mock)
└── WebApplicationFactory (API configurada)
```

### TestContainerTestBase

Classe base que fornece:
- **Containers Docker** automaticamente gerenciados
- **HttpClient** pré-configurado com autenticação
- **Service Scope** para acesso ao DI container
- **Cleanup automático** após cada teste
- **Faker** para geração de dados de teste

## Configuração

### Requisitos

- Docker Desktop instalado e rodando
- .NET 10.0 SDK
- Pacotes NuGet:
  - `Testcontainers.PostgreSql`
  - `Testcontainers.Redis`
  - `Microsoft.AspNetCore.Mvc.Testing`

### Variáveis de Ambiente

A infraestrutura sobrescreve automaticamente as configurações para testes:

```json
{
  "Keycloak:Enabled": false,  // Usa MockKeycloakService
  "Database:Host": "<container-host>",  // Provido pelo TestContainer
  "Redis:Configuration": "<container-config>"  // Provido pelo TestContainer
}
```

## Como Usar

### Criar um Novo Teste E2E

```csharp
using MeAjudaAi.E2E.Tests.Base;

public class MeuModuloE2ETests : TestContainerTestBase
{
    [Fact]
    public async Task DeveRealizarOperacao()
    {
        // Arrange
        AuthenticateAsAdmin(); // Opcional: autentica como admin
        
        var request = new
        {
            Campo1 = Faker.Lorem.Word(),
            Campo2 = Faker.Random.Int(1, 100)
        };

        // Act
        var response = await PostJsonAsync("/api/v1/meu-endpoint", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Acessar o Banco de Dados Diretamente

```csharp
[Fact]
public async Task DeveValidarPersistencia()
{
    // Act - Criar via API
    await PostJsonAsync("/api/v1/endpoint", data);

    // Assert - Verificar no banco
    await WithServiceScopeAsync(async services =>
    {
        var context = services.GetRequiredService<MeuDbContext>();
        var entity = await context.MinhasEntidades.FirstOrDefaultAsync();
        
        entity.Should().NotBeNull();
        entity!.Propriedade.Should().Be(valorEsperado);
    });
}
```

### Autenticação em Testes

```csharp
// Sem autenticação (anônimo)
var response = await ApiClient.GetAsync("/api/v1/public");

// Como usuário autenticado
AuthenticateAsUser();
var response = await ApiClient.GetAsync("/api/v1/user-endpoint");

// Como administrador
AuthenticateAsAdmin();
var response = await ApiClient.GetAsync("/api/v1/admin-endpoint");
```

## MockKeycloakService

O `MockKeycloakService` substitui o Keycloak real em testes, fornecendo:

- ✅ Validação de tokens simulada
- ✅ Criação de usuários mock
- ✅ Claims personalizadas
- ✅ Operações sempre bem-sucedidas

### Configuração Automática

O mock é registrado automaticamente quando `Keycloak:Enabled = false`:

```csharp
if (!keycloakSettings.Enabled)
{
    services.AddSingleton<IKeycloakService, MockKeycloakService>();
}
```

## Desempenho

### Tempos Típicos

- **Inicialização dos containers**: ~4-6 segundos
- **Primeiro teste**: ~6-8 segundos
- **Testes subsequentes**: ~0.5-2 segundos
- **Cleanup**: ~1-2 segundos

### Otimizações

1. **Reutilização de containers**: Containers são compartilhados por classe de teste
2. **Cleanup assíncrono**: Disparo acontece em background
3. **Pooling de conexões**: PostgreSQL usa connection pooling
4. **Cache de schemas**: Migrações são aplicadas uma vez

## Boas Práticas

### ✅ Fazer

- Usar `TestContainerTestBase` como base para testes E2E
- Limpar dados entre testes usando `WithServiceScopeAsync`
- Usar `Faker` para geração de dados realistas
- Testar fluxos completos (API → Application → Domain → Infrastructure)
- Verificar persistência no banco quando relevante

### ❌ Evitar

- Conectar a banco de dados externo (localhost:5432)
- Depender do Aspire ou infraestrutura externa
- Compartilhar estado entre testes
- Hardcodear dados de teste (use Faker)
- Misturar testes unitários com E2E

## Troubleshooting

### Docker não está rodando

```bash
Error: Docker daemon is not running
```

**Solução**: Iniciar Docker Desktop

### Porta já em uso

```bash
Error: Port 5432 is already allocated
```

**Solução**: Os TestContainers usam portas dinâmicas. Se persistir, reiniciar Docker.

### Timeout na inicialização

```bash
Error: Container failed to start within timeout
```

**Solução**: 
1. Verificar se Docker tem recursos suficientes
2. Aumentar timeout em `PostgreSqlContainer` se necessário

### Testes lentos

**Soluções**:
1. Rodar testes em paralelo (xUnit faz por padrão)
2. Reduzir número de dados criados
3. Usar `InlineData` para testes parametrizados

## Estrutura de Testes

```text
tests/MeAjudaAi.E2E.Tests/
├── Base/
│   ├── TestContainerTestBase.cs      # Base class principal
│   ├── TestTypes.cs                   # Tipos reutilizáveis
│   └── MockKeycloakService.cs         # Mock de autenticação
├── Modules/
│   ├── Users/
│   │   └── UsersEndToEndTests.cs     # Testes E2E de Users
│   ├── ServiceCatalogs/
│   │   └── ServiceCatalogsEndToEndTests.cs  # Testes E2E de ServiceCatalogs
│   └── Providers/
│       └── ProvidersEndToEndTests.cs # Testes E2E de Providers
├── Integration/
│   ├── ModuleIntegrationTests.cs     # Integração entre módulos
│   └── ServiceCatalogsModuleIntegrationTests.cs
└── Infrastructure/
    └── InfrastructureHealthTests.cs  # Testes de saúde da infra
```

## Migração de Testes Existentes

### De testes sem TestContainers

```csharp
// Antes
public class MeuTeste
{
    [Fact]
    public async Task Teste()
    {
        var client = new HttpClient();
        // ...
    }
}

// Depois
public class MeuTeste : TestContainerTestBase
{
    [Fact]
    public async Task Teste()
    {
        // ApiClient já disponível
        var response = await ApiClient.GetAsync(...);
    }
}
```

## Status Atual

### ✅ Implementado (Otimização IClassFixture)

#### TestContainerFixture (Nova Abordagem)
- **Pattern**: IClassFixture para compartilhar containers entre testes da mesma classe
- **Performance**: 70% mais rápido (32min → 8-10min quando Docker funciona)
- **Retry Logic**: 3 tentativas com exponential backoff para falhas transientes do Docker
- **Timeouts**: Aumentados de 1min → 5min para maior confiabilidade
- **Containers**: PostgreSQL (postgis/postgis:16-3.4), Redis (7-alpine), Azurite
- **Overhead**: Reduzido de 6s por teste para 6s por classe

#### Classes Migradas
- ✅ `InfrastructureHealthTests` (proof of concept)

#### Bloqueios Conhecidos
- ❌ **Docker Desktop local**: `InternalServerError` em `npipe://./pipe/docker_engine`
  - **Solução 1**: Reiniciar Docker Desktop ou WSL2 (`wsl --shutdown`)
  - **Solução 2**: Reinstalar Docker Desktop
  - **Workaround**: Testes E2E funcionam perfeitamente na pipeline CI/CD (GitHub Actions)

### 🔄 Próximos Passos

- [ ] Migrar 18 classes E2E restantes para IClassFixture (2-3 dias)
- [ ] Adicionar health checks no `TestContainerFixture.InitializeAsync`
- [ ] Implementar `CleanupDatabaseAsync` entre testes para isolamento
- [ ] Configurar paralelização via `xunit.runner.json`
- [ ] Adicionar retry logic para falhas de rede transientes

### 📊 E2E Tests Overview

**Total**: 96 testes E2E em 19 classes

**Categorias**:
- **Infrastructure** (6 testes): Health checks, database, Redis
- **Authorization** (8 testes): Permission-based authorization
- **Integration** (37 testes): Módulos comunicando, API versioning, domain events
- **Modules** (45 testes): Users (12), Providers (22), Documents (15), ServiceCatalogs (12)

**Pipeline Status**: ✅ Todos passam na CI/CD (GitHub Actions com Docker nativo)  
**Local Status**: ❌ Falhando devido a Docker Desktop

## Problemas Comuns e Soluções

### ⚠️ Timeout nos Containers Docker

**Sintoma:**
```
System.Threading.Tasks.TaskCanceledException: The operation was canceled.
  at Docker.DotNet.DockerClient.PrivateMakeRequestAsync(...)
```

**Causas:**
- Docker Desktop não está rodando
- Rede Docker configurada incorretamente
- Imagens não foram baixadas previamente
- Timeout padrão muito curto

**Soluções:**
1. Iniciar Docker Desktop e aguardar ficar pronto
2. Reiniciar WSL2: `wsl --shutdown`
3. Aumentar timeout em TestContainerFixture
4. Pré-baixar imagens: `docker pull postgis/postgis:16-3.4`

### ⚠️ Compartilhamento de Estado Entre Testes

**Problema:** Testes podem compartilhar dados e afetar uns aos outros

**Solução:**
```csharp
private async Task CleanupDatabaseAsync()
{
    await WithServiceScopeAsync(async services =>
    {
        var db = services.GetRequiredService<UsersDbContext>();
        await db.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE users CASCADE;
            TRUNCATE TABLE providers CASCADE;
        ");
    });
}
```

### ⚠️ Performance Ruim

**Números Típicos:**
- Sem otimização: ~32 minutos (19 classes × 6s setup cada)
- Com IClassFixture: ~8-10 minutos

**Otimizações Aplicadas:**
1. IClassFixture para compartilhar containers por classe
2. Retry logic para evitar falhas transientes
3. Timeouts aumentados para ambientes lentos
4. Connection pooling no PostgreSQL

## Referências

- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [xUnit Best Practices](https://xunit.net/docs/getting-started)

---

## Testes de Middleware

### Cobertura de Middlewares (Dez/2024)

**E2E Tests** (comportamento completo):
- ✅ BusinessMetricsMiddleware
- ✅ LoggingContextMiddleware (CorrelationId)
- ✅ SecurityHeadersMiddleware
- ✅ CompressionSecurityMiddleware
- ✅ RateLimitingMiddleware
- ✅ RequestLoggingMiddleware

**Integration Tests** (lógica específica):
- ✅ GeographicRestrictionMiddleware
- ✅ SecurityHeadersMiddleware (headers específicos)
- ✅ CompressionSecurityMiddleware (regras BREACH)

**Arquivos:**
- `tests/MeAjudaAi.E2E.Tests/Infrastructure/MiddlewareEndToEndTests.cs` (23 testes)
- `tests/MeAjudaAi.E2E.Tests/Infrastructure/RateLimitingEndToEndTests.cs` (4 testes)
- `tests/MeAjudaAi.Integration.Tests/Middleware/SecurityHeadersMiddlewareTests.cs` (10 testes)
- `tests/MeAjudaAi.Integration.Tests/Middleware/CompressionSecurityMiddlewareTests.cs` (6 testes)

### Problemas Corrigidos (Dez/2024)

1. **StaticFilesMiddleware duplicado**
   - ❌ Estava registrado 2x (UseApiServices + UseApiMiddlewares)
   - ✅ Removido de UseApiMiddlewares

2. **RequestLoggingMiddleware ordem incorreta**
   - ❌ Estava DEPOIS de Compression (não via response original)
   - ✅ Movido para logo APÓS ForwardedHeaders

3. **PermissionOptimizationMiddleware não registrado**
   - ✅ Já estava registrado via UsePermissionOptimization()

4. **CorrelationId não propagado**
   - ✅ Já estava sendo propagado via LoggingContextMiddleware

Veja detalhes em: [Middleware Testing Strategy](middleware-testing-strategy.md)

