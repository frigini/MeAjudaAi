# Keycloak Client Setup - Admin Portal

## Pré-requisitos
- Keycloak rodando em `http://localhost:8080`
- Realm `meajudaai` criado e configurado

## Configuração do Client

### 1. Acessar Keycloak Admin Console
- URL: `http://localhost:8080/admin`
- Login: `admin` / `admin123` (padrão desenvolvimento)

### 2. Selecionar Realm
- No menu superior esquerdo, selecionar realm **meajudaai**

### 3. Criar Client
Navegar para **Clients** → **Create client**

#### **General Settings**
- **Client type**: `OpenID Connect`
- **Client ID**: `admin-portal`
- **Name**: `MeAjudaAi Admin Portal`
- **Description**: `Portal administrativo para gestão da plataforma MeAjudaAi`

Clicar em **Next**

#### **Capability config**
- **Client authentication**: `OFF` (aplicação pública Blazor WASM)
- **Authorization**: `OFF`
- **Authentication flow**:
  - ✅ **Standard flow** (Authorization Code)
  - ✅ **Direct access grants** (Resource Owner Password)
  - ❌ Implicit flow (deprecated)
  - ❌ Service accounts roles

Clicar em **Next**

#### **Login settings**
- **Root URL**: `https://localhost:7281`
- **Home URL**: `https://localhost:7281`
- **Valid redirect URIs**: 
  - `https://localhost:7281/*`
  - `https://localhost:7281/authentication/login-callback`
  - `http://localhost:5281/*` (HTTP fallback)
- **Valid post logout redirect URIs**:
  - `https://localhost:7281/*`
  - `https://localhost:7281/authentication/logout-callback`
- **Web origins**: 
  - `https://localhost:7281`
  - `http://localhost:5281`

Clicar em **Save**

### 4. Configurar Roles (Opcional)
Navegar para **Clients** → **admin-portal** → **Roles**

Criar roles:
- `admin` - Administrador total
- `operator` - Operador (leitura/escrita limitada)
- `viewer` - Visualizador (somente leitura)

### 5. Mapear Roles no Token
Navegar para **Clients** → **admin-portal** → **Client scopes** → **admin-portal-dedicated**

#### Adicionar Mapper de Roles
1. Clicar em **Add mapper** → **By configuration**
2. Selecionar **User Realm Role**
3. Configurar:
   - **Name**: `realm-roles`
   - **Token Claim Name**: `roles`
   - **Claim JSON Type**: `String`
   - **Add to ID token**: `ON`
   - **Add to access token**: `ON`
   - **Add to userinfo**: `ON`

Clicar em **Save**

### 6. Criar Usuário Admin de Teste
Navegar para **Users** → **Add user**

#### User Details
- **Username**: `admin.portal`
- **Email**: `admin@meajudaai.local`
- **First name**: `Admin`
- **Last name**: `Portal`
- **Email verified**: `ON`
- **Enabled**: `ON`

Clicar em **Create**

#### Definir Senha
1. Navegar para tab **Credentials**
2. Clicar em **Set password**
3. **Password**: `admin123` (desenvolvimento)
4. **Temporary**: `OFF`
5. Clicar em **Save**

#### Atribuir Roles
1. Navegar para tab **Role mapping**
2. Clicar em **Assign role**
3. Filtrar por **Filter by clients**
4. Selecionar `admin-portal: admin`
5. Clicar em **Assign**

## Configuração do Blazor WASM

### appsettings.json
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/meajudaai",
    "ClientId": "admin-portal",
    "ResponseType": "code",
    "Scope": "openid profile email roles"
  },
  "ApiBaseUrl": "https://localhost:7524"
}
```

### Program.cs
```csharp
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.UserOptions.RoleClaim = "roles";
});
```

## Testes de Autenticação

### 1. Iniciar Aplicação
```powershell
# Aspire AppHost (recomendado)
dotnet run --project src/Aspire/MeAjudaAi.AppHost

# Ou standalone
dotnet run --project src/Web/MeAjudaAi.Web.Admin
```

### 2. Acessar Admin Portal
- URL: `https://localhost:7281`
- Será redirecionado para Keycloak login

### 3. Login
- **Username**: `admin.portal`
- **Password**: `admin123`

### 4. Verificar Token
Após login bem-sucedido:
- Abrir DevTools (F12)
- Console: `localStorage.getItem('oidc.user:http://localhost:8080/realms/meajudaai:admin-portal')`
- Verificar presença de `access_token`, `id_token`, `roles`

## Troubleshooting

### Erro: "Invalid redirect URI"
- Verificar **Valid redirect URIs** no client
- Adicionar wildcards: `https://localhost:7281/*`

### Erro: "CORS policy"
- Verificar **Web origins** no client
- Adicionar: `https://localhost:7281` e `http://localhost:5281`

### Token sem "roles" claim
- Verificar mapper **realm-roles** em **Client scopes**
- Verificar user tem role atribuído em **Role mapping**

### Login loop infinito
- Verificar **Client authentication** = `OFF` (public client)
- Limpar localStorage do browser
- Verificar Authority URL correto (com `/realms/meajudaai`)

## Referências
- [Keycloak Documentation](https://www.keycloak.org/docs/latest/server_admin/)
- [Microsoft OIDC Blazor WASM](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library)
- [Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/)
