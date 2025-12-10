# Script para gerar coverage EXCLUINDO código gerado do compilador
# Uso: .\scripts\generate-clean-coverage.ps1

Write-Host "🔬 Gerando Code Coverage (EXCLUINDO código gerado)" -ForegroundColor Cyan
Write-Host ""

# Limpar coverage anterior
Write-Host "1. Limpando diretório coverage..." -ForegroundColor Yellow
if (Test-Path coverage) {
    Remove-Item coverage -Recurse -Force
}

# Rodar testes com exclusões configuradas
Write-Host "2. Rodando testes com Coverlet (pode demorar ~25 minutos)..." -ForegroundColor Yellow
Write-Host "   Excluindo padrões: **/*OpenApi*.generated.cs, **/*System.Runtime.CompilerServices*.cs, **/*RegexGenerator.g.cs" -ForegroundColor Gray
Write-Host ""

dotnet test `
    --configuration Debug `
    --collect:"XPlat Code Coverage" `
    --results-directory:"coverage" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/*OpenApi*.generated.cs,**/*System.Runtime.CompilerServices*.cs,**/*RegexGenerator.g.cs"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao rodar testes!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Testes concluídos com sucesso!" -ForegroundColor Green
Write-Host ""

# Gerar relatório consolidado
Write-Host "3. Gerando relatório HTML consolidado..." -ForegroundColor Yellow

reportgenerator `
    -reports:"coverage/**/coverage.cobertura.xml" `
    -targetdir:"coverage/report" `
    -reporttypes:"Html;TextSummary;JsonSummary" `
    -assemblyfilters:"+*;-*.Tests;-*.Tests.*;-*Test*;-testhost;-MeAjudaAi.AppHost;-MeAjudaAi.ServiceDefaults" `
    -classfilters:"-*Migrations*;-*Migration;-*MigrationBuilder*;-*DbContextModelSnapshot*;-*OpenApi.Generated*;-*.Keycloak.*;-*.Monitoring.*;-*NoOp*;-*RabbitMq*;-*ServiceBus*;-*Hangfire*;-*.Jobs.*;-*Options;-*BaseDesignTimeDbContextFactory*;-*SchemaPermissionsManager;-*SimpleHostEnvironment;-*CacheWarmupService;-*GeoPointConverter;-*ModuleNames;-*ModuleApiInfo;-*MessagingExtensions;-*ICacheableQuery"

Write-Host ""
Write-Host "✅ Relatório gerado em coverage/report/index.html" -ForegroundColor Green
Write-Host ""

# Exibir sumário
Write-Host "📊 Resumo de Coverage (SEM código gerado):" -ForegroundColor Cyan
Get-Content coverage/report/Summary.txt | Select-Object -First 20

Write-Host ""
Write-Host "🌐 Abrindo relatório no navegador..." -ForegroundColor Yellow
Start-Process (Resolve-Path coverage/report/index.html).Path

Write-Host ""
Write-Host "✅ CONCLUÍDO!" -ForegroundColor Green
Write-Host ""
Write-Host "O relatório agora exclui código gerado pelo compilador (OpenApi, CompilerServices, RegexGenerator)." -ForegroundColor Yellow
Write-Host "Compare com relatórios anteriores para ver a cobertura real do código manual." -ForegroundColor Green
