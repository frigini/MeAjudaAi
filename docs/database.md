# 🗄️ Estratégia de Limites de Banco de Dados - Plataforma MeAjudaAi

Seguindo a [abordagem de Milan Jovanović](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith) para manter os limites de dados em Monólitos Modulares.

## 🎯 Princípios Fundamentais

### Limites Forçados no Nível do Banco de Dados
- ✅ **Um schema por módulo** com função de banco de dados dedicada
- ✅ **Permissões baseadas em funções** restringem o acesso apenas ao schema do próprio módulo
- ✅ **Um DbContext por módulo** com configuração de schema padrão
- ✅ **Strings de conexão separadas** usando credenciais específicas do módulo
- ✅ **Acesso entre módulos** apenas através de views explícitas ou APIs

## 📁 Estrutura de Arquivos

```text
infrastructure/database/
├── 📂 shared/                          # Scripts base da plataforma
│   ├── 00-create-base-roles.sql        # Funções compartilhadas
│   └── 01-create-base-schemas.sql      # Schemas compartilhados
│
├── 📂 modules/                         # Scripts específicos de módulos
│   ├── 📂 users/                       # Módulo de Usuários (IMPLEMENTADO)
│   │   ├── 00-create-roles.sql         # Funções do módulo
│   │   ├── 01-create-schemas.sql       # Schemas do módulo
│   │   └── 02-grant-permissions.sql    # Permissões do módulo
│   │
│   ├── 📂 providers/                   # Módulo de Provedores (FUTURO)
│   │   ├── 00-create-roles.sql
│   │   ├── 01-create-schemas.sql
│   │   └── 02-grant-permissions.sql
│   │
│   └── 📂 services/                    # Módulo de Serviços (FUTURO)
│       ├── 00-create-roles.sql
│       ├── 01-create-schemas.sql
│       └── 02-grant-permissions.sql
│
├── 📂 views/                          # Consultas transversais
│   └── cross-module-views.sql         # Acesso controlado entre módulos
│
├── 📂 orchestrator/                   # Coordenação e controle
│   └── module-registry.sql            # Registro de módulos instalados
│
└── README.md                          # Documentação
```

## 🏗️ Organização de Schemas

### Estrutura de Schemas do Banco de Dados

```text
-- Database: meajudaai
├── users (schema)         - Dados de gerenciamento de usuários
├── providers (schema)     - Dados de provedores de serviço
├── services (schema)      - Dados de catálogo de serviços
├── bookings (schema)      - Agendamentos e reservas
├── notifications (schema) - Sistema de mensagens
└── public (schema)        - Views transversais e dados compartilhados
```

## 🔐 Funções do Banco de Dados

| Função | Schema | Propósito |
|--------|--------|-----------|  
| `users_role` | `users` | Perfis de usuário, dados de autenticação |
| `providers_role` | `providers` | Informações de provedores de serviço |
| `services_role` | `services` | Catálogo de serviços e precificação |
| `bookings_role` | `bookings` | Agendamentos e reservas |
| `notifications_role` | `notifications` | Sistema de mensagens e alertas |
| `meajudaai_app_role` | `public` | Acesso entre módulos via views |

## 🔧 Implementação Atual

### Módulo de Usuários (Ativo)
- **Schema**: `users`
- **Função**: `users_role` 
- **Search Path**: `users, public`
- **Permissões**: CRUD completo no schema users, acesso limitado ao public para migrations do EF

### Configuração de String de Conexão
```json
{
  "ConnectionStrings": {
    "Users": "Host=localhost;Database=meajudaai;Username=users_role;Password=${USERS_ROLE_PASSWORD}",
    "Providers": "Host=localhost;Database=meajudaai;Username=providers_role;Password=${PROVIDERS_ROLE_PASSWORD}",
    "DefaultConnection": "Host=localhost;Database=meajudaai;Username=meajudaai_app_role;Password=${APP_ROLE_PASSWORD}"
  }
}
```

### Configuração do DbContext

```csharp
public class UsersDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define schema padrão para todas as entidades
        modelBuilder.HasDefaultSchema("users");
        base.OnModelCreating(modelBuilder);
    }
}

// Registro com migrations específicas do schema
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString, 
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "users")));
```

## 🚀 Benefícios desta Estratégia

### Limites Forçados
- Cada módulo opera em seu próprio contexto de segurança
- Acesso a dados entre módulos deve ser explícito (views ou APIs)
- Dependências tornam-se visíveis e mantíveis
- Fácil identificar violações de limites

### Extração Futura de Microsserviços
- Limites limpos facilitam a extração de módulos
- Banco de dados pode ser dividido ao longo das linhas de schema existentes
- Refatoração mínima necessária para separação de serviços

### Principais Vantagens
1. **🔒 Isolamento em Nível de Banco de Dados**: Previne acesso acidental entre módulos
2. **🎯 Propriedade Clara**: Cada módulo possui seu schema e dados
3. **📈 Escalabilidade Independente**: Módulos podem ser extraídos para bancos de dados separados posteriormente
4. **🛡️ Segurança**: Controle de acesso baseado em funções no nível do banco de dados
5. **🔄 Segurança de Migration**: Histórico de migration separado por módulo

## 🚀 Adicionando Novos Módulos

### Passo 1: Copiar Template de Módulo
```bash
# Copiar template para novo módulo
cp -r infrastructure/database/modules/users infrastructure/database/modules/providers
```
### Passo 2: Atualizar Scripts SQL
Substituir `users` pelo nome do novo módulo em:
- `00-create-roles.sql`
- `01-create-schemas.sql` 
- `02-grant-permissions.sql`

### Passo 3: Criar DbContext
```csharp
public class ProvidersDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");
        base.OnModelCreating(modelBuilder);
    }
}
```

### Step 4: Register in DI
```csharp
builder.Services.AddDbContext<ProvidersDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Providers"), 
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "providers")));
```
## 🔄 Migration Commands

### Generate Migrations
```bash
# Generate migration for Users module
dotnet ef migrations add AddUserProfile --context UsersDbContext --output-dir Infrastructure/Persistence/Migrations

# Generate migration for Providers module (future)
dotnet ef migrations add InitialProviders --context ProvidersDbContext --output-dir Infrastructure/Persistence/Migrations
```

### Apply Migrations

```bash
# Apply all migrations for Users module
dotnet ef database update --context UsersDbContext

# Apply specific migration
dotnet ef database update AddUserProfile --context UsersDbContext
```

### Remove Migrations
```bash
# Remove last migration for Users module
dotnet ef migrations remove --context UsersDbContext
```

### Controle de Migrations em Produção

Por padrão, cada módulo aplica suas migrations automaticamente no startup. Para ambientes de produção com múltiplas instâncias ou pipelines de deployment controlados, você pode desabilitar migrations automáticas usando a variável de ambiente `APPLY_MIGRATIONS`:

```bash
# Desabilitar migrations automáticas (recomendado para produção)
APPLY_MIGRATIONS=false

# Habilitar migrations automáticas (padrão em desenvolvimento)
APPLY_MIGRATIONS=true
# ou simplesmente não definir a variável
```

**Quando usar `APPLY_MIGRATIONS=false`:**
- ✅ Ambientes de produção com múltiplas instâncias (evita race conditions)
- ✅ Deployments controlados via pipeline de CI/CD
- ✅ Blue-green deployments onde migrations devem rodar antes do switch
- ✅ Ambientes que exigem aprovação manual de mudanças no schema

**Implementação por Módulo:**

Cada módulo implementa o controle em seu arquivo `API/Extensions.cs`:

```csharp
private static void EnsureDatabaseMigrations(WebApplication app)
{
    // Pular em ambientes de teste
    if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Testing"))
    {
        return;
    }

    // Controle via variável de ambiente
    var applyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
    if (!string.IsNullOrEmpty(applyMigrations) && 
        bool.TryParse(applyMigrations, out var shouldApply) && !shouldApply)
    {
        logger?.LogInformation("Migrações automáticas desabilitadas via APPLY_MIGRATIONS=false");
        return;
    }

    // Aplicar migrations...
    context.Database.Migrate();
}
```

**Aplicar Migrations via Pipeline:**

```bash
# No seu pipeline de CI/CD, antes do deployment
dotnet ef database update --context DocumentsDbContext --connection "$CONNECTION_STRING"
dotnet ef database update --context UsersDbContext --connection "$CONNECTION_STRING"
dotnet ef database update --context ProvidersDbContext --connection "$CONNECTION_STRING"
# ... outros módulos

# Depois fazer o deployment com APPLY_MIGRATIONS=false
```

**Módulos que implementam este controle:**
- ✅ Documents
- ⏳ Users (pendente)
- ⏳ Providers (pendente)
- ⏳ ServiceCatalogs (pendente)
- ⏳ Locations (pendente)

## 🌐 Cross-Module Access Strategies

### Option 1: Database Views (Current)
```sql
CREATE VIEW public.user_bookings_summary AS
SELECT u.id, u.email, b.booking_date, s.service_name
FROM users.users u
JOIN bookings.bookings b ON b.user_id = u.id
JOIN services.services s ON s.id = b.service_id;

GRANT SELECT ON public.user_bookings_summary TO meajudaai_app_role;
```

### Opção 2: APIs de Módulo (Recomendada)

```csharp
// Cada módulo expõe uma API limpa
public interface IUsersModuleApi
{
    Task<UserSummaryDto?> GetUserSummaryAsync(Guid userId);
    Task<bool> UserExistsAsync(Guid userId);
}

// Implementação usa DbContext interno
public class UsersModuleApi : IUsersModuleApi
{
    private readonly UsersDbContext _context;
    
    public async Task<UserSummaryDto?> GetUserSummaryAsync(Guid userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName))
            .FirstOrDefaultAsync();
    }
}

// Uso em outros módulos
public class BookingService
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        // Validar se usuário existe via API
        var userExists = await _usersApi.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new UserNotFoundException();
            
        // Criar agendamento...
    }
}
```

### Opção 3: Read Models Orientados a Eventos (Futuro)
```csharp
// Módulo Users publica eventos
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Outros módulos se inscrevem e constroem read models
public class NotificationEventHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Construir read model específico de notificações
        await _notificationContext.UserNotificationPreferences.AddAsync(
            new UserNotificationPreference 
            { 
                UserId = notification.UserId, 
                EmailEnabled = true 
            });
    }
}
```

## ⚡ Configuração de Desenvolvimento

### Desenvolvimento Local
1. **Aspire**: Cria automaticamente o banco de dados e executa scripts de inicialização
2. **Docker**: Container PostgreSQL com montagem de volumes para scripts de schema
3. **Migrations**: Cada módulo mantém histórico de migration separado

### Considerações de Produção
- Usar Azure PostgreSQL com schemas separados
- Considerar réplicas de leitura para views entre módulos
- Monitorar consultas entre schemas para desempenho
- Planejar eventual divisão de banco de dados se os módulos precisarem escalar independentemente

## ✅ Checklist de Conformidade

- [x] Cada módulo tem seu próprio schema
- [x] Cada módulo tem sua própria função de banco de dados
- [x] Permissões de funções restritas apenas ao schema do módulo
- [x] DbContext configurado com schema padrão
- [x] Tabela de histórico de migrations no schema do módulo
- [x] Strings de conexão usam credenciais específicas do módulo
- [x] Search path configurado para o schema do módulo
- [x] Acesso entre módulos controlado via views/APIs
- [ ] Módulos adicionais seguem o mesmo padrão
- [ ] Views transversais criadas conforme necessário

## 🎓 Referências

Baseado nos excelentes artigos de Milan Jovanović:
- [How to Keep Your Data Boundaries Intact in a Modular Monolith](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith)
- [Modular Monolith Data Isolation](https://www.milanjovanovic.tech/blog/modular-monolith-data-isolation)
- [Internal vs Public APIs in Modular Monoliths](https://www.milanjovanovic.tech/blog/internal-vs-public-apis-in-modular-monoliths)

---

## 🔒 Isolamento de Schema para Módulo de Usuários

O `SchemaPermissionsManager` implementa **isolamento de segurança para o módulo Users** usando os scripts SQL existentes em `infrastructure/database/schemas/`.

### 🎯 Objetivos

- **Isolamento de Dados**: O módulo Users só acessa o schema `users`.
- **Segurança**: O `users_role` não pode acessar outros dados.
- **Reusabilidade**: Usa scripts de infraestrutura existentes.
- **Flexibilidade**: Pode ser habilitado/desabilitado por configuração.

### 🚀 Como Usar

#### 1. Desenvolvimento (Padrão Atual)
```csharp
// Program.cs - modo atual (sem isolamento)
services.AddUsersModule(configuration);
```

#### 2. Produção (Com Isolamento)
```csharp
// Program.cs - modo seguro
if (app.Environment.IsProduction())
{
    await services.AddUsersModuleWithSchemaIsolationAsync(configuration);
}
else
{
    services.AddUsersModule(configuration);
}
```

#### 3. Configuração (appsettings.Production.json)
```json
{
  "Database": {
    "EnableSchemaIsolation": true
  },
  "ConnectionStrings": {
    "meajudaai-db-admin": "Host=prod-db;Database=meajudaai;Username=admin;Password=admin_password;"
  },
  "Postgres": {
    "UsersRolePassword": "users_secure_password_123",
    "AppRolePassword": "app_secure_password_456"
  }
}
```

### 🔧 Scripts Existentes Utilizados

- **00-create-roles-users-only.sql**: Cria `users_role` e `meajudaai_app_role`.
- **02-grant-permissions-users-only.sql**: Concede permissões específicas para o módulo Users.

> **📝 Nota sobre Schemas**: O schema `users` é criado automaticamente pelo Entity Framework Core através da configuração `HasDefaultSchema("users")`. Não há necessidade de scripts específicos de criação de schema.

### ⚡ Benefícios

- ✅ **Reutiliza infraestrutura existente**: Usa scripts já testados.
- ✅ **Zero configuração manual**: Configuração automática quando necessário.
- ✅ **Flexível**: Pode ser habilitado apenas em produção.
- ✅ **Seguro**: Isolamento real para o módulo Users.
- ✅ **Consistente**: Alinhado com a estrutura atual do projeto.
- ✅ **Simplificado**: EF Core gerencia a criação de schema automaticamente.

### 📊 Cenários de Uso

| Ambiente | Configuração | Comportamento |
|---|---|---|
| **Desenvolvimento** | `EnableSchemaIsolation: false` | Usa usuário admin padrão |
| **Teste** | `EnableSchemaIsolation: false` | TestContainers com um único usuário |

| **Produção** | `EnableSchemaIsolation: true` | Máxima segurança para Users |

### 🛡️ Estrutura de Segurança

- **users_role**: Acesso exclusivo ao schema `users`.
- **meajudaai_app_role**: Acesso transversal para operações gerais.
- **Isolamento**: O schema `users` está isolado de outros dados.
- **Search path**: `users,public` - prioriza dados do módulo.

Esta solução **aproveita completamente** sua infraestrutura existente! 🚀
# Organização de Scripts de Banco de Dados

## 🔒 Aviso de Segurança

**Importante**: Nunca codifique senhas diretamente em scripts SQL ou documentação. Todas as senhas de banco de dados devem ser:
- Recuperadas de variáveis de ambiente
- Armazenadas em provedores de configuração seguros (Azure Key Vault, AWS Secrets Manager, etc.)
- Geradas usando geradores aleatórios criptograficamente seguros
- Rotacionadas regularmente de acordo com políticas de segurança

## �📁 Structure Overview

```text
infrastructure/database/
├── modules/
│   ├── users/                    ✅ IMPLEMENTED
│   │   ├── 00-roles.sql
│   │   └── 01-permissions.sql
│   ├── providers/                🔄 FUTURE MODULE
│   │   ├── 00-roles.sql
│   │   └── 01-permissions.sql
│   └── services/                 🔄 FUTURE MODULE
│       ├── 00-roles.sql
│       └── 01-permissions.sql
└── README.md                     # Esta documentação
```

**Nota**: Schema isolation é gerenciado em runtime por `SchemaPermissionsManager` via `ConfigureAllModulesSchemaIsolation()`. Para adicionar um novo módulo, crie manualmente os arquivos `00-roles.sql` e `01-permissions.sql` seguindo os templates existentes.

## 🛠️ Adding New Modules

### Step 1: Create Module Folder Structure

```bash
# For new module (example: providers)
mkdir infrastructure/database/modules/providers
```

### Step 2: Create Scripts Using Templates

#### `00-roles.sql` Template:
```sql
-- [MODULE_NAME] Module - Database Roles
-- Create dedicated role for [module_name] module
-- Note: Replace $PASSWORD with secure password from environment variables or secrets store
CREATE ROLE [module_name]_role LOGIN PASSWORD '$PASSWORD';

-- Grant [module_name] role to app role for cross-module access
GRANT [module_name]_role TO meajudaai_app_role;
```

#### `01-permissions.sql` Template:
```sql
-- [MODULE_NAME] Module - Permissions
-- Grant permissions for [module_name] module
GRANT USAGE ON SCHEMA [module_name] TO [module_name]_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA [module_name] TO [module_name]_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA [module_name] TO [module_name]_role;

-- Set default privileges for future tables and sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA [module_name] GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO [module_name]_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA [module_name] GRANT USAGE, SELECT ON SEQUENCES TO [module_name]_role;

-- Set default search path
ALTER ROLE [module_name]_role SET search_path = [module_name], public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA [module_name] TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA [module_name] TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA [module_name] TO meajudaai_app_role;

-- Set default privileges for app role
ALTER DEFAULT PRIVILEGES IN SCHEMA [module_name] GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA [module_name] GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant permissions on public schema
GRANT USAGE ON SCHEMA public TO [module_name]_role;
```

### Step 3: Update SchemaPermissionsManager

Adicionar novos métodos para cada módulo:

```csharp
public async Task EnsureProvidersModulePermissionsAsync(string adminConnectionString,
    string providersRolePassword, string appRolePassword)
{
    // Implementação similar a EnsureUsersModulePermissionsAsync
}
```csharp
> ⚠️ **AVISO DE SEGURANÇA**: Nunca codifique senhas diretamente em assinaturas de métodos ou código-fonte!

**Padrão de Recuperação Segura de Senhas:**

```csharp
// ✅ SEGURO: Recuperar senhas de configuração/segredos
public async Task ConfigureProvidersModule(IConfiguration configuration)
{
    var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
    
    // Opção 1: Variáveis de ambiente
    var providersPassword = Environment.GetEnvironmentVariable("PROVIDERS_ROLE_PASSWORD");
    var appPassword = Environment.GetEnvironmentVariable("APP_ROLE_PASSWORD");
    
    // Opção 2: Configuração com provedores de segredos (Azure Key Vault, etc.)
    var providersPassword = configuration["Database:Roles:ProvidersPassword"];
    var appPassword = configuration["Database:Roles:AppPassword"];
    
    // Opção 3: Serviço de segredos dedicado
    var secretsService = serviceProvider.GetRequiredService<ISecretsService>();
    var providersPassword = await secretsService.GetSecretAsync("db-providers-password");
    var appPassword = await secretsService.GetSecretAsync("db-app-password");
    
    if (string.IsNullOrEmpty(providersPassword) || string.IsNullOrEmpty(appPassword))
    {
        throw new InvalidOperationException("Senhas de funções do banco de dados devem ser configuradas via provedor de segredos");
    }
    
    await schemaManager.EnsureProvidersModulePermissionsAsync(
        adminConnectionString, providersPassword, appPassword);
}
```text
### Passo 4: Atualizar Registro do Módulo

No `Extensions.cs` de cada módulo:

```csharp
// Opção 1: Usando IServiceScopeFactory (recomendado para métodos de extensão)
public static IServiceCollection AddProvidersModuleWithSchemaIsolation(
    this IServiceCollection services, IConfiguration configuration)
{
    var enableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
    
    if (enableSchemaIsolation)
    {
        // Registrar um método factory que será executado quando necessário
        services.AddSingleton<Func<Task>>(provider =>
        {
            return async () =>
            {
                using var scope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var schemaManager = scope.ServiceProvider.GetRequiredService<SchemaPermissionsManager>();
                var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
                await schemaManager.EnsureProvidersModulePermissionsAsync(adminConnectionString!);
            };
        });
    }
    
    return services;
}

// Opção 2: Usando IHostedService (recomendado para inicialização na startup)
public class DatabaseSchemaInitializationService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public DatabaseSchemaInitializationService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enableSchemaIsolation = _configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
        
        if (enableSchemaIsolation)
        {
            using var scope = _scopeFactory.CreateScope();
            var schemaManager = scope.ServiceProvider.GetRequiredService<SchemaPermissionsManager>();
            var adminConnectionString = _configuration.GetConnectionString("AdminPostgres");
            await schemaManager.EnsureProvidersModulePermissionsAsync(adminConnectionString!);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// Registrar o hosted service no Program.cs ou Startup.cs:
// services.AddHostedService<DatabaseSchemaInitializationService>();
```csharp
## 🔧 Convenções de Nomenclatura

### Objetos de Banco de Dados:
- **Schema**: `[module_name]` (ex: `users`, `providers`, `services`)
- **Função**: `[module_name]_role` (ex: `users_role`, `providers_role`)
- **Senha**: Recuperada de configuração segura (variáveis de ambiente, Key Vault ou gerenciador de segredos)

### Nomes de Arquivos:
- **Funções**: `00-roles.sql`
- **Permissões**: `01-permissions.sql`

### Configuração do DbContext:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("[module_name]");
    // EF Core criará o schema automaticamente
}
```

## ⚡ Adicionando Novo Módulo

Para adicionar um novo módulo ao sistema de schema isolation:

### 1. Criar estrutura de diretórios

```bash
mkdir infrastructure/database/modules/new_module
```

### 2. Criar `00-roles.sql`

Copie o template de um módulo existente (ex: `users/00-roles.sql`) e ajuste os placeholders. Os placeholders `{{ROLE_NAME}}`, `{{ROLE_OWNER_NAME}}`, `{{APP_ROLE_NAME}}`, `{{SCHEMA_NAME}}` são substituídos automaticamente pelo `SchemaPermissionsManager` em runtime.

### 3. Criar `01-permissions.sql`

Copie o template de um módulo existente (ex: `users/01-permissions.sql`) e ajuste os placeholders.

### 4. Registrar no Shared

Adicione o módulo ao método `ConfigureAllModulesSchemaIsolation()` em `src/Shared/Database/DatabaseExtensions.cs`:

```csharp
var modules = new[]
{
    // ... módulos existentes
    (ModuleNames.NewModule, Schemas.NewModule, DatabaseRoleConstants.NewModule)
};
```

### 5. Adicionar constantes

- `src/Shared/Utilities/Constants/ModuleNames.cs` - adicionar `NewModule`
- `src/Shared/Utilities/Constants/Schemas.cs` - adicionar `NewModule`
- `src/Shared/Utilities/Constants/DatabaseRoleConstants.cs` - adicionar `NewModule`

## 🔒 Melhores Práticas de Segurança

1. **Isolamento de Schema**: Cada módulo tem seu próprio schema e função
2. **Princípio do Menor Privilégio**: Funções têm apenas as permissões necessárias
3. **Acesso Entre Módulos**: Controlado através de `meajudaai_app_role`
4. **Gerenciamento de Senhas**: Usar senhas seguras em produção
5. **Search Path**: Sempre incluir schema do módulo primeiro, depois public

## 🔄 Integração com SchemaPermissionsManager

O `SchemaPermissionsManager` automaticamente gerencia:
- ✅ Criação de funções e gerenciamento de senhas
- ✅ Configuração de permissões de schema
- ✅ Configuração de acesso entre módulos
- ✅ Privilégios padrão para objetos futuros
- ✅ Otimização de search path
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
