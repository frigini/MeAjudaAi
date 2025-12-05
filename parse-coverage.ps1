#!/usr/bin/env pwsh
# Parse coverage report HTML and extract low coverage classes

$htmlPath = "coverage-github-report\index.html"
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
    $coverage = [decimal]$match.Groups[2].Value
    
    # Skip summary rows and focus on actual classes
    if ($className -notmatch '^(MeAjudaAi\.|Total)') {
        continue
    }
    
    $coverageData += [PSCustomObject]@{
        Class = $className
        Coverage = $coverage
    }
}

# Sort by coverage and show results
Write-Host "`n📊 COVERAGE ANALYSIS (78.43% total)" -ForegroundColor Cyan
Write-Host "=" * 80

Write-Host "`n🔴 LOWEST COVERAGE CLASSES (<60%):" -ForegroundColor Red
$coverageData | Where-Object { $_.Coverage -lt 60 } | Sort-Object Coverage | Format-Table -AutoSize

Write-Host "`n🟡 MEDIUM COVERAGE CLASSES (60-80%):" -ForegroundColor Yellow
$coverageData | Where-Object { $_.Coverage -ge 60 -and $_.Coverage -lt 80 } | Sort-Object Coverage | Format-Table -AutoSize

Write-Host "`n✅ HIGH COVERAGE CLASSES (80-100%):" -ForegroundColor Green
$coverageData | Where-Object { $_.Coverage -ge 80 } | Sort-Object Coverage | Select-Object -First 10 | Format-Table -AutoSize

# Summary statistics
$low = ($coverageData | Where-Object { $_.Coverage -lt 60 }).Count
$medium = ($coverageData | Where-Object { $_.Coverage -ge 60 -and $_.Coverage -lt 80 }).Count
$high = ($coverageData | Where-Object { $_.Coverage -ge 80 }).Count

Write-Host "`n📈 SUMMARY:" -ForegroundColor Cyan
Write-Host "  Low coverage (<60%): $low classes" -ForegroundColor Red
Write-Host "  Medium coverage (60-80%): $medium classes" -ForegroundColor Yellow
Write-Host "  High coverage (80%+): $high classes" -ForegroundColor Green
Write-Host "  Total classes analyzed: $($coverageData.Count)"
