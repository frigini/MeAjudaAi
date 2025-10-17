# Keycloak Configuration

This directory contains all Keycloak-related configuration for the MeAjudaAi project.

## Directory Structure

```text
keycloak/
├── realms/
│   └── meajudaai-realm.json          # Realm configuration for import
└── README.md
```text
## Realm Import

The `meajudaai-realm.json` file contains the MeAjudaAi realm configuration. To import the realm on startup, start Keycloak with the `--import-realm` flag (e.g., `kc.sh start --optimized --import-realm`). The default import directory is `/opt/keycloak/data/import`.

### Included Configuration

#### Clients
- **meajudaai-api**: Backend API client with client credentials
- **meajudaai-web**: Frontend web client (public)

#### Roles
- **customer**: Regular users
- **service-provider**: Service professionals  
- **admin**: Administrators
- **super-admin**: Super administrators

#### Test Users
- **admin** / admin123 (admin, super-admin roles)
- **customer1** / customer123 (customer role)
- **provider1** / provider123 (service-provider role)

### Client Configuration

#### API Client (meajudaai-api)
- **Client ID**: meajudaai-api
- **Client Secret**: your-client-secret-here
- **Flow**: Standard + Direct Access Grants + Service Account
- **Token Lifespan**: 30 minutes

#### Web Client (meajudaai-web)  
- **Client ID**: meajudaai-web
- **Type**: Public client
- **Allowed Redirects**: http://localhost:3000/*, http://localhost:5000/*
- **Allowed Origins**: http://localhost:3000, http://localhost:5000

### Security Settings

- **SSL**: Required for external requests
- **Registration**: Enabled
- **Email Login**: Enabled
- **Brute Force Protection**: Enabled
- **Password Reset**: Enabled

### Development vs Production

For production:
1. Change all default passwords
2. Generate new client secrets
3. Update redirect URIs to production domains
4. Enable proper SSL configuration
5. Configure email settings for notifications