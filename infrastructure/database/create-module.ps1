#!/usr/bin/env pwsh
# create-module.ps1
# Script para criar estrutura de banco de dados para novos módulos
#
# SECURITY NOTE: This script generates SQL templates with password placeholders.
# Always replace <secure_password> placeholders with strong passwords from secure configuration.
# Never commit actual passwords to version control.

param(
    [Parameter(Mandatory=$true, HelpMessage="Nome do módulo (ex: providers, services)")]
    [string]$ModuleName
)

# Validar nome do módulo
if ($ModuleName -notmatch '^[a-z]+$') {
    Write-Error "❌ Nome do módulo deve conter apenas letras minúsculas (ex: providers, services)"
    exit 1
}

$ModulePath = "infrastructure/database/modules/$ModuleName"

# Criar diretório do módulo
Write-Host "📁 Criando diretório: $ModulePath" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $ModulePath -Force | Out-Null

# Criar 00-roles.sql
Write-Host "🔐 Criando script de roles..." -ForegroundColor Yellow
$RolesContent = @"
-- $($ModuleName.ToUpper()) Module - Database Roles
-- Create dedicated role for $ModuleName module

-- SECURITY: Replace <secure_password> with a strong, environment-specific secret before applying
-- Generate with: openssl rand -base64 32
-- Never commit actual passwords to version control
CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '<secure_password>';

-- Grant $ModuleName role to app role for cross-module access
GRANT ${ModuleName}_role TO meajudaai_app_role;
"@

$RolesContent | Out-File -FilePath "$ModulePath/00-roles.sql" -Encoding UTF8

# Criar 01-permissions.sql
Write-Host "🔑 Criando script de permissões..." -ForegroundColor Yellow
$PermissionsContent = @"
-- $($ModuleName.ToUpper()) Module - Permissions
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

# Criar template para o SchemaPermissionsManager
Write-Host "🔧 Criando template para SchemaPermissionsManager..." -ForegroundColor Yellow
$ManagerTemplate = @"
// Adicione este método ao SchemaPermissionsManager.cs:

/// <summary>
/// Garante que as permissões do módulo $($ModuleName.ToUpper()) estejam configuradas
/// </summary>
/// <param name="adminConnectionString">Connection string with admin privileges</param>
/// <param name="${ModuleName}RolePassword">Strong password for ${ModuleName}_role - NEVER use default values in production</param>
public async Task Ensure$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModulePermissionsAsync(
    string adminConnectionString,
    string ${ModuleName}RolePassword)
{
    if (await Are$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))PermissionsConfiguredAsync(adminConnectionString))
    {
        logger.LogInformation("Permissões do módulo $($ModuleName.ToUpper()) já estão configuradas");
        return;
    }

    logger.LogInformation("Configurando permissões para módulo $($ModuleName.ToUpper()) usando scripts existentes");

    using var connection = new NpgsqlConnection(adminConnectionString);
    await connection.OpenAsync();

    try
    {
        // Executar os scripts na ordem correta
        // NOTA: Schema '$ModuleName' será criado automaticamente pelo EF Core durante as migrações
        await Execute$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaScript(connection, "00-roles", ${ModuleName}RolePassword);
        await Execute$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaScript(connection, "01-permissions");

        logger.LogInformation("✅ Permissões configuradas com sucesso para módulo $($ModuleName.ToUpper())");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erro ao configurar permissões para módulo $($ModuleName.ToUpper())");
        throw;
    }
}

/// <summary>
/// Verifica se as permissões do módulo $($ModuleName.ToUpper()) já estão configuradas
/// </summary>
public async Task<bool> Are$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))PermissionsConfiguredAsync(string adminConnectionString)
{
    try
    {
        using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        var result = await ExecuteScalarAsync<bool>(connection, `$`"
            SELECT EXISTS (
                SELECT 1 FROM pg_catalog.pg_roles 
                WHERE rolname = '${ModuleName}_role'
            )
            `$`");

        return result;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Erro ao verificar permissões do módulo $($ModuleName.ToUpper()), assumindo não configurado");
        return false;
    }
}

private async Task Execute$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaScript(NpgsqlConnection connection, string scriptType, params string[] parameters)
{
    string sql = scriptType switch
    {
        "00-roles" => Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))CreateRolesScript(parameters[0]),
        "01-permissions" => Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))GrantPermissionsScript(),
        _ => throw new ArgumentException(`$`"Script type '{scriptType}' not recognized for $ModuleName module")
    };

    logger.LogDebug("Executando script do módulo $($ModuleName.ToUpper()): {ScriptType}", scriptType);
    await ExecuteSqlAsync(connection, sql);
}

private string Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))CreateRolesScript(string ${ModuleName}Password) => `$`"
    -- Create dedicated role for $ModuleName module
    -- SECURITY: Password provided via secure parameter, never hardcoded
    CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '{${ModuleName}Password}';

    -- Grant ${ModuleName} role to app role for cross-module access
    -- NOTE: Assumes meajudaai_app_role already exists (created during initial setup)
    GRANT ${ModuleName}_role TO meajudaai_app_role;
    `$`";

private string Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))GrantPermissionsScript() => `$`"
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
    `$`";
"@

$ManagerTemplate | Out-File -FilePath "$ModulePath/SchemaPermissionsManager-template.cs" -Encoding UTF8

# Criar template para Extensions.cs do módulo
Write-Host "⚙️ Criando template para Extensions.cs..." -ForegroundColor Yellow
$ExtensionsTemplate = @"
// Adicione este método ao Extensions.cs do módulo $($ModuleName.ToUpper()):

/// <summary>
/// Adiciona o módulo $($ModuleName.ToUpper()) com registro de serviços apenas
/// A configuração de permissões deve ser feita durante a inicialização da aplicação
/// </summary>
public static IServiceCollection Add$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModuleWithSchemaIsolation(
    this IServiceCollection services, IConfiguration configuration)
{
    // Register module services only - no runtime permission setup here
    services.Add$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))Module(configuration);
    
    // Register schema isolation configuration for later use during startup
    services.Configure<$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaOptions>(options =>
    {
        options.EnableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
        options.ModuleRolePasswordConfigKey = "Database:$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))RolePassword";
    });
    
    return services;
}

/// <summary>
/// Inicializa as permissões do módulo $($ModuleName.ToUpper()) durante o startup da aplicação
/// Chame este método após o host ser construído, usando app.Services.CreateScope()
/// </summary>
public static async Task Initialize$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaPermissionsAsync(
    this IServiceProvider serviceProvider, IConfiguration configuration)
{
    var enableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
    
    if (!enableSchemaIsolation)
    {
        return; // Schema isolation disabled, skip permission setup
    }

    using var scope = serviceProvider.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var logger = scopedServices.GetRequiredService<ILogger<SchemaPermissionsManager>>();
    
    try
    {
        var schemaManager = scopedServices.GetRequiredService<SchemaPermissionsManager>();
        var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
        
        // SECURITY: Get passwords from secure configuration (Azure Key Vault, environment variables, etc.)
        var ${ModuleName}RolePassword = configuration.GetValue<string>("Database:$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))RolePassword") 
            ?? throw new InvalidOperationException("$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))RolePassword must be configured in secure configuration");
        
        if (!string.IsNullOrEmpty(adminConnectionString))
        {
            await schemaManager.Ensure$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModulePermissionsAsync(
                adminConnectionString, ${ModuleName}RolePassword);
            
            logger.LogInformation("✅ Schema permissions initialized for $($ModuleName.ToUpper()) module");
        }
        else
        {
            logger.LogWarning("⚠️ AdminPostgres connection string not found, skipping $($ModuleName.ToUpper()) schema permission setup");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Failed to initialize $($ModuleName.ToUpper()) module schema permissions");
        throw; // Re-throw to prevent application startup with incorrect permissions
    }
}

/// <summary>
/// Configuration options for $($ModuleName.ToUpper()) module schema isolation
/// </summary>
public class $($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaOptions
{
    public bool EnableSchemaIsolation { get; set; }
    public string ModuleRolePasswordConfigKey { get; set; } = string.Empty;
}
// USAGE EXAMPLE in Program.cs or Startup:
// 
// // 1. During service registration:
// builder.Services.Add$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModuleWithSchemaIsolation(builder.Configuration);
// 
// // 2. After building the host, during application startup:
// var app = builder.Build();
// 
// // Initialize schema permissions before starting the application
// await app.Services.Initialize$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaPermissionsAsync(app.Configuration);
// 
// app.Run();
"@

$ExtensionsTemplate | Out-File -FilePath "$ModulePath/Extensions-template.cs" -Encoding UTF8

# Resumo
Write-Host ""
Write-Host "✅ Módulo '$ModuleName' criado com sucesso!" -ForegroundColor Green
Write-Host "📁 Localização: $ModulePath" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor White
Write-Host "1. 📝 Configure o DbContext com: modelBuilder.HasDefaultSchema(`"$ModuleName`")" -ForegroundColor Gray
Write-Host "2. 🔧 Adicione os métodos do template ao SchemaPermissionsManager.cs" -ForegroundColor Gray
Write-Host "3. ⚙️ Adicione o método do template ao Extensions.cs do módulo" -ForegroundColor Gray
Write-Host "4. � IMPORTANTE: Substitua <secure_password> no arquivo 00-roles.sql por senha forte" -ForegroundColor Red
Write-Host "5. �🔑 Configure senhas via Azure Key Vault ou variáveis de ambiente seguras" -ForegroundColor Red
Write-Host "6. ⚠️  NUNCA comite senhas reais no código fonte" -ForegroundColor Red
Write-Host ""
Write-Host "📄 Templates criados:" -ForegroundColor White
Write-Host "  - $ModulePath/SchemaPermissionsManager-template.cs" -ForegroundColor Gray
Write-Host "  - $ModulePath/Extensions-template.cs" -ForegroundColor Gray