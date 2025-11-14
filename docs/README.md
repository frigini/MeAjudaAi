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

### **🔐 Segurança e Autenticação**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[� Autenticação Completa](./authentication.md)** | Keycloak, JWT e sistema de autorização | Desenvolvedores |
| **[🛡️ Implementação de Autorização](./authorization_implementation.md)** | Sistema type-safe de permissões | Desenvolvedores |
| **[🔑 Permissões Type-Safe](./type_safe_permissions.md)** | Detalhes do sistema baseado em EPermission | Desenvolvedores |
| **[🖥️ Permissões Server-Side](./server_side_permissions.md)** | Resolução de permissões no servidor | Desenvolvedores backend |
| **[🔑 Integração Keycloak](./keycloak_integration.md)** | Configuração e integração detalhada | Administradores |

### **📱 Módulos de Domínio**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[👥 Módulo Users](./modules/users.md)** | Gestão de usuários, autenticação e perfis | Desenvolvedores |
| **[🔧 Módulo Providers](./modules/providers.md)** | Prestadores de serviços, verificação e documentos | Desenvolvedores |
| **[📋 Módulo Services](./modules/services.md)** | Catálogo de serviços (planejado) | Desenvolvedores |
| **[📅 Módulo Bookings](./modules/bookings.md)** | Sistema de agendamentos (planejado) | Desenvolvedores |
| **[🗺️ Roadmap do Projeto](./ROADMAP.md)** | Funcionalidades futuras e planejamento | Todos |

### **🚀 Infraestrutura e Deploy**

| Documento | Descrição | Para quem |
|-----------|-----------|-----------|
| **[🚀 Infraestrutura](./infrastructure.md)** | Docker, Aspire, Azure e configuração de ambientes | DevOps |
| **[🔄 CI/CD & Security](./ci_cd.md)** | Pipelines, deploy, automação e security scanning | DevOps |
| **[🌍 Ambientes de Deploy](./deployment_environments.md)** | Configuração de ambientes | DevOps |

### **⚙️ Configuração e Constantes**

| Documento | Descrição | Para quem |
|-----------|-----------|--------|
| **[📋 Templates de Configuração](./configuration-templates/)** | Templates para todos os ambientes | Desenvolvedores |
| **[🔧 Sistema de Constantes](./constants_system.md)** | Gestão centralizada de constantes | Desenvolvedores |

### **📚 Guias e Relatórios**

| Documento | Descrição | Para quem |
|-----------|-----------|--------|
| **[📝 EditorConfig Implementation Guide](./guides/editorconfig-implementation-guide.md)** | Guia de implementação do EditorConfig | Desenvolvedores |
| **[🔒 Security Improvements Report](./reports/security-improvements-report.md)** | Relatório de melhorias de segurança | Arquitetos, DevOps |
| **[📋 PLAN.md](./PLAN.md)** | Plano geral do projeto | Todos |
| **[🚀 WARP.md](./WARP.md)** | Documentação WARP | Todos |

## 📁 Documentação Especializada

### **💬 Messaging**

| Documento | Descrição | Nível |
|-----------|-----------|-------|
| **[💀 Dead Letter Queue Strategy](./messaging/dead_letter_queue_strategy.md)** | Estratégia completa de DLQ com operações | Avançado |
| **[📊 Resumo da Implementação DLQ](./messaging/dead_letter_queue_implementation_summary.md)** | Resumo da implementação | Intermediário |
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

### ✅ **Completo e Atualizado (Novembro 2025)**
- ✅ Guia de Desenvolvimento com Testes Integrados
- ✅ Sistema Completo de Autenticação e Autorização Type-Safe
- ✅ Arquitetura Clean Architecture + DDD + CQRS
- ✅ Infraestrutura Docker + Aspire + Azure
- ✅ CI/CD com Security Scanning Integrado
- ✅ Dead Letter Queue Strategy Operacional
- ✅ Database Boundaries e Migration Strategy
- ✅ Logging Estruturado e Observabilidade
- ✅ Configuration Templates por Ambiente
- ✅ Módulo Users - Gestão completa de usuários
- ✅ Módulo Providers - Prestadores de serviços implementado

### 🔄 **Em Evolução**
- 🔄 Documentação de APIs (com crescimento do projeto)
- 🔄 Guias de usuário final (futuro)
- 🔄 Módulo Services (planejado)
- 🔄 Módulo Bookings (planejado)

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
├── 📁 messaging/ (4 documentos)
├── 📁 guides/ (guias de implementação)
│   └── editorconfig-implementation-guide.md
├── 📁 reports/ (relatórios de análise)
│   └── security-improvements-report.md
└── 📁 modules/ (documentação de módulos)
    ├── users.md
    ├── providers.md
    └── documents.md
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

*📅 Última atualização: 14 de Novembro de 2025*  
*✨ Documentação reorganizada e consolidada pela equipe MeAjudaAi*  
*📂 Arquivos reorganizados: guias → docs/guides/, relatórios → docs/reports/*