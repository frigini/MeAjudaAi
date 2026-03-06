### ≡ƒö« **Baixa Prioridade (12+ meses - Fase 3)**
1. ≡ƒôà Service Requests & Booking
2. ≡ƒô▒ Mobile Apps (iOS/Android nativo)
3. ≡ƒºá Recomenda├º├╡es com ML
4. ≡ƒÄ« Gamifica├º├úo avan├ºada
5. ≡ƒÆ¼ Chat interno
6. ≡ƒîÉ Internacionaliza├º├úo

---

## ≡ƒôÜ Refer├¬ncias e Recursos

### ≡ƒôû Documenta├º├úo Relacionada
- **Arquitetura**: [`docs/architecture.md`](./architecture.md) - Princ├¡pios e padr├╡es arquiteturais
- **Desenvolvimento**: [`docs/development.md`](./development.md) - Guia de setup e workflow
- **Autentica├º├úo**: [`docs/authentication-and-authorization.md`](./authentication-and-authorization.md) - Keycloak e OIDC
- **CI/CD**: [`docs/ci-cd.md`](./ci-cd.md) - Pipeline e deployment

### ≡ƒöº Ferramentas e Tecnologias
- **.NET 10.0** - Runtime principal (migrado de .NET 9.0)
- **PostgreSQL + PostGIS** - Database com suporte geoespacial
- **Keycloak** - Identity & Access Management
- **Stripe** - Payment processing
- **Azure Blob Storage** - Document storage
- **OpenTelemetry + Aspire** - Observability

### ≡ƒîÉ APIs Externas
- **IBGE Localidades API** - Valida├º├úo oficial de munic├¡pios brasileiros
  - Base URL: `https://servicodados.ibge.gov.br/api/v1/localidades/`
  - Documenta├º├úo: <https://servicodados.ibge.gov.br/api/docs/localidades>
  - Uso: Valida├º├úo geogr├ífica para restri├º├úo de cidades piloto
- **Nominatim (OpenStreetMap)** - Geocoding (planejado para Sprint 4 - optional improvement)
  - Base URL: `https://nominatim.openstreetmap.org/`
  - Documenta├º├úo: <https://nominatim.org/release-docs/latest/>
  - Uso: Geocoding (lat/lon lookup) para cidades/endere├ºos
  - **Note**: Post-MVP feature, n├úo ├⌐ blocker para geographic-restriction inicial
- **ViaCep API** - Lookup de CEP brasileiro
  - Base URL: `https://viacep.com.br/ws/`
  - Documenta├º├úo: <https://viacep.com.br/>
- **BrasilApi CEP** - Lookup de CEP (fallback)
  - Base URL: `https://brasilapi.com.br/api/cep/v1/`
  - Documenta├º├úo: <https://brasilapi.com.br/docs>
- **OpenCep API** - Lookup de CEP (fallback)
  - Base URL: `https://opencep.com/v1/`
  - Documenta├º├úo: <https://opencep.com/>

---

*≡ƒôà ├Ültima atualiza├º├úo: 5 de Mar├ºo de 2026 (Sprint 8B Conclusion Review)*  
*≡ƒöä Roadmap em constante evolu├º├úo baseado em feedback, m├⌐tricas e aprendizados*
*≡ƒôè Status atual: Sprint 8B Γ£à CONCLU├ìDO | MVP Launch em 28 de Mar├ºo de 2026*
