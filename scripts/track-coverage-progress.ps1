#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Track code coverage progress toward 70% target

.DESCRIPTION
    Runs tests with coverage, generates report, and shows progress metrics

.EXAMPLE
    .\track-coverage-progress.ps1
    .\track-coverage-progress.ps1 -SkipTests (use existing coverage data)
#>

param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

Write-Host "🎯 Coverage Progress Tracker" -ForegroundColor Cyan
Write-Host "Target: 70% | Current: ?" -ForegroundColor Gray
Write-Host ""

# Define target
$TARGET_COVERAGE = 70.0

if (-not $SkipTests) {
    Write-Host "▶️  Running tests with coverage collection..." -ForegroundColor Yellow
    
    # Clean previous results
    if (Test-Path "TestResults") {
        Remove-Item -Recurse -Force "TestResults"
    }
    
    # Run tests with coverage
    dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Some tests failed, but continuing with coverage analysis..." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "📊 Generating coverage report..." -ForegroundColor Yellow

# Generate report
reportgenerator `
    -reports:"TestResults/**/coverage.opencover.xml" `
    -targetdir:"CoverageReport" `
    -reporttypes:"Html;JsonSummary;Cobertura" | Out-Null

# Read summary
$summary = Get-Content "CoverageReport\Summary.json" | ConvertFrom-Json

# Extract metrics
$lineCoverage = $summary.summary.linecoverage
$branchCoverage = $summary.summary.branchcoverage
$methodCoverage = $summary.summary.methodcoverage
$coveredLines = $summary.summary.coveredlines
$coverableLines = $summary.summary.coverablelines
$uncoveredLines = $summary.summary.uncoveredlines

# Calculate progress
$progressPercentage = ($lineCoverage / $TARGET_COVERAGE) * 100
$remainingLines = [Math]::Ceiling($coverableLines * ($TARGET_COVERAGE / 100) - $coveredLines)
$remainingPercentage = $TARGET_COVERAGE - $lineCoverage

# Display results
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📈 COVERAGE SUMMARY" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

Write-Host "  Line Coverage:   " -NoNewline -ForegroundColor White
if ($lineCoverage -ge $TARGET_COVERAGE) {
    Write-Host "$lineCoverage% ✅" -ForegroundColor Green
} elseif ($lineCoverage -ge 50) {
    Write-Host "$lineCoverage% 🟡" -ForegroundColor Yellow
} else {
    Write-Host "$lineCoverage% 🔴" -ForegroundColor Red
}

Write-Host "  Branch Coverage: " -NoNewline -ForegroundColor White
Write-Host "$branchCoverage%" -ForegroundColor Gray

Write-Host "  Method Coverage: " -NoNewline -ForegroundColor White
Write-Host "$methodCoverage%" -ForegroundColor Gray

Write-Host ""
Write-Host "  Covered Lines:   " -NoNewline -ForegroundColor White
Write-Host "$coveredLines / $coverableLines" -ForegroundColor Gray

Write-Host "  Uncovered Lines: " -NoNewline -ForegroundColor White
Write-Host "$uncoveredLines" -ForegroundColor Red

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🎯 PROGRESS TO TARGET (70%)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Progress bar
$barLength = 40
$filledLength = [Math]::Floor($barLength * ($lineCoverage / $TARGET_COVERAGE))
$emptyLength = $barLength - $filledLength
$progressBar = "█" * $filledLength + "░" * $emptyLength

Write-Host "  [$progressBar] " -NoNewline
Write-Host "$([Math]::Round($progressPercentage, 1))%" -ForegroundColor Cyan

Write-Host ""
Write-Host "  Current:   " -NoNewline -ForegroundColor White
Write-Host "$lineCoverage%" -ForegroundColor $(if ($lineCoverage -ge 50) { "Yellow" } else { "Red" })

Write-Host "  Target:    " -NoNewline -ForegroundColor White
Write-Host "$TARGET_COVERAGE%" -ForegroundColor Green

Write-Host "  Remaining: " -NoNewline -ForegroundColor White
Write-Host "+$([Math]::Round($remainingPercentage, 1))pp ($remainingLines lines)" -ForegroundColor Magenta

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📋 TOP 10 MODULES TO IMPROVE" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Get bottom 10 assemblies by coverage (excluding generated code)
$assemblies = $summary.coverage.assemblies | 
    Where-Object { $_.name -notmatch "Generated|CompilerServices" } |
    Sort-Object coverage |
    Select-Object -First 10

foreach ($assembly in $assemblies) {
    $name = $assembly.name -replace "MeAjudaAi\.", ""
    $coverage = $assembly.coverage
    $uncovered = $assembly.coverablelines - $assembly.coveredlines
    
    # Shorten name if too long
    if ($name.Length -gt 40) {
        $name = $name.Substring(0, 37) + "..."
    }
    
    # Color based on coverage
    $color = if ($coverage -ge 70) { "Green" }
             elseif ($coverage -ge 50) { "Yellow" }
             elseif ($coverage -ge 30) { "DarkYellow" }
             else { "Red" }
    
    # Format with padding
    $namePadded = $name.PadRight(45)
    $coveragePadded = "$coverage%".PadLeft(6)
    $uncoveredPadded = "+$uncovered lines".PadLeft(12)
    
    Write-Host "  $namePadded " -NoNewline -ForegroundColor Gray
    Write-Host "$coveragePadded " -NoNewline -ForegroundColor $color
    Write-Host "$uncoveredPadded" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "💡 QUICK WINS (High Impact, Low Effort)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Analyze classes with 0% coverage and <100 lines
$quickWins = @()
foreach ($assembly in $summary.coverage.assemblies) {
    if ($assembly.name -match "Generated|CompilerServices") { continue }
    
    foreach ($class in $assembly.classesinassembly) {
        if ($class.coverage -eq 0 -and $class.coverablelines -gt 10 -and $class.coverablelines -lt 150) {
            $quickWins += [PSCustomObject]@{
                Assembly = $assembly.name -replace "MeAjudaAi\.", ""
                Class = $class.name
                Lines = $class.coverablelines
                Impact = $class.coverablelines / $coverableLines * 100
            }
        }
    }
}

$topQuickWins = $quickWins | Sort-Object -Descending Lines | Select-Object -First 5

foreach ($win in $topQuickWins) {
    $className = $win.Class
    if ($className.Length -gt 50) {
        $className = $className.Substring(0, 47) + "..."
    }
    
    Write-Host "  • " -NoNewline -ForegroundColor Yellow
    Write-Host "$className" -NoNewline -ForegroundColor White
    Write-Host " ($($win.Lines) lines, +$([Math]::Round($win.Impact, 2))pp)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🚀 NEXT STEPS" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

if ($lineCoverage -lt 20) {
    Write-Host "  1. Focus on Infrastructure layer repositories" -ForegroundColor Yellow
    Write-Host "  2. Add basic CRUD tests for uncovered repos" -ForegroundColor Yellow
    Write-Host "  3. See: docs/testing/coverage-improvement-plan.md" -ForegroundColor Gray
} elseif ($lineCoverage -lt 40) {
    Write-Host "  1. Complete repository test coverage" -ForegroundColor Yellow
    Write-Host "  2. Add domain event handler tests" -ForegroundColor Yellow
    Write-Host "  3. Review 'Quick Wins' list above" -ForegroundColor Gray
} elseif ($lineCoverage -lt 60) {
    Write-Host "  1. Add application handler tests" -ForegroundColor Yellow
    Write-Host "  2. Improve domain layer coverage" -ForegroundColor Yellow
    Write-Host "  3. Start API E2E tests" -ForegroundColor Gray
} else {
    Write-Host "  1. Add edge case tests" -ForegroundColor Yellow
    Write-Host "  2. Complete E2E test coverage" -ForegroundColor Yellow
    Write-Host "  3. Final push to 70%!" -ForegroundColor Green
}

Write-Host ""
Write-Host "  📖 Full plan:    " -NoNewline -ForegroundColor White
Write-Host "docs/testing/coverage-improvement-plan.md" -ForegroundColor Cyan

Write-Host "  📊 HTML Report:  " -NoNewline -ForegroundColor White
Write-Host "CoverageReport/index.html" -ForegroundColor Cyan

Write-Host "  🔍 Gap Script:   " -NoNewline -ForegroundColor White
Write-Host "scripts/find-coverage-gaps.ps1" -ForegroundColor Cyan

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Exit with error if below target
if ($lineCoverage -lt $TARGET_COVERAGE) {
    Write-Host "⚠️  Coverage below target ($lineCoverage% < $TARGET_COVERAGE%)" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✅ Coverage target reached! ($lineCoverage% >= $TARGET_COVERAGE%)" -ForegroundColor Green
    exit 0
}
