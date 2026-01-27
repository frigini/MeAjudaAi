# Script para reimportar o realm do Keycloak
# Este script deleta o realm existente e reimporta com as novas configuracoes

$KEYCLOAK_URL = "http://localhost:8080"
$REALM_NAME = "meajudaai"
$REALM_FILE = ".\infrastructure\keycloak\realms\meajudaai-realm.dev.json"

Write-Host "Reimportando realm do Keycloak..." -ForegroundColor Cyan
Write-Host "URL: $KEYCLOAK_URL"
Write-Host "Realm: $REALM_NAME"
Write-Host "Arquivo: $REALM_FILE"
Write-Host ""

# 1. Obter token de admin
Write-Host "1. Obtendo token de administrador..." -ForegroundColor Yellow

$tokenBody = @{
    username   = "admin"
    password   = "admin123"
    grant_type = "password"
    client_id  = "admin-cli"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" `
        -Method Post `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $tokenBody

    $ADMIN_TOKEN = $tokenResponse.access_token
    Write-Host "Token obtido com sucesso" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao obter token de admin. Verifique se o Keycloak esta rodando e as credenciais estao corretas." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""

# 2. Deletar realm existente (se existir)
Write-Host "2. Deletando realm existente (se existir)..." -ForegroundColor Yellow

$headers = @{
    Authorization = "Bearer $ADMIN_TOKEN"
}

try {
    Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME" `
        -Method Delete `
        -Headers $headers `
        -ErrorAction Stop
    Write-Host "Realm deletado com sucesso" -ForegroundColor Green
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "Realm nao existia, continuando..." -ForegroundColor Gray
    }
    else {
        Write-Host "Resposta inesperada ao deletar realm: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""

# 3. Importar novo realm
Write-Host "3. Importando novo realm..." -ForegroundColor Yellow

$realmContent = Get-Content -Path $REALM_FILE -Raw

try {
    Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms" `
        -Method Post `
        -Headers $headers `
        -ContentType "application/json" `
        -Body $realmContent `
        -ErrorAction Stop

    Write-Host "Realm importado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Configuracoes do Service Account:" -ForegroundColor Cyan
    Write-Host "   Client ID: meajudaai-api-service"
    Write-Host "   Client Secret: meajudaai-api-secret-dev-2024"
    Write-Host ""
    Write-Host "Usuarios criados:" -ForegroundColor Cyan
    Write-Host "   Admin: admin@meajudaai.dev / admin123"
    Write-Host "   Customer: customer@meajudaai.dev / customer123"
    Write-Host ""
    Write-Host "Reimportacao concluida! Reinicie a aplicacao Aspire." -ForegroundColor Green
}
catch {
    Write-Host "Erro ao importar realm." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
