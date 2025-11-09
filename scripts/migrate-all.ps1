#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Script para executar migrações de todos os módulos do MeAjudaAi

.DESCRIPTION
    Este script facilita a execução da ferramenta de migração para todos os módulos.
    Ele descobre automaticamente todos os DbContexts e aplica as migrações necessárias.

.PARAMETER Command
    O comando a ser executado:
    - migrate: Aplica todas as migrações pendentes (padrão)
    - create: Cria os bancos de dados se não existirem
    - reset: Remove e recria todos os bancos
    - status: Mostra o status das migrações

.PARAMETER ConnectionString
    String de conexão customizada (opcional)

.EXAMPLE
    .\migrate-all.ps1
    Aplica todas as migrações pendentes

.EXAMPLE
    .\migrate-all.ps1 -Command status
    Mostra o status das migrações

.EXAMPLE
    .\migrate-all.ps1 -Command reset
    Remove e recria todos os bancos
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet("migrate", "create", "reset", "status")]
    [string]$Command = "migrate",
    
    [Parameter()]
    [string]$ConnectionString = $null
)

# Cores para output
$Green = "`e[32m"
$Red = "`e[31m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = $Reset)
    Write-Host "$Color$Message$Reset"
}

# Verificar se estamos no diretório raiz do projeto
$solutionFile = Get-ChildItem -Name "*.sln" -ErrorAction SilentlyContinue
if (-not $solutionFile) {
    Write-ColoredOutput "❌ Arquivo .sln não encontrado. Execute este script no diretório raiz do projeto." $Red
    exit 1
}

Write-ColoredOutput "🔧 MeAjudaAi Migration Tool" $Blue
Write-ColoredOutput "📋 Comando: $Command" $Blue
Write-ColoredOutput "📁 Projeto: $($solutionFile[0])" $Blue
Write-Host

# Verificar se o PostgreSQL está rodando
try {
    # Verificar se existe algum container com nome "postgres" 
    $existingContainer = & docker ps -a --filter "name=postgres" --format "{{.Names}}" 2>$null
    
    if ($existingContainer -match "postgres") {
        # Verificar se está rodando
        $runningContainer = & docker ps --filter "name=postgres" --format "{{.Names}}" 2>$null
        if ($runningContainer -match "postgres") {
            Write-ColoredOutput "✅ PostgreSQL container já está rodando" $Green
        } else {
            Write-ColoredOutput "⚠️  PostgreSQL container existe mas não está rodando. Iniciando..." $Yellow
            & docker start postgres
            if ($LASTEXITCODE -ne 0) {
                Write-ColoredOutput "❌ Erro ao iniciar container PostgreSQL existente" $Red
                exit 1
            }
            Start-Sleep -Seconds 5
            Write-ColoredOutput "✅ PostgreSQL container iniciado" $Green
        }
    } else {
        Write-ColoredOutput "⚠️  PostgreSQL container não encontrado. Criando novo..." $Yellow
        & docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15
        if ($LASTEXITCODE -ne 0) {
            Write-ColoredOutput "❌ Erro ao criar container PostgreSQL" $Red
            exit 1
        }
        Start-Sleep -Seconds 5
        Write-ColoredOutput "✅ PostgreSQL container criado e iniciado" $Green
    }
} catch {
    Write-ColoredOutput "❌ Erro ao verificar/iniciar PostgreSQL: $_" $Red
    exit 1
}

# Construir a ferramenta de migração
Write-ColoredOutput "🔨 Construindo a ferramenta de migração..." $Blue
try {
    $buildResult = & dotnet build tools/MigrationTool --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-ColoredOutput "❌ Erro ao construir a ferramenta de migração" $Red
        exit 1
    }
    Write-ColoredOutput "✅ Ferramenta construída com sucesso" $Green
} catch {
    Write-ColoredOutput "❌ Erro ao construir a ferramenta: $_" $Red
    exit 1
}

# Executar a ferramenta
Write-ColoredOutput "🚀 Executando comando: $Command" $Blue
Write-Host

try {
    if ($ConnectionString) {
        $env:ConnectionString = $ConnectionString
    }
    
    & dotnet run --project tools/MigrationTool --configuration Release -- $Command
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host
        Write-ColoredOutput "✅ Comando executado com sucesso!" $Green
    } else {
        Write-Host
        Write-ColoredOutput "❌ Comando falhou com código de saída: $LASTEXITCODE" $Red
        exit $LASTEXITCODE
    }
} catch {
    Write-ColoredOutput "❌ Erro ao executar a ferramenta: $_" $Red
    exit 1
} finally {
    if ($ConnectionString) {
        Remove-Item Env:ConnectionString -ErrorAction SilentlyContinue
    }
}

# Sugestões baseadas no comando executado
Write-Host
switch ($Command) {
    "migrate" {
        Write-ColoredOutput "💡 Dica: Use './migrate-all.ps1 status' para verificar o status das migrações" $Yellow
    }
    "create" {
        Write-ColoredOutput "💡 Dica: Use './migrate-all.ps1 migrate' para aplicar as migrações" $Yellow
    }
    "reset" {
        Write-ColoredOutput "💡 Dica: Use './migrate-all.ps1 status' para verificar se tudo foi resetado corretamente" $Yellow
    }
    "status" {
        Write-ColoredOutput "💡 Dica: Use './migrate-all.ps1 migrate' se houver migrações pendentes" $Yellow
    }
}