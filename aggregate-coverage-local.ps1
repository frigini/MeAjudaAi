# Script para rodar TODOS os testes (Unit + Integration + E2E) e agregar coverage
# Simula exatamente o que o GitHub Actions faz

Write-Host "📊 Running ALL tests with coverage (Unit + Integration + E2E)..." -ForegroundColor Cyan
Write-Host "⚠️  NOTE: Integration/E2E tests require Docker containers running!" -ForegroundColor Yellow
Write-Host ""

# Limpar coverage anterior
Remove-Item -Recurse -Force coverage -ErrorAction SilentlyContinue

# Criar runsettings com mesmos filtros do GitHub
$runsettingsPath = "coverage.runsettings"
$excludeByFile = "**/*OpenApi*.generated.cs,**/System.Runtime.CompilerServices*.cs,**/*RegexGenerator.g.cs"
$excludeFilter = "[*.Tests]*,[*.Tests.*]*,[*Test*]*,[testhost]*,[xunit*]*"
$excludeByAttribute = "Obsolete,GeneratedCode,CompilerGenerated"

@"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>opencover</Format>
          <Exclude>$excludeFilter</Exclude>
          <ExcludeByFile>$excludeByFile</ExcludeByFile>
          <ExcludeByAttribute>$excludeByAttribute</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
"@ | Out-File -FilePath $runsettingsPath -Encoding utf8

Write-Host "✅ Created runsettings with GitHub filters" -ForegroundColor Green
Write-Host "   - Excludes: .Tests assemblies, compiler-generated code" -ForegroundColor Gray
Write-Host ""

# Rodar TODOS os testes (Unit + Integration + E2E) com coverage
Write-Host "🧪 Running ALL tests..." -ForegroundColor Cyan
dotnet test MeAjudaAi.sln `
    --collect:"XPlat Code Coverage" `
    --results-directory ./coverage `
    --settings $runsettingsPath `
    --configuration Debug `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Some tests failed, but continuing with coverage aggregation..." -ForegroundColor Yellow
}

Write-Host ""

# Instalar ReportGenerator se necessário
$reportGen = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $reportGen) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Encontrar todos os arquivos de coverage
Write-Host "`n🔍 Finding coverage files..." -ForegroundColor Cyan
$coverageFiles = Get-ChildItem -Path "coverage" -Recurse -Filter "*.xml" | Where-Object { $_.Name -like "*coverage*" -or $_.Name -like "*.cobertura.xml" -or $_.Name -like "*.opencover.xml" }

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage files:" -ForegroundColor Green
$coverageFiles | ForEach-Object { Write-Host "  ✅ $($_.FullName)" -ForegroundColor Gray }

# Gerar relatório agregado (mesmos filtros do GitHub)
Write-Host "`n🔗 Generating aggregated report..." -ForegroundColor Cyan

$includeFilter = "+MeAjudaAi.Modules.Users.*;+MeAjudaAi.Modules.Providers.*;+MeAjudaAi.Modules.Documents.*;+MeAjudaAi.Modules.ServiceCatalogs.*;+MeAjudaAi.Modules.Locations.*;+MeAjudaAi.Modules.SearchProviders.*;+MeAjudaAi.Shared*;+MeAjudaAi.ApiService*"
$excludeFilter = "-[*.Tests]*;-[*.Tests.*]*;-[*Test*]*;-[testhost]*;-[xunit*]*;-[*]*.Migrations.*;-[*]*.Contracts;-[*]*.Database"

# Criar lista de reports
$reports = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

# Criar diretório de saída
New-Item -ItemType Directory -Force -Path "coverage\aggregate" | Out-Null

# Executar ReportGenerator
reportgenerator `
    "-reports:$reports" `
    "-targetdir:coverage\aggregate" `
    "-reporttypes:Cobertura;HtmlInline_AzurePipelines" `
    "-assemblyfilters:$includeFilter" `
    "-classfilters:$excludeFilter"

if (Test-Path "coverage\aggregate\Cobertura.xml") {
    Write-Host "`n✅ Aggregated coverage report generated!" -ForegroundColor Green
    Write-Host "   Location: coverage\aggregate\Cobertura.xml" -ForegroundColor Gray
    
    # Extrair porcentagem
    $xml = [xml](Get-Content "coverage\aggregate\Cobertura.xml")
    $lineRate = [double]$xml.coverage.'line-rate'
    $percentage = [math]::Round($lineRate * 100, 2)
    
    Write-Host "`n📈 Combined Line Coverage: $percentage%" -ForegroundColor Cyan
    
    # Abrir HTML
    if (Test-Path "coverage\aggregate\index.html") {
        Write-Host "`n🌐 Opening HTML report..." -ForegroundColor Cyan
        Start-Process "coverage\aggregate\index.html"
    }
} else {
    Write-Host "`n❌ Failed to generate aggregated coverage report!" -ForegroundColor Red
}
