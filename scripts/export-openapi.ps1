#requires -Version 5.1
Set-StrictMode -Version Latest
param(
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$OutputPath = "api-spec.json"
)
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$OutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $ProjectRoot $OutputPath }
try {
    Write-Host "Validando especificacao OpenAPI..." -ForegroundColor Cyan
    if (Test-Path -PathType Leaf $OutputPath) {
        $Content = Get-Content -Raw -ErrorAction Stop $OutputPath | ConvertFrom-Json -ErrorAction Stop
        if (-not $Content.paths) {
            Write-Error "Secao 'paths' ausente no OpenAPI: $OutputPath"
            exit 1
        }
        if (-not $Content.openapi -or -not ($Content.openapi -match '^3(\.|$)')) {
            Write-Error "Campo 'openapi' 3.x ausente ou invalido no OpenAPI: $OutputPath"
            exit 1
        }
        # Define valid HTTP operation names (case-insensitive)
        $httpMethods = @('get', 'post', 'put', 'delete', 'patch', 'options', 'head', 'trace')
        
        $allPaths  = $Content.paths.PSObject.Properties
        $PathCount = @($allPaths).Count
        Write-Host "Total paths: $PathCount" -ForegroundColor Green
        $totalOps = [int](($allPaths |
            ForEach-Object {
                ($_.Value.PSObject.Properties | Where-Object { $httpMethods -contains $_.Name.ToLowerInvariant() }).Count
            } | Measure-Object -Sum
        ).Sum)
        Write-Host "Total operations: $totalOps" -ForegroundColor Green
        $usersPaths = $Content.paths.PSObject.Properties | Where-Object { $_.Name -match '^/api/v1/users($|/)' }
        
        # Count only HTTP operations, not other properties like "parameters"
        $usersCount = [int](($usersPaths | ForEach-Object { 
            $httpOps = $_.Value.PSObject.Properties | Where-Object { $httpMethods -contains $_.Name.ToLowerInvariant() }
            $httpOps.Count
        } | Measure-Object -Sum).Sum)
        
        Write-Host "Users endpoints: $usersCount" -ForegroundColor Green
        $sortedUsersPaths = $usersPaths | Sort-Object Name
        foreach ($path in $sortedUsersPaths) {
            # Filter to only HTTP operation names
            $httpOps = $path.Value.PSObject.Properties | Where-Object { $httpMethods -contains $_.Name.ToLowerInvariant() }
            $methods = ($httpOps.Name | Sort-Object | ForEach-Object { $_.ToUpperInvariant() }) -join ", "
            if ([string]::IsNullOrWhiteSpace($methods)) { $methods = "(no operations)" }
            Write-Host "  $($path.Name): $methods" -ForegroundColor White
        }
        Write-Host "Especificacao OK!" -ForegroundColor Green
    } else {
        Write-Host "Arquivo nao encontrado: $OutputPath" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Error ("Falha ao validar especificacao: " + $_.Exception.Message)
    exit 1
}
