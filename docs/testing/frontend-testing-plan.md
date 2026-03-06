# Plano de Implementação de Testes - React 19 + TypeScript
## Projeto: MeAjudaAi.Web.Consumer (Monorepo .NET)

## Sumário

1. [Contexto do Projeto](#contexto-do-projeto)
2. [Decisao Arquitetural](#decisao-arquitetural)
3. [Bibliotecas e Dependencias](#bibliotecas-e-dependencias)
4. [Estrutura de Pastas](#estrutura-de-pastas)
5. [Configuracao](#configuracao)
6. [Estrutura dos Arquivos de Teste](#estrutura-dos-arquivos-de-teste)
7. [Integração com Pipeline CI/CD](#integração-com-pipeline-cicd)
8. [Pipeline CI/CD Robusta (Ref. Medium)](#pipeline-cicd-robusta-ref-medium)
9. [Comandos Úteis](#comandos-úteis)
10. [Boas Práticas](#boas-práticas)

---

## Contexto do Projeto

O projeto está integrado em um **monorepo .NET** com arquitetura de **monolito modular**. A estrutura atual já possui:

- **Backend .NET** com testes completos organizados por camada:
  - `MeAjudaAi.ApiService.Tests`
  - `MeAjudaAi.Architecture.Tests`
  - `MeAjudaAi.E2E.Tests`
  - `MeAjudaAi.Integration.Tests`
  - `MeAjudaAi.Shared.Tests`
  - `MeAjudaAi.Web.Admin.Tests` (Blazor WASM com bUnit)

- **Frontend React** localizado em:
  - `src/Web/MeAjudaAi.Web.Consumer`

---

## Decisao Arquitetural

### ✅ Recomendação: Criar Projeto Separado de Testes

**Criar:** `tests/MeAjudaAi.Web.Consumer.Tests`

### Justificativa

1. **Consistência com a arquitetura existente**: Todos os projetos de teste já estão separados na pasta `tests/`
2. **Separação de responsabilidades**: Backend (.NET) e Frontend (React) mantêm seus testes isolados
3. **Pipeline CI/CD independente**: Permite executar testes frontend/backend separadamente
4. **Mesma abordagem do Web.Admin**: O portal admin Blazor já segue este padrão com `MeAjudaAi.Web.Admin.Tests`
5. **Facilita manutenção**: Dependências JavaScript não poluem projetos .NET
6. **Clareza organizacional**: Fica explícito que são testes de frontend

### Estrutura Completa do Monorepo

```text
MeAjudaAi/
├── src/
│   ├── Web/
│   │   ├── MeAjudaAi.Web.Consumer/         # ← Projeto React
│   │   │   ├── src/
│   │   │   ├── public/
│   │   │   ├── package.json
│   │   │   ├── vite.config.ts
│   │   │   └── tsconfig.json
│   │   └── MeAjudaAi.Web.Admin/            # Blazor WASM
│   ├── ApiService/
│   ├── Architecture/
│   └── Shared/
├── tests/
│   ├── MeAjudaAi.ApiService.Tests/         # .NET
│   ├── MeAjudaAi.Architecture.Tests/       # .NET
│   ├── MeAjudaAi.E2E.Tests/                # .NET
│   ├── MeAjudaAi.Integration.Tests/        # .NET
│   ├── MeAjudaAi.Shared.Tests/             # .NET
│   ├── MeAjudaAi.Web.Admin.Tests/          # bUnit (Blazor)
│   └── MeAjudaAi.Web.Consumer.Tests/       # ← NOVO: Vitest/React Testing Library
│       ├── src/
│       │   ├── components/
│       │   ├── hooks/
│       │   ├── pages/
│       │   ├── utils/
│       │   └── __tests__/
│       ├── e2e/
│       ├── package.json
│       ├── vitest.config.ts
│       ├── playwright.config.ts
│       └── tsconfig.json
├── .editorconfig
├── parallel.runsettings
├── sequential.runsettings
├── xunit.runner.json
└── MeAjudaAi.sln
```

---

## Bibliotecas e Dependencias

### Instalação Básica

```bash
# Testing framework e runners
npm install --save-dev vitest @vitest/ui jsdom

# React Testing Library
npm install --save-dev @testing-library/react @testing-library/jest-dom @testing-library/user-event

# Suporte a TypeScript para testes
npm install --save-dev @testing-library/dom@^10.x

# Cobertura de código
npm install --save-dev @vitest/coverage-v8

# Mock Service Worker (para mock de APIs)
npm install --save-dev msw

# Testes E2E (opcional mas recomendado)
npm install --save-dev @playwright/test
```

### Pacotes Adicionais (Opcionais)

```bash
# Para testes de acessibilidade
npm install --save-dev jest-axe

# Para snapshots visuais
npm install --save-dev @storybook/test-runner

# Nota: O pacote @testing-library/react-hooks está depreciado.
# Para testar hooks, use renderHook diretamente de '@testing-library/react' (v13.1+).
```

---

## Estrutura de Pastas

### Estrutura do Projeto de Testes: `tests/MeAjudaAi.Web.Consumer.Tests/`

```text
MeAjudaAi.Web.Consumer.Tests/
├── src/
│   ├── components/
│   │   ├── Button/
│   │   │   ├── Button.test.tsx
│   │   │   └── Button.integration.test.tsx
│   │   ├── Input/
│   │   │   ├── Input.test.tsx
│   │   │   └── Input.accessibility.test.tsx
│   │   ├── Card/
│   │   │   └── Card.test.tsx
│   │   └── Layout/
│   │       ├── Header.test.tsx
│   │       └── Sidebar.test.tsx
│   ├── hooks/
│   │   ├── useAuth.test.ts
│   │   ├── useLocalStorage.test.ts
│   │   └── useDebounce.test.ts
│   ├── pages/
│   │   ├── Home/
│   │   │   └── Home.test.tsx
│   │   ├── Dashboard/
│   │   │   └── Dashboard.test.tsx
│   │   └── Profile/
│   │       └── Profile.test.tsx
│   ├── services/
│   │   ├── api.test.ts
│   │   ├── auth.service.test.ts
│   │   └── storage.service.test.ts
│   ├── utils/
│   │   ├── formatters.test.ts
│   │   ├── validators.test.ts
│   │   └── helpers.test.ts
│   └── __tests__/
│       ├── setup.ts
│       ├── helpers/
│       │   ├── test-utils.tsx
│       │   ├── mock-data.ts
│       │   ├── custom-matchers.ts
│       │   └── test-providers.tsx
│       └── mocks/
│           ├── handlers.ts
│           ├── server.ts
│           └── browser.ts
├── e2e/
│   ├── tests/
│   │   ├── auth/
│   │   │   ├── login.spec.ts
│   │   │   ├── register.spec.ts
│   │   │   └── logout.spec.ts
│   │   ├── navigation/
│   │   │   ├── menu.spec.ts
│   │   │   └── routes.spec.ts
│   │   └── user-flows/
│   │       ├── complete-profile.spec.ts
│   │       └── request-help.spec.ts
│   ├── fixtures/
│   │   ├── users.ts
│   │   └── requests.ts
│   └── utils/
│       └── helpers.ts
├── coverage/
│   └── (gerado automaticamente)
├── node_modules/
├── package.json
├── package-lock.json
├── vitest.config.ts
├── playwright.config.ts
├── tsconfig.json
├── .gitignore
└── README.md
```

### Mapeamento para o Código Fonte

Os testes espelham a estrutura do projeto principal:

```text
src/Web/MeAjudaAi.Web.Consumer/src/     →  tests/MeAjudaAi.Web.Consumer.Tests/src/
    components/Button/Button.tsx        →      components/Button/Button.test.tsx
    hooks/useAuth.ts                    →      hooks/useAuth.test.ts
    pages/Home/Home.tsx                 →      pages/Home/Home.test.tsx
    utils/formatters.ts                 →      utils/formatters.test.ts
```

### Estrutura de Nomenclatura

- **Testes unitários**: `*.test.tsx` ou `*.test.ts`
- **Testes de integração**: `*.integration.test.tsx`
- **Testes de acessibilidade**: `*.accessibility.test.tsx`
- **Testes E2E**: `*.spec.ts` (dentro de `e2e/tests/`)

### Vantagens desta Estrutura

✅ **Separação clara**: Testes não poluem o código de produção  
✅ **Consistência**: Segue o padrão do monorepo (.NET tests separados)  
✅ **Build otimizado**: Fácil excluir testes do bundle de produção  
✅ **CI/CD independente**: Pode rodar testes frontend/backend separadamente  
✅ **Organização**: Mesma abordagem do `MeAjudaAi.Web.Admin.Tests`

---

## Configuracao

### 1. `vitest.config.ts`

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/__tests__/setup.ts',
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      exclude: [
        'node_modules/',
        'src/__tests__/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockData',
        'src/main.tsx',
      ],
      thresholds: {
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@src': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src'),
      '@components': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/components'),
      '@hooks': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/hooks'),
      '@utils': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/utils'),
      '@pages': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/pages'),
    },
  },
});
```

### 2. `src/__tests__/setup.ts`

```typescript
import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach, beforeAll, afterAll } from 'vitest';
import { server } from './mocks/server';

// Estabelece requisições de API mockadas
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));

// Reseta handlers entre testes
afterEach(() => {
  cleanup();
  server.resetHandlers();
});

// Limpa após todos os testes
afterAll(() => server.close());

// Mock do matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {}, // deprecated
    removeListener: () => {}, // deprecated
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => {},
  }),
});

// Mock do IntersectionObserver
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return [];
  }
  unobserve() {}
} as any;
```

### 3. `src/__tests__/helpers/test-utils.tsx`

```typescript
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';

// Provider customizado se você tiver (Context, Theme, etc)
interface AllTheProvidersProps {
  children: React.ReactNode;
}

const AllTheProviders = ({ children }: AllTheProvidersProps) => {
  return (
    <BrowserRouter>
      {/* Adicione seus providers aqui */}
      {/* <ThemeProvider> */}
      {/* <AuthProvider> */}
      {children}
      {/* </AuthProvider> */}
      {/* </ThemeProvider> */}
    </BrowserRouter>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllTheProviders, ...options });

// Re-exporta tudo
export * from '@testing-library/react';
export { customRender as render };
```

### 4. `src/__tests__/mocks/handlers.ts`

```typescript
import { http, HttpResponse } from 'msw';

export const handlers = [
  // GET exemplo
  http.get('/api/users', () => {
    return HttpResponse.json([
      { id: 1, name: 'John Doe', email: 'john@example.com' },
      { id: 2, name: 'Jane Smith', email: 'jane@example.com' },
    ]);
  }),

  // POST exemplo
  http.post('/api/login', async ({ request }) => {
    const { email, password } = await request.json();
    
    if (email === 'test@test.com' && password === 'password123') {
      return HttpResponse.json({
        token: 'fake-jwt-token',
        user: { id: 1, email, name: 'Test User' },
      });
    }
    
    return HttpResponse.json(
      { message: 'Invalid credentials' },
      { status: 401 }
    );
  }),

  // Erro exemplo
  http.get('/api/error', () => {
    return HttpResponse.json(
      { message: 'Internal Server Error' },
      { status: 500 }
    );
  }),
];
```

### 5. `src/__tests__/mocks/server.ts`

```typescript
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);
```

### 6. `playwright.config.ts`

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e/tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
});
```

### 7. `package.json` - Scripts

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:run": "vitest run",
    "test:coverage": "vitest run --coverage",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:report": "playwright show-report"
  }
}
```

---

## Estrutura dos Arquivos de Teste

### Teste de Componente com Base UI React

**`src/components/Button/Button.tsx`**

```typescript
import { Button as BaseButton } from '@base-ui/react/Button';
import { tv, type VariantProps } from 'tailwind-variants';
import { twMerge } from 'tailwind-merge';

const button = tv({
  base: 'inline-flex items-center justify-center rounded-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 disabled:pointer-events-none disabled:opacity-50',
  variants: {
    variant: {
      primary: 'bg-blue-600 text-white hover:bg-blue-700',
      secondary: 'bg-gray-200 text-gray-900 hover:bg-gray-300',
      destructive: 'bg-red-600 text-white hover:bg-red-700',
      ghost: 'hover:bg-gray-100',
    },
    size: {
      sm: 'h-9 px-3 text-sm',
      md: 'h-10 px-4',
      lg: 'h-11 px-8 text-lg',
    },
  },
  defaultVariants: {
    variant: 'primary',
    size: 'md',
  },
});

export type ButtonProps = VariantProps<typeof button> & {
  children: React.ReactNode;
  className?: string;
  disabled?: boolean;
  type?: 'button' | 'submit' | 'reset';
  onClick?: () => void;
};

export function Button({
  children,
  variant,
  size,
  className,
  ...props
}: ButtonProps) {
  return (
    <BaseButton
      className={twMerge(button({ variant, size }), className)}
      {...props}
    >
      {children}
    </BaseButton>
  );
}
```

**`src/components/Button/Button.test.tsx`**

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@/__tests__/helpers/test-utils';
import userEvent from '@testing-library/user-event';
import { Button } from '@src/components/Button/Button';

describe('Button Component', () => {
  it('deve renderizar corretamente', () => {
    render(<Button>Clique aqui</Button>);
    
    expect(screen.getByRole('button', { name: /clique aqui/i })).toBeInTheDocument();
  });

  it('deve aplicar a variante primary por padrão', () => {
    render(<Button>Primary</Button>);
    
    const button = screen.getByRole('button');
    expect(button).toHaveClass('bg-blue-600');
  });

  it('deve aplicar diferentes variantes', () => {
    const { rerender } = render(<Button variant="secondary">Secondary</Button>);
    expect(screen.getByRole('button')).toHaveClass('bg-gray-200');

    rerender(<Button variant="destructive">Destructive</Button>);
    expect(screen.getByRole('button')).toHaveClass('bg-red-600');

    rerender(<Button variant="ghost">Ghost</Button>);
    expect(screen.getByRole('button')).toHaveClass('hover:bg-gray-100');
  });

  it('deve aplicar diferentes tamanhos', () => {
    const { rerender } = render(<Button size="sm">Small</Button>);
    expect(screen.getByRole('button')).toHaveClass('h-9');

    rerender(<Button size="md">Medium</Button>);
    expect(screen.getByRole('button')).toHaveClass('h-10');

    rerender(<Button size="lg">Large</Button>);
    expect(screen.getByRole('button')).toHaveClass('h-11');
  });

  it('deve aceitar className customizada', () => {
    render(<Button className="custom-class">Custom</Button>);
    
    expect(screen.getByRole('button')).toHaveClass('custom-class');
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Button disabled>Disabled</Button>);
    
    const button = screen.getByRole('button');
    expect(button).toBeDisabled();
    expect(button).toHaveClass('opacity-50');
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

  it('deve renderizar diferentes tipos de button', () => {
    const { rerender } = render(<Button type="submit">Submit</Button>);
    expect(screen.getByRole('button')).toHaveAttribute('type', 'submit');

    rerender(<Button type="reset">Reset</Button>);
    expect(screen.getByRole('button')).toHaveAttribute('type', 'reset');
  });
});
```

### Teste de Hook Customizado

**`src/hooks/useLocalStorage.ts`**

```typescript
import { useState, useEffect } from 'react';

export function useLocalStorage<T>(key: string, initialValue: T) {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(error);
      return initialValue;
    }
  });

  const setValue = (value: T | ((val: T) => T)) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value;
      setStoredValue(valueToStore);
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      console.error(error);
    }
  };

  return [storedValue, setValue] as const;
}
```

**`src/hooks/useLocalStorage.test.ts`**

```typescript
import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useLocalStorage } from '@src/hooks/useLocalStorage';

describe('useLocalStorage Hook', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('deve retornar o valor inicial quando não há valor armazenado', () => {
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    
    expect(result.current[0]).toBe('initial');
  });

  it('deve retornar o valor armazenado do localStorage', () => {
    localStorage.setItem('test-key', JSON.stringify('stored-value'));
    
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    
    expect(result.current[0]).toBe('stored-value');
  });

  it('deve atualizar o localStorage quando setValue é chamado', () => {
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    
    act(() => {
      result.current[1]('new-value');
    });
    
    expect(result.current[0]).toBe('new-value');
    expect(localStorage.getItem('test-key')).toBe(JSON.stringify('new-value'));
  });

  it('deve funcionar com objetos', () => {
    const initialObject = { name: 'John', age: 30 };
    const { result } = renderHook(() => useLocalStorage('user', initialObject));
    
    const newObject = { name: 'Jane', age: 25 };
    
    act(() => {
      result.current[1](newObject);
    });
    
    expect(result.current[0]).toEqual(newObject);
  });

  it('deve funcionar com função updater', () => {
    const { result } = renderHook(() => useLocalStorage('counter', 0));
    
    act(() => {
      result.current[1](prev => prev + 1);
    });
    
    expect(result.current[0]).toBe(1);
    
    act(() => {
      result.current[1](prev => prev + 5);
    });
    
    expect(result.current[0]).toBe(6);
  });
});
```

### Teste de Página com API

**`src/pages/Users/Users.tsx`**

```typescript
import { useState, useEffect } from 'react';
import { Button } from '@src/components/Button/Button';

interface User {
  id: number;
  name: string;
  email: string;
}

export function Users() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch('/api/users')
      .then(res => res.json())
      .then(data => {
        setUsers(data);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Carregando...</div>;
  if (error) return <div>Erro: {error}</div>;

  return (
    <div>
      <h1>Usuários</h1>
      <ul>
        {users.map(user => (
          <li key={user.id}>
            {user.name} - {user.email}
          </li>
        ))}
      </ul>
      <Button onClick={() => console.log('Refresh')}>
        Atualizar
      </Button>
    </div>
  );
}
```

**`src/pages/Users/Users.test.tsx`**

```typescript
import { describe, it, expect } from 'vitest';
import { render, screen, waitFor } from '@/__tests__/helpers/test-utils';
import { Users } from '@src/pages/Users/Users';

describe('Users Page', () => {
  it('deve mostrar loading inicialmente', () => {
    render(<Users />);
    
    expect(screen.getByText(/carregando/i)).toBeInTheDocument();
  });

  it('deve renderizar lista de usuários após carregamento', async () => {
    render(<Users />);
    
    await waitFor(() => {
      expect(screen.queryByText(/carregando/i)).not.toBeInTheDocument();
    });

    expect(screen.getByText(/john doe/i)).toBeInTheDocument();
    expect(screen.getByText(/jane smith/i)).toBeInTheDocument();
  });

  it('deve renderizar botão de atualizar', async () => {
    render(<Users />);
    
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /atualizar/i })).toBeInTheDocument();
    });
  });
});
```

### Teste de Utilidade

**`src/utils/formatters.ts`**

```typescript
export function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value);
}

export function formatDate(date: Date): string {
  return new Intl.DateTimeFormat('pt-BR').format(date);
}

export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength) + '...';
}
```

**`src/utils/formatters.test.ts`**

```typescript
import { describe, it, expect } from 'vitest';
import { formatCurrency, formatDate, truncateText } from '@src/utils/formatters';

describe('formatCurrency', () => {
  it('deve formatar número como moeda brasileira', () => {
    expect(formatCurrency(1000)).toBe('R$ 1.000,00');
    expect(formatCurrency(50.5)).toBe('R$ 50,50');
  });
});

describe('formatDate', () => {
  it('deve formatar data no padrão brasileiro', () => {
    const date = new Date('2024-01-15');
    expect(formatDate(date)).toBe('15/01/2024');
  });
});

describe('truncateText', () => {
  it('deve retornar texto original se menor que maxLength', () => {
    expect(truncateText('Hello', 10)).toBe('Hello');
  });

  it('deve truncar texto e adicionar reticências', () => {
    expect(truncateText('Hello World', 5)).toBe('Hello...');
  });

  it('deve funcionar com maxLength exato', () => {
    expect(truncateText('Hello', 5)).toBe('Hello');
  });
});
```

### Teste E2E com Playwright

**`e2e/tests/user-flow.spec.ts`**

```typescript
import { test, expect } from '@playwright/test';

test.describe('Fluxo de Usuário', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('deve navegar para página de usuários', async ({ page }) => {
    await page.click('text=Usuários');
    
    await expect(page).toHaveURL(/.*users/);
    await expect(page.locator('h1')).toContainText('Usuários');
  });

  test('deve mostrar lista de usuários', async ({ page }) => {
    await page.goto('/users');
    
    await expect(page.locator('li')).toHaveCount(2);
    await expect(page.locator('text=John Doe')).toBeVisible();
  });

  test('deve clicar no botão de atualizar', async ({ page }) => {
    await page.goto('/users');
    
    const button = page.getByRole('button', { name: /atualizar/i });
    await button.click();
    
    // Verificar algum comportamento esperado
  });
});
```

---

---

## Integração com Pipeline CI/CD

### Integração com o Monorepo .NET

O projeto está em um monorepo .NET e usa GitHub Actions para CI/CD. A integração dos testes JavaScript no pipeline existente garante que todos os testes (backend e frontend) sejam executados automaticamente.

### 1. Adicionar Scripts ao `package.json`

**`tests/MeAjudaAi.Web.Consumer.Tests/package.json`**

```json
{
  "name": "meajudaai.web.consumer.tests",
  "version": "1.0.0",
  "private": true,
  "type": "module",
  "scripts": {
    "test": "vitest",
    "test:run": "vitest run",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest run --coverage",
    "test:ci": "vitest run --coverage --reporter=junit",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:ci": "playwright test --reporter=html,junit",
    "test:e2e:report": "playwright show-report"
  },
  "dependencies": {},
  "devDependencies": {
    "@testing-library/dom": "^10.4.0",
    "@testing-library/jest-dom": "^6.1.5",
    "@testing-library/react": "^16.2.0",
    "@testing-library/user-event": "^14.5.1",
    "@vitejs/plugin-react": "^4.3.4",
    "@vitest/coverage-v8": "^3.0.0",
    "@vitest/ui": "^3.0.0",
    "@playwright/test": "^1.50.0",
    "jsdom": "^26.0.0",
    "msw": "^2.0.11",
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "vitest": "^3.0.0"
  }
}
```

### 2. Configuração do GitHub Actions

**`.github/workflows/tests.yml`**

```yaml
name: Tests

on:
  push:
    branches: [master, main, develop]
    paths:
      - 'src/Web/MeAjudaAi.Web.Consumer/**'
      - 'tests/MeAjudaAi.Web.Consumer.Tests/**'
  pull_request:
    branches: [master, main, develop]

jobs:
  backend-tests:
    name: Backend Tests (.NET)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run tests
        run: dotnet test --no-restore --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          files: '**/coverage.cobertura.xml'
          flags: backend
          token: ${{ secrets.CODECOV_TOKEN }}

  frontend-unit-tests:
    name: Frontend Unit Tests (React)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: tests/MeAjudaAi.Web.Consumer.Tests/package-lock.json
      
      - name: Install dependencies
        working-directory: tests/MeAjudaAi.Web.Consumer.Tests
        run: npm ci
      
      - name: Run tests
        working-directory: tests/MeAjudaAi.Web.Consumer.Tests
        run: npm run test:ci
      
      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          files: tests/MeAjudaAi.Web.Consumer.Tests/coverage/coverage-final.json
          flags: frontend
          token: ${{ secrets.CODECOV_TOKEN }}

  frontend-e2e-tests:
    name: Frontend E2E Tests (Playwright)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: tests/MeAjudaAi.Web.Consumer.Tests/package-lock.json
      
      - name: Install dependencies
        working-directory: tests/MeAjudaAi.Web.Consumer.Tests
        run: npm ci
      
      - name: Install Playwright browsers
        working-directory: tests/MeAjudaAi.Web.Consumer.Tests
        run: npx playwright install --with-deps
      
      - name: Run E2E tests
        working-directory: tests/MeAjudaAi.Web.Consumer.Tests
        run: npm run test:e2e:ci
      
      - name: Upload Playwright report
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: playwright-report
          path: tests/MeAjudaAi.Web.Consumer.Tests/playwright-report/
          retention-days: 30
```

---

## Pipeline CI/CD Robusta (Ref. Medium)

Baseado no guia "[Building a Robust CI/CD Pipeline for React Apps](https://medium.com/@lamjed.gaidi070/building-a-robust-ci-cd-pipeline-for-react-apps-with-testing-and-static-analysis-05e14735f8f0)", nossa estratégia inclui as seguintes considerações:

### 1. Análise Estática com SonarQube
Além do ESLint, o projeto deve integrar o **SonarScanner** no pipeline para:
- Monitorar a saúde do código a longo prazo.
- Definir **Quality Gates** (ex: falhar build se cobertura cair de 80%).
- Detectar vulnerabilidades de segurança (Security Hotspots).

### 2. Fluxo Completo do Pipeline
Diferente de um pipeline simples de build, o fluxo robusto implementado seguirá:
1. **Lint & Static Analysis**: ESLint + Prettier + SonarQube Scan.
2. **Unit & Integration Tests**: Execução com Vitest (com geração de relatório LCOV para o Sonar).
3. **Build & Package**: Geração da build de produção do Vite para MeAjudaAi.Web.Consumer.
4. **Containerization (Contexto Aspire)**: O `dotnet aspire` facilita a geração de imagens Docker que serão enviadas para o Registry (Azure Container Registry).
5. **E2E Testing**: Execução do Playwright contra o container de staging.
6. **Deployment**: Via `azd deploy` para Azure Container Apps.

### 3. Comparativo de Ferramentas

| Ferramenta Artigo | Nossa Escolha | Justificativa |
|-------------------|---------------|---------------|
| Jest | **Vitest** | Nativo para Vite, performance significativamente superior. |
| Cypress / Selenium | **Playwright** | Melhor suporte a múltiplos browsers, mais rápido e resiliente. |
| SonarQube | **SonarQube** | Mantido como padrão para métricas de qualidade. |
| Docker / K8s | **Docker / Aspire** | Usamos Docker via Aspire, que abstrai a complexidade do K8s facilitando o deploy. |


---

### 3. Configuração de Reports para CI/CD

**`tests/MeAjudaAi.Web.Consumer.Tests/vitest.config.ts`**

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/__tests__/setup.ts',
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov', 'cobertura', 'json-summary'],
      reportsDirectory: './coverage',
      exclude: [
        'node_modules/',
        'src/__tests__/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockData',
      ],
      thresholds: {
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
    },
    reporters: ['default', 'junit', 'json'],
    outputFile: {
      junit: './test-results/junit.xml',
      json: './test-results/results.json',
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@src': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src'),
    },
  },
});
```

### 4. Executar Testes Localmente

```bash
# Navegar para o projeto de testes
cd tests/MeAjudaAi.Web.Consumer.Tests

# Instalar dependências (primeira vez)
npm install

# Executar testes unitários
npm test

# Executar testes com cobertura
npm run test:coverage

# Executar testes E2E
npm run test:e2e

# Executar todos os testes (CI mode)
npm run test:ci && npm run test:e2e:ci
```

### 5. Scripts Helper no Root do Monorepo

Criar script para facilitar execução de testes:

**`scripts/run-frontend-tests.sh`**

```bash
#!/bin/bash

echo "🧪 Executando testes do Frontend (React)..."

cd tests/MeAjudaAi.Web.Consumer.Tests

# Verificar se node_modules existe
if [ ! -d "node_modules" ]; then
    echo "📦 Instalando dependências..."
    npm install
fi

# Executar testes unitários
echo "🔬 Executando testes unitários..."
npm run test:run

# Executar testes E2E
echo "🎭 Executando testes E2E..."
npm run test:e2e

echo "✅ Testes concluídos!"
```

**`scripts/run-all-tests.sh`**

```bash
#!/bin/bash

echo "🧪 Executando TODOS os testes do monorepo..."

# Backend tests
echo "🔵 Executando testes Backend (.NET)..."
dotnet test

# Frontend tests
echo "🟢 Executando testes Frontend (React)..."
./scripts/run-frontend-tests.sh

echo "✅ Todos os testes concluídos!"
```

---

## Comandos Úteis

### Navegação e Setup Inicial

```bash
# Navegar para o projeto de testes
cd tests/MeAjudaAi.Web.Consumer.Tests

# Instalar dependências (primeira vez ou após pull)
npm install

# Instalar browsers do Playwright
npx playwright install
```

### Testes Unitários (Vitest)

```bash
# Executar todos os testes em modo watch
npm test

# Executar testes uma única vez
npm run test:run

# Executar testes com interface visual
npm run test:ui

# Executar apenas um arquivo específico
npm test -- src/components/Button/Button.test.tsx

# Executar testes que correspondem a um padrão
npm test -- Button

# Executar testes em modo CI (sem watch)
npm run test:ci
```

### Cobertura de Código

```bash
# Gerar relatório de cobertura
npm run test:coverage

# Ver relatório HTML no navegador
open coverage/index.html

# Cobertura com threshold definido (falha se < 80%)
npm run test:coverage -- --coverage.thresholds.lines=80
```

### Testes E2E (Playwright)

```bash
# Executar todos os testes E2E
npm run test:e2e

# Executar em modo UI (debug visual)
npm run test:e2e:ui

# Executar apenas um arquivo
npm run test:e2e -- e2e/tests/auth/login.spec.ts

# Executar em modo headed (ver o browser)
npm run test:e2e -- --headed

# Executar em modo debug
npm run test:e2e -- --debug

# Executar em browser específico
npm run test:e2e -- --project=chromium
npm run test:e2e -- --project=firefox

# Ver relatório após execução
npm run test:e2e:report
```

### Executar do Root do Monorepo

```bash
# A partir da raiz do projeto MeAjudaAi/

# Executar apenas testes frontend
./scripts/run-frontend-tests.sh

# Executar todos os testes (Backend + Frontend)
./scripts/run-all-tests.sh

# Executar testes Backend (.NET)
dotnet test

# Executar testes Frontend específicos
cd tests/MeAjudaAi.Web.Consumer.Tests && npm test
```

### Debugging e Troubleshooting

```bash
# Ver output detalhado
npm test -- --reporter=verbose

# Executar com logs de debug
DEBUG=* npm test

# Limpar cache e node_modules
rm -rf node_modules coverage .vitest
npm install

# Atualizar snapshots
npm test -- -u

# Executar apenas testes modificados
npm test -- --changed
```

### Integração com IDE (VS Code)

Instale as extensões recomendadas:

```json
// .vscode/extensions.json
{
  "recommendations": [
    "vitest.explorer",
    "ms-playwright.playwright",
    "firsttris.vscode-jest-runner"
  ]
}
```

Configuração de tasks:

```json
// .vscode/tasks.json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Run Frontend Tests",
      "type": "shell",
      "command": "cd tests/MeAjudaAi.Web.Consumer.Tests && npm test",
      "group": "test",
      "presentation": {
        "reveal": "always",
        "panel": "new"
      }
    },
    {
      "label": "Run Frontend Tests with Coverage",
      "type": "shell",
      "command": "cd tests/MeAjudaAi.Web.Consumer.Tests && npm run test:coverage",
      "group": "test"
    },
    {
      "label": "Run E2E Tests",
      "type": "shell",
      "command": "cd tests/MeAjudaAi.Web.Consumer.Tests && npm run test:e2e",
      "group": "test"
    }
  ]
}
```

---

## Boas Práticas

### 1. **Nomenclatura**
- Arquivos de teste: `ComponentName.test.tsx`
- Describes: `describe('ComponentName', ...)`
- Tests: `it('deve fazer algo específico', ...)`

### 2. **Organização dos Testes**
```typescript
describe('ComponentName', () => {
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
  });

  // Testes de acessibilidade
  describe('Accessibility', () => {
    it('deve ter roles corretos', () => {});
  });
});
```

### 3. **AAA Pattern (Arrange, Act, Assert)**
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

### 4. **Queries Prioritárias**
1. `getByRole` (preferencial)
2. `getByLabelText`
3. `getByPlaceholderText`
4. `getByText`
5. `getByTestId` (último recurso)

### 5. **Evitar Detalhes de Implementação**
```typescript
// ❌ Ruim - testa implementação
expect(component.state.count).toBe(1);

// ✅ Bom - testa comportamento
expect(screen.getByText('1')).toBeInTheDocument();
```

### 6. **Testes Assíncronos**
```typescript
// Use waitFor para operações assíncronas
await waitFor(() => {
  expect(screen.getByText('Dados carregados')).toBeInTheDocument();
});

// Use findBy* quando esperar elemento aparecer
const element = await screen.findByText('Dados carregados');
```

### 7. **Mocks Limpos**
```typescript
import { vi } from 'vitest';

// Mock de função
const mockFn = vi.fn();

// Mock de módulo
vi.mock('./api', () => ({
  fetchUsers: vi.fn(() => Promise.resolve([]))
}));
```

### 8. **Cobertura de Teste**
- **Componentes**: 80%+ de cobertura
- **Hooks**: 90%+ de cobertura
- **Utils**: 95%+ de cobertura
- **Integração**: Fluxos principais
- **E2E**: Jornadas críticas do usuário

### 9. **Testes de Acessibilidade**
```typescript
import { axe, toHaveNoViolations } from 'jest-axe';

expect.extend(toHaveNoViolations);

it('não deve ter violações de acessibilidade', async () => {
  const { container } = render(<Button>Click</Button>);
  const results = await axe(container);
  
  expect(results).toHaveNoViolations();
});
```

### 10. **Snapshot Testing (com moderação)**
```typescript
it('deve corresponder ao snapshot', () => {
  const { container } = render(<Button>Test</Button>);
  expect(container.firstChild).toMatchSnapshot();
});
```

---

## 📊 Métricas de Qualidade

### Cobertura Mínima Recomendada
- **Statements**: 80%
- **Branches**: 80%
- **Functions**: 80%
- **Lines**: 80%

### Pirâmide de Testes
```text
       /\
      /E2E\         10% - Testes E2E (fluxos críticos)
     /------\
    /Integration\   20% - Testes de Integração
   /------------\
  /    Unit      \  70% - Testes Unitários
 /----------------\
```

---

## 🔧 Troubleshooting

### Problema: Testes não encontram elementos
**Solução**: Use `screen.debug()` para ver a estrutura DOM

```typescript
render(<Component />);
screen.debug(); // Mostra HTML renderizado
```

### Problema: Testes assíncronos falhando
**Solução**: Sempre use `await` com queries assíncronas

```typescript
// ❌ Errado
const element = screen.findByText('Text');

// ✅ Correto
const element = await screen.findByText('Text');
```

### Problema: Mock não está sendo aplicado
**Solução**: Verifique se o mock está antes do import do componente

```typescript
// ✅ Correto
vi.mock('./api');
import { Component } from './Component';
```

---

## 📚 Recursos Adicionais

- [Vitest Documentation](https://vitest.dev/)
- [Testing Library](https://testing-library.com/react)
- [Playwright](https://playwright.dev/)
- [MSW](https://mswjs.io/)
- [Kent C. Dodds - Testing Blog](https://kentcdodds.com/blog)

---

**Última atualização**: Fevereiro 2026  
**Versão**: 2.0.0 (Adaptado para Monorepo .NET)

---

## Checklist de Implementação

### Fase 1: Setup Inicial ✅

- [ ] Criar pasta `tests/MeAjudaAi.Web.Consumer.Tests/`
- [ ] Criar `package.json` com dependências
- [ ] Instalar todas as bibliotecas necessárias
- [ ] Configurar `vitest.config.ts`
- [ ] Configurar `playwright.config.ts`
- [ ] Criar estrutura de pastas (`src/`, `e2e/`, etc.)
- [ ] Configurar `tsconfig.json`

### Fase 2: Configuração de Testes ✅

- [ ] Criar `src/__tests__/setup.ts`
- [ ] Criar `src/__tests__/helpers/test-utils.tsx`
- [ ] Configurar MSW (`handlers.ts`, `server.ts`)
- [ ] Adicionar mocks de APIs necessárias
- [ ] Configurar aliases de imports

### Fase 3: Primeiros Testes 🎯

- [ ] Escrever teste para componente Button
- [ ] Escrever teste para hook useLocalStorage
- [ ] Escrever teste para uma página simples
- [ ] Escrever teste para utility function
- [ ] Validar cobertura mínima (80%)

### Fase 4: Testes E2E 🎭

- [ ] Configurar Playwright com app local
- [ ] Criar primeiro teste E2E de login
- [ ] Criar teste de navegação
- [ ] Criar teste de fluxo completo
- [ ] Configurar screenshots e vídeos

### Fase 5: Integração CI/CD 🔄

- [ ] Adicionar scripts no `package.json`
- [ ] Configurar Azure DevOps pipeline ou GitHub Actions
- [ ] Configurar reports de cobertura
- [ ] Criar scripts helper no root do monorepo
- [ ] Testar pipeline completo

### Fase 6: Documentação e Padrões 📚

- [ ] Documentar padrões de teste no README
- [ ] Criar templates de testes
- [ ] Configurar pre-commit hooks (Husky)
- [ ] Treinar equipe nos padrões
- [ ] Estabelecer code review guidelines

---

## Próximos Passos Recomendados

### 1. Criar o Projeto de Testes

```bash
# Na raiz do monorepo
mkdir -p tests/MeAjudaAi.Web.Consumer.Tests
cd tests/MeAjudaAi.Web.Consumer.Tests

# Inicializar projeto Node
npm init -y

# Instalar dependências conforme documentado
# (veja seção "Bibliotecas e Dependências")
```

### 2. Configurar Alias de Imports

Adicionar ao `vitest.config.ts` para referenciar o código fonte:

```typescript
resolve: {
  alias: {
    '@': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src'),
    '@components': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/components'),
    '@hooks': path.resolve(__dirname, '../../src/Web/MeAjudaAi.Web.Consumer/src/hooks'),
  },
},
```

### 3. Criar README.md no Projeto de Testes

```markdown
# MeAjudaAi.Web.Consumer.Tests

Testes automatizados para o projeto React Consumer.

## Stack de Testes

- **Vitest**: Framework de testes
- **React Testing Library**: Testes de componentes
- **Playwright**: Testes E2E
- **MSW**: Mock de APIs

## Executar Testes

\`\`\`bash
# Testes unitários
npm test

# Testes E2E
npm run test:e2e

# Cobertura
npm run test:coverage
\`\`\`

## Estrutura

- `src/`: Testes unitários e de integração
- `e2e/`: Testes end-to-end
- `__tests__/`: Setup e helpers

## Cobertura Mínima

- Lines: 80%
- Functions: 80%
- Branches: 80%
- Statements: 80%
```

### 4. Adicionar ao .gitignore

```gitignore
# tests/MeAjudaAi.Web.Consumer.Tests/.gitignore

# Dependencies
node_modules/

# Coverage
coverage/
.nyc_output/

# Test results
test-results/
playwright-report/
playwright/.cache/

# Build
dist/
build/

# Environment
.env
.env.local

# IDE
.vscode/
.idea/
*.swp
*.swo

# OS
.DS_Store
Thumbs.db
```

### 5. Configurar Pre-commit Hooks (Opcional)

```bash
# Instalar Husky
npm install --save-dev husky lint-staged

# Configurar package.json
{
  "lint-staged": {
    "*.{ts,tsx}": [
      "npm run test:run -- --related"
    ]
  }
}

# Criar hook
npx husky install
npx husky add .husky/pre-commit "cd tests/MeAjudaAi.Web.Consumer.Tests && npx lint-staged"
```

---

## Dicas de Implementação

### Comece Pequeno

1. **Primeiro componente**: Escolha um componente simples (Button, Input)
2. **Primeiro hook**: Teste um hook utilitário (useLocalStorage)
3. **Primeira página**: Teste uma página sem muitas dependências
4. **Primeiro E2E**: Fluxo de login básico

### Mantenha Consistência

- Use o mesmo padrão de nomenclatura do backend (.NET)
- Siga as convenções de teste já estabelecidas
- Mantenha a mesma estrutura de pastas espelhada

### Integre Gradualmente

1. Configure CI/CD desde o início
2. Estabeleça métricas de cobertura progressivas
3. Documente decisões e padrões
4. Faça code review de testes também

---

## Alinhamento com Backend (.NET)

### Similaridades Intencionais

| Backend (.NET) | Frontend (React) |
|---|---|
| xUnit | Vitest |
| FluentAssertions | Jest-DOM matchers |
| Moq | MSW |
| Integration Tests | E2E Tests (Playwright) |
| Code Coverage (Coverlet) | Code Coverage (v8) |
| Projetos separados em `tests/` | Projeto separado em `tests/` |

### Benefícios desta Abordagem

✅ **Equipe unificada**: Mesma estrutura mental para backend e frontend  
✅ **CI/CD consistente**: Pipelines similares, fácil manutenção  
✅ **Onboarding simplificado**: Novos devs entendem rapidamente  
✅ **Qualidade padronizada**: Mesmos critérios de cobertura e qualidade  
