# 📚 Documentação - MeAjudaAi

Bem-vindo à documentação completa do projeto MeAjudaAi! Esta plataforma conecta pessoas que precisam de serviços domésticos com prestadores qualificados, usando tecnologias modernas e arquitetura escalável.

## 🚀 Primeiros Passos

Se você é novo no projeto, comece por aqui:

1. **[📖 README Principal](../README.md)** - Visão geral do projeto e setup inicial
2. **[🛠️ Guia de Desenvolvimento](./development.md)** - Setup completo, workflows e diretrizes de testes
3. **[🏗️ Arquitetura](./architecture.md)** - Entenda a estrutura e padrões

## 📋 Documentação Principal

### **🛠️ Desenvolvimento**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🛠️ Guia de Desenvolvimento](./development.md)** | Setup completo, convenções, workflows, debugging e testes | Desenvolvedores |
| **[🏗️ Arquitetura](./architecture.md)** | Clean Architecture, DDD, CQRS e padrões | Arquitetos e desenvolvedores |
| **[📦 Adicionando Novos Módulos](./adding-new-modules.md)** | Como adicionar módulos com testes e cobertura | Desenvolvedores |
| **[🔄 Workflow Fixes](./workflow-fixes.md)** | Correções e melhorias de workflow | DevOps |

### **🔐 Segurança e Autenticação**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[� Autenticação Completa](./authentication.md)** | Keycloak, JWT e sistema de autorização | Desenvolvedores |
| **[🛡️ Implementação de Autorização](./authorization_implementation.md)** | Sistema type-safe de permissões | Desenvolvedores |
| **[🔑 Permissões Type-Safe](./type_safe_permissions.md)** | Detalhes do sistema baseado em EPermission | Desenvolvedores |
| **[🖥️ Permissões Server-Side](./server_side_permissions.md)** | Resolução de permissões no servidor | Desenvolvedores backend |
| **[🔄 Refatoração de Autorização](./authorization_refactoring.md)** | Melhorias e refatorações | Desenvolvedores |
| **[🔑 Integração Keycloak](./keycloak_integration.md)** | Configuração e integração detalhada | Administradores |

### **🚀 Infraestrutura e Deploy**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🚀 Infraestrutura](./infrastructure.md)** | Docker, Aspire, Azure e configuração de ambientes | DevOps |
| **[🔄 CI/CD & Security](./ci_cd.md)** | Pipelines, deploy, automação e security scanning | DevOps |
| **[🌍 Ambientes de Deploy](./deployment_environments.md)** | Configuração de ambientes | DevOps |

### **⚙️ Configuração e Constantes**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[📋 Templates de Configuração](./configuration-templates/)** | Templates para todos os ambientes | Desenvolvedores |
| **[🔧 Sistema de Constantes](./constants_system.md)** | Gestão centralizada de constantes | Desenvolvedores |

## 📁 Documentação Especializada

### **💬 Messaging**

| Documento | Descrição | Nível |
|-----------|-----------|-------|
| **[💀 Dead Letter Queue Strategy](./messaging/dead_letter_queue_strategy.md)** | Estratégia completa de DLQ com operações | Avançado |
| **[📊 DLQ Implementation Summary](./messaging/dead_letter_queue_implementation_summary.md)** | Resumo da implementação | Intermediário |
| **[� Message Bus Strategy](./messaging/message_bus_strategy.md)** | Estratégia de messaging por ambiente | Avançado |
| **[🧪 Messaging Mocks](./messaging/messaging_mocks.md)** | Mocks para testes de messaging | Avançado |

### **🗄️ Database**

| Documento | Descrição | Nível |
|-----------|-----------|-------|
| **[🔄 Database Migration](./database/database_migration.md)** | Estratégia de migrations | Intermediário |
| **[🏭 DbContext Factory](./database/db_context_factory.md)** | Factory pattern para Entity Framework | Intermediário |
| **[🗄️ Database Boundaries](./database/database_boundaries.md)** | Estratégia de schemas modulares | Avançado |
| **[📊 PostgreSQL Setup](./database/postgresql_setup.md)** | Configuração e otimização | Intermediário |
| **[🔒 Database Security](./database/database_security.md)** | Segurança e acesso | Avançado |

### **📝 Logging**

| Documento | Descrição | Nível |
|-----------|-----------|-------|
| **[� Logging Strategy](./logging/logging_strategy.md)** | Estratégia de logs estruturados | Intermediário |
| **[📊 Seq Setup](./logging/seq_setup.md)** | Configuração do Seq | Intermediário |
| **[🔍 Observability](./logging/observability.md)** | Monitoramento e métricas | Avançado |
| **[🐛 Troubleshooting](./logging/troubleshooting.md)** | Guia de resolução de problemas | Intermediário |

## 🎯 Guias por Cenário

### **🆕 Novo Desenvolvedor**
1. 📖 Leia o [README principal](../README.md) para entender o projeto
2. 🛠️ Siga o [Guia de Desenvolvimento](./development.md) para setup completo
3. 🏗️ Estude a [Arquitetura](./architecture.md) para entender os padrões
4. 🔐 Configure [Autenticação](./authentication.md) para desenvolvimento
5. 🧪 Aprenda sobre [Testes](./development.md#-diretrizes-de-testes)
6. 🚀 Configure [Infraestrutura](./infrastructure.md) local

### **🏗️ Arquiteto de Software**
1. 🏗️ Analise a [Arquitetura](./architecture.md) completa
2. 📐 Revise os padrões DDD e CQRS
3. 🗄️ Entenda a [estratégia de dados](./database/database_boundaries.md)
4. 💬 Avalie as [estratégias de messaging](./messaging/message_bus_strategy.md)
5. 🔐 Revise o [sistema de permissões](./type_safe_permissions.md)

### **🚀 DevOps Engineer**
1. 🚀 Configure a [Infraestrutura](./infrastructure.md)
2. 🔄 Implemente os [pipelines CI/CD](./ci_cd.md)
3. 🌍 Gerencie [ambientes](./deployment_environments.md)
4. 📊 Configure [monitoramento](./logging/observability.md)
5. 🔒 Implemente [security scanning](./ci_cd.md#-security-scanning-fixes)

### **🧪 QA Engineer**
1. 🧪 Entenda as [estratégias de teste](./development.md#-diretrizes-de-testes)
2. 🔐 Configure [autenticação de testes](./development.md#3-test-authentication-handler)
3. 🚀 Use [ambientes de teste](./infrastructure.md)
4. 🧪 Implemente [mocks de messaging](./messaging/messaging_mocks.md)

## 📈 Status da Documentação

### ✅ **Completo e Atualizado (Outubro 2025)**
- ✅ Guia de Desenvolvimento com Testes Integrados
- ✅ Sistema Completo de Autenticação e Autorização Type-Safe
- ✅ Arquitetura Clean Architecture + DDD + CQRS
- ✅ Infraestrutura Docker + Aspire + Azure
- ✅ CI/CD com Security Scanning Integrado
- ✅ Dead Letter Queue Strategy Operacional
- ✅ Database Boundaries e Migration Strategy
- ✅ Logging Estruturado e Observabilidade
- ✅ Configuration Templates por Ambiente

### 🔄 **Em Evolução**
- � Documentação de APIs (com crescimento do projeto)
- 🔄 Guias de usuário final (futuro)
- 🔄 Documentação de módulos específicos (conforme implementação)

## 🧹 Reorganização Recente

**Outubro 2025**: Documentação completamente reorganizada para eliminar redundância:

### ✅ **Consolidações Realizadas**
- 📁 **Removidas 7 pastas** redundantes: `examples/`, `operations/`, `authentication/`, `technical/`, `testing/`, `deployment/`
- 📄 **Consolidados 15+ arquivos** duplicados
- 🔗 **Atualizados 25+ links** quebrados
- 📚 **Integradas** estratégias de testes ao `development.md`
- 🔐 **Unificadas** documentações de segurança e CI/CD
- 💀 **Consolidadas** múltiplas versões de Dead Letter Queue docs

### 🏗️ **Nova Estrutura**
```
docs/
├── 📄 Arquivos principais (14 documentos)
├── 📁 configuration-templates/ (7 templates)
├── 📁 database/ (5 documentos)
├── 📁 logging/ (4 documentos)
└── 📁 messaging/ (4 documentos)
```

## 🤝 Como Contribuir

### **Melhorar Documentação**
1. Identifique informações desatualizadas ou confusas
2. Abra uma [issue](https://github.com/frigini/MeAjudaAi/issues) ou PR
3. Use commits semânticos: `docs(scope): description`

### **Adicionar Documentação**
1. Siga a estrutura e formatação existente
2. Use Markdown com emojis para identificação visual
3. Inclua exemplos práticos e código
4. Atualize este README

### **Padrões**
- **Títulos**: Use emojis para identificação visual
- **Código**: Syntax highlighting apropriado
- **Links**: Referências relativas para docs internos
- **Idioma**: Português brasileiro
- **Estrutura**: Siga padrões estabelecidos

## 🔗 Links Úteis

### **Repositório**
- 🏠 [Repositório GitHub](https://github.com/frigini/MeAjudaAi)
- 🐛 [Issues e Bugs](https://github.com/frigini/MeAjudaAi/issues)
- 📋 [Project Board](https://github.com/frigini/MeAjudaAi/projects)

### **Tecnologias**
- 🟣 [.NET 9](https://docs.microsoft.com/dotnet/)
- 🐘 [PostgreSQL](https://www.postgresql.org/docs/)
- 🔑 [Keycloak](https://www.keycloak.org/documentation)
- ☁️ [Azure](https://docs.microsoft.com/azure/)
- 🚀 [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)

### **Padrões**
- 🏗️ [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- 📐 [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- ⚡ [CQRS Pattern](https://docs.microsoft.com/azure/architecture/patterns/cqrs)

---

## 📞 Suporte

**Problemas na documentação?**
- � Abra uma [issue](https://github.com/frigini/MeAjudaAi/issues)
- 🔄 Sugira melhorias via pull request

**Ajuda com desenvolvimento?**
- 📖 Consulte os guias relevantes
- 🛠️ Verifique troubleshooting guides
- 🤝 Entre em contato com a equipe

---

*📅 Última atualização: Outubro 2025*  
*✨ Documentação reorganizada e consolidada pela equipe MeAjudaAi*

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🔐 Guia de Autenticação](./authentication.md)** | Keycloak, JWT e configuração completa de auth | Desenvolvedores |
| **[🛡️ Autenticação](./architecture.md#padroes-de-seguranca)** | JWT, Keycloak e autorização | Desenvolvedores |
| **[🔒 Validação](./architecture.md#validation-pattern)** | FluentValidation e input validation | Desenvolvedores |
| **[🧪 Testes de Autenticação](./testing/)** | TestAuthenticationHandler e exemplos | Desenvolvedores |
| **[🚨 Security Scan](./ci_cd.md#-configuração-do-azure-devops)** | Análise de segurança e vulnerabilidades | DevOps |

## 🔧 Documentação Técnica Avançada

Para implementações específicas e detalhes técnicos:

### **Implementações Detalhadas**

| Documento | Descrição | Nível |
|-----------|-----------|-------|
| **[📨 MessageBus Strategy](./technical/message_bus_environment_strategy.md)** | Estratégia de messaging por ambiente | Avançado |
| **[🧪 Messaging Mocks](./technical/messaging_mocks_implementation.md)** | Mocks para Azure Service Bus e RabbitMQ | Avançado |
| **[🏭 DbContext Factory](./technical/db_context_factory_pattern.md)** | Factory pattern para Entity Framework | Intermediário |
| **[🔐 Keycloak Config](./technical/keycloak_configuration.md)** | Configuração detalhada do Keycloak | Intermediário |
| **[🗄️ Database Boundaries](./technical/database_boundaries.md)** | Estratégia de schemas modulares | Avançado |

## 🎯 Guias por Cenário

### **🆕 Novo Desenvolvedor**
1. Leia o [README principal](../README.md) para entender o projeto
2. Siga o [Guia de Desenvolvimento](./development.md) para setup
3. Consulte as [Diretrizes de Desenvolvimento](./development.md) para padrões
4. Configure [Autenticação](./authentication.md) para desenvolvimento
5. Estude a [Arquitetura](./architecture.md) para entender os padrões
6. Consulte a [Infraestrutura](./infrastructure.md) para ambientes

### **🏗️ Arquiteto de Software**
1. Analise a [Arquitetura](./architecture.md) completa
2. Revise os [padrões DDD](./architecture.md#-domain-driven-design-ddd)
3. Entenda a [estratégia de dados](./technical/database_boundaries.md)
4. Avalie as [estratégias de messaging](./technical/message_bus_environment_strategy.md)

### **🚀 DevOps Engineer**
1. Configure a [Infraestrutura](./infrastructure.md)
2. Implemente os [pipelines CI/CD](./ci_cd.md)
3. Gerencie os [recursos Azure](./infrastructure.md#recursos-azure)
4. Configure [monitoramento](./ci_cd.md#-monitoramento-e-métricas)

### **🧪 QA Engineer**
1. Entenda as [estratégias de teste](./development.md#-diretrizes-de-testes)
2. Configure os [ambientes de teste](./infrastructure.md#docker-compose-alternativo)
3. Implemente [testes E2E](./development.md#-diretrizes-de-testes)
4. Use os [mocks disponíveis](./technical/messaging_mocks_implementation.md)

## 📈 Status da Documentação

### ✅ **Completo e Atualizado**
- ✅ Guia de Desenvolvimento
- ✅ Diretrizes de Desenvolvimento e Padrões de Código
- ✅ Guia Completo de Autenticação e Segurança
- ✅ Documentação de Testes de Autenticação
- ✅ Arquitetura e Padrões
- ✅ Infraestrutura e Deploy
- ✅ CI/CD e Automação
- ✅ Configurações de Segurança

### 🔄 **Em Evolução**
- 🔄 Documentação de APIs (com crescimento do projeto)
- 🔄 Guias de usuário final (futuro)
- 🔄 Documentação de módulos específicos (conforme implementação)

## 🤝 Como Contribuir

### **Melhorar Documentação Existente**
1. Identifique informações desatualizadas ou confusas
2. Abra uma [issue](https://github.com/frigini/MeAjudaAi/issues) ou PR
3. Use o padrão de commits semânticos: `docs(scope): description`

### **Adicionar Nova Documentação**
1. Siga a estrutura e formatação existente
2. Use Markdown com emojis para melhor legibilidade
3. Inclua exemplos práticos e código quando aplicável
4. Atualize este índice com novas adições

### **Padrões de Documentação**
- **Títulos**: Use emojis para identificação visual
- **Código**: Sempre com syntax highlighting apropriado
- **Links**: Use referências relativas para documentos internos
- **Idioma**: Português brasileiro para toda documentação
- **Estrutura**: Siga o padrão estabelecido nos documentos existentes

## 🔗 Links Úteis

### **Repositório e Projeto**
- 🏠 [Repositório GitHub](https://github.com/frigini/MeAjudaAi)
- 🐛 [Issues e Bugs](https://github.com/frigini/MeAjudaAi/issues)
- 📋 [Project Board](https://github.com/frigini/MeAjudaAi/projects)

### **Tecnologias Utilizadas**
- 🟣 [.NET 9](https://docs.microsoft.com/dotnet/)
- 🐘 [PostgreSQL](https://www.postgresql.org/docs/)
- 🔑 [Keycloak](https://www.keycloak.org/documentation)
- ☁️ [Azure](https://docs.microsoft.com/azure/)
- 🚀 [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)

### **Padrões e Arquitetura**
- 🏗️ [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- 📐 [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- ⚡ [CQRS Pattern](https://docs.microsoft.com/azure/architecture/patterns/cqrs)
- 🔄 [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)

---

## 📞 Suporte

**Encontrou algum problema na documentação?**
- 📧 Abra uma [issue](https://github.com/frigini/MeAjudaAi/issues)
- 💬 Entre em contato com a equipe de desenvolvimento
- 🔄 Sugira melhorias via pull request

**Precisa de ajuda com desenvolvimento?**
- 📖 Consulte primeiro os guias relevantes
- 🛠️ Verifique os troubleshooting guides
- 🤝 Entre em contato com mentores da equipe

---

*📅 Última atualização: Dezembro 2024*  
*✨ Documentação mantida pela equipe de desenvolvimento MeAjudaAi*