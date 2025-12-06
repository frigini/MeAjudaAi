#!/usr/bin/env pwsh
# Parse coverage report HTML and summarize per-class coverage with detailed analysis

$htmlPath = Join-Path $PSScriptRoot 'coverage-github-report' 'index.html'
if (-not (Test-Path $htmlPath)) {
    Write-Error "Coverage report not found at $htmlPath"
    exit 1
}

$html = Get-Content $htmlPath -Raw

# Extract coverage data using regex
$pattern = '<tr[^>]*>\s*<td[^>]*>\s*<a[^>]*>([^<]+)</a>.*?<td[^>]*class="right"[^>]*>(\d+\.?\d*)%</td>'
$matches = [regex]::Matches($html, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

$coverageData = @()
foreach ($match in $matches) {
    $className = $match.Groups[1].Value.Trim()
    try {
        $coverage = [decimal]$match.Groups[2].Value
    } catch {
        $matchValue = $match.Groups[2].Value
        Write-Warning "Failed to parse coverage for $className`: $matchValue"
        continue
    }
    
    # Skip summary rows (Total) and non-MeAjudaAi classes
    if ($className -eq 'Total' -or $className -notmatch '^MeAjudaAi\.') {
        continue
    }
    
    $coverageData += [PSCustomObject]@{
        Class = $className
        Coverage = $coverage
    }
}

if ($coverageData.Count -eq 0) {
    Write-Warning "No coverage data extracted from $htmlPath"
    exit 1
}

# Extract total line coverage from summary card
$lineCoveragePattern = '<th>Line coverage:</th>\s*<td[^>]*>(\d+\.?\d*)%</td>'
$lineCoverageMatch = [regex]::Match($html, $lineCoveragePattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
if ($lineCoverageMatch.Success) {
    $totalCoverage = [decimal]$lineCoverageMatch.Groups[1].Value
    $totalCoverageStr = $totalCoverage.ToString("F2")
} else {
    Write-Warning "Could not find line coverage in HTML report"
    # Fallback: try to extract from large card display
    $cardPattern = '<div[^>]*class="large[^"]*">(\d+)%</div>'
    $cardMatch = [regex]::Match($html, $cardPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if ($cardMatch.Success) {
        $totalCoverage = [decimal]$cardMatch.Groups[1].Value
        $totalCoverageStr = $totalCoverage.ToString("F2")
    } else {
        Write-Error "Unable to extract total coverage from HTML report"
        exit 1
    }
}

# Sort by coverage and show results
Write-Host "`n📊 COVERAGE ANALYSIS ($totalCoverageStr% total)" -ForegroundColor Cyan
Write-Host "=" * 80

Write-Host "`n🔴 LOWEST COVERAGE CLASSES (<70%):" -ForegroundColor Red
$coverageData | Where-Object { $_.Coverage -lt 70 } | Sort-Object Coverage | Format-Table -AutoSize

Write-Host "`n🟡 MEDIUM COVERAGE CLASSES (70-85%):" -ForegroundColor Yellow
$coverageData | Where-Object { $_.Coverage -ge 70 -and $_.Coverage -lt 85 } | Sort-Object Coverage | Format-Table -AutoSize

Write-Host "`n🟢 GOOD COVERAGE CLASSES (85-90%):" -ForegroundColor Green
$coverageData | Where-Object { $_.Coverage -ge 85 -and $_.Coverage -lt 90 } | Sort-Object Coverage | Format-Table -AutoSize

$highCoverageClasses = $coverageData | Where-Object { $_.Coverage -ge 90 } | Sort-Object Coverage -Descending
Write-Host "`n⭐ EXCELLENT COVERAGE CLASSES (90-100%) - Showing top 10 of $($highCoverageClasses.Count):" -ForegroundColor Cyan
$highCoverageClasses | Select-Object -First 10 | Format-Table -AutoSize

# Summary statistics
$low = ($coverageData | Where-Object { $_.Coverage -lt 70 }).Count
$medium = ($coverageData | Where-Object { $_.Coverage -ge 70 -and $_.Coverage -lt 85 }).Count
$high = ($coverageData | Where-Object { $_.Coverage -ge 85 }).Count

Write-Host "`n📈 SUMMARY:" -ForegroundColor Cyan
Write-Host "  Low coverage (<70%): $low classes" -ForegroundColor Red
Write-Host "  Medium coverage (70-85%): $medium classes" -ForegroundColor Yellow
Write-Host "  High coverage (85%+): $high classes" -ForegroundColor Green
Write-Host "  Total classes analyzed: $($coverageData.Count)"
