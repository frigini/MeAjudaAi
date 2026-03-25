import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';

// Mock next-auth, next/navigation, next/link, and theme dependencies
vi.mock('next-auth/react', () => ({
  useSession: () => ({ data: { user: { name: 'Carlos Admin', roles: ['admin'] } } }),
  signOut: vi.fn(),
}));
vi.mock('next/navigation', () => ({
  usePathname: () => '/dashboard',
}));
vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));
vi.mock('@/components/ui/theme-toggle', () => ({
  ThemeToggle: () => <button aria-label="Toggle theme">Theme</button>,
}));
vi.mock('@/lib/types', () => ({
  APP_ROUTES: {
    DASHBOARD: '/dashboard',
    PROVIDERS: '/providers',
    DOCUMENTS: '/documents',
    CATEGORIES: '/categories',
    SERVICES: '/services',
    CITIES: '/allowed-cities',
    SETTINGS: '/settings',
  },
  APP_ROUTE_LABELS: {
    DASHBOARD: 'Dashboard',
    PROVIDERS: 'Prestadores',
    DOCUMENTS: 'Documentos',
    CATEGORIES: 'Categorias',
    SERVICES: 'Serviços',
    CITIES: 'Cidades Atendidas',
    SETTINGS: 'Configurações',
  },
  ROLES: { ADMIN: 'admin' },
}));

import { Sidebar } from '@/components/layout/sidebar';

describe('Sidebar', () => {
  it('deve renderizar o logo MeAjudaAí', () => {
    render(<Sidebar />);
    expect(screen.getByText('MeAjudaAí')).toBeInTheDocument();
  });

  it('deve exibir itens de navegação', () => {
    render(<Sidebar />);
    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Prestadores')).toBeInTheDocument();
    expect(screen.getByText('Categorias')).toBeInTheDocument();
  });

  it('deve exibir nome do usuário da sessão', () => {
    render(<Sidebar />);
    expect(screen.getByText('Carlos Admin')).toBeInTheDocument();
  });

  it('deve exibir label de Administrador para role admin', () => {
    render(<Sidebar />);
    expect(screen.getByText('Administrador')).toBeInTheDocument();
  });

  it('deve ter botão de abrir menu mobile', () => {
    render(<Sidebar />);
    expect(screen.getByLabelText('Open sidebar')).toBeInTheDocument();
  });

  it('deve ter botão de sair', () => {
    render(<Sidebar />);
    expect(screen.getByText('Sair')).toBeInTheDocument();
  });

  it('deve chamar signOut ao clicar no botão de sair', async () => {
    const { signOut } = await import('next-auth/react');
    render(<Sidebar />);
    const logoutButton = screen.getByText('Sair');
    logoutButton.click();
    expect(signOut).toHaveBeenCalled();
  });
});
