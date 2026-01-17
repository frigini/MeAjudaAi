# keycloak-init-dev.ps1
# Development Keycloak initialization script with demo secrets
# Only for development environment - NOT for production use

$ErrorActionPreference = "Stop"

# Configuration
$KEYCLOAK_URL = if ($env:KEYCLOAK_URL) { $env:KEYCLOAK_URL } else { "http://localhost:8080" }
$REALM_NAME = if ($env:REALM_NAME) { $env:REALM_NAME } else { "meajudaai" }
$ADMIN_USERNAME = if ($env:KEYCLOAK_ADMIN) { $env:KEYCLOAK_ADMIN } else { "admin" }
$ADMIN_PASSWORD = if ($env:KEYCLOAK_ADMIN_PASSWORD) { $env:KEYCLOAK_ADMIN_PASSWORD } else { "admin123" }
$PRINT_SECRETS = if ($env:PRINT_SECRETS) { $env:PRINT_SECRETS } else { "false" }

# Development-only secrets (safe for VCS in dev script)
$DEV_API_CLIENT_SECRET = if ($env:MEAJUDAAI_API_CLIENT_SECRET) { $env:MEAJUDAAI_API_CLIENT_SECRET } else { "dev_api_secret_123" }

Write-Host "🚀 Starting Keycloak development initialization..." -ForegroundColor Cyan

# Wait for Keycloak to be ready
Write-Host "⏳ Waiting for Keycloak to be ready..." -ForegroundColor Yellow
$maxAttempts = 60
$attempt = 0
$isReady = $false

while ($attempt -lt $maxAttempts -and -not $isReady) {
    try {
        $response = Invoke-WebRequest -Uri "$KEYCLOAK_URL/health/ready" -Method Get -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $isReady = $true
            Write-Host "✅ Keycloak is ready" -ForegroundColor Green
        }
    }
    catch {
        $attempt++
        if ($attempt -ge $maxAttempts) {
            Write-Host "❌ Timeout waiting for Keycloak to be ready" -ForegroundColor Red
            exit 1
        }
        Start-Sleep -Seconds 5
    }
}

# Authenticate with Keycloak admin
Write-Host "🔑 Authenticating with Keycloak admin..." -ForegroundColor Cyan

$authBody = @{
    username   = $ADMIN_USERNAME
    password   = $ADMIN_PASSWORD
    grant_type = "password"
    client_id  = "admin-cli"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" `
        -Method Post `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $authBody

    $ADMIN_TOKEN = $tokenResponse.access_token

    if (-not $ADMIN_TOKEN) {
        throw "No access token received"
    }

    Write-Host "✅ Successfully authenticated with Keycloak" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to authenticate with Keycloak admin" -ForegroundColor Red
    Write-Host "ℹ️  Make sure Keycloak admin credentials are correct" -ForegroundColor Yellow
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if realm exists
Write-Host "🔍 Checking if realm '$REALM_NAME' exists..." -ForegroundColor Cyan

try {
    $realmExists = $false
    try {
        $realm = Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME" `
            -Method Get `
            -Headers @{ Authorization = "Bearer $ADMIN_TOKEN" }
        $realmExists = $true
        Write-Host "✅ Realm '$REALM_NAME' already exists" -ForegroundColor Green
    }
    catch {
        Write-Host "📦 Realm '$REALM_NAME' does not exist, importing..." -ForegroundColor Yellow
        
        # Import realm from JSON file
        $realmFile = "$PSScriptRoot\..\realms\meajudaai-realm.dev.json"
        
        if (Test-Path $realmFile) {
            try {
                $realmJson = Get-Content $realmFile -Raw
                
                Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms" `
                    -Method Post `
                    -Headers @{ 
                        Authorization = "Bearer $ADMIN_TOKEN"
                        "Content-Type" = "application/json"
                    } `
                    -Body $realmJson
                
                $realmExists = $true
                Write-Host "✅ Realm imported successfully" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Failed to import realm: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "   You may need to import manually via Keycloak Admin Console" -ForegroundColor Yellow
                exit 1
            }
        }
        else {
            Write-Host "❌ Realm file not found: $realmFile" -ForegroundColor Red
            Write-Host "   See: docs/keycloak-admin-portal-setup.md" -ForegroundColor Yellow
            exit 1
        }
    }

    if ($realmExists) {
        # Configure API client secret for development
        Write-Host "🔧 Configuring API client secret for development..." -ForegroundColor Cyan

        try {
            # Get client ID
            $clients = Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" `
                -Method Get `
                -Headers @{ Authorization = "Bearer $ADMIN_TOKEN" }

            $apiClient = $clients | Where-Object { $_.clientId -eq "meajudaai-api" }

            if ($apiClient) {
                $clientUuid = $apiClient.id

                # Update client secret
                $secretBody = @{
                    secret = $DEV_API_CLIENT_SECRET
                } | ConvertTo-Json

                Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients/$clientUuid" `
                    -Method Put `
                    -Headers @{ 
                        Authorization = "Bearer $ADMIN_TOKEN"
                        "Content-Type" = "application/json"
                    } `
                    -Body $secretBody

                Write-Host "✅ API client secret configured successfully" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️  Client 'meajudaai-api' not found in realm" -ForegroundColor Yellow
                Write-Host "   Make sure the realm import includes the API client" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "⚠️  Failed to configure API client secret: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "❌ Error checking realm: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Keycloak development initialization completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Development Configuration:" -ForegroundColor Cyan

if ($PRINT_SECRETS -eq "true") {
    Write-Host "  • API client secret: $DEV_API_CLIENT_SECRET" -ForegroundColor White
}
else {
    Write-Host "  • API client secret: [MASKED - set PRINT_SECRETS=true to show]" -ForegroundColor White
}

Write-Host "  • Demo users available in realm import" -ForegroundColor White
Write-Host "  • Registration: Enabled for testing" -ForegroundColor White
Write-Host "  • Local redirect URIs: Configured" -ForegroundColor White
Write-Host ""

if ($PRINT_SECRETS -eq "true") {
    Write-Host "🔐 Demo Users:" -ForegroundColor Cyan
    Write-Host "  • admin@meajudaai.dev / dev_admin_123 (admin, super-admin)" -ForegroundColor White
    Write-Host "  • joao@dev.example.com / dev_customer_123 (customer)" -ForegroundColor White
    Write-Host "  • maria@dev.example.com / dev_provider_123 (service-provider)" -ForegroundColor White
}
else {
    Write-Host "🔐 Demo Users:" -ForegroundColor Cyan
    Write-Host "  • admin@meajudaai.dev / [MASKED] (admin, super-admin)" -ForegroundColor White
    Write-Host "  • joao@dev.example.com / [MASKED] (customer)" -ForegroundColor White
    Write-Host "  • maria@dev.example.com / [MASKED] (service-provider)" -ForegroundColor White
    Write-Host "  ℹ️  Set PRINT_SECRETS=true to show passwords" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🌐 Keycloak URLs:" -ForegroundColor Cyan
Write-Host "  • Admin Console: $KEYCLOAK_URL/admin" -ForegroundColor White
Write-Host "  • Realm: $KEYCLOAK_URL/realms/$REALM_NAME" -ForegroundColor White
Write-Host "  • OIDC Config: $KEYCLOAK_URL/realms/$REALM_NAME/.well-known/openid-configuration" -ForegroundColor White
