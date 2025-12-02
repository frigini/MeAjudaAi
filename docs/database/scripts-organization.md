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