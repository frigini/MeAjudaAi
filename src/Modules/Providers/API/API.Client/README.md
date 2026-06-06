# API.Client - Módulo Providers

Esta pasta contém coleções Bruno (`.bru`) para testar os endpoints do módulo Providers.

## 📁 Estrutura

```text
API.Client/
├── collection.bru.example  # Template de variáveis (copie para collection.bru)
├── collection.bru          # Variáveis globais (não versionado - crie local)
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

1. **Instale o Bruno**: [https://usebruno.com/](https://usebruno.com/)
2. **Abra a pasta** `API.Client` no Bruno.
3. **Configure o Ambiente**:
   - Utilize o arquivo de ambiente compartilhado: `src/Shared/API.Collections/environments/Local.bru`.
   - No Bruno, você pode importar este arquivo como um novo ambiente.
4. **Obtenha o Token**:
   - Abra a coleção `src/Shared/API.Collections`.
   - Execute `Setup/SetupGetKeycloakToken.bru` usando o ambiente `Local`.
5. **Execute os Endpoints**: Certifique-se de que a coleção `Providers API` está usando o mesmo ambiente `Local`.

## 🔐 Autenticação

Todos os endpoints requerem autenticação via Bearer Token. O token é gerenciado automaticamente via variável de ambiente `accessToken` após a execução do setup compartilhado.


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