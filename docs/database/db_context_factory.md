# DbContext Factory Pattern - Documentação

## Visão Geral

A classe `BaseDesignTimeDbContextFactory<TContext>` fornece uma implementação base para factories de DbContext em tempo de design (design-time), utilizizada principalmente para operações de migração do Entity Framework Core.

## Objetivo

- **Padronização**: Centraliza a configuração comum para factories de DbContext
- **Reutilização**: Permite que módulos implementem facilmente suas próprias factories
- **Consistência**: Garante configuração uniforme de migrações across módulos
- **Manutenibilidade**: Facilita mudanças futuras na configuração base

## Como Usar



// Namespace: MeAjudaAi.Modules.Orders.Infrastructure.Persistence  ### 1. Implementação Básica

// Module Name detectado: "Orders"

``````csharp

public class UsersDbContextFactory : BaseDesignTimeDbContextFactory<UsersDbContext>

### 2. Configuração Automática{

Com base no nome do módulo detectado, a factory configura automaticamente:    protected override string GetDesignTimeConnectionString()

    {

- **Migrations Assembly**: `MeAjudaAi.Modules.{ModuleName}.Infrastructure`        return "Host=localhost;Database=meajudaai_dev;Username=postgres;Password=postgres";

- **Schema**: `{modulename}` (lowercase)    }

- **Connection String**: Baseada no módulo com fallback para configuração padrão

    protected override string GetMigrationsAssembly()

### 3. Configuração Flexível    {

Suporta configuração via `appsettings.json`:        return "MeAjudaAi.Modules.Users.Infrastructure";

    }

```json

{    protected override string GetMigrationsHistorySchema()

  "ConnectionStrings": {    {

    "UsersDatabase": "Host=prod-server;Database=meajudaai_prod;Username=app;Password=secret;SearchPath=users,public",        return "users";

    "OrdersDatabase": "Host=prod-server;Database=meajudaai_prod;Username=app;Password=secret;SearchPath=orders,public"    }

  }

}    protected override UsersDbContext CreateDbContextInstance(DbContextOptions<UsersDbContext> options)

```    {

        return new UsersDbContext(options);

## Como Usar    }

}

### 1. Implementação Simples```csharp
```csharp

public class UsersDbContextFactory : BaseDesignTimeDbContextFactory<UsersDbContext>### 2. Configuração Adicional (Opcional)

{

    protected override UsersDbContext CreateDbContextInstance(DbContextOptions<UsersDbContext> options)```csharp

    {public class AdvancedDbContextFactory : BaseDesignTimeDbContextFactory<AdvancedDbContext>

        return new UsersDbContext(options);{

    }    // ... implementações obrigatórias ...

}

```    protected override void ConfigureAdditionalOptions(DbContextOptionsBuilder<AdvancedDbContext> optionsBuilder)

    {

### 2. Execução de Migrations        // Configurações específicas do módulo

```bash        optionsBuilder.EnableSensitiveDataLogging();

# Funciona automaticamente - detecta o módulo do namespace        optionsBuilder.EnableDetailedErrors();

dotnet ef migrations add NewMigration --project src/Modules/Users/Infrastructure --startup-project src/Bootstrapper/MeAjudaAi.ApiService    }

}

# Lista migrations existentes```csharp
dotnet ef migrations list --project src/Modules/Users/Infrastructure --startup-project src/Bootstrapper/MeAjudaAi.ApiService

```## Métodos Abstratos



## Estrutura de Arquivos| Método | Descrição | Exemplo |

|--------|-----------|---------|

```| `GetDesignTimeConnectionString()` | Connection string para design-time | `"Host=localhost;Database=..."` |

src/| `GetMigrationsAssembly()` | Assembly onde as migrações ficam | `"MeAjudaAi.Modules.Users.Infrastructure"` |

├── Modules/| `GetMigrationsHistorySchema()` | Schema para tabela de histórico | `"users"` |

│   ├── Users/| `CreateDbContextInstance()` | Cria instância do DbContext | `new UsersDbContext(options)` |

│   │   └── Infrastructure/

│   │       └── Persistence/## Métodos Virtuais

│   │           ├── UsersDbContext.cs

│   │           └── UsersDbContextFactory.cs  ← namespace detecta "Users"| Método | Descrição | Uso |

│   └── Orders/|--------|-----------|-----|

│       └── Infrastructure/| `ConfigureAdditionalOptions()` | Configurações extras | Override para configurações específicas |

│           └── Persistence/

│               ├── OrdersDbContext.cs## Características

│               └── OrdersDbContextFactory.cs  ← namespace detecta "Orders"

└── Shared/- ✅ **PostgreSQL**: Configurado para usar Npgsql

    └── MeAjudaAi.Shared/- ✅ **Migrations Assembly**: Configuração automática

        └── Database/- ✅ **Schema Separation**: Cada módulo tem seu schema

            └── BaseDesignTimeDbContextFactory.cs  ← classe base- ✅ **Design-Time Only**: Connection string não usada em produção

```- ✅ **Extensível**: Permite configurações adicionais



## Vantagens## Convenções



1. **Zero Hardcoding**: Não há valores hardcoded no código### Connection String

2. **Convenção sobre Configuração**: Funciona automaticamente seguindo a estrutura de namespaces- **Formato**: `Host=localhost;Database={database};Username=postgres;Password=postgres`

3. **Reutilizável**: Mesma implementação para todos os módulos- **Uso**: Apenas para operações de design-time (migrations)

4. **Configurável**: Permite override via configuração quando necessário- **Produção**: Connection string real vem de configuração

5. **Type-Safe**: Usa reflection de forma segura com validação de namespace

### Schema

## Resolução de Problemas- **Padrão**: Cada módulo usa seu próprio schema

- **Exemplos**: `users`, `orders`, `notifications`

### Namespace Inválido- **Histórico**: `__EFMigrationsHistory` sempre no schema do módulo

Se o namespace não seguir o padrão `MeAjudaAi.Modules.{ModuleName}.Infrastructure.Persistence`, será lançada uma exceção explicativa.

### Assembly

### Connection String- **Localização**: Sempre no projeto Infrastructure do módulo

A factory tenta encontrar uma connection string específica do módulo primeiro, depois usa a padrão:- **Formato**: `MeAjudaAi.Modules.{ModuleName}.Infrastructure`

1. `{ModuleName}Database` (ex: "UsersDatabase")

2. `DefaultConnection`## Exemplo Completo - Novo Módulo

3. Fallback para desenvolvimento local

```csharp

## Exemplo Completo// Em MeAjudaAi.Modules.Orders.Infrastructure/Persistence/OrdersDbContextFactory.cs

using Microsoft.EntityFrameworkCore;

Para adicionar um novo módulo "Products":using MeAjudaAi.Shared.Database;



1. Criar namespace: `MeAjudaAi.Modules.Products.Infrastructure.Persistence`namespace MeAjudaAi.Modules.Orders.Infrastructure.Persistence;

2. Implementar factory:

```csharppublic class OrdersDbContextFactory : BaseDesignTimeDbContextFactory<OrdersDbContext>

public class ProductsDbContextFactory : BaseDesignTimeDbContextFactory<ProductsDbContext>{

{    protected override string GetDesignTimeConnectionString()

    protected override ProductsDbContext CreateDbContextInstance(DbContextOptions<ProductsDbContext> options)    {

    {        return "Host=localhost;Database=meajudaai_dev;Username=postgres;Password=postgres";

        return new ProductsDbContext(options);    }

    }

}    protected override string GetMigrationsAssembly()

```    {

3. Pronto! A detecção automática cuidará do resto.        return "MeAjudaAi.Modules.Orders.Infrastructure";

    }

## Testado e Validado ✅

    protected override string GetMigrationsHistorySchema()

Sistema confirmado funcionando através de:    {

- Compilação bem-sucedida        return "orders";

- Comando `dotnet ef migrations list` detectando automaticamente módulo "Users"    }

- Localização correta da migration `20250914145433_InitialCreate`
    protected override OrdersDbContext CreateDbContextInstance(DbContextOptions<OrdersDbContext> options)
    {
        return new OrdersDbContext(options);
    }
}
```bash
## Comandos de Migração

```bash
# Adicionar migração
dotnet ef migrations add InitialCreate --project src/Modules/Users/Infrastructure/MeAjudaAi.Modules.Users.Infrastructure

# Aplicar migração
dotnet ef database update --project src/Modules/Users/Infrastructure/MeAjudaAi.Modules.Users.Infrastructure
```text
## Benefícios

1. **Consistência**: Todas as factories seguem o mesmo padrão
2. **Manutenção**: Mudanças na configuração base afetam todos os módulos
3. **Simplicidade**: Implementação reduzida por módulo
4. **Testabilidade**: Configuração centralizada facilita testes
5. **Documentação**: Padrão claro para novos desenvolvedores