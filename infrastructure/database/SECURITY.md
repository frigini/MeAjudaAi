# 🔒 Database Security Guidelines

## Padrão de Segurança para Arquivos SQL

### ⚠️ **NUNCA fazer**

```sql
-- ❌ ERRADO: Senhas hardcoded ou placeholders inseguros
CREATE ROLE some_role LOGIN PASSWORD 'password123';
CREATE ROLE some_role LOGIN PASSWORD '<secure_password>';
```

### ✅ **Padrão seguro**

```sql
-- ✅ CORRETO: Roles NOLOGIN para agrupamento de permissões
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'module_role') THEN
        CREATE ROLE module_role NOLOGIN;
    END IF;
END
$$;
```

## Princípios de Segurança

### 1. **Roles NOLOGIN**
- Use `NOLOGIN` roles para agrupamento de permissões
- Nunca inclua senhas em arquivos de schema
- Senhas devem ser gerenciadas através de:
  - Variáveis de ambiente
  - Azure Key Vault (produção)
  - Ferramentas de configuração segura

### 2. **Operações Idempotentes**
```sql
-- Verifica se o role existe antes de criar
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'role_name') THEN
        CREATE ROLE role_name NOLOGIN;
    END IF;
END
$$;

-- Verifica se o grant já existe antes de aplicar
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'parent_role' AND r2.rolname = 'child_role'
    ) THEN
        GRANT child_role TO parent_role;
    END IF;
END
$$;
```

### 3. **Estrutura por Módulo**
```
database/modules/
├── users/
│   ├── 00-roles.sql        # Roles e hierarquia
│   └── 01-permissions.sql  # Permissões específicas
└── [future_modules]/
    ├── 00-roles.sql
    └── 01-permissions.sql
```

### 4. **Nomenclatura Padrão**
- **Module roles**: `{module}_role` (ex: `users_role`)
- **App role**: `meajudaai_app_role` (cross-cutting)
- **Schemas**: Nome do módulo (ex: `users`, `providers`)

## Exemplos Práticos

### Role de Módulo
```sql
-- Cria role do módulo se não existir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'users_role') THEN
        CREATE ROLE users_role NOLOGIN;
    END IF;
END
$$;
```

### Permissões de Schema
```sql
-- Concede permissões no schema
GRANT USAGE ON SCHEMA users TO users_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO users_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO users_role;

-- Privilégios padrão para objetos futuros
ALTER DEFAULT PRIVILEGES FOR ROLE users_role IN SCHEMA users 
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO users_role;
```

### Cross-Module Access
```sql
-- Role da aplicação para acesso cross-module
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'meajudaai_app_role') THEN
        CREATE ROLE meajudaai_app_role NOLOGIN;
    END IF;
END
$$;

-- Grant idempotente
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'meajudaai_app_role' AND r2.rolname = 'users_role'
    ) THEN
        GRANT users_role TO meajudaai_app_role;
    END IF;
END
$$;
```

## Deployment em Produção

### Configuração de Application User
```bash
# As senhas devem ser configuradas via environment variables
export DB_APP_PASSWORD=$(openssl rand -base64 32)

# Ou via Azure Key Vault/secrets management
psql -c "ALTER ROLE meajudaai_app_user PASSWORD '$DB_APP_PASSWORD';"
```

### Connection Strings
```csharp
// ✅ CORRETO: Via configuração segura
"ConnectionStrings:DefaultConnection": "Host=localhost;Database=meajudaai;Username=app_user;Password=${DB_PASSWORD}"

// ❌ ERRADO: Senha hardcoded
"ConnectionStrings:DefaultConnection": "Host=localhost;Database=meajudaai;Username=app_user;Password=password123"
```

---

**⚠️ Lembre-se**: Nunca commite senhas reais no controle de versão. Use sempre ferramentas de configuração segura em produção.