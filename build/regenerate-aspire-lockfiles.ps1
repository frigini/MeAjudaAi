#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Regenerates package lock files for Aspire-related projects to ensure cross-platform compatibility.

.DESCRIPTION
    This script regenerates package.lock.json files for projects that depend on Aspire.Dashboard.Sdk,
    which has platform-specific runtime dependencies (linux-x64, win-x64). Running this on Linux ensures
    the lockfiles include linux-x64 dependencies required by CI/CD pipelines.

.PARAMETER Force
    Force regeneration even if lockfiles appear up-to-date.

.EXAMPLE
    ./regenerate-aspire-lockfiles.ps1
    Regenerates lockfiles for AppHost, Integration.Tests, and E2E.Tests

.EXAMPLE
    ./regenerate-aspire-lockfiles.ps1 -Force
    Forces regeneration regardless of current state

.NOTES
    Author: MeAjudaAi Team
    This script should be run on Linux (CI environment) to generate cross-platform lockfiles.
    When run on Windows, it will generate Windows-specific lockfiles that break Linux CI.
#>

[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Projects that require Aspire.Dashboard.Sdk platform-specific dependencies
$aspirePlatformProjects = @(
    "src/Aspire/MeAjudaAi.AppHost/MeAjudaAi.AppHost.csproj",
    "tests/MeAjudaAi.Integration.Tests/MeAjudaAi.Integration.Tests.csproj",
    "tests/MeAjudaAi.E2E.Tests/MeAjudaAi.E2E.Tests.csproj"
)

# Get repository root (script is in build/ directory)
$repoRoot = Split-Path -Parent $PSScriptRoot
Write-Host "📁 Repository root: $repoRoot" -ForegroundColor Cyan

# Detect platform
$isLinux = $IsLinux -or ($env:OS -notlike "Windows*")
$platform = if ($isLinux) { "Linux" } else { "Windows" }
Write-Host "🖥️  Platform detected: $platform" -ForegroundColor Cyan

if (-not $isLinux -and -not $Force) {
    Write-Warning "⚠️  Running on Windows will generate Windows-specific lockfiles that break Linux CI!"
    Write-Warning "   This script is intended to run on Linux (CI environment)."
    Write-Warning "   Use -Force to proceed anyway (for testing only)."
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "❌ Aborted by user" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n🔄 Regenerating Aspire platform-specific lockfiles..." -ForegroundColor Green
Write-Host "   Projects: $($aspirePlatformProjects.Count)" -ForegroundColor Gray

$success = 0
$failed = 0

foreach ($project in $aspirePlatformProjects) {
    $projectPath = Join-Path $repoRoot $project
    $projectName = Split-Path -Leaf $project
    
    if (-not (Test-Path $projectPath)) {
        Write-Warning "⚠️  Project not found: $projectPath"
        $failed++
        continue
    }

    Write-Host "`n📦 Processing: $projectName" -ForegroundColor Yellow
    
    try {
        # Remove existing lockfile to force regeneration
        $lockfilePath = Join-Path (Split-Path $projectPath) "packages.lock.json"
        if (Test-Path $lockfilePath) {
            Write-Host "   🗑️  Removing existing lockfile..." -ForegroundColor Gray
            Remove-Item $lockfilePath -Force
        }

        # Restore with force-evaluate to regenerate lockfile
        Write-Host "   🔧 Restoring packages..." -ForegroundColor Gray
        $restoreOutput = dotnet restore $projectPath --force-evaluate 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   ❌ Restore failed!" -ForegroundColor Red
            Write-Host $restoreOutput -ForegroundColor Red
            $failed++
            continue
        }

        # Verify lockfile was created
        if (-not (Test-Path $lockfilePath)) {
            Write-Host "   ❌ Lockfile not created!" -ForegroundColor Red
            $failed++
            continue
        }

        # Check for platform-specific dependencies
        $lockfileContent = Get-Content $lockfilePath -Raw | ConvertFrom-Json
        $hasDashboardSdk = $lockfileContent.dependencies.PSObject.Properties.Name -contains 'net10.0' -and
                           $lockfileContent.dependencies.'net10.0'.PSObject.Properties.Name -match 'Aspire\.Dashboard\.Sdk'

        if ($hasDashboardSdk) {
            Write-Host "   ✅ Lockfile regenerated successfully with Aspire.Dashboard.Sdk" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Lockfile regenerated but Aspire.Dashboard.Sdk not found" -ForegroundColor Yellow
        }

        $success++
    }
    catch {
        Write-Host "   ❌ Error: $_" -ForegroundColor Red
        $failed++
    }
}

Write-Host "`n📊 Summary:" -ForegroundColor Cyan
Write-Host "   ✅ Success: $success" -ForegroundColor Green
Write-Host "   ❌ Failed:  $failed" -ForegroundColor Red

if ($failed -gt 0) {
    Write-Host "`n❌ Some lockfiles failed to regenerate!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ All Aspire lockfiles regenerated successfully!" -ForegroundColor Green
Write-Host "`n💡 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Verify changes with: git diff **/*.lock.json" -ForegroundColor Gray
Write-Host "   2. Validate restore: dotnet restore MeAjudaAi.sln --locked-mode" -ForegroundColor Gray
Write-Host "   3. Commit changes: git add **/*.lock.json && git commit -m 'chore: regenerate Aspire lockfiles for Linux compatibility'" -ForegroundColor Gray

exit 0
