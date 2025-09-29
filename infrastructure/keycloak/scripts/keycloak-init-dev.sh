#!/bin/bash
# keycloak-init-dev.sh
# Development Keycloak initialization script with demo secrets
# Only for development environment - NOT for production use

set -euo pipefail

# Dependency checks
command -v curl >/dev/null || { echo "❌ curl not found"; exit 1; }
command -v jq >/dev/null   || { echo "❌ jq not found"; exit 1; }

# Configuration
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080}"
REALM_NAME="${REALM_NAME:-meajudaai}"
ADMIN_USERNAME="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:?KEYCLOAK_ADMIN_PASSWORD is required}"
PRINT_SECRETS="${PRINT_SECRETS:-false}"

# Development-only secrets (safe for VCS in dev script)
DEV_API_CLIENT_SECRET="${MEAJUDAAI_API_CLIENT_SECRET:-dev_api_secret_123}"

echo "🚀 Starting Keycloak development initialization..."

# Wait for Keycloak to be ready
echo "⏳ Waiting for Keycloak to be ready..."
for i in {1..60}; do
    if curl -sf "${KEYCLOAK_URL}/health/ready" >/dev/null 2>&1; then
        echo "✅ Keycloak is ready"
        break
    fi
    if [[ $i -eq 60 ]]; then
        echo "❌ Timeout waiting for Keycloak to be ready"
        exit 1
    fi
    sleep 5
done

# Authenticate with Keycloak admin
echo "🔑 Authenticating with Keycloak admin..."
ADMIN_TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=${ADMIN_USERNAME}" \
    -d "password=${ADMIN_PASSWORD}" \
    -d "grant_type=password" \
    -d "client_id=admin-cli" | jq -r '.access_token' 2>/dev/null || echo "null")

if [[ "${ADMIN_TOKEN}" == "null" || -z "${ADMIN_TOKEN}" ]]; then
    echo "❌ Failed to authenticate with Keycloak admin"
    echo "ℹ️ Make sure Keycloak admin credentials are correct"
    exit 1
fi

echo "✅ Successfully authenticated with Keycloak"

# Configure API client secret for development
echo "🔧 Configuring API client secret for development..."
curl -sf -X PUT "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients/meajudaai-api" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" \
    -H "Content-Type: application/json" \
    -d "{\"secret\": \"${DEV_API_CLIENT_SECRET}\"}" || {
    echo "❌ Failed to configure API client secret"
    exit 1
}

echo "✅ Keycloak development initialization completed successfully!"
echo ""
echo "📋 Development Configuration:"
if [[ "${PRINT_SECRETS}" == "true" ]]; then
    echo "  • API client secret: ${DEV_API_CLIENT_SECRET}"
else
    echo "  • API client secret: [MASKED - set PRINT_SECRETS=true to show]"
fi
echo "  • Demo users available in realm import"
echo "  • Registration: Enabled for testing"
echo "  • Local redirect URIs: Configured"
echo ""
if [[ "${PRINT_SECRETS}" == "true" ]]; then
    echo "🔐 Demo Users:"
    echo "  • admin@meajudaai.dev / dev_admin_123 (admin, super-admin)"
    echo "  • joao@dev.example.com / dev_customer_123 (customer)"
    echo "  • maria@dev.example.com / dev_provider_123 (service-provider)"
else
    echo "🔐 Demo Users:"
    echo "  • admin@meajudaai.dev / [MASKED] (admin, super-admin)"
    echo "  • joao@dev.example.com / [MASKED] (customer)"
    echo "  • maria@dev.example.com / [MASKED] (service-provider)"
    echo "  ℹ️ Set PRINT_SECRETS=true to show passwords"
fi