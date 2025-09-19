# 🔒 Isolamento de Schema para Módulo Users

## 📋 Visão Geral

O `SchemaPermissionsManager` implementa **isolamento de segurança para o módulo Users** usando os scripts SQL existentes em `infrastructure/database/schemas/`.

## 🎯 Objetivos

- **Isolamento de dados**: Módulo Users só acessa schema `users`
- **Segurança**: `users_role` não pode acessar outros dados  
- **Reutilização**: Usa scripts existentes da infraestrutura
- **Flexibilidade**: Pode ser habilitado/desabilitado por configuração

## 🚀 Como Usar

### 1. Desenvolvimento (Padrão Atual)
```csharp
// Program.cs - modo atual (sem isolamento)
services.AddUsersModule(configuration);
```

### 2. Produção (Com Isolamento)
```csharp
// Program.cs - modo seguro
if (app.Environment.IsProduction())
{
    await services.AddUsersModuleWithSchemaIsolationAsync(configuration);
}
else
{
    services.AddUsersModule(configuration);
}
```

### 3. Configuração (appsettings.Production.json)
```json
{
  "Database": {
    "EnableSchemaIsolation": true
  },
  "ConnectionStrings": {
    "meajudaai-db-admin": "Host=prod-db;Database=meajudaai;Username=admin;Password=admin_password;"
  },
  "Postgres": {
    "UsersRolePassword": "users_secure_password_123",
    "AppRolePassword": "app_secure_password_456"
  }
}
```

## 🔧 Scripts Existentes Utilizados

### 1. **00-create-roles-users-only.sql**
```sql
CREATE ROLE users_role LOGIN PASSWORD 'users_secret';
CREATE ROLE meajudaai_app_role LOGIN PASSWORD 'app_secret';
GRANT users_role TO meajudaai_app_role;
```

### 2. **02-grant-permissions-users-only.sql**
```sql
-- Permissões específicas do módulo Users
-- Search path: users, public
-- Isolamento completo de outros schemas
```

> **📝 Nota sobre Schemas**: O schema `users` é criado automaticamente pelo Entity Framework Core através da configuração `HasDefaultSchema("users")`. Não há necessidade de scripts específicos para criação de schemas.

## ⚡ Benefícios

✅ **Reutiliza infraestrutura existente**: Usa scripts já testados  
✅ **Zero configuração manual**: Setup automático quando necessário  
✅ **Flexível**: Pode ser habilitado apenas em produção  
✅ **Seguro**: Isolamento real para o módulo Users  
✅ **Consistente**: Alinhado com a estrutura atual do projeto  
✅ **Simplificado**: EF Core gerencia a criação de schemas automaticamente

## 📊 Cenários de Uso

| Ambiente | Configuração | Comportamento |
|----------|-------------|---------------|
| **Desenvolvimento** | `EnableSchemaIsolation: false` | Usa usuário admin padrão |
| **Teste** | `EnableSchemaIsolation: false` | TestContainers com usuário único |
| **Staging** | `EnableSchemaIsolation: true` | Usuário `users_role` dedicado |
| **Produção** | `EnableSchemaIsolation: true` | Máxima segurança para Users |

## 🛡️ Estrutura de Segurança

- **users_role**: Acesso exclusivo ao schema `users`
- **meajudaai_app_role**: Acesso cross-cutting para operações gerais
- **Isolamento**: Schema `users` isolado de outros dados
- **Search path**: `users,public` - prioriza dados do módulo

A solução **aproveita totalmente** sua infraestrutura existente! 🚀