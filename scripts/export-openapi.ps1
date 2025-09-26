param([string]$OutputPath = "api-spec.json")
$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot
$OutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $ProjectRoot $OutputPath }
try {
    Write-Host "Validando especificacao OpenAPI..." -ForegroundColor Cyan
    if (Test-Path $OutputPath) {
        $Content = Get-Content -Raw $OutputPath | ConvertFrom-Json
        # Define valid HTTP operation names (case-insensitive)
        $httpMethods = @('get', 'post', 'put', 'delete', 'patch', 'options', 'head', 'trace')
        
        $PathCount = $Content.paths.PSObject.Properties.Count
        Write-Host "Total endpoints: $PathCount" -ForegroundColor Green
        $usersPaths = $Content.paths.PSObject.Properties | Where-Object { $_.Name -like "/api/v1/users*" }
        
        # Count only HTTP operations, not other properties like "parameters"
        $usersCount = ($usersPaths | ForEach-Object { 
            $httpOps = $_.Value.PSObject.Properties | Where-Object { $httpMethods -contains $_.Name.ToLower() }
            $httpOps.Count
        } | Measure-Object -Sum).Sum
        
        Write-Host "Users endpoints: $usersCount" -ForegroundColor Green
        foreach ($path in $usersPaths) {
            # Filter to only HTTP operation names
            $httpOps = $path.Value.PSObject.Properties | Where-Object { $httpMethods -contains $_.Name.ToLower() }
            $methods = $httpOps.Name -join ", "
            Write-Host "  $($path.Name): $methods" -ForegroundColor White
        }
        Write-Host "Especificacao OK!" -ForegroundColor Green
    } else {
        Write-Host "Arquivo nao encontrado: $OutputPath" -ForegroundColor Red
        exit 1
    }
} finally { Pop-Location }
