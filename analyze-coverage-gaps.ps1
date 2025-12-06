#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Analisa gaps de coverage baixando o relatório HTML do GitHub Actions

.DESCRIPTION
    Este script baixa o relatório de coverage do GitHub Actions Artifacts
    e abre localmente para análise detalhada de gaps por módulo/classe/método
#>

param(
    [string]$RunId = ""
)

Write-Host "🔍 Analisando gaps de coverage..." -ForegroundColor Cyan

# Se não forneceu RunId, pega o último run bem-sucedido
if ([string]::IsNullOrEmpty($RunId)) {
    Write-Host "📋 Buscando último run bem-sucedido..." -ForegroundColor Yellow
    
    $runs = gh run list --branch improve-tests-coverage-2 --limit 5 --json databaseId,conclusion,displayTitle,createdAt | ConvertFrom-Json
    $successfulRun = $runs | Where-Object { $_.conclusion -eq "success" } | Select-Object -First 1
    
    if ($null -eq $successfulRun) {
        Write-Host "❌ Nenhum run bem-sucedido encontrado" -ForegroundColor Red
        exit 1
    }
    
    $RunId = $successfulRun.databaseId
    Write-Host "✅ Usando run: $($successfulRun.displayTitle) (ID: $RunId)" -ForegroundColor Green
}

# Lista artifacts disponíveis
Write-Host "`n📦 Artifacts disponíveis:" -ForegroundColor Cyan
$artifacts = gh api repos/frigini/MeAjudaAi/actions/runs/$RunId/artifacts | ConvertFrom-Json | Select-Object -ExpandProperty artifacts
$artifacts | ForEach-Object { Write-Host "  - $($_.name) ($([math]::Round($_.size_in_bytes/1MB, 2)) MB)" -ForegroundColor Gray }

# Baixa o relatório de coverage agregado
$artifactName = "coverage-reports"
Write-Host "`n⬇️  Baixando artifact '$artifactName'..." -ForegroundColor Yellow

# Remove diretório antigo se existir
if (Test-Path "coverage-github") {
    Remove-Item "coverage-github" -Recurse -Force
}

# Baixa o artifact
gh run download $RunId --name $artifactName --dir coverage-github

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao baixar artifact" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Artifact baixado com sucesso!" -ForegroundColor Green

# Gera relatório HTML a partir dos XMLs do GitHub
Write-Host "`n📊 Gerando relatório HTML..." -ForegroundColor Cyan

$reportGenerator = dotnet tool list -g | Select-String "reportgenerator"
if ($null -eq $reportGenerator) {
    Write-Host "⚠️  Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Usa o Cobertura.xml agregado do GitHub que JÁ inclui Unit + Integration + E2E
$coberturaFile = Join-Path "coverage-github" "aggregate\Cobertura.xml"

if (Test-Path $coberturaFile) {
    Write-Host "   ✅ Usando Cobertura.xml agregado (Unit + Integration + E2E)" -ForegroundColor Green
    $coverageFiles = @($coberturaFile)
} else {
    # Fallback: Coleta todos os XMLs
    Write-Host "   ⚠️  Cobertura.xml não encontrado, coletando XMLs individuais..." -ForegroundColor Yellow
    $coverageFiles = Get-ChildItem -Path "coverage-github" -Recurse -Filter "*.xml" | Where-Object { $_.Name -match "coverage|Cobertura" } | Select-Object -ExpandProperty FullName
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ Nenhum arquivo de coverage encontrado" -ForegroundColor Red
    exit 1
}

Write-Host "   Encontrados $($coverageFiles.Count) arquivo(s) de coverage" -ForegroundColor Gray

# Gera relatório agregado
$outputPath = "coverage-github-report"
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

$reportsParam = ($coverageFiles -join ";")
reportgenerator `
    "-reports:$reportsParam" `
    "-targetdir:$outputPath" `
    "-reporttypes:Html" `
    "-assemblyfilters:+MeAjudaAi.Modules.*;+MeAjudaAi.ApiService;+MeAjudaAi.Shared" `
    "-classfilters:-[*.Tests]*;-[*.Tests.*]*;-[*Test*]*;-[testhost]*;-[xunit*]*" | Out-Null

$indexPath = Join-Path $outputPath "index.html"

if (-not (Test-Path $indexPath)) {
    Write-Host "❌ Falha ao gerar relatório HTML" -ForegroundColor Red
    exit 1
}

Write-Host "`n📊 Abrindo relatório de coverage do GitHub..." -ForegroundColor Cyan
Write-Host "   Path: $indexPath" -ForegroundColor Gray

# Abre o relatório no navegador
Start-Process $indexPath

Write-Host "`n✅ PRONTO! Agora você pode:" -ForegroundColor Green
Write-Host "   1. Ver coverage exato por módulo (Users, Providers, Documents, etc.)" -ForegroundColor White
Write-Host "   2. Ver classes com baixa coverage (clique em cada módulo)" -ForegroundColor White
Write-Host "   3. Ver linhas NÃO cobertas (vermelho) e cobertas (verde)" -ForegroundColor White
Write-Host "   4. Identificar métodos/branches sem testes" -ForegroundColor White
Write-Host "`n💡 Dica: Foque em classes com <60% de coverage primeiro!" -ForegroundColor Yellow
