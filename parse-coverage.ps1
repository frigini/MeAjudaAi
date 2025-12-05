#!/usr/bin/env pwsh
# Parse coverage report HTML and extract low coverage classes

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
        Write-Warning "Failed to parse coverage for $className: $($match.Groups[2].Value)"
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

# Extract total coverage from summary row
$totalPattern = '<tr[^>]*>\s*<td[^>]*>\s*<strong>Total</strong>.*?<td[^>]*class="right"[^>]*><strong>(\d+\.?\d*)%</strong></td>'
$totalMatch = [regex]::Match($html, $totalPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
if ($totalMatch.Success) {
    $totalCoverage = [decimal]$totalMatch.Groups[1].Value
    $totalCoverageStr = $totalCoverage.ToString("F2")
} else {
    # Fallback: calculate simple average from class data
    if ($coverageData.Count -gt 0) {
        $avgCoverage = ($coverageData | Measure-Object -Property Coverage -Average).Average
        $totalCoverageStr = $avgCoverage.ToString("F2")
        Write-Verbose "Total coverage not found in HTML; calculated average: $totalCoverageStr%"
    } else {
        $totalCoverageStr = "N/A"
        Write-Warning "Could not determine total coverage"
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
$low = ($coverageData | Where-Object { $_.Coverage -lt 60 }).Count
$medium = ($coverageData | Where-Object { $_.Coverage -ge 60 -and $_.Coverage -lt 80 }).Count
$high = ($coverageData | Where-Object { $_.Coverage -ge 80 }).Count

Write-Host "`n📈 SUMMARY:" -ForegroundColor Cyan
Write-Host "  Low coverage (<60%): $low classes" -ForegroundColor Red
Write-Host "  Medium coverage (60-80%): $medium classes" -ForegroundColor Yellow
Write-Host "  High coverage (80%+): $high classes" -ForegroundColor Green
Write-Host "  Total classes analyzed: $($coverageData.Count)"
