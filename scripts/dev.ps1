<#
.SYNOPSIS
    Inicia o ambiente de desenvolvimento do MeAjudaAi
.DESCRIPTION
    Script para iniciar a aplicação via Aspire AppHost
.EXAMPLE
    .\scripts\dev.ps1
#>

$ErrorActionPreference = "Stop"

# Configurar variáveis de ambiente para desenvolvimento
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"
$env:POSTGRES_PASSWORD = "postgres"

Write-Host "🚀 Iniciando MeAjudaAi - Ambiente de Desenvolvimento" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Docker
Write-Host "🐳 Verificando Docker..." -ForegroundColor Yellow
try {
    $dockerStatus = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Docker não está rodando. Inicie o Docker Desktop primeiro." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Docker está rodando" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não encontrado. Instale o Docker Desktop." -ForegroundColor Red
    exit 1
}

# Verificar .NET SDK
Write-Host ""
Write-Host "📦 Verificando .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK não encontrado. Instale o .NET 10 SDK." -ForegroundColor Red
    exit 1
}

# Restaurar dependências
Write-Host ""
Write-Host "📥 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependências restauradas" -ForegroundColor Green

# Iniciar aplicação
Write-Host ""
Write-Host "▶️  Iniciando Aspire AppHost..." -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  NOTA: Se houver erro relacionado a DCP/Dashboard, execute via VS Code (F5)" -ForegroundColor Yellow
Write-Host "   Mais detalhes: https://github.com/dotnet/aspire/issues/6789" -ForegroundColor Gray
Write-Host ""
Write-Host "📊 Aspire Dashboard estará disponível em:" -ForegroundColor Yellow
Write-Host "   https://localhost:17063" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Serviços que serão iniciados:" -ForegroundColor Yellow
Write-Host "   - PostgreSQL (porta 5432)" -ForegroundColor White
Write-Host "   - Redis (porta 6379)" -ForegroundColor White
Write-Host "   - Keycloak (porta 8080)" -ForegroundColor White
Write-Host "   - RabbitMQ (porta 5672)" -ForegroundColor White
Write-Host "   - API Backend (porta 7524/5545)" -ForegroundColor White
Write-Host "   - Admin Portal Blazor" -ForegroundColor White
Write-Host ""
Write-Host "Pressione Ctrl+C para parar..." -ForegroundColor Gray
Write-Host ""

try {
    Push-Location "$PSScriptRoot\..\src\Aspire\MeAjudaAi.AppHost"
    dotnet run

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "❌ Aspire AppHost exited with code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} finally {
    Pop-Location
}
