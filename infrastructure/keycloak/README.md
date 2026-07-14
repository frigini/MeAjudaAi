# Keycloak Configuration

Este diretório contém configurações do Keycloak para autenticação OIDC da plataforma MeAjudaAi.

## 🚀 Setup Automatizado

**A configuração do Keycloak é feita automaticamente pelo código!**

Quando você executa `.\scripts\dev.ps1`, o AppHost configura automaticamente:
- ✅ Realm `meajudaai`
- ✅ Clients OIDC: `admin-portal` e `customer-app`
- ✅ Roles: admin, customer, operator, viewer
- ✅ Usuários demo para desenvolvimento

**Código:** [`src/Aspire/MeAjudaAi.AppHost/Extensions/KeycloakExtensions.cs`](../../src/Aspire/MeAjudaAi.AppHost/Extensions/KeycloakExtensions.cs)

**Documentação completa:** [`docs/keycloak-admin-portal-setup.md`](../../docs/keycloak-admin-portal-setup.md)

---

## 📁 Estrutura de Arquivos

```text
keycloak/
├── README.md                     # Este arquivo
├── realms/                       # Realm configurations (JSON exports)
│   ├── meajudaai-realm.dev.json  # Development realm
│   └── meajudaai-realm.prod.json # Production realm
├── scripts/
│   └── keycloak-init-prod.sh     # Production initialization
└── themes/                       # Custom Keycloak themes
    └── meajudaai/                # MeAjudaAi custom theme
```

---

## ⚙️ Realm Configuration Details

### Development Realm (`meajudaai-realm.dev.json`)

**Configurações**:
- `verifyEmail: false` - Email não verificado (facilita desenvolvimento)
- `users`: Contém usuário seed `admin.portal` (senha: `admin123`)
- Redirect URIs: localhost + domínio de produção
- Web Origins: localhost + domínio de produção

🔴 **AVISO CRÍTICO DE SEGURANÇA - Usuário Seed**:

O usuário `admin.portal` com senha `admin123` é **EXCLUSIVAMENTE PARA DESENVOLVIMENTO LOCAL**.

**Ações Obrigatórias**:
1. **Alterar a senha imediatamente** no primeiro login em qualquer ambiente acessível
2. **Remover o usuário seed** antes de deployment em staging/produção
3. **NUNCA aplicar** o realm dev (`meajudaai-realm.dev.json`) em ambientes expostos
4. **Rotacionar credenciais/segredos** se o realm dev for acidentalmente exposto

**Como remover o seed**:
- Produção: Use `meajudaai-realm.prod.json` (não contém usuários seed)
- Staging: Remova a seção `"users": [...]` do realm antes de importar

⚠️ **Importante**: As origens de produção (`https://admin.meajudaai.com.br`) estão incluídas no realm dev para facilitar testes de integração com ambientes híbridos. Se não forem necessárias, remover para reduzir superfície de ataque.

### Production Realm (`meajudaai-realm.prod.json`)

**Configurações**:
- `verifyEmail: true` - ⚠️ **REQUER SMTP configurado**
- `accessTokenLifespan: 900` (15 min)
- `ssoSessionIdleTimeout: 1800` (30 min)
- `ssoSessionMaxLifespan: 36000` (10h)
- `users: []` - ⚠️ **SEM USUÁRIOS SEED**

⚠️ **Checklist de Deployment de Produção**:

1. **SMTP Configuration** (obrigatório para `verifyEmail: true`):
   ```bash
   KC_SMTP_HOST=smtp.example.com
   KC_SMTP_PORT=587
   KC_SMTP_FROM=noreply@meajudaai.com.br
   KC_SMTP_USER=...
   KC_SMTP_PASSWORD=...
   ```

2. **Usuário Administrativo Inicial**:
   - Garantir mecanismo de provisionamento antes do rollout
   - Opções: Admin Console, variáveis de ambiente, ou script de bootstrap
   - Ver seção "Production Environment" abaixo

3. **Validação de Session Lifespans**:
   - Valores atuais refletem balance entre segurança e UX
   - Ajustar conforme requisitos específicos de negócio

---

## 🔒 Security Architecture

### Environment-Specific Realm Files

- **`meajudaai-realm.dev.json`**: Development realm with demo users and non-sensitive test data
- **`meajudaai-realm.prod.json`**: Production realm without secrets or demo users

### Secure Secret Management

Secrets are **NEVER** stored in realm files. Instead:
- Development: Scripts inject development-safe secrets
- Production: Secrets are provided via environment variables at runtime

## 🚀 Usage

### Development Environment

The development realm is imported **automatically** by Aspire (`KeycloakExtensions.cs`) when you run `.\scripts\dev.ps1`.

No manual steps needed — Aspire mounts the realm files, imports them via `--import-realm`, and resolves client secrets from environment variables.

### Production Environment

```bash
# 1. Set required environment variables
export MEAJUDAAI_API_CLIENT_SECRET="$(openssl rand -base64 32)"
export MEAJUDAAI_WEB_REDIRECT_URIS="https://yourapp.com/*,https://api.yourapp.com/*"
export MEAJUDAAI_WEB_ORIGINS="https://yourapp.com,https://api.yourapp.com"
export INITIAL_ADMIN_USERNAME="admin"
export INITIAL_ADMIN_PASSWORD="$(openssl rand -base64 32)"
export INITIAL_ADMIN_EMAIL="admin@yourcompany.com"

# 2. Run production initialization script (after Keycloak starts)
./scripts/keycloak-init-prod.sh
```

## 🔐 Production Security Features

- **SSL Required**: All connections must use HTTPS
- **Registration Disabled**: No self-registration allowed
- **Strong Password Policy**: Enforced complexity requirements
- **No Hardcoded Secrets**: All secrets generated at runtime
- **No Demo Users**: Clean production environment
- **Secret Rotation**: Secrets can be updated via environment variables

## 📋 Required Environment Variables

### Production
- `KEYCLOAK_ADMIN`: Keycloak admin username (defaults to `admin`, can be overridden)
- `KEYCLOAK_ADMIN_PASSWORD`: Keycloak admin password
- `MEAJUDAAI_API_CLIENT_SECRET`: API client secret
- `MEAJUDAAI_WEB_REDIRECT_URIS`: Comma-separated redirect URIs
- `MEAJUDAAI_WEB_ORIGINS`: Comma-separated web origins
- `INITIAL_ADMIN_USERNAME`: Initial admin username (optional)
- `INITIAL_ADMIN_PASSWORD`: Initial admin password (optional)
- `INITIAL_ADMIN_EMAIL`: Initial admin email (optional)

### Development
- `KEYCLOAK_ADMIN`: Keycloak admin username (defaults to `admin`, can be overridden)
- `KEYCLOAK_ADMIN_PASSWORD`: Keycloak admin password
- `MEAJUDAAI_API_CLIENT_SECRET`: API client secret (optional, defaults to dev secret)

## 🛡️ Security Best Practices

1. **Never commit secrets**: All realm files are secret-free
2. **Environment separation**: Different configurations per environment
3. **Runtime secret injection**: Secrets added after realm import
4. **Strong password policies**: Enforced in production
5. **Minimal permissions**: Only necessary redirect URIs and origins
6. **Audit logging**: All admin actions are logged
