# 🗄️ Database Boundaries Strategy - MeAjudaAi Platform

Following [Milan Jovanović's approach](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith) for maintaining data boundaries in Modular Monoliths.

## 🎯 Core Principles

### Enforced Boundaries at Database Level
- ✅ **One schema per module** with dedicated database role
- ✅ **Role-based permissions** restrict access to module's own schema only
- ✅ **One DbContext per module** with default schema configuration
- ✅ **Separate connection strings** using module-specific credentials
- ✅ **Cross-module access** only through explicit views or APIs

## 📁 File Structure

```text
infrastructure/database/
├── 📂 shared/                          # Base platform scripts
│   ├── 00-create-base-roles.sql        # Shared roles
│   └── 01-create-base-schemas.sql      # Shared schemas
│
├── 📂 modules/                         # Module-specific scripts
│   ├── 📂 users/                       # Users Module (IMPLEMENTED)
│   │   ├── 00-create-roles.sql         # Module roles
│   │   ├── 01-create-schemas.sql       # Module schemas
│   │   └── 02-grant-permissions.sql    # Module permissions
│   │
│   ├── 📂 providers/                   # Providers Module (FUTURE)
│   │   ├── 00-create-roles.sql
│   │   ├── 01-create-schemas.sql
│   │   └── 02-grant-permissions.sql
│   │
│   └── 📂 services/                    # Services Module (FUTURE)
│       ├── 00-create-roles.sql
│       ├── 01-create-schemas.sql
│       └── 02-grant-permissions.sql
│
├── 📂 views/                          # Cross-cutting queries
│   └── cross-module-views.sql         # Controlled cross-module access
│
├── 📂 orchestrator/                   # Coordination and control
│   └── module-registry.sql            # Registry of installed modules
│
└── README.md                          # Documentation
```csharp
## 🏗️ Schema Organization

### Database Schema Structure
```sql
-- Database: meajudaai
├── users (schema)         - User management data
├── providers (schema)     - Service provider data  
├── services (schema)      - Service catalog data
├── bookings (schema)      - Appointments and reservations
├── notifications (schema) - Messaging system
└── public (schema)        - Cross-cutting views and shared data
```text
## 🔐 Database Roles

| Role | Schema | Purpose |
|------|--------|---------|
| `users_role` | `users` | User profiles, authentication data |
| `providers_role` | `providers` | Service provider information |
| `services_role` | `services` | Service catalog and pricing |
| `bookings_role` | `bookings` | Appointments and reservations |
| `notifications_role` | `notifications` | Messaging and alerts |
| `meajudaai_app_role` | `public` | Cross-module access via views |

## 🔧 Current Implementation

### Users Module (Active)
- **Schema**: `users`
- **Role**: `users_role` 
- **Search Path**: `users, public`
- **Permissions**: Full CRUD on users schema, limited access to public for EF migrations

### Connection String Configuration
```json
{
  "ConnectionStrings": {
    "Users": "Host=localhost;Database=meajudaai;Username=users_role;Password=${USERS_ROLE_PASSWORD}",
    "Providers": "Host=localhost;Database=meajudaai;Username=providers_role;Password=${PROVIDERS_ROLE_PASSWORD}",
    "DefaultConnection": "Host=localhost;Database=meajudaai;Username=meajudaai_app_role;Password=${APP_ROLE_PASSWORD}"
  }
}
```csharp
### DbContext Configuration
```csharp
public class UsersDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities
        modelBuilder.HasDefaultSchema("users");
        base.OnModelCreating(modelBuilder);
    }
}

// Registration with schema-specific migrations
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString, 
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "users")));
```yaml
## 🚀 Benefits of This Strategy

### Enforceable Boundaries
- Each module operates in its own security context
- Cross-module data access must be explicit (views or APIs)
- Dependencies become visible and maintainable
- Easy to spot boundary violations

### Future Microservice Extraction
- Clean boundaries make module extraction straightforward
- Database can be split along existing schema lines
- Minimal refactoring required for service separation

### Key Advantages
1. **🔒 Database-Level Isolation**: Prevents accidental cross-module access
2. **🎯 Clear Ownership**: Each module owns its schema and data
3. **📈 Independent Scaling**: Modules can be extracted to separate databases later
4. **🛡️ Security**: Role-based access control at database level
5. **🔄 Migration Safety**: Separate migration history per module

## 🚀 Adding New Modules

### Step 1: Copy Module Template
```bash
# Copy template for new module
cp -r infrastructure/database/modules/users infrastructure/database/modules/providers
```
### Step 2: Update SQL Scripts
Replace `users` with new module name in:
- `00-create-roles.sql`
- `01-create-schemas.sql` 
- `02-grant-permissions.sql`

### Step 3: Create DbContext
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
```yaml
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
## 🌐 Cross-Module Access Strategies

### Option 1: Database Views (Current)
```sql
CREATE VIEW public.user_bookings_summary AS
SELECT u.id, u.email, b.booking_date, s.service_name
FROM users.users u
JOIN bookings.bookings b ON b.user_id = u.id
JOIN services.services s ON s.id = b.service_id;

GRANT SELECT ON public.user_bookings_summary TO meajudaai_app_role;
```yaml
### Option 2: Module APIs (Recommended)
```csharp
// Each module exposes a clean API
public interface IUsersModuleApi
{
    Task<UserSummaryDto?> GetUserSummaryAsync(Guid userId);
    Task<bool> UserExistsAsync(Guid userId);
}

// Implementation uses internal DbContext
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

// Usage in other modules
public class BookingService
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        // Validate user exists via API
        var userExists = await _usersApi.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new UserNotFoundException();
            
        // Create booking...
    }
}
```csharp
### Option 3: Event-Driven Read Models (Future)
```csharp
// Users module publishes events
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Other modules subscribe and build read models
public class NotificationEventHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Build notification-specific read model
        await _notificationContext.UserNotificationPreferences.AddAsync(
            new UserNotificationPreference 
            { 
                UserId = notification.UserId, 
                EmailEnabled = true 
            });
    }
}
```text
## ⚡ Development Setup

### Local Development
1. **Aspire**: Automatically creates database and runs initialization scripts
2. **Docker**: PostgreSQL container with volume mounts for schema scripts
3. **Migrations**: Each module maintains separate migration history

### Production Considerations
- Use Azure PostgreSQL with separate schemas
- Consider read replicas for cross-module views
- Monitor cross-schema queries for performance
- Plan for eventual database splitting if modules need to scale independently

## ✅ Compliance Checklist

- [x] Each module has its own schema
- [x] Each module has its own database role
- [x] Role permissions restricted to module schema only
- [x] DbContext configured with default schema
- [x] Migrations history table in module schema
- [x] Connection strings use module-specific credentials
- [x] Search path set to module schema
- [x] Cross-module access controlled via views/APIs
- [ ] Additional modules follow the same pattern
- [ ] Cross-cutting views created as needed

## 🎓 References

Based on Milan Jovanović's excellent articles:
- [How to Keep Your Data Boundaries Intact in a Modular Monolith](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith)
- [Modular Monolith Data Isolation](https://www.milanjovanovic.tech/blog/modular-monolith-data-isolation)
- [Internal vs Public APIs in Modular Monoliths](https://www.milanjovanovic.tech/blog/internal-vs-public-apis-in-modular-monoliths)

---

## 🔒 Schema Isolation for Users Module

The `SchemaPermissionsManager` implements **security isolation for the Users module** using the existing SQL scripts in `infrastructure/database/schemas/`.

### 🎯 Objectives

- **Data Isolation**: The Users module only accesses the `users` schema.
- **Security**: The `users_role` cannot access other data.
- **Reusability**: Uses existing infrastructure scripts.
- **Flexibility**: Can be enabled/disabled by configuration.

### 🚀 How to Use

#### 1. Development (Current Default)
```csharp
// Program.cs - current mode (without isolation)
services.AddUsersModule(configuration);
```

#### 2. Production (With Isolation)
```csharp
// Program.cs - secure mode
if (app.Environment.IsProduction())
{
    await services.AddUsersModuleWithSchemaIsolationAsync(configuration);
}
else
{
    services.AddUsersModule(configuration);
}
```

#### 3. Configuration (appsettings.Production.json)
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

### 🔧 Existing Scripts Used

- **00-create-roles-users-only.sql**: Creates `users_role` and `meajudaai_app_role`.
- **02-grant-permissions-users-only.sql**: Grants specific permissions for the Users module.

> **📝 Note on Schemas**: The `users` schema is created automatically by Entity Framework Core through the `HasDefaultSchema("users")` configuration. There is no need for specific schema creation scripts.

### ⚡ Benefits

- ✅ **Reuses existing infrastructure**: Uses already tested scripts.
- ✅ **Zero manual configuration**: Automatic setup when needed.
- ✅ **Flexible**: Can be enabled only in production.
- ✅ **Secure**: Real isolation for the Users module.
- ✅ **Consistent**: Aligned with the current project structure.
- ✅ **Simplified**: EF Core manages schema creation automatically.

### 📊 Usage Scenarios

| Environment | Configuration | Behavior |
|---|---|---|
| **Development** | `EnableSchemaIsolation: false` | Uses default admin user |
| **Test** | `EnableSchemaIsolation: false` | TestContainers with a single user |
| **Staging** | `EnableSchemaIsolation: true` | Dedicated `users_role` user |
| **Production** | `EnableSchemaIsolation: true` | Maximum security for Users |

### 🛡️ Security Structure

- **users_role**: Exclusive access to the `users` schema.
- **meajudaai_app_role**: Cross-cutting access for general operations.
- **Isolation**: The `users` schema is isolated from other data.
- **Search path**: `users,public` - prioritizes module data.

This solution **fully leverages** your existing infrastructure! 🚀
# Database Scripts Organization

## � Security Notice

**Important**: Never hardcode passwords in SQL scripts or documentation. All database passwords must be:
- Retrieved from environment variables
- Stored in secure configuration providers (Azure Key Vault, AWS Secrets Manager, etc.)
- Generated using cryptographically secure random generators
- Rotated regularly according to security policies

## �📁 Structure Overview

```csharp
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
├── views/
│   └── cross-module-views.sql
├── create-module.ps1             # Script para criar novos módulos
└── README.md                     # Esta documentação
```text
## 🛠️ Adding New Modules

### Step 1: Create Module Folder Structure

```bash
# For new module (example: providers)
mkdir infrastructure/database/modules/providers
```yaml
### Step 2: Create Scripts Using Templates

#### `00-roles.sql` Template:
```sql
-- [MODULE_NAME] Module - Database Roles
-- Create dedicated role for [module_name] module
-- Note: Replace $PASSWORD with secure password from environment variables or secrets store
CREATE ROLE [module_name]_role LOGIN PASSWORD '$PASSWORD';

-- Grant [module_name] role to app role for cross-module access
GRANT [module_name]_role TO meajudaai_app_role;
```csharp
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
```text
### Step 3: Update SchemaPermissionsManager

Add new methods for each module:

```csharp
public async Task EnsureProvidersModulePermissionsAsync(string adminConnectionString,
    string providersRolePassword, string appRolePassword)
{
    // Implementation similar to EnsureUsersModulePermissionsAsync
}
```csharp
> ⚠️ **SECURITY WARNING**: Never hardcode passwords in method signatures or source code!

**Secure Password Retrieval Pattern:**

```csharp
// ✅ SECURE: Retrieve passwords from configuration/secrets
public async Task ConfigureProvidersModule(IConfiguration configuration)
{
    var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
    
    // Option 1: Environment variables
    var providersPassword = Environment.GetEnvironmentVariable("PROVIDERS_ROLE_PASSWORD");
    var appPassword = Environment.GetEnvironmentVariable("APP_ROLE_PASSWORD");
    
    // Option 2: Configuration with secret providers (Azure Key Vault, etc.)
    var providersPassword = configuration["Database:Roles:ProvidersPassword"];
    var appPassword = configuration["Database:Roles:AppPassword"];
    
    // Option 3: Dedicated secrets service
    var secretsService = serviceProvider.GetRequiredService<ISecretsService>();
    var providersPassword = await secretsService.GetSecretAsync("db-providers-password");
    var appPassword = await secretsService.GetSecretAsync("db-app-password");
    
    if (string.IsNullOrEmpty(providersPassword) || string.IsNullOrEmpty(appPassword))
    {
        throw new InvalidOperationException("Database role passwords must be configured via secrets provider");
    }
    
    await schemaManager.EnsureProvidersModulePermissionsAsync(
        adminConnectionString, providersPassword, appPassword);
}
```text
### Step 4: Update Module Registration

In each module's `Extensions.cs`:

```csharp
// Option 1: Using IServiceScopeFactory (recommended for extension methods)
public static IServiceCollection AddProvidersModuleWithSchemaIsolation(
    this IServiceCollection services, IConfiguration configuration)
{
    var enableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
    
    if (enableSchemaIsolation)
    {
        // Register a factory method that will be executed when needed
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

// Option 2: Using IHostedService (recommended for startup initialization)
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

// Register the hosted service in Program.cs or Startup.cs:
// services.AddHostedService<DatabaseSchemaInitializationService>();
```csharp
## 🔧 Naming Conventions

### Database Objects:
- **Schema**: `[module_name]` (e.g., `users`, `providers`, `services`)
- **Role**: `[module_name]_role` (e.g., `users_role`, `providers_role`)
- **Password**: Retrieved from secure configuration (environment variables, Key Vault, or secrets manager)

### File Names:
- **Roles**: `00-roles.sql`
- **Permissions**: `01-permissions.sql`

### DbContext Configuration:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("[module_name]");
    // EF Core will create the schema automatically
}
```csharp
## ⚡ Quick Module Creation Script

Create this PowerShell script for quick module setup:

```powershell
# create-module.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName
)

$ModulePath = "infrastructure/database/modules/$ModuleName"
New-Item -ItemType Directory -Path $ModulePath -Force

# Create 00-roles.sql
$RolesContent = @"
-- $ModuleName Module - Database Roles
-- Create dedicated role for $ModuleName module
-- Note: Replace `$env:DB_ROLE_PASSWORD with actual environment variable or secure password retrieval
CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '`$env:DB_ROLE_PASSWORD';

-- Grant $ModuleName role to app role for cross-module access
GRANT ${ModuleName}_role TO meajudaai_app_role;
"@

$RolesContent | Out-File -FilePath "$ModulePath/00-roles.sql" -Encoding UTF8

# Create 01-permissions.sql
$PermissionsContent = @"
-- $ModuleName Module - Permissions
-- Grant permissions for $ModuleName module
GRANT USAGE ON SCHEMA $ModuleName TO ${ModuleName}_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA $ModuleName TO ${ModuleName}_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA $ModuleName TO ${ModuleName}_role;

-- Set default privileges for future tables and sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA $ModuleName GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO ${ModuleName}_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA $ModuleName GRANT USAGE, SELECT ON SEQUENCES TO ${ModuleName}_role;

-- Set default search path
ALTER ROLE ${ModuleName}_role SET search_path = $ModuleName, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA $ModuleName TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA $ModuleName TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA $ModuleName TO meajudaai_app_role;

-- Set default privileges for app role
ALTER DEFAULT PRIVILEGES IN SCHEMA $ModuleName GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA $ModuleName GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant permissions on public schema
GRANT USAGE ON SCHEMA public TO ${ModuleName}_role;
"@

$PermissionsContent | Out-File -FilePath "$ModulePath/01-permissions.sql" -Encoding UTF8

Write-Host "✅ Module '$ModuleName' database scripts created successfully!" -ForegroundColor Green
Write-Host "📁 Location: $ModulePath" -ForegroundColor Cyan
```sql
## 📝 Usage Example

```bash
# Create new providers module
./create-module.ps1 -ModuleName "providers"

# Create new services module  
./create-module.ps1 -ModuleName "services"
```text
## 🔒 Security Best Practices

1. **Schema Isolation**: Each module has its own schema and role
2. **Principle of Least Privilege**: Roles only have necessary permissions
3. **Cross-Module Access**: Controlled through `meajudaai_app_role`
4. **Password Management**: Use secure passwords in production
5. **Search Path**: Always include module schema first, then public

## 🔄 Integration with SchemaPermissionsManager

The `SchemaPermissionsManager` automatically handles:
- ✅ Role creation and password management
- ✅ Schema permissions setup
- ✅ Cross-module access configuration
- ✅ Default privileges for future objects
- ✅ Search path optimization
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
