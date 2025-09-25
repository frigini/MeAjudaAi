param([string]$OutputPath = "api-spec.json")
$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot
$OutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $ProjectRoot $OutputPath }
try {
    Write-Host "Validando especificacao OpenAPI..." -ForegroundColor Cyan
    if (Test-Path $OutputPath) {
        $Content = Get-Content $OutputPath | ConvertFrom-Json
        $PathCount = $Content.paths.PSObject.Properties.Count
        Write-Host "Total endpoints: $PathCount" -ForegroundColor Green
        $usersPaths = $Content.paths.PSObject.Properties | Where-Object { $_.Name -like "/api/v1/users*" }
        $usersCount = ($usersPaths | ForEach-Object { $_.Value.PSObject.Properties.Count } | Measure-Object -Sum).Sum
        Write-Host "Users endpoints: $usersCount" -ForegroundColor Green
        foreach ($path in $usersPaths) {
            $methods = $path.Value.PSObject.Properties.Name -join ", "
            Write-Host "  $($path.Name): $methods" -ForegroundColor White
        }
        Write-Host "Especificacao OK!" -ForegroundColor Green
    } else {
        Write-Host "Arquivo nao encontrado: $OutputPath" -ForegroundColor Red
    }
} finally { Pop-Location }
