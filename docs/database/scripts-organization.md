# Database Scripts Organization

## 📁 Structure Overview

```
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
```

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
CREATE ROLE [module_name]_role LOGIN PASSWORD '[module_name]_secret';

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

Add new methods for each module:

```csharp
public async Task EnsureProvidersModulePermissionsAsync(string adminConnectionString,
    string providersRolePassword = "providers_secret", string appRolePassword = "app_secret")
{
    // Implementation similar to EnsureUsersModulePermissionsAsync
}
```

### Step 4: Update Module Registration

In each module's `Extensions.cs`:

```csharp
public static async Task<IServiceCollection> AddProvidersModuleWithSchemaIsolationAsync(
    this IServiceCollection services, IConfiguration configuration)
{
    var enableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
    
    if (enableSchemaIsolation)
    {
        var schemaManager = services.BuildServiceProvider().GetRequiredService<SchemaPermissionsManager>();
        var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
        await schemaManager.EnsureProvidersModulePermissionsAsync(adminConnectionString!);
    }
    
    return services;
}
```

## 🔧 Naming Conventions

### Database Objects:
- **Schema**: `[module_name]` (e.g., `users`, `providers`, `services`)
- **Role**: `[module_name]_role` (e.g., `users_role`, `providers_role`)
- **Password**: `[module_name]_secret` (e.g., `users_secret`, `providers_secret`)

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
```

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
CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '${ModuleName}_secret';

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
```

## 📝 Usage Example

```bash
# Create new providers module
./create-module.ps1 -ModuleName "providers"

# Create new services module  
./create-module.ps1 -ModuleName "services"
```

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