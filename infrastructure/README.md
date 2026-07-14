# MeAjudaAi Infrastructure

Infrastructure as Code (IaC) and runtime configuration for the MeAjudaAi platform.

## Contents

```
infrastructure/
├── main.bicep              # Azure resource definitions (PostgreSQL, Redis)
├── dev.parameters.json     # Azure parameters for dev environment
├── prod.parameters.json    # Azure parameters for production environment
├── database/               # SQL scripts executed at runtime by the application
│   ├── 01-init-meajudaai.sh
│   ├── modules/            # Schema roles & permissions (10 modules)
│   └── seeds/              # Domain seed data
├── keycloak/
│   ├── realms/             # OIDC realm configurations (JSON)
│   └── themes/             # Custom Keycloak login theme
└── compose/                # Docker Compose files (fallback for local dev without Aspire)
```

## Azure Deployment

Infrastructure is deployed via the `deploy-azure.yml` GitHub Actions workflow using Bicep templates.

**Resources provisioned:**
- Azure Database for PostgreSQL Flexible Server
- Azure Cache for Redis Enterprise

**How to deploy:**
1. Go to **Actions** > **Deploy to Azure**
2. Select environment (`dev` or `prod`)
3. Toggle `deploy_infrastructure` to provision Azure resources
4. Requires secrets: `AZURE_CREDENTIALS`, `POSTGRES_ADMIN_PASSWORD`

**When to update `main.bicep`:** When adding new managed resources (databases, storage accounts, etc.) to the AppHost. The Bicep file is the source of truth for Azure.

## Database Modules

The `database/modules/` directory contains SQL scripts that are **executed at runtime** by the application via `SchemaPermissionsManager.cs`. Each module has:
- `00-roles.sql` — PostgreSQL roles for schema isolation
- `01-permissions.sql` — GRANT/REVOKE statements

**These files are not dead code — they are critical runtime configuration.**

## Keycloak

- `realms/meajudaai-realm.dev.json` — Development realm (imported by Aspire via `--import-realm`)
- `themes/meajudaai/` — Custom login theme (mounted by Aspire)

The production realm (`meajudaai-realm.prod.json`) is maintained for reference but not currently imported by any automation.

## Docker Compose (Fallback)

The `compose/` directory contains Docker Compose files for local development **without Aspire**. The primary development workflow uses .NET Aspire (`.\scripts\dev.ps1`), which manages all infrastructure automatically.
