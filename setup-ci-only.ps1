# Setup script for MeAjudaAi CI-Only Pipeline
# Run this script to set up GitHub Actions CI pipeline without deployment

Write-Host "🚀 MeAjudaAi CI-Only Setup Script" -ForegroundColor Blue
Write-Host "===================================" -ForegroundColor Blue

# Check if we're in a git repository
try {
    $gitStatus = git status 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Not in a git repository. Please run this from your project root." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Git repository detected" -ForegroundColor Green
} catch {
    Write-Host "❌ Git is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Check if GitHub CLI is available (optional)
try {
    $ghVersion = gh --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ GitHub CLI available: $($ghVersion[0])" -ForegroundColor Green
        $hasGhCli = $true
    }
} catch {
    Write-Host "⚠️  GitHub CLI not found (optional for easier setup)" -ForegroundColor Yellow
    $hasGhCli = $false
}

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ .NET SDK version: $dotnetVersion" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install .NET 9 SDK: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Verify Aspire workload
Write-Host "`n🔧 Checking Aspire workload..." -ForegroundColor Blue
try {
    $workloads = dotnet workload list 2>$null
    if ($workloads -match "aspire") {
        Write-Host "✅ Aspire workload is installed" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Installing Aspire workload..." -ForegroundColor Yellow
        dotnet workload install aspire
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Aspire workload installed successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to install Aspire workload" -ForegroundColor Red
            exit 1
        }
    }
} catch {
    Write-Host "❌ Error checking Aspire workload" -ForegroundColor Red
    exit 1
}

# Test build locally
Write-Host "`n🏗️  Testing local build..." -ForegroundColor Blue
try {
    Write-Host "Restoring dependencies..." -ForegroundColor Gray
    dotnet restore MeAjudaAi.sln | Out-Null
    
    Write-Host "Building solution..." -ForegroundColor Gray
    dotnet build MeAjudaAi.sln --configuration Release --no-restore | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Local build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Local build failed. Please fix build errors before setting up CI." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error during local build test" -ForegroundColor Red
    exit 1
}

# Check if GitHub Actions workflow already exists
$workflowPath = ".github/workflows/aspire-ci-cd.yml"
if (Test-Path $workflowPath) {
    Write-Host "`n✅ GitHub Actions workflow already exists: $workflowPath" -ForegroundColor Green
} else {
    Write-Host "`n❌ GitHub Actions workflow not found at: $workflowPath" -ForegroundColor Red
    Write-Host "The workflow file should have been created. Please check if it exists." -ForegroundColor Yellow
}

# Repository setup instructions
Write-Host "`n📚 Next Steps:" -ForegroundColor Blue
Write-Host "===============" -ForegroundColor Blue

if ($hasGhCli) {
    Write-Host "Option 1 - Using GitHub CLI (Recommended):" -ForegroundColor Green
    Write-Host "1. Commit and push your changes:" -ForegroundColor White
    Write-Host "   git add ." -ForegroundColor Cyan
    Write-Host "   git commit -m 'Add CI pipeline'" -ForegroundColor Cyan
    Write-Host "   git push" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. View your workflow runs:" -ForegroundColor White
    Write-Host "   gh run list" -ForegroundColor Cyan
    Write-Host "   gh run watch" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Option $(if($hasGhCli){'2'}else{'1'}) - Manual GitHub setup:" -ForegroundColor Green
Write-Host "1. Commit and push your changes to GitHub" -ForegroundColor White
Write-Host "2. Go to your repository on GitHub.com" -ForegroundColor White
Write-Host "3. Click 'Actions' tab to see your CI pipeline" -ForegroundColor White
Write-Host "4. The pipeline will run automatically on push/PR" -ForegroundColor White
Write-Host ""

Write-Host "📋 What this CI pipeline does:" -ForegroundColor Blue
Write-Host "===============================" -ForegroundColor Blue
Write-Host "✅ Builds your entire .NET solution" -ForegroundColor Green
Write-Host "✅ Runs unit tests" -ForegroundColor Green
Write-Host "✅ Validates Aspire configuration" -ForegroundColor Green
Write-Host "✅ Checks code formatting and quality" -ForegroundColor Green
Write-Host "✅ Validates services can be containerized" -ForegroundColor Green
Write-Host "💰 NO deployment = NO cloud costs!" -ForegroundColor Cyan
Write-Host ""

Write-Host "🔄 When you're ready to add deployment:" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host "• Set up Azure credentials using setup-cicd.ps1" -ForegroundColor White
Write-Host "• Enable deployment in the workflow file" -ForegroundColor White
Write-Host "• Or choose an alternative cloud provider" -ForegroundColor White
Write-Host ""

Write-Host "🎉 CI-Only pipeline setup completed!" -ForegroundColor Green
Write-Host "Push your code to see the pipeline in action." -ForegroundColor Cyan
