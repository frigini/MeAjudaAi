# 📚 Documentação - MeAjudaAi

Bem-vindo à documentação completa do projeto MeAjudaAi! Esta plataforma conecta pessoas que precisam de serviços domésticos com prestadores qualificados, usando tecnologias modernas e arquitetura escalável.

## 🚀 Primeiros Passos

Se você é novo no projeto, comece por aqui:

1. **[📖 README Principal](../README.md)** - Visão geral do projeto e setup inicial
2. **[🛠️ Guia de Desenvolvimento](./development_guide.md)** - Setup completo e workflows
3. **[🏗️ Arquitetura](./architecture.md)** - Entenda a estrutura e padrões

## 📋 Índice da Documentação

### **Desenvolvimento e Setup**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🛠️ Guia de Desenvolvimento](./development_guide.md)** | Setup completo, convenções, workflows e debugging | Desenvolvedores novos e experientes |
| **[📋 Diretrizes de Desenvolvimento](./development_guide.md)** | Padrões de código, estrutura, Module APIs e ID generation | Desenvolvedores |
| **[🚀 Infraestrutura](./infrastructure.md)** | Docker, Aspire, Azure e configuração de ambientes | DevOps e desenvolvedores |
| **[🔄 CI/CD](./ci_cd.md)** | Pipelines, deploy e automação | DevOps e tech leads |
| **[📦 Adicionando Novos Módulos](./adding-new-modules.md)** | Como adicionar módulos com testes e cobertura | Desenvolvedores |

### **Arquitetura e Design**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🏗️ Arquitetura](./architecture.md)** | Clean Architecture, DDD, CQRS e padrões | Arquitetos e desenvolvedores sênior |
| **[📐 Domain-Driven Design](./architecture.md#-domain-driven-design-ddd)** | Bounded contexts, agregados e eventos | Desenvolvedores de domínio |
| **[⚡ CQRS](./architecture.md#-cqrs-command-query-responsibility-segregation)** | Commands, queries e handlers | Desenvolvedores backend |

### **Infraestrutura e Deploy**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🐳 Containers](./infrastructure.md#-configuração-para-desenvolvimento)** | Docker Compose e Aspire | Desenvolvedores |
| **[☁️ Azure](./infrastructure.md#-deploy-em-produção)** | Container Apps, Bicep e recursos Azure | DevOps |
| **[🔐 Keycloak](./infrastructure.md#-configuração-do-keycloak)** | Autenticação e autorização | Desenvolvedores e administradores |
| **[🗄️ PostgreSQL](./infrastructure.md#-configuração-de-banco-de-dados)** | Schemas, migrations e estratégia de dados | Desenvolvedores backend |

### **Qualidade e Testes**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🧪 Estratégias de Teste](./development_guide.md#-estratégias-de-teste)** | Unit, integration e E2E tests | Desenvolvedores |
| **[📊 Code Quality](./ci_cd.md#-monitoramento-e-métricas)** | Quality gates, cobertura e métricas | Tech leads |
| **[🔍 Debugging](./development_guide.md#-debugging-e-troubleshooting)** | Logs, métricas e troubleshooting | Desenvolvedores |

### **Segurança**

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
2. Siga o [Guia de Desenvolvimento](./development_guide.md) para setup
3. Consulte as [Diretrizes de Desenvolvimento](./development_guide.md) para padrões
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
1. Entenda as [estratégias de teste](./development_guide.md#-estratégias-de-teste)
2. Configure os [ambientes de teste](./infrastructure.md#docker-compose-alternativo)
3. Implemente [testes E2E](./development_guide.md#e2e-tests---api-layer)
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