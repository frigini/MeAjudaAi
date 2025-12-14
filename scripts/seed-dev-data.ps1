#requires -Version 7.0
<#
.SYNOPSIS
    Seed inicial de dados para ambiente de desenvolvimento

.DESCRIPTION
    Popula o banco de dados com dados iniciais para desenvolvimento e testes:
    - Categorias de serviços
    - Serviços básicos
    - Cidades permitidas
    - Usuários de teste
    - Providers de exemplo

.PARAMETER Environment
    Ambiente alvo (Development, Staging). Default: Development

.EXAMPLE
    .\seed-dev-data.ps1
    
.EXAMPLE
    .\seed-dev-data.ps1 -Environment Staging
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Development', 'Staging')]
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
Write-Host "📦 Seeding: ServiceCatalogs" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

# Categorias
$categories = @(
    @{ name = "Saúde"; description = "Serviços relacionados à saúde e bem-estar" }
    @{ name = "Educação"; description = "Serviços educacionais e de capacitação" }
    @{ name = "Assistência Social"; description = "Programas de assistência e suporte social" }
    @{ name = "Jurídico"; description = "Serviços jurídicos e advocatícios" }
    @{ name = "Habitação"; description = "Moradia e programas habitacionais" }
    @{ name = "Alimentação"; description = "Programas de segurança alimentar" }
)

$categoryIds = @{}

foreach ($cat in $categories) {
    Write-Info "Criando categoria: $($cat.name)"
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/catalogs/admin/categories" `
            -Method Post `
            -Headers $headers `
            -Body ($cat | ConvertTo-Json -Depth 10)
        
        $categoryIds[$cat.name] = $response.id
        Write-Success "Categoria '$($cat.name)' criada (ID: $($response.id))"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Warning "Categoria '$($cat.name)' já existe"
        } else {
            Write-Error "Erro ao criar categoria '$($cat.name)': $_"
        }
    }
}

# Serviços
if ($categoryIds.Count -gt 0) {
    $services = @(
        @{ 
            name = "Atendimento Psicológico Gratuito"
            description = "Atendimento psicológico individual ou em grupo"
            categoryId = $categoryIds["Saúde"]
            eligibilityCriteria = "Renda familiar até 3 salários mínimos"
            requiredDocuments = @("RG", "CPF", "Comprovante de residência", "Comprovante de renda")
        }
        @{
            name = "Curso de Informática Básica"
            description = "Curso gratuito de informática e inclusão digital"
            categoryId = $categoryIds["Educação"]
            eligibilityCriteria = "Jovens de 14 a 29 anos"
            requiredDocuments = @("RG", "CPF", "Comprovante de escolaridade")
        }
        @{
            name = "Cesta Básica"
            description = "Distribuição mensal de cestas básicas"
            categoryId = $categoryIds["Alimentação"]
            eligibilityCriteria = "Famílias em situação de vulnerabilidade"
            requiredDocuments = @("Cadastro único", "Comprovante de residência")
        }
        @{
            name = "Orientação Jurídica Gratuita"
            description = "Atendimento jurídico para questões civis e trabalhistas"
            categoryId = $categoryIds["Jurídico"]
            eligibilityCriteria = "Renda familiar até 2 salários mínimos"
            requiredDocuments = @("RG", "CPF", "Documentos relacionados ao caso")
        }
    )

    foreach ($service in $services) {
        if ($service.categoryId) {
            Write-Info "Criando serviço: $($service.name)"
            try {
                $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/catalogs/admin/services" `
                    -Method Post `
                    -Headers $headers `
                    -Body ($service | ConvertTo-Json -Depth 10)
                
                Write-Success "Serviço '$($service.name)' criado"
            } catch {
                if ($_.Exception.Response.StatusCode -eq 409) {
                    Write-Warning "Serviço '$($service.name)' já existe"
                } else {
                    Write-Error "Erro ao criar serviço '$($service.name)': $_"
                }
            }
        }
    }
}

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
Write-Host "🎉 Seed Concluído!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Dados inseridos:" -ForegroundColor Cyan
# Computar contagens seguras para evitar referência a variáveis indefinidas
$categoryCount = if ($categories) { $categories.Count } else { 0 }
$serviceCount = if ($services) { $services.Count } else { 0 }
$cityCount = if ($allowedCities) { $allowedCities.Count } else { 0 }
Write-Host "   • Categorias: $categoryCount" -ForegroundColor White
Write-Host "   • Serviços: $serviceCount" -ForegroundColor White
Write-Host "   • Cidades: $cityCount" -ForegroundColor White
Write-Host ""
Write-Host "💡 Próximos passos:" -ForegroundColor Cyan
Write-Host "   1. Cadastrar providers usando Bruno collections" -ForegroundColor White
Write-Host "   2. Indexar providers para busca" -ForegroundColor White
Write-Host "   3. Testar endpoints de busca" -ForegroundColor White
Write-Host ""
