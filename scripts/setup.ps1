<#
.SYNOPSIS
    Setup inicial do projeto MeAjudaAi
.DESCRIPTION
    Configura o ambiente de desenvolvimento do zero
.PARAMETER DevOnly
    Setup apenas para desenvolvimento (sem Azure/Cloud)
.PARAMETER Verbose
    Exibe logs detalhados
.EXAMPLE
    .\scripts\setup.ps1
    .\scripts\setup.ps1 -DevOnly
#>

param(
    [switch]$DevOnly,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "⚙️  Setup MeAjudaAi - Configuração Inicial" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar pré-requisitos
Write-Host "1️⃣  Verificando pré-requisitos..." -ForegroundColor Yellow
Write-Host ""

$missing = @()

# .NET SDK
Write-Host "  📦 .NET SDK..." -NoNewline
try {
    $dotnetVersion = dotnet --version
    try {
        $version = [Version]::new($dotnetVersion)
        $requiredVersion = [Version]::new("10.0.0")
        if ($version -ge $requiredVersion) {
            Write-Host " ✅ v$dotnetVersion" -ForegroundColor Green
        } else {
            Write-Host " ⚠️  v$dotnetVersion (recomendado 10.0+)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host " ⚠️  v$dotnetVersion (não foi possível validar versão)" -ForegroundColor Yellow
        Write-Host "      Versão detectada mas formato inesperado: $_" -ForegroundColor Yellow
    }
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $missing += ".NET 10 SDK (https://dotnet.microsoft.com/download/dotnet/10.0)"
}

# Docker
Write-Host "  🐳 Docker..." -NoNewline
try {
    docker info 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $dockerVersion = (docker --version) -replace "Docker version ", "" -replace ",.*", ""
        Write-Host " ✅ v$dockerVersion" -ForegroundColor Green
    } else {
        Write-Host " ⚠️  Instalado mas não está rodando" -ForegroundColor Yellow
        $missing += "Docker Desktop (precisa estar rodando)"
    }
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $missing += "Docker Desktop (https://www.docker.com/products/docker-desktop)"
}

# Git
Write-Host "  🔧 Git..." -NoNewline
try {
    $gitVersion = (git --version) -replace "git version ", ""
    Write-Host " ✅ v$gitVersion" -ForegroundColor Green
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $missing += "Git (https://git-scm.com/)"
}

if ($missing.Count -gt 0) {
    Write-Host ""
    Write-Host "❌ Pré-requisitos faltando:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "Instale os itens acima e execute o setup novamente." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "✅ Todos os pré-requisitos estão instalados!" -ForegroundColor Green
Write-Host ""

# 2. Restaurar dependências
Write-Host "2️⃣  Restaurando dependências NuGet..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependências restauradas" -ForegroundColor Green
Write-Host ""

# 3. Build inicial
Write-Host "3️⃣  Compilando solução..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na compilação" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Compilação bem-sucedida" -ForegroundColor Green
Write-Host ""

# 4. Configurar Keycloak (instruções)
Write-Host "4️⃣  Configuração do Keycloak" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  IMPORTANTE: Configuração manual necessária" -ForegroundColor Yellow
Write-Host ""
Write-Host "O Admin Portal Blazor requer um client configurado no Keycloak." -ForegroundColor White
Write-Host ""
Write-Host "📖 Siga as instruções em:" -ForegroundColor Cyan
Write-Host "   docs/keycloak-admin-portal-setup.md" -ForegroundColor White
Write-Host ""
Write-Host "Resumo rápido:" -ForegroundColor Yellow
Write-Host "   1. Execute: .\scripts\dev.ps1" -ForegroundColor White
Write-Host "   2. Acesse: http://localhost:8080" -ForegroundColor White
Write-Host "   3. Login: admin/admin" -ForegroundColor White
Write-Host "   4. Realm: meajudaai" -ForegroundColor White
Write-Host "   5. Clients → Create Client" -ForegroundColor White
Write-Host "   6. Client ID: admin-portal" -ForegroundColor White
Write-Host "   7. Configure conforme documentação" -ForegroundColor White
Write-Host ""

# 5. Próximos passos
Write-Host "✅ Setup concluído!" -ForegroundColor Green
Write-Host ""
Write-Host "📚 Próximos passos:" -ForegroundColor Cyan
Write-Host ""
Write-Host "   1. Iniciar desenvolvimento:" -ForegroundColor White
Write-Host "      .\scripts\dev.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "   2. Executar testes:" -ForegroundColor White
Write-Host "      dotnet test" -ForegroundColor Gray
Write-Host ""
Write-Host "   3. Ver documentação:" -ForegroundColor White
Write-Host "      mkdocs serve" -ForegroundColor Gray
Write-Host "      https://frigini.github.io/MeAjudaAi/" -ForegroundColor Gray
Write-Host ""
Write-Host "   4. Comandos disponíveis (via Makefile):" -ForegroundColor White
Write-Host "      make help" -ForegroundColor Gray
Write-Host ""

if (-not $DevOnly) {
    Write-Host "💡 Dica: Use 'make dev' para atalhos rápidos!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green
