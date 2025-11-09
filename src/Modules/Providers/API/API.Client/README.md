# API.Client - Módulo Providers

Esta pasta contém coleções Bruno (`.bru`) para testar os endpoints do módulo Providers.

## 📁 Estrutura

```
API.Client/
├── collection.bru          # Variáveis globais da coleção
├── README.md               # Este arquivo  
└── ProviderAdmin/          # Endpoints administrativos
    ├── CreateProvider.bru
    ├── DeleteProvider.bru
    ├── GetProviders.bru
    ├── GetProviderById.bru
    ├── GetProviderByUserId.bru
    ├── GetProvidersByCity.bru
    ├── GetProvidersByState.bru
    ├── GetProvidersByType.bru
    ├── GetProvidersByVerificationStatus.bru
    ├── UpdateProviderProfile.bru
    ├── UpdateVerificationStatus.bru
    ├── AddDocument.bru
    └── RemoveDocument.bru
```

## 🚀 Como Usar

1. **Instale o Bruno**: https://usebruno.com/
2. **Abra a pasta** `API.Client` no Bruno
3. **Configure as variáveis** em `collection.bru`:
   - `baseUrl`: URL da API (padrão: http://localhost:5000)
   - `accessToken`: Token JWT obtido após autenticação
   - Outras variáveis conforme necessário

## 🔐 Autenticação

Todos os endpoints requerem autenticação via Bearer Token:

```
Authorization: Bearer {{accessToken}}
```

Para obter um token:
1. Use o endpoint de autenticação do Keycloak
2. Configure a variável `accessToken` com o token retornado

## 📝 Exemplos de Uso

### Listar Prestadores
```http
GET /api/v1/providers?pageNumber=1&pageSize=10&name=joão
```

### Buscar por Cidade
```http
GET /api/v1/providers/by-city/São Paulo
```

### Filtrar por Tipo
```http
GET /api/v1/providers/by-type/1
```

## 🏷️ Tags e Organização

- **ProviderAdmin**: Operações administrativas de prestadores
- Endpoints organizados por funcionalidade
- Documentação inline em cada arquivo `.bru`

## 🧪 Testes

Cada endpoint inclui:
- Exemplos de request/response
- Códigos de status esperados
- Documentação dos parâmetros
- Cenários de teste comuns