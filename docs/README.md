# 📚 Documentação - MeAjudaAi

Bem-vindo à documentação completa do projeto MeAjudaAi! Esta plataforma conecta pessoas que precisam de serviços domésticos com prestadores qualificados, usando tecnologias modernas e arquitetura escalável.

## 🚀 Primeiros Passos

Se você é novo no projeto, comece por aqui:

1. **[📖 README Principal](../README.md)** - Visão geral do projeto e setup inicial
2. **[🛠️ Guia de Desenvolvimento](./development.md)** - Setup completo, workflows e diretrizes de testes
3. **[🏗️ Arquitetura](./architecture.md)** - Entenda a estrutura e padrões

## 📋 Documentação Principal

| Documento | Descrição |
|-----------|-----------|
| **[🏗️ Arquitetura](./architecture.md)** | Clean Architecture, DDD, CQRS e padrões |
| **[🔐 Autenticação e Autorização](./authentication_and_authorization.md)** | Keycloak, JWT e sistema de permissões type-safe |
| **[🔄 CI/CD & Security](./ci_cd.md)** | Pipelines, deploy, automação e security scanning |
| **[⚙️ Configuração](./configuration.md)** | Gestão de constantes e configuração por ambiente |
| **[🛠️ Guia de Desenvolvimento](./development.md)** | Setup completo, convenções, workflows, debugging e testes |
| **[🚀 Infraestrutura](./infrastructure.md)** | Docker, Aspire, Azure e configuração de ambientes |
| **[🗺️ Roadmap do Projeto](./roadmap.md)** | Funcionalidades futuras e planejamento |
| **[🔩 Débito Técnico](./technical_debt.md)** | Itens de débito técnico e melhorias planejadas |

## 📁 Documentação Especializada

### **🗄️ Database**

| Documento | Descrição |
|-----------|-----------|
| **[🗄️ Limites do Banco de Dados](./database/database_boundaries.md)** | Estratégia de schemas modulares |
| **[🏭 DbContext Factory](./database/db_context_factory.md)** | Factory pattern para Entity Framework |
| **[🗃️ Organização de Scripts](./database/scripts_organization.md)** | Como organizar e criar scripts de banco para novos módulos |

### **📝 Logging**

| Documento | Descrição |
|-----------|-----------|
| **[🆔 Correlation ID](./logging/CORRELATION_ID.md)** | Melhores práticas para implementação e uso de Correlation IDs |
| **[⏱️ Desempenho](./logging/PERFORMANCE.md)** | Estratégias e ferramentas de monitoramento de desempenho |
| **[📊 Seq Setup](./logging/SEQ_SETUP.md)** | Configuração do Seq para logging estruturado |

### **💬 Messaging**

| Documento | Descrição |
|-----------|-----------|
| **[💀 Dead Letter Queue](./messaging/dead_letter_queue.md)** | Estratégia completa de DLQ com operações |
| **[🚌 Estratégia de Message Bus](./messaging/message_bus_strategy.md)** | Estratégia de messaging por ambiente |
| **[🧪 Mocks de Messaging](./messaging/messaging_mocks.md)** | Mocks para testes de messaging |

### **📱 Módulos de Domínio**

| Documento | Descrição |
|-----------|-----------|
| **[📅 Módulo Bookings](./modules/bookings.md)** | Sistema de agendamentos (planejado) |
| **[📄 Módulo Documents](./modules/documents.md)** | Gerenciamento de documentos |
| **[🔧 Módulo Providers](./modules/providers.md)** | Prestadores de serviços, verificação e documentos |
| **[🔍 Módulo Search](./modules/search.md)** | Busca geoespacial de prestadores com PostGIS |
| **[📋 Módulo Services](./modules/services.md)** | Catálogo de serviços (planejado) |
| **[👥 Módulo Users](./modules/users.md)** | Gestão de usuários, autenticação e perfis |

### **🧪 Testes**

| Documento | Descrição |
|-----------|-----------|
| **[📊 Guia de Cobertura de Código](./testing/code_coverage_guide.md)** | Como visualizar e interpretar a cobertura de código |
| **[⚙️ Testes de Integração](./testing/integration_tests.md)** | Guia para escrever e manter testes de integração |
| **[🔒 Exemplos de Testes de Autenticação](./testing/test_auth_examples.md)** | Exemplos práticos do TestAuthenticationHandler |

### **📚 Guias e Relatórios**

| Documento | Descrição |
|-----------|-----------|
| **[📝 Guia de Implementação do EditorConfig](./guides/editorconfig_implementation_guide.md)** | Guia de implementação do EditorConfig |
| **[🔒 Relatório de Melhorias de Segurança](./reports/security_improvements_report.md)** | Relatório de melhorias de segurança |

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
-  Abra uma [issue](https://github.com/frigini/MeAjudaAi/issues)
- 🔄 Sugira melhorias via pull request

**Ajuda com desenvolvimento?**
- 📖 Consulte os guias relevantes
- 🛠️ Verifique troubleshooting guides
- 🤝 Entre em contato com a equipe

---

*📅 Última atualização: 14 de Novembro de 2025*  
*✨ Documentação reorganizada e consolidada pela equipe MeAjudaAi*
