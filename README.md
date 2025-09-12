# MeAjudaAi

A comprehensive service platform built with .NET Aspire, designed to connect service providers with customers.

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code

### Running Locally

1. **Start Infrastructure Services**
   ```bash
   cd infrastructure
   ./scripts/start-dev.sh
   ```

2. **Run the Application**
   ```bash
   dotnet run --project src/Aspire/MeAjudaAi.AppHost
   ```

### Services URLs
- **Application Dashboard**: https://localhost:15152
- **Keycloak Admin**: http://localhost:8080 (admin/admin)
- **API Service**: https://localhost:5001

## 📁 Project Structure

```
MeAjudaAi/
├── src/
│   ├── Aspire/                 # .NET Aspire orchestration
│   ├── Bootstrapper/           # API service bootstrapper
│   ├── Modules/                # Feature modules
│   │   └── Users/              # User management module
│   └── Shared/                 # Shared libraries
├── infrastructure/            # Docker infrastructure
│   ├── compose/               # Docker Compose files
│   ├── keycloak/              # Keycloak configuration
│   └── scripts/               # Convenience scripts
├── tests/                     # Test projects
└── docs/                      # Documentation
```

## 🏗️ Architecture

- **Backend**: .NET 8 with Clean Architecture
- **Frontend**: Blazor (planned)
- **Authentication**: Keycloak
- **Database**: PostgreSQL
- **Caching**: Redis
- **Messaging**: RabbitMQ
- **Orchestration**: .NET Aspire

## 📚 Documentation

- [Infrastructure Setup](infrastructure/Infrastructure.md)
- [Docker Quick Start](infrastructure/QUICK-START.md)
- [CI/CD Setup](docs/CI-CD-Setup.md)

## 🔧 Development

### Module Structure
Each module follows Clean Architecture principles:
- `API/` - Controllers and endpoints
- `Application/` - Use cases and business logic
- `Domain/` - Entities and domain services
- `Infrastructure/` - Data access and external services

### Contributing
1. Create a feature branch
2. Follow existing patterns and naming conventions
3. Add tests for new functionality
4. Update documentation as needed

## 🚢 Deployment

For production deployment, see [Infrastructure Documentation](infrastructure/Infrastructure.md).

## 📄 License

This project is proprietary software.