# MeAjudaAi

Uma plataforma abrangente de serviços construída com .NET Aspire, projetada para conectar prestadores de serviços com clientes usando arquitetura monólito modular.

<!-- Atualizado: 24 Março 2026 - Sprint 8D (Admin Portal Migration: Blazor → React) -->

## 🎯 Visão Geral

O **MeAjudaAi** é uma plataforma moderna de marketplace de serviços que implementa as melhores práticas de desenvolvimento, incluindo Domain-Driven Design (DDD), CQRS, e arquitetura de monólito modular. A aplicação utiliza tecnologias de ponta como .NET 10, Azure, e containerização com Docker.

### 🏗️ Arquitetura

- **Monólito Modular**: Separação clara de responsabilidades por módulos de domínio
- **Domain-Driven Design (DDD)**: Modelagem rica de domínio com agregados, entidades e value objects
- **CQRS**: Separação de comandos e consultas para melhor performance e escalabilidade
- **Event-Driven**: Comunicação entre módulos através de eventos de domínio e integração
- **Clean Architecture**: Separação em camadas com inversão de dependências

### 🚀 Tecnologias Principais

- **.NET 10.0.2** - Framework principal
- **.NET Aspire 13.1** - Orquestração e observabilidade
- **React 19 + Next.js 15** - Frontend Web Apps (Customer, Provider, Admin)
- **Tailwind CSS v4** - Styling
- **Zustand + TanStack Query** - State management
- **Playwright** - E2E Testing
- **Entity Framework Core 10.0.2** - ORM e persistência
- **Microsoft.OpenApi 2.6.1** - OpenAPI specification
- **SonarAnalyzer.CSharp 10.19.0** - Code quality analysis
- **PostgreSQL 16** - Banco de dados principal
- **Keycloak 26.0.2** - Autenticação OAuth2/OIDC
- **Redis 7** - Cache distribuído
- **RabbitMQ 3** / **Azure Service Bus** - Messaging
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

- **Primeiros Passos**: [development.md](docs/development.md) - Setup de desenvolvimento e configuração
- **Arquitetura**: [architecture.md](docs/architecture.md) - Design do sistema, padrões DDD/CQRS
- **Infraestrutura**: [infrastructure.md](docs/infrastructure.md) - Docker, Azure, deployment
- **Módulos**: [docs/modules/](docs/modules/) - Documentação por módulo de domínio
- **Autenticação**: [authentication-and-authorization.md](docs/authentication-and-authorization.md) - Keycloak OIDC
- **CI/CD**: [ci-cd.md](docs/ci-cd.md) - Pipelines GitHub Actions
- **Testes**: [docs/testing/](docs/testing/) - Estratégias, guias e cobertura
- **API**: [api-automation.md](docs/api-automation.md) - Geração de clientes REST
- **Segurança**: [docs/security/](docs/security/) - CSP, vulnerabilidades, configuração segura
- **Roadmap**: [roadmap.md](docs/roadmap.md) - Sprints e planejamento

## 📦 Estrutura do Projeto

O projeto foi organizado para facilitar navegação e manutenção:

```text
📦 MeAjudaAi/
├── 📁 api/              # Especificações OpenAPI (api-spec.json)
├── 📁 automation/       # Automações de repositório (.github workflows)
├── 📁 build/           # Scripts Unix (Makefile, dotnet-install.sh)
├── 📁 config/          # Configurações de ferramentas
│   ├── coverage.runsettings  # Configuração de coverage
│   ├── coverlet.json        # Exclusões de cobertura
│   └── lychee.toml         # Link checker config
├── 📁 docs/            # Documentação técnica (MkDocs Material)
│   ├── architecture.md      # Arquitetura DDD/CQRS
│   ├── development.md       # Guia de desenvolvimento
│   ├── infrastructure.md    # Setup Docker/Azure
│   ├── modules/            # Docs por módulo de domínio
│   ├── testing/            # Estratégias de testes
│   ├── security/           # CSP, vulnerabilidades
│   └── roadmap.md          # Planejamento de sprints
├── 📁 infrastructure/  # Infraestrutura como código
│   ├── automation/     # Scripts CI/CD Azure
│   ├── compose/        # Docker Compose (dev/test)
│   ├── database/       # Init scripts + seeds SQL
│   ├── keycloak/       # Keycloak realms + setup automatizado
│   └── rabbitmq/       # RabbitMQ definitions
├── 📁 scripts/         # Scripts PowerShell de desenvolvimento
├── 📁 src/             # Código fonte da aplicação
│   ├── Aspire/         # .NET Aspire AppHost
│   ├── Bootstrapper/   # API Service entry point
│   ├── Modules/        # Módulos de domínio (DDD)
│   ├── Shared/         # Contratos e abstrações
│   └── Web/            # Aplicações Web (NX Workspace)
│       ├── MeAjudaAi.Web.Admin/     # Admin Portal (React + Next.js 15)
│       ├── MeAjudaAi.Web.Customer/ # Customer Web App (Next.js 15)
│       └── MeAjudaAi.Web.Provider/  # Provider Web App (Next.js 15)
├── 📁 tests/           # Testes automatizados (xUnit v3)
└── 📁 tools/           # Ferramentas de desenvolvimento
    └── api-collections/  # Gerador Bruno/Postman collections
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

### ⚡ Setup Automatizado (Primeira Vez)

```powershell
# 1. Iniciar desenvolvimento (detecta primeira execução e faz setup automático)
.\scripts\dev.ps1
```

**Pronto!** 🎉 Acesse os serviços em desenvolvimento:

| Serviço | URL | Credenciais | Descrição |
|---------|-----|-------------|--------------|
| **Aspire Dashboard** | https://localhost:17063/ | - | Orquestração e observabilidade |
| **Admin Portal** | https://localhost:7032/ | admin.portal/admin123 | Portal administrativo Blazor |
| **Customer Web App** | http://localhost:3000/ | - | Aplicação pública Next.js (clientes/prestadores) |
| **API** | https://localhost:7524/swagger | - | API REST com Swagger UI |
| **Keycloak** | http://localhost:8080/ | admin/[console logs] | Autenticação OAuth2/OIDC |
| **PostgreSQL** | localhost:5432 | postgres/[gerada] | Banco de dados |
| **Redis** | localhost:6379 | - | Cache distribuído |
| **RabbitMQ** | http://localhost:15672/ | meajudaai/[gerada] | Message broker |

> ⚠️ **Ambiente local**: Credenciais/portas acima são valores de desenvolvimento. **Não reutilize em produção.**

### 🔄 Uso Diário

```powershell
# Iniciar desenvolvimento
.\scripts\dev.ps1

# Executar testes
dotnet test

# Ver logs da aplicação
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```

### 🔧 Configuração Keycloak (Automatizada)

**Configuração totalmente automática!** O AppHost configura Keycloak no startup:

- ✅ Realm `meajudaai` criado automaticamente
- ✅ Clients OIDC (admin-portal + customer-app)
- ✅ Roles (admin, customer, operator, viewer)
- ✅ Usuários demo: admin.portal/admin123, customer.demo/customer123

👉 Detalhes: [docs/keycloak-admin-portal-setup.md](docs/keycloak-admin-portal-setup.md)

### 🧪 Executar Testes

```powershell
# Todos os testes (unit + integration)
dotnet test

# Com cobertura de código
dotnet test /p:CollectCoverage=true

# Testes de um módulo específico
dotnet test tests/MeAjudaAi.Modules.Users.Tests/
```

### Pré-requisitos

| Ferramenta | Versão | Link |
|------------|--------|------|
| **.NET SDK** | 10.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **Docker Desktop** | Latest | [Download](https://www.docker.com/products/docker-desktop) |
| **Git** | Latest | [Download](https://git-scm.com/) |
| Azure CLI (opcional) | Latest | Para deploy em produção |

✅ **Verificar instalação**: Execute `.\scripts\setup.ps1` que valida tudo automaticamente.

### 🛠️ Scripts Disponíveis

| Script | Descrição | Uso |
|--------|-----------|-----|
| **`scripts/setup.ps1`** | Setup inicial completo | Primeira vez no projeto |
| **`scripts/dev.ps1`** | Iniciar desenvolvimento | Uso diário |
| `scripts/ef-migrate.ps1` | Entity Framework migrations | Gerenciar banco de dados |
| `scripts/seed-dev-data.ps1` | Popular dados de teste | Ambiente de desenvolvimento |
| `scripts/export-openapi.ps1` | Exportar especificação API | Gerar documentação/clientes |

**Automação CI/CD** (em `infrastructure/automation/`):
- `setup-cicd.ps1` - Setup completo CI/CD com Azure
- `setup-ci-only.ps1` - Setup apenas CI sem deploy

**Makefile** (em `build/Makefile`):
- `make help` - Ver todos os comandos disponíveis
- `make dev` - Iniciar desenvolvimento
- `make test` - Executar testes
- `make clean` - Limpar artefatos

---

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

## 🎨 Admin Portal

**Portal administrativo** React + Next.js para gestão completa da plataforma.

**Funcionalidades:**
- ✅ Autenticação via Keycloak OIDC (Authorization Code + PKCE)
- ✅ Dashboard com KPIs e gráficos interativos
- ✅ Gestão de Providers (CRUD completo + verificação)
- ✅ Gestão de Documentos (upload, OCR, verificação)
- ✅ Gestão de Service Catalogs (categorias + serviços)
- ✅ Restrições Geográficas (cidades permitidas)
- ✅ Admin Portal React com Tailwind CSS
- ✅ E2E Tests com Playwright

**Como Executar:**

```powershell
# Via Aspire (recomendado)
.\scripts\dev.ps1
# Acessar: https://localhost:7032/
```

---

## 🌐 Customer Web App

**Aplicação pública** Next.js 15 para clientes e prestadores de serviços.

**Stack Tecnológico:**
- ⚛️ **React 19** (Server + Client Components)
- 🔄 **Next.js 15** (App Router, SSR/SSG)
- 🎨 **Tailwind CSS v4** (Design System do Figma)
- 🔐 **NextAuth.js v5** (Autenticação via Keycloak)
- 📝 **TypeScript 5.7+** (Strict mode)
- 🎯 **Tailwind Variants** (Component variants)
- 🪝 **React Hook Form + Zod** (Formulários e validação)
- 🎭 **Lucide React** (Ícones)

**Funcionalidades Implementadas:**
- ✅ Design System completo (cores do Figma: #355873 azul, #D06704 laranja)
- ✅ Componentes base (Button, Card, Input, Badge, Rating, Avatar)
- ✅ Layout (Header com busca, Footer com missão/visão/valores)
- ✅ Home page (Hero, "Como funciona?", CTA prestadores)
- ✅ Busca de prestadores (/buscar) com filtros
- ✅ Perfil de prestador (/prestador/[id]) com avaliações
- ✅ Integração com Aspire (orquestração automática)
- ✅ Acessibilidade (ARIA labels, htmlFor/id associations)

**Como Executar:**

```powershell
# Via Aspire (recomendado - inicia tudo automaticamente)
.\scripts\dev.ps1
# Acessar: http://localhost:3000/

# Ou manualmente (apenas Next.js)
cd src/Web/MeAjudaAi.Web.Customer
npm install
npm run dev
```

**Estrutura:**
```text
src/Web/meajudaai-web-customer/
├── app/                    # Next.js App Router
│   ├── layout.tsx          # Root layout (Header + Footer)
│   ├── page.tsx            # Home page
│   ├── buscar/             # Search page
│   └── prestador/[id]/     # Provider profile
├── components/
│   ├── ui/                 # Base components (Button, Card, Input...)
│   ├── layout/             # Header, Footer
│   ├── providers/          # ProviderCard, ProviderGrid
│   └── reviews/            # Review components
├── lib/
│   └── utils/              # Utilities (cn helper)
├── types/
│   └── api/                # TypeScript types (will be auto-generated)
└── globals.css             # Tailwind v4 + Design tokens
```

**Próximos Passos:**
- [ ] NextAuth.js + Keycloak integration
- [ ] OpenAPI TypeScript generator (auto-generate types from backend)
- [ ] API client with authentication
- [ ] Protected routes
- [ ] Edit profile page
- [ ] Login/Cadastro pages

👉 Detalhes: [docs/customer-web-app.md](docs/customer-web-app.md) *(a ser criado)*

---

📖 **Documentação**: [docs/architecture.md](docs/architecture.md) | [docs/modules/admin-portal.md](docs/modules/admin-portal.md)

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
- **Compatibilidade PostgreSQL 16** para melhor indexação
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

### Padrões de Código

- **CQRS**: Commands/Queries separados para write/read
- **Domain Events**: Comunicação interna no módulo
- **Integration Events**: Comunicação entre módulos via message bus
- **Value Objects**: Conceitos imutáveis (Email, CPF, Address)
- **Aggregates**: Consistência transacional (Provider, User, Document)
- **Result Pattern**: Tratamento de erros funcional (sem exceptions)

### Commits Convencionais

```bash
feat(module): adicionar nova funcionalidade
fix(module): corrigir bug
docs: atualizar documentação
test(module): adicionar/atualizar testes
refactor(module): refatorar código
perf(module): melhoria de performance
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

## 🔒 Segurança

- **Autenticação**: Keycloak OAuth2/OIDC com RBAC
- **Autorização**: Policy-based por endpoint
- **Database**: Isolamento por schema com roles dedicados
- **API**: Rate limiting e validação de requests
- **Secrets**: Azure Key Vault (produção) + User Secrets (dev)
- **CSP**: Content Security Policy configurado
- **Vulnerabilidades**: Auditoria automatizada de pacotes NuGet

## 🚢 Deploy

### Desenvolvimento Local
```powershell
.\scripts\dev.ps1  # Aspire orchestration
```

### Produção (Azure Container Apps)
```bash
azd auth login
azd up  # Provisiona infraestrutura + deploy
```

**Recursos provisionados**: Container Apps, PostgreSQL Flexible Server, Service Bus, Container Registry, Key Vault, Application Insights.

💰 **Custo estimado**: ~$10-30 USD/mês por ambiente.

## 🆘 Troubleshooting

**Pipeline não executa no PR:**
- Verifique secret `AZURE_CREDENTIALS` em Settings > Secrets
- Confirme que a branch é `master` ou `develop`

**Keycloak não inicia:**
- Execute `docker logs keycloak` para ver logs
- Verifique porta 8080 disponível: `netstat -ano | findstr :8080`

**Testes falhando:**
- Limpe containers: `docker compose -f infrastructure/compose/environments/testing.yml down -v`
- Rebuild: `dotnet build --no-incremental`

### Links Úteis

- 📚 [Documentação Online](https://frigini.github.io/MeAjudaAi/)
- 🏗️ [Infraestrutura](infrastructure/README.md)
- 🔄 [CI/CD](docs/ci-cd.md)
- 🐛 [Issues](https://github.com/frigini/MeAjudaAi/issues)

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch: `git checkout -b feature/MinhaFeature`
3. Commit: `git commit -m 'feat(module): adicionar MinhaFeature'`
4. Push: `git push origin feature/MinhaFeature`
5. Abra um Pull Request para `develop`

## 📄 Licença

MIT License - veja [LICENSE](LICENSE) para detalhes.

## 📞 Contato

- **Desenvolvedor**: [frigini](https://github.com/frigini)
- **Repositório**: [github.com/frigini/MeAjudaAi](https://github.com/frigini/MeAjudaAi)

---

⭐ Se este projeto te ajudou, considere dar uma estrela!