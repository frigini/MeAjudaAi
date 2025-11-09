#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Comando simplificado para aplicar migrações usando dotnet ef diretamente

.DESCRIPTION
    Este script aplica migrações usando comandos dotnet ef para cada módulo individualmente.
    Mais simples e direto que a ferramenta customizada.

    Configuração de banco de dados via variáveis de ambiente:
    - DB_HOST (padrão: localhost)
    - DB_PORT (padrão: 5432)
    - DB_NAME (padrão: MeAjudaAi)
    - DB_USER (padrão: postgres)
    - DB_PASSWORD (obrigatório - será solicitado se não definido)

.PARAMETER Command
    O comando a ser executado:
    - migrate: Aplica todas as migrações (padrão)
    - add: Adiciona uma nova migração
    - remove: Remove a última migração
    - status: Lista migrações aplicadas e pendentes

.PARAMETER Module
    Módulo específico (Users, Providers, etc.). Se não especificado, executa para todos.

.PARAMETER MigrationName
    Nome da migração (apenas para comando 'add')

.EXAMPLE
    .\ef-migrate.ps1
    Aplica migrações para todos os módulos

.EXAMPLE
    .\ef-migrate.ps1 -Module Providers
    Aplica migrações apenas para o módulo Providers

.EXAMPLE
    .\ef-migrate.ps1 -Command add -Module Users -MigrationName "AddNewUserField"
    Adiciona nova migração ao módulo Users
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet("migrate", "add", "remove", "status", "list")]
    [string]$Command = "migrate",
    
    [Parameter()]
    [ValidateSet("Users", "Providers")]
    [string]$Module = $null,
    
    [Parameter()]
    [string]$MigrationName = $null
)

# Função para obter configuração do banco de dados
function Get-DatabaseConfig {
    $dbHost = $env:DB_HOST ?? "localhost"
    $dbPort = $env:DB_PORT ?? "5432"
    $dbName = $env:DB_NAME ?? "MeAjudaAi"
    $dbUser = $env:DB_USER ?? "postgres"
    $dbPassword = $env:DB_PASSWORD
    
    if (-not $dbPassword) {
        Write-ColoredOutput "❌ Variável de ambiente DB_PASSWORD não definida." $Red
        Write-ColoredOutput "Configure as seguintes variáveis de ambiente:" $Yellow
        Write-ColoredOutput "  DB_HOST (padrão: localhost)" $Yellow
        Write-ColoredOutput "  DB_PORT (padrão: 5432)" $Yellow
        Write-ColoredOutput "  DB_NAME (padrão: MeAjudaAi)" $Yellow
        Write-ColoredOutput "  DB_USER (padrão: postgres)" $Yellow
        Write-ColoredOutput "  DB_PASSWORD (obrigatório)" $Yellow
        Write-Host
        Write-ColoredOutput "Exemplo:" $Blue
        Write-ColoredOutput "`$env:DB_PASSWORD='suasenha'; .\ef-migrate.ps1" $Blue
        exit 1
    }
    
    return "Host=$dbHost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPassword"
}

# Obter string de conexão
$connectionString = Get-DatabaseConfig

# Definir módulos e seus contextos
$Modules = @{
    "Users" = @{
        "Project" = "src/Modules/Users/Infrastructure/MeAjudaAi.Modules.Users.Infrastructure.csproj"
        "Context" = "UsersDbContext"
        "OutputDir" = "Persistence/Migrations"
        "ConnectionString" = $connectionString
    }
    "Providers" = @{
        "Project" = "src/Modules/Providers/Infrastructure/MeAjudaAi.Modules.Providers.Infrastructure.csproj"
        "Context" = "ProvidersDbContext"
        "OutputDir" = "Persistence/Migrations"
        "ConnectionString" = $connectionString
    }
}

# Cores
$Green = "`e[32m"; $Red = "`e[31m"; $Yellow = "`e[33m"; $Blue = "`e[34m"; $Reset = "`e[0m"

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = $Reset)
    Write-Host "$Color$Message$Reset"
}

function Invoke-EFCommand {
    param(
        [string]$ModuleName,
        [hashtable]$ModuleInfo,
        [string]$EFCommand
    )
    
    Write-ColoredOutput "📦 $ModuleName`: $EFCommand" $Blue
    
    try {
        # Set connection string as environment variable
        $env:ConnectionStrings__DefaultConnection = $ModuleInfo.ConnectionString
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        
        Invoke-Expression "dotnet ef $EFCommand --project `"$($ModuleInfo.Project)`" --context $($ModuleInfo.Context) --verbose"
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColoredOutput "  ✅ Sucesso" $Green
            return $true
        } else {
            Write-ColoredOutput "  ❌ Falhou (código: $LASTEXITCODE)" $Red
            return $false
        }
    } catch {
        Write-ColoredOutput "  ❌ Erro: $_" $Red
        return $false
    } finally {
        Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
        Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    }
}

# Determinar quais módulos processar
$ModulesToProcess = if ($Module) { 
    @($Module) 
} else { 
    $Modules.Keys 
}

Write-ColoredOutput "🔧 Entity Framework Migration Tool" $Blue
Write-ColoredOutput "📋 Comando: $Command" $Blue
Write-ColoredOutput "🎯 Módulos: $($ModulesToProcess -join ', ')" $Blue
Write-Host

# Verificar se dotnet ef está instalado
try {
    & dotnet ef --version 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-ColoredOutput "❌ dotnet ef não encontrado. Instalando..." $Yellow
        & dotnet tool install --global dotnet-ef
        if ($LASTEXITCODE -ne 0) {
            Write-ColoredOutput "❌ Falha ao instalar dotnet ef" $Red
            exit 1
        }
    }
    Write-ColoredOutput "✅ dotnet ef disponível" $Green
} catch {
    Write-ColoredOutput "❌ Erro ao verificar dotnet ef: $_" $Red
    exit 1
}

$successCount = 0
$totalCount = 0
$failedCount = 0

foreach ($ModuleName in $ModulesToProcess) {
    if (-not $Modules.ContainsKey($ModuleName)) {
        Write-ColoredOutput "⚠️  Módulo '$ModuleName' não encontrado" $Yellow
        continue
    }
    
    $moduleInfo = $Modules[$ModuleName]
    $totalCount++
    
    # Verificar se o projeto existe
    if (-not (Test-Path $moduleInfo.Project)) {
        Write-ColoredOutput "❌ Projeto não encontrado: $($moduleInfo.Project)" $Red
        $failedCount++
        continue
    }
    
    switch ($Command) {
        "migrate" {
            $efCommand = "database update"
            if (Invoke-EFCommand $ModuleName $moduleInfo $efCommand) {
                $successCount++
            } else {
                $failedCount++
            }
        }
        
        "add" {
            if (-not $MigrationName) {
                Write-ColoredOutput "❌ Nome da migração é obrigatório para o comando 'add'" $Red
                $failedCount++
                continue
            }
            $efCommand = "migrations add `"$MigrationName`" --output-dir `"$($moduleInfo.OutputDir)`""
            if (Invoke-EFCommand $ModuleName $moduleInfo $efCommand) {
                $successCount++
            } else {
                $failedCount++
            }
        }
        
        "remove" {
            $efCommand = "migrations remove"
            if (Invoke-EFCommand $ModuleName $moduleInfo $efCommand) {
                $successCount++
            } else {
                $failedCount++
            }
        }
        
        "status" {
            $efCommand = "migrations list"
            if (Invoke-EFCommand $ModuleName $moduleInfo $efCommand) {
                $successCount++
            } else {
                $failedCount++
            }
        }
        
        "list" {
            $efCommand = "migrations list"
            if (Invoke-EFCommand $ModuleName $moduleInfo $efCommand) {
                $successCount++
            } else {
                $failedCount++
            }
        }
    }
    
    Write-Host
}

# Resumo
Write-ColoredOutput "📊 Resumo: $successCount sucessos, $failedCount falhas de $totalCount módulos" $Blue

if ($failedCount -eq 0 -and $totalCount -gt 0) {
    Write-ColoredOutput "✅ Todos os comandos executados com sucesso!" $Green
    exit 0
} elseif ($totalCount -eq 0) {
    Write-ColoredOutput "⚠️  Nenhum módulo foi processado." $Yellow
    exit 1
} else {
    Write-ColoredOutput "❌ $failedCount comandos falharam. Verifique os logs acima." $Red
    exit 1
}