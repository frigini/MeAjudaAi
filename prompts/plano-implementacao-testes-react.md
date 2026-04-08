# Plano de Implementação de Testes - React 19 + TypeScript
## Projeto: MeAjudaAi Web (Monorepo Nx)

## 📋 Sumário

1. [Contexto do Projeto](#contexto-do-projeto)
2. [Decisão Arquitetural](#decisao-arquitetural)
3. [Bibliotecas e Dependências](#bibliotecas-e-dependencias)
4. [Estrutura de Pastas](#estrutura-de-pastas)
5. [Configuração](#configuracao)
6. [Estrutura dos Arquivos de Teste](#estrutura-dos-arquivos-de-teste)
7. [Exemplos Práticos](#exemplos-praticos)
8. [Integração com Pipeline CI/CD](#integração-com-pipeline-cicd)
9. [Comandos Úteis](#comandos-uteis)
10. [Boas Práticas](#boas-praticas)
## 11. Checklist de Implementação

### Fase 1: Fundação (Concluída - Sprint 8E)
- [x] Criar `libs/test-support` (setup, utils, mock-data)
- [x] Configurar `vitest.config.ts` em todos os projetos
- [x] Configurar `project.json` (NX targets) para todos os projetos
- [x] Configurar scripts no `package.json` raiz (`src/Web/`)
- [x] Implementar MSW em `MeAjudaAi.Web.Customer`
- [x] Implementar MSW em `MeAjudaAi.Web.Admin`
- [x] Implementar MSW em `MeAjudaAi.Web.Provider`
- [x] Corrigir infraestrutura de CI/CD (`ci-frontend.yml`, `ci-backend.yml`, `ci-e2e.yml`)
- [x] Implementar agregação de cobertura global (`scripts/merge-coverage.mjs`)

### Fase 2: Cobertura Admin & Provider (Concluída - Sprint 8E)
- [x] Criar testes unitários para hooks Admin
- [x] Criar testes unitários para componentes Admin
- [x] Criar testes unitários para componentes Provider
- [x] Validar funcionamento local e em CI

### Fase 3: Maturidade e E2E Full (Roadmap Futuro)
- [ ] Expandir cobertura unitária para >80%
- [ ] Implementar testes de contrato (Pact)
- [ ] Integrar testes E2E com .NET Aspire (containers locais)
- [ ] Implementar BDD com Gherkin para fluxos críticos

---

## 🏗️ Contexto do Projeto

O projeto está integrado em um **monorepo .NET + Nx** com arquitetura de **monolito modular**. A estrutura possui:

- **Backend .NET** com testes organizados por camada em `tests/`
- **3 projetos React/Next.js** em `src/Web/`:
  - `MeAjudaAi.Web.Customer` — Portal do cliente
  - `MeAjudaAi.Web.Admin` — Portal administrativo (Next.js)
  - `MeAjudaAi.Web.Provider` — Portal do prestador (Next.js)
- **Libs compartilhadas** em `src/Web/libs/`:
  - `auth` — Autenticação compartilhada
  - `e2e-support` — Suporte para testes E2E
  - `assets` — Assets compartilhados

---

## 🎯 Decisão Arquitetural

### ✅ Testes Dentro de Cada Projeto

Os testes ficam **dentro de cada projeto React** em uma pasta `__tests__/`, com infraestrutura compartilhada em `libs/test-support/`.

### Justificativa

1. **Proximidade com o código**: Testes ficam junto do projeto que testam
2. **Independência**: Cada projeto pode rodar seus testes isoladamente
3. **Consistência**: Mesma abordagem dos testes E2E que já existem em `e2e/`
4. **Simplicidade**: Evita path aliases complexos entre projetos separados
5. **Nx-friendly**: Alinhado com a estrutura do monorepo Nx

### Estrutura Completa do Monorepo

```text
MeAjudaAi/
├── src/
│   └── Web/
│       ├── libs/
│       │   ├── test-support/              ← NOVO: Infra compartilhada de testes
│       │   │   ├── src/
│       │   │   │   ├── setup.ts
│       │   │   │   ├── test-utils.tsx
│       │   │   │   ├── mock-data.ts
│       │   │   │   └── index.ts
│       │   │   ├── package.json
│       │   │   └── tsconfig.json
│       │   ├── e2e-support/               ← Já existe
│       │   ├── auth/                      ← Já existe
│       │   └── assets/                    ← Já existe
│       │
│       ├── MeAjudaAi.Web.Customer/
│       │   ├── __tests__/                 ← NOVO: Testes unitários/integração
│       │   │   ├── components/
│       │   │   ├── hooks/
│       │   │   ├── lib/
│       │   │   └── mocks/
│       │   ├── e2e/                       ← Já existe: Testes E2E (Playwright)
│       │   ├── vitest.config.ts           ← NOVO
│       │   ├── app/
│       │   ├── components/
│       │   ├── hooks/
│       │   └── lib/
│       │
│       ├── MeAjudaAi.Web.Admin/
│       │   ├── __tests__/                 ← NOVO
│       │   ├── e2e/                       ← Já existe
│       │   ├── vitest.config.ts           ← NOVO
│       │   └── ...
│       │
│       ├── MeAjudaAi.Web.Provider/
│       │   ├── __tests__/                 ← NOVO
│       │   ├── e2e/                       ← Já existe
│       │   ├── vitest.config.ts           ← NOVO
│       │   └── ...
│       │
│       ├── package.json                   ← Adicionar scripts de teste
│       └── playwright.config.ts           ← Já existe
│
├── tests/                                 ← Testes .NET (não alterado)
└── MeAjudaAi.sln
```

---

## 📦 Bibliotecas e Dependências

### Instalação no `package.json` raiz (`src/Web/`)

```bash
# Testing framework e runners (já instalados: vitest, @vitest/ui, jsdom)
npm install --save-dev @vitest/coverage-v8

# React Testing Library
npm install --save-dev @testing-library/react @testing-library/jest-dom @testing-library/user-event

# Mock Service Worker (para mock de APIs)
npm install --save-dev msw
```

### Pacotes Opcionais

```bash
# Para testes de acessibilidade
npm install --save-dev jest-axe
```

> **Nota**: `vitest`, `@vitest/ui`, `jsdom` e `@playwright/test` já estão instalados no `package.json` raiz.
> **Nota**: Para testes de hooks, utilize o `renderHook` exportado por `test-support` que já inclui o provider do React Query configurado.

---

## 📁 Estrutura de Pastas

### Padrão por Projeto

Cada projeto segue a mesma estrutura interna em `__tests__/`, espelhando o código-fonte:

```
MeAjudaAi.Web.Customer/
├── __tests__/
│   ├── components/
│   │   ├── auth/
│   │   │   ├── login-form.test.tsx
│   │   │   └── customer-register-form.test.tsx
│   │   ├── providers/
│   │   │   ├── provider-card.test.tsx
│   │   │   └── provider-grid.test.tsx
│   │   ├── search/
│   │   │   ├── search-filters.test.tsx
│   │   │   └── city-search.test.tsx
│   │   ├── reviews/
│   │   │   ├── review-card.test.tsx
│   │   │   └── review-form.test.tsx
│   │   └── ui/
│   │       ├── button.test.tsx
│   │       └── input.test.tsx
│   ├── hooks/
│   │   ├── use-via-cep.test.ts
│   │   ├── use-services.test.ts
│   │   ├── use-register-provider.test.ts
│   │   └── use-provider-status.test.ts
│   ├── lib/
│   │   ├── utils/
│   │   │   ├── normalization.test.ts
│   │   │   └── phone.test.ts
│   │   ├── schemas/
│   │   │   └── verification-status.test.ts
│   │   └── services/
│   │       └── geocoding.test.ts
│   └── mocks/
│       ├── handlers.ts
│       └── server.ts
├── e2e/
│   ├── auth.spec.ts
│   ├── onboarding.spec.ts
│   ├── performance.spec.ts
│   ├── profile.spec.ts
│   └── search.spec.ts
├── vitest.config.ts
└── ...
```

### Mapeamento Código Fonte → Testes

```
components/auth/login-form.tsx       →  __tests__/components/auth/login-form.test.tsx
hooks/use-via-cep.ts                 →  __tests__/hooks/use-via-cep.test.ts
lib/utils/phone.ts                   →  __tests__/lib/utils/phone.test.ts
lib/schemas/verification-status.ts   →  __tests__/lib/schemas/verification-status.test.ts
```

### Nomenclatura

- **Testes unitários**: `*.test.tsx` ou `*.test.ts`
- **Testes de integração**: `*.integration.test.tsx`
- **Testes de acessibilidade**: `*.accessibility.test.tsx`
- **Testes E2E**: `*.spec.ts` (dentro de `e2e/`)

---

## ⚙️ Configuração

### 1. `libs/test-support/src/setup.ts`

Setup global compartilhado entre todos os projetos:

```typescript
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, beforeAll, afterAll } from 'vitest';

// Cleanup automático após cada teste
afterEach(() => {
  cleanup();
});

// Mock do matchMedia (necessário para componentes responsivos)
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
});

// Mock do IntersectionObserver (necessário para lazy loading)
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return [];
  }
  unobserve() {}
} as any;

// Mock do ResizeObserver
global.ResizeObserver = class ResizeObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  unobserve() {}
} as any;
```

### 2. `libs/test-support/src/test-utils.tsx`

Custom render com providers comuns:

```typescript
import React, { ReactElement, useMemo } from 'react';
import { render, RenderOptions, renderHook, RenderHookOptions, RenderHookResult } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// Cria um QueryClient limpo para cada teste
function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: Infinity,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

interface AllTheProvidersProps {
  children: React.ReactNode;
  queryClient?: QueryClient;
}

const AllTheProviders = ({ children, queryClient: client }: AllTheProvidersProps) => {
  const queryClient = useMemo(() => client ?? createTestQueryClient(), [client]);
  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllTheProviders, ...options });

const AllTheProvidersWrapper = ({ children }: { children: React.ReactNode }) => (
  <AllTheProviders>{children}</AllTheProviders>
);

function customRenderHook<TProps, TValue>(
  callback: (props: TProps) => TValue,
  options?: Omit<RenderHookOptions<TProps>, 'wrapper'>
): RenderHookResult<TValue, TProps> {
  return renderHook(callback, { wrapper: AllTheProvidersWrapper, ...options });
}

// Re-exporta tudo
export * from '@testing-library/react';
export { customRender as render };
export { customRenderHook as renderHook };
export { createTestQueryClient };
export { AllTheProvidersWrapper };
```

### 3. `libs/test-support/src/mock-data.ts`

Fábricas de objetos de teste compartilhados:

```typescript
// Fábricas de dados de teste reutilizáveis

export function createProvider(overrides = {}) {
  return {
    id: 'test-provider-id',
    name: 'Prestador Teste',
    slug: 'prestador-teste',
    email: 'prestador@teste.com',
    phone: '21999999999',
    verificationStatus: 'Pending',
    ...overrides,
  };
}

export function createUser(overrides = {}) {
  return {
    id: 'test-user-id',
    name: 'Usuário Teste',
    email: 'usuario@teste.com',
    ...overrides,
  };
}

export function createService(overrides = {}) {
  return {
    id: 'test-service-id',
    name: 'Serviço Teste',
    categoryId: 'test-category-id',
    ...overrides,
  };
}

export function createReview(overrides = {}) {
  return {
    id: 'test-review-id',
    rating: 5,
    text: 'Excelente serviço!',
    reviewerName: 'Avaliador Teste',
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}
```

### 4. `libs/test-support/src/index.ts`

```typescript
export * from './test-utils';
export * from './mock-data';
```

### 5. `vitest.config.ts` (por projeto — exemplo Customer)

Cada projeto tem seu próprio `vitest.config.ts`:

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: '../libs/test-support/src/setup.ts',
    css: true,
    include: ['__tests__/**/*.test.{ts,tsx}'],
    exclude: ['node_modules/', '.next/', 'e2e/'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      reportsDirectory: './coverage',
      include: [
        'components/**/*.{ts,tsx}',
        'hooks/**/*.{ts,tsx}',
        'lib/**/*.{ts,tsx}',
      ],
      exclude: [
        'node_modules/',
        '__tests__/',
        'e2e/',
        '.next/',
        '**/*.d.ts',
        '**/*.config.*',
        'lib/api/generated/**',
        'app/**',
        'types/**',
      ],
      thresholds: {
        // Início progressivo — aumentar conforme cobertura cresce
        lines: 50,
        functions: 50,
        branches: 50,
        statements: 50,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './'),
      '@test-support': path.resolve(__dirname, '../libs/test-support/src'),
    },
  },
});
```

### 6. MSW — Mock por projeto

**`__tests__/mocks/handlers.ts`** (exemplo Customer):

```typescript
import { http, HttpResponse } from 'msw';

export const handlers = [
  // Busca de prestadores
  http.get('/api/providers', () => {
    return HttpResponse.json([
      {
        id: '1',
        name: 'Eletricista João',
        slug: 'eletricista-joao',
        verificationStatus: 'Verified',
      },
    ]);
  }),

  // Perfil do prestador
  http.get('/api/providers/:id', ({ params }) => {
    return HttpResponse.json({
      id: params.id,
      name: 'Prestador Teste',
      services: ['Elétrica', 'Hidráulica'],
    });
  }),

  // Busca de CEP via ViaCEP
  http.get('https://viacep.com.br/ws/:cep/json/', ({ params }) => {
    return HttpResponse.json({
      cep: params.cep,
      logradouro: 'Rua Teste',
      bairro: 'Bairro Teste',
      localidade: 'Rio de Janeiro',
      uf: 'RJ',
    });
  }),
];
```

**`__tests__/mocks/server.ts`**:

```typescript
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);
```

### 7. Playwright — Já configurado

O arquivo `playwright.config.ts` na raiz de `src/Web/` já está configurado e funcional. Os testes E2E em `e2e/` de cada projeto continuam como estão.

---

## 📝 Estrutura dos Arquivos de Teste

### Organização Interna dos Testes

```typescript
describe('NomeDoComponente', () => {
  // Testes de renderização
  describe('Rendering', () => {
    it('deve renderizar corretamente', () => {});
  });

  // Testes de interação
  describe('Interactions', () => {
    it('deve chamar callback ao clicar', () => {});
  });

  // Testes de estados
  describe('States', () => {
    it('deve mostrar loading', () => {});
    it('deve mostrar erro', () => {});
  });

  // Testes de acessibilidade (quando aplicável)
  describe('Accessibility', () => {
    it('deve ter roles corretos', () => {});
  });
});
```

---

## 💡 Exemplos Práticos

### Teste de Componente UI

**Componente**: `components/ui/button.tsx`  
**Teste**: `__tests__/components/ui/button.test.tsx`

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@test-support';
import userEvent from '@testing-library/user-event';
import { Button } from '@/components/ui/button';

describe('Button Component', () => {
  it('deve renderizar corretamente', () => {
    render(<Button>Clique aqui</Button>);
    expect(screen.getByRole('button', { name: /clique aqui/i })).toBeInTheDocument();
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Button disabled>Disabled</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('deve chamar onClick quando clicado', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();

    render(<Button onClick={handleClick}>Click me</Button>);
    await user.click(screen.getByRole('button'));

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('não deve chamar onClick quando desabilitado', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();

    render(<Button onClick={handleClick} disabled>Click me</Button>);
    await user.click(screen.getByRole('button'));

    expect(handleClick).not.toHaveBeenCalled();
  });
});
```

### Teste de Utility Function

**Componente**: `lib/utils/phone.ts`  
**Teste**: `__tests__/lib/utils/phone.test.ts`

```typescript
import { describe, it, expect } from 'vitest';
import { formatPhone, validatePhone } from '@/lib/utils/phone';

describe('formatPhone', () => {
  it('deve formatar telefone com DDD', () => {
    expect(formatPhone('21999999999')).toBe('(21) 99999-9999');
  });

  it('deve retornar vazio para entrada inválida', () => {
    expect(formatPhone('')).toBe('');
  });
});

describe('validatePhone', () => {
  it('deve aceitar telefone válido', () => {
    expect(validatePhone('21999999999')).toBe(true);
  });

  it('deve rejeitar telefone com poucos dígitos', () => {
    expect(validatePhone('2199')).toBe(false);
  });
});
```

### Teste de Schema Zod

**Componente**: `lib/schemas/verification-status.ts`  
**Teste**: `__tests__/lib/schemas/verification-status.test.ts`

> Migração do teste ad-hoc existente para o formato Vitest.

```typescript
import { describe, it, expect } from 'vitest';
import { VerificationStatusSchema } from '@/lib/schemas/verification-status';
import { EVerificationStatus } from '@/types/api/provider';

describe('VerificationStatusSchema', () => {
  it.each([
    { input: 0, expected: EVerificationStatus.None },
    { input: 1, expected: EVerificationStatus.Pending },
    { input: '0', expected: EVerificationStatus.None },
    { input: '1', expected: EVerificationStatus.Pending },
    { input: 'verified', expected: EVerificationStatus.Verified },
    { input: 'REJECTED', expected: EVerificationStatus.Rejected },
    { input: 'inprogress', expected: EVerificationStatus.InProgress },
    { input: 'in_progress', expected: EVerificationStatus.InProgress },
    { input: 'suspended', expected: EVerificationStatus.Suspended },
    { input: 'none', expected: EVerificationStatus.None },
    { input: 3, expected: EVerificationStatus.Verified },
  ])('deve converter "$input" para $expected', ({ input, expected }) => {
    const result = VerificationStatusSchema.safeParse(input);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data).toBe(expected);
    }
  });

  it('deve retornar fallback para valores desconhecidos', () => {
    const result = VerificationStatusSchema.safeParse('unknown');
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data).toBe(EVerificationStatus.Pending);
    }
  });

  it.each([null, undefined])('deve tratar %s graciosamente', (input) => {
    const result = VerificationStatusSchema.safeParse(input);
    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
  });
});
```

### Teste de Hook com API (MSW)

**Hook**: `hooks/use-via-cep.ts`  
**Teste**: `__tests__/hooks/use-via-cep.test.ts`

```typescript
import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { renderHook, waitFor } from '@test-support';
import { useViaCep } from '@/hooks/use-via-cep';
import { server } from '../mocks/server';

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('useViaCep Hook', () => {
  it('deve retornar dados do endereço para um CEP válido', async () => {
    const { result } = renderHook(() => useViaCep('20550160'));

    await waitFor(() => {
      expect(result.current.data).toBeDefined();
    });

    expect(result.current.data?.logradouro).toBe('Rua Teste');
    expect(result.current.data?.localidade).toBe('Rio de Janeiro');
  });

  it('deve retornar loading enquanto busca', () => {
    const { result } = renderHook(() => useViaCep('20550160'));
    expect(result.current.isLoading).toBe(true);
  });
});
```

### Teste de Componente de Feature

**Componente**: `components/auth/login-form.tsx`  
**Teste**: `__tests__/components/auth/login-form.test.tsx`

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@test-support';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '@/components/auth/login-form';

describe('LoginForm', () => {
  it('deve renderizar campos de email e senha', () => {
    render(<LoginForm />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument();
  });

  it('deve validar email obrigatório', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.click(screen.getByRole('button', { name: /entrar/i }));

    await waitFor(() => {
      expect(screen.getByText(/campo obrigatório|email.*obrigatório/i)).toBeInTheDocument();
    });
  });

  it('deve validar formato de email', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.type(screen.getByLabelText(/email/i), 'email-invalido');
    await user.click(screen.getByRole('button', { name: /entrar/i }));

    await waitFor(() => {
      expect(screen.getByText(/email inválido/i)).toBeInTheDocument();
    });
  });
});
```

---

## 🔄 Integração com Pipeline CI/CD

### Scripts no `package.json` raiz (`src/Web/`)

```json
{
  "scripts": {
    "test:all": "npm run test:customer && npm run test:admin && npm run test:provider",
    "test": "npm run test:all",
    "test:customer": "cd MeAjudaAi.Web.Customer && npx vitest run --config vitest.config.ts",
    "test:admin": "cd MeAjudaAi.Web.Admin && npx vitest run --config vitest.config.ts",
    "test:provider": "cd MeAjudaAi.Web.Provider && npx vitest run --config vitest.config.ts",
    "test:customer:watch": "cd MeAjudaAi.Web.Customer && npx vitest --config vitest.config.ts",
    "test:admin:watch": "cd MeAjudaAi.Web.Admin && npx vitest --config vitest.config.ts",
    "test:provider:watch": "cd MeAjudaAi.Web.Provider && npx vitest --config vitest.config.ts",
    "test:customer:coverage": "cd MeAjudaAi.Web.Customer && npx vitest run --coverage --config vitest.config.ts",
    "test:admin:coverage": "cd MeAjudaAi.Web.Admin && npx vitest run --coverage --config vitest.config.ts",
    "test:provider:coverage": "cd MeAjudaAi.Web.Provider && npx vitest run --coverage --config vitest.config.ts",
    "test:coverage:all": "npm run test:customer:coverage && npm run test:admin:coverage && npm run test:provider:coverage",
    "test:coverage:merge": "node scripts/merge-coverage.mjs",
    "test:coverage:global": "npm run test:coverage:all && npm run test:coverage:merge",
    "test:ci": "npm run test:coverage:global",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:ci": "playwright test --project=ci --reporter=html --reporter=junit"
  }
}
```

### GitHub Actions

> **Nota**: O projeto utiliza workflows canônicos localizados em `.github/workflows/`:
> - `ci-frontend.yml` - Testes unitários (Vitest) com cobertura consolidada
> - `ci-e2e.yml` - Testes E2E (Playwright) com geração de OpenAPI
>
> Estes workflows implementam as melhores práticas de CI/CD para o monorepo.

---

## 🎯 Comandos Úteis

### Testes Unitários (Vitest)

```bash
# A partir de src/Web/

# Executar todos os testes
npm test

# Executar testes de um projeto específico
npm run test:customer
npm run test:admin
npm run test:provider

# Executar em modo watch (desenvolvimento)
# Nota: Use npx vitest diretamente no diretório do projeto
cd MeAjudaAi.Web.Customer && npx vitest

# Executar com cobertura (global - consolida todos os projetos)
npm run test:coverage:global

# Executar apenas um arquivo específico
npx vitest run --config MeAjudaAi.Web.Customer/vitest.config.ts __tests__/lib/utils/phone.test.ts

# Executar testes que correspondem a um padrão
npx vitest run --config MeAjudaAi.Web.Customer/vitest.config.ts -t "formatPhone"
```

### Testes E2E (Playwright)

```bash
# Executar todos os testes E2E
npm run test:e2e

# Executar em modo UI (debug visual)
npm run test:e2e:ui

# Executar apenas um arquivo
npx playwright test MeAjudaAi.Web.Customer/e2e/auth.spec.ts

# Executar em modo headed (ver o browser)
npx playwright test --headed

# Ver relatório
npx playwright show-report
```

### Cobertura

```bash
# Gerar relatório de cobertura
npm run test:coverage

# Ver relatório HTML
open MeAjudaAi.Web.Customer/coverage/index.html
```

---

## ✅ Boas Práticas

### 1. Nomenclatura
- Arquivos de teste: `nome-do-componente.test.tsx`
- Describes: `describe('NomeDoComponente', ...)`
- Tests: `it('deve fazer algo específico', ...)`

### 2. Padrão AAA (Arrange, Act, Assert)
```typescript
it('deve incrementar contador', async () => {
  // Arrange
  const user = userEvent.setup();
  render(<Counter />);

  // Act
  await user.click(screen.getByRole('button'));

  // Assert
  expect(screen.getByText('1')).toBeInTheDocument();
});
```

### 3. Queries Prioritárias (Testing Library)
1. `getByRole` (preferencial — testa acessibilidade)
2. `getByLabelText`
3. `getByPlaceholderText`
4. `getByText`
5. `getByTestId` (último recurso)

### 4. Evitar Detalhes de Implementação
```typescript
// ❌ Ruim — testa implementação interna
expect(component.state.count).toBe(1);

// ✅ Bom — testa comportamento visível ao usuário
expect(screen.getByText('1')).toBeInTheDocument();
```

### 5. Testes Assíncronos
```typescript
// Use waitFor para operações assíncronas
await waitFor(() => {
  expect(screen.getByText('Dados carregados')).toBeInTheDocument();
});

// Use findBy* como atalho
const element = await screen.findByText('Dados carregados');
```

### 6. Mocks Limpos
```typescript
import { vi } from 'vitest';

// Mock de função
const mockFn = vi.fn();

// Mock de módulo
vi.mock('@/lib/api/client', () => ({
  fetchProviders: vi.fn(() => Promise.resolve([]))
}));
```

### 7. Cobertura Mínima por Camada
- **Utils/Schemas**: 90%+ (funções puras, fácil testar)
- **Hooks**: 80%+ (lógica de negócio encapsulada)
- **Estratégia**: Arquitetura de Testes Descentralizada (cada projeto gerencia seus próprios testes unitários e E2E).
- **Cobertura**: Threshold Global de 70% consolidado via script `merge-coverage.mjs`.
- **Unitários**: Vitest + React Testing Library + MSW.
- **E2E**: Playwright (specs localizadas na pasta `e2e/` de cada projeto).
- **CI/CD**: Geração automática de `api-spec.json` seguida de `generate:api` para garantir sincronia de tipos.

### 8. Testes de Acessibilidade (opcional)
```typescript
import { axe, toHaveNoViolations } from 'jest-axe';

expect.extend(toHaveNoViolations);

it('não deve ter violações de acessibilidade', async () => {
  const { container } = render(<Button>Click</Button>);
  const results = await axe(container);
  expect(results).toHaveNoViolations();
});
```

---

## 📊 Métricas de Qualidade

### Pirâmide de Testes

```
       /\
      /E2E\         10% - Testes E2E (fluxos críticos)
     /------\
    /Integration\   20% - Testes de Integração (componentes + API)
   /------------\
  /    Unit      \  70% - Testes Unitários
 /----------------\
```

### Thresholds Progressivos

| Fase | Lines | Functions | Branches | Statements |
|------|-------|-----------|----------|------------|
| Início | 50% | 50% | 50% | 50% |
| Meta intermediária | 65% | 65% | 65% | 65% |
| Meta final | 80% | 80% | 80% | 80% |

---

## 🔧 Troubleshooting

### Problema: Testes não encontram elementos
```typescript
render(<Component />);
screen.debug(); // Mostra HTML renderizado no console
```

### Problema: Testes assíncronos falhando
```typescript
// ❌ Falta await
const element = screen.findByText('Text');

// ✅ Correto
const element = await screen.findByText('Text');
```

### Problema: Mock não funciona
```typescript
// ✅ vi.mock é hoisted — sempre fica no topo do arquivo
vi.mock('@/lib/api/client');
import { Component } from '@/components/Component';
```

### Problema: Imports com @ não resolvem
Verificar que o `vitest.config.ts` do projeto tem os aliases corretos:
```typescript
resolve: {
  alias: {
    '@/': path.resolve(__dirname, './'),
  },
},
```

---

## 📋 Checklist de Implementação

### Fase 1: Infraestrutura Base 🔨

- [ ] Criar `libs/test-support/` com `setup.ts`, `test-utils.tsx`, `mock-data.ts`
- [ ] Instalar dependências: `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`, `msw`, `@vitest/coverage-v8`
- [ ] Criar `vitest.config.ts` no Customer
- [ ] Criar `vitest.config.ts` no Admin
- [ ] Criar `vitest.config.ts` no Provider
- [ ] Adicionar scripts de teste ao `package.json` raiz
- [ ] Validar que `npx vitest run` funciona em cada projeto

### Fase 2: Testes de Utils e Schemas 🧮

Funções puras — sem dependências de React ou API.

- [ ] `lib/utils/normalization.test.ts` (Customer + Provider)
- [ ] `lib/utils/phone.test.ts` (Customer + Provider)
- [ ] `lib/utils/cn.test.ts` (Customer + Provider)
- [ ] `lib/schemas/verification-status.test.ts` — migrar teste ad-hoc existente
- [ ] `lib/schemas/auth.test.ts` (Customer + Provider)
- [ ] `lib/api/response-utils.test.ts` (Customer + Provider)
- [ ] `lib/api/mappers.test.ts` (Customer + Provider)

### Fase 3: Testes de Hooks 🪝

Hooks com lógica de negócio — usar `renderHook` + MSW.

**Customer:**
- [ ] `hooks/use-via-cep.test.ts`
- [ ] `hooks/use-services.test.ts`
- [ ] `hooks/use-register-provider.test.ts`
- [ ] `hooks/use-provider-status.test.ts`
- [ ] `hooks/use-my-provider-profile.test.ts`
- [ ] `hooks/use-document-upload.test.ts`
- [ ] `hooks/use-update-provider-profile.test.ts`

**Admin:**
- [ ] `hooks/admin/use-providers.test.ts`
- [ ] `hooks/admin/use-categories.test.ts`
- [ ] `hooks/admin/use-dashboard.test.ts`
- [ ] `hooks/admin/use-services.test.ts`
- [ ] `hooks/admin/use-allowed-cities.test.ts`
- [ ] `hooks/admin/use-users.test.ts`

### Fase 4: Testes de Componentes UI 🎨

Componentes de `components/ui/` — cada projeto testa os seus.

**Customer:**
- [ ] `components/ui/button.test.tsx`
- [ ] `components/ui/input.test.tsx`
- [ ] `components/ui/card.test.tsx`
- [ ] `components/ui/select.test.tsx`
- [ ] `components/ui/dialog.test.tsx`
- [ ] `components/ui/badge.test.tsx`

**Admin:**
- [ ] `components/ui/button.test.tsx`
- [ ] `components/ui/card.test.tsx`
- [ ] `components/ui/input.test.tsx`
- [ ] `components/ui/dialog.test.tsx`
- [ ] `components/ui/select.test.tsx`
- [ ] `components/ui/theme-toggle.test.tsx`

**Provider:**
- [ ] `components/ui/button.test.tsx`
- [ ] `components/ui/card.test.tsx`
- [ ] `components/ui/input.test.tsx`
- [ ] `components/ui/file-upload.test.tsx`

### Fase 5: Testes de Componentes de Feature 🏗️

Componentes com lógica de negócio — os mais impactantes.

**Customer:**
- [ ] `components/auth/login-form.test.tsx`
- [ ] `components/auth/customer-register-form.test.tsx`
- [ ] `components/providers/provider-card.test.tsx`
- [ ] `components/providers/provider-grid.test.tsx`
- [ ] `components/search/search-filters.test.tsx`
- [ ] `components/search/city-search.test.tsx`
- [ ] `components/reviews/review-card.test.tsx`
- [ ] `components/reviews/review-form.test.tsx`
- [ ] `components/layout/header.test.tsx`

**Admin:**
- [ ] `components/layout/sidebar.test.tsx`

**Provider:**
- [ ] `components/dashboard/profile-status-card.test.tsx`
- [ ] `components/dashboard/verification-card.test.tsx`
- [ ] `components/profile/profile-header.test.tsx`
- [ ] `components/profile/profile-services.test.tsx`

### Fase 6: MSW + Testes de Integração 🔗

- [ ] Configurar MSW handlers por projeto (`__tests__/mocks/`)
- [ ] Testes de integração: componentes + API (loading → data → error)
- [ ] Testes de fluxos: login → redirect, cadastro → confirmação

### Fase 7: CI/CD e Cobertura 🚀

- [ ] Adicionar step de testes frontend no GitHub Actions
- [ ] Configurar reports (JUnit XML, coverage JSON)
- [ ] Estabelecer thresholds progressivos: 50% → 65% → 80%
- [ ] Adicionar badge de cobertura no README

---

## 📚 Recursos Adicionais

- [Vitest Documentation](https://vitest.dev/)
- [Testing Library — React](https://testing-library.com/docs/react-testing-library/intro/)
- [Playwright](https://playwright.dev/)
- [MSW — Mock Service Worker](https://mswjs.io/)
- [Kent C. Dodds — Testing Blog](https://kentcdodds.com/blog)

---

## 🤝 Alinhamento com Backend (.NET)

| Backend (.NET) | Frontend (React) |
|---|---|
| xUnit | Vitest |
| FluentAssertions | Jest-DOM matchers |
| Moq | MSW |
| Integration Tests | E2E Tests (Playwright) |
| Code Coverage (Coverlet) | Code Coverage (v8) |
| Testes separados em `tests/` | Testes dentro de cada projeto em `__tests__/` |

---

**Última atualização**: Março 2026
**Versão**: 3.0.0 (Testes dentro de cada projeto React)
