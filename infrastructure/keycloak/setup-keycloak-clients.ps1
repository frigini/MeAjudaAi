<#
.SYNOPSIS
    Automatiza a configuração de clientes Keycloak para MeAjudaAi.

.DESCRIPTION
    Este script cria automaticamente:
    - Realm "meajudaai" (se não existir)
    - Clientes OIDC: "admin-portal" e "customer-app"
    - Roles: admin, customer, operator, viewer
    - Usuários demo com senhas para desenvolvimento

.PARAMETER KeycloakUrl
    URL base do Keycloak (padrão: http://localhost:8080)

.PARAMETER AdminUsername
    Username do admin do Keycloak (padrão: admin)

.PARAMETER AdminPassword
    Password do admin do Keycloak (padrão: admin)

.PARAMETER RealmName
    Nome do realm (padrão: meajudaai)

.EXAMPLE
    .\setup-keycloak-clients.ps1
    
.EXAMPLE
    .\setup-keycloak-clients.ps1 -KeycloakUrl "http://localhost:9090" -AdminPassword "mypassword"
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$KeycloakUrl = "http://localhost:8080",
    
    [Parameter()]
    [string]$AdminUsername = "admin",
    
    [Parameter()]
    [string]$AdminPassword = "admin",
    
    [Parameter()]
    [string]$RealmName = "meajudaai"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Cores para output
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"
$ErrorColor = "Red"

function Write-Step {
    param([string]$Message)
    Write-Host "➜ $Message" -ForegroundColor $InfoColor
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor $SuccessColor
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor $WarningColor
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor $ErrorColor
}

function Test-KeycloakRunning {
    Write-Step "Validando se Keycloak está rodando..."
    
    try {
        $response = Invoke-WebRequest -Uri "$KeycloakUrl/health/ready" -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction SilentlyContinue
        
        if ($response.StatusCode -eq 200) {
            Write-Success "Keycloak está rodando em $KeycloakUrl"
            return $true
        }
    }
    catch {
        Write-Error "Keycloak não está acessível em $KeycloakUrl"
        Write-Host "   Certifique-se de que o Keycloak está rodando:" -ForegroundColor Gray
        Write-Host "   docker-compose up -d keycloak" -ForegroundColor Gray
        Write-Host "   ou" -ForegroundColor Gray
        Write-Host "   dotnet run --project src/Aspire/MeAjudaAi.AppHost" -ForegroundColor Gray
        return $false
    }
}

function Get-AdminToken {
    Write-Step "Obtendo token de administrador..."
    
    $tokenUrl = "$KeycloakUrl/realms/master/protocol/openid-connect/token"
    $body = @{
        grant_type = "password"
        client_id  = "admin-cli"
        username   = $AdminUsername
        password   = $AdminPassword
    }
    
    try {
        $response = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"
        Write-Success "Token obtido com sucesso"
        return $response.access_token
    }
    catch {
        Write-Error "Falha ao obter token de administrador"
        Write-Host "   Verifique as credenciais: -AdminUsername '$AdminUsername' -AdminPassword '***'" -ForegroundColor Gray
        throw
    }
}

function Test-RealmExists {
    param([string]$Token)
    
    $headers = @{
        Authorization  = "Bearer $Token"
        "Content-Type" = "application/json"
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName" -Method Get -Headers $headers -ErrorAction SilentlyContinue
        return $true
    }
    catch {
        return $false
    }
}

function New-Realm {
    param([string]$Token)
    
    Write-Step "Criando realm '$RealmName'..."
    
    $headers = @{
        Authorization  = "Bearer $Token"
        "Content-Type" = "application/json"
    }
    
    $realm = @{
        realm               = $RealmName
        enabled             = $true
        displayName         = "MeAjudaAi"
        displayNameHtml     = "<b>MeAjudaAi</b> Platform"
        registrationAllowed = $false
        loginWithEmailAllowed = $true
        duplicateEmailsAllowed = $false
        resetPasswordAllowed = $true
        editUsernameAllowed = $false
        bruteForceProtected = $true
        sslRequired         = "external"
        accessTokenLifespan = 300
        accessTokenLifespanForImplicitFlow = 900
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms" -Method Post -Headers $headers -Body $realm
        Write-Success "Realm '$RealmName' criado"
    }
    catch {
        Write-Error "Falha ao criar realm: $_"
        throw
    }
}

function New-Client {
    param(
        [string]$Token,
        [string]$ClientId,
        [string]$Name,
        [string]$Description,
        [string[]]$RedirectUris,
        [string[]]$WebOrigins
    )
    
    Write-Step "Criando client '$ClientId'..."
    
    $headers = @{
        Authorization  = "Bearer $Token"
        "Content-Type" = "application/json"
    }
    
    $client = @{
        clientId                   = $ClientId
        name                       = $Name
        description                = $Description
        enabled                    = $true
        publicClient               = $true
        protocol                   = "openid-connect"
        standardFlowEnabled        = $true
        implicitFlowEnabled        = $false
        directAccessGrantsEnabled  = $true
        serviceAccountsEnabled     = $false
        authorizationServicesEnabled = $false
        redirectUris               = $RedirectUris
        webOrigins                 = $WebOrigins
        attributes                 = @{
            "pkce.code.challenge.method" = "S256"
        }
        defaultClientScopes        = @("profile", "email", "roles", "web-origins")
        optionalClientScopes       = @("address", "phone", "offline_access", "microprofile-jwt")
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/clients" -Method Post -Headers $headers -Body $client
        Write-Success "Client '$ClientId' criado"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Warning "Client '$ClientId' já existe"
        }
        else {
            Write-Error "Falha ao criar client '$ClientId': $_"
            throw
        }
    }
}

function New-RealmRole {
    param(
        [string]$Token,
        [string]$RoleName,
        [string]$Description
    )
    
    $headers = @{
        Authorization  = "Bearer $Token"
        "Content-Type" = "application/json"
    }
    
    $role = @{
        name        = $RoleName
        description = $Description
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/roles" -Method Post -Headers $headers -Body $role -ErrorAction SilentlyContinue
        Write-Success "Role '$RoleName' criada"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Warning "Role '$RoleName' já existe"
        }
        else {
            throw
        }
    }
}

function New-User {
    param(
        [string]$Token,
        [string]$Username,
        [string]$Email,
        [string]$FirstName,
        [string]$LastName,
        [string]$Password,
        [string[]]$Roles
    )
    
    Write-Step "Criando usuário '$Username'..."
    
    $headers = @{
        Authorization  = "Bearer $Token"
        "Content-Type" = "application/json"
    }
    
    # Criar usuário
    $user = @{
        username      = $Username
        email         = $Email
        firstName     = $FirstName
        lastName      = $LastName
        enabled       = $true
        emailVerified = $true
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/users" -Method Post -Headers $headers -Body $user
        Write-Success "Usuário '$Username' criado"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Warning "Usuário '$Username' já existe"
            return
        }
        else {
            Write-Error "Falha ao criar usuário '$Username': $_"
            throw
        }
    }
    
    # Obter ID do usuário
    $users = Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/users?username=$Username" -Method Get -Headers $headers
    $userId = $users[0].id
    
    # Definir senha
    $credentials = @{
        type      = "password"
        value     = $Password
        temporary = $false
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/users/$userId/reset-password" -Method Put -Headers $headers -Body $credentials
    Write-Success "Senha definida para '$Username'"
    
    # Atribuir roles
    if ($Roles.Count -gt 0) {
        $realmRoles = Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/roles" -Method Get -Headers $headers
        $rolesToAssign = $realmRoles | Where-Object { $Roles -contains $_.name } | Select-Object id, name
        
        $roleMapping = $rolesToAssign | ConvertTo-Json -AsArray
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName/users/$userId/role-mappings/realm" -Method Post -Headers $headers -Body $roleMapping
        Write-Success "Roles atribuídas: $($Roles -join ', ')"
    }
}

# ============================================
# Main Script
# ============================================

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      Keycloak Client Automation - MeAjudaAi Platform          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 1. Validar Keycloak
if (-not (Test-KeycloakRunning)) {
    exit 1
}

Write-Host ""

# 2. Obter token admin
try {
    $token = Get-AdminToken
}
catch {
    exit 1
}

Write-Host ""

# 3. Criar ou validar realm
if (Test-RealmExists -Token $token) {
    Write-Warning "Realm '$RealmName' já existe"
}
else {
    New-Realm -Token $token
}

Write-Host ""

# 4. Criar clients
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host " Criando Clients OIDC" -ForegroundColor Cyan
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# Admin Portal Client
New-Client -Token $token `
    -ClientId "admin-portal" `
    -Name "MeAjudaAi Admin Portal" `
    -Description "Portal administrativo para gestão da plataforma" `
    -RedirectUris @(
        "https://localhost:7281/*",
        "https://localhost:7281/authentication/login-callback",
        "http://localhost:5281/*",
        "https://admin.meajudaai.com.br/*"
    ) `
    -WebOrigins @(
        "https://localhost:7281",
        "http://localhost:5281",
        "https://admin.meajudaai.com.br"
    )

Write-Host ""

# Customer App Client
New-Client -Token $token `
    -ClientId "customer-app" `
    -Name "MeAjudaAi Customer App" `
    -Description "Aplicativo do cliente (Web + Mobile)" `
    -RedirectUris @(
        "https://localhost:7282/*",
        "https://localhost:7282/authentication/login-callback",
        "http://localhost:5282/*",
        "https://app.meajudaai.com.br/*",
        "meajudaai://callback"
    ) `
    -WebOrigins @(
        "https://localhost:7282",
        "http://localhost:5282",
        "https://app.meajudaai.com.br"
    )

Write-Host ""

# 5. Criar roles
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host " Criando Roles" -ForegroundColor Cyan
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

New-RealmRole -Token $token -RoleName "admin" -Description "Administrador total da plataforma"
New-RealmRole -Token $token -RoleName "operator" -Description "Operador com leitura/escrita limitada"
New-RealmRole -Token $token -RoleName "viewer" -Description "Visualizador somente leitura"
New-RealmRole -Token $token -RoleName "customer" -Description "Cliente da plataforma"

Write-Host ""

# 6. Criar usuários demo
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host " Criando Usuários Demo" -ForegroundColor Cyan
Write-Host "──────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

New-User -Token $token `
    -Username "admin.portal" `
    -Email "admin@meajudaai.com.br" `
    -FirstName "Admin" `
    -LastName "Portal" `
    -Password "admin123" `
    -Roles @("admin")

Write-Host ""

New-User -Token $token `
    -Username "customer.demo" `
    -Email "customer@meajudaai.com.br" `
    -FirstName "Customer" `
    -LastName "Demo" `
    -Password "customer123" `
    -Roles @("customer")

Write-Host ""

# 7. Resumo
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                  Configuração Concluída! ✓                     ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Keycloak configurado com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Resumo da Configuração:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Realm:          " -NoNewline; Write-Host $RealmName -ForegroundColor Yellow
Write-Host "  Keycloak URL:   " -NoNewline; Write-Host $KeycloakUrl -ForegroundColor Yellow
Write-Host "  Admin Console:  " -NoNewline; Write-Host "$KeycloakUrl/admin" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔐 Clients Criados:" -ForegroundColor Cyan
Write-Host "  • admin-portal   (Admin Portal OIDC)" -ForegroundColor White
Write-Host "  • customer-app   (Customer App OIDC)" -ForegroundColor White
Write-Host ""
Write-Host "👤 Usuários Demo:" -ForegroundColor Cyan
Write-Host "  • admin.portal / admin123      (role: admin)" -ForegroundColor White
Write-Host "  • customer.demo / customer123  (role: customer)" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Próximos Passos:" -ForegroundColor Cyan
Write-Host "  1. Acesse o Admin Portal: https://localhost:7281" -ForegroundColor Gray
Write-Host "  2. Faça login com: admin.portal / admin123" -ForegroundColor Gray
Write-Host "  3. Verifique se a autenticação está funcionando" -ForegroundColor Gray
Write-Host ""
Write-Host "📚 Documentação: docs/keycloak-admin-portal-setup.md" -ForegroundColor Gray
Write-Host ""
