# MeAjudaAi

Uma plataforma abrangente de serviços construída com .NET Aspire, projetada para conectar prestadores de serviços com clientes usando arquitetura monólito modular.

<!-- Last updated: December 30, 2025 - Sprint 5.5 COMPLETED / Sprint 6 Planning -->

## 🎯 Visão Geral

O **MeAjudaAi** é uma plataforma moderna de marketplace de serviços que implementa as melhores práticas de desenvolvimento, incluindo Domain-Driven Design (DDD), CQRS, e arquitetura de monólito modular. A aplicação utiliza tecnologias de ponta como .NET 10, Azure, e containerização com Docker.

### 🏗️ Arquitetura

- **Monólito Modular**: Separação clara de responsabilidades por módulos de domínio
- **Domain-Driven Design (DDD)**: Modelagem rica de domínio com agregados, entidades e value objects
- **CQRS**: Separação de comandos e consultas para melhor performance e escalabilidade
- **Event-Driven**: Comunicação entre módulos através de eventos de domínio e integração
- **Clean Architecture**: Separação em camadas com inversão de dependências

### 🚀 Tecnologias Principais

- **.NET 10** - Framework principal
- **.NET Aspire 13** - Orquestração e observabilidade
- **Entity Framework Core 10** - ORM e persistência
- **PostgreSQL** - Banco de dados principal
- **Keycloak** - Autenticação e autorização
- **Redis** - Cache distribuído
- **RabbitMQ/Azure Service Bus** - Messaging
- **Docker** - Containerização
- **Azure** - Hospedagem em nuvem

## 📚 Documentação

A documentação completa do projeto está disponível em **MkDocs Material** com suporte completo em português.

### Visualização Local

Para visualizar a documentação localmente:

```bash
# Instalar MkDocs Material (apenas uma vez)
pip install mkdocs-material mkdocs-git-revision-date-localized-plugin

# Iniciar servidor de desenvolvimento
mkdocs serve

# Acessar: http://127.0.0.1:8000/MeAjudaAi/
```

### GitHub Pages

Após o merge para `master`, a documentação será publicada automaticamente em:
**https://frigini.github.io/MeAjudaAi/**

### Estrutura da Documentação

- **Primeiros Passos** - Setup de desenvolvimento e configuração
- **Arquitetura** - Design do sistema, padrões e infraestrutura
- **Módulos** - Documentação específica de cada módulo de domínio
- **CI/CD** - Pipelines, workflows e automação
- **Testes** - Estratégias, guias e relatórios de cobertura
- **Referência da API** - Endpoints REST documentados

## 📦 Estrutura do Projeto

O projeto foi organizado para facilitar navegação e manutenção:

```
📦 MeAjudaAi/
├── 📁 api/              # Especificações de API (OpenAPI)
├── 📁 build/           # Scripts de build e Makefile
├── 📁 config/          # Configurações de ferramentas
│   ├── .editorconfig  # Estilo de código
│   ├── .yamllint.yml  # Lint de YAML
│   ├── coverage.runsettings  # Configuração de coverage
│   └── coverlet.json  # Exclusões de cobertura
├── 📁 docs/            # Documentação técnica (MkDocs)
│   ├── api-reference.md    # Referência da API REST
│   ├── architecture.md     # Arquitetura do sistema
│   ├── database.md        # Estratégia de banco de dados
│   ├── logging.md         # Logging e observabilidade
│   ├── messaging.md       # Message bus e eventos
│   ├── modules/          # Documentação por módulo
│   └── testing/          # Guias de testes
├── 📁 infrastructure/  # IaC e configurações de infraestrutura
│   ├── automation/     # Scripts de setup CI/CD
│   ├── compose/        # Docker Compose configs
│   ├── database/       # Database init scripts + seeds
│   └── keycloak/       # Keycloak configuration
├── 📁 scripts/         # Scripts de desenvolvimento
├── 📁 src/             # Código fonte da aplicação
├── 📁 tests/           # Testes automatizados
└── 📁 tools/           # Ferramentas de desenvolvimento
    └── api-collections/     # Gerador de coleções Bruno/Postman
```

### Diretórios Principais

| Diretório | Propósito | Exemplos |
|-----------|-----------|----------|
| `src/` | Código fonte da aplicação | Módulos, APIs, domínios |
| `tests/` | Testes unitários e integração | xUnit v3, testes por módulo |
| `docs/` | Documentação técnica | Arquitetura, guias, ADRs |
| `infrastructure/` | Infraestrutura como código | Bicep, Docker, database, CI/CD automation |
| `scripts/` | Scripts de desenvolvimento | Exportar API, testes, deploy |
| `build/` | Build e automação | Makefile, scripts de CI |
| `config/` | Configurações de ferramentas | Linting, segurança, cobertura |

## 🚀 Início Rápido

### Para Desenvolvedores

Para instruções detalhadas, consulte o [**Guia de Desenvolvimento Completo**](./docs/development.md).

**Setup via .NET Aspire:**
```powershell
# Execute o AppHost do Aspire
cd src/Aspire/MeAjudaAi.AppHost
dotnet run
```

**Ou via Docker Compose:**
```powershell
cd infrastructure/compose
docker compose -f environments/development.yml up -d
```

### Para Testes

```powershell
# Todos os testes
dotnet test

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

📖 **[Guia Completo de Desenvolvimento](docs/development.md)**

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (para deploy em produção)
- [Git](https://git-scm.com/) para controle de versão

### ⚙️ Configuração de Ambiente

**Para deployments não-desenvolvimento:** Configure as variáveis de ambiente necessárias copiando `infrastructure/.env.example` para `infrastructure/.env` e definindo valores seguros. As seguintes variáveis são obrigatórias:
- `POSTGRES_PASSWORD` - Senha do banco de dados PostgreSQL
- `RABBITMQ_USER` e `RABBITMQ_PASS` - Credenciais do RabbitMQ

### Scripts de Automação

O projeto inclui scripts automatizados na raiz:

| Script | Descrição | Quando usar |
|--------|-----------|-------------|
| `setup-cicd.ps1` | Setup completo CI/CD com Azure | Para pipelines com deploy |
| `setup-ci-only.ps1` | Setup apenas CI sem custos | Para validação de código apenas |
| `run-local.sh` | Execução local com orquestração | Desenvolvimento local |

### Execução Local

#### Opção 1: .NET Aspire (Recomendado)

```bash
# Clone o repositório
git clone https://github.com/frigini/MeAjudaAi.git
cd MeAjudaAi

# Execute o AppHost do Aspire
cd src/Aspire/MeAjudaAi.AppHost
dotnet run
```

#### Opção 2: Docker Compose

```bash
# PRIMEIRO: Defina as senhas necessárias
export KEYCLOAK_ADMIN_PASSWORD=$(openssl rand -base64 32)
export RABBITMQ_PASS=$(openssl rand -base64 32)

# Execute usando Docker Compose
cd infrastructure/compose
docker compose -f environments/development.yml up -d
```

### URLs dos Serviços

> **📝 Nota**: As URLs abaixo são baseadas nas configurações em `launchSettings.json` e `docker-compose.yml`. 
> Para atualizações de portas, consulte:
> - **Aspire Dashboard**: `src/Aspire/MeAjudaAi.AppHost/Properties/launchSettings.json`
> - **API Service**: `src/Bootstrapper/MeAjudaAi.ApiService/Properties/launchSettings.json`
> - **Infraestrutura**: `infrastructure/compose/environments/development.yml`

> ⚠️ **Somente desenvolvimento**: credenciais/portas abaixo são valores locais de exemplo. Não reutilize em produção.

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Aspire Dashboard** | [https://localhost:17063](https://localhost:17063)<br/>[http://localhost:15297](http://localhost:15297) | - |
| **API Service** | [https://localhost:7524](https://localhost:7524)<br/>[http://localhost:5545](http://localhost:5545) | - |
| **Keycloak Admin** | [http://localhost:8080](http://localhost:8080) | admin/[senha gerada] |
| **PostgreSQL** | localhost:5432 | postgres/dev123 |
| **Redis** | localhost:6379 | - |
| **RabbitMQ Management** | [http://localhost:15672](http://localhost:15672) | meajudaai/[senha gerada] |

## 📁 Estrutura do Projeto

```text
MeAjudaAi/
├── src/
│   ├── Aspire/                     # Orquestração .NET Aspire
│   │   ├── MeAjudaAi.AppHost/      # Host da aplicação
│   │   └── MeAjudaAi.ServiceDefaults/ # Configurações compartilhadas
│   ├── Bootstrapper/               # API service bootstrapper
│   │   └── MeAjudaAi.ApiService/   # Ponto de entrada da API
│   ├── Modules/                    # Módulos de domínio (Clean Architecture + DDD)
│   │   ├── Users/                  # Gestão de usuários e autenticação
│   │   │   ├── API/                # Endpoints (Minimal APIs)
│   │   │   ├── Application/        # Use cases, CQRS handlers, DTOs
│   │   │   ├── Domain/             # Entidades, agregados, eventos de domínio
│   │   │   ├── Infrastructure/     # EF Core, repositórios, event handlers
│   │   │   └── Tests/              # Testes unitários e de integração
│   │   ├── Providers/              # Prestadores de serviços e verificação
│   │   ├── Documents/              # Processamento de documentos com AI
│   │   ├── ServiceCatalogs/        # Catálogo de serviços e categorias
│   │   ├── SearchProviders/        # Busca geoespacial de prestadores (PostGIS)
│   │   └── Locations/              # Integração com API IBGE (CEP, cidades)
│   └── Shared/                     # Componentes compartilhados
│       └── MeAjudaAi.Shared/       # Abstrações, contratos, utilidades
├── tests/                          # Testes de integração
├── infrastructure/                 # Infraestrutura e deployment
│   ├── compose/                    # Docker Compose
│   ├── keycloak/                   # Configuração Keycloak
│   └── database/                   # Scripts de banco de dados
└── docs/                          # Documentação
```

## 🧩 Módulos do Sistema

### 👥 Users
- **Domínio**: Gestão de usuários, perfis e autenticação
- **Features**: Registro, autenticação, perfis, RBAC (cliente, prestador, admin)
- **Tecnologias**: Keycloak OAuth2/OIDC, PostgreSQL, Event-Driven
- **Comunicação**: Module API pattern para validação cross-module

### 🏢 Providers
- **Domínio**: Prestadores de serviços e processo de verificação
- **Features**: Cadastro, perfis empresariais, documentos, qualificações, status de verificação
- **Eventos**: Domain Events + Integration Events para auditoria e comunicação
- **Arquitetura**: Clean Architecture, CQRS, DDD, Event Sourcing

### 📄 Documents
- **Domínio**: Processamento e validação de documentos
- **Features**: Upload, OCR com Azure Document Intelligence, validação, armazenamento (Azure Blob)
- **AI/ML**: Extração automática de dados de documentos (CNH, RG, CPF)
- **Integração**: Azure Storage, eventos para notificação de processamento

### 📋 ServiceCatalogs
- **Domínio**: Catálogo de serviços e categorias
- **Features**: CRUD de serviços/categorias, ativação/desativação, hierarquia de categorias
- **Testes**: 141 testes (100% passing), cobertura 26% Domain, 50% Infrastructure
- **Otimização**: Testes paralelos desabilitados para evitar conflitos de chave única

### 🔍 SearchProviders
- **Domínio**: Busca geoespacial de prestadores
- **Features**: Busca por coordenadas/raio, filtros (serviços, rating), paginação
- **Tecnologias**: PostGIS para queries espaciais, PostgreSQL 16 com extensão PostGIS 3.4
- **Performance**: Índices GiST para consultas geoespaciais otimizadas

### 📍 Locations
- **Domínio**: Integração com dados geográficos brasileiros
- **Features**: Consulta de CEP, cidades, estados via API IBGE
- **Validação**: Middleware de restrição geográfica (ex: disponível apenas RJ)
- **Caching**: Redis para otimizar consultas frequentes

---

## 🎨 Admin Portal (NEW - Sprint 6)

### Blazor WebAssembly + Fluxor + MudBlazor

Portal administrativo moderno para gestão da plataforma MeAjudaAi.

**Stack Tecnológica:**
- **Blazor WebAssembly**: .NET 10 SPA client-side
- **MudBlazor 7.21.0**: Material Design UI components
- **Fluxor 6.1.0**: Redux-pattern state management
- **Refit 9.0.2**: Type-safe HTTP clients
- **Keycloak OIDC**: Authentication via Authorization Code flow

**Funcionalidades Implementadas (Sprint 6):**
- ✅ **Autenticação**: Login/Logout via Keycloak OIDC
- ✅ **Dashboard**: 3 KPIs (Total Providers, Pending Verifications, Active Services)
- ✅ **Providers Management**: Listagem paginada (read-only)
- ✅ **Dark Mode**: Toggle com Fluxor state management
- ✅ **Portuguese Localization**: UI completa em português

**Como Executar:**

```powershell
# Via Aspire AppHost (recomendado)
cd src/Aspire/MeAjudaAi.AppHost
dotnet run

# Acessar: https://localhost:7281
# Login: admin.portal / admin123 (após criar client no Keycloak)
```

**Configuração Keycloak:**

Siga o guia completo em [docs/keycloak-admin-portal-setup.md](docs/keycloak-admin-portal-setup.md) para criar o client `admin-portal` no realm `meajudaai`.

**Testes:**

```powershell
# Executar testes bUnit
dotnet test tests/MeAjudaAi.Web.Admin.Tests

# 10 testes: ProvidersPage, Dashboard, DarkMode
```

**Estrutura:**

```text
src/Web/MeAjudaAi.Web.Admin/
├── Pages/                # Razor pages (Dashboard, Providers, Authentication)
├── Features/             # Fluxor stores (Providers, Dashboard, Theme)
├── Layout/               # MainLayout, NavMenu
└── wwwroot/              # appsettings.json, static assets

tests/MeAjudaAi.Web.Admin.Tests/
├── Pages/                # bUnit component tests
└── Layout/               # Layout component tests
```

**Próximos Passos (Sprint 7):**
- CRUD completo de Providers (create, update, delete, verify)
- Gestão de Documentos (upload, verificação)
- Gestão de Service Catalogs (categorias + serviços)
- Gráficos Dashboard (providers por status, evolução temporal)

📖 **Documentação Completa**: [Frontend Architecture](docs/architecture.md#frontend-architecture-sprint-6)

---

### 🔮 Roadmap - Próximos Módulos
- **Bookings**: Agendamentos e reservas
- **Payments**: Processamento de pagamentos (Stripe/PagSeguro)
- **Reviews**: Avaliações, feedback e rating de prestadores
- **Notifications**: Sistema de notificações multi-canal (email, SMS, push)

## ⚡ Melhorias Recentes

### 🆔 UUID v7 Implementation
- **Migração completa** de UUID v4 para UUID v7 (.NET 10)
- **Performance melhorada** com ordenação temporal nativa
- **Compatibilidade PostgreSQL 18** para melhor indexação
- **UuidGenerator centralizado** em `MeAjudaAi.Shared.Time`

### 🔌 Module APIs Pattern  
- **Comunicação inter-módulos** via interfaces tipadas
- **In-process performance** sem overhead de rede
- **Type safety** com compile-time checking
- **Exemplo**: `IUsersModuleApi` para validação de usuários em outros módulos

```csharp
// Exemplo de uso da Module API
public class OrderValidationService
{
    private readonly IUsersModuleApi _usersApi;
    
    public async Task<bool> ValidateOrder(Guid userId)
    {
        var userExists = await _usersApi.UserExistsAsync(userId);
        return userExists.IsSuccess && userExists.Value;
    }
}
```

## 🛠️ Desenvolvimento

### Executar Testes

```bash
# Todos os testes
dotnet test

# Testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Testes de um módulo específico
dotnet test src/Modules/Users/Tests/
```

### Padrões de Código

- **Commands/Queries**: Implementar padrão CQRS
- **Domain Events**: Eventos de domínio para comunicação interna
- **Integration Events**: Eventos para comunicação entre módulos
- **Value Objects**: Para conceitos de domínio imutáveis
- **Aggregates**: Para consistência transacional

#### Implementação de Eventos - Módulo Providers

O módulo Providers implementa um sistema completo de eventos para comunicação inter-modular:

**Domain Events:**
- `ProviderRegisteredDomainEvent` - Novo prestador cadastrado
- `ProviderDeletedDomainEvent` - Prestador removido do sistema
- `ProviderVerificationStatusUpdatedDomainEvent` - Status de verificação alterado
- `ProviderProfileUpdatedDomainEvent` - Perfil do prestador atualizado

**Integration Events:**
- Conversão automática via Domain Event Handlers
- Publicação em message bus para outros módulos
- Suporte completo a event sourcing e auditoria

### Estrutura de Commits

```bash
feat(users): adicionar endpoint de criação de usuário
fix(auth): corrigir validação de token JWT
docs(readme): atualizar guia de instalação
test(users): adicionar testes de integração
```

## 🔧 Configuração de CI/CD

### GitHub Actions Setup

O projeto possui pipelines automatizadas que executam em PRs e pushes para as branches principais.

#### 1. **Configure as Credenciais Azure**

```powershell
# Execute o script de setup (requer Azure CLI)
.\setup-cicd.ps1 -SubscriptionId "your-subscription-id"
```

**O que este script faz:**
- ✅ Cria um Service Principal no Azure com role `Contributor`
- ✅ Gera as credenciais JSON necessárias para o GitHub
- ✅ Salva as credenciais em `azure-credentials.json`

#### 2. **Configure o GitHub Repository**

**Secrets necessários** (`Settings > Secrets and variables > Actions`):

| Secret Name | Valor | Descrição |
|-------------|-------|-----------|
| `AZURE_CREDENTIALS` | JSON gerado pelo script | Credenciais do Service Principal |

**Environments recomendados** (`Settings > Environments`):
- `development`
- `production`

#### 3. **Pipeline Automática**

✅ **A pipeline executa automaticamente quando você:**
- Abrir um PR para `master` ou `develop`
- Fazer push para essas branches

✅ **O que a pipeline faz:**
- Build da solução .NET 10
- Execução de testes unitários
- Validação da configuração Aspire
- Verificações de qualidade de código
- Containerização (quando habilitada)

#### 4. **Alternativa Apenas CI (Sem Deploy)**

Se quiser apenas CI sem custos Azure:

```powershell
# Setup apenas para build/test (sem deploy)
.\setup-ci-only.ps1
```

💰 **Custo**: ~$0 (apenas validação, sem recursos Azure)

## 🌐 Deploy em Produção

### Azure Container Apps

```bash
# Autenticar no Azure
azd auth login

# Deploy completo (infraestrutura + aplicação)
azd up

# Deploy apenas da aplicação
azd deploy

# Deploy apenas da infraestrutura
azd provision
```

### Recursos Azure Provisionados

- **Container Apps Environment**: Hospedagem da aplicação
- **PostgreSQL Flexible Server**: Banco de dados principal
- **Service Bus Standard**: Sistema de messaging
- **Container Registry**: Registro de imagens
- **Key Vault**: Gerenciamento de segredos
- **Application Insights**: Monitoramento e telemetria

**💰 Custo Estimado**: ~$10-30 USD/mês por environment

## 🧪 Testes

### Estratégia de Testes

- **Unit Tests**: Testes de domínio e lógica de negócio
- **Integration Tests**: Testes com banco de dados e serviços externos
- **E2E Tests**: Testes completos de fluxos de usuário
- **Contract Tests**: Validação de contratos entre módulos

### Mocks e Doubles

- **MockServiceBusMessageBus**: Mock do Azure Service Bus
- **MockRabbitMqMessageBus**: Mock do RabbitMQ  
- **TestContainers**: Containers para testes de integração
- **InMemory Database**: Banco em memória para testes rápidos

## 📚 Documentação

- [**Guia de Infraestrutura**](docs/infrastructure.md) - Setup e deploy
- [**Arquitetura e Padrões**](docs/architecture.md) - Decisões arquiteturais
- [**Guia de Desenvolvimento**](docs/development.md) - Convenções e práticas
- [**CI/CD**](docs/ci-cd.md) - Pipeline de integração contínua
- [**Diretrizes de Desenvolvimento**](docs/development-guidelines.md) - Padrões e boas práticas

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'feat: adicionar AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 📞 Contato

- **Desenvolvedor**: [frigini](https://github.com/frigini)
- **Projeto**: [MeAjudaAi](https://github.com/frigini/MeAjudaAi)

---

⭐ Se este projeto te ajudou, considere dar uma estrela!

### Aplicar migrations (módulo específico)

```bash
dotnet ef database update --context UsersDbContext
```

### Adding New Modules
1. Create module structure following Users module pattern
2. Add new schema and role in `infrastructure/database/schemas/`
3. Configure dedicated connection string in appsettings
4. Register module services in `Program.cs`

## 🔒 Security Features

- **Authentication**: Keycloak integration with role-based access
- **Authorization**: Policy-based authorization per endpoint
- **Database**: Role-based access control per schema
- **API**: Rate limiting and request validation
- **Secrets**: Azure Key Vault integration for production

## 🚢 Deployment Environments

### Development
- **Local**: `dotnet run` (Aspire orchestration)
- **Database**: PostgreSQL container with auto-schema setup
- **Authentication**: Local Keycloak with realm auto-import

### Production
- **Platform**: Azure Container Apps
- **Database**: Azure PostgreSQL Flexible Server
- **Authentication**: Azure-hosted Keycloak
- **Monitoring**: Application Insights + OpenTelemetry

## 🧪 Testing Strategy

- **Unit Tests**: Domain logic and business rules
- **Integration Tests**: API endpoints and database operations with TestContainers
- **Module Tests**: Cross-boundary communication via events
- **E2E Tests**: Full user scenarios via API

### Testing Infrastructure

```bash
# Start testing services (separate from development)
cd infrastructure/compose
docker compose -f environments/testing.yml up -d

# Test services run on alternate ports:
# - PostgreSQL: localhost:5433 (postgres/test123)
# - Keycloak: localhost:8081 (admin/admin) - version pinned for reproducibility
# - Redis: localhost:6380 (no auth)
```

**Reproducible Testing**: All service versions are pinned (no `:latest` tags) to ensure consistent test results across different environments and time periods.

**TestContainers**: Integration tests use TestContainers with:
- **PostgreSQL + PostGIS**: `postgis/postgis:15-3.4` for geographic data support
- **Redis**: For cache and distributed lock testing
- **Automatic cleanup**: Containers are disposed after each test class

**Test Organization** (Sprint 5.5 - Dec 2025)
- `MeAjudaAi.Shared.Tests/TestInfrastructure/`: 8 organized subfolders (Base, Builders, Configuration, Containers, Fixtures, Handlers, Mocks, Options, Services)
- Module-specific tests moved to individual modules (Documents, Providers, ServiceCatalogs, Users)
- Comprehensive AAA pattern and translated documentation

## 📈 Monitoring & Observability

- **Metrics**: OpenTelemetry with Prometheus
- **Logging**: Structured logging with Serilog
- **Tracing**: Distributed tracing across modules
- **Health Checks**: Custom health checks per module

## 🆘 Troubleshooting

### Problemas Comuns

**"Pipeline não executa no PR"**
- ✅ Verifique se o secret `AZURE_CREDENTIALS` está configurado
- ✅ Confirme que a branch é `master` ou `develop`

**"Azure deployment failed"**
- ✅ Execute `az login` para verificar autenticação
- ✅ Verifique se o Service Principal tem permissões `Contributor`

**"Docker containers conflicting"**
- ✅ Execute `make clean-docker` (via `./build/Makefile`) para limpar containers
- ✅ Use `docker system prune -a` para limpeza completa

### Links Úteis

- 📚 [Documentação Técnica](https://frigini.github.io/MeAjudaAi/)
- 🏗️ [Guia de Infraestrutura](infrastructure/README.md)
- 🔄 [Setup de CI/CD Detalhado](docs/ci-cd.md)
- 🐛 [Issues e Bugs](https://github.com/frigini/MeAjudaAi/issues)

## 🤝 Contributing

1. Create a feature branch from `develop`
2. Follow existing patterns and naming conventions
3. Add tests for new functionality
4. Update documentation as needed
5. Open PR to `develop` branch