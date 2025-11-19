# 📬 API Collections Generator

Gerador automático de coleções Postman a partir da especificação OpenAPI/Swagger da API do MeAjudaAi.

## 📋 Visão Geral

Esta ferramenta Node.js lê a especificação OpenAPI da API em execução e gera:
- **Coleções Postman** organizadas por módulo
- **Ambientes** (development, staging, production)
- **Variáveis** pré-configuradas (baseUrl, tokens, etc.)
- **Requests** com exemplos e documentação

## 🚀 Uso Rápido

### 1. Instalar Dependências

```bash
cd tools/api-collections
npm install
```

### 2. Iniciar a API

```bash
# Em outro terminal, na raiz do projeto
cd src/Bootstrapper/MeAjudaAi.ApiService
dotnet run
```

Aguarde a API iniciar (geralmente em `http://localhost:5000`).

### 3. Gerar Coleções

```bash
# Windows
.\generate-all-collections.bat

# Linux/macOS
./generate-all-collections.sh

# Ou diretamente com Node.js
node generate-postman-collections.js
```

## 📂 Output

As coleções são geradas em:
```text
src/Shared/API.Collections/Generated/
├── MeAjudaAi-Users-Collection.json
├── MeAjudaAi-Providers-Collection.json
├── MeAjudaAi-Documents-Collection.json
├── MeAjudaAi-Complete-Collection.json
└── environments/
    ├── development.json
    ├── staging.json
    └── production.json
```

## 📥 Importar no Postman

1. Abra o Postman
2. **File** → **Import**
3. Selecione os arquivos `.json` gerados
4. Configure o ambiente desejado (development/staging/production)

## ⚙️ Configuração

### Variáveis de Ambiente

Você pode customizar a geração via variáveis de ambiente:

```bash
# URL da API (padrão: http://localhost:5000)
export API_BASE_URL=http://localhost:5000

# Endpoint do Swagger (padrão: /api-docs/v1/swagger.json)
export SWAGGER_ENDPOINT=/api-docs/v1/swagger.json

# Executar
node generate-postman-collections.js
```

### Editar Ambientes

Edite `generate-postman-collections.js` para customizar ambientes:

```javascript
environments: {
  development: {
    baseUrl: 'http://localhost:5000',
    keycloakUrl: 'http://localhost:8080'
  },
  staging: {
    baseUrl: 'https://api-staging.meajudaai.com',
    keycloakUrl: 'https://auth-staging.meajudaai.com'
  },
  production: {
    baseUrl: 'https://api.meajudaai.com',
    keycloakUrl: 'https://auth.meajudaai.com'
  }
}
```

## 🔍 Estrutura da Coleção

Cada coleção gerada contém:

### Pasta por Módulo
```text
📁 Users
  ├── 📄 GET /api/v1/users
  ├── 📄 GET /api/v1/users/{id}
  ├── 📄 POST /api/v1/users
  └── ...

📁 Providers
  ├── 📄 GET /api/v1/providers
  └── ...

📁 Documents
  ├── 📄 POST /api/v1/documents/upload
  ├── 📄 GET /api/v1/documents/status/{id}
  └── ...
```

### Requests com
- ✅ Headers automáticos (Authorization, Content-Type)
- ✅ Exemplos de request/response
- ✅ Descrições da documentação OpenAPI
- ✅ Variáveis de ambiente ({{baseUrl}}, {{token}})

## 🛠️ Desenvolvimento

### Estrutura do Código

```javascript
class PostmanCollectionGenerator {
  fetchSwaggerSpec()      // Busca spec OpenAPI da API
  generateCollection()    // Converte OpenAPI → Postman
  generateEnvironments()  // Cria arquivos de ambiente
  saveCollection()        // Salva arquivos JSON
}
```

### Adicionar Novo Módulo

Os módulos são detectados automaticamente do OpenAPI. Basta adicionar endpoints com tags:

```csharp
// No controller
[Tags("NovoModulo")]
[Route("api/v{version:apiVersion}/novomodulo")]
public class NovoModuloController : ControllerBase
```

### Troubleshooting

#### Erro: "Cannot fetch swagger spec"
- Certifique-se de que a API está rodando
- Verifique a URL: `http://localhost:5000/api-docs/v1/swagger.json`
- Confirme que o Swagger está habilitado em Development

#### Erro: "Module X not found"
- Verifique se o controller tem a tag correta: `[Tags("X")]`
- Confirme que o endpoint está no Swagger: navegue para `/swagger`

#### Coleção vazia
- Verifique se há endpoints públicos (sem `[Authorize]`) para teste
- Confirme que a versão da API está correta (v1)

## 📚 Dependências

```json
{
  "dependencies": {
    "axios": "^1.x",      // HTTP client para fetch da spec
    "fs-extra": "^11.x",  // Operações de arquivo
    "postman-collection": "^4.x"  // Biblioteca oficial Postman
  }
}
```

## 🔄 Atualização Automática

Para manter as coleções sincronizadas:

```bash
# Adicionar ao pre-commit hook
echo "cd tools/api-collections && npm run generate" >> .git/hooks/pre-commit

# Ou criar task no VSCode
{
  "label": "Update Postman Collections",
  "type": "shell",
  "command": "cd tools/api-collections && npm run generate"
}
```

## 📊 CI/CD Integration

```yaml
# .github/workflows/api-collections.yml
- name: Generate API Collections
  run: |
    cd tools/api-collections
    npm install
    npm run generate
    
- name: Upload Collections
  uses: actions/upload-artifact@v3
  with:
    name: postman-collections
    path: src/Shared/API.Collections/Generated/
```

## 📚 Referências

- [Postman Collection Format](https://www.postman.com/collection/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Postman Collection SDK](https://www.postmanlabs.com/postman-collection/)
