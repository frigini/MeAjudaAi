# MeAjudaAi

Platform connecting customers with service providers for home services and professional assistance.

## Quick Links

- [Getting Started](development.md) - Setup your development environment
- [Architecture](architecture.md) - System design and components
- [Configuration](configuration.md) - Environment and deployment settings
- [Testing](testing/unit-vs-integration-tests.md) - Testing strategy and guides
- [CI/CD](ci-cd.md) - Continuous integration and deployment
- [Roadmap](roadmap.md) - Project planning and milestones

## Project Status

- **.NET Version**: 10.0 LTS
- **Aspire Version**: 13.0.2 GA
- **Test Coverage**: 90.56%
- **Current Sprint**: Sprint 3 (started 10 Dec 2025)

## Key Features

- Multi-tenant architecture
- Role-based access control (Customer, Provider, Admin)
- Document processing with Azure Document Intelligence
- Search and geolocation services
- Message-driven architecture with RabbitMQ
- Distributed caching with Redis
- Comprehensive observability with OpenTelemetry

## Development Stack

- **.NET 10.0** - Application framework
- **ASP.NET Core** - Web APIs
- **Entity Framework Core** - Data access
- **PostgreSQL** - Primary database
- **RabbitMQ** - Message broker
- **Redis** - Distributed cache
- **Keycloak** - Identity provider
- **Azure Services** - Cloud infrastructure
- **.NET Aspire** - Cloud-native orchestration

## Documentation Structure

- **Getting Started** - Development setup and configuration
- **Architecture** - System design, patterns, and infrastructure
- **Modules** - Domain-specific documentation
- **CI/CD** - Build, test, and deployment automation
- **Testing** - Test strategies and coverage reports
- **Reference** - Roadmap, technical debt, and security

## Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the [development guide](development.md)
4. Submit a pull request

## License

See [LICENSE](../LICENSE) file for details.
