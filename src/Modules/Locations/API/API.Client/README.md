# MeAjudaAi Locations API Client

Coleção Bruno para serviços de localização (CEP lookup, validação geográfica).

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
