# Estratégia de Testes para Sistema de Permissões Type-Safe

Este documento detalha a estratégia de testes implementada para o sistema de permissões type-safe e modular do MeAjudaAi.

## Visão Geral da Pirâmide de Testes

A estratégia segue a pirâmide de testes com diferentes níveis de granularidade:

```text
                    /\
                   /  \
                  / E2E \
                 /________\
                /          \
               / Integration \
              /_______________\
             /                 \
            /   Unit Tests      \
           /____________________\
          /                      \
         /   Architecture Tests   \
        /_______________________\
```csharp
## 1. Testes Unitários (Base da Pirâmide)

### 📁 Localização
```text
tests/MeAjudaAi.Shared.Tests/Unit/Authorization/
├── PermissionTests.cs
├── PermissionServiceTests.cs
├── ClaimsPrincipalExtensionsTests.cs
└── UsersPermissionResolverTests.cs
```text
### 🎯 Objetivo
Testar componentes individuais isoladamente com mocks/stubs.

### ✅ Cobertura
- **PermissionTests**: Validação do enum `Permission` e suas extensões
  - Valores corretos dos Display attributes
  - Conversões string ↔ enum
  - Extração de módulos
  - Validação de unicidade
  
- **PermissionServiceTests**: Lógica do `PermissionService`
  - Resolução de permissões com cache
  - Verificação de permissões individuais e múltiplas
  - Invalidação de cache
  - Tratamento de casos edge (usuário inválido, etc.)
  
- **ClaimsPrincipalExtensionsTests**: Extensões para `ClaimsPrincipal`
  - Verificação de permissões em claims
  - Extração de informações de contexto (tenant, organização)
  - Comportamento com usuários não autenticados
  
- **UsersPermissionResolverTests**: Resolver específico do módulo Users
  - Mapeamento de roles para permissões
  - Organização de permissões por categoria
  - Validação de módulo correto

### 🛠️ Tecnologias
- **xUnit**: Framework de testes
- **Moq**: Mocking de dependências
- **Theory/InlineData**: Testes parametrizados

## 2. Testes de Integração

### 📁 Localização
```text
tests/MeAjudaAi.Integration.Tests/Authorization/
└── PermissionAuthorizationIntegrationTests.cs
```csharp
### 🎯 Objetivo
Testar a integração entre componentes do sistema de autorização em um ambiente controlado.

### ✅ Cobertura
- **Middleware de Autorização**: Integração com ASP.NET Core
- **Extensões de Endpoint**: `RequirePermission()`, `RequirePermissions()`, `RequireAnyPermission()`
- **Claims Transformation**: Conversão de tokens em claims de permissão
- **Authorization Handlers**: Processamento de requirements de autorização
- **Política de Autorização**: Aplicação de políticas baseadas em permissões

### 🛠️ Tecnologias
- **WebApplicationFactory**: Ambiente de teste integrado
- **TestServer**: Servidor de teste para ASP.NET Core
- **TestAuthenticationHandler**: Autenticação simulada para testes

### 📋 Cenários Testados
1. Endpoint com permissão válida → Sucesso (200)
2. Endpoint sem permissão → Forbidden (403)
3. Múltiplas permissões requeridas → Validação de todas
4. Qualquer permissão aceita → Validação de pelo menos uma
5. Admin do sistema → Acesso completo
6. Permissões específicas de módulo → Isolamento correto
7. Usuário não autenticado → Unauthorized (401)

## 3. Testes End-to-End (E2E)

### 📁 Localização
```text
tests/MeAjudaAi.E2E.Tests/Authorization/
└── PermissionAuthorizationE2ETests.cs
```csharp
### 🎯 Objetivo
Simular cenários reais de usuários com diferentes perfis acessando o sistema completo.

### ✅ Cobertura
- **Workflows Completos de Usuário**:
  - Usuário Básico: Perfil próprio, leitura limitada
  - Admin de Usuários: CRUD de usuários, área administrativa
  - Admin do Sistema: Acesso completo a todas as funcionalidades
  
- **Isolamento de Módulos**: Permissões de um módulo não dão acesso a outros
- **Concorrência**: Múltiplos usuários acessando simultaneamente
- **Cache de Permissões**: Consistência entre requisições
- **Tratamento de Erros**: Tokens inválidos, permissões insuficientes

### 🛠️ Tecnologias
- **WebApplicationFactory**: Aplicação completa
- **JWT Simulado**: Tokens de teste para diferentes perfis
- **HttpClient**: Simulação de requisições reais

### 📋 Fluxos Testados
1. **BasicUserWorkflow**: Operações permitidas e negadas
2. **UserAdminWorkflow**: Funcionalidades administrativas de usuários
3. **SystemAdminWorkflow**: Acesso completo ao sistema
4. **ModuleSpecificPermissions**: Isolamento entre módulos
5. **ConcurrentUsers**: Múltiplos usuários simultâneos
6. **PermissionCaching**: Cache funcionando entre requisições
7. **TokenValidation**: Tratamento de tokens inválidos

## 4. Testes de Arquitetura

### 📁 Localização
```text
tests/MeAjudaAi.Architecture.Tests/Authorization/
└── PermissionArchitectureTests.cs
```yaml
### 🎯 Objetivo
Garantir que o sistema de permissões siga as regras arquiteturais e mantenha a integridade do design.

### ✅ Cobertura
- **Convenções de Nomenclatura**:
  - PermissionResolvers terminam com "PermissionResolver"
  - Classes de permissão estão no namespace correto
  - Enum segue padrão "module:action"
  
- **Implementação de Interfaces**:
  - Todos os resolvers implementam `IModulePermissionResolver`
  - Requirements implementam `IAuthorizationRequirement`
  
- **Isolamento de Dependências**:
  - Shared não depende de módulos específicos
  - Resolvers estão na camada Application
  
- **Integridade dos Dados**:
  - Permissões têm Display attributes
  - Valores únicos no enum
  - Claims types são constantes

### 🛠️ Tecnologias
- **NetArchTest.Rules**: Análise de arquitetura baseada em regras
- **Reflection**: Verificação de metadados e convenções

### 📋 Regras Verificadas
1. **Modularidade**: Componentes respeitam fronteiras de módulos
2. **Encapsulamento**: Classes são sealed quando apropriado
3. **Convenções**: Nomenclatura e organização consistentes
4. **Dependências**: Não há dependências circulares problemáticas
5. **Configuração**: Serviços registrados com lifetime correto

## 5. Estratégia de Cobertura

### 📊 Métricas de Cobertura Esperadas
- **Testes Unitários**: 90%+ cobertura de código
- **Testes de Integração**: 100% dos fluxos principais
- **Testes E2E**: 100% dos cenários de usuário
- **Testes de Arquitetura**: 100% das regras definidas

### 🎯 Áreas Críticas (100% de cobertura obrigatória)
1. **Enum Permission** e extensões
2. **PermissionService** - lógica de resolução
3. **Claims transformations** - segurança crítica
4. **Authorization handlers** - aplicação de políticas
5. **Extensões de endpoint** - configuração de autorização

### 🔄 Testes de Regressão
- **CI/CD Pipeline**: Todos os testes executados em cada PR
- **Smoke Tests**: Cenários críticos em deployment
- **Performance Tests**: Cache de permissões não degrada performance

## 6. Configuração de Testes

### 🔧 Configuração Base
```csharp
// TestAuthenticationHandler para testes de integração
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    // Simula autenticação com claims específicos para cada teste
}

// Factory para testes E2E
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // Configuração completa da aplicação para testes
}
```csharp
### 📋 Dados de Teste Padronizados
```text
// Usuários padrão para testes
- "basic-user-123": Permissões básicas (UsersRead, UsersProfile)
- "user-admin-456": Admin de usuários (Users module completo)
- "system-admin-789": Admin completo (todas as permissões)
```bash
### 🏃 Execução dos Testes
```bash
# Testes unitários apenas
dotnet test --filter "Category=Unit"

# Testes de integração
dotnet test --filter "Category=Integration"

# Testes E2E
dotnet test --filter "Category=E2E"

# Testes de arquitetura
dotnet test --filter "Category=Architecture"

# Todos os testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```text
## 7. Cenários de Teste por Funcionalidade

### 🔐 Sistema de Permissões Core

| Componente | Unit | Integration | E2E | Architecture |
|------------|------|-------------|-----|--------------|
| Permission Enum | ✅ | ✅ | ✅ | ✅ |
| PermissionService | ✅ | ✅ | ✅ | ✅ |
| Claims Extensions | ✅ | ✅ | ✅ | ✅ |
| Authorization Handlers | ✅ | ✅ | ✅ | ✅ |

### 👥 Módulo Users

| Componente | Unit | Integration | E2E | Architecture |
|------------|------|-------------|-----|--------------|
| UsersPermissionResolver | ✅ | ✅ | ✅ | ✅ |
| UsersPermissions | ✅ | ⚪ | ✅ | ✅ |
| Users Endpoints | ⚪ | ✅ | ✅ | ⚪ |

### 🔗 Integração Sistema

| Aspecto | Unit | Integration | E2E | Architecture |
|---------|------|-------------|-----|--------------|
| Cache (HybridCache) | ✅ | ✅ | ✅ | ⚪ |
| Claims Transformation | ✅ | ✅ | ✅ | ⚪ |
| Endpoint Extensions | ⚪ | ✅ | ✅ | ✅ |

**Legenda**: ✅ Implementado | ⚪ Não aplicável | ❌ Pendente

## 8. Manutenção e Evolução

### 🔄 Adição de Novos Módulos
Quando um novo módulo for adicionado:

1. **Unit Tests**: Criar `{Module}PermissionResolverTests.cs`
2. **Integration Tests**: Adicionar cenários de endpoint do módulo
3. **E2E Tests**: Incluir workflow específico do módulo
4. **Architecture Tests**: Verificar se segue as convenções

### 📈 Melhoria Contínua
- **Code Review**: Verificar cobertura de novos testes
- **Refactoring**: Manter testes atualizados com mudanças
- **Performance**: Monitorar tempo de execução dos testes
- **Flaky Tests**: Identificar e corrigir testes instáveis

O sistema de testes está estruturado para crescer organicamente com o sistema de permissões, mantendo alta qualidade e confiabilidade em todas as camadas.