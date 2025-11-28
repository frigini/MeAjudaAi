#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Identifica gaps de code coverage no projeto MeAjudaAi

.DESCRIPTION
    Analisa o código fonte e identifica:
    - CommandHandlers sem testes
    - QueryHandlers sem testes
    - Validators sem testes
    - Value Objects sem testes
    - Repositories sem testes

.EXAMPLE
    .\scripts\find-coverage-gaps.ps1
#>

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Analisando gaps de code coverage..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# 1. Command/Query Handlers
# ============================================================================

Write-Host "📋 COMMAND/QUERY HANDLERS SEM TESTES" -ForegroundColor Yellow
Write-Host "=" * 80

$handlers = Get-ChildItem -Path "src/Modules/*/Application" -Recurse -Filter "*Handler.cs" | 
    Where-Object { $_.Name -match "(Command|Query)Handler\.cs$" }

$missingHandlerTests = @()

foreach ($handler in $handlers) {
    $handlerName = $handler.BaseName
    $testName = "${handlerName}Tests"
    $module = ($handler.FullName -split "Modules\\")[1] -split "\\" | Select-Object -First 1
    
    # Procurar teste correspondente
    $testPath = "src/Modules/$module/Tests/**/${testName}.cs"
    $testExists = Test-Path $testPath -PathType Leaf
    
    if (-not $testExists) {
        # Tentar buscar em qualquer lugar dentro de Tests
        $searchResult = Get-ChildItem -Path "src/Modules/$module/Tests" -Recurse -Filter "${testName}.cs" -ErrorAction SilentlyContinue
        
        if (-not $searchResult) {
            $missingHandlerTests += [PSCustomObject]@{
                Module = $module
                Handler = $handlerName
                ExpectedTest = $testName
                Type = if ($handlerName -match "Command") { "Command" } else { "Query" }
            }
        }
    }
}

if ($missingHandlerTests.Count -eq 0) {
    Write-Host "✅ Todos os handlers possuem testes!" -ForegroundColor Green
} else {
    $missingHandlerTests | Format-Table -AutoSize
    Write-Host "❌ Total: $($missingHandlerTests.Count) handlers sem testes" -ForegroundColor Red
}

Write-Host ""

# ============================================================================
# 2. Validators
# ============================================================================

Write-Host "✅ VALIDATORS (FLUENTVALIDATION) SEM TESTES" -ForegroundColor Yellow
Write-Host "=" * 80

$validators = Get-ChildItem -Path "src/Modules/*/Application" -Recurse -Filter "*Validator.cs" |
    Where-Object { $_.Name -match "Validator\.cs$" -and $_.Name -notmatch "Tests" }

$missingValidatorTests = @()

foreach ($validator in $validators) {
    $validatorName = $validator.BaseName
    $testName = "${validatorName}Tests"
    $module = ($validator.FullName -split "Modules\\")[1] -split "\\" | Select-Object -First 1
    
    $searchResult = Get-ChildItem -Path "src/Modules/$module/Tests" -Recurse -Filter "${testName}.cs" -ErrorAction SilentlyContinue
    
    if (-not $searchResult) {
        $missingValidatorTests += [PSCustomObject]@{
            Module = $module
            Validator = $validatorName
            ExpectedTest = $testName
        }
    }
}

if ($missingValidatorTests.Count -eq 0) {
    Write-Host "✅ Todos os validators possuem testes!" -ForegroundColor Green
} else {
    $missingValidatorTests | Format-Table -AutoSize
    Write-Host "❌ Total: $($missingValidatorTests.Count) validators sem testes" -ForegroundColor Red
}

Write-Host ""

# ============================================================================
# 3. Value Objects (Domain)
# ============================================================================

Write-Host "💎 VALUE OBJECTS (DOMAIN) SEM TESTES" -ForegroundColor Yellow
Write-Host "=" * 80

$commonValueObjects = @(
    "Address", "Email", "PhoneNumber", "CPF", "CNPJ", 
    "DocumentType", "Money", "DateRange", "TimeSlot"
)

$missingVOTests = @()

foreach ($module in (Get-ChildItem -Path "src/Modules" -Directory).Name) {
    $domainPath = "src/Modules/$module/Domain"
    
    if (Test-Path $domainPath) {
        # Buscar por Value Objects comuns
        foreach ($vo in $commonValueObjects) {
            $voFile = Get-ChildItem -Path $domainPath -Recurse -Filter "${vo}.cs" -ErrorAction SilentlyContinue
            
            if ($voFile) {
                $testName = "${vo}Tests"
                $testExists = Get-ChildItem -Path "src/Modules/$module/Tests" -Recurse -Filter "${testName}.cs" -ErrorAction SilentlyContinue
                
                if (-not $testExists) {
                    $missingVOTests += [PSCustomObject]@{
                        Module = $module
                        ValueObject = $vo
                        ExpectedTest = $testName
                    }
                }
            }
        }
    }
}

if ($missingVOTests.Count -eq 0) {
    Write-Host "✅ Principais Value Objects possuem testes!" -ForegroundColor Green
} else {
    $missingVOTests | Format-Table -AutoSize
    Write-Host "❌ Total: $($missingVOTests.Count) value objects sem testes" -ForegroundColor Red
}

Write-Host ""

# ============================================================================
# 4. Repositories
# ============================================================================

Write-Host "🗄️ REPOSITORIES SEM TESTES" -ForegroundColor Yellow
Write-Host "=" * 80

$repositories = Get-ChildItem -Path "src/Modules/*/Infrastructure" -Recurse -Filter "*Repository.cs" |
    Where-Object { $_.Name -match "Repository\.cs$" -and $_.Name -notmatch "Interface|Tests" }

$missingRepoTests = @()

foreach ($repo in $repositories) {
    $repoName = $repo.BaseName
    $testName = "${repoName}Tests"
    $module = ($repo.FullName -split "Modules\\")[1] -split "\\" | Select-Object -First 1
    
    $searchResult = Get-ChildItem -Path "src/Modules/$module/Tests" -Recurse -Filter "${testName}.cs" -ErrorAction SilentlyContinue
    
    if (-not $searchResult) {
        $missingRepoTests += [PSCustomObject]@{
            Module = $module
            Repository = $repoName
            ExpectedTest = $testName
        }
    }
}

if ($missingRepoTests.Count -eq 0) {
    Write-Host "✅ Todos os repositories possuem testes!" -ForegroundColor Green
} else {
    $missingRepoTests | Format-Table -AutoSize
    Write-Host "❌ Total: $($missingRepoTests.Count) repositories sem testes" -ForegroundColor Red
}

Write-Host ""

# ============================================================================
# 5. Resumo
# ============================================================================

Write-Host "📊 RESUMO DE GAPS" -ForegroundColor Cyan
Write-Host "=" * 80

$totalGaps = $missingHandlerTests.Count + $missingValidatorTests.Count + 
             $missingVOTests.Count + $missingRepoTests.Count

Write-Host "Handlers sem testes:       $($missingHandlerTests.Count)" -ForegroundColor $(if ($missingHandlerTests.Count -eq 0) { "Green" } else { "Red" })
Write-Host "Validators sem testes:     $($missingValidatorTests.Count)" -ForegroundColor $(if ($missingValidatorTests.Count -eq 0) { "Green" } else { "Red" })
Write-Host "Value Objects sem testes:  $($missingVOTests.Count)" -ForegroundColor $(if ($missingVOTests.Count -eq 0) { "Green" } else { "Red" })
Write-Host "Repositories sem testes:   $($missingRepoTests.Count)" -ForegroundColor $(if ($missingRepoTests.Count -eq 0) { "Green" } else { "Red" })
Write-Host ""
Write-Host "TOTAL DE GAPS:             $totalGaps" -ForegroundColor $(if ($totalGaps -eq 0) { "Green" } else { "Red" })

Write-Host ""

# ============================================================================
# 6. Estimativa de Impacto no Coverage
# ============================================================================

Write-Host "📈 ESTIMATIVA DE IMPACTO NO COVERAGE" -ForegroundColor Cyan
Write-Host "=" * 80

# Estimativas conservadoras:
# - Cada handler: +0.5pp
# - Cada validator: +0.3pp
# - Cada Value Object: +0.4pp
# - Cada repository: +0.6pp

$estimatedImpact = ($missingHandlerTests.Count * 0.5) + 
                   ($missingValidatorTests.Count * 0.3) + 
                   ($missingVOTests.Count * 0.4) + 
                   ($missingRepoTests.Count * 0.6)

Write-Host "Coverage atual (pipeline): 35.11%"
Write-Host "Coverage estimado após fixes: $(35.11 + $estimatedImpact)% (+$($estimatedImpact)pp)"
Write-Host ""

if ($estimatedImpact -ge 20) {
    Write-Host "✅ Potencial para atingir meta de 55%!" -ForegroundColor Green
} elseif ($estimatedImpact -ge 10) {
    Write-Host "⚠️ Bom progresso, mas pode precisar de mais testes" -ForegroundColor Yellow
} else {
    Write-Host "⚠️ Impacto baixo, considere outras áreas" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 PRÓXIMOS PASSOS" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host "1. Priorize handlers críticos (Commands > Queries)"
Write-Host "2. Adicione testes para validators (rápido, alto impacto)"
Write-Host "3. Teste Value Objects com casos edge (validações)"
Write-Host "4. Repositories: use InMemory DbContext ou mocks"
Write-Host ""

# Exportar para arquivo CSV (opcional)
if ($Verbose) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportPath = "coverage-gaps-$timestamp.csv"
    
    $allGaps = @()
    $allGaps += $missingHandlerTests | Select-Object @{N='Category';E={'Handler'}}, Module, @{N='Name';E={$_.Handler}}, ExpectedTest
    $allGaps += $missingValidatorTests | Select-Object @{N='Category';E={'Validator'}}, Module, @{N='Name';E={$_.Validator}}, ExpectedTest
    $allGaps += $missingVOTests | Select-Object @{N='Category';E={'ValueObject'}}, Module, @{N='Name';E={$_.ValueObject}}, ExpectedTest
    $allGaps += $missingRepoTests | Select-Object @{N='Category';E={'Repository'}}, Module, @{N='Name';E={$_.Repository}}, ExpectedTest
    
    $allGaps | Export-Csv -Path $reportPath -NoTypeInformation -Encoding UTF8
    Write-Host "📄 Relatório exportado: $reportPath" -ForegroundColor Green
}

exit $totalGaps
