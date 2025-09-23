# Infraestrutura de Testes E2E - TestContainers

## Visão Geral

A nova infraestrutura de testes E2E usa **TestContainers** para criar ambientes isolados e confiáveis, eliminando dependências externas e problemas de configuração.

## Arquitetura

### `TestContainerTestBase`

Base class para todos os testes E2E que:

- 🐳 **TestContainers**: Cria containers Docker isolados para PostgreSQL e Redis
- 🔧 **WebApplicationFactory**: Configura a aplicação com infraestrutura de teste
- 🗃️ **Database**: Aplica schema automaticamente usando `EnsureCreated()`
- ⚡ **Performance**: Otimizado para execução rápida e limpeza automática

### Estrutura de Arquivos

```
tests/MeAjudaAi.E2E.Tests/
├── Base/
│   ├── TestContainerTestBase.cs      # Base class principal
│   └── ...
├── Simple/
│   └── InfrastructureHealthTests.cs  # Testes de infraestrutura
├── UsersEndToEndTests.cs             # Testes E2E de usuários
├── AuthenticationTests.cs            # Testes de autenticação
└── README-TestContainers.md          # Esta documentação
```

### Benefícios

✅ **Isolamento**: Cada teste roda em containers limpos
✅ **Confiabilidade**: Sem dependência de serviços externos
✅ **Paralelização**: Testes podem rodar em paralelo sem conflitos
✅ **CI/CD Ready**: Funciona em qualquer ambiente com Docker
✅ **Desenvolvimento**: Não requer setup manual de infraestrutura

## Como Usar

### 1. Teste Básico de API

```csharp
public class MyApiTests : TestContainerTestBase
{
    [Fact]
    public async Task GetEndpoint_Should_Return_Success()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/my-endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### 2. Teste com Dados

```csharp
[Fact]
public async Task CreateEntity_Should_Persist_To_Database()
{
    // Arrange
    var request = new { Name = "Test", Value = 123 };

    // Act
    var response = await PostJsonAsync("/api/entities", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    
    // Verificar no banco
    await WithServiceScopeAsync(async services =>
    {
        var context = services.GetRequiredService<MyDbContext>();
        var entity = await context.Entities.FirstAsync();
        entity.Name.Should().Be("Test");
    });
}
```

### 3. Acesso Direto ao Banco

```csharp
[Fact]
public async Task DirectDatabaseAccess_Should_Work()
{
    // Arrange - Criar dados diretamente no banco
    await WithServiceScopeAsync(async services =>
    {
        var context = services.GetRequiredService<MyDbContext>();
        context.Entities.Add(new Entity { Name = "Direct" });
        await context.SaveChangesAsync();
    });

    // Act & Assert
    var response = await ApiClient.GetAsync("/api/entities");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Métodos Disponíveis

### HTTP Helpers
- `PostJsonAsync<T>(string uri, T content)` - POST com JSON
- `PutJsonAsync<T>(string uri, T content)` - PUT com JSON  
- `ReadJsonAsync<T>(HttpResponseMessage response)` - Ler JSON da resposta

### Database Helpers
- `WithServiceScopeAsync(Func<IServiceProvider, Task> action)` - Executar com scope de serviços
- `WithServiceScopeAsync<T>(Func<IServiceProvider, Task<T>> action)` - Executar e retornar valor

### Propriedades
- `ApiClient` - HttpClient configurado para a API
- `Faker` - Gerador de dados fake (Bogus)
- `JsonOptions` - Opções de serialização JSON consistentes

## Configuração dos Containers

### PostgreSQL
- **Image**: `postgres:13-alpine`
- **Database**: `meajudaai_test`
- **Credentials**: `postgres/test123`
- **Schema**: Criado automaticamente via `EnsureCreated()`

### Redis
- **Image**: `redis:7-alpine`
- **Port**: Alocado dinamicamente
- **Cache**: Disponível para testes de cache

### Serviços Desabilitados em Teste
- **Keycloak**: Desabilitado para maior performance e confiabilidade
- **RabbitMQ**: Desabilitado por padrão
- **Logging**: Reduzido ao mínimo

## Performance

- ⚡ **Containers**: Reutilizados quando possível
- 🧹 **Cleanup**: Automático após cada teste
- 📊 **Logs**: Minimizados para reduzir overhead
- ⏱️ **Timeouts**: Otimizados para CI/CD

## Comparação com Aspire

| Aspecto | TestContainers | Aspire AppHost |
|---------|----------------|----------------|
| **Isolamento** | ✅ Total | ❌ Compartilhado |
| **Performance** | ✅ Rápido | ❌ Lento |
| **Confiabilidade** | ✅ Alta | ❌ Instável |
| **Setup** | ✅ Zero | ❌ Complexo |
| **CI/CD** | ✅ Nativo | ❌ Problemático |

## Migração Concluída

Os seguintes testes foram migrados do Aspire para TestContainers:

### ✅ Migrados
- `TestContainerHealthTests.cs` → `InfrastructureHealthTests.cs`
- `UsersEndToEndTests.cs` → **Migrado** para `TestContainerTestBase`
- `KeycloakIntegrationTests.cs` → **Substituído** por `AuthenticationTests.cs`

### 📁 Arquivos de Backup
- `UsersEndToEndTests.cs.backup` - Versão original
- `KeycloakIntegrationTests.cs.backup` - Versão original

## Migração de Testes Existentes

Para criar novos testes E2E, use `TestContainerTestBase`:

1. **Definir herança**:
   ```csharp
   public class MyTests : TestContainerTestBase
   ```

2. **Atualizar usings**:
   ```csharp
   using MeAjudaAi.E2E.Tests.Base;
   ```

3. **Ajustar endpoints**:
   ```csharp
   // Atualizar de /api/v1/users para /api/users
   // Remover campos que não existem na API atual (ex: Password)
   ```

4. **Usar novos helpers** para acesso ao banco e HTTP

## Exemplos Completos

### Testes de Usuários
Ver `UsersEndToEndTests.cs` para exemplo completo de:
- Testes de API CRUD
- Manipulação de dados
- Verificação de persistência
- Uso de helpers

### Testes de Autenticação  
Ver `AuthenticationTests.cs` para exemplo de:
- Testes sem dependências externas
- Validação de comportamento sem Keycloak
- Testes de endpoints públicos/protegidos

### Testes de Infraestrutura
Ver `InfrastructureHealthTests.cs` para exemplo de:
- Validação de conectividade PostgreSQL
- Validação de conectividade Redis
- Health checks da API

## Troubleshooting

### Docker não encontrado
Verifique se Docker Desktop está rodando.

### Testes lentos
TestContainers reutiliza containers quando possível. Primeira execução é mais lenta.

### Conflitos de porta
TestContainers aloca portas dinamicamente, evitando conflitos.

### Problemas de conectividade
Containers são criados na mesma rede Docker, conectividade é automática.