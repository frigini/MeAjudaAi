#!/bin/bash
# keycloak-init-prod.sh
# Production Keycloak initialization script for secure secret management
# This script should be run after Keycloak starts to configure clients with secure secrets

set -euo pipefail

# Configuration
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080}"
REALM_NAME="${REALM_NAME:-meajudaai}"
ADMIN_USERNAME="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:?Error: KEYCLOAK_ADMIN_PASSWORD must be set}"

# Required environment variables for production secrets
WEB_REDIRECT_URIS="${MEAJUDAAI_WEB_REDIRECT_URIS:?Error: MEAJUDAAI_WEB_REDIRECT_URIS must be set}"
WEB_ORIGINS="${MEAJUDAAI_WEB_ORIGINS:?Error: MEAJUDAAI_WEB_ORIGINS must be set}"

echo "🔐 Starting Keycloak production initialization..."

# Check for required tools before any operations
command -v jq >/dev/null 2>&1 || { echo "❌ Error: 'jq' is required"; exit 1; }
command -v curl >/dev/null 2>&1 || { echo "❌ Error: 'curl' is required"; exit 1; }

# Wait for Keycloak to be ready
echo "⏳ Waiting for Keycloak to be ready..."
READY_ATTEMPTS="${READY_ATTEMPTS:-60}"
READY_SLEEP_SEC="${READY_SLEEP_SEC:-5}"
for ((i=1; i<=READY_ATTEMPTS; i++)); do
    if curl -sf "${KEYCLOAK_URL}/health/ready" >/dev/null 2>&1; then
        echo "✅ Keycloak is ready"
        break
    fi
    if [[ $i -eq ${READY_ATTEMPTS} ]]; then
        echo "❌ Timeout waiting for Keycloak to be ready"
        exit 1
    fi
    sleep "${READY_SLEEP_SEC}"
done

# Authenticate with Keycloak admin
echo "🔑 Authenticating with Keycloak admin..."
ADMIN_TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    --data-urlencode "username=${ADMIN_USERNAME}" \
    --data-urlencode "password=${ADMIN_PASSWORD}" \
    --data-urlencode "grant_type=password" \
    --data-urlencode "client_id=admin-cli" | jq -r '.access_token')

if [[ "${ADMIN_TOKEN}" == "null" || -z "${ADMIN_TOKEN}" ]]; then
    echo "❌ Failed to authenticate with Keycloak admin"
    exit 1
fi

echo "✅ Successfully authenticated with Keycloak"

# Configure API client secret
echo "🔧 Configuring API client secret..."

# Fetch API client UUID
API_CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients?clientId=meajudaai-api-service" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.[0].id')

if [[ -z "${API_CLIENT_UUID}" || "${API_CLIENT_UUID}" == "null" ]]; then
    echo "❌ Could not locate meajudaai-api client"
    exit 1
fi

# Initialize status tracking for client secret rotation
API_SECRET_STATUS="skipped"

# Conditionally rotate client secret (only if explicitly requested)
if [[ "${ROTATE_API_CLIENT_SECRET:-}" == "true" ]]; then
    echo "🔄 Rotating API client secret..."
    
    # Rotate client secret and capture the generated value
    NEW_SECRET_RESPONSE=$(curl -sf -X POST \
        "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients/${API_CLIENT_UUID}/client-secret" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}")

    if [[ $? -ne 0 || -z "${NEW_SECRET_RESPONSE}" ]]; then
        echo "❌ Failed to configure API client secret"
        API_SECRET_STATUS="failed"
        exit 1
    fi

    CONFIGURED_SECRET=$(echo "$NEW_SECRET_RESPONSE" | jq -r '.value')
    if [[ -z "${CONFIGURED_SECRET}" || "${CONFIGURED_SECRET}" == "null" ]]; then
      echo "❌ Could not read generated client secret"
      API_SECRET_STATUS="failed"
      exit 1
    fi

    # Optionally persist secret if a target is provided
    if [[ -n "${WRITE_API_CLIENT_SECRET_TO:-}" ]]; then
      # Ensure parent directory exists with secure permissions
      parent_dir=$(dirname -- "${WRITE_API_CLIENT_SECRET_TO}")
      parent_dir_existed=false
      
      # Check if parent directory already exists
      if [[ -d "$parent_dir" ]]; then
        parent_dir_existed=true
      fi
      
      if ! mkdir -p "$parent_dir" 2>/dev/null; then
        echo "❌ Failed to create directory: $parent_dir"
        API_SECRET_STATUS="failed"
        exit 1
      fi
      
      # Only set restrictive permissions on directories we created
      if [[ "$parent_dir_existed" == false ]]; then
        chmod 700 "$parent_dir"
      fi
      
      umask 077; printf '%s' "${CONFIGURED_SECRET}" > "${WRITE_API_CLIENT_SECRET_TO}"
      echo "✅ API client secret written to ${WRITE_API_CLIENT_SECRET_TO}"
    fi
    
    echo "✅ API client secret rotated successfully"
    API_SECRET_STATUS="rotated"
else
    echo "ℹ️ API client secret rotation skipped (set ROTATE_API_CLIENT_SECRET=true to enable)"
fi

# Configure web client redirect URIs and origins
echo "🌐 Configuring web client redirect URIs and origins..."

# Fetch web client UUID
WEB_CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients?clientId=admin-portal" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.[0].id')

if [[ -z "${WEB_CLIENT_UUID}" || "${WEB_CLIENT_UUID}" == "null" ]]; then
    echo "❌ Could not locate admin-portal client"
    exit 1
fi

IFS=',' read -ra REDIRECT_ARRAY <<< "${WEB_REDIRECT_URIS}"
IFS=',' read -ra ORIGINS_ARRAY <<< "${WEB_ORIGINS}"

# Clean arrays by trimming whitespace and filtering empty entries
CLEAN_REDIRECTS=()
for uri in "${REDIRECT_ARRAY[@]}"; do
    # Trim leading/trailing whitespace and strip carriage returns
    cleaned_uri=$(echo "$uri" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//;s/\r$//')
    # Skip empty entries
    if [[ -n "$cleaned_uri" ]]; then
        CLEAN_REDIRECTS+=("$cleaned_uri")
    fi
done

CLEAN_ORIGINS=()
for origin in "${ORIGINS_ARRAY[@]}"; do
    # Trim leading/trailing whitespace and strip carriage returns
    cleaned_origin=$(echo "$origin" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//;s/\r$//')
    # Skip empty entries
    if [[ -n "$cleaned_origin" ]]; then
        CLEAN_ORIGINS+=("$cleaned_origin")
    fi
done

# Generate JSON from cleaned arrays
REDIRECT_JSON=$(printf '%s\n' "${CLEAN_REDIRECTS[@]}" | jq -R . | jq -s .)
ORIGINS_JSON=$(printf '%s\n' "${CLEAN_ORIGINS[@]}" | jq -R . | jq -s .)

# Fetch current client configuration and update redirect URIs and origins
WEB_CLIENT_PAYLOAD=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients/${WEB_CLIENT_UUID}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq \
    --argjson redirects "${REDIRECT_JSON}" \
    --argjson origins "${ORIGINS_JSON}" \
    '.redirectUris=$redirects | .webOrigins=$origins')

curl -sf -X PUT "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients/${WEB_CLIENT_UUID}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" \
    -H "Content-Type: application/json" \
    -d "${WEB_CLIENT_PAYLOAD}" || {
    echo "❌ Failed to configure admin-portal client"
    exit 1
}

# Create initial admin user if specified
if [[ -n "${INITIAL_ADMIN_USERNAME:-}" && -n "${INITIAL_ADMIN_PASSWORD:-}" && -n "${INITIAL_ADMIN_EMAIL:-}" ]]; then
    echo "👤 Creating initial admin user..."
    
    # Check if user already exists
    USER_EXISTS=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/users?username=${INITIAL_ADMIN_USERNAME}" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq length)
    
    if [[ "${USER_EXISTS}" -eq 0 ]]; then
        echo "🔄 Step 1: Creating user with basic info..."
        # Create user with only username, email, and enabled status
        USER_CREATION_RESPONSE=$(curl -sf -o /dev/null -w "%{http_code}" -X POST "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/users" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" \
            -H "Content-Type: application/json" \
            -d "{
                \"username\": \"${INITIAL_ADMIN_USERNAME}\",
                \"email\": \"${INITIAL_ADMIN_EMAIL}\",
                \"enabled\": true
            }")
        
        HTTP_CODE="${USER_CREATION_RESPONSE}"
        if [[ "${HTTP_CODE}" != "201" ]]; then
            echo "❌ Failed to create initial admin user (HTTP ${HTTP_CODE})"
            exit 1
        fi
        
        echo "🔄 Step 2: Retrieving created user ID..."
        # Retrieve the created user's ID
        USER_ID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/users?username=${INITIAL_ADMIN_USERNAME}" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.[0].id')
        
        if [[ -z "${USER_ID}" || "${USER_ID}" == "null" ]]; then
            echo "❌ Failed to retrieve created user ID"
            exit 1
        fi
        
        echo "🔄 Step 3: Setting user password..."
        # Set user password using the reset-password endpoint
        PASSWORD_RESPONSE=$(curl -sf -o /dev/null -w "%{http_code}" -X PUT "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/users/${USER_ID}/reset-password" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" \
            -H "Content-Type: application/json" \
            -d "{
                \"type\": \"password\",
                \"value\": \"${INITIAL_ADMIN_PASSWORD}\",
                \"temporary\": true
            }")
        
        HTTP_CODE="${PASSWORD_RESPONSE}"
        if [[ "${HTTP_CODE}" != "204" ]]; then
            echo "❌ Failed to set user password (HTTP ${HTTP_CODE})"
            exit 1
        fi
        
        echo "🔄 Step 4: Fetching realm role representations..."
        # Fetch admin role representation
        ADMIN_ROLE=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/roles/admin" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}")
        
        if [[ -z "${ADMIN_ROLE}" || "${ADMIN_ROLE}" == "null" ]]; then
            echo "❌ Failed to fetch admin role representation"
            exit 1
        fi
        
        # Fetch super-admin role representation
        SUPER_ADMIN_ROLE=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/roles/super-admin" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}")
        
        if [[ -z "${SUPER_ADMIN_ROLE}" || "${SUPER_ADMIN_ROLE}" == "null" ]]; then
            echo "❌ Failed to fetch super-admin role representation"
            exit 1
        fi
        
        echo "🔄 Step 5: Assigning realm roles..."
        # Assign realm roles to the user
        ROLES_PAYLOAD=$(echo "[${ADMIN_ROLE}, ${SUPER_ADMIN_ROLE}]")
        ROLE_ASSIGNMENT_RESPONSE=$(curl -sf -o /dev/null -w "%{http_code}" -X POST "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/users/${USER_ID}/role-mappings/realm" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" \
            -H "Content-Type: application/json" \
            -d "${ROLES_PAYLOAD}")
        
        HTTP_CODE="${ROLE_ASSIGNMENT_RESPONSE}"
        if [[ "${HTTP_CODE}" != "204" ]]; then
            echo "❌ Failed to assign realm roles (HTTP ${HTTP_CODE})"
            exit 1
        fi
        
        echo "✅ Initial admin user created successfully with all roles assigned"
    else
        echo "ℹ️ Initial admin user already exists, skipping creation"
    fi
fi

# Configure production realm security settings
echo "🔒 Configuring production security settings..."

# Fetch current realm configuration and apply security settings
REALM_PAYLOAD=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | \
    jq '.registrationAllowed=false | .sslRequired="all" | .passwordPolicy="length(12) and digits(1) and lowerCase(1) and upperCase(1) and specialChars(1) and notUsername and notEmail"')

curl -sf -X PUT "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" \
    -H "Content-Type: application/json" \
    -d "${REALM_PAYLOAD}" || {
    echo "❌ Failed to configure realm security settings"
    exit 1
}

echo "✅ Production security settings applied"

echo "✅ Keycloak production initialization completed successfully!"
echo ""
echo "📋 Configuration Summary:"

# Generate appropriate status message for API client secret
case "${API_SECRET_STATUS}" in
    "rotated")
        echo "  • API client secret: Rotated via Admin REST"
        ;;
    "skipped")
        echo "  • API client secret: Skipped (rotation not enabled)"
        ;;
    "failed")
        echo "  • API client secret: Rotation failed"
        ;;
    *)
        echo "  • API client secret: Unknown status"
        ;;
esac

echo "  • Web client redirects: ${WEB_REDIRECT_URIS}"
echo "  • Web client origins: ${WEB_ORIGINS}"
echo "  • Registration: Disabled for production"
echo "  • SSL Required: All connections"
echo "  • Password Policy: Enforced strong passwords"