# MeAjudaAi SearchProviders API Client

Esta coleção do Bruno contém todos os endpoints do módulo de busca de prestadores da aplicação MeAjudaAi.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token de Admin (necessário para indexação).
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📁 Estrutura da Collection

```text
API.Client/
├── README.md                    # Documentação completa  
└── SearchAdmin/
    ├── SearchProviders.bru      # POST /api/v1/search
    ├── IndexProvider.bru        # POST /api/v1/search/providers/{id}/index
    └── RemoveProvider.bru       # DELETE /api/v1/search/providers/{id}
```

**🔗 Recursos Compartilhados (em `src/Shared/API.Collections/`):**
- `Setup/SetupGetKeycloakToken.bru` - Autenticação Keycloak

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/v1/search` | Buscar prestadores por critérios | AllowAnonymous |
| POST | `/api/v1/search/providers/{id}/index` | Indexar prestador (admin) | AdminOnly |
| DELETE | `/api/v1/search/providers/{id}` | Remover do índice (admin) | AdminOnly |

## 🎯 Lógica de Ranking

A busca ordena resultados por:
1. **SubscriptionTier** (Platinum > Gold > Standard > Free)
2. **AverageRating** (descendente)
3. **Distance** (crescente) - desempate

## 🔧 Variáveis da Collection

```text
baseUrl: http://localhost:5000
accessToken: [AUTO-SET by shared setup]
providerId: [CONFIGURE_AQUI]
latitude: -21.1306  # Muriaé, MG
longitude: -42.3667
```

---

**📝 Última atualização**: Novembro 2025  
**🏗️ Versão da API**: v1
