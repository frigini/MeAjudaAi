# MeAjudaAi

Uma plataforma abrangente de serviços construída com .NET Aspire, projetada para conectar prestadores de serviços com clientes usando arquitetura monólito modular.

## 🎯 Visão Geral

O **MeAjudaAi** é uma plataforma moderna de marketplace de serviços que implementa as melhores práticas de desenvolvimento, incluindo Domain-Driven Design (DDD), CQRS, e arquitetura de monólito modular. A aplicação utiliza tecnologias de ponta como .NET 9, Azure, e containerização com Docker.

### 🏗️ Arquitetura

- **Monólito Modular**: Separação clara de responsabilidades por módulos de domínio
- **Domain-Driven Design (DDD)**: Modelagem rica de domínio com agregados, entidades e value objects
- **CQRS**: Separação de comandos e consultas para melhor performance e escalabilidade
- **Event-Driven**: Comunicação entre módulos através de eventos de domínio e integração
- **Clean Architecture**: Separação em camadas com inversão de dependências

### 🚀 Tecnologias Principais

- **.NET 9** - Framework principal
- **.NET Aspire** - Orquestração e observabilidade
- **Entity Framework Core** - ORM e persistência
- **PostgreSQL** - Banco de dados principal
- **Keycloak** - Autenticação e autorização
- **Redis** - Cache distribuído
- **RabbitMQ/Azure Service Bus** - Messaging
- **Docker** - Containerização
- **Azure** - Hospedagem em nuvem

## 🚀 Início Rápido

### Para Desenvolvedores

**Setup completo (recomendado):**
```bash
./run-local.sh setup
```

**Execução rápida:**
```bash
./run-local.sh run
```

**Modo interativo:**
```bash
./run-local.sh
```

### Para Testes

```bash
# Todos os testes
./test.sh all

# Apenas unitários
./test.sh unit

# Com relatório de cobertura
./test.sh coverage
```

📖 **[Guia Completo de Desenvolvimento](docs/development_guide.md)**

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
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

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Aspire Dashboard** | https://localhost:15888 | - |
| **API Service** | https://localhost:7032 | - |
| **Keycloak Admin** | http://localhost:8080 | admin/[senha gerada] |
| **PostgreSQL** | localhost:5432 | postgres/dev123 |
| **Redis** | localhost:6379 | - |
| **RabbitMQ Management** | http://localhost:15672 | meajudaai/[senha gerada] |

## 📁 Estrutura do Projeto

```
MeAjudaAi/
├── src/
│   ├── Aspire/                     # Orquestração .NET Aspire
│   │   ├── MeAjudaAi.AppHost/      # Host da aplicação
│   │   └── MeAjudaAi.ServiceDefaults/ # Configurações compartilhadas
│   ├── Bootstrapper/               # API service bootstrapper
│   │   └── MeAjudaAi.ApiService/   # Ponto de entrada da API
│   ├── Modules/                    # Módulos de domínio
│   │   └── Users/                  # Módulo de usuários
│   │       ├── API/                # Endpoints e controllers
│   │       ├── Application/        # Use cases e handlers CQRS
│   │       ├── Domain/             # Entidades, value objects, eventos
│   │       ├── Infrastructure/     # Persistência e serviços externos
│   │       └── Tests/              # Testes do módulo
│   └── Shared/                     # Componentes compartilhados
│       └── MeAjudaAi.Shared/       # Abstrações e utilities
├── tests/                          # Testes de integração
├── infrastructure/                 # Infraestrutura e deployment
│   ├── compose/                    # Docker Compose
│   ├── keycloak/                   # Configuração Keycloak
│   └── database/                   # Scripts de banco de dados
└── docs/                          # Documentação
```

## 🧩 Módulos do Sistema

### 📱 Módulo Users
- **Domain**: Gestão de usuários, perfis e autenticação
- **Features**: Registro, login, perfis, papéis (cliente, prestador, admin)
- **Integração**: Keycloak para autenticação OAuth2/OIDC

### 🔮 Módulos Futuros
- **Services**: Catálogo de serviços e categorias
- **Bookings**: Agendamentos e reservas
- **Payments**: Processamento de pagamentos
- **Reviews**: Avaliações e feedback
- **Notifications**: Sistema de notificações

## ⚡ Melhorias Recentes

### 🆔 UUID v7 Implementation
- **Migração completa** de UUID v4 para UUID v7 (.NET 9)
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
- Abrir um PR para `main` ou `develop`
- Fazer push para essas branches

✅ **O que a pipeline faz:**
- Build da solução .NET 9
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
- [**Guia de Desenvolvimento**](docs/development_guide.md) - Convenções e práticas
- [**CI/CD**](docs/ci_cd.md) - Pipeline de integração contínua
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

# Apply migrations for specific module
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
- **Integration Tests**: API endpoints and database operations
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

## 📈 Monitoring & Observability

- **Metrics**: OpenTelemetry with Prometheus
- **Logging**: Structured logging with Serilog
- **Tracing**: Distributed tracing across modules
- **Health Checks**: Custom health checks per module

## 🆘 Troubleshooting

### Problemas Comuns

**"Pipeline não executa no PR"**
- ✅ Verifique se o secret `AZURE_CREDENTIALS` está configurado
- ✅ Confirme que a branch é `main` ou `develop`

**"Azure deployment failed"**
- ✅ Execute `az login` para verificar autenticação
- ✅ Verifique se o Service Principal tem permissões `Contributor`

**"Docker containers conflicting"**
- ✅ Execute `make clean-docker` para limpar containers
- ✅ Use `docker system prune -a` para limpeza completa

### Links Úteis

- 📚 [Documentação Técnica](docs/README.md)
- 🏗️ [Guia de Infraestrutura](infrastructure/README.md)
- 🔄 [Setup de CI/CD Detalhado](docs/ci_cd.md)
- 🐛 [Issues e Bugs](https://github.com/frigini/MeAjudaAi/issues)

## 🤝 Contributing

1. Create a feature branch from `develop`
2. Follow existing patterns and naming conventions
3. Add tests for new functionality
4. Update documentation as needed
5. Open PR to `develop` branch