# Keycloak Configuration

This directory contains the Keycloak realm configuration for MeAjudaAi with environment-specific security.

## 🔒 Security Architecture

### Environment-Specific Realm Files

- **`meajudaai-realm.dev.json`**: Development realm with demo users and non-sensitive test data
- **`meajudaai-realm.prod.json`**: Production realm without secrets or demo users

### Secure Secret Management

Secrets are **NEVER** stored in realm files. Instead:
- Development: Scripts inject development-safe secrets
- Production: Secrets are provided via environment variables at runtime

## 🚀 Usage

### Development Environment

```bash
# 1. Import development realm (contains demo users)
docker exec keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/meajudaai-realm.dev.json

# 2. Run development initialization script
./scripts/keycloak-init-dev.sh
```

### Production Environment

```bash
# 1. Set required environment variables
export MEAJUDAAI_API_CLIENT_SECRET="$(openssl rand -base64 32)"
export MEAJUDAAI_WEB_REDIRECT_URIS="https://yourapp.com/*,https://api.yourapp.com/*"
export MEAJUDAAI_WEB_ORIGINS="https://yourapp.com,https://api.yourapp.com"
export INITIAL_ADMIN_USERNAME="admin"
export INITIAL_ADMIN_PASSWORD="$(openssl rand -base64 32)"
export INITIAL_ADMIN_EMAIL="admin@yourcompany.com"

# 2. Import production realm (no secrets, no demo users)
# Note: For Docker Compose setups, use: docker compose exec keycloak /opt/keycloak/bin/kc.sh import ...
# Ensure realm files are mounted at /opt/keycloak/data/import/ via volumes
docker exec keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/meajudaai-realm.prod.json

# 3. Run production initialization script
./scripts/keycloak-init-prod.sh
```

## 🔐 Production Security Features

- **SSL Required**: All connections must use HTTPS
- **Registration Disabled**: No self-registration allowed
- **Strong Password Policy**: Enforced complexity requirements
- **No Hardcoded Secrets**: All secrets generated at runtime
- **No Demo Users**: Clean production environment
- **Secret Rotation**: Secrets can be updated via environment variables

## 📋 Required Environment Variables

### Production
- `KEYCLOAK_ADMIN`: Keycloak admin username (defaults to `admin`, can be overridden)
- `KEYCLOAK_ADMIN_PASSWORD`: Keycloak admin password
- `MEAJUDAAI_API_CLIENT_SECRET`: API client secret
- `MEAJUDAAI_WEB_REDIRECT_URIS`: Comma-separated redirect URIs
- `MEAJUDAAI_WEB_ORIGINS`: Comma-separated web origins
- `INITIAL_ADMIN_USERNAME`: Initial admin username (optional)
- `INITIAL_ADMIN_PASSWORD`: Initial admin password (optional)
- `INITIAL_ADMIN_EMAIL`: Initial admin email (optional)

### Development
- `KEYCLOAK_ADMIN`: Keycloak admin username (defaults to `admin`, can be overridden)
- `KEYCLOAK_ADMIN_PASSWORD`: Keycloak admin password
- `MEAJUDAAI_API_CLIENT_SECRET`: API client secret (optional, defaults to dev secret)

## 🛡️ Security Best Practices

1. **Never commit secrets**: All realm files are secret-free
2. **Environment separation**: Different configurations per environment
3. **Runtime secret injection**: Secrets added after realm import
4. **Strong password policies**: Enforced in production
5. **Minimal permissions**: Only necessary redirect URIs and origins
6. **Audit logging**: All admin actions are logged