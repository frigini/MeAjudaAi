# Script para executar testes com cobertura igual à pipeline
# Gera relatórios no formato OpenCover e aplica os mesmos filtros

Write-Host "🧪 Executando testes com cobertura (formato pipeline)..." -ForegroundColor Cyan

# Limpar cobertura anterior
if (Test-Path "coverage") {
    Remove-Item "coverage" -Recurse -Force
}

# Filtros iguais à pipeline
$EXCLUDE_BY_FILE = "**/*OpenApi*.generated.cs,**/System.Runtime.CompilerServices*.cs,**/*RegexGenerator.g.cs"
$EXCLUDE_BY_ATTRIBUTE = "Obsolete,GeneratedCode,CompilerGenerated"
$EXCLUDE_FILTER = "[*.Tests]*,[*.Tests.*]*,[*Test*]*,[testhost]*"
$INCLUDE_FILTER = "[MeAjudaAi*]*"

# Criar runsettings temporário
$runsettings = @"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>opencover</Format>
          <ExcludeByFile>$EXCLUDE_BY_FILE</ExcludeByFile>
          <ExcludeByAttribute>$EXCLUDE_BY_ATTRIBUTE</ExcludeByAttribute>
          <Exclude>$EXCLUDE_FILTER</Exclude>
          <Include>$INCLUDE_FILTER</Include>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
"@

$runsettingsFile = "coverage.runsettings"
$runsettings | Out-File -FilePath $runsettingsFile -Encoding UTF8

Write-Host "`n📦 Executando testes com cobertura OpenCover..." -ForegroundColor Yellow

dotnet test --configuration Release `
  --collect:"XPlat Code Coverage" `
  --results-directory ./coverage `
  --settings $runsettingsFile `
  --verbosity minimal

# Verificar arquivos gerados
Write-Host "`n📊 Arquivos de cobertura gerados:" -ForegroundColor Cyan
Get-ChildItem -Path "coverage" -Filter "*.xml" -Recurse | ForEach-Object {
    Write-Host "  ✅ $($_.FullName)" -ForegroundColor Green
}

# Gerar relatório agregado com filtros da pipeline
Write-Host "`n🔗 Gerando relatório agregado com filtros da pipeline..." -ForegroundColor Cyan

# Instalar/atualizar ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool 2>$null
dotnet tool update --global dotnet-reportgenerator-globaltool 2>$null

# Filtros da pipeline
$INCLUDE_ASSEMBLY = "+MeAjudaAi.Modules.Users.*;+MeAjudaAi.Modules.Providers.*;+MeAjudaAi.Modules.Documents.*;+MeAjudaAi.Modules.ServiceCatalogs.*;+MeAjudaAi.Modules.Locations.*;+MeAjudaAi.Modules.SearchProviders.*;+MeAjudaAi.Shared*;+MeAjudaAi.ApiService*"
$EXCLUDE_CLASS = "-*.Tests;-*.Tests.*;-*Test*;-testhost;-xunit*;-*.Migrations.*;-*.Contracts;-*.Database;-*.Keycloak.*;-*.Monitoring.*;-*NoOp*;-*RabbitMq*;-*ServiceBus*;-*Hangfire*;-*.Jobs.*;-*Options;-*BaseDesignTimeDbContextFactory*;-*SchemaPermissionsManager;-*SimpleHostEnvironment;-*CacheWarmupService;-*GeoPointConverter;-*ModuleNames;-*ModuleApiInfo;-*MessagingExtensions;-*ICacheableQuery"

reportgenerator `
  -reports:"coverage/**/*.xml" `
  -targetdir:"coverage/report-pipeline" `
  -reporttypes:"TextSummary;HtmlSummary;Cobertura" `
  -assemblyfilters:"$INCLUDE_ASSEMBLY" `
  -classfilters:"$EXCLUDE_CLASS" `
  -filefilters:"-*Migrations*"

if (Test-Path "coverage/report-pipeline/Summary.txt") {
    Write-Host "`n📈 COBERTURA COM FILTROS DA PIPELINE:" -ForegroundColor Green
    Write-Host "======================================`n" -ForegroundColor Green
    Get-Content "coverage/report-pipeline/Summary.txt" | Select-Object -First 20
    
    # Extrair porcentagem de linha
    $summaryContent = Get-Content "coverage/report-pipeline/Summary.txt"
    $lineCoverage = ($summaryContent | Select-String "Line coverage:").ToString() -replace '.*Line coverage:\s*', ''
    Write-Host "`n🎯 Line Coverage (igual pipeline): $lineCoverage" -ForegroundColor Cyan
    Write-Host "🎯 Meta: 90%" -ForegroundColor Yellow
    
    # Abrir relatório HTML (cross-platform)
    $htmlReport = "coverage/report-pipeline/summary.html"
    if (Test-Path $htmlReport) {
        Write-Host "`n🌐 Abrindo relatório HTML..." -ForegroundColor Cyan
        $fullPath = Resolve-Path $htmlReport
        
        try {
            if ($IsWindows -or (-not (Test-Path variable:IsWindows))) {
                Start-Process $fullPath
            }
            elseif ($IsMacOS) {
                & open $fullPath
            }
            elseif ($IsLinux) {
                if (Get-Command xdg-open -ErrorAction SilentlyContinue) {
                    & xdg-open $fullPath
                }
                elseif (Get-Command sensible-browser -ErrorAction SilentlyContinue) {
                    & sensible-browser $fullPath
                }
                else {
                    Write-Host "⚠️ Não foi possível abrir automaticamente. Relatório em: $fullPath" -ForegroundColor Yellow
                }
            }
        }
        catch {
            Write-Host "⚠️ Erro ao abrir relatório: $_" -ForegroundColor Yellow
            Write-Host "📄 Relatório disponível em: $fullPath" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "`n❌ Falha ao gerar relatório agregado" -ForegroundColor Red
}

# Limpar runsettings
Remove-Item $runsettingsFile -ErrorAction SilentlyContinue

Write-Host "`n✅ Análise completa!" -ForegroundColor Green
