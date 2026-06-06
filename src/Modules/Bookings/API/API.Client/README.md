# MeAjudaAi Bookings API Client

Esta coleção do Bruno contém os endpoints para o módulo de agendamentos.

## 🚀 Setup Inicial

1. **Abra a coleção** `src/Shared/API.Collections` no Bruno.
2. **Selecione o ambiente** `Local`.
3. **Execute** `Setup/SetupGetKeycloakToken.bru` para obter o token.
4. **Nesta coleção**, selecione o mesmo ambiente `Local` para herdar `baseUrl` e `accessToken`.

## 📁 Estrutura da Coleção

```text
API.Client/
├── collection.bru.example       # Template de configuração
├── collection.bru               # Configuração local (não versionado)
├── README.md                    # Este arquivo
└── Bookings/                    # Endpoints de agendamento
```

## 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/bookings` | Criar um novo agendamento |
| GET | `/api/v1/bookings/{id}` | Buscar agendamento por ID |
| GET | `/api/v1/bookings/my` | Listar meus agendamentos |
| PUT | `/api/v1/bookings/{id}/confirm` | Confirmar agendamento |
| PUT | `/api/v1/bookings/{id}/reject` | Rejeitar agendamento |
| PUT | `/api/v1/bookings/{id}/cancel` | Cancelar agendamento |
| PUT | `/api/v1/bookings/{id}/complete` | Concluir agendamento |
| GET | `/api/v1/bookings/availability/{providerId}` | Consultar disponibilidade |
| POST | `/api/v1/bookings/schedule` | Definir agenda do prestador |
