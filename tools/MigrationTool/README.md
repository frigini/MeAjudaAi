# 🔧 Migration Tool

Ferramenta CLI para gerenciar migrações de banco de dados de todos os módulos do MeAjudaAi.

## 📋 Visão Geral

O Migration Tool automatiza a aplicação de migrações em todos os módulos (Users, Providers, Documents), eliminando a necessidade de executar comandos `dotnet ef` manualmente para cada módulo.

## 🚀 Uso

### Comandos Disponíveis

```bash
# Aplicar todas as migrações pendentes
dotnet run --project tools/MigrationTool -- migrate

# Criar bancos de dados se não existirem
dotnet run --project tools/MigrationTool -- create

# Remover e recriar todos os bancos (⚠️ CUIDADO: apaga dados!)
dotnet run --project tools/MigrationTool -- reset

# Mostrar status das migrações
dotnet run --project tools/MigrationTool -- status
```

### Exemplos

```bash
# Verificar status antes de aplicar
cd tools/MigrationTool
dotnet run -- status

# Aplicar migrações
dotnet run -- migrate

# Resetar ambiente de desenvolvimento
dotnet run -- reset
```

## ⚙️ Configuração

### Connection String

Por padrão, usa `localhost:5432` com usuário `postgres` e senha `test123`. Para alterar, edite `Program.cs`:

```csharp
private static readonly Dictionary<string, string> _connectionStrings = new()
{
    ["Users"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=YOUR_PASSWORD",
    ["Providers"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=YOUR_PASSWORD",
    ["Documents"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=YOUR_PASSWORD"
};
```

Ou use variáveis de ambiente (planejado para versão futura).

### Schemas PostgreSQL

Cada módulo usa seu próprio schema:
- **Users** → `users`
- **Providers** → `providers`
- **Documents** → `documents`

## 🔍 Como Funciona

1. **Auto-discovery**: Escaneia assemblies `*.Infrastructure.dll` em busca de classes `DbContext`
2. **Registro automático**: Registra todos os contextos encontrados com suas connection strings
3. **Execução**: Aplica operações em todos os contextos simultaneamente
4. **Logging**: Exibe progresso e status de cada módulo

## 📊 Output de Exemplo

```text
🔧 MeAjudaAi Migration Tool
📋 Comando: status

📦 UsersDbContext
  ✅ Migrações aplicadas: 5
    - 20241101_InitialCreate
    - 20241102_AddUserRoles
    - 20241103_AddEmailVerification
  ✅ Todas as migrações estão aplicadas

📦 ProvidersDbContext
  ✅ Migrações aplicadas: 3
  ⏳ Migrações pendentes: 1
    - 20241110_AddProviderVerification

📦 DocumentsDbContext
  ✅ Migrações aplicadas: 2
  ✅ Todas as migrações estão aplicadas
```

## ⚠️ Avisos Importantes

- **Reset**: O comando `reset` **apaga todos os dados**. Use apenas em desenvolvimento!
- **Produção**: Nunca use esta ferramenta em produção. Aplique migrações via pipeline CI/CD.
- **Backup**: Sempre faça backup antes de operações destrutivas.

## 🛠️ Desenvolvimento

### Adicionar Novo Módulo

Quando criar um novo módulo, adicione sua connection string em `_connectionStrings`:

```csharp
["NovoModulo"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123"
```

O auto-discovery detectará automaticamente o `DbContext` do novo módulo.

### Troubleshooting

#### Erro: "Cannot find DbContext"
- Certifique-se de que o assembly `*.Infrastructure.dll` foi compilado
- Verifique se o namespace contém "MeAjudaAi" e "Infrastructure"

#### Erro: "Connection failed"
- Verifique se o PostgreSQL está rodando
- Confirme usuário/senha na connection string
- Teste conexão com `psql -h localhost -U postgres -d meajudaai`

## 📚 Referências

- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
