### 🔍 **Baixa Prioridade (12+ meses - Fase 3)**
1. 📅 Service Requests & Booking
2. 📱 Mobile Apps (iOS/Android nativo)
3. 🧠 Recomendações com ML
4. 🎮 Gamificação avançada
5. 💬 Chat interno
6. 🌐 Internacionalização (i18n)
7. 🧪 Evolução da Infraestrutura de Testes

---

## 📚 Referências e Recursos

### 📖 Documentação Relacionada
- **Arquitetura**: [`docs/architecture.md`](./architecture.md) - Princípios e padrões arquiteturais
- **Desenvolvimento**: [`docs/development.md`](./development.md) - Guia de setup e workflow
- **Autenticação**: [`docs/authentication-and-authorization.md`](./authentication-and-authorization.md) - Keycloak e OIDC
- **CI/CD**: [`docs/ci-cd.md`](./ci-cd.md) - Pipeline e deployment

### 🔥 Ferramentas e Tecnologias
- **.NET 10.0** - Runtime principal (migrado de .NET 9.0)
- **PostgreSQL + PostGIS** - Database com suporte geoespacial
- **Keycloak** - Identity & Access Management
- **Stripe** - Payment processing
- **Azure Blob Storage** - Document storage
- **OpenTelemetry + Aspire** - Observability

### 🌐 APIs Externas
- **IBGE Localidades API** - Validação oficial de municípios brasileiros
  - Base URL: `https://servicodados.ibge.gov.br/api/v1/localidades/`
  - Documentação: <https://servicodados.ibge.gov.br/api/docs/localidades>
  - Uso: Validação geográfica para restrição de cidades piloto
- **Nominatim (OpenStreetMap)** - Geocoding (planejado para Sprint 4 - optional improvement)
  - Base URL: `https://nominatim.openstreetmap.org/`
  - Documentação: <https://nominatim.org/release-docs/latest/>
  - Uso: Geocoding (lat/lon lookup) para cidades/endereços
  - **Note**: Post-MVP feature, não é blocker para geographic-restriction inicial
- **ViaCep API** - Lookup de CEP brasileiro
  - Base URL: `https://viacep.com.br/ws/`
  - Documentação: <https://viacep.com.br/>
- **BrasilApi CEP** - Lookup de CEP (fallback)
  - Base URL: `https://brasilapi.com.br/api/cep/v1/`
  - Documentação: <https://brasilapi.com.br/docs>
- **OpenCep API** - Lookup de CEP (fallback)
  - Base URL: `https://opencep.com/v1/`
  - Documentação: <https://opencep.com/>

---

## 🧪 Evolução da Infraestrutura de Testes

O planejamento de qualidade prevê a evolução contínua da pirâmide de testes:

### Fase 3.1: Cobertura e Contratos (Curto/Médio Prazo)
- **Meta de Cobertura**: Manter >70% de cobertura global no MVP e atingir >80% na Fase 3.
- **Testes de Contrato (Consumer-Driven)**: Implementação de Pact.io para garantir que o frontend Next.js é compatível com a API .NET sem depender de ambiente real.

### Fase 3.2: E2E Full-Stack & BDD (Médio Prazo)
- **Orquestração com Aspire**: Migração dos testes E2E para rodarem contra instâncias locais orquestradas pelo .NET Aspire (subindo PostgreSQL + PostGIS, API e Redis em containers temporários durante o suite).
- **BDD (Behavior Driven Development)**: Adoção de Gherkin (Cucumber ou Playwright-BDD) para os fluxos críticos de negócio (ex: ciclo de vida do serviço).

---

*📅 Última atualização: 25 de Março de 2026 (Pós-Sprint 8E)*  
*🔄 Roadmap em constante evolução baseado em feedback, métricas e aprendizados*
*📊 Status atual: Sprint 9 🔄 EM ANDAMENTO | MVP Launch em 12-16 de Maio de 2026*
