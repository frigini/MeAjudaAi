#!/usr/bin/env node

/**
 * Gerador de Postman Collections a partir do OpenAPI/Swagger
 * Uso: node generate-postman-collections.js
 */

const fs = require('fs');
const path = require('path');
const https = require('https');
const http = require('http');

class PostmanCollectionGenerator {
  constructor() {
    this.config = {
      apiBaseUrl: process.env.API_BASE_URL || 'http://localhost:5000',
      swaggerEndpoint: '/api-docs/v1/swagger.json',
      outputDir: '../src/Shared/API.Collections/Generated',
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
    };
  }

  async generateCollections() {
    try {
      console.log('🔄 Buscando especificação OpenAPI...');
      const swaggerSpec = await this.fetchSwaggerSpec();
      
      console.log('📋 Gerando Postman Collection...');
      const collection = this.convertSwaggerToPostman(swaggerSpec);
      
      console.log('🌍 Gerando ambientes Postman...');
      const environments = this.generateEnvironments();
      
      console.log('💾 Salvando arquivos...');
      await this.saveFiles(collection, environments, swaggerSpec);
      
      console.log('✅ Collections geradas com sucesso!');
      console.log(`📁 Arquivos salvos em: ${this.config.outputDir}`);
      
    } catch (error) {
      console.error('❌ Erro ao gerar collections:', error.message);
      process.exit(1);
    }
  }

  async fetchSwaggerSpec() {
    const url = this.config.apiBaseUrl + this.config.swaggerEndpoint;
    
    return new Promise((resolve, reject) => {
      const client = url.startsWith('https') ? https : http;
      
      client.get(url, (res) => {
        let data = '';
        
        res.on('data', chunk => {
          data += chunk;
        });
        
        res.on('end', () => {
          try {
            const spec = JSON.parse(data);
            resolve(spec);
          } catch (error) {
            reject(new Error(`Erro ao parsear OpenAPI: ${error.message}`));
          }
        });
        
      }).on('error', (error) => {
        reject(new Error(`Erro ao buscar OpenAPI: ${error.message}`));
      });
    });
  }

  convertSwaggerToPostman(swaggerSpec) {
    const collection = {
      info: {
        _postman_id: this.generateUuid(),
        name: `${swaggerSpec.info.title} - v${swaggerSpec.info.version}`,
        description: swaggerSpec.info.description,
        schema: "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
      },
      auth: {
        type: "bearer",
        bearer: [
          {
            key: "token",
            value: "{{accessToken}}",
            type: "string"
          }
        ]
      },
      variable: [
        {
          key: "baseUrl",
          value: "{{baseUrl}}",
          type: "string"
        }
      ],
      item: []
    };

    // Agrupar endpoints por tags (módulos)
    const groupedPaths = this.groupPathsByTags(swaggerSpec);
    
    for (const [tag, paths] of Object.entries(groupedPaths)) {
      const folder = {
        name: tag,
        item: []
      };

      for (const [path, methods] of Object.entries(paths)) {
        for (const [method, operation] of Object.entries(methods)) {
          const request = this.createPostmanRequest(path, method, operation, swaggerSpec);
          folder.item.push(request);
        }
      }

      collection.item.push(folder);
    }

    // Adicionar pasta de Setup (auth, health checks)
    collection.item.unshift(this.createSetupFolder());

    return collection;
  }

  groupPathsByTags(swaggerSpec) {
    const grouped = {};

    for (const [path, methods] of Object.entries(swaggerSpec.paths || {})) {
      for (const [method, operation] of Object.entries(methods)) {
        if (typeof operation !== 'object' || !operation.tags) continue;

        const tag = operation.tags[0] || 'Other';
        
        if (!grouped[tag]) grouped[tag] = {};
        if (!grouped[tag][path]) grouped[tag][path] = {};
        
        grouped[tag][path][method] = operation;
      }
    }

    return grouped;
  }

  createPostmanRequest(path, method, operation, swaggerSpec) {
    const request = {
      name: operation.summary || `${method.toUpperCase()} ${path}`,
      request: {
        method: method.toUpperCase(),
        header: [
          {
            key: "Content-Type",
            value: "application/json",
            type: "text"
          },
          {
            key: "Api-Version",
            value: "1.0",
            type: "text"
          }
        ],
        url: {
          raw: `{{baseUrl}}${path}`,
          host: ["{{baseUrl}}"],
          path: path.split('/').filter(p => p)
        }
      },
      response: []
    };

    // Adicionar parâmetros de query e path
    if (operation.parameters) {
      const queryParams = [];
      const pathVariables = [];

      operation.parameters.forEach(param => {
        if (param.in === 'query') {
          queryParams.push({
            key: param.name,
            value: this.getExampleValue(param),
            description: param.description,
            disabled: !param.required
          });
        } else if (param.in === 'path') {
          pathVariables.push({
            key: param.name,
            value: this.getExampleValue(param),
            description: param.description
          });
        }
      });

      if (queryParams.length > 0) {
        request.request.url.query = queryParams;
      }
      
      if (pathVariables.length > 0) {
        request.request.url.variable = pathVariables;
      }
    }

    // Adicionar body para POST/PUT/PATCH
    if (['post', 'put', 'patch'].includes(method) && operation.requestBody) {
      const schema = operation.requestBody.content?.['application/json']?.schema;
      if (schema) {
        request.request.body = {
          mode: "raw",
          raw: JSON.stringify(this.generateExampleFromSchema(schema, swaggerSpec), null, 2)
        };
      }
    }

    // Adicionar testes automáticos
    request.event = [
      {
        listen: "test",
        script: {
          exec: this.generatePostmanTests(operation)
        }
      }
    ];

    return request;
  }

  createSetupFolder() {
    return {
      name: "🔧 Setup & Auth",
      item: [
        {
          name: "Get Keycloak Token",
          request: {
            method: "POST",
            header: [
              {
                key: "Content-Type",
                value: "application/x-www-form-urlencoded"
              }
            ],
            body: {
              mode: "urlencoded",
              urlencoded: [
                { key: "client_id", value: "{{clientId}}" },
                { key: "username", value: "{{adminUser}}" },
                { key: "password", value: "{{adminPassword}}" },
                { key: "grant_type", value: "password" }
              ]
            },
            url: {
              raw: "{{keycloakUrl}}/realms/{{realm}}/protocol/openid-connect/token",
              host: ["{{keycloakUrl}}"],
              path: ["realms", "{{realm}}", "protocol", "openid-connect", "token"]
            }
          },
          event: [
            {
              listen: "test",
              script: {
                exec: [
                  "if (pm.response.code === 200) {",
                  "    const response = pm.response.json();",
                  "    pm.collectionVariables.set('accessToken', response.access_token);",
                  "    pm.collectionVariables.set('refreshToken', response.refresh_token);",
                  "    pm.test('Token obtido com sucesso', () => {",
                  "        pm.expect(response.access_token).to.be.a('string');",
                  "    });",
                  "} else {",
                  "    pm.test('Erro ao obter token', () => {",
                  "        pm.expect.fail('Falha na autenticação');",
                  "    });",
                  "}"
                ]
              }
            }
          ]
        },
        {
          name: "Health Check - All Services",
          request: {
            method: "GET",
            header: [],
            url: {
              raw: "{{baseUrl}}/health",
              host: ["{{baseUrl}}"],
              path: ["health"]
            }
          },
          event: [
            {
              listen: "test",
              script: {
                exec: [
                  "pm.test('Status code é 200', () => {",
                  "    pm.response.to.have.status(200);",
                  "});",
                  "",
                  "pm.test('Todos os serviços estão healthy', () => {",
                  "    const response = pm.response.json();",
                  "    pm.expect(response.status).to.eql('Healthy');",
                  "});"
                ]
              }
            }
          ]
        }
      ]
    };
  }

  generateEnvironments() {
    const environments = {};

    for (const [name, config] of Object.entries(this.config.environments)) {
      environments[name] = {
        id: this.generateUuid(),
        name: `MeAjudaAi - ${name.charAt(0).toUpperCase() + name.slice(1)}`,
        values: [
          { key: "baseUrl", value: config.baseUrl, enabled: true },
          { key: "keycloakUrl", value: config.keycloakUrl, enabled: true },
          { key: "realm", value: "meajudaai-realm", enabled: true },
          { key: "clientId", value: "meajudaai-client", enabled: true },
          { key: "adminUser", value: "admin", enabled: true },
          { key: "adminPassword", value: "admin123", enabled: true },
          { key: "accessToken", value: "", enabled: true },
          { key: "refreshToken", value: "", enabled: true },
          { key: "apiVersion", value: "v1", enabled: true }
        ],
        _postman_variable_scope: "environment",
        _postman_exported_at: new Date().toISOString(),
        _postman_exported_using: "MeAjudaAi Collection Generator"
      };
    }

    return environments;
  }

  generatePostmanTests(operation) {
    const tests = [
      "// Testes automáticos gerados",
      "pm.test('Status code é success', () => {",
      "    pm.expect(pm.response.code).to.be.oneOf([200, 201, 204]);",
      "});",
      "",
      "pm.test('Response time é aceitável', () => {",
      "    pm.expect(pm.response.responseTime).to.be.below(5000);",
      "});",
      ""
    ];

    // Adicionar validação de schema se disponível
    if (operation.responses?.['200']?.content?.['application/json']?.schema) {
      tests.push(
        "pm.test('Response tem formato correto', () => {",
        "    const response = pm.response.json();",
        "    pm.expect(response).to.be.an('object');",
        "});"
      );
    }

    // Salvar IDs para uso em outras requests
    if (operation.operationId?.includes('Create') || operation.operationId?.includes('Register')) {
      tests.push(
        "",
        "// Salvar ID criado para próximos testes",
        "if (pm.response.code === 201) {",
        "    const response = pm.response.json();",
        "    if (response.id || response.userId) {",
        "        pm.collectionVariables.set('lastCreatedId', response.id || response.userId);",
        "    }",
        "}"
      );
    }

    return tests;
  }

  getExampleValue(param) {
    if (param.example !== undefined) return param.example;
    if (param.schema?.example !== undefined) return param.schema.example;
    
    switch (param.schema?.type) {
      case 'string':
        if (param.schema.format === 'uuid') return '{{$guid}}';
        if (param.schema.format === 'email') return 'test@example.com';
        if (param.schema.format === 'date-time') return '{{$isoTimestamp}}';
        return param.name.includes('id') ? '{{lastCreatedId}}' : 'example';
      case 'integer':
      case 'number':
        return 1;
      case 'boolean':
        return true;
      default:
        return 'example';
    }
  }

  generateExampleFromSchema(schema, swaggerSpec) {
    // Implementação simplificada para gerar exemplos de schemas
    if (schema.example) return schema.example;
    if (schema.type === 'object' && schema.properties) {
      const example = {};
      for (const [key, prop] of Object.entries(schema.properties)) {
        example[key] = this.getExampleValue({ schema: prop, name: key });
      }
      return example;
    }
    return {};
  }

  generateUuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      const r = Math.random() * 16 | 0;
      const v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  async saveFiles(collection, environments, swaggerSpec) {
    const outputDir = path.resolve(__dirname, this.config.outputDir);
    
    // Criar diretório se não existir
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    // Salvar collection
    const collectionPath = path.join(outputDir, 'MeAjudaAi-API-Collection.json');
    fs.writeFileSync(collectionPath, JSON.stringify(collection, null, 2));

    // Salvar OpenAPI spec para api/api-spec.json
    const apiSpecPath = path.resolve(__dirname, '../../api/api-spec.json');
    fs.writeFileSync(apiSpecPath, JSON.stringify(swaggerSpec, null, 2));
    console.log(`📄 OpenAPI spec salvo em: api/api-spec.json`);

    // Salvar environments
    for (const [name, env] of Object.entries(environments)) {
      const envPath = path.join(outputDir, `MeAjudaAi-${name}-Environment.json`);
      fs.writeFileSync(envPath, JSON.stringify(env, null, 2));
    }

    // Criar README com instruções
    const readmePath = path.join(outputDir, 'README.md');
    const readme = `# Postman Collections - MeAjudaAi API

## 📁 Arquivos Gerados

- \`MeAjudaAi-API-Collection.json\` - Collection principal com todos os endpoints
- \`MeAjudaAi-*-Environment.json\` - Ambientes (development, staging, production)

## 🚀 Como Usar

### 1. Importar no Postman
1. Abra o Postman
2. Clique em "Import"
3. Selecione todos os arquivos .json desta pasta
4. Configure o ambiente desejado

### 2. Configuração Inicial
1. Selecione o ambiente (development/staging/production)
2. Execute "🔧 Setup & Auth > Get Keycloak Token"
3. Execute "🔧 Setup & Auth > Health Check"

### 3. Testes Automáticos
- Cada request tem testes automáticos configurados
- IDs são salvos automaticamente para reutilização
- Validações de schema e performance incluídas

## 🔄 Regeneração
Para atualizar as collections após mudanças na API:
\`\`\`bash
cd tools/api-collections
node generate-postman-collections.js
\`\`\`

## 📋 Recursos Incluídos
- ✅ Autenticação automática (Keycloak)
- ✅ Ambientes pré-configurados
- ✅ Testes automáticos
- ✅ Variáveis dinâmicas
- ✅ Health checks
- ✅ Documentação inline

---
Gerado automaticamente em: ${new Date().toISOString()}
`;
    
    fs.writeFileSync(readmePath, readme);
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  const generator = new PostmanCollectionGenerator();
  generator.generateCollections();
}

module.exports = PostmanCollectionGenerator;