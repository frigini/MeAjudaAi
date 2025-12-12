# API Specifications

This directory contains API specifications and related documentation for the MeAjudaAi project.

## Files Overview

### OpenAPI Specifications
- **`api-spec.json`** - Generated OpenAPI 3.x specification for the entire API surface

## File Descriptions

### API Specification (`api-spec.json`)
Complete OpenAPI specification containing:
- **All endpoints** across all modules (Users, Organizations, etc.)
- **Request/response schemas** with detailed examples
- **Authentication requirements** for each endpoint
- **Health check endpoints** (health, ready, live)
- **Error response formats** with proper HTTP status codes

## Generation

⚠️ **IMPORTANTE**: O arquivo `api-spec.json` deve ser atualizado sempre que houver mudanças nos endpoints da API.

### Quando Atualizar
Atualize o arquivo após:
- ✅ Adicionar novos endpoints
- ✅ Modificar schemas de request/response
- ✅ Alterar rotas ou métodos HTTP
- ✅ Modificar validações ou DTOs
- ✅ Atualizar documentação XML dos endpoints

### Como Atualizar

```bash
# Windows: Gerar OpenAPI spec + Postman Collections
cd tools/api-collections
.\generate-all-collections.bat

# Linux/macOS: Gerar OpenAPI spec + Postman Collections
cd tools/api-collections
./generate-all-collections.sh

# Apenas OpenAPI (sem Collections)
cd tools/api-collections
npm install
node generate-postman-collections.js

# Após gerar, commitar as mudanças
git add api/api-spec.json
git commit -m "docs: atualizar especificação OpenAPI"
```

### Automação (GitHub Pages)

#### 🤖 Geração Automática
O `api-spec.json` é **automaticamente atualizado** via GitHub Actions sempre que houver mudanças em:
- Controllers, endpoints, DTOs
- Requests, Responses, schemas  
- Qualquer arquivo em `src/**/API/`

**Workflow**: `.github/workflows/update-api-docs.yml`

#### 🔄 Processo Automatizado
1. ✅ Detecta mudanças em endpoints (via `paths` no workflow)
2. 🔨 Builda a aplicação (Release mode)
3. 📄 Gera `api-spec.json` via Swashbuckle CLI
4. ✅ Valida JSON e conta endpoints
5. 💾 Commita automaticamente (com `[skip ci]`)
6. 🚀 Faz deploy para GitHub Pages com ReDoc

#### 📚 URLs Publicadas
- 📖 **ReDoc (interativo)**: https://frigini.github.io/MeAjudaAi/api/
- 📄 **OpenAPI JSON**: https://frigini.github.io/MeAjudaAi/api/api-spec.json
- 🔄 **Atualização**: Automática a cada push na branch main

## Features

### Offline Generation
- No need to run the application
- Works from compiled assemblies
- Always reflects current codebase

### Client Integration
Compatible with popular API clients:
- **APIDog** - Import for advanced testing
- **Postman** - Generate collections automatically
- **Insomnia** - REST client integration
- **Bruno** - Open-source API client
- **Thunder Client** - VS Code extension

### Development Benefits
- **Realistic examples** in request/response schemas
- **Complete type information** for all DTOs
- **Authentication schemes** clearly documented
- **Error handling patterns** standardized

## Usage Patterns

### For Frontend Development
```bash
# Generate spec for frontend team
./scripts/export-openapi.ps1 -OutputPath "api/frontend-api.json"
# Frontend team imports into their preferred client
```

### For API Testing
```bash
# Generate spec for QA testing
./scripts/export-openapi.ps1 -OutputPath "api/test-api.json"
# Import into Postman/APIDog for comprehensive testing
```

### For Documentation
```bash
# Generate spec for documentation site
./scripts/export-openapi.ps1 -OutputPath "docs/api-reference.json"
# Use with Swagger UI or similar documentation tools
```

## Version Control

API specification files are **not version controlled** (included in .gitignore) because:
- They are generated artifacts
- Always reflect current codebase state
- Avoid merge conflicts
- Regenerated on demand

## Structure Purpose

This directory provides a dedicated location for API-related artifacts, making it clear where to find and generate API specifications for different use cases.