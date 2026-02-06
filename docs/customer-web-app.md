# Customer Web App

**Aplicação pública Next.js 15** para clientes e prestadores de serviços.

---

## 🎯 Visão Geral

O Customer Web App é a interface pública da plataforma MeAjudaAi, construída com React 19 e Next.js 15. Permite que clientes busquem prestadores de serviços e que prestadores gerenciem seus perfis e interajam com clientes.

### Público-Alvo

1. **Clientes** (consumidores de serviços)
   - Buscar prestadores por serviço e localização
   - Ver perfis e avaliações de prestadores
   - Solicitar serviços
   - Avaliar prestadores após conclusão

2. **Prestadores** (providers)
   - Gerenciar perfil público
   - Ver solicitações de serviços
   - Responder a clientes
   - Visualizar avaliações recebidas

---

## 🚀 Stack Tecnológico

### Core
- **React 19** - Server Components + Client Components
- **Next.js 15** - App Router, SSR/SSG, Image Optimization
- **TypeScript 5.7+** - Strict mode, type safety

### Styling
- **Tailwind CSS v4** - Utility-first CSS com `@theme` inline
- **Tailwind Variants** - Type-safe component variants
- **Tailwind Merge** - Intelligent class merging
- **clsx** - Conditional class composition

### State & Data
- **Zustand** - Client state management (planejado)
- **TanStack Query v5** - Server state, caching, mutations (planejado)
- **React Hook Form** - Form management
- **Zod** - Schema validation

### UI & Icons
- **Lucide React** - Icon library
- **Custom components** - Design system baseado no Figma

### Authentication
- **NextAuth.js v5** - Authentication via Keycloak OIDC (planejado)

---

## 🎨 Design System

### Cores (do Figma)

```css
/* globals.css */
@theme inline {
  --color-primary: #355873;              /* Azul escuro */
  --color-primary-foreground: #ffffff;
  --color-primary-hover: #2a4660;
  
  --color-secondary: #d06704;            /* Laranja */
  --color-secondary-light: #f2ae72;      /* Laranja claro */
  --color-secondary-foreground: #ffffff;
  --color-secondary-hover: #b85703;
  
  --color-foreground: #2e2e2e;           /* Texto principal */
  --color-foreground-subtle: #666666;    /* Texto secundário */
  
  --color-border: #e0e0e0;
  --color-surface: #ffffff;
  --color-surface-raised: #f5f5f5;
}
```

### Componentes Base

| Componente | Variantes | Uso |
|------------|-----------|-----|
| **Button** | primary (orange), secondary (blue), outline, ghost | CTAs, ações |
| **Card** | padding: none, sm, md, lg | Containers de conteúdo |
| **Input** | default, error | Formulários |
| **Badge** | default, primary, secondary, success, warning | Tags, status |
| **Rating** | 1-5 estrelas | Avaliações |
| **Avatar** | sm, md, lg, xl | Fotos de perfil |

---

## 📁 Estrutura do Projeto

```text
src/Web/meajudaai-web-customer/
├── app/                          # Next.js App Router
│   ├── layout.tsx                # Root layout (Header + Footer)
│   ├── page.tsx                  # Home page
│   ├── globals.css               # Tailwind v4 + Design tokens
│   ├── buscar/
│   │   └── page.tsx              # Search page
│   ├── prestador/
│   │   └── [id]/
│   │       └── page.tsx          # Provider profile
│   └── api/
│       └── auth/
│           └── [...nextauth]/    # NextAuth.js routes (planejado)
├── components/
│   ├── ui/                       # Base components
│   │   ├── button.tsx
│   │   ├── card.tsx
│   │   ├── input.tsx
│   │   ├── badge.tsx
│   │   ├── rating.tsx
│   │   └── avatar.tsx
│   ├── layout/                   # Layout components
│   │   ├── header.tsx
│   │   └── footer.tsx
│   ├── providers/                # Provider-specific
│   │   ├── provider-card.tsx
│   │   └── provider-grid.tsx
│   └── reviews/                  # Review components (planejado)
├── lib/
│   └── utils/
│       └── cn.ts                 # Class name utility (clsx + twMerge)
├── types/
│   └── api/
│       └── provider.ts           # TypeScript types (temporário)
├── package.json
├── tsconfig.json
├── tailwind.config.ts
├── postcss.config.mjs
└── next.config.ts
```

---

## 🚀 Como Rodar

### Opção 1: Via Aspire (Recomendado)

```powershell
# Inicia toda a stack (API, Admin, Customer Web, Keycloak, PostgreSQL, Redis, RabbitMQ)
.\scripts\dev.ps1
```

Acesse: http://localhost:3000/

### Opção 2: Standalone (Desenvolvimento)

```powershell
cd src/Web/meajudaai-web-customer

# Primeira vez: instalar dependências
npm install

# Desenvolvimento
npm run dev

# Build de produção
npm run build
npm run start

# Lint
npm run lint
```

### Variáveis de Ambiente

Crie `.env.local`:

```bash
# API Backend
NEXT_PUBLIC_API_URL=http://localhost:7524

# NextAuth.js (quando implementado)
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-secret-here

# Keycloak
KEYCLOAK_CLIENT_ID=meajudaai-customer
KEYCLOAK_CLIENT_SECRET=your-secret
KEYCLOAK_ISSUER=http://localhost:8080/realms/meajudaai
```

---

## 🔗 Integração com Backend

### OpenAPI TypeScript Generator (Planejado)

Tipos TypeScript serão gerados automaticamente do backend .NET:

```bash
# Gerar tipos do OpenAPI spec
npx openapi-typescript-codegen \
  --input http://localhost:7524/swagger/v1/swagger.json \
  --output ./types/api/generated \
  --client fetch
```

**Resultado**:
```typescript
// types/api/generated/models/ProviderDto.ts
export type ProviderDto = {
  id: string;
  name: string;
  email: string;
  averageRating: number;
  reviewCount: number;
  services: ServiceDto[];
  city: string;
  state: string;
  // ... auto-generated from C# DTOs
}
```

### API Client (Planejado)

```typescript
// lib/api/providers.ts
import { getSession } from "next-auth/react";
import type { ProviderDto } from "@/types/api/generated";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

async function getAuthHeaders() {
  const session = await getSession();
  return {
    "Authorization": `Bearer ${session?.accessToken}`,
    "Content-Type": "application/json"
  };
}

export async function searchProviders(query: string): Promise<ProviderDto[]> {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/api/providers/search`, {
    method: "POST",
    headers,
    body: JSON.stringify({ query })
  });
  return response.json();
}
```

---

## 🔐 Autenticação (Planejado)

### NextAuth.js + Keycloak

```typescript
// app/api/auth/[...nextauth]/route.ts
import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

export const authOptions = {
  providers: [
    KeycloakProvider({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET!,
      issuer: process.env.KEYCLOAK_ISSUER,
    })
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken;
      return session;
    }
  }
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
```

### Protected Routes

```typescript
// middleware.ts
export { default } from "next-auth/middleware";

export const config = {
  matcher: ["/perfil/:path*", "/prestador/editar/:path*"]
};
```

---

## 📊 Páginas Implementadas

### ✅ Home (`/`)
- Hero section com busca
- "Como funciona?" (3 cards)
- CTA para prestadores

### ✅ Busca (`/buscar`)
- Filtros (serviço, cidade)
- Grid de prestadores
- Paginação (planejado)

### ✅ Perfil do Prestador (`/prestador/[id]`)
- Informações completas
- Avaliações
- Botões de contato

### 🔄 Planejadas
- `/perfil/editar` - Editar perfil (prestador)
- `/login` - Login
- `/cadastro` - Cadastro
- `/servicos` - Catálogo de serviços

---

## ✅ Acessibilidade

- ✅ ARIA labels em inputs de busca
- ✅ `htmlFor`/`id` associations em labels
- ✅ `role="img"` e `aria-label` em avatars com iniciais
- ✅ Semantic HTML (header, footer, main)
- ✅ Keyboard navigation support

---

## 🧪 Testes (Planejado)

```bash
# Jest + React Testing Library
npm run test

# E2E com Playwright
npm run test:e2e

# Storybook
npm run storybook
```

---

## 📦 Build & Deploy

### Build de Produção

```bash
npm run build
# Output: .next/ directory
```

### Deploy (Opções)

1. **Vercel** (recomendado para Next.js)
2. **Azure Static Web Apps**
3. **Docker** (via Aspire auto-generated Dockerfile)

---

## 🐳 Aspire Integration

O Customer Web App está integrado ao Aspire via `AddJavaScriptApp()`:

```csharp
// src/Aspire/MeAjudaAi.AppHost/Program.cs
var customerWebPath = Path.Combine(builder.AppHostDirectory, "..", "..", "Web", "meajudaai-web-customer");
_ = builder.AddJavaScriptApp("customer-web", customerWebPath)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("NEXT_PUBLIC_API_URL", apiService.GetEndpoint("http"))
    .WaitFor(apiService);
```

**Benefícios**:
- ✅ Orquestração automática com backend
- ✅ Service discovery (API URL injetada automaticamente)
- ✅ Observabilidade (logs, traces, metrics no Aspire Dashboard)
- ✅ Dockerfile auto-gerado para produção
- ✅ Hot Module Replacement em desenvolvimento

---

## 🔄 Próximos Passos

### Sprint 8A (Restante)
- [ ] NextAuth.js + Keycloak integration
- [ ] OpenAPI TypeScript generator setup
- [ ] API client implementation
- [ ] Replace mock data com API calls
- [ ] Protected routes
- [ ] Edit profile page
- [ ] Login/Cadastro pages

### Sprint 8B (Mobile)
- [ ] React Native + Expo setup
- [ ] Compartilhar componentes com Web
- [ ] Native navigation
- [ ] Push notifications

---

## 📚 Referências

- [Next.js 15 Documentation](https://nextjs.org/docs)
- [React 19 Documentation](https://react.dev/)
- [Tailwind CSS v4](https://tailwindcss.com/docs)
- [NextAuth.js v5](https://next-auth.js.org/)
- [Aspire for JavaScript Developers](https://devblogs.microsoft.com/aspire/aspire-for-javascript-developers/)
