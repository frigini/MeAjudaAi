# 🧪 Estratégia de Testes Multi-Ambientes

Este projeto implementa uma estratégia de testes em múltiplos ambientes para otimizar velocidade e cobertura.

## 🎯 Ambientes Disponíveis

### 1. **Testing Environment** ⚡ (Rápido)
- **Uso**: Testes unitários de API endpoints
- **Fixture**: `AspireAppFixture` 
- **Características**:
  - ✅ PostgreSQL via TestContainers
  - ✅ Autenticação mock (`TestAuthenticationHandler`)
  - ❌ RabbitMQ desabilitado (`NoOpMessageBus`)
  - ❌ Redis desabilitado (falha silenciosa)
  - ⚡ **~13-30 segundos** de startup

### 2. **Integration Environment** 🔗 (Completo)
- **Uso**: Testes de integração entre módulos
- **Fixture**: `AspireIntegrationFixture`
- **Características**:
  - ✅ PostgreSQL via TestContainers
  - ✅ Redis para cache distribuído
  - ✅ RabbitMQ para comunicação entre módulos
  - ✅ Autenticação mock (`TestAuthenticationHandler`)
  - 🐌 **~45-60 segundos** de startup

### 3. **Development Environment** 🚀 (Local)
- **Uso**: Desenvolvimento local
- **Características**:
  - ✅ Todos os serviços externos
  - ✅ Swagger UI completo
  - ✅ Logs detalhados

## 📝 Como Usar

### Testes Rápidos de API (Testing)
```csharp
public class UsersApiTests : ApiTestBase
{
    public UsersApiTests(AspireAppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact] 
    public async Task GetUsers_ShouldReturnOk()
    {
        // Teste rápido sem dependências externas
    }
}
```csharp
### Testes de Integração Completa (Integration)
```csharp
public class UsersIntegrationTests : IntegrationTestBase  
{
    public UsersIntegrationTests(AspireIntegrationFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task CreateUser_ShouldTriggerEvents()
    {
        // Teste completo com RabbitMQ e Redis
        await WaitForMessageProcessing(); // Helper para aguardar eventos
    }
}
```csharp
## 🔄 Configurações por Ambiente

| Recurso | Testing | Integration | Development |
|---------|---------|-------------|-------------|
| PostgreSQL | ✅ TestContainers | ✅ TestContainers | ✅ Local |
| Redis | ❌ Mock | ✅ Local/Container | ✅ Local |
| RabbitMQ | ❌ NoOp | ✅ Local/Container | ✅ Local |
| Auth | ✅ Mock | ✅ Mock | ❌ Real JWT |
| Swagger | ❌ | ✅ | ✅ |
| Startup Time | ~13-30s | ~45-60s | ~5-10s |

## 🚀 Comandos de Teste

```bash
# Testes rápidos (Testing environment)
dotnet test --filter "ApiTests"

# Testes de integração (Integration environment) 
dotnet test --filter "IntegrationTests"

# Todos os testes
dotnet test
```csharp
## 📋 Boas Práticas

1. **Use Testing** para a maioria dos testes de API
2. **Use Integration** apenas quando precisar testar:
   - Comunicação entre módulos via eventos
   - Comportamento com cache Redis
   - Fluxos end-to-end completos
3. **Evite** Integration desnecessariamente (é mais lento)
4. **Organize** testes em namespaces claros (`*.Api.*` vs `*.Integration.*`)

## 🔧 Configuração de CI/CD

```yaml
# Pipeline sugerido
stages:
  - fast-tests:    # Testing environment (~2-5 min)
      filter: "ApiTests"
  - integration:   # Integration environment (~10-15 min) 
      filter: "IntegrationTests"
      depends: fast-tests
```text
## 🎯 Resultado

- ⚡ **95%** dos testes executam rapidamente (Testing)
- 🔗 **5%** dos testes validam integração completa (Integration)
- 🚀 **Feedback rápido** para desenvolvimento
- 🛡️ **Cobertura completa** para deploy