# MeAjudaAi.AppHost

Projeto Aspire AppHost para orquestração da infraestrutura do MeAjudaAi.

## 📁 Estrutura do Projeto

```
MeAjudaAi.AppHost/
├── Extensions/           # Extension methods para configuração de infraestrutura
│   ├── KeycloakExtensions.cs
│   ├── PostgreSqlExtensions.cs
│   └── MigrationExtensions.cs
├── Options/             # Classes de configuração (Options)
│   ├── MeAjudaAiKeycloakOptions.cs
│   └── MeAjudaAiPostgreSqlOptions.cs
├── Results/             # Classes de resultado (Results)
│   ├── MeAjudaAiKeycloakResult.cs
│   └── MeAjudaAiPostgreSqlResult.cs
├── Services/            # Hosted services
│   └── MigrationHostedService.cs
├── Helpers/             # Classes auxiliares
├── Program.cs           # Configuração principal do AppHost
└── appsettings.*.json   # Configurações por ambiente
```

## 🚀 Extensions Disponíveis

### PostgreSQL
```csharp
// Detecção automática de ambiente
var postgresql = builder.AddMeAjudaAiPostgreSQL();

// Configuração manual
var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
{
    options.MainDatabase = "myapp-db";
    options.IncludePgAdmin = true;
});

// Produção (Azure PostgreSQL)
var postgresqlAzure = builder.AddMeAjudaAiAzurePostgreSQL(opts =>
{
    opts.Username = "meajudaai_admin";
    opts.MainDatabase = "meajudaai";
});
```

### Keycloak
```csharp
// Desenvolvimento
var keycloak = builder.AddMeAjudaAiKeycloak();

// Produção - REQUER variáveis de ambiente seguras
var keycloak = builder.AddMeAjudaAiKeycloakProduction();
```

## ⚙️ Configuração

### Variáveis de Ambiente Necessárias

**Desenvolvimento/Teste:**
```bash
export POSTGRES_PASSWORD='strong-dev-password'
```

**Produção:**
```bash
export KEYCLOAK_ADMIN_PASSWORD="your-secure-admin-password"
export POSTGRES_PASSWORD="your-secure-database-password"
```

## 📚 Documentação Completa

Para mais detalhes sobre infraestrutura e deployment, consulte:
- [docs/infrastructure.md](../../../docs/infrastructure.md)
- [docs/deployment-environments.md](../../../docs/deployment-environments.md)
- [docs/architecture.md](../../../docs/architecture.md#🚀-configuração-do-net-aspire-apphost)

## 🎯 Benefícios das Extensions

- **Detecção Automática de Ambiente**: Configurações otimizadas baseadas no ambiente
- **Configurações de Teste**: Otimizações para performance e rapidez nos testes
- **Ferramentas de Desenvolvimento**: PgAdmin, Redis Commander, RabbitMQ Management
- **Produção Pronta**: Configurações Azure e parâmetros seguros
- **Código Limpo**: Separação de responsabilidades (Options/Results/Extensions)
