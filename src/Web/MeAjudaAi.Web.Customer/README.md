# MeAjudaAi - Customer Web App

Esta é a aplicação web para clientes do sistema **MeAjudaAi**, desenvolvida com [Next.js](https://nextjs.org) e [Tailwind CSS v4](https://tailwindcss.com).

## Estrutura do Projeto

A aplicação utiliza a estrutura de diretórios do Next.js App Router com Grupos de Rotas:

-   `app/(main)/`: Contém as páginas principais (Home, Buscar, Perfil, Prestador) que compartilham o layout padrão.
-   `app/(auth)/`: Contém as páginas de autenticação (Login, Cadastro).
-   `components/`: Componentes React reutilizáveis.
    -   `ui/`: Componentes de interface base (Design System).
    -   `profile/`, `providers/`, `reviews/`: Componentes específicos por funcionalidade.
-   `lib/`: Utilitários, mappers e cliente API gerado.
-   `types/`: Definições de tipos TypeScript.

## Configuração

### Requisitos

-   Node.js 18+
-   npm / yarn / pnpm

### Variáveis de Ambiente

Crie um arquivo `.env.local` na raiz com:

```env
NEXT_PUBLIC_API_URL=http://localhost:7002
OPENAPI_SPEC_URL=http://localhost:7002/api-docs/v1/swagger.json

# Keycloak Authentication (NextAuth)
KEYCLOAK_ISSUER=http://localhost:8080/realms/meajudaai
KEYCLOAK_CLIENT_ID=meajudaai-web
KEYCLOAK_CLIENT_SECRET=your_client_secret_here
```

### Instalação e Desenvolvimento

```bash
npm install
npm run dev
```

## Funcionalidades Principais

-   🔍 **Busca de Prestadores**: Filtre prestadores por cidade, estado e tipo de serviço.
-   ⭐ **Avaliações**: Visualize e envie avaliações para os prestadores.
-   👤 **Gerenciamento de Perfil**: Edite suas informações pessoais.
-   🔐 **Autenticação**: Integração com Keycloak via NextAuth.js.

## Padrões de Código

-   **Tailwind v4**: Estilos declarativos diretamente no CSS (`globals.css`).
-   **API Client**: Código gerado automaticamente a partir do Swagger/OpenAPI.
-   **TypeScript**: Tipagem estrita em toda a aplicação.

### Geração e Atualização do Cliente API

-   Para regenerar o cliente API após mudanças no backend:
    ```bash
    npm run generate:api
    ```
-   Sempre que possível, reutilize enums e constantes de `Shared.Contracts` para manter alinhamento com o backend.
