#!/usr/bin/env pwsh
# create-module.ps1
# Script para criar estrutura de banco de dados para novos módulos

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
CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '${ModuleName}_secret';

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
public async Task Ensure$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModulePermissionsAsync(
    string adminConnectionString,
    string ${ModuleName}RolePassword = "${ModuleName}_secret", 
    string appRolePassword = "app_secret")
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
        await Execute$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))SchemaScript(connection, "00-roles", ${ModuleName}RolePassword, appRolePassword);
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
        "00-roles" => Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))CreateRolesScript(parameters[0], parameters[1]),
        "01-permissions" => Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))GrantPermissionsScript(),
        _ => throw new ArgumentException(`$`"Script type '{scriptType}' not recognized for $ModuleName module")
    };

    logger.LogDebug("Executando script do módulo $($ModuleName.ToUpper()): {ScriptType}", scriptType);
    await ExecuteSqlAsync(connection, sql);
}

private string Get$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))CreateRolesScript(string ${ModuleName}Password, string appPassword) => `$`"
    -- Create dedicated role for $ModuleName module
    CREATE ROLE ${ModuleName}_role LOGIN PASSWORD '{${ModuleName}Password}';

    -- Grant ${ModuleName} role to app role for cross-module access
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
/// Adiciona o módulo $($ModuleName.ToUpper()) com isolamento de schema opcional
/// </summary>
public static async Task<IServiceCollection> Add$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModuleWithSchemaIsolationAsync(
    this IServiceCollection services, IConfiguration configuration)
{
    var enableSchemaIsolation = configuration.GetValue<bool>("Database:EnableSchemaIsolation", false);
    
    if (enableSchemaIsolation)
    {
        var serviceProvider = services.BuildServiceProvider();
        var schemaManager = serviceProvider.GetRequiredService<SchemaPermissionsManager>();
        var adminConnectionString = configuration.GetConnectionString("AdminPostgres");
        
        if (!string.IsNullOrEmpty(adminConnectionString))
        {
            await schemaManager.Ensure$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))ModulePermissionsAsync(adminConnectionString);
        }
    }
    
    // Continue with regular module registration...
    return services.Add$($ModuleName.Substring(0,1).ToUpper() + $ModuleName.Substring(1))Module(configuration);
}
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
Write-Host "4. 🔑 Configure as senhas em production (não usar padrões)" -ForegroundColor Gray
Write-Host ""
Write-Host "📄 Templates criados:" -ForegroundColor White
Write-Host "  - $ModulePath/SchemaPermissionsManager-template.cs" -ForegroundColor Gray
Write-Host "  - $ModulePath/Extensions-template.cs" -ForegroundColor Gray