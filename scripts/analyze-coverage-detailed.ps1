#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Análise detalhada de code coverage por camada e módulo

.DESCRIPTION
    Gera análise detalhada mostrando classes com baixo coverage
    e identifica alvos de alta prioridade para melhorias

.EXAMPLE
    .\analyze-coverage-detailed.ps1
#>

param(
    [int]$TopN = 20,
    [double]$LowCoverageThreshold = 30.0
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Análise Detalhada de Code Coverage" -ForegroundColor Cyan
Write-Host ""

# Verificar se existe relatório
if (-not (Test-Path "CoverageReport\Summary.json")) {
    Write-Host "❌ Relatório de coverage não encontrado!" -ForegroundColor Red
    Write-Host "Execute: dotnet test --collect:'XPlat Code Coverage'" -ForegroundColor Yellow
    exit 1
}

# Ler summary
$summary = Get-Content "CoverageReport\Summary.json" | ConvertFrom-Json

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📊 COVERAGE GERAL" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Linhas:  $($summary.summary.linecoverage)% ($($summary.summary.coveredlines)/$($summary.summary.coverablelines))" -ForegroundColor White
Write-Host "  Branches: $($summary.summary.branchcoverage)% ($($summary.summary.coveredbranches)/$($summary.summary.totalbranches))" -ForegroundColor White
Write-Host "  Métodos:  $($summary.summary.methodcoverage)% ($($summary.summary.coveredmethods)/$($summary.summary.totalmethods))" -ForegroundColor White
Write-Host ""

# Análise por camada
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🏗️  COVERAGE POR CAMADA" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

$layers = @{
    "Domain" = @()
    "Application" = @()
    "Infrastructure" = @()
    "API" = @()
    "Tests" = @()
    "Other" = @()
}

foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -match "Generated|CompilerServices") { continue }
    
    $layer = "Other"
    if ($assembly.name -match "\.Domain$") { $layer = "Domain" }
    elseif ($assembly.name -match "\.Application$") { $layer = "Application" }
    elseif ($assembly.name -match "\.Infrastructure$") { $layer = "Infrastructure" }
    elseif ($assembly.name -match "\.API$") { $layer = "API" }
    elseif ($assembly.name -match "Tests") { $layer = "Tests" }
    
    $layers[$layer] += $assembly
}

foreach ($layerName in @("Domain", "Application", "Infrastructure", "API", "Other")) {
    $layerAssemblies = $layers[$layerName]
    if ($layerAssemblies.Count -eq 0) { continue }
    
    $totalLines = ($layerAssemblies | Measure-Object -Property coverablelines -Sum).Sum
    $coveredLines = ($layerAssemblies | Measure-Object -Property coveredlines -Sum).Sum
    $avgCoverage = if ($totalLines -gt 0) { [Math]::Round(($coveredLines / $totalLines) * 100, 1) } else { 0 }
    
    $color = if ($avgCoverage -ge 70) { "Green" }
             elseif ($avgCoverage -ge 50) { "Yellow" }
             elseif ($avgCoverage -ge 30) { "DarkYellow" }
             else { "Red" }
    
    Write-Host "  $($layerName.PadRight(15)) " -NoNewline -ForegroundColor Gray
    Write-Host "$avgCoverage% " -NoNewline -ForegroundColor $color
    Write-Host "($coveredLines/$totalLines linhas, $($layerAssemblies.Count) assemblies)" -ForegroundColor DarkGray
}

Write-Host ""

# Top N classes com BAIXO coverage
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🎯 TOP $TopN CLASSES COM BAIXO COVERAGE (<$LowCoverageThreshold%)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

$lowCoverageClasses = @()

foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -match "Generated|CompilerServices|Tests") { continue }
    
    foreach ($class in $assembly.classesinassembly) {
        if ($class.coverage -lt $LowCoverageThreshold -and $class.coverablelines -gt 20) {
            $lowCoverageClasses += [PSCustomObject]@{
                Assembly = $assembly.name -replace "MeAjudaAi\.", ""
                Class = $class.name -replace "MeAjudaAi\.", ""
                Coverage = $class.coverage
                Lines = $class.coverablelines
                UncoveredLines = $class.coverablelines - $class.coveredlines
                Impact = if ($summary.summary.coverablelines -eq 0) { 0 } else { ($class.coverablelines - $class.coveredlines) / $summary.summary.coverablelines * 100 }
            }
        }
    }
}

$topLowCoverage = $lowCoverageClasses | 
    Sort-Object -Property UncoveredLines -Descending | 
    Select-Object -First $TopN

$count = 1
foreach ($item in $topLowCoverage) {
    $className = $item.Class
    if ($className.Length -gt 55) {
        $className = $className.Substring(0, 52) + "..."
    }
    
    $color = if ($item.Coverage -eq 0) { "Red" }
             elseif ($item.Coverage -lt 10) { "DarkRed" }
             elseif ($item.Coverage -lt 20) { "DarkYellow" }
             else { "Yellow" }
    
    Write-Host "  $($count.ToString().PadLeft(2)). " -NoNewline -ForegroundColor Gray
    Write-Host "$className" -ForegroundColor White
    Write-Host "      Coverage: " -NoNewline -ForegroundColor DarkGray
    Write-Host "$($item.Coverage)% " -NoNewline -ForegroundColor $color
    Write-Host "| Linhas: $($item.Lines) | Não cobertas: $($item.UncoveredLines) " -NoNewline -ForegroundColor DarkGray
    Write-Host "(+$([Math]::Round($item.Impact, 2))pp)" -ForegroundColor Magenta
    Write-Host "      Módulo: $($item.Assembly)" -ForegroundColor DarkGray
    Write-Host ""
    
    $count++
}

# Análise por módulo
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📦 COVERAGE POR MÓDULO" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

$modules = @{}

foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -match "Generated|CompilerServices|Tests|ApiService|AppHost|ServiceDefaults|Shared$") { continue }
    
    # Extrair nome do módulo
    if ($assembly.name -match "Modules\.(\w+)\.") {
        $moduleName = $Matches[1]
        
        if (-not $modules.ContainsKey($moduleName)) {
            $modules[$moduleName] = @{
                Assemblies = @()
                TotalLines = 0
                CoveredLines = 0
            }
        }
        
        $modules[$moduleName].Assemblies += $assembly
        $modules[$moduleName].TotalLines += $assembly.coverablelines
        $modules[$moduleName].CoveredLines += $assembly.coveredlines
    }
}

$moduleStats = $modules.GetEnumerator() | ForEach-Object {
    $avgCoverage = if ($_.Value.TotalLines -gt 0) {
        [Math]::Round(($_.Value.CoveredLines / $_.Value.TotalLines) * 100, 1)
    } else { 0 }
    
    [PSCustomObject]@{
        Module = $_.Key
        Coverage = $avgCoverage
        Lines = $_.Value.TotalLines
        Uncovered = $_.Value.TotalLines - $_.Value.CoveredLines
        AssemblyCount = $_.Value.Assemblies.Count
    }
} | Sort-Object -Property Coverage

foreach ($stat in $moduleStats) {
    $color = if ($stat.Coverage -ge 70) { "Green" }
             elseif ($stat.Coverage -ge 50) { "Yellow" }
             elseif ($stat.Coverage -ge 30) { "DarkYellow" }
             else { "Red" }
    
    $modulePadded = $stat.Module.PadRight(20)
    $coveragePadded = "$($stat.Coverage)%".PadLeft(6)
    
    Write-Host "  $modulePadded " -NoNewline -ForegroundColor Gray
    Write-Host "$coveragePadded " -NoNewline -ForegroundColor $color
    Write-Host "($($stat.Lines) linhas, +$($stat.Uncovered) não cobertas)" -ForegroundColor DarkGray
}

Write-Host ""

# Recomendações
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "💡 RECOMENDAÇÕES PRIORITÁRIAS" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Identificar handlers sem coverage
$uncoveredHandlers = @()
foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -notmatch "Application" -or $assembly.name -match "Tests") { continue }
    
    foreach ($class in $assembly.classesinassembly) {
        if ($class.name -match "Handler$" -and $class.coverage -eq 0) {
            $uncoveredHandlers += [PSCustomObject]@{
                Module = ($assembly.name -replace "MeAjudaAi\.Modules\.", "" -replace "\.Application", "")
                Handler = ($class.name -replace "MeAjudaAi\..+\.", "")
                Lines = $class.coverablelines
            }
        }
    }
}

if ($uncoveredHandlers.Count -gt 0) {
    Write-Host "  🔴 $($uncoveredHandlers.Count) HANDLERS SEM COVERAGE:" -ForegroundColor Red
    Write-Host ""
    foreach ($handler in $uncoveredHandlers | Sort-Object -Property Lines -Descending | Select-Object -First 5) {
        Write-Host "     • $($handler.Module): " -NoNewline -ForegroundColor Yellow
        Write-Host "$($handler.Handler) " -NoNewline -ForegroundColor White
        Write-Host "($($handler.Lines) linhas)" -ForegroundColor DarkGray
    }
    Write-Host ""
}

# Identificar repositories sem coverage
$uncoveredRepos = @()
foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -notmatch "Infrastructure" -or $assembly.name -match "Tests") { continue }
    
    foreach ($class in $assembly.classesinassembly) {
        if ($class.name -match "Repository$" -and $class.coverage -eq 0) {
            $uncoveredRepos += [PSCustomObject]@{
                Module = ($assembly.name -replace "MeAjudaAi\.Modules\.", "" -replace "\.Infrastructure", "")
                Repository = ($class.name -replace "MeAjudaAi\..+\.", "")
                Lines = $class.coverablelines
            }
        }
    }
}

if ($uncoveredRepos.Count -gt 0) {
    Write-Host "  🔴 $($uncoveredRepos.Count) REPOSITORIES SEM COVERAGE:" -ForegroundColor Red
    Write-Host ""
    foreach ($repo in $uncoveredRepos | Sort-Object -Property Lines -Descending) {
        Write-Host "     • $($repo.Module): " -NoNewline -ForegroundColor Yellow
        Write-Host "$($repo.Repository) " -NoNewline -ForegroundColor White
        Write-Host "($($repo.Lines) linhas)" -ForegroundColor DarkGray
    }
    Write-Host ""
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "📖 Detalhes completos: " -NoNewline -ForegroundColor White
Write-Host "CoverageReport\index.html" -ForegroundColor Cyan
Write-Host "📋 Plano de ação: " -NoNewline -ForegroundColor White
Write-Host "docs\testing\coverage-improvement-plan.md" -ForegroundColor Cyan
Write-Host ""
