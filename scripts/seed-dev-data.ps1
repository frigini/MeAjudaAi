#requires -Version 7.0
<#
.SYNOPSIS
    Seed de dados de TESTE para ambiente de desenvolvimento

.DESCRIPTION
    Popula o banco de dados com dados de TESTE via API REST:
    - Cidades permitidas (10 capitais brasileiras)
    - Usuários de teste (futuro)
    - Providers de exemplo (futuro)

    NOTA: Dados ESSENCIAIS de domínio (ServiceCategories, Services) devem ser 
    inseridos via SQL script após migrations. Veja: scripts/seed-service-catalogs.sql

.PARAMETER Environment
    Ambiente alvo (Development apenas). Default: Development

.PARAMETER ApiBaseUrl
    URL base da API. Default: http://localhost:5000
    Use portas Aspire quando executar via Aspire orchestration (ex: https://localhost:7524)

.EXAMPLE
    .\seed-dev-data.ps1

.EXAMPLE
    .\seed-dev-data.ps1 -ApiBaseUrl "https://localhost:7524"
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Development')]
    [string]$Environment = 'Development',
    
    [Parameter()]
    [string]$ApiBaseUrl = 'http://localhost:5000'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Cores para output
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }

Write-Host "🌱 Seed de Dados - MeAjudaAi [$Environment]" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Verificar se API está rodando
Write-Info "Verificando API em $ApiBaseUrl..."
try {
    $health = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method Get -TimeoutSec 5
    Write-Success "API está rodando"
} catch {
    Write-Error "API não está acessível em $ApiBaseUrl"
    Write-Host "Inicie a API primeiro: cd src/Bootstrapper/MeAjudaAi.ApiService && dotnet run" -ForegroundColor Yellow
    exit 1
}

# Obter token de autenticação
Write-Info "Obtendo token de autenticação..."
$keycloakUrl = "http://localhost:8080"
$tokenParams = @{
    Uri = "$keycloakUrl/realms/meajudaai/protocol/openid-connect/token"
    Method = 'Post'
    ContentType = 'application/x-www-form-urlencoded'
    Body = @{
        client_id = 'meajudaai-api'
        username = 'admin'
        password = 'admin123'
        grant_type = 'password'
    }
}

try {
    $tokenResponse = Invoke-RestMethod @tokenParams
    $token = $tokenResponse.access_token
    Write-Success "Token obtido com sucesso"
} catch {
    Write-Error "Falha ao obter token do Keycloak"
    Write-Host "Verifique se Keycloak está rodando: docker-compose up keycloak" -ForegroundColor Yellow
    exit 1
}

$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type' = 'application/json'
    'Api-Version' = '1.0'
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "ℹ️  ServiceCatalogs: Usando seed SQL" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Info "ServiceCategories e Services são criados via SQL após migrations"
Write-Info "Execute: psql -f scripts/seed-service-catalogs.sql"
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📍 Seeding: Locations (AllowedCities)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

$allowedCities = @(
    @{ ibgeCode = "3550308"; cityName = "São Paulo"; state = "SP"; isActive = $true }
    @{ ibgeCode = "3304557"; cityName = "Rio de Janeiro"; state = "RJ"; isActive = $true }
    @{ ibgeCode = "3106200"; cityName = "Belo Horizonte"; state = "MG"; isActive = $true }
    @{ ibgeCode = "4106902"; cityName = "Curitiba"; state = "PR"; isActive = $true }
    @{ ibgeCode = "4314902"; cityName = "Porto Alegre"; state = "RS"; isActive = $true }
    @{ ibgeCode = "5300108"; cityName = "Brasília"; state = "DF"; isActive = $true }
    @{ ibgeCode = "2927408"; cityName = "Salvador"; state = "BA"; isActive = $true }
    @{ ibgeCode = "2304400"; cityName = "Fortaleza"; state = "CE"; isActive = $true }
    @{ ibgeCode = "2611606"; cityName = "Recife"; state = "PE"; isActive = $true }
    @{ ibgeCode = "1302603"; cityName = "Manaus"; state = "AM"; isActive = $true }
)

foreach ($city in $allowedCities) {
    Write-Info "Adicionando cidade: $($city.cityName)/$($city.state)"
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/locations/admin/allowed-cities" `
            -Method Post `
            -Headers $headers `
            -Body ($city | ConvertTo-Json -Depth 10)
        
        Write-Success "Cidade '$($city.cityName)/$($city.state)' adicionada"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Warning "Cidade '$($city.cityName)/$($city.state)' já existe"
        } else {
            Write-Error "Erro ao adicionar cidade '$($city.cityName)/$($city.state)': $_"
        }
    }
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🎉 Seed de Dados de Teste Concluído!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Dados de TESTE inseridos:" -ForegroundColor Cyan
Write-Host "   • Cidades permitidas: $cityCount" -ForegroundColor White
Write-Host ""
Write-Host "💡 Dados ESSENCIAIS (via SQL):" -ForegroundColor Cyan
Write-Host "   • ServiceCategories: 8 categorias" -ForegroundColor White
Write-Host "   • Services: 12 serviços padrão" -ForegroundColor White
Write-Host "   • Execute: psql -f scripts/seed-service-catalogs.sql" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 Próximos passos:" -ForegroundColor Cyan
Write-Host "   1. Cadastrar providers usando Bruno collections" -ForegroundColor White
Write-Host "   2. Indexar providers para busca" -ForegroundColor White
Write-Host "   3. Testar endpoints de busca" -ForegroundColor White
Write-Host ""
