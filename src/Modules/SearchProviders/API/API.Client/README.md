# MeAjudaAi SearchProviders API Client

Esta coleção do Bruno contém todos os endpoints do módulo de busca de prestadores da aplicação MeAjudaAi.

## 📁 Estrutura da Collection

```
API.Client/
├── README.md                    # Documentação completa  
└── SearchAdmin/
    ├── SearchProviders.bru      # POST /api/v1/search
    ├── SearchByRadius.bru       # POST /api/v1/search/radius
    ├── IndexProvider.bru        # POST /api/v1/search/providers/{id}/index
    └── RemoveProvider.bru       # DELETE /api/v1/search/providers/{id}
```

**🔗 Recursos Compartilhados (em `src/Shared/API.Collections/`):**
- `Setup/SetupGetKeycloakToken.bru` - Autenticação Keycloak

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/v1/search` | Buscar prestadores por critérios | AllowAnonymous |
| POST | `/api/v1/search/radius` | Buscar por raio geográfico | AllowAnonymous |
| POST | `/api/v1/search/providers/{id}/index` | Indexar prestador (admin) | AdminOnly |
| DELETE | `/api/v1/search/providers/{id}` | Remover do índice (admin) | AdminOnly |

## 🎯 Lógica de Ranking

A busca ordena resultados por:
1. **SubscriptionTier** (Platinum > Gold > Standard > Free)
2. **AverageRating** (descendente)
3. **Distance** (crescente) - desempate

## 🔧 Variáveis da Collection

```
baseUrl: http://localhost:5000
accessToken: [AUTO-SET by shared setup]
providerId: [CONFIGURE_AQUI]
latitude: -21.1306  # Muriaé, MG
longitude: -42.3667
```

---

**📝 Última atualização**: Novembro 2025  
**🏗️ Versão da API**: v1
