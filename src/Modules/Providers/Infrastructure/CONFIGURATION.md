# Configuration Guide

## Database Connection String

The database connection string should **never** be committed to source control. Use one of the following methods to configure it:

### Local Development (Recommended)

Use .NET User Secrets:

```bash
# Navigate to the Infrastructure project
cd src/Modules/Providers/Infrastructure

# Set the connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=YOUR_PASSWORD"
```

### CI/CD and Production

Set the connection string via environment variable:

```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Host=your-host;Port=5432;Database=meajudaai;Username=user;Password=password"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=your-host;Port=5432;Database=meajudaai;Username=user;Password=password"
```

### Docker/Container Environments

Use environment variables in your docker-compose.yml or Kubernetes manifests:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=meajudaai;Username=user;Password=${DB_PASSWORD}
```

## Security Note

⚠️ **IMPORTANT**: The original hardcoded credentials have been removed from appsettings.json. If you previously used these credentials in production, please rotate them immediately.
