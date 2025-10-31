# Setup script for MeAjudaAi CI/CD Pipeline
# Run this script to help set up your GitHub Actions pipeline

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "meajudaai-github-actions"
)

Write-Host "🚀 MeAjudaAi CI/CD Setup Script" -ForegroundColor Blue
Write-Host "=================================" -ForegroundColor Blue

# Check if Azure CLI is installed
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "✅ Azure CLI version: $($azVersion.'azure-cli')" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Login to Azure
Write-Host "`n🔐 Checking Azure authentication..." -ForegroundColor Blue
try {
    $account = az account show --output json | ConvertFrom-Json
    Write-Host "✅ Logged in to Azure as: $($account.user.name)" -ForegroundColor Green
    Write-Host "✅ Subscription: $($account.name) ($($account.id))" -ForegroundColor Green
    
    if ($account.id -ne $SubscriptionId) {
        Write-Host "⚠️  Setting subscription to: $SubscriptionId" -ForegroundColor Yellow
        az account set --subscription $SubscriptionId
    }
} catch {
    Write-Host "❌ Not logged in to Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

# Create Service Principal
Write-Host "`n🔧 Creating Service Principal for GitHub Actions..." -ForegroundColor Blue
try {
    $spOutput = az ad sp create-for-rbac `
        --name $ServicePrincipalName `
        --role "Contributor" `
        --scopes "/subscriptions/$SubscriptionId" `
        --sdk-auth `
        --output json

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Service Principal created successfully" -ForegroundColor Green
        
        # Parse and display the credentials
        $credentials = $spOutput | ConvertFrom-Json
        
        Write-Host "`n📋 GitHub Secret Configuration:" -ForegroundColor Blue
        Write-Host "================================" -ForegroundColor Blue
        Write-Host "Add this as AZURE_CREDENTIALS secret in your GitHub repository:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host $spOutput -ForegroundColor Cyan
        Write-Host ""
        
        # Save to file for easy copying
        $spOutput | Out-File -FilePath "azure-credentials.json" -Encoding UTF8
        Write-Host "✅ Credentials also saved to: azure-credentials.json" -ForegroundColor Green
        Write-Host "🔒 Remember to delete this file after copying to GitHub!" -ForegroundColor Red
        
    } else {
        Write-Host "❌ Failed to create Service Principal" -ForegroundColor Red
        Write-Host "This might be because a Service Principal with this name already exists." -ForegroundColor Yellow
        Write-Host "Try using a different name or delete the existing one first." -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error creating Service Principal: $($_.Exception.Message)" -ForegroundColor Red
}

# Instructions
Write-Host "`n📚 Next Steps:" -ForegroundColor Blue
Write-Host "===============" -ForegroundColor Blue
Write-Host "1. Go to your GitHub repository settings" -ForegroundColor White
Write-Host "2. Navigate to: Settings > Secrets and variables > Actions" -ForegroundColor White
Write-Host "3. Click 'New repository secret'" -ForegroundColor White
Write-Host "4. Name: AZURE_CREDENTIALS" -ForegroundColor White
Write-Host "5. Value: Copy the JSON content from above" -ForegroundColor White
Write-Host ""
Write-Host "6. Create GitHub Environments:" -ForegroundColor White
Write-Host "   - Settings > Environments > New environment" -ForegroundColor White
Write-Host "   - Create: development, production" -ForegroundColor White
Write-Host ""
Write-Host "7. Push your code to trigger the pipeline!" -ForegroundColor White

# Resource group information
Write-Host "`n🌍 Environment Configuration:" -ForegroundColor Blue
Write-Host "==============================" -ForegroundColor Blue
Write-Host "Development:  meajudaai-dev      (auto-deploy from 'develop' branch or manual)" -ForegroundColor Green
Write-Host ""
Write-Host "💡 This is a dev-only setup optimized for local development." -ForegroundColor Cyan
Write-Host "   You can add production environments later when needed." -ForegroundColor Cyan

# Cost reminder
Write-Host "`n💰 Cost Reminder:" -ForegroundColor Blue
Write-Host "==================" -ForegroundColor Blue
Write-Host "Each environment costs ~$10 USD/month when running" -ForegroundColor Yellow
Write-Host "Use the cleanup workflow to delete dev resources when not needed" -ForegroundColor Yellow

Write-Host "`n🎉 Setup completed! Your CI/CD pipeline is ready to use." -ForegroundColor Green
