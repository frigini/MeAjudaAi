# MeAjudaAi

Plataforma conectando clientes com prestadores de serviços para serviços domésticos e assistência profissional.

## Links Rápidos

- [Primeiros Passos](development.md) - Configure seu ambiente de desenvolvimento
- [Arquitetura](architecture.md) - Design e componentes do sistema
- [Referência da API](https://frigini.github.io/MeAjudaAi/redoc/) - Documentação OpenAPI (ReDoc)
- [Configuração](configuration.md) - Configurações de ambiente e deploy
- [Testes](testing/unit-vs-integration-tests.md) - Estratégias e guias de testes
- [CI/CD](ci-cd.md) - Integração e deploy contínuos
- [Roadmap](roadmap.md) - Planejamento e marcos do projeto

## Status do Projeto

- **Versão .NET**: 10.0 LTS
- **Versão Aspire**: 13.0.2 GA
- **Cobertura de Testes**: 90.56%
- **Sprint Atual**: Sprint 6 - Blazor Admin Portal Setup (30 Dez 2025 - 17 Jan 2026)

## Principais Funcionalidades

- Arquitetura multi-tenant
- Controle de acesso baseado em roles (Cliente, Prestador, Admin)
- Processamento de documentos com Azure Document Intelligence
- Serviços de busca e geolocalização
- Arquitetura orientada a mensagens com RabbitMQ
- Cache distribuído com Redis
- Observabilidade abrangente com OpenTelemetry

## Stack de Desenvolvimento

### Backend
- **.NET 10.0** - Framework da aplicação
- **ASP.NET Core** - APIs Web
- **Entity Framework Core** - Acesso a dados
- **PostgreSQL** - Banco de dados principal
- **RabbitMQ** - Message broker
- **Redis** - Cache distribuído
- **Keycloak** - Provedor de identidade
- **Azure Services** - Infraestrutura em nuvem
- **.NET Aspire** - Orquestração cloud-native

### Frontend
- **Blazor WebAssembly (.NET 10)** - SPA framework
- **MudBlazor 7.21.0+** - Material Design UI components
- **Fluxor 6.1.0+** - Flux/Redux state management
- **Refit 9.0.2+** - Typed HTTP client
- **bUnit** - Component testing
- **Playwright** - E2E testing

## Estrutura da Documentação

- **Primeiros Passos** - Configuração e setup de desenvolvimento
- **Arquitetura** - Design do sistema, padrões e infraestrutura
- **Módulos** - Documentação específica de domínio
  - [Providers](modules/providers.md) - Gestão de prestadores de serviço
  - [Documents](modules/documents.md) - Processamento e validação de documentos
  - [Service Catalogs](modules/service-catalogs.md) - Catálogo de serviços
  - [Locations](modules/locations.md) - Geolocalização e endereços
  - [Search Providers](modules/search-providers.md) - Busca e indexação
  - [Users](modules/users.md) - Gerenciamento de usuários
  - [Admin Portal](modules/admin-portal.md) - **Portal administrativo Blazor WASM**
- **CI/CD** - Automação de build, testes e deploy
- **Testes** - Estratégias de testes e relatórios de cobertura
- **Referência** - Roadmap, débito técnico e segurança

## Contribuindo

1. Faça um fork do repositório
2. Crie uma branch de feature
3. Siga o [guia de desenvolvimento](development.md)
4. Envie um pull request

## Licença

Veja o arquivo [LICENSE](https://github.com/frigini/MeAjudaAi/blob/master/LICENSE) para detalhes.
