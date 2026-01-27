# Script para reimportar o realm do Keycloak
# Este script deleta o realm existente e reimporta com as novas configurações
#
# Uso:
#   .\reimport-realm.ps1 [-ShowSecrets] [-KeycloakUrl <url>] [-RealmName <name>] [-RealmFile <path>]
#
# Variáveis de ambiente:
#   KEYCLOAK_ADMIN_USER - Nome de usuário do admin (padrão: "admin")
#   KEYCLOAK_CLIENT_ID - Client ID para autenticação (padrão: "admin-cli")
#   KEYCLOAK_URL - URL do Keycloak (padrão: "http://localhost:8080")
#   REALM_NAME - Nome do realm (padrão: "meajudaai")
#   REALM_FILE - Caminho do arquivo de configuração do realm

param(
    [switch]$ShowSecrets = $false,
    [string]$KeycloakUrl = $env:KEYCLOAK_URL,
    [string]$RealmName = $env:REALM_NAME,
    [string]$RealmFile = $env:REALM_FILE
)

# Configurações padrão
if ([string]::IsNullOrWhiteSpace($KeycloakUrl)) {
    $KeycloakUrl = "http://localhost:8080"
}

if ([string]::IsNullOrWhiteSpace($RealmName)) {
    $RealmName = "meajudaai"
}

if ([string]::IsNullOrWhiteSpace($RealmFile)) {
    $RealmFile = ".\infrastructure\keycloak\realms\meajudaai-realm.dev.json"
}

# Validar que o arquivo existe
if (-not (Test-Path $RealmFile)) {
    Write-Error "Arquivo de realm não encontrado: $RealmFile"
    exit 1
}

# Obter credenciais de ambiente ou prompt
$adminUser = $env:KEYCLOAK_ADMIN_USER
if ([string]::IsNullOrWhiteSpace($adminUser)) {
    $adminUser = "admin"
}

$clientId = $env:KEYCLOAK_CLIENT_ID
if ([string]::IsNullOrWhiteSpace($clientId)) {
    $clientId = "admin-cli"
}

# Solicitar senha de forma segura
$adminPassword = Read-Host -Prompt "Digite a senha do admin do Keycloak" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($adminPassword)
$adminPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
[System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)

Write-Host "Reimportando realm do Keycloak..." -ForegroundColor Cyan
Write-Host "URL: $KeycloakUrl"
Write-Host "Realm: $RealmName"
Write-Host "Arquivo: $RealmFile"
if ($ShowSecrets) {
    Write-Host "Admin User: $adminUser"
    Write-Host "Client ID: $clientId"
}
Write-Host ""

# 1. Obter token de admin
Write-Host "1. Obtendo token de administrador..." -ForegroundColor Yellow

$tokenBody = @{
    username   = $adminUser
    password   = $adminPasswordPlain
    grant_type = "password"
    client_id  = $clientId
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" `
        -Method Post `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $tokenBody `
        -ErrorAction Stop

    $ADMIN_TOKEN = $tokenResponse.access_token
    Write-Host "Token obtido com sucesso" -ForegroundColor Green
}
catch {
    Write-Error "Erro ao obter token de admin. Verifique se o Keycloak está rodando e as credenciais estão corretas."
    Write-Error $_.Exception.Message
    exit 1
}
finally {
    # Limpar senha da memória
    $adminPasswordPlain = $null
    $tokenBody = $null
}

Write-Host ""

# 2. Deletar realm existente (se existir)
Write-Host "2. Deletando realm existente (se existir)..." -ForegroundColor Yellow

$headers = @{
    Authorization = "Bearer $ADMIN_TOKEN"
}

try {
    Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/$RealmName" `
        -Method Delete `
        -Headers $headers `
        -ErrorAction Stop
    Write-Host "Realm deletado com sucesso" -ForegroundColor Green
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "Realm não existia, continuando..." -ForegroundColor Gray
    }
    else {
        Write-Error "Erro ao deletar realm: $($_.Exception.Message)"
        exit 1
    }
}

Write-Host ""

# 3. Importar novo realm
Write-Host "3. Importando novo realm..." -ForegroundColor Yellow

$realmContent = Get-Content -Path $RealmFile -Raw -ErrorAction Stop

try {
    Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms" `
        -Method Post `
        -Headers $headers `
        -ContentType "application/json" `
        -Body $realmContent `
        -ErrorAction Stop

    Write-Host "Realm importado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "ATENÇÃO: Configurações de segurança necessárias:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Configure o Client Secret para 'meajudaai-api-service':" -ForegroundColor Cyan
    Write-Host "   - Acesse: $KeycloakUrl/admin/master/console/#/$RealmName/clients"
    Write-Host "   - Selecione 'meajudaai-api-service'"
    Write-Host "   - Vá para a aba 'Credentials'"
    Write-Host "   - Gere ou configure um novo secret"
    Write-Host "   - Atualize a variável de ambiente KEYCLOAK_CLIENT_SECRET na aplicação"
    Write-Host ""
    Write-Host "2. Configure senhas para os usuários:" -ForegroundColor Cyan
    Write-Host "   - Acesse: $KeycloakUrl/admin/master/console/#/$RealmName/users"
    Write-Host "   - Para cada usuário (admin.portal, customer.demo):"
    Write-Host "     * Clique no usuário"
    Write-Host "     * Vá para 'Credentials'"
    Write-Host "     * Defina uma senha"
    Write-Host ""
    Write-Host "Reimportação concluída! Reinicie a aplicação Aspire após configurar as credenciais." -ForegroundColor Green
}
catch {
    Write-Error "Erro ao importar realm."
    Write-Error $_.Exception.Message
    exit 1
}
