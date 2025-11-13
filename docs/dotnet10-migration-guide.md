# .NET 10 Migration Guide

Este documento detalha as mudanças realizadas na migração de .NET 9 para .NET 10 e as breaking changes que devem ser consideradas.

## Sumário

1. [Alterações Realizadas](#alterações-realizadas)
2. [Breaking Changes do .NET 10](#breaking-changes-do-net-10)
3. [Novas Funcionalidades do C# 14](#novas-funcionalidades-do-c-14)
4. [Checklist de Validação](#checklist-de-validação)

---

## Alterações Realizadas

### 1. Configuração do SDK

**Arquivo:** `global.json` (NOVO)

Criado arquivo na raiz do projeto especificando a versão do .NET 10 SDK:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor",
    "allowPrerelease": true
  },
  "msbuild-sdks": {
    "Aspire.AppHost.Sdk": "10.0.0",
    "Microsoft.Build.NoTargets": "3.7.56"
  }
}
```

### 2. Target Framework

**Arquivo:** `Directory.Build.props`

- Atualizado `TargetFramework` de `net9.0` para `net10.0`
- Adicionado `LangVersion` explícito para `14.0` (C# 14)

### 3. Pacotes NuGet

**Arquivo:** `Directory.Packages.props`

Pacotes atualizados para versões compatíveis com .NET 10:

#### Microsoft Core & ASP.NET Core
- `Microsoft.NET.Test.Sdk`: 18.0.0 → 18.1.0
- `Microsoft.AspNetCore.*`: 9.0.9 → 10.0.0
- `Microsoft.AspNetCore.Http.Abstractions`: 2.3.0 → 10.0.0

#### Entity Framework Core
- Todos os pacotes `Microsoft.EntityFrameworkCore.*`: 9.0.9 → 10.0.0
- `Npgsql.EntityFrameworkCore.PostgreSQL`: 9.0.4 → 10.0.0
- `EFCore.NamingConventions`: 9.0.0 → 10.0.0

#### Microsoft Extensions
- Todos os pacotes `Microsoft.Extensions.*`: 9.0.9 → 10.0.0
- `Microsoft.Extensions.Caching.Hybrid`: 9.9.0 → 10.0.0
- `Microsoft.Extensions.Http.Resilience`: 9.9.0 → 10.0.0
- `Microsoft.Extensions.ServiceDiscovery`: 9.0.1-preview → 10.0.0-preview.1

#### Aspire
- Todos os pacotes `Aspire.*`: 9.0.0-preview.5 → 10.0.0-preview.1

#### System
- `System.Text.Json`: 9.0.9 → 10.0.0
- `Microsoft.Data.Sqlite`: 9.0.9 → 10.0.0

### 4. Arquivos .csproj

Todos os arquivos `.csproj` que tinham `<TargetFramework>net9.0</TargetFramework>` explícito foram atualizados para `net10.0`.

---

## Breaking Changes do .NET 10

### 1. ASP.NET Core Security - Cookie-Based Login Redirects

**Impacto:** APIs que usam autenticação baseada em cookies

**Mudança:** 
- Endpoints de API não farão mais redirecionamentos automáticos para páginas de login
- Em vez disso, retornarão `401 Unauthorized` por padrão

**Ação Requerida:**
- Verificar se nossas APIs estão retornando códigos de status HTTP adequados
- Nosso sistema usa JWT Bearer authentication, então este breaking change tem **impacto mínimo**
- Manter documentação Swagger/OpenAPI atualizada com os códigos de resposta corretos

**Status:** ✅ Sem ação necessária (usamos JWT, não cookies)

### 2. DllImport Search Path Restrictions

**Impacto:** Aplicações que usam interoperabilidade nativa (P/Invoke)

**Mudança:**
- O caminho de busca para bibliotecas nativas está sendo restringido
- Aplicações single-file não buscarão mais no diretório do executável por padrão

**Ação Requerida:**
- Revisar qualquer uso de `[DllImport]` no código
- Garantir que bibliotecas nativas estejam em locais apropriados

**Status:** ✅ Sem ação necessária (não usamos P/Invoke extensivamente)

### 3. System.Linq.AsyncEnumerable Integration

**Impacto:** Código que usa `IAsyncEnumerable<T>`

**Mudança:**
- `System.Linq.AsyncEnumerable` agora faz parte das bibliotecas core
- Pode requerer ajustes em namespaces

**Ação Requerida:**
- Revisar código que usa `IAsyncEnumerable<T>`
- Remover referências ao pacote `System.Linq.Async` se instalado
- Atualizar statements `using` se necessário

**Status:** ⚠️ Requer verificação em:
- Queries assíncronas do Entity Framework Core
- Handlers de comandos/queries que retornam streams

### 4. W3C Trace Context Default

**Impacto:** Distributed tracing e observabilidade

**Mudança:**
- W3C Trace Context se torna o formato de trace padrão
- Substitui o formato proprietário anterior

**Ação Requerida:**
- Verificar compatibilidade com ferramentas de observabilidade (Azure Monitor, Seq)
- Atualizar configuração do OpenTelemetry se necessário
- Testar propagação de trace context entre serviços

**Status:** ⚠️ Requer teste com:
- Azure Monitor OpenTelemetry
- Seq
- Aspire Dashboard
- Distributed tracing entre módulos

---

## Novas Funcionalidades do C# 14

O .NET 10 traz o C# 14 com recursos poderosos. Considere adotá-los gradualmente:

### 1. `field` Keyword

Acesso direto ao backing field de auto-properties:

```csharp
// Antes (C# 13)
private string _name = string.Empty;
public string Name 
{
    get => _name;
    set => _name = value?.Trim() ?? string.Empty;
}

// Depois (C# 14)
public string Name { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;
```

**Oportunidade:** Simplificar properties em Value Objects e Entities do DDD

### 2. Extension Members

Propriedades de extensão, métodos estáticos e operadores:

```csharp
// Antes - apenas métodos de instância
public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) 
        => string.IsNullOrWhiteSpace(value);
}

// Depois - propriedades de extensão
public static class StringExtensions
{
    public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);
}

// Uso
var isEmpty = myString.IsEmpty; // property-like syntax
```

**Oportunidade:** Melhorar legibilidade em classes auxiliares e mappers

### 3. Partial Constructors and Events

Source generators podem augmentar construtores:

```csharp
public partial class ProviderBuilder
{
    // Construtor base definido manualmente
    public partial ProviderBuilder();
    
    // Source generator pode adicionar lógica
}
```

**Oportunidade:** Integração com source generators para builders de testes

### 4. Null-Conditional Assignments

Sintaxe mais concisa para atribuições condicionais:

```csharp
// Antes
x ??= GetDefaultValue();

// Continua funcionando, mas agora com melhor otimização do compilador
```

**Oportunidade:** Já usamos, mas com melhor performance

### 5. File-Based Apps

Executar arquivos C# standalone:

```bash
dotnet run MyScript.cs
```

**Oportunidade:** Scripts de automação e utilitários para o projeto

---

## Checklist de Validação

Use este checklist para validar a migração:

### Compilação

- [ ] `dotnet restore` executa sem erros
- [ ] `dotnet build` compila todo o projeto
- [ ] Nenhum warning crítico (apenas informativos)

### Testes

- [ ] Testes unitários passam: `dotnet test --filter "FullyQualifiedName!~Integration"`
- [ ] Testes de integração passam: `dotnet test --filter "FullyQualifiedName~Integration"`
- [ ] Testes E2E passam (se aplicável)
- [ ] Testes de arquitetura passam

### Funcionalidades Críticas

- [ ] Autenticação JWT funciona corretamente
- [ ] Autorização baseada em claims funciona
- [ ] Entity Framework Core queries executam corretamente
- [ ] Migrações de banco de dados funcionam
- [ ] Mensageria (RabbitMQ/Azure Service Bus) funciona
- [ ] Health checks respondem corretamente

### Observabilidade

- [ ] OpenTelemetry exporta traces corretamente
- [ ] Logs aparecem no Seq
- [ ] Métricas são coletadas
- [ ] Aspire Dashboard mostra dados corretos
- [ ] Azure Monitor recebe telemetria (se configurado)

### Performance

- [ ] Tempo de startup não aumentou significativamente
- [ ] Memory footprint está similar ou melhor
- [ ] Tempo de resposta de APIs está similar ou melhor

### Infraestrutura

- [ ] Docker images constroem corretamente
- [ ] Aspire AppHost inicia sem erros
- [ ] Containers PostgreSQL, Redis, RabbitMQ conectam

---

## Próximos Passos

Após validação completa:

1. ✅ Commitar mudanças na branch `migration-to-dotnet-10`
2. ⏭️ Executar CI/CD pipeline
3. ⏭️ Realizar testes em ambiente de staging
4. ⏭️ Documentar quaisquer problemas encontrados
5. ⏭️ Criar PR para merge na branch principal
6. ⏭️ Atualizar PLAN.md marcando seção 7 como concluída

---

## Recursos Adicionais

- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [ASP.NET Core 10.0 Breaking Changes](https://learn.microsoft.com/aspnet/core/migration/90-to-10)
- [C# 14 What's New](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Entity Framework Core 10.0 What's New](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew)

---

**Última atualização:** 2025-11-12  
**Responsável:** Equipe de Desenvolvimento MeAjudaAi
