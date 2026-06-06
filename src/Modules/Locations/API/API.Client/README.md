# MeAjudaAi Locations API Client

Coleção Bruno para serviços de localização (CEP lookup, validação geográfica).

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` (se necessário para endpoints protegidos).
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## Endpoints

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/api/v1/locations/cep/{cep}` | Buscar endereço por CEP | AllowAnonymous |
| POST | `/api/v1/locations/validate-city` | Validar cidade/estado | AllowAnonymous |
| GET | `/api/v1/locations/city/{cityName}` | Detalhes da cidade (IBGE) | AllowAnonymous |

## Provedores CEP
1. ViaCEP (primary)
2. BrasilAPI (fallback)
3. OpenCEP (fallback)

## IBGE Integration
Validação oficial de municípios brasileiros.
