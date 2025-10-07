# MeAjudaAi Aspire Extensions

## 📁 Estrutura das Extensions

Esta pasta contém as extension methods customizadas para simplificar a configuração da infraestrutura do MeAjudaAi no Aspire AppHost.

### Arquivos

- **PostgreSqlExtensions.cs**: Configuração otimizada do PostgreSQL para teste/dev/produção
- **RedisExtensions.cs**: Configuração do Redis com otimizações por ambiente
- **RabbitMQExtensions.cs**: Configuração do RabbitMQ/Service Bus
- **KeycloakExtensions.cs**: Configuração do Keycloak com diferentes bancos de dados

## 🚀 Como Usar

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
    opts.Username = "meajudaai_admin"; // não use nomes reservados
    opts.MainDatabase = "meajudaai";
});
```

**Nota**: para ambientes local/teste, defina `POSTGRES_PASSWORD` antes de subir:
```bash
export POSTGRES_PASSWORD='strong-dev-password'
```

### Redis
```csharp
var redis = builder.AddMeAjudaAiRedis(options =>
{
    options.MaxMemory = "512mb";
    options.IncludeRedisCommander = true;
});
```

### RabbitMQ
```csharp
// Desenvolvimento/Teste
var rabbitMq = builder.AddMeAjudaAiRabbitMQ();

// Produção (Service Bus)
var serviceBus = builder.AddMeAjudaAiServiceBus();
```

### Keycloak
```csharp
// Desenvolvimento
var keycloak = builder.AddMeAjudaAiKeycloak();

// Produção - REQUER variáveis de ambiente seguras
var keycloak = builder.AddMeAjudaAiKeycloakProduction();
```

#### ⚠️ Requisitos de Segurança para Produção

Para usar `AddMeAjudaAiKeycloakProduction()`, as seguintes variáveis de ambiente **devem** estar definidas:

- `KEYCLOAK_ADMIN_PASSWORD`: Senha segura para o administrador do Keycloak
- `POSTGRES_PASSWORD`: Senha segura para o banco de dados PostgreSQL

**Exemplo de configuração:**
```bash
export KEYCLOAK_ADMIN_PASSWORD="your-secure-admin-password-here"
export POSTGRES_PASSWORD="your-secure-database-password-here"
```

⚠️ **Nunca use senhas padrão ou fracas em produção!** O método falhará se essas variáveis não estiverem definidas, evitando deployments inseguros.

#### 🔒 Restrições do Azure PostgreSQL

**Nomes de usuário não permitidos no Azure PostgreSQL:**
- `postgres`, `admin`, `administrator`, `root`, `guest`, `public`
- Use nomes específicos da aplicação como `meajudaai_admin`, `app_user`, etc.

## 🎯 Benefícios

- **Detecção Automática de Ambiente**: Configurações otimizadas baseadas no ambiente
- **Configurações de Teste**: Otimizações para performance e rapidez nos testes
- **Ferramentas de Desenvolvimento**: PgAdmin, Redis Commander, RabbitMQ Management
- **Produção Pronta**: Configurações Azure e parâmetros seguros
- **Código Limpo**: Program.cs reduzido em 45% (220 → 120 linhas)