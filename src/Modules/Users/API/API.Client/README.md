# MeAjudaAi API Client

Esta coleção do Bruno contém todos os endpoints do módulo de usuários da aplicação MeAjudaAi.

## 📁 Estrutura da Collection

```
API.Client/
├── collection.bru.example       # Template de configuração (copie para collection.bru)
├── collection.bru               # Configuração local (não versionado - criar local)
├── README.md                    # Documentação completa  
└── UserAdmin/
    ├── GetUsers.bru            # GET /api/v1/users (paginado)
    ├── CreateUser.bru          # POST /api/v1/users
    ├── GetUserById.bru         # GET /api/v1/users/{id}
    ├── GetUserByEmail.bru      # GET /api/v1/users/by-email/{email}
    ├── UpdateUser.bru          # PUT /api/v1/users/{id}
    └── DeleteUser.bru          # DELETE /api/v1/users/{id}
```

**🔗 Recursos Compartilhados (em `src/Shared/API.Collections/`):**
- `Setup/SetupGetKeycloakToken.bru` - Autenticação Keycloak
- `Common/GlobalVariables.bru` - Variáveis globais  
- `Common/StandardHeaders.bru` - Headers padrão

## 🚀 Como usar esta coleção

### 1. Pré-requisitos
- [Bruno](https://www.usebruno.com/) instalado
- Aplicação MeAjudaAi rodando localmente
- Keycloak configurado e rodando

### 2. Configuração Inicial

#### ⚡ **PRIMEIRO: Crie seu arquivo de configuração local**
```bash
# No diretório API.Client
cp collection.bru.example collection.bru
```

#### ⚡ **Execute PRIMEIRO a configuração compartilhada**
1. **Navegue para**: `src/Shared/API.Collections/Setup/`
2. **Execute**: `SetupGetKeycloakToken.bru` para autenticar
3. **Resultado**: Token de acesso será definido automaticamente para TODOS os módulos

#### Configurações Globais (Compartilhadas):
- **Variáveis**: `src/Shared/API.Collections/Common/GlobalVariables.bru`
- **Headers Padrão**: `src/Shared/API.Collections/Common/StandardHeaders.bru`
- **Autenticação**: `src/Shared/API.Collections/Setup/SetupGetKeycloakToken.bru`

#### Iniciar a aplicação:
```bash
# Na raiz do projeto
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```

#### URLs principais:
- **API**: [http://localhost:5545](http://localhost:5545)
- **Aspire Dashboard**: [https://localhost:17063](https://localhost:17063)
- **Keycloak**: [http://localhost:8080](http://localhost:8080)

### 3. Executar Endpoints dos Usuários

Uma vez que o token foi obtido na configuração compartilhada, todos os endpoints desta coleção herdarão automaticamente:
- ✅ Token de autenticação
- ✅ Headers padrão
- ✅ Variáveis globais
- ✅ Configurações de timeout e retry

Como a autenticação é gerenciada pelo **Keycloak**, você precisa obter um token válido:

#### Opção A: Via Keycloak Admin Console
1. Acesse: [http://localhost:8080/admin](http://localhost:8080/admin)
2. Login: `admin` / `admin123`
3. Vá para: Realm `meajudaai-realm` > Users
4. Crie ou selecione um usuário
5. Copie o token da sessão

#### Opção B: Via Keycloak REST API
```bash
curl -X POST "http://localhost:8080/realms/meajudaai-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=meajudaai-client" \
  -d "username=SEU_USERNAME" \
  -d "password=SUA_SENHA"
```

#### Opção C: Via Aspire Dashboard
1. Acesse: [https://localhost:17063](https://localhost:17063)
2. Verifique logs do Keycloak
3. Encontre tokens nos logs de autenticação

### 4. Configurar Token no Bruno

1. Abra a collection no Bruno
2. Vá em **Environment/Variables**
3. Cole o `access_token` na variável `accessToken`
4. Configure outras variáveis se necessário:
   - `userId`: ID de um usuário válido
   - `testEmail`: Email de um usuário existente

### 5. Executar Endpoints

#### Sequência Recomendada:
1. **SetupGetKeycloakToken** - Obter token do Keycloak primeiro
2. **GetUsers** - Listar usuários existentes
3. **CreateUser** - Criar novo usuário (admin)
4. **GetUserById** - Buscar usuário criado
5. **UpdateUser** - Atualizar dados
6. **GetUserByEmail** - Buscar por email
7. **DeleteUser** - Remover usuário (admin)

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/v1/users` | Listar usuários (paginado) | SelfOrAdmin |
| POST | `/api/v1/users` | Criar usuário | AdminOnly |
| GET | `/api/v1/users/{id}` | Buscar por ID | SelfOrAdmin |
| GET | `/api/v1/users/by-email/{email}` | Buscar por email | AdminOnly |
| PUT | `/api/v1/users/{id}` | Atualizar usuário | SelfOrAdmin |
| DELETE | `/api/v1/users/{id}` | Deletar usuário | AdminOnly |

## 🔒 Políticas de Autorização

- **AllowAnonymous**: Sem autenticação necessária
- **AdminOnly**: Apenas administradores
- **SelfOrAdmin**: Usuário pode acessar próprios dados OU admin acessa qualquer
- **RequireAuthorization**: Token válido obrigatório

## 🔧 Variáveis da Collection

```
baseUrl: http://localhost:5000
keycloakUrl: http://localhost:8080
realm: meajudaai-realm
clientId: meajudaai-client
adminUser: admin
adminPassword: admin123
accessToken: [CONFIGURE_AQUI]
userId: [CONFIGURE_AQUI]
testEmail: test@example.com
```

## 🚨 Troubleshooting

### Erro 401 (Unauthorized)
- Verifique se o token está configurado
- Confirme se o token não expirou
- Teste obter novo token do Keycloak

### Erro 403 (Forbidden)
- Verifique se o usuário tem as permissões necessárias
- Confirme a política de autorização do endpoint
- Para endpoints AdminOnly, use token de administrador

### Erro 404 (Not Found)
- Confirme se a aplicação está rodando
- Verifique se os IDs/emails existem
- Execute "Get Users" primeiro para ver dados disponíveis

### Erro 500 (Internal Server Error)
- Verifique logs no Aspire Dashboard
- Confirme se o banco de dados está disponível
- Verifique se o Keycloak está respondendo

## 📚 Documentação Adicional

- **Aspire Dashboard**: [https://localhost:17063](https://localhost:17063)
- **Keycloak Admin**: [http://localhost:8080/admin](http://localhost:8080/admin)
- **OpenAPI/Swagger**: [http://localhost:5545/swagger](http://localhost:5545/swagger) (se habilitado)

## 🎯 Próximos Passos

1. **Teste todos os endpoints** para validar funcionamento
2. **Configure ambientes** (dev, prod)
3. **Adicione testes automatizados** no Bruno
4. **Documente cenários de erro** específicos
5. **Crie scripts de setup** para dados de teste

---

**📝 Última atualização**: September 2025  
**🏗️ Versão da API**: v1  
**🔧 Bruno Version**: Compatível com versões recentes