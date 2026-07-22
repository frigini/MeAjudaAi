# MeAjudaAi

Uma plataforma abrangente de serviços construída com .NET Aspire, projetada para conectar prestadores de serviços com clientes usando uma arquitetura de monólito modular.

<!-- Atualizado: 22 JULHO 2026 - Sprint 14.6 -->

## 🎯 Visão Geral

O **MeAjudaAi** é uma plataforma moderna de marketplace de serviços que implementa as melhores práticas de desenvolvimento, incluindo Domain-Driven Design (DDD), CQRS, e arquitetura de monólito modular. A aplicação utiliza tecnologias de ponta como .NET 10, Azure e containerização com Docker, orquestrados através do .NET Aspire.

### 🏗️ Arquitetura

- **Monólito Modular**: Separação clara de responsabilidades por módulos de domínio independentes e desacoplados.
- **Domain-Driven Design (DDD)**: Modelagem rica de domínio com agregados, entidades, value objects e eventos de domínio.
- **CQRS**: Separação de comandos (escrita) e consultas (leitura) para melhor performance e escalabilidade.
- **Event-Driven**: Comunicação assíncrona entre módulos através de eventos de integração utilizando RabbitMQ (desenvolvimento) ou Azure Service Bus (produção).
- **Clean Architecture**: Separação em camadas com inversão de dependências em cada módulo.

### 🚀 Tecnologias Principais

- **.NET 10.0.2** - Framework principal do backend
- **.NET Aspire 13.1** - Orquestração de recursos, dependências e observabilidade local
- **React 19 + Next.js 15** - Três Web Apps dedicados estruturados em um monorepo NX (Customer, Provider, Admin)
- **Tailwind CSS v4** - Sistema de estilização moderno e otimizado com design tokens customizados
- **Zustand + TanStack Query** - Gerenciamento de estado e requisições no frontend
- **Entity Framework Core 10.0.2** - ORM e persistência de dados
- **PostgreSQL 16 + PostGIS 3.4** - Banco de dados relacional com extensões geoespaciais
- **Keycloak 26.0.2** - Autenticação e autorização centralizada via OAuth2/OIDC
- **Redis 7** - Cache distribuído para performance e otimização de consultas
- **RabbitMQ 3** / **Azure Service Bus** - Message broker para comunicação baseada em eventos
- **Playwright** - Testes ponta a ponta (E2E) para os Web Apps
- **Vitest** - Testes unitários para o ecossistema frontend
- **xUnit v3** - Framework de testes unitários e de integração para .NET

---

## 📦 Estrutura do Projeto

O repositório está organizado de forma modular e clara:

```text
📦 MeAjudaAi/
├── 📁 api/              # Especificações OpenAPI (api-spec.json)
├── 📁 automation/       # Automações de repositório e CI/CD (.github workflows)
├── 📁 build/           # Scripts Unix e automações de build (Makefile, dotnet-install.sh)
├── 📁 config/          # Configurações globais de ferramentas (linting, coverage, etc.)
├── 📁 docs/            # Documentação técnica completa em MkDocs Material
├── 📁 infrastructure/  # Infraestrutura como código (Bicep, Docker Compose, Keycloak realm)
├── 📁 src/             # Código fonte da aplicação
│   ├── Aspire/         # Projetos do .NET Aspire (.AppHost e .ServiceDefaults)
│   ├── Bootstrapper/   # Pontos de entrada da API e Gateway
│   │   ├── MeAjudaAi.ApiService/  # API unificada dos módulos
│   │   └── MeAjudaAi.Gateway/     # API Gateway baseado em YARP para roteamento
│   ├── Modules/        # Módulos de domínio (Monólito Modular)
│   ├── Shared/         # Abstrações e utilitários compartilhados (.NET)
│   └── Web/            # Frontend (NX Monorepo contendo Admin, Customer e Provider)
├── 📁 tests/           # Projetos de testes automatizados (.NET e E2E)
└── 📁 tools/           # Ferramentas auxiliares de desenvolvimento
```

---

## 🧩 Módulos do Sistema

A plataforma é dividida em 10 módulos de domínio, cada um com sua responsabilidade bem definida:

1. **👥 Users**: Gestão de usuários, perfis e controle de acesso baseado em roles (RBAC) via Keycloak OIDC.
2. **🏢 Providers**: Cadastro, perfis detalhados de prestadores, qualificações e processo de verificação.
3. **📄 Documents**: Upload e validação de documentos com processamento de OCR integrado via Azure Document Intelligence.
4. **📋 ServiceCatalogs**: Catálogo de serviços, especialidades, hierarquia de categorias de serviços e restrições.
5. **🔍 SearchProviders**: Busca de prestadores com suporte a consultas geoespaciais complexas (coordenadas/raio) via PostGIS.
6. **📍 Locations**: Gerenciamento de endereços, busca de CEP, cidades permitidas e integração com API do IBGE.
7. **📅 Bookings**: Agendamentos, controle de disponibilidade de horários (schedules) e fluxo completo de reservas de serviços.
8. **💳 Payments**: Monetização da plataforma contendo checkout Stripe, tratamento de webhooks, assinaturas recorrentes e faturamento.
9. **⭐ Ratings**: Sistema de avaliações, feedbacks e moderação de conteúdo (com suporte a Azure AI Content Safety).
10. **✉️ Communications**: Templates de emails, logs de envios e motor de notificações multicanal (Email, SMS, Push) baseado no padrão Outbox.

---

## 🚀 Início Rápido (Setup Local)

### Pré-requisitos

| Ferramenta | Versão Mínima | Link para Download |
|------------|---------------|-------------------|
| **.NET SDK** | 10.0+ | [Download .NET](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **Docker Desktop** | Mais recente | [Download Docker](https://www.docker.com/products/docker-desktop) |
| **Node.js** | 20+ | [Download Node.js](https://nodejs.org/) |
| **Git** | Mais recente | [Download Git](https://git-scm.com/) |

### ⚡ Setup e Inicialização com .NET Aspire (Recomendado)

O **.NET Aspire** gerencia toda a inicialização local dos containers (Postgres, Keycloak, Redis, RabbitMQ), do Gateway, da API e de todos os três frontends Next.js.

1. Certifique-se de que o **Docker Desktop** está em execução.
2. Navegue até o diretório do `AppHost` e execute a aplicação:
   ```powershell
   cd src/Aspire/MeAjudaAi.AppHost
   dotnet run
   ```

3. Acesse o **Dashboard do Aspire** exibido no console (geralmente em [https://localhost:17155](https://localhost:17155) ou [http://localhost:15165](http://localhost:15165)) para acompanhar os logs e monitorar todos os serviços.

### 📋 Mapeamento de Portas e Endereços locais

Quando executado via Aspire, os seguintes serviços estarão disponíveis:

| Serviço | URL Local | Descrição |
|---------|-----------|-----------|
| **Aspire Dashboard** | [https://localhost:17155](https://localhost:17155) | Painel central de orquestração e observabilidade |
| **Customer Web App** | [http://localhost:3000](http://localhost:3000) | Aplicação web pública para clientes |
| **Provider Web App** | [http://localhost:3001](http://localhost:3001) | Painel dedicado para prestadores de serviço |
| **Admin Portal** | [http://localhost:3002](http://localhost:3002) | Painel administrativo (login: `admin.portal` / `admin123`) |
| **API Gateway** | [http://localhost:52861](http://localhost:52861) | Gateway de entrada YARP (redireciona para o ApiService) |
| **API Swagger** | [https://localhost:7001/swagger](https://localhost:7001/swagger) | Documentação interativa da API principal |
| **Keycloak** | [http://localhost:8080](http://localhost:8080) | Painel do Keycloak (admin / admin123) |
| **RabbitMQ Management** | [http://localhost:15672](http://localhost:15672) | Monitor de mensageria (meajudaai / test123) |

---

## 🔄 Fluxo de Desenvolvimento Diário

### 🎨 Trabalhando no Frontend (Next.js Monorepo)

O diretório `src/Web` está estruturado como um monorepo e possui scripts unificados no `package.json` para facilitar o desenvolvimento:

```powershell
# 1. Instalar as dependências do monorepo
cd src/Web
npm install

# 2. Rodar todos os três frontends em paralelo (portas 3000, 3001 e 3002)
npm run dev:all

# 3. Rodar um frontend específico
npm run dev:customer  # Roda na porta 3000
npm run dev:provider  # Roda na porta 3001
npm run dev:admin     # Roda na porta 3002

# 4. Gerar os clientes de API baseados no OpenAPI do backend
npm run generate:api:customer
npm run generate:api:provider
npm run generate:api:admin
```

---

## 🧪 Executando Testes

### Testes do Backend (.NET)

O projeto adota uma estratégia com testes unitários, testes de arquitetura e testes de integração rápidos utilizando Testcontainers.

```powershell
# Executar todos os testes da solução
dotnet test

# Executar testes com coleta de cobertura de código
dotnet test /p:CollectCoverage=true
```

> ⚡ **Dica de Performance (RequiredModules)**: Para evitar inicializar migrations de todos os módulos nos testes de integração locais, certifique-se de declarar apenas os módulos necessários no seu arquivo de teste usando a propriedade `RequiredModules`:
> ```csharp
> protected override TestModule RequiredModules => TestModule.Documents | TestModule.Providers;
> ```
> Isso reduz o tempo de boot dos Testcontainers em até 83%!

### Testes do Frontend e E2E (TypeScript)

Os Web Apps possuem testes unitários com **Vitest** e testes de integração ponta a ponta com **Playwright**.

```powershell
# Executar testes unitários do frontend
cd src/Web
npm run test:customer
npm run test:admin
npm run test:provider

# Executar testes E2E com Playwright
npm run test:e2e
npm run test:e2e:ui     # Abre a interface visual do Playwright
```

---

## 🔧 Configuração de CI/CD e Deploy na Nuvem

### Azure Container Apps (Deploy Rápido)

A plataforma está pronta para deploy automatizado na Azure usando o **Azure Developer CLI (`azd`)**:

```bash
# 1. Autenticar no Azure
azd auth login

# 2. Provisionar a infraestrutura e fazer deploy da aplicação
azd up

# 3. Atualizar apenas o código dos containers após modificações
azd deploy
```

### GitHub Actions

As credenciais necessárias para automação de CI/CD via GitHub Actions podem ser geradas localmente (requer Azure CLI instalado):

```powershell
# Setup completo do pipeline com deploy
.\setup-cicd.ps1 -SubscriptionId "seu-subscription-id"

# Apenas validação de Build/Test na nuvem (sem deploy/custo)
.\setup-ci-only.ps1
```

---

## 📚 Documentação Técnica Adicional

A documentação detalhada pode ser lida na pasta `docs/` ou servida localmente com o **MkDocs**:

```powershell
# Instalar dependências da documentação
pip install mkdocs-material mkdocs-git-revision-date-localized-plugin

# Iniciar o servidor de documentação local
mkdocs serve
```
Acesse a documentação completa em: [http://127.0.0.1:8000/MeAjudaAi/](http://127.0.0.1:8000/MeAjudaAi/)

* Para decisões de arquitetura e design: [docs/architecture.md](docs/architecture.md)
* Para setup inicial detalhado: [docs/development.md](docs/development.md)
* Para infraestrutura e containers: [docs/infrastructure.md](docs/infrastructure.md)